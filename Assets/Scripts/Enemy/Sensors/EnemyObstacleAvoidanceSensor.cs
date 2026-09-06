using UnityEngine;

public class EnemyObstacleAvoidanceSensor : MonoBehaviour, IAvoidanceSensor
{
    [SerializeField] private float detectionRadius = 3.5f;
    [SerializeField] private float clearanceRadius = 1.4f;
    [SerializeField] private float avoidanceWeight = 2.5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector3 sensorOffset = new Vector3(0f, 0.5f, 0f);

    private int currentObstacleId = 0;
    private float currentAvoidanceSign = 1f;

    private Vector3 lastDesiredDir;
    private Vector3 lastSteeredDir;
    private Vector3 lastObstaclePos;
    private bool hasDetectedObstacle;

    public Vector3 GetSteeredDirection(Vector3 desiredDirection)
    {
        if (desiredDirection == Vector3.zero) return Vector3.zero;

        Vector3 origin = transform.position + sensorOffset;
        desiredDirection.y = 0f;
        Vector3 normalizedDesired = desiredDirection.normalized;

        Collider[] hits = Physics.OverlapSphere(origin, detectionRadius, obstacleMask, QueryTriggerInteraction.Ignore);

        Collider closestCollider = null;
        Vector3 closestThreatPoint = Vector3.zero;
        float minForwardDist = float.MaxValue;
        float clearanceSqr = clearanceRadius * clearanceRadius;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col == null || col.transform.root == transform.root) continue;

            Vector3 surfacePoint = col.ClosestPoint(origin);
            Vector3 toSurface = surfacePoint - origin;
            toSurface.y = 0f;

            float forwardDist = Vector3.Dot(toSurface, normalizedDesired);
            if (forwardDist <= 0.05f || forwardDist > detectionRadius) continue;

            Vector3 lateralToSurface = toSurface - normalizedDesired * forwardDist;
            if (lateralToSurface.sqrMagnitude > clearanceSqr) continue;

            if (forwardDist < minForwardDist)
            {
                minForwardDist = forwardDist;
                closestCollider = col;
                closestThreatPoint = surfacePoint;
            }
        }

        if (closestCollider == null)
        {
            currentObstacleId = 0;
            hasDetectedObstacle = false;
            lastDesiredDir = normalizedDesired;
            lastSteeredDir = normalizedDesired;
            return normalizedDesired;
        }

        hasDetectedObstacle = true;
        lastObstaclePos = closestThreatPoint;

        int obstacleId = closestCollider.GetInstanceID();
        if (currentObstacleId != obstacleId)
        {
            currentObstacleId = obstacleId;
            Vector3 toCenter = closestCollider.bounds.center - origin;
            toCenter.y = 0f;
            currentAvoidanceSign = Vector3.Cross(normalizedDesired, toCenter).y >= 0f ? -1f : 1f;
        }

        Vector3 lateralRight = Vector3.Cross(Vector3.up, normalizedDesired).normalized;
        Vector3 lateralAvoidDir = lateralRight * currentAvoidanceSign;

        float urgency = 1f - Mathf.Clamp01(minForwardDist / detectionRadius);
        float forwardWeight = Mathf.Lerp(1f, 0.15f, urgency);
        float lateralWeight = avoidanceWeight * Mathf.Lerp(0.8f, 1.6f, urgency);

        Vector3 finalDirection = (normalizedDesired * forwardWeight + lateralAvoidDir * lateralWeight).normalized;

        lastDesiredDir = normalizedDesired;
        lastSteeredDir = finalDirection;

        return finalDirection;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + sensorOffset;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(origin, detectionRadius);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawLine(origin + right * clearanceRadius, origin + right * clearanceRadius + forward * detectionRadius);
        Gizmos.DrawLine(origin - right * clearanceRadius, origin - right * clearanceRadius + forward * detectionRadius);

        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin, lastDesiredDir * 2f);

        if (hasDetectedObstacle)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastObstaclePos, 0.2f);
            Gizmos.DrawLine(origin, lastObstaclePos);

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(origin, lastSteeredDir * 2.5f);
        }
    }
}
