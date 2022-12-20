using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory {

    [System.Serializable]
    private class EnemyConfig {
        public Enemy prefab = default;

        [FloatRangeSlider(0.5f,2f)]
        public FloatRange scale = new FloatRange(1f);

        [FloatRangeSlider(0.5f,2f)]
        public FloatRange speed = new FloatRange(1f);

        [FloatRangeSlider(-0.4f,0.4f)]
        public FloatRange offSet = new FloatRange(0f);

        [FloatRangeSlider(10f,1000f)]
        public FloatRange health = new FloatRange(100f);
    }

    [SerializeField]
    private EnemyConfig _small  = default,
                        _medium = default,
                        _large  = default;

    private EnemyConfig GetConfig(EnemyType type) {
        switch (type) {
            case EnemyType.Small: return _small;
            case EnemyType.Medium: return _medium;
            case EnemyType.Large: return _large;
        }
        Debug.Assert(false, "Unsupported enemy type");
        return null;
    }

    public Enemy Get(EnemyType enemyType = EnemyType.Medium) {
        var config = GetConfig(enemyType);
        var instance = CreateGameObjectInstance(config.prefab);
        instance.OriginFactory = this;
        instance.Initialize(
            config.scale.RandomValueInRange,
            config.speed.RandomValueInRange,
            config.offSet.RandomValueInRange,
            config.health.RandomValueInRange);
        return instance;
    }

    public void Reclaim(Enemy enemy) {
        Debug.Assert(
            enemy.OriginFactory == this, "Wrong Factory Reclaimed");
        Destroy(enemy.gameObject);
    }
}