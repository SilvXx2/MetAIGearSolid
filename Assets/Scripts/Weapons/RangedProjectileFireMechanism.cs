using UnityEngine;

public class RangedProjectileFireMechanism : MonoBehaviour, IWeaponFireMechanism
{
    [Header("Disparo de Proyectiles")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Vector3 fallbackMuzzleOffset = new Vector3(0.24f, 0.25f, 0.95f);
    [SerializeField] private float muzzleForwardPadding = 0.45f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 2.5f;
    [SerializeField] private Color fallbackTracerColor = new Color(1f, 0.45f, 0.1f, 1f);

    public Vector3 GetMuzzlePosition(Transform attacker)
    {
        if (muzzlePoint != null)
        {
            return muzzlePoint.position + muzzlePoint.forward * muzzleForwardPadding;
        }

        if (attacker != null)
        {
            return attacker.TransformPoint(fallbackMuzzleOffset);
        }

        return transform.position;
    }

    public bool ExecuteFire(Transform attacker, Transform target, Vector3 aimDirection, float damage, LayerMask obstacleMask)
    {
        Vector3 spawnPosition = GetMuzzlePosition(attacker);
        Vector3 direction = aimDirection;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            if (target != null)
            {
                direction = target.position - spawnPosition;
                direction.y = 0f;
            }
            else if (attacker != null)
            {
                direction = attacker.forward;
                direction.y = 0f;
            }
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector3.forward;
        }
        else
        {
            direction.Normalize();
        }

        EnemyProjectile projectile = projectilePrefab != null
            ? Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction))
            : CreateFallbackProjectile(spawnPosition, direction);

        projectile.Initialize(attacker, direction, damage, obstacleMask, projectileSpeed, projectileLifetime);
        return true;
    }

    private EnemyProjectile CreateFallbackProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        GameObject bulletObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulletObj.name = "EnemyBullet_Runtime";
        bulletObj.transform.position = spawnPosition;
        bulletObj.transform.rotation = Quaternion.LookRotation(direction);
        bulletObj.transform.localScale = new Vector3(0.22f, 0.22f, 0.55f);

        Collider col = bulletObj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer rend = bulletObj.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = FeedbackMaterials.CreateTransparentVertexColor("EnemyBulletMat");
            mat.color = fallbackTracerColor;
            rend.sharedMaterial = mat;
        }

        TrailRenderer trail = bulletObj.AddComponent<TrailRenderer>();
        trail.time = 0.16f;
        trail.startWidth = 0.16f;
        trail.endWidth = 0.02f;
        trail.material = FeedbackMaterials.CreateTransparentVertexColor("EnemyBulletTrailMat");
        trail.startColor = fallbackTracerColor;
        trail.endColor = new Color(fallbackTracerColor.r, fallbackTracerColor.g, fallbackTracerColor.b, 0f);

        return bulletObj.AddComponent<EnemyProjectile>();
    }
}
