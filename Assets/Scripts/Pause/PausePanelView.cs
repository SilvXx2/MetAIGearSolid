using UnityEngine;

public class PausePanelView : MonoBehaviour, IPauseView
{
    [Header("Panel")]
    [Tooltip("GameObject del panel que se muestra al pausar. Tiene que ser un hijo, no este mismo objeto.")]
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        if (panel == null)
        {
            Debug.LogError($"{name}: falta asignar 'Panel'.", this);
            return;
        }

        if (panel == gameObject)
        {
            Debug.LogError($"{name}: 'Panel' no puede ser este mismo GameObject, al ocultarlo se apagaria a si mismo. Usa un hijo.", this);
            panel = null;
        }
    }

    public void Show(bool paused)
    {
        if (panel == null) return;

        panel.SetActive(paused);
    }
}
