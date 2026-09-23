using UnityEngine;

public class HealthEndTrigger : MonoBehaviour
{
    [Header("Final que Dispara")]
    [SerializeField] private GameResult result = GameResult.Defeat;

    [Header("Vida que Vigila")]
    [Tooltip("GameObject que tiene el Health. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject healthSource;

    [Header("Quien Recibe el Final")]
    [Tooltip("GameObject que tiene el GameEndController. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject endHandlerSource;

    private IHealthObservable health;
    private IGameEndHandler handler;

    private void Awake()
    {
        if (healthSource == null) healthSource = gameObject;
        if (endHandlerSource == null) endHandlerSource = gameObject;

        health = healthSource.GetComponentInChildren<IHealthObservable>(true);

        if (health == null)
        {
            Debug.LogError($"{name}: '{healthSource.name}' no tiene ningun componente que implemente IHealthObservable (por ejemplo Health).", this);
            enabled = false;
            return;
        }

        handler = endHandlerSource.GetComponentInChildren<IGameEndHandler>(true);

        if (handler == null)
        {
            Debug.LogError($"{name}: '{endHandlerSource.name}' no tiene ningun componente que implemente IGameEndHandler (por ejemplo GameEndController).", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        health.Died += HandleDied;
    }

    private void OnDisable()
    {
        health.Died -= HandleDied;
    }

    private void HandleDied()
    {
        handler.Report(result);
    }
}
