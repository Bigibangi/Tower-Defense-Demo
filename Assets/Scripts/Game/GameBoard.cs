using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    [SerializeField]
    private Transform _ground = default;

    [SerializeField]
    private GameTile _tilePrefab = default;

    [SerializeField]
    private Texture2D _gridTexture = default;

    private Vector2Int _size;
    private GameTile[] _tiles;
    private Queue<GameTile> _searchFrontier = new Queue<GameTile>();
    private GameTileContentFactory _contentFactory;

    private bool _showGrid,
                            _showPaths;

    private List<GameTile> _spawnPoints = new List<GameTile>();
    private List<GameTileContent> _updatingContents = new List<GameTileContent>();

    public bool ShowGrid {
        get => _showGrid;
        set {
            _showGrid = value;
            Material m = _ground.GetComponent<MeshRenderer>().material;
            if (_showGrid) {
                m.mainTexture = _gridTexture;
                m.SetTextureScale("_MainTex", _size);
            }
            else {
                m.mainTexture = null;
            }
        }
    }

    public bool ShowPaths {
        get => _showPaths;
        set {
            _showPaths = value;
            if (_showPaths) {
                foreach (GameTile tile in _tiles) {
                    tile.ShowPath();
                }
            }
            else {
                foreach (GameTile tile in _tiles) {
                    tile.HidePath();
                }
            }
        }
    }

    public int SpawnPointCount => _spawnPoints.Count;

    public void Initialize(
        Vector2Int size, GameTileContentFactory contentFactory
    ) {
        this._size = size;
        this._contentFactory = contentFactory;
        _ground.localScale = new Vector3(size.x, size.y, 1f);

        Vector2 offset = new Vector2(
            (size.x - 1) * 0.5f, (size.y - 1) * 0.5f
        );
        _tiles = new GameTile[size.x * size.y];
        for (int i = 0, y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++, i++) {
                GameTile tile = _tiles[i] = Instantiate(_tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(
                    x - offset.x, 0f, y - offset.y
                );

                if (x > 0) {
                    GameTile.MakeWestEastNeighbors(tile, _tiles[i - 1]);
                }
                if (y > 0) {
                    GameTile.MakeSouthNorthNeighbors(tile, _tiles[i - size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0) {
                    tile.IsAlternative = !tile.IsAlternative;
                }
            }
        }
        Clear();
    }

    public void GameUpdate() {
        foreach (var content in _updatingContents) {
            content.GameUpdate();
        }
    }

    public void ToggleDestination(GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Destination) {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths()) {
                tile.Content =
                    _contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = _contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Wall) {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = _contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths()) {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public void ToggleTower(GameTile tile, TowerType type) {
        if (tile.Content.Type == GameTileContentType.Tower) {
            _updatingContents.Remove(tile.Content);
            if (((Tower) tile.Content).TowerType == type) {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else {
                tile.Content = _contentFactory.Get(type);
                _updatingContents.Add(tile.Content);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = _contentFactory.Get(type);
            if (FindPaths()) {
                _updatingContents.Add(tile.Content);
            }
            else {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Wall) {
            _updatingContents.Add(tile.Content);
            tile.Content = _contentFactory.Get(type);
        }
    }

    public void ToggleSpawnPoint(GameTile tile) {
        if (tile.Content.Type == GameTileContentType.SpawnPoint) {
            if (_spawnPoints.Count > 1) {
                _spawnPoints.Remove(tile);
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = _contentFactory.Get(GameTileContentType.SpawnPoint);
            _spawnPoints.Add(tile);
        }
    }

    public GameTile GetTile(Ray ray) {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1)) {
            int x = (int)(hit.point.x + _size.x * 0.5f);
            int y = (int)(hit.point.z + _size.y * 0.5f);
            if (x >= 0 && x < _size.x && y >= 0 && y < _size.y) {
                return _tiles[x + y * _size.x];
            }
        }
        return null;
    }

    public GameTile GetSpawnPoint(int index) {
        return _spawnPoints[index];
    }

    public void Clear() {
        foreach (var tile in _tiles) {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }
        _spawnPoints.Clear();
        _updatingContents.Clear();
        ToggleDestination(_tiles[_tiles.Length / 2]);
        ToggleSpawnPoint(_tiles[0]);
    }

    private bool FindPaths() {
        foreach (GameTile tile in _tiles) {
            if (tile.Content.Type == GameTileContentType.Destination) {
                tile.BecomeDestination();
                _searchFrontier.Enqueue(tile);
            }
            else {
                tile.ClearPath();
            }
        }
        if (_searchFrontier.Count == 0) {
            return false;
        }

        while (_searchFrontier.Count > 0) {
            GameTile tile = _searchFrontier.Dequeue();
            if (tile != null) {
                if (tile.IsAlternative) {
                    _searchFrontier.Enqueue(tile.GrowPathToNorth());
                    _searchFrontier.Enqueue(tile.GrowPathToSouth());
                    _searchFrontier.Enqueue(tile.GrowPathToEast());
                    _searchFrontier.Enqueue(tile.GrowPathToWest());
                }
                else {
                    _searchFrontier.Enqueue(tile.GrowPathToWest());
                    _searchFrontier.Enqueue(tile.GrowPathToEast());
                    _searchFrontier.Enqueue(tile.GrowPathToSouth());
                    _searchFrontier.Enqueue(tile.GrowPathToNorth());
                }
            }
        }

        foreach (GameTile tile in _tiles) {
            if (!tile.HasPath) {
                return false;
            }
        }

        if (_showPaths) {
            foreach (GameTile tile in _tiles) {
                tile.ShowPath();
            }
        }
        return true;
    }
}