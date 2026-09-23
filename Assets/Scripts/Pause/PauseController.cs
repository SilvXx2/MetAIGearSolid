using UnityEngine;

public class PauseController : MonoBehaviour, IPauseService
{
    private IPauseView view;
    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        view = GetComponent<IPauseView>();

        if (view == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IPauseView (por ejemplo PausePanelView).", this);
        }
    }

    private void Start()
    {
        if (view != null) view.Show(isPaused);
    }

    public void SetPaused(bool paused)
    {
        if (paused == isPaused) return;

        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (view != null) view.Show(isPaused);
    }

    private void OnDisable()
    {
        if (isPaused) Time.timeScale = 1f;
    }
}
