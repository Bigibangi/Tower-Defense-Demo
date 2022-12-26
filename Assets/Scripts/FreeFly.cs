using System;
using UnityEngine;

using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody))]
public class FreeFly : MonoBehaviour {

    #region Editor

    [SerializeField, Range (0, 100f)]
    private float     _maxSpeed         = 10f,
                      _maxAcceleration  = 10f,
                      _maxAirAcceleration = 1f,
                      _maxSnapSpeed = 100f;

    [SerializeField, Range(0f,90f)]
    private float     _maxGroundAngle   = 25f,
                      _maxStairsAngle   = 50f;

    [SerializeField]
    private Transform _playerInputSpace = default;

    [SerializeField,Range(0f,10f)]
    private float     _jumpHeight       = 2f;

    [SerializeField, Range(0,5)]
    private int       _maxAirJumps      = 0;

    [SerializeField, Min(0f)]
    private float     _probeDistance    = 1f;

    [SerializeField]
    private LayerMask   _probeMask = -1,
                        _stairsMask = -1;

    #endregion Editor

    #region Fields

    private Vector2 _moveDirection;

    private Vector3 _velocity,
                    _desiredVelocity,
                    _contactNormal,
                    _steepNormal,
                    _upAxis;

    private Rigidbody _body;
    private bool _desiredJump;

    private int _jumpPhase,
                _groundContactCount,
                _stepsSinceLastGrounded,
                _stepsSinceLastJump,
                _steepContactCount;

    private float _minGroundDotProduct,
                  _minStairsDotProduct;

    #endregion Fields

    #region Properties

    private bool OnGround => _groundContactCount > 0;
    private bool OnSteep => _steepContactCount > 0;

    #endregion Properties

    #region Events

    public event Action<Vector3> OnMoved;

    #endregion Events

    #region Monobehaviour

    private void Awake() {
        _body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void FixedUpdate() {
        _upAxis = -Physics.gravity.normalized;
        UpdateState();
        Move(_moveDirection);
        if (_desiredJump) {
            _desiredJump = false;
            Jump();
        }
        GetComponent<Renderer>().material.SetColor(
            "_Color", OnGround ? Color.black : Color.white
        );
        ClearState();
    }

    private void OnDisable() {
    }

    private void OnCollisionEnter(Collision collision) {
        EvaluateColission(collision);
    }

    private void OnCollisionStay(Collision collision) {
        EvaluateColission(collision);
    }

    private void OnValidate() {
        _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
        _minStairsDotProduct = Mathf.Cos(_maxStairsAngle * Mathf.Deg2Rad);
    }

    #endregion Monobehaviour

    public void OnMove(CallbackContext context) {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public void OnLook(CallbackContext context) {
        Look();
    }

    public void OnJump(CallbackContext context) {
        _desiredJump |= context.ReadValueAsButton();
    }

    private void Move(Vector2 direction) {
        direction = Vector2.ClampMagnitude(direction, 1f);
        if (_playerInputSpace) {
            var forward = _playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            var right = _playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            _desiredVelocity = (forward * direction.y + right * direction.x) * _maxSpeed;
        }
        else {
            _desiredVelocity = new Vector3(direction.x, 0, direction.y) * _maxSpeed;
        }
        _velocity = _body.velocity;
        AdjustVelocity();
        _body.velocity = _velocity;
        OnMoved?.Invoke(transform.position);
    }

    private void Jump() {
        Vector3 jumpDirection;
        if (OnGround) {
            jumpDirection = _contactNormal;
        }
        else if (OnSteep) {
            jumpDirection = _steepNormal;
            _jumpPhase = 0;
        }
        else if (_maxAirJumps > 0 && _jumpPhase <= _maxAirJumps) {
            if (_jumpPhase == 0) {
                _jumpPhase = 1;
            }
            jumpDirection = _contactNormal;
        }
        else {
            return;
        }
        _jumpPhase++;
        _stepsSinceLastJump = 0;
        var jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * _jumpHeight);
        jumpDirection = (jumpDirection + _upAxis).normalized;
        var alignedSpeed = Vector3.Dot(_velocity,jumpDirection);
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        _velocity += jumpSpeed * jumpDirection;
        _body.velocity = _velocity;
    }

    private void UpdateState() {
        _stepsSinceLastGrounded += 1;
        _stepsSinceLastJump += 1;
        if (OnGround || SnapToGround() || CheckSteepContacts()) {
            _stepsSinceLastGrounded = 0;
            if (_stepsSinceLastJump > 1) {
                _jumpPhase = 0;
            }
            if (_groundContactCount > 1) {
                _contactNormal.Normalize();
            }
        }
        else {
            _contactNormal = _upAxis;
        }
    }

    private void ClearState() {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = _steepNormal = Vector3.zero;
    }

    private void Look() {
        Debug.Log("Look");
    }

    private void EvaluateColission(Collision collision) {
        var minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            var normal = collision.GetContact(i).normal;
            var upDot = Vector3.Dot(_upAxis, normal);
            if (upDot >= minDot) {
                _groundContactCount++;
                _contactNormal += normal;
            }
            else if (upDot >= -0.01f) {
                _steepContactCount++;
                _steepNormal += normal;
            }
        }
    }

    private float GetMinDot(int layer) {
        return (_stairsMask & (1 << layer)) == 0 ?
            _minGroundDotProduct : _minStairsDotProduct;
    }

    private void AdjustVelocity() {
        var xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        var zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        var currentX = Vector3.Dot(_velocity, xAxis);
        var currentZ = Vector3.Dot(_velocity,zAxis);
        var acceleration = OnGround?_maxAcceleration:_maxAirAcceleration;
        var maxSpeedChange = acceleration * Time.deltaTime;
        var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        var newZ = Mathf.MoveTowards(currentZ,_desiredVelocity.z, maxSpeedChange);
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector) {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    private bool SnapToGround() {
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2) {
            return false;
        }
        var speed = _velocity.magnitude;
        if (speed > _maxSnapSpeed) {
            return false;
        }
        if (!Physics.Raycast(
            _body.position,
            -_upAxis,
            out RaycastHit hit,
            _probeDistance,
            _probeMask)) {
            return false;
        }
        if (hit.normal.y < _minGroundDotProduct) {
            return false;
        }
        _groundContactCount = 1;
        _contactNormal = hit.normal;
        var dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f) {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
            _body.velocity = _velocity;
        }
        return true;
    }

    private bool CheckSteepContacts() {
        if (_steepContactCount > 1) {
            _steepNormal.Normalize();
            float upDot = Vector3.Dot(_upAxis, _steepNormal);
            if (upDot >= _minGroundDotProduct) {
                _steepContactCount = 0;
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }
        return false;
    }
}