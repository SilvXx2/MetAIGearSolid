using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInteractInput : MonoBehaviour, IInteractInput
{
    [Header("Tecla para Interactuar")]
    [SerializeField] private Key interactKey = Key.E;

    public bool IsInteractPressed()
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[interactKey].wasPressedThisFrame;
    }
}
