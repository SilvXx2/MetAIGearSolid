using UnityEngine;

public class Evade
{
    private readonly Transform self;
    private readonly float maxPredictionTime;

    public Evade(Transform self, Transform targetTransform = null, float maxPredictionTime = 1.5f)
    {
        this.self = self;
        this.maxPredictionTime = Mathf.Max(0.05f, maxPredictionTime);
    }

    public Vector3 CalculateEvadeDirection(Vector3 targetPosition, Vector3 targetVelocity, float agentSpeed)
    {
        if (self == null) return Vector3.zero;

        Vector3 agentPos = self.position;
        agentPos.y = 0f;
        targetPosition.y = 0f;
        targetVelocity.y = 0f;

        float lookAheadTime = Mathf.Clamp(
            Vector3.Distance(agentPos, targetPosition) / Mathf.Max(agentSpeed, 0.1f),
            0.1f,
            maxPredictionTime);

        Vector3 predictedTargetPos = targetPosition + targetVelocity * lookAheadTime;
        Vector3 evadeDir = agentPos - predictedTargetPos;
        evadeDir.y = 0f;

        return evadeDir.sqrMagnitude > 0.0001f ? evadeDir.normalized : Vector3.zero;
    }
}

