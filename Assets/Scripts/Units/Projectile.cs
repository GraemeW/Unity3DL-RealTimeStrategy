using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    // Tunables
    [Header("Projectile Behavior")]
    [SerializeField] Rigidbody projectileRigidbody = null;
    [SerializeField] float launchForce = 10f;
    [SerializeField] float destroyAfterSeconds = 5.0f;
    [Header("Projectile Properties")]
    [SerializeField] int damageToDeal = 20;

    private void Start()
    {
        Vector3 forceVector = transform.forward * launchForce;
        projectileRigidbody.AddForce(forceVector);
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            if (networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
            DestroySelf();
        }
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
