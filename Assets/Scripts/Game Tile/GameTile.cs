using UnityEngine;

public class GameTile : MonoBehaviour
{

    static Quaternion
        _northRotation = Quaternion.Euler(90f,0f,0f),
        _eastRotation = Quaternion.Euler(90f,90f,0f),
        _southRotation = Quaternion.Euler(90f,180f,0f),
        _westRotation = Quaternion.Euler(90f,270f,0f);

    [SerializeField]
    private Transform   _arrow = default;

    public bool HasPath => _distance != int.MaxValue;
    public bool IsAlternative { get; set; }
    public GameTileContent Content {
        get => _content;
        set {
            Debug.Assert(value != null, "NULL assigned to content");
            if(_content != null) {
                _content.Recycle();
            }
            _content = value;
            _content.transform.localPosition = transform.localPosition;
        }
    }
    public GameTile NexOnPath => _nextOnPath;
    public Vector3 ExitPoint { get; private set; }
    public Direction PathDirection { get; private set; }

    GameTile         _north,
                     _south,
                     _west,
                     _east,
                     _nextOnPath;

    int              _distance;
    GameTileContent  _content;

    public static void MakeWestEastNeighbors(GameTile east, GameTile west) {
        Debug.Assert(
            west._east == null && east._west == null, "Redefined Neighbors");
        west._east = east;
        east._west = west;
    }

    public static void MakeSouthNorthNeighbors(GameTile north, GameTile south) {
        Debug.Assert(
            south._north == null && north._south == null, "Redefined Neighbors");
        south._north = north;
        north._south = south;
    }

    public void ClearPath() {
        _distance = int.MaxValue;
        _nextOnPath = null;
    }

    public void BecomeDestination() {
        _distance = 0;
        _nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public void ShowPath() {
        if (_distance == 0) {
            _arrow.gameObject.SetActive(false);
            return;
        }
        _arrow.gameObject.SetActive(true);
        _arrow.localRotation =
            _nextOnPath == _north ? _northRotation :
            _nextOnPath == _east ? _eastRotation :
            _nextOnPath == _south ? _southRotation :
            _westRotation;
    }
    public void HidePath() {
        _arrow.gameObject.SetActive(false);
    }

    public GameTile GrowPathToNorth() => GrowPathTo(_north, Direction.South);

    public GameTile GrowPathToSouth() => GrowPathTo(_south, Direction.North);

    public GameTile GrowPathToEast() => GrowPathTo(_east, Direction.West);

    public GameTile GrowPathToWest() => GrowPathTo(_west, Direction.East);

    private GameTile GrowPathTo(GameTile neighbor, Direction direction) {
        Debug.Assert(HasPath, "No Path");
        if (neighbor == null || neighbor.HasPath) {
            return null;
        }
        neighbor._distance = _distance + 1;
        neighbor._nextOnPath = this;
        neighbor.ExitPoint =
            neighbor.transform.localPosition + direction.GetHalfVector();
        neighbor.PathDirection = direction;
        return neighbor.Content.BlockPath ? null : neighbor;
    }
}
