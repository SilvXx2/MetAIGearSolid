using UnityEngine;

public class KeyHudPresenter : MonoBehaviour
{
    [Header("Inventario a Mostrar")]
    [Tooltip("GameObject que tiene el inventario de llaves. Normalmente el Player.")]
    [SerializeField] private GameObject inventorySource;

    private IKeyInventoryObservable inventory;
    private IKeyHudView view;

    private void Awake()
    {
        view = GetComponent<IKeyHudView>();

        if (view == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IKeyHudView (por ejemplo KeyHudCanvasView).", this);
            enabled = false;
            return;
        }

        if (inventorySource == null)
        {
            Debug.LogError($"{name}: falta asignar 'Inventory Source'.", this);
            enabled = false;
            return;
        }

        inventory = inventorySource.GetComponentInChildren<IKeyInventoryObservable>(true);

        if (inventory == null)
        {
            Debug.LogError($"{name}: 'Inventory Source' no tiene ningun componente que implemente IKeyInventoryObservable (por ejemplo KeyInventory).", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        inventory.KeyAdded += HandleKeyAdded;
        view.Render(inventory.Keys);
    }

    private void OnDisable()
    {
        inventory.KeyAdded -= HandleKeyAdded;
    }

    private void HandleKeyAdded(KeyDefinition key)
    {
        view.Render(inventory.Keys);
    }
}
