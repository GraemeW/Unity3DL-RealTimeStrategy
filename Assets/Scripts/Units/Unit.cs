using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] UnitMover unitMover = null;
    [SerializeField] UnityEvent onSelected = null;
    [SerializeField] UnityEvent onDeselected = null;

    public UnitMover GetUnitMover()
    {
        return unitMover;
    }

    #region Client
    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }

        if (onSelected != null)
        {
            onSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (!hasAuthority) { return; }

        if (onDeselected != null)
        {
            onDeselected.Invoke();
        }
    }
    #endregion
}
