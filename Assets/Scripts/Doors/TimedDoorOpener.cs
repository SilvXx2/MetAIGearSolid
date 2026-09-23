using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimedDoorOpener : MonoBehaviour, IDoorOpener
{
    [Header("Cuerpo de la Puerta")]
    [Tooltip("GameObject que se desactiva al abrir. Tiene que ser un hijo, no este mismo objeto.")]
    [SerializeField] private GameObject doorBody;

    [Header("Tiempo Abierta")]
    [SerializeField] private float openDuration = 3f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;

    public bool IsOpen => doorBody != null && !doorBody.activeSelf;

    private void Reset()
    {
        if (transform.childCount > 0)
        {
            doorBody = transform.GetChild(0).gameObject;
        }
    }

    private void Awake()
    {
        if (doorBody == null)
        {
            Debug.LogError($"{name}: falta asignar 'Door Body'.", this);
            enabled = false;
            return;
        }

        if (doorBody == gameObject)
        {
            Debug.LogError($"{name}: 'Door Body' no puede ser este mismo GameObject. Usa un hijo con la malla y el collider.", this);
            enabled = false;
        }
    }

    public void Open()
    {
        if (!enabled || IsOpen) return;

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        doorBody.SetActive(false);
        onOpened?.Invoke();

        yield return new WaitForSeconds(openDuration);

        doorBody.SetActive(true);
        onClosed?.Invoke();
    }
}
