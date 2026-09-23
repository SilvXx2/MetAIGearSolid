using UnityEngine;

/// <summary>
/// Implementación de Steering Behaviour de Grupo: "Separation" (Craig Reynolds, GDC 1999 - Red3D).
/// 
/// JUSTIFICACIÓN TEÓRICA:
/// ---------------------
/// "Separation steering behavior gives a character the ability to maintain a certain
/// separation distance from others nearby. This can be used to prevent characters from
/// crowding together... For each nearby character, a repulsive force is computed by
/// subtracting the positions of our character and the nearby character, normalizing,
/// and then applying a 1/r weighting." (https://www.red3d.com/cwr/steer/gdc99/)
/// 
/// Evita que múltiples unidades (como los ratones o los guardianes) se superpongan entre sí
/// al huir o patrullar por pasillos estrechos.
/// </summary>
public class SeparationSteering
{
    private readonly Transform agentTransform;
    private readonly float separationRadius;
    private readonly LayerMask enemyLayerMask;

    public SeparationSteering(Transform agentTransform, float separationRadius = 1.5f, LayerMask enemyLayerMask = default)
    {
        this.agentTransform = agentTransform;
        this.separationRadius = separationRadius;
        this.enemyLayerMask = enemyLayerMask;
    }

    /// <summary>
    /// Calcula la fuerza vectorial repulsiva con ponderación 1/r respecto a otros enemigos cercanos.
    /// </summary>
    public Vector3 CalculateSeparationForce()
    {
        Vector3 agentPos = agentTransform.position;
        agentPos.y = 0f;

        Collider[] neighbors;
        if (enemyLayerMask.value != 0)
        {
            neighbors = Physics.OverlapSphere(agentTransform.position, separationRadius, enemyLayerMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            neighbors = Physics.OverlapSphere(agentTransform.position, separationRadius, ~0, QueryTriggerInteraction.Ignore);
        }

        Vector3 separationVector = Vector3.zero;
        int count = 0;

        for (int i = 0; i < neighbors.Length; i++)
        {
            Collider col = neighbors[i];
            if (col == null || col.transform.root == agentTransform.root) continue;

            // Solo interactuar con otros agentes con Mover / EnemyController
            if (col.GetComponent<IMover>() == null && col.GetComponentInParent<IMover>() == null) continue;

            Vector3 neighborPos = col.transform.position;
            neighborPos.y = 0f;

            Vector3 offset = agentPos - neighborPos;
            float distance = offset.magnitude;

            if (distance > 0.001f && distance < separationRadius)
            {
                // Ponderación proporcional inversa 1/r de Reynolds
                float strength = (separationRadius - distance) / separationRadius;
                separationVector += offset.normalized * strength;
                count++;
            }
        }

        if (count > 0)
        {
            separationVector /= count;
        }

        return separationVector;
    }
}
