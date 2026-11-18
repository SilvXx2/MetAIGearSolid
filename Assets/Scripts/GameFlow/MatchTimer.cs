using UnityEngine;

// Temporizador de partida (consigna: entre 40 y 180 segundos). Al agotarse reporta el resultado configurado
// al IGameEndHandler (por defecto: derrota, porque no se llegó a la zona de extracción a tiempo).
public class MatchTimer : MonoBehaviour, IMatchTimer
{
    private const float MinDuration = 40f;
    private const float MaxDuration = 180f;

    [Header("Duración de la Partida (40s a 180s)")]
    [SerializeField] private float duration = 150f;

    [Header("Resultado al Agotarse el Tiempo")]
    [SerializeField] private GameResult resultOnTimeout = GameResult.Defeat;

    [Header("Quien Recibe el Final")]
    [Tooltip("GameObject que tiene el GameEndController. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject endHandlerSource;

    private IGameEndHandler handler;
    private float remaining;
    private bool finished;

    public float Duration => duration;
    public float RemainingSeconds => remaining;
    public bool IsRunning => !finished;

    private void Awake()
    {
        if (endHandlerSource == null) endHandlerSource = gameObject;
        handler = endHandlerSource.GetComponentInChildren<IGameEndHandler>(true);

        if (handler == null)
        {
            Debug.LogError($"{name}: '{endHandlerSource.name}' no tiene ningun componente que implemente IGameEndHandler (por ejemplo GameEndController).", this);
        }

        remaining = duration;
    }

    private void Update()
    {
        if (finished) return;

        remaining -= Time.deltaTime;
        if (remaining > 0f) return;

        remaining = 0f;
        finished = true;
        handler?.Report(resultOnTimeout);
    }

    private void OnValidate()
    {
        duration = Mathf.Clamp(duration, MinDuration, MaxDuration);
    }
}
