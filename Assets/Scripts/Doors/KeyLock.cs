using UnityEngine;

public class KeyLock : MonoBehaviour, IDoorLock
{
    [Header("Llave Requerida")]
    [SerializeField] private KeyDefinition requiredKey;

    public bool CanOpen(GameObject interactor)
    {
        if (requiredKey == null) return true;

        var inventory = interactor.GetComponentInParent<IKeyInventoryReader>();
        return inventory != null && inventory.HasKey(requiredKey);
    }
}
