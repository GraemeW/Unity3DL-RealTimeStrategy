using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitMover : NetworkBehaviour
{
    // Tunables
    [SerializeField] NavMeshAgent navMeshAgent = null;

    #region Server
    [Command]
    public void CmdMove(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        navMeshAgent.SetDestination(position);
    }

    [Command]
    public void CmdWarp(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        navMeshAgent.Warp(position);
    }

    #endregion
}
