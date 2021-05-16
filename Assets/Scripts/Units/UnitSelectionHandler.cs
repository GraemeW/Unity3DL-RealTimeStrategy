using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] LayerMask layerMask = new LayerMask();

    // Cached References
    Camera mainCamera = null;

    // State
    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            DeselectUnits();
            // TODO:  Start selection area
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
    }

    private void ClearSelectionArea()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
        if (!hit.collider.TryGetComponent(out Unit unit)) { return; }
        if (!unit.hasAuthority) { return; }

        SelectUnits(unit);
    }

    private void SelectUnits(Unit unit)
    {
        SelectedUnits.Add(unit);
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Select();
        }
    }

    private void DeselectUnits()
    {
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }
        SelectedUnits.Clear();
    }
}
