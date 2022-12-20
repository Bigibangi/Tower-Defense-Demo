using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory {

    [SerializeField]
    private GameTileContent _destinationPrefab,
                            _emptyPrefab,
                            _wallPrefab,
                            _spawnPointPrefab;
    [SerializeField]
    Tower[]                 _towerPrefabs;

    public void Reclaim(GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong Factory Reclaimed!");
        Destroy(content.gameObject);
    }

    public GameTileContent Get(GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Destination: return Get(_destinationPrefab);
            case GameTileContentType.Empty: return Get(_emptyPrefab);
            case GameTileContentType.Wall: return Get(_wallPrefab);
            case GameTileContentType.SpawnPoint: return Get(_spawnPointPrefab);
        }
        Debug.Assert(false, "Unsupported type " + type);
        return null;
    }

    public Tower Get(TowerType type) {
        Debug.Assert((int) type < _towerPrefabs.Length, "Unsupported tower type");
        var prefab = _towerPrefabs[(int) type];
        Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index");
        return Get(prefab);
    }

    T Get<T>(T prefab) 
        where T : GameTileContent{
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }
}
