using UnityEngine;

public class Arrive : ISteering
{
    private readonly Transform self;
    private readonly float slowingDistance;
    private readonly float arrivalThreshold;
    private Vector3 targetPosition;

    public Vector3 TargetPosition
    {
        get => targetPosition;
        set => targetPosition = value;
    }

    public Arrive(Transform self, float slowingDistance = 2.5f, float arrivalThreshold = 0.6f)
    {
        this.self = self;
        this.slowingDistance = Mathf.Max(0.1f, slowingDistance);
        this.arrivalThreshold = Mathf.Max(0.05f, arrivalThreshold);
    }

    public Vector3 GetDir()
    {
        return CalculateArriveVelocityFactor(targetPosition, null);
    }

    public Vector3 CalculateArriveVelocityFactor(Vector3 destination, IAvoidanceSensor avoidanceSensor = null)
    {
        if (self == null) return Vector3.zero;

        Vector3 agentPos = self.position;
        agentPos.y = 0f;
        destination.y = 0f;

        Vector3 toSpot = destination - agentPos;
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
}

