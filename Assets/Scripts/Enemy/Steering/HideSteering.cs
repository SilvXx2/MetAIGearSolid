using UnityEngine;

public class HideSteering
{
    private readonly Transform agentTransform;
    private readonly LayerMask obstacleMask;
    private readonly float scanRadius;
    private readonly float coverOffset;
    private readonly float slowingDistance;
    private readonly float arrivalThreshold;

    private Vector3 lastChosenSpot;
    private bool hasHidingSpot;

    public HideSteering(
        Transform agentTransform,
        LayerMask obstacleMask,
        float scanRadius = 15f,
        float coverOffset = 1.5f,
        float slowingDistance = 2.5f,
        float arrivalThreshold = 0.6f)
    {
        this.agentTransform = agentTransform;
        this.obstacleMask = obstacleMask;
        this.scanRadius = scanRadius;
        this.coverOffset = coverOffset;
        this.slowingDistance = slowingDistance;
        this.arrivalThreshold = arrivalThreshold;
    }

    public Vector3 CalculateHideDirection(
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float agentSpeed,
        IAvoidanceSensor avoidanceSensor = null)
    {
        Vector3 agentPos = agentTransform.position;
        agentPos.y = 0f;
        targetPosition.y = 0f;

        Collider[] obstacles = Physics.OverlapSphere(agentTransform.position, scanRadius, obstacleMask, QueryTriggerInteraction.Ignore);

        Collider bestObstacle = null;
        Vector3 bestHidingSpot = Vector3.zero;
        float minDistanceToSpot = float.MaxValue;

        for (int i = 0; i < obstacles.Length; i++)
        {
            Collider col = obstacles[i];
            if (col == null || col.transform.root == agentTransform.root) continue;

            Vector3 obstacleCenter = col.bounds.center;
            obstacleCenter.y = 0f;

            float obstacleRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);

            Vector3 toObstacle = (obstacleCenter - targetPosition);
            if (toObstacle.sqrMagnitude < 0.001f) continue;

            Vector3 hideDir = toObstacle.normalized;

            Vector3 spot = obstacleCenter + hideDir * (obstacleRadius + coverOffset);
            spot.y = agentTransform.position.y;

            float distToSpot = Vector3.Distance(agentPos, spot);
            if (distToSpot < minDistanceToSpot)
            {
                minDistanceToSpot = distToSpot;
                bestHidingSpot = spot;
                bestObstacle = col;
            }
        }

        if (bestObstacle == null)
        {
            hasHidingSpot = false;
            float lookAheadTime = Mathf.Clamp(Vector3.Distance(agentPos, targetPosition) / Mathf.Max(agentSpeed, 0.1f), 0.1f, 1.5f);
            Vector3 predictedTargetPos = targetPosition + targetVelocity * lookAheadTime;
            predictedTargetPos.y = agentPos.y;

            Vector3 evadeDir = (agentPos - predictedTargetPos).normalized;
            if (avoidanceSensor != null)
            {
                evadeDir = avoidanceSensor.GetSteeredDirection(evadeDir);
            }
            return evadeDir;
        }

        hasHidingSpot = true;
        lastChosenSpot = bestHidingSpot;

        Vector3 toSpot = bestHidingSpot - agentPos;
        toSpot.y = 0f;
        float distance = toSpot.magnitude;

        if (distance <= arrivalThreshold)
        {
            return Vector3.zero;
        }

        float speedFactor = (distance > slowingDistance)
            ? 1f
            : Mathf.Clamp01(distance / slowingDistance);

        Vector3 desiredDirection = toSpot.normalized;

        Vector3 finalDirection = avoidanceSensor != null
            ? avoidanceSensor.GetSteeredDirection(desiredDirection)
            : desiredDirection;

        return finalDirection * speedFactor;
    }

    public void DrawGizmos(Vector3 currentPosition, Vector3 targetPosition)
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        Gizmos.DrawWireSphere(currentPosition, scanRadius);

        if (hasHidingSpot)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastChosenSpot, 0.4f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(targetPosition, lastChosenSpot);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(currentPosition, lastChosenSpot);
        }
    }
}
