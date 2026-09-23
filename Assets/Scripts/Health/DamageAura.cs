using System.Collections.Generic;
using UnityEngine;

public class DamageAura : MonoBehaviour
{
    [Header("Alcance")]
    [SerializeField] private Transform auraOrigin;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private LayerMask targetMask = ~0;

    [Header("Daño")]
    [SerializeField] private float damagePerTick = 20f;
    [SerializeField] private float tickInterval = 0.5f;

    private readonly HashSet<IDamageable> damagedThisTick = new HashSet<IDamageable>();
    private float tickTimer;

    private Transform Origin => auraOrigin != null ? auraOrigin : transform;

    private void OnEnable()
    {
        tickTimer = 0f;
    }

    private void Update()
    {
        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;

        ApplyDamageInRadius();
        tickTimer = Mathf.Max(0.05f, tickInterval);
    }

    private void ApplyDamageInRadius()
    {
        Collider[] hits = Physics.OverlapSphere(Origin.position, radius, targetMask, QueryTriggerInteraction.Collide);
        damagedThisTick.Clear();

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform.root)) continue;

            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null) continue;
            if (!damagedThisTick.Add(damageable)) continue;

            damageable.TakeDamage(damagePerTick);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(Origin.position, radius);
    }
}
