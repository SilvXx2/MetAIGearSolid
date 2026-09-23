using UnityEngine;

public class GameEndController : MonoBehaviour, IGameEndHandler
{
    [Header("Catalogo de Escenas")]
    [Tooltip("GameObject que tiene el SceneLoader con la lista. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject sceneCatalogSource;

    [Header("Escenas de Final")]
    [Tooltip("Nombre exacto de la escena de victoria, tal cual figura en la lista del SceneLoader.")]
    [SerializeField] private string victorySceneName = "Victoria";

    [Tooltip("Nombre exacto de la escena de derrota, tal cual figura en la lista del SceneLoader.")]
    [SerializeField] private string defeatSceneName = "Derrota";

    private ISceneCatalog catalog;
    private bool hasEnded;

    private void Awake()
    {
        if (sceneCatalogSource == null) sceneCatalogSource = gameObject;

        catalog = sceneCatalogSource.GetComponentInChildren<ISceneCatalog>(true);

        if (catalog == null)
        {
            Debug.LogError($"{name}: '{sceneCatalogSource.name}' no tiene ningun componente que implemente ISceneCatalog (por ejemplo SceneLoader).", this);
        }
    }

    public void Report(GameResult result)
    {
        if (hasEnded || catalog == null) return;

        string sceneName = result == GameResult.Victory ? victorySceneName : defeatSceneName;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"{name}: falta el nombre de escena para el resultado '{result}'.", this);
            return;
        }

        hasEnded = true;
        catalog.Load(sceneName);
    }
}
