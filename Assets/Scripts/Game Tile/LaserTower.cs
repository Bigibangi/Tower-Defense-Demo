using static UnityEngine.GraphicsBuffer;
using UnityEngine;

public class LaserTower : Tower{

    [SerializeField,Range(1f,100f)]
    private float _damagePerSecond;
    [SerializeField]
    private Transform _turret       = default,
                      _laserBeam    = default;

    public override TowerType TowerType => TowerType.Laser;

    Vector3       _laserBeamScale;
    TargetPoint   _target;

    private void Awake() {
        _laserBeamScale = _laserBeam.localScale;
    }

    public override void GameUpdate() {
        if (TrackTarget(ref _target) || AcquireTarget(out _target)) {
            Shoot();
        }
        else
            _laserBeam.localScale = Vector3.zero;
    }

    private void Shoot() {
        var point = _target.Position;
        _turret.LookAt(point);
        _laserBeam.localRotation = _turret.localRotation;
        var d = Vector3.Distance(_turret.position,point);
        _laserBeamScale.z = d;
        _laserBeam.localScale = _laserBeamScale;
        _laserBeam.localPosition = _turret.localPosition + 0.5f * d * _laserBeam.forward;
        _target.Enemy.ApplyDamage(_damagePerSecond * Time.deltaTime);
    }
}