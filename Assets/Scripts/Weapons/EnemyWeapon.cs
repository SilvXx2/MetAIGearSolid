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
    }

    public void Configure(
        int configuredMaxAmmo,
        int configuredCurrentAmmo,
        float configuredAttackRange,
        float configuredReloadDuration,
        IAttackEffect effectOverride = null)
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
    }

    public bool TryAttack(Transform attacker, Transform target)
    {
        if (!CanFire) return false;

        currentAmmo = Mathf.Max(0, currentAmmo - 1);

        if (attackEffect == null && attacker != null)
        {
            attackEffect = attacker.GetComponent<IAttackEffect>();
        }

        if (attackEffect != null)
        {
            attackEffect.TriggerEffect();
        }
        else if (target != null && directDamage > 0f)
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
