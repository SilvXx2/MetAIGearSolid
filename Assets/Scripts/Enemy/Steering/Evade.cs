using UnityEngine;

public class Evade : ISteering
{
    private readonly Transform self;
    private Transform targetTransform;
    private Rigidbody targetRb;
    private readonly float maxPredictionTime;
    private float referenceSpeed;

    private Vector3 lastTargetPos;
    private bool hasLastPos;

    public Transform TargetTransform
    {
        get => targetTransform;
        set
        {
            if (targetTransform == value) return;
            targetTransform = value;
            targetRb = value != null ? value.GetComponent<Rigidbody>() : null;
            ResetTracking();
        }
    }

    public float ReferenceSpeed
    {
        get => referenceSpeed;
        set => referenceSpeed = Mathf.Max(0.1f, value);
    }

    public Evade(Transform self, Transform targetTransform = null, float maxPredictionTime = 1.5f, float referenceSpeed = 6f)
    {
        this.self = self;
        this.targetTransform = targetTransform;
        this.targetRb = targetTransform != null ? targetTransform.GetComponent<Rigidbody>() : null;
        this.maxPredictionTime = Mathf.Max(0.05f, maxPredictionTime);
        this.referenceSpeed = Mathf.Max(0.1f, referenceSpeed);
        ResetTracking();
    }

    public void ResetTracking()
    {
        if (targetTransform != null)
        {
            lastTargetPos = targetTransform.position;
            hasLastPos = true;
        }
        else
        {
            hasLastPos = false;
        }
    }

    public Vector3 GetDir()
    {
        if (self == null || targetTransform == null) return Vector3.zero;

        Vector3 currentTargetPos = targetTransform.position;
        Vector3 targetVelocity = Vector3.zero;

        if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            targetVelocity = targetRb.linearVelocity;
        }
        else if (hasLastPos && Time.deltaTime > 0f)
        {
            targetVelocity = (currentTargetPos - lastTargetPos) / Time.deltaTime;
        }

        lastTargetPos = currentTargetPos;
        hasLastPos = true;
        targetVelocity.y = 0f;

        return CalculateEvadeDirection(currentTargetPos, targetVelocity, referenceSpeed);
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

