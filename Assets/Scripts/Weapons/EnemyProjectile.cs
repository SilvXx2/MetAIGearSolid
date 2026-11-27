using System;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Parámetros del Proyectil")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxLifetime = 2.5f;
    [SerializeField] private float hitRadius = 0.22f;

    private Transform shooterRoot;
    private Vector3 moveDirection;
    private float damage;
    private LayerMask obstacleMask;
    private float remainingLifetime;
    private bool initialized;

    public void Initialize(
        Transform shooter,
        Vector3 direction,
        float damageAmount,
        LayerMask wallMask,
        float overrideSpeed = -1f,
        float overrideLifetime = -1f)
    {
        shooterRoot = shooter != null ? shooter.root : null;

        direction.y = 0f;
        moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            moveDirection.Normalize();
            transform.forward = moveDirection;
        }

        damage = Mathf.Max(0f, damageAmount);
        obstacleMask = wallMask;
        if (overrideSpeed > 0f) speed = overrideSpeed;
        remainingLifetime = overrideLifetime > 0f ? overrideLifetime : maxLifetime;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            Destroy(gameObject);
            return;
        }

        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float step = speed * Time.deltaTime;
        if (step <= 0f) return;

        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            hitRadius,
            moveDirection,
            step,
            ~0,
            QueryTriggerInteraction.Ignore);

        if (hits.Length > 1)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;
            if (col == null) continue;
            if (shooterRoot != null && col.transform.root == shooterRoot) continue;

            if (IsInLayerMask(col.gameObject.layer, obstacleMask))
            {
                transform.position = hits[i].point;
                Destroy(gameObject);
                return;
            }

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                transform.position = hits[i].point;
                Destroy(gameObject);
                return;
            }
        }

        transform.position += moveDirection * step;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
