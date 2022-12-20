using UnityEngine;

public class Game : MonoBehaviour {
    private const float pauseTimeScale = 0f;

    private static Game _instance;

    [SerializeField]
    private Vector2Int              _boardSize          =   new Vector2Int(11,11);

    [SerializeField]
    private GameBoard               _board              =   default;

    [SerializeField]
    private GameTileContentFactory  _tileContentFactory =   default;

    [SerializeField]
    private WarFactory              _warFactory         =   default;

    [SerializeField]
    private GameScenario            _scenario           =   default;

    [SerializeField, Range(0,100)]
    private int                     _startingHealth     =   10;

    [SerializeField, Range(1f,10f)]
    private float                   _playSpeed          =   1f;

    private GameBehaviourCollection _enemies = new GameBehaviourCollection();
    private GameBehaviourCollection _nonEnemies = new GameBehaviourCollection();
    private GameScenario.State _activeScenario;
    private TowerType _selectedTowerType;
    private int _playerHealth;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    #region Monobehaviour

    private void Awake() {
        _board.Initialize(_boardSize, _tileContentFactory);
        _board.ShowGrid = true;
        _activeScenario = _scenario.Begin();
        _playerHealth = _startingHealth;
    }

    private void OnEnable() {
        _instance = this;
    }

    private void OnValidate() {
        if (_boardSize.x < 2) {
            _boardSize.x = 2;
        }
        if (_boardSize.y < 2) {
            _boardSize.y = 2;
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            HandleTouch();
        }
        if (Input.GetMouseButtonDown(1)) {
            HandleAlternativeTouch();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            _board.ShowPaths = !_board.ShowPaths;
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            _board.ShowGrid = !_board.ShowGrid;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            _selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            _selectedTowerType = TowerType.Mortar;
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            BeginNewGame();
        }
        if (_playerHealth <= 0 && _startingHealth > 0) {
            Debug.Log("Defeat!");
            BeginNewGame();
        }
        if (!_activeScenario.Progress() && _enemies.IsEmpty) {
            Debug.Log("Victory!");
            BeginNewGame();
            _activeScenario.Progress();
        }
        if (Input.GetKey(KeyCode.Space)) {
            Time.timeScale =
                Time.timeScale > pauseTimeScale ? pauseTimeScale : _playSpeed;
        }
        else if (Time.timeScale > pauseTimeScale) {
            Time.timeScale = _playSpeed;
        }
        _enemies.GameUpdate();
        Physics.SyncTransforms();
        _board.GameUpdate();
        _nonEnemies.GameUpdate();
    }

    #endregion Monobehaviour

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type) {
        var spawnPoint = _instance._board.GetSpawnPoint(Random.Range(0,_instance._board.SpawnPointCount));
        var enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        _instance._enemies.Add(enemy);
    }

    public static Shell SpawnShell() {
        var shell = _instance._warFactory.Shell;
        _instance._nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion() {
        var explosion = _instance._warFactory.Explosion;
        _instance._nonEnemies.Add(explosion);
        return explosion;
    }

    public static void EnemyReachDestination() {
        _instance._playerHealth -= 1;
    }

    private void BeginNewGame() {
        _playerHealth = _startingHealth;
        _enemies.Clear();
        _nonEnemies.Clear();
        _board.Clear();
        _activeScenario = _scenario.Begin();
    }

    private void HandleAlternativeTouch() {
        var tile = _board.GetTile(TouchRay);
        if (tile != null) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                _board.ToggleDestination(tile);
            }
            else {
                _board.ToggleSpawnPoint(tile);
            }
        }
    }

    private void HandleTouch() {
        var tile = _board.GetTile(TouchRay);
        if (tile != null) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                _board.ToggleTower(tile, _selectedTowerType);
            }
            else {
                _board.ToggleWall(tile);
            }
        }
    }
}