using UnityEngine;

public class Shell : WarEntity {

    Vector3 _laucnPoint,
            _targetPoint,
            _launchVelocity;
    float   _age,
            _blastRadius,
            _damage;

    public void Initialize(
        Vector3 launchPoint,
        Vector3 targetPoint,
        Vector3 launchVelocity,
        float blastRadius,
        float damage) {
        _laucnPoint = launchPoint;
        _targetPoint = targetPoint;
        _launchVelocity = launchVelocity;
        _blastRadius = blastRadius;
        _damage = damage;
    }

    public override bool GameUpdate() {
        _age += Time.deltaTime;
        var p = _laucnPoint + _launchVelocity * _age;
        var d = _launchVelocity;
        d.y -= 9.81f * _age;
        p.y -= 0.5f * 9.81f * _age * _age;
        if (p.y < 0) {
            Game.SpawnExplosion().Initialize(_targetPoint, _blastRadius, _damage);
            OriginFactory.Reclaim(this);
            return false;
        }
        transform.localPosition = p;
        transform.localRotation = Quaternion.LookRotation(d);
        Game.SpawnExplosion().Initialize(p, 0.1f);
        return true;
    }
} 
