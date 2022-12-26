using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {

    [SerializeField]
    private Transform _focus            = default;

    [SerializeField, Range(1f, 20f)]
    private float     _distance         = 5f;

    [SerializeField, Min(0f)]
    private float     _focusRadius      = 1f;

    [SerializeField, Range(0f, 1f)]
    private float     _focusCenter      = 0.5f;

    [SerializeField, Range(1f, 360f)]
    private float     _rotationSpeed    = 90f;

    [SerializeField, Range(-89f, 89f)]
    private float     _minVerticalAngle = -30f,
                      _maxVerticleAngle = 60f;

    [SerializeField, Min(0f)]
    private float     _alignDelay       = 5f;

    [SerializeField, Range(0f, 90f)]
    private float     _alignSmothRange  = 45f;

    [SerializeField]
    private LayerMask         _obstructionMask  = -1;

    private Vector3 _focusPoint,
                      _previousFocusPoint;

    private Vector2 _orbitAngles = new Vector2(45f, 0f);
    private float _lastManualRotationTime;
    private Camera _regularCamera;

    #region MonoBehaviour

    private void Awake() {
        _regularCamera = GetComponent<Camera>();
        _focusPoint = _focus.position;
        transform.localRotation = Quaternion.Euler(_orbitAngles);
    }

    private void OnValidate() {
        if (_maxVerticleAngle < _minVerticalAngle) {
            _maxVerticleAngle = _minVerticalAngle;
        }
    }

    private void LateUpdate() {
        UpdateFocusPoint();
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotion()) {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(_orbitAngles);
        }
        else {
            lookRotation = transform.localRotation;
        }
        var lookDirection = lookRotation * Vector3.forward;
        var lookPosition = _focusPoint - lookDirection * _distance;
        var rectOffset = lookDirection * _regularCamera.nearClipPlane;
        var rectPosition = lookPosition + rectOffset;
        var castFrom = _focus.position;
        var castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;
        if (Physics.BoxCast(
            castFrom,
            CameraHalfExtends,
            castDirection,
            out RaycastHit hit,
            lookRotation,
            castDistance,
            _obstructionMask)) {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    #endregion MonoBehaviour

    private void ConstrainAngles() {
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticleAngle);
        if (_orbitAngles.y < 0f) {
            _orbitAngles.y += 360f;
        }
        else if (_orbitAngles.y > 360f) {
            _orbitAngles.y -= 360f;
        }
    }

    private bool ManualRotation() {
        var input = new Vector2(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
            _orbitAngles += _rotationSpeed * Time.unscaledDeltaTime * input;
            _lastManualRotationTime = Time.unscaledDeltaTime;
            return true;
        }
        return false;
    }

    private bool AutomaticRotion() {
        if (Time.unscaledDeltaTime - _lastManualRotationTime < _alignDelay) {
            return false;
        }
        var movement = new Vector2(
            _focusPoint.x - _previousFocusPoint.x,
            _focusPoint.z - _previousFocusPoint.z);
        var movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f) {
            return false;
        }
        var headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        var deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
        var rotationChange = _rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < _alignSmothRange) {
            rotationChange *= deltaAbs / _alignSmothRange;
        }
        else if (180f - deltaAbs < _alignSmothRange) {
            rotationChange *= 180f - deltaAbs / _alignSmothRange;
        }
        _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    private static float GetAngle(Vector2 direction) {
        var angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    private void UpdateFocusPoint() {
        _previousFocusPoint = _focusPoint;
        var targetPoint = _focus.position;
        if (_focusRadius > 0f) {
            var distance = Vector3.Distance(targetPoint, _focusPoint);
            var t = 1f;
            if (distance > 0.1f && _focusCenter > 0f) {
                t = Mathf.Pow(1f - _focusCenter, Time.unscaledDeltaTime);
            }
            if (distance > _focusRadius) {
                t = Mathf.Min(t, _focusRadius / distance);
            }
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else {
            _focusPoint = targetPoint;
        }
    }

    private Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y =
                _regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * _regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }
}