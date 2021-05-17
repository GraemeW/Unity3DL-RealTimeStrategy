using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    // Tunables
    [SerializeField] UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] LayerMask layerMask = new LayerMask();

    // Cached References
    Camera mainCamera = null;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (CastAndTarget(ray)) { return; }
        if (CastAndMove(ray)) { return; }

    }

    private bool CastAndTarget(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
        foreach (RaycastHit currentHit in hits)
        {
            UnityEngine.Debug.Log(currentHit.collider.gameObject.name);
            // No treatment on top-most, just grab whatever one satisifies first
            if (currentHit.collider.TryGetComponent(out Targetable targetable))
            {
                if (targetable.hasAuthority) { continue; }
                TryTarget(targetable);
                return true;
            }
        }
        return false;
    }

    private bool CastAndMove(Ray ray)
    {
        // Default behavior:  Just Move
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return false; }
        TryMove(hit.point);
        return true;
    }

    private void TryMove(Vector3 point)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMover().CmdMove(point);
        }
    }

    private void TryTarget(Targetable targetable)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().CmdSetTarget(targetable.gameObject);
        }
    }
}
