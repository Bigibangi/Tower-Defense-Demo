using System;
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
        var center = hexCell.transform.localPosition;
        for (int i = 0; i < 6; i++) {
            AddTriangle(
                center,
                center + HexMetrics.corners[i],
                center + HexMetrics.corners[i + 1]);
            AddTriangleColor(hexCell.color);
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
}