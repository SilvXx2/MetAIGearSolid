using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Eventos")]
    [SerializeField] private UnityEvent onAccessDenied;

    private IDoorOpener opener;
    private IDoorLock doorLock;

    private void Awake()
    {
        opener = GetComponent<IDoorOpener>();
        doorLock = GetComponent<IDoorLock>();

        if (opener == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IDoorOpener (por ejemplo TimedDoorOpener).", this);
        }
    }

    public void Interact(GameObject interactor)
    {
        if (opener == null || opener.IsOpen) return;

        if (doorLock != null && !doorLock.CanOpen(interactor))
        {
            onAccessDenied?.Invoke();
            return;
        }

        opener.Open();
    }
}
