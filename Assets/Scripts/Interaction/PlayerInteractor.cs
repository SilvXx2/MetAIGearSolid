using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Deteccion")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableMask = ~0;

    private IInteractInput input;

    private void Awake()
    {
        if (interactionOrigin == null) interactionOrigin = transform;

        input = GetComponent<IInteractInput>();

        if (input == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IInteractInput (por ejemplo KeyboardInteractInput).", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!input.IsInteractPressed()) return;

        IInteractable interactable = FindClosestInteractable();
        interactable?.Interact(gameObject);
    }

    private IInteractable FindClosestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            interactionOrigin.position,
            interactionRadius,
            interactableMask,
            QueryTriggerInteraction.Collide);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            var candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null) continue;

            float distance = (hit.transform.position - interactionOrigin.position).sqrMagnitude;
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            closest = candidate;
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = interactionOrigin != null ? interactionOrigin : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, interactionRadius);
    }
}
