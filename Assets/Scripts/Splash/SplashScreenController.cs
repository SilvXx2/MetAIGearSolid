using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Splash inicial con los datos de los desarrolladores: fade in, espera, fade out y carga del Main Screen.
// Se puede saltear con cualquier tecla o click.
public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private CanvasGroup content;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float holdDuration = 3.5f;

    [Header("Escena Siguiente")]
    [Tooltip("GameObject que tiene el SceneLoader con la lista. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject sceneCatalogSource;
    [SerializeField] private string nextSceneName = "Menu";

    private ISceneCatalog catalog;
    private bool skipRequested;
    private bool loading;

    private void Awake()
    {
        if (sceneCatalogSource == null) sceneCatalogSource = gameObject;
        catalog = sceneCatalogSource.GetComponentInChildren<ISceneCatalog>(true);

        if (catalog == null)
        {
            Debug.LogError($"{name}: '{sceneCatalogSource.name}' no tiene ningun componente que implemente ISceneCatalog (por ejemplo SceneLoader).", this);
        }

        if (content != null) content.alpha = 0f;
    }

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (keyPressed || clicked) skipRequested = true;
    }

    private IEnumerator PlaySequence()
    {
        yield return Fade(0f, 1f);

        float timer = 0f;
        while (timer < holdDuration && !skipRequested)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return Fade(content != null ? content.alpha : 1f, 0f);
        LoadNext();
    }

    private IEnumerator Fade(float from, float to)
    {
        if (content == null) yield break;

        float timer = 0f;
        float duration = skipRequested ? fadeDuration * 0.3f : fadeDuration;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            content.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        content.alpha = to;
    }

    private void LoadNext()
    {
        if (loading || catalog == null) return;
        loading = true;
        catalog.Load(nextSceneName);
    }
}
