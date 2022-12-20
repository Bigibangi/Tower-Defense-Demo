using UnityEngine;

public class TargetPoint : MonoBehaviour {
    private const int enemyLayerMask = 1 << 9;

    private static Collider[] _buffer = new Collider[100];

    public static int BufferedCount { get; private set; }

    public static TargetPoint randomBuffered =>
        GetBuffered(Random.Range(0, BufferedCount));

    public Enemy Enemy { get; private set; }
    public Vector3 Position => transform.position;

    #region MonoBehaviour

    private void Awake() {
        Enemy = transform.root.GetComponent<Enemy>();
        Debug.Assert(
            Enemy != null,
            "Target point without enemy root",
            this);
        Debug.Assert(
            GetComponent<SphereCollider>() != null,
            "Target point without sphere collider",
            this);
        Debug.Assert(
            gameObject.layer == 9,
            "Target point on wrong layer",
            this);
        Enemy.TargetPointCollider = GetComponent<Collider>();
    }

    #endregion MonoBehaviour

    public static bool FillBuffer(Vector3 position, float range) {
        var top = position;
        top.y += 3f;
        BufferedCount = Physics.OverlapCapsuleNonAlloc(
            position,
            top,
            range,
            _buffer,
            enemyLayerMask);
        return BufferedCount > 0;
    }

    public static TargetPoint GetBuffered(int index) {
        var target = _buffer[index].GetComponent<TargetPoint>();
        Debug.Assert(target != null, "Target not enemy", _buffer[0]);
        return target;
    }
}