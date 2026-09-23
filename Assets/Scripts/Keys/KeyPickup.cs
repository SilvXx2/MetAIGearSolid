using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class KeyPickup : MonoBehaviour
{
    [Header("Llave que Entrega")]
    [SerializeField] private KeyDefinition key;

    [Header("Eventos")]
    [SerializeField] private UnityEvent onCollected;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;

        var body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (key == null)
        {
            Debug.LogWarning($"{name}: no tiene asignada una KeyDefinition.", this);
            return;
        }

        var inventory = other.GetComponentInParent<IKeyInventoryWriter>();
        if (inventory == null) return;

        if (!inventory.AddKey(key)) return;

        onCollected?.Invoke();
        Destroy(gameObject);
    }
}
