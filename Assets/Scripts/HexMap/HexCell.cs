using UnityEngine;

public class HexCell : MonoBehaviour {

    [SerializeField]
    private HexCell[] _neighbors;

    public HexCoordinates coordinates;
    public Color color;
    public RectTransform uiRect;

    private int _elevation;

    public int Elevation {
        get {
            return _elevation;
        }
        set {
            _elevation = value;
            var position = transform.localPosition;
            position.y = value * HexMetrics.ELEVATION_STEP;
            transform.localPosition = position;
            var uiPosition = uiRect.localPosition;
            uiPosition.z = _elevation * -HexMetrics.ELEVATION_STEP;
            uiRect.localPosition = uiPosition;
        }
    }

    public HexCell GetNeighbor(HexDirection direction) {
        return _neighbors[(int) direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell neighbor) {
        _neighbors[(int) direction] = neighbor;
        neighbor._neighbors[(int) direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection direction) {
        return HexMetrics.GetEdgeType(_elevation, _neighbors[(int) direction]._elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell) {
        return HexMetrics.GetEdgeType(_elevation, otherCell._elevation);
    }
}