using System.IO;
using UnityEngine;

[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [Tooltip("Arrastra aca el archivo .unity de la escena.")]
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
#endif

    [Tooltip("Se completa solo al asignar la escena de arriba. Editalo a mano solo si no usas ese campo.")]
    [SerializeField] private string scenePath;

    public string ScenePath => scenePath;

    public string SceneName => string.IsNullOrEmpty(scenePath)
        ? string.Empty
        : Path.GetFileNameWithoutExtension(scenePath);

#if UNITY_EDITOR
    public void SyncFromAsset()
    {
        if (sceneAsset != null)
        {
            scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
        }
    }
#endif
}
