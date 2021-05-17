using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    // Tunables
    [SerializeField] Rigidbody projectileRigidbody = null;
    [SerializeField] float launchForce = 10f;
    [SerializeField] float destroyAfterSeconds = 5.0f;

    private void Start()
    {
        Vector3 forceVector = transform.forward * launchForce;
        projectileRigidbody.AddForce(forceVector);
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
