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

    private void TriangulateConnection(
        HexDirection direction,
        HexCell hexCell,
        Vector3 v1,
        Vector3 v2) {
        var neighbor = hexCell.GetNeighbor(direction);
        if (neighbor == null)
            return;
        var bridge = HexMetrics.GetBridge(direction);
        var v3 = v1 + bridge;
        var v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Elevation * HexMetrics.ELEVATION_STEP;
        if (hexCell.GetEdgeType(direction) == HexEdgeType.Slope) {
            TriangulateEdgeTerraces(v1, v2, hexCell, v3, v4, neighbor);
        }
        else {
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(hexCell.color, neighbor.color);
        }
        var nextNeighbor = hexCell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null) {
            var v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.ELEVATION_STEP;
            if (hexCell.Elevation <= neighbor.Elevation) {
                if (hexCell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(
                        v2, hexCell, v4, neighbor, v5, nextNeighbor);
                }
                else {
                    TriangulateCorner(
                        v5, nextNeighbor, v2, hexCell, v4, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, hexCell);
            }
            else {
                TriangulateCorner(v5, nextNeighbor, v2, hexCell, v4, neighbor);
            }
            //AddTriangle(v2, v4, v5);
            //AddTriangleColor(
            //    hexCell.color,
            //    neighbor.color,
            //    nextNeighbor.color);
        }
    }

    private void TriangulateEdgeTerraces(
        Vector3 beginLeft,
        Vector3 beginRight,
        HexCell beginCell,
        Vector3 endLeft,
        Vector3 endRight,
        HexCell endCell) {
        var v3 = HexMetrics.TerraceLerp(beginLeft,endLeft,1);
        var v4 = HexMetrics.TerraceLerp(beginRight,endRight,1);
        var c2 = HexMetrics.TerraceLerp(beginCell.color,endCell.color,1);
        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.color, c2);
        for (int i = 2; i < HexMetrics.TERRACE_STEPS; i++) {
            var v1 = v3;
            var v2 = v4;
            var c1 = c2;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }
        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.color);
    }

    private void TriangulateCorner(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell) {
        var leftEdgeType = bottomCell.GetEdgeType(leftCell);
        var rightEdgeType = bottomCell.GetEdgeType(rightCell);
        if (leftEdgeType == HexEdgeType.Slope) {
            if (rightEdgeType == HexEdgeType.Slope) {
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat) {
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
            else {
                TriangulateCornerTerracesCliff(
                bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (rightEdgeType == HexEdgeType.Slope) {
            if (leftEdgeType == HexEdgeType.Flat) {
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else {
                TriangulateCornerCliffTerraces(
                bottom, bottomCell, left, leftCell, right, rightCell
            );
            }
        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
            if (leftCell.Elevation < rightCell.Elevation) {
                TriangulateCornerCliffTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
        }
        else {
            AddTriangle(bottom, left, right);
            AddTriangleColor(
                bottomCell.color,
                leftCell.color,
                rightCell.color);
        }
    }

    private void TriangulateCornerTerraces(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell) {
        var v3 = HexMetrics.TerraceLerp(bottom,left,1);
        var v4 = HexMetrics.TerraceLerp(bottom,right,1);
        var c3 = HexMetrics.TerraceLerp(bottomCell.color,leftCell.color,1);
        var c4 = HexMetrics.TerraceLerp(bottomCell.color,rightCell.color,1);
        AddTriangle(bottom, v3, v4);
        AddTriangleColor(bottomCell.color, c3, c4);
        for (int i = 2; i < HexMetrics.TERRACE_STEPS; i++) {
            var v1 = v3;
            var v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(bottom, left, i);
            v4 = HexMetrics.TerraceLerp(bottom, right, i);
            c3 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(bottomCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }
        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);
    }

    private void TriangulateCornerTerracesCliff(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell) {
        var b = 1f / (rightCell.Elevation - bottomCell.Elevation);
        if (b < 0) {
            b = -b;
        }
        var boundary = Vector3.Lerp(bottom,right,b);
        var boundaryColor=Color.Lerp(bottomCell.color,rightCell.color,b);
        TriangulateBoundaryTriangle(bottom, bottomCell, left, leftCell, boundary, boundaryColor);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    private void TriangulateCornerCliffTerraces(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell) {
        var b = 1f / (leftCell.Elevation - bottomCell.Elevation);
        if (b < 0) {
            b = -b;
        }
        var boundary = Vector3.Lerp(bottom,left,b);
        var boundaryColor=Color.Lerp(bottomCell.color,leftCell.color,b);
        TriangulateBoundaryTriangle(right, rightCell, bottom, bottomCell, boundary, boundaryColor);
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    private void TriangulateBoundaryTriangle(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 boundary,
        Color boundaryColor) {
        var v2 = HexMetrics.TerraceLerp(bottom,left,1);
        var c2 = HexMetrics.TerraceLerp(bottomCell.color,leftCell.color,1);
        AddTriangle(bottom, v2, boundary);
        AddTriangleColor(bottomCell.color, c2, boundaryColor);
        for (int i = 2; i < HexMetrics.TERRACE_STEPS; i++) {
            var v1 = v2;
            var c1 = c2;
            v2 = HexMetrics.TerraceLerp(bottom, left, i);
            c2 = HexMetrics.TerraceLerp(bottomCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }
        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
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

    private void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c3);
        _colors.Add(c4);
    }
}