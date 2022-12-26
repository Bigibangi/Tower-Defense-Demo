using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

    #region Fields

    private Mesh _hexmesh;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Color> _colors;
    private MeshCollider _meshCollider;

    #endregion Fields

    #region MonoBehaviour

    private void Awake() {
        GetComponent<MeshFilter>().mesh = _hexmesh = new Mesh();
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _hexmesh.name = "Hex Mesh";
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _colors = new List<Color>();
    }

    #endregion MonoBehaviour

    public void Triangulate(HexCell[] cells) {
        _hexmesh.Clear();
        _vertices.Clear();
        _triangles.Clear();
        _colors.Clear();
        for (int i = 0; i < cells.Length; i++) {
            Triangulate(cells[i]);
        }
        _hexmesh.vertices = _vertices.ToArray();
        _hexmesh.colors = _colors.ToArray();
        _hexmesh.triangles = _triangles.ToArray();
        _hexmesh.RecalculateNormals();
        _meshCollider.sharedMesh = _hexmesh;
    }

    private void Triangulate(HexCell hexCell) {
        for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
            Triangulate(d, hexCell);
        }
    }

    private void Triangulate(HexDirection direction, HexCell hexCell) {
        var center = hexCell.transform.localPosition;
        var v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        var v2 = center + HexMetrics.GetSecondSolidCorner(direction);
        AddTriangle(center, v1, v2);
        AddTriangleColor(hexCell.color);
        if (direction <= HexDirection.SE) {
            TriangulateConnection(direction, hexCell, v1, v2);
        }
    }

    private void TriangulateConnection(HexDirection direction, HexCell hexCell, Vector3 v1, Vector3 v2) {
        var neighbor = hexCell.GetNeighbor(direction);
        if (neighbor == null)
            return;
        var bridge = HexMetrics.GetBridge(direction);
        var v3 = v1 + bridge;
        var v4 = v2 + bridge;
        AddQuad(v1, v2, v3, v4);
        AddQuadColor(hexCell.color, neighbor.color);
        var nextNeighbor = hexCell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null) {
            AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(
                hexCell.color,
                neighbor.color,
                nextNeighbor.color);
        }
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
        var vertexIndex = _vertices.Count;
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }

    private void AddTriangleColor(Color color) {
        _colors.Add(color);
        _colors.Add(color);
        _colors.Add(color);
    }

    private void AddTriangleColor(Color c1, Color c2, Color c3) {
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c3);
    }

    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        var vertexIndex = _vertices.Count;
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
        _vertices.Add(v4);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

    private void AddQuadColor(Color c1, Color c2) {
        _colors.Add(c1);
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c2);
    }
}