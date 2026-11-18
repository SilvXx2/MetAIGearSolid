using TMPro;
using UnityEngine;

// Muestra el tiempo restante de la partida en el HUD; parpadea en rojo en los últimos segundos.
public class MatchTimerView : MonoBehaviour
{
    [Tooltip("GameObject con el componente que implementa IMatchTimer (por ejemplo GameFlow).")]
    [SerializeField] private GameObject timerSource;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float warningThreshold = 20f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.25f, 0.2f);

    private IMatchTimer timer;
    private int lastShownSecond = -1;

    private void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (timerSource != null) timer = timerSource.GetComponentInChildren<IMatchTimer>(true);

        if (timer == null)
        {
            Debug.LogError($"{name}: falta asignar un 'Timer Source' que implemente IMatchTimer.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        float remaining = Mathf.Max(0f, timer.RemainingSeconds);
        int seconds = Mathf.CeilToInt(remaining);

        if (seconds != lastShownSecond)
        {
            label.text = $"{seconds / 60:0}:{seconds % 60:00}";
            lastShownSecond = seconds;
        }

        if (remaining <= warningThreshold)
        {
            float blink = 0.5f + 0.5f * Mathf.Sin(Time.time * 8f);
            label.color = Color.Lerp(warningColor, Color.white, blink * 0.4f);
        }
        else
        {
            label.color = normalColor;
        }
    }
}
