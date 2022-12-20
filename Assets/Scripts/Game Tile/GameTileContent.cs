using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour {

    [SerializeField]
    private GameTileContentType _type = default;

    public GameTileContentType Type => _type;
    public bool BlockPath =>
        Type == GameTileContentType.Wall ||
        Type == GameTileContentType.Tower;

    GameTileContentFactory _originFactory;

    public GameTileContentFactory OriginFactory {
        get => _originFactory;
        set {
            Debug.Assert(_originFactory == null, "Redefined Factory");
            _originFactory = value;
        }
    }

    public void Recycle() {
        _originFactory.Reclaim(this);
    }

    public virtual void GameUpdate() { }
}