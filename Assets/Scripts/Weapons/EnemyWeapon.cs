using UnityEngine;

public class EnemyWeapon : MonoBehaviour, IReloadableWeapon
{
    [Header("Configuración del Arma")]
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private int currentAmmo = 5;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float reloadDuration = 3.5f;
    [SerializeField] private float directDamage = 20f;

    [Header("Feedback Visual del Arma")]
    [Tooltip("Objeto visual opcional del arma equipada en mano (distinto del destello de ataque).")]
    [SerializeField] private GameObject equippedHolsterVisual;

    private IAttackEffect attackEffect;
    private IWeaponFireMechanism fireMechanism;
    private IVisionSensor visionSensor;
    private float reloadTimer;
    private bool isReloading;

    public int MaxAmmo => Mathf.Max(1, maxAmmo);
    public int CurrentAmmo => Mathf.Clamp(currentAmmo, 0, MaxAmmo);
    public float AttackRange => Mathf.Max(0.5f, attackRange);
    public float ReloadDuration => Mathf.Max(0.1f, reloadDuration);
    public float ReloadTimer => reloadTimer;
    public bool CanFire => CurrentAmmo > 0 && !isReloading;

    private void Awake()
    {
        attackEffect = GetComponent<IAttackEffect>();
        fireMechanism = GetComponent<IWeaponFireMechanism>();
        visionSensor = GetComponent<IVisionSensor>();
    }

    public Vector3 GetMuzzlePosition(Transform attacker)
    {
        if (fireMechanism == null && attacker != null)
        {
            fireMechanism = attacker.GetComponent<IWeaponFireMechanism>();
        }

        if (fireMechanism != null)
        {
            return fireMechanism.GetMuzzlePosition(attacker);
        }

        if (equippedHolsterVisual != null)
        {
            return equippedHolsterVisual.transform.position;
        }

        return attacker != null ? attacker.position + Vector3.up * 1.1f : transform.position + Vector3.up * 1.1f;
    }

    public void Configure(
        int configuredMaxAmmo,
        int configuredCurrentAmmo,
        float configuredAttackRange,
        float configuredReloadDuration,
        IAttackEffect effectOverride = null,
        IWeaponFireMechanism fireMechanismOverride = null)
    {
        maxAmmo = Mathf.Max(1, configuredMaxAmmo);
        currentAmmo = Mathf.Clamp(configuredCurrentAmmo, 0, maxAmmo);
        attackRange = Mathf.Max(0.5f, configuredAttackRange);
        reloadDuration = Mathf.Max(0.1f, configuredReloadDuration);

        if (effectOverride != null)
        {
            attackEffect = effectOverride;
        }
        else if (attackEffect == null)
        {
            attackEffect = GetComponent<IAttackEffect>();
        }

        if (fireMechanismOverride != null)
        {
            fireMechanism = fireMechanismOverride;
        }
        else if (fireMechanism == null)
        {
            fireMechanism = GetComponent<IWeaponFireMechanism>();
        }

        if (visionSensor == null)
        {
            visionSensor = GetComponent<IVisionSensor>();
        }
    }

    public bool TryAttack(Transform attacker, Transform target)
    {
        Vector3 aimDir = Vector3.zero;
        if (attacker != null && target != null)
        {
            aimDir = target.position - attacker.position;
            aimDir.y = 0f;
        }
        else if (attacker != null)
        {
            aimDir = attacker.forward;
            aimDir.y = 0f;
        }

        return TryAttack(attacker, target, aimDir);
    }

    public bool TryAttack(Transform attacker, Transform target, Vector3 aimDirection)
    {
        if (!CanFire) return false;

        currentAmmo = Mathf.Max(0, currentAmmo - 1);

        if (attackEffect == null && attacker != null)
        {
            attackEffect = attacker.GetComponent<IAttackEffect>();
        }

        if (fireMechanism == null && attacker != null)
        {
            fireMechanism = attacker.GetComponent<IWeaponFireMechanism>();
        }

        if (visionSensor == null && attacker != null)
        {
            visionSensor = attacker.GetComponent<IVisionSensor>();
        }

        attackEffect?.TriggerEffect();

        if (fireMechanism != null)
        {
            LayerMask obstacleMask = visionSensor != null ? visionSensor.ObstacleMask : LayerMask.GetMask("ObstacleMask");
            fireMechanism.ExecuteFire(attacker, target, aimDirection, directDamage, obstacleMask);
        }
        else if (attackEffect == null && target != null && directDamage > 0f)
        {
            var damageable = target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(directDamage);
        }

        return true;
    }

    public void StartReload()
    {
        if (isReloading) return;
        isReloading = true;
        reloadTimer = 0f;
    }

    public bool TickReload(float deltaTime)
    {
        if (!isReloading)
        {
            isReloading = true;
            reloadTimer = 0f;
        }

        reloadTimer += Mathf.Max(0f, deltaTime);
        if (reloadTimer >= ReloadDuration)
        {
            RefillImmediate();
            return true;
        }

        return false;
    }

    public void CancelReload()
    {
        isReloading = false;
        reloadTimer = 0f;
    }

    public void RefillImmediate()
    {
        currentAmmo = MaxAmmo;
        isReloading = false;
        reloadTimer = 0f;
    }

    public void SetAmmo(int current, int max)
    {
        maxAmmo = Mathf.Max(1, max);
        currentAmmo = Mathf.Clamp(current, 0, maxAmmo);
    }

    public void SetEquippedVisual(bool equipped)
    {
        if (equippedHolsterVisual != null)
        {
            equippedHolsterVisual.SetActive(equipped);
        }
    }
}
