using UnityEngine;

public class Enemy : GameBehaviour {

    [SerializeField]
    private Transform               _model           = default;

    [SerializeField]
    private EnemyAnimationConfig    _animationConfig = default;

    private EnemyAnimator _animator;
    private EnemyFactory _originFactory;

    private GameTile _tileFrom,
                    _tileTo;

    private Vector3 _positionFrom,
                    _positionTo;

    private float _progress,
                    _progressFactor,
                    _directionAngleFrom,
                    _directionAngleTo,
                    _pathOffset,
                    _speed;

    private Direction _direction;
    private DirectionChange _directionChange;
    private Collider _targetPointCollider;

    public Collider TargetPointCollider {
        set {
            Debug.Assert(_targetPointCollider == null, "Redefined collider");
            _targetPointCollider = value;
        }
    }

    public float Scale { get; private set; }
    public float Health { get; set; }

    public EnemyFactory OriginFactory {
        get => _originFactory;
        set {
            Debug.Assert(_originFactory == null, "Redefined Origin Factory");
            _originFactory = value;
        }
    }

    public bool IsValidTarget => _animator.CurrentClip == EnemyAnimator.Clip.Move;

    private void Awake() {
        _animator.Configure(
            _model.GetChild(0).gameObject.AddComponent<Animator>(),
            _animationConfig);
    }

    private void OnDestroy() {
        _animator.Destroy();
    }

    public void Initialize(float scale, float speed, float pathOffset, float health) {
        _model.localScale = new Vector3(scale, scale, scale);
        _pathOffset = pathOffset;
        _speed = speed;
        Scale = scale;
        Health = health;
        _animator.PlayIntro();
        _targetPointCollider.enabled = false;
    }

    public void SpawnOn(GameTile spawnPoint) {
        Debug.Assert(spawnPoint.NexOnPath != null, "Nowhere to go!", this);
        _tileFrom = spawnPoint;
        _tileTo = spawnPoint.NexOnPath;
        _progress = 0f;
        PrepareIntro();
    }

    public override bool GameUpdate() {
#if UNITY_EDITOR
        if (!_animator.IsValid) {
            _animator.RestoreHotReload(
                _model.GetChild(0).GetComponent<Animator>(),
                _animationConfig,
                _speed / Scale);
        }
#endif
        _animator.GameUpdate();
        if (_animator.CurrentClip == EnemyAnimator.Clip.Intro) {
            if (!_animator.IsDone) {
                return true;
            }
            _animator.PlayMove(_speed / Scale);
            _targetPointCollider.enabled = true;
        }
        else if (_animator.CurrentClip >= EnemyAnimator.Clip.Outro) {
            if (_animator.IsDone) {
                Recycle();
                return false;
            }
            return true;
        }
        if (Health <= 0f) {
            _animator.PlayDying();
            _targetPointCollider.enabled = false;
            return true;
        }
        _progress += Time.deltaTime * _progressFactor;
        while (_progress >= 1f) {
            if (_tileTo == null) {
                Game.EnemyReachDestination();
                _animator.PlayOutro();
                _targetPointCollider.enabled = false;
                return true;
            }
            _progress = (_progress - 1f) / _progressFactor;
            PrepareNextState();
            _progress *= _progressFactor;
        }
        if (_directionChange == DirectionChange.None) {
            transform.localPosition =
                Vector3.LerpUnclamped(
                    _positionFrom,
                    _positionTo,
                    _progress);
        }
        else {
            var angle = Mathf.LerpUnclamped(_directionAngleFrom,_directionAngleTo,_progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }

    public void ApplyDamage(float damage) {
        Debug.Assert(damage >= 0f, "Negative damage Applied");
        Health -= damage;
    }

    private void PrepareNextState() {
        _tileFrom = _tileTo;
        _tileTo = _tileTo.NexOnPath;
        _positionFrom = _positionTo;
        if (_tileTo == null) {
            PrepareOutro();
            return;
        }
        _positionTo = _tileFrom.ExitPoint;
        _directionChange = _direction.GetDirectonChange(_tileFrom.PathDirection);
        _direction = _tileFrom.PathDirection;
        _directionAngleFrom = _directionAngleTo;
        switch (_directionChange) {
            case DirectionChange.None: PrepareForward(); break;
            case DirectionChange.TurnRight: PrepareTurnRight(); break;
            case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
            default: PrepareTurnAround(); break;
        }
    }

    private void PrepareForward() {
        transform.localRotation = _direction.GetRotation();
        _directionAngleTo = _direction.GetAngle();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        _progressFactor = _speed;
    }

    private void PrepareTurnRight() {
        _directionAngleTo = _directionAngleFrom + 90f;
        _model.localPosition = new Vector3(_pathOffset - 0.5f, 0f);
        transform.localPosition = _positionFrom + _direction.GetHalfVector();
        _progressFactor = _speed / (Mathf.PI * 0.5f * (0.5f - _pathOffset));
    }

    private void PrepareTurnLeft() {
        _directionAngleTo = _directionAngleFrom - 90f;
        _model.localPosition = new Vector3(_pathOffset + 0.5f, 0f);
        transform.localPosition = _positionFrom + _direction.GetHalfVector();
        _progressFactor = _speed / (Mathf.PI * 0.5f * (0.5f - _pathOffset));
    }

    private void PrepareTurnAround() {
        _directionAngleTo = _directionAngleFrom + (_pathOffset < 0f ? 180f : -180f);
        _model.localPosition = new Vector3(_pathOffset, 0f);
        transform.localPosition = _positionFrom;
        _progressFactor = _speed / (Mathf.PI * Mathf.Max(Mathf.Abs(_pathOffset), 0.2f));
    }

    private void PrepareIntro() {
        _positionFrom = _tileFrom.transform.localPosition;
        transform.localPosition = _positionFrom;
        _positionTo = _tileFrom.ExitPoint;
        _direction = _tileFrom.PathDirection;
        _directionChange = DirectionChange.None;
        _directionAngleFrom = _directionAngleTo = _direction.GetAngle();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        transform.localRotation = _direction.GetRotation();
        _progressFactor = _speed;
    }

    private void PrepareOutro() {
        _positionTo = _tileFrom.transform.localPosition;
        _directionChange = DirectionChange.None;
        _directionAngleTo = _direction.GetAngle();
        _model.localPosition = new Vector3(_pathOffset, 0f);
        transform.localRotation = _direction.GetRotation();
        _progressFactor = _speed;
    }

    public override void Recycle() {
        _animator.Stop();
        OriginFactory.Reclaim(this);
    }
}