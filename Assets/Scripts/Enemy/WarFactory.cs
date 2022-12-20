using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory {

    [SerializeField]
    private Shell _shellPrefab = default;
    [SerializeField]
    private Explosion _explosionPrefab = default;

    public Shell Shell => Get(_shellPrefab);
    public Explosion Explosion => Get(_explosionPrefab);

    T Get<T>(T prefab)
        where T : WarEntity {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(WarEntity warEntity) {
        Debug.Assert(warEntity.OriginFactory == this, "Wrong Factory reclaimed");
        Destroy(warEntity.gameObject);
    }
}