using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {
    public Color[] colors;
    public HexGrid hexgrid;

    private Color _activeColor;
    private int _activeElevation;

    #region Monobehaviour

    private void Awake() {
        SelectColor(0);
    }

    private void Update() {
        //migrate to Unity.InputSystem
        if (Input.GetMouseButtonDown(0) &&
                !EventSystem.current.IsPointerOverGameObject()) {
            HandleInput();
        }
    }

    #endregion Monobehaviour

    private void HandleInput() {
        //DI
        var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit)) {
            EditCell(hexgrid.GetCell(hit.point, _activeColor));
        }
    }

    public void SelectColor(int index) {
        _activeColor = colors[index];
    }

    public void SetElevation(float elevation) {
        _activeElevation = (int) elevation;
    }

    private void EditCell(HexCell hexCell) {
        hexCell.color = _activeColor;
        hexCell.Elevation = _activeElevation;
        hexgrid.Refresh();
    }
}