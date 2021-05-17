using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{

    // State
    [SerializeField] Targetable target = null;

    public Targetable GetTarget()
    {
        return target;
    }

    #region Server

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent(out Targetable target)) { return; }

        this.target = target;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion
}
