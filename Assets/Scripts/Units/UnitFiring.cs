using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    // Tunables
    [SerializeField] Targeter targeter = null;
    [SerializeField] GameObject projectilePrefab = null;
    [SerializeField] Transform projectileSpawnPoint = null;
    [SerializeField] float fireRange = 5.0f;
    [SerializeField] float fireRate = 1.0f;
    [SerializeField] float rotationSpeed = 20f;

    //Cached References

    // State
    float lastFireTime = 0f;

    [ServerCallback]
    private void Update()
    {
        Targetable targetable = targeter.GetTarget();

        if (!CanFireAtTarget(targetable)) { return; }

        Quaternion targetRotation = Quaternion.LookRotation(targetable.transform.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate + lastFireTime))
        {
            Quaternion projectileRotation = Quaternion.LookRotation(targetable.GetAimAtPoint().position - projectileSpawnPoint.position);
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            projectileInstance.transform.SetParent(null);
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget(Targetable targetable)
    {
        if (targeter.GetTarget() == null) { return false; }
        return ((targetable.transform.position - transform.position).sqrMagnitude < fireRange * fireRange);
    }
}
