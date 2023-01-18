using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

    public float springForce = 20f,
        damping = 5f;

    private Mesh _deformingMesh;
    private float _uniformScale = 1f;

    private Vector3[] _originalVertices,
        _displacedVertices,
        _vertexVelocities;

    private void Start() {
        _deformingMesh = GetComponent<MeshFilter>().mesh;
        _originalVertices = _deformingMesh.vertices;
        _displacedVertices = new Vector3[_originalVertices.Length];
        _vertexVelocities = new Vector3[_originalVertices.Length];
        for (int i = 0; i < _originalVertices.Length; i++) {
            _displacedVertices[i] = _originalVertices[i];
        }
    }

    private void Update() {
        _uniformScale = transform.localScale.x;
        for (int i = 0; i < _displacedVertices.Length; i++) {
            UpdateVertex(i);
        }
        _deformingMesh.vertices = _displacedVertices;
        _deformingMesh.RecalculateNormals();
    }

    private void UpdateVertex(int i) {
        var velocity = _vertexVelocities[i];
        var displacement = _displacedVertices[i] - _originalVertices[i];
        displacement *= _uniformScale;
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        _vertexVelocities[i] = velocity;
        _displacedVertices[i] += velocity * (Time.deltaTime / _uniformScale);
    }

    public void AddDeformingForce(Vector3 point, float force) {
        point = transform.InverseTransformDirection(point);
        for (int i = 0; i < _displacedVertices.Length; i++) {
            AddForceToVertex(i, point, force);
        }
    }

    private void AddForceToVertex(int i, Vector3 point, float force) {
        var pointToVertex = _displacedVertices[i] - point;
        pointToVertex *= _uniformScale;
        var attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        var velocity = attenuatedForce * Time.deltaTime;
        _vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
}