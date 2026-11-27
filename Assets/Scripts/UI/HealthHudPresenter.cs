using UnityEngine;

// Presenter MVP que vincula IHealthObservable (Modelo) con IHealthHudView (Vista).
public class HealthHudPresenter : MonoBehaviour
{
    [Header("Fuente de Vida")]
    [Tooltip("GameObject que tiene un componente IHealthObservable (normalmente el Player). Si está vacío se busca automáticamente.")]
    [SerializeField] private GameObject healthSource;

    private IHealthObservable healthObservable;
    private IHealthHudView view;
    private float lastKnownHealth;
    private bool hasStarted;

    private void Awake()
    {
        view = GetComponent<IHealthHudView>();
        if (view == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IHealthHudView (por ejemplo HealthHudCanvasView).", this);
            enabled = false;
            return;
        }

        if (healthSource == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                healthSource = player.gameObject;
            }
        }

        if (healthSource != null)
        {
            healthObservable = healthSource.GetComponentInChildren<IHealthObservable>(true);
        }

        if (healthObservable == null)
        {
            Debug.LogError($"{name}: no se encontró ningún componente IHealthObservable para mostrar en el HUD.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        hasStarted = true;
        SyncInitialHealth();
    }

    private void OnEnable()
    {
        if (healthObservable == null || view == null) return;

        healthObservable.HealthChanged += HandleHealthChanged;
        if (hasStarted)
        {
            SyncInitialHealth();
        }
    }

    private void SyncInitialHealth()
    {
        if (healthObservable == null || view == null) return;
        lastKnownHealth = healthObservable.CurrentHealth;
        view.Render(healthObservable.CurrentHealth, healthObservable.MaxHealth);
    }

    private void OnDisable()
    {
        if (healthObservable == null) return;
        healthObservable.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        view.Render(currentHealth, maxHealth);
        if (currentHealth < lastKnownHealth)
        {
            view.PlayDamageFlash();
        }

        lastKnownHealth = currentHealth;
    }
}
