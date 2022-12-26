using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {
    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;
    public Text textPrefab;

    public Color defaultColor = Color.white;
    private Canvas _gridCanvas;
    private HexCell[] _cells;
    private HexMesh _hexMesh;

    #region MonoBehavior

    private void Awake() {
        _gridCanvas = GetComponentInChildren<Canvas>();
        _hexMesh = GetComponentInChildren<HexMesh>();
        _cells = new HexCell[height * width];
        for (int z = 0, i = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                CreateCells(x, z, i++);
            }
        }
    }

    private void Start() {
        _hexMesh.Triangulate(_cells);
    }

    #endregion MonoBehavior

    private void CreateCells(int x, int z, int i) {
        Vector3 position;
        position.x = (x + 0.5f * z - z / 2) * (HexMetrics.INNER_RADIUS * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.OUTER_RADIUS * 1.5f);

        var cell = _cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;
        if (x > 0) {
            cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
        }
        if (z > 0) {
            if ((z & 1) == 0) {
                cell.SetNeighbor(HexDirection.SE, _cells[i - width]);
                if (x > 0) {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - width - 1]);
                }
            }
            else {
                cell.SetNeighbor(HexDirection.SW, _cells[i - width]);
                if (x < width - 1) {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - width + 1]);
                }
            }
        }
        var label = Instantiate<Text>(textPrefab);
        label.rectTransform.SetParent(_gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLine();
        cell.uiRect = label.rectTransform;
    }

    public HexCell GetCell(Vector3 position, Color color) {
        position = transform.InverseTransformPoint(position);
        var coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        return _cells[index];
    }

    public void Refresh() {
        _hexMesh.Triangulate(_cells);
    }
}