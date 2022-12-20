using System;
using UnityEngine;
using UnityEngine.WSA;

public class MortarTower : Tower {

    [SerializeField, Range(0.5f,2f)]
    private float   _shotsPerSecond = 1f;
    [SerializeField]
    private Transform _mortar = default;
    [SerializeField, Range(0.5f, 10f)]
    private float _shellBlastRadius = 1f;
    [SerializeField, Range(1f,100f)]
    private float _shellDamage = 20f;

    float   _launchSpeed,
            _launchProgress;

    public override TowerType TowerType => TowerType.Mortar;

    public void Awake() {
        OnValidate();
    }

    private void OnValidate() {
        var x = _targetingRange + 0.25001f;
        var y = -_mortar.position.y;
        _launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate() {
        _launchProgress += _shotsPerSecond * Time.deltaTime;
        while(_launchProgress >= 1f) {
            if (AcquireTarget(out var target)) {
                Launch(target);
                _launchProgress -= 1f;
            }
            else {
                _launchProgress = 0.999f;
            }
        }
    }

    private void Launch(TargetPoint target) {
        var launchPoint = _mortar.position;
        var targetPoint = target.Position;
        targetPoint.y = 0f;
        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        var x = dir.magnitude;
        var y = -launchPoint.y;
        dir /= x;
        float g = 9.81f;
        float s = _launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        Debug.Assert(r >= 0f, "Launch velocity insufficient for range");
        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;
        _mortar.localRotation =
            Quaternion.LookRotation(
                new Vector3(
                    dir.x,
                    tanTheta,
                    dir.y));
        Game.SpawnShell().Initialize(
            launchPoint,
            targetPoint,
            new Vector3(
                s * cosTheta * dir.x,
                s * sinTheta,
                s * cosTheta * dir.y),
            _shellBlastRadius,
            _shellDamage);
        Vector3 prev = launchPoint, next;
        for (int i = 0; i <= 10; i++) {
            float t = i / 10f;
            float dx = s * cosTheta * t;
            float dy = s * sinTheta * t - 0.5f * g * t * t;
            next = launchPoint + new Vector3(dir.x * dx, dy, dir.y * dx);
            Debug.DrawLine(prev, next, Color.blue, 1f);
            prev = next;
        }
    }
}

