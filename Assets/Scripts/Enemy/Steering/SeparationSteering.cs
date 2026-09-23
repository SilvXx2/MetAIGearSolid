using UnityEngine;

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

            if (col.GetComponent<IMover>() == null && col.GetComponentInParent<IMover>() == null) continue;

            Vector3 neighborPos = col.transform.position;
            neighborPos.y = 0f;

            Vector3 offset = agentPos - neighborPos;
            float distance = offset.magnitude;

            if (distance > 0.001f && distance < separationRadius)
            {
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
