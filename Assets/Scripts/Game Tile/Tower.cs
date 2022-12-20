using System;
using UnityEngine;

public abstract class Tower : GameTileContent {
    private static Collider[] _targetsBuffer = new Collider[100];

    [SerializeField, Range(1.5f,10f)]
    protected float       _targetingRange  = 1.5f;

    public abstract TowerType TowerType { get; }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        var position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, _targetingRange);
    }

    protected bool AcquireTarget(out TargetPoint target) {
        if (TargetPoint.FillBuffer(transform.localPosition, _targetingRange)) {
            target = TargetPoint.randomBuffered;
            return true;
        }
        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target) {
        if (target == null || target.Enemy.IsValidTarget) {
            return false;
        }
        var a = transform.localPosition;
        var b = target.Position;
        var x = a.x - b.x;
        var z = a.z - b.z;
        var r = _targetingRange + 0.125f * target.Enemy.Scale;
        if (x * x + z * z > r * r) {
            target = null;
            return false;
        }
        return true;
    }
}