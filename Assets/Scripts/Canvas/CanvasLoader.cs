using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CanvasLoader : MonoBehaviour
{
    [Header("Referencias Canvas")]
    [Tooltip("Canvas que se activa al apretar el boton.")]
    [SerializeField] private GameObject targetCanvas;

    [Tooltip("Canvas que se desactiva al cambiar. Opcional si 'Deactivate Siblings' ya lo cubre.")]
    [SerializeField] private GameObject currentRootCanvas;

    [Header("Opciones")]
    [Tooltip("Deja activo el canvas actual en vez de apagarlo.")]
    [SerializeField] private bool keepCurrentActive;

    [Tooltip("Apaga todos los canvas hermanos del target dentro del mismo padre.")]
    [SerializeField] private bool deactivateSiblings = true;

    private void Awake()
    {
        if (targetCanvas == null)
        {
            Debug.LogError($"{name}: falta asignar 'Target Canvas'.", this);
            enabled = false;
            return;
        }

        GetComponent<Button>().onClick.AddListener(SwitchCanvas);
    }

    public void SwitchCanvas()
    {
        if (deactivateSiblings)
        {
            DeactivateSiblings();
        }

        if (!keepCurrentActive && currentRootCanvas != null && currentRootCanvas != targetCanvas)
        {
            currentRootCanvas.SetActive(false);
        }

        targetCanvas.SetActive(true);
    }

    private void DeactivateSiblings()
    {
        Transform parent = targetCanvas.transform.parent;
        if (parent == null) return;

        foreach (Transform sibling in parent)
        {
            if (sibling.gameObject != targetCanvas)
            {
                sibling.gameObject.SetActive(false);
            }
        }
    }
}
