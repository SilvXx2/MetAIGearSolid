using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderEndTrigger : MonoBehaviour
{
    [Header("Final que Dispara")]
    [SerializeField] private GameResult result = GameResult.Victory;

    [Header("Quien lo Activa")]
    [Tooltip("Tag que tiene que tener lo que entra. Tiene que ser un tag que exista en el proyecto.")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Quien Recibe el Final")]
    [Tooltip("GameObject que tiene el GameEndController. Vacio = este mismo GameObject.")]
    [SerializeField] private GameObject endHandlerSource;

    private IGameEndHandler handler;

    private void Reset()
    {
        var triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (endHandlerSource == null) endHandlerSource = gameObject;

        handler = endHandlerSource.GetComponentInChildren<IGameEndHandler>(true);

        if (handler == null)
        {
            Debug.LogError($"{name}: '{endHandlerSource.name}' no tiene ningun componente que implemente IGameEndHandler (por ejemplo GameEndController).", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        Transform entered = body != null ? body.transform : other.transform;

        if (!entered.CompareTag(requiredTag)) return;

        handler.Report(result);
    }
}
