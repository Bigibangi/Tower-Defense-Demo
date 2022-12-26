using UnityEngine;

public class HexCell : MonoBehaviour {

    [SerializeField]
    private HexCell[] _neighbors;

    public HexCoordinates coordinates;
    public Color color;

    public HexCell GetNeighbor(HexDirection direction) {
        return _neighbors[(int) direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell neighbor) {
        _neighbors[(int) direction] = neighbor;
        neighbor._neighbors[(int) direction.Opposite()] = this;
    }
}