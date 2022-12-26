using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {
    public Color[] colors;
    public HexGrid hexgrid;

    private Color _activeColor;

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
            hexgrid.ColorCell(hit.point, _activeColor);
        }
    }

    public void SelectColor(int index) {
        _activeColor = colors[index];
    }
}