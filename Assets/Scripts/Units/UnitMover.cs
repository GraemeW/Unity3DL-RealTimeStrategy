using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMover : NetworkBehaviour
{
    // Tunables
    [SerializeField] NavMeshAgent navMeshAgent = null;

    // Cached References
    Camera mainCamera = null;

    #region Server
    [Command]
    private void CmdMove(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        navMeshAgent.SetDestination(position);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!hasAuthority) { return; }

        if (HandleMouseInput()) { return; }
    }

    private bool HandleMouseInput()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return false; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return false; }

        CmdMove(hit.point);
        return true;
    }

    #endregion
}
