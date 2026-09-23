using UnityEngine;
using UnityEngine.InputSystem;

public class PauseToggleInput : MonoBehaviour
{
    [Header("Tecla")]
    [SerializeField] private Key pauseKey = Key.Escape;

    [Header("Quien Maneja la Pausa")]
    [Tooltip("GameObject que tiene el PauseController. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject pauseSource;

    private IPauseService pause;

    private void Awake()
    {
        if (pauseSource == null) pauseSource = gameObject;

        pause = pauseSource.GetComponentInChildren<IPauseService>(true);

        if (pause == null)
        {
            Debug.LogError($"{name}: '{pauseSource.name}' no tiene ningun componente que implemente IPauseService (por ejemplo PauseController).", this);
            enabled = false;
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[pauseKey].wasPressedThisFrame)
        {
            pause.SetPaused(!pause.IsPaused);
        }
    }
}
