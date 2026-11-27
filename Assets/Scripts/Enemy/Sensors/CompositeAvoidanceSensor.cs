using System.Collections.Generic;
using UnityEngine;

// Compone evasión dinámica (ratas) + evasión estática (paredes) bajo el contrato IAvoidanceSensor (OCP + LSP + DIP).
// Mantiene ObstacleMask apuntando solo a paredes estáticas para que HideSteering nunca intente cubrirse detrás de una rata.
public class CompositeAvoidanceSensor : MonoBehaviour, IAvoidanceSensor
{
    [SerializeField] private EnemyObstacleAvoidanceSensor staticObstacleSensor;

    private readonly List<ISteeringModifier> modifiers = new List<ISteeringModifier>();
    private bool initialized;

    public LayerMask ObstacleMask
    {
        get
        {
            EnsureInitialized();
            return staticObstacleSensor != null ? staticObstacleSensor.ObstacleMask : default;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    public Vector3 GetSteeredDirection(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 0.0001f) return Vector3.zero;

        EnsureInitialized();

        Vector3 currentDirection = desiredDirection;
        currentDirection.y = 0f;
        if (currentDirection.sqrMagnitude <= 0.0001f) return Vector3.zero;
        currentDirection.Normalize();

        // 1. Primero aplicamos modificadores dinámicos (ej. esquive de ratas)
        for (int i = 0; i < modifiers.Count; i++)
        {
            ISteeringModifier modifier = modifiers[i];
            if (modifier == null) continue;
            if (modifier is Behaviour behaviour && !behaviour.enabled) continue;

            currentDirection = modifier.ModifyDirection(currentDirection);
        }

        // 2. Por último aplicamos evasión de paredes estáticas para garantizar que esquivar una rata no empuje contra un muro
        if (staticObstacleSensor != null && staticObstacleSensor.enabled)
        {
            currentDirection = staticObstacleSensor.GetSteeredDirection(currentDirection);
        }

        return currentDirection;
    }

    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (staticObstacleSensor == null)
        {
            staticObstacleSensor = GetComponent<EnemyObstacleAvoidanceSensor>();
        }

        modifiers.Clear();
        GetComponents(modifiers);
    }
}
