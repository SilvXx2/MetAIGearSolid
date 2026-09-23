using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour, ISceneCatalog
{
    [Header("Escenas")]
    [SerializeField] private List<SceneReference> scenes = new List<SceneReference>();

    private ISceneLoader loader;

    private void Awake()
    {
        loader = GetComponent<ISceneLoader>();

        if (loader == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente ISceneLoader (por ejemplo UnitySceneLoader).", this);
        }
    }

    public void Load(string sceneName)
    {
        if (loader == null) return;

        foreach (SceneReference scene in scenes)
        {
            if (scene.SceneName == sceneName)
            {
                loader.Load(scene.ScenePath);
                return;
            }
        }

        Debug.LogError($"{name}: la escena '{sceneName}' no esta en la lista.", this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (SceneReference scene in scenes)
        {
            scene.SyncFromAsset();
        }
    }
#endif
}
