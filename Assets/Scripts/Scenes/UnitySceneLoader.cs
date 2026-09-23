using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitySceneLoader : MonoBehaviour, ISceneLoader
{
    [Header("Modo de Carga")]
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    public void Load(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError($"{name}: la escena no tiene ninguna ruta asignada.", this);
            return;
        }

        if (!IsInBuildSettings(scenePath))
        {
            Debug.LogError($"{name}: la escena '{scenePath}' no esta agregada en Build Settings, Unity no la puede cargar.", this);
            return;
        }

        SceneManager.LoadScene(scenePath, loadMode);
    }

    private static bool IsInBuildSettings(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneUtility.GetScenePathByBuildIndex(i) == scenePath) return true;
        }

        return false;
    }
}
