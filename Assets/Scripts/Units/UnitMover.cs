using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitMover : NetworkBehaviour
{
    // Tunables
    [SerializeField] NavMeshAgent navMeshAgent = null;
    [SerializeField] Targeter targeter = null;
    [SerializeField] float chaseRange = 10f;

    #region Server
    [ServerCallback]
    private void Update()
    {
        Targetable targetable = targeter.GetTarget();

        if (TargetingBehavior(targetable)) { return; }
        if (MovementBehavior()) { return; }
    }

    private bool TargetingBehavior(Targetable targetable)
    {
        if (targetable != null)
        {
            if((targetable.transform.position - transform.position).sqrMagnitude > (chaseRange * chaseRange)) // avoid sqrt -> distance every frame
            {
                navMeshAgent.SetDestination(targetable.transform.position);
            }
            else if (navMeshAgent.hasPath)
            {
                navMeshAgent.ResetPath();
            }
            return true;
        }
        return false;
    }

    private bool MovementBehavior()
    {
        if (!navMeshAgent.hasPath) { return false; }
        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance) { navMeshAgent.ResetPath(); return false; }

        return true;
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        navMeshAgent.SetDestination(position);
    }

    #endregion
}
