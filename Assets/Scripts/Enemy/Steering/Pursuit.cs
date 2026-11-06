using UnityEngine;

public class Pursuit : ISteering
{
    private readonly Transform self;
    private Rigidbody targetRb;
    private Transform targetTransform;
    private readonly float maxPredictionTime;
    private float referenceSpeed;

    private Vector3 lastTargetPos;
    private bool hasLastPos;

    // Propiedad usada por LeaderBehaviour de la Clase 12
    public Rigidbody Target
    {
        get => targetRb;
        set
        {
            targetRb = value;
            targetTransform = value != null ? value.transform : null;
            ResetTracking();
        }
    }

    public Transform TargetTransform
    {
        get => targetTransform != null ? targetTransform : (targetRb != null ? targetRb.transform : null);
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

    // Constructor compatible con LeaderBehaviour de la Clase 12
    public Pursuit(Transform self, Rigidbody target, float timePrediction = 1.5f, float referenceSpeed = 6f)
    {
        this.self = self;
        this.targetRb = target;
        this.targetTransform = target != null ? target.transform : null;
        this.maxPredictionTime = Mathf.Max(0.05f, timePrediction);
        this.referenceSpeed = Mathf.Max(0.1f, referenceSpeed);
        ResetTracking();
    }

    public Pursuit(Transform self, Transform targetTransform, float timePrediction = 1.5f, float referenceSpeed = 6f)
    {
        this.self = self;
        this.targetTransform = targetTransform;
        this.targetRb = targetTransform != null ? targetTransform.GetComponent<Rigidbody>() : null;
        this.maxPredictionTime = Mathf.Max(0.05f, timePrediction);
        this.referenceSpeed = Mathf.Max(0.1f, referenceSpeed);
        ResetTracking();
    }

    public void ResetTracking()
    {
        Transform t = TargetTransform;
        if (t != null)
        {
            lastTargetPos = t.position;
            hasLastPos = true;
        }
        else
        {
            hasLastPos = false;
        }
    }

    public Vector3 GetDir()
    {
        Transform t = TargetTransform;
        if (self == null || t == null) return Vector3.zero;

        Vector3 currentTargetPos = t.position;
        Vector3 targetVelocity = CalculateTargetVelocity(currentTargetPos);

        Vector3 toTarget = currentTargetPos - self.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f) return Vector3.zero;

        float predictionTime = Mathf.Clamp(distance / Mathf.Max(referenceSpeed, 0.1f), 0.1f, maxPredictionTime);
        Vector3 futurePosition = currentTargetPos + targetVelocity * predictionTime;
        futurePosition.y = self.position.y;

        Vector3 desiredDirection = futurePosition - self.position;
        desiredDirection.y = 0f;

        return desiredDirection.sqrMagnitude > 0.0001f ? desiredDirection.normalized : toTarget.normalized;
    }

    private Vector3 CalculateTargetVelocity(Vector3 currentTargetPos)
    {
        Vector3 velocity = Vector3.zero;

        if (targetRb != null && targetRb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            velocity = targetRb.linearVelocity;
        }
        else if (hasLastPos && Time.deltaTime > 0f)
        {
            velocity = (currentTargetPos - lastTargetPos) / Time.deltaTime;
        }

        lastTargetPos = currentTargetPos;
        hasLastPos = true;
        velocity.y = 0f;
        return velocity;
    }
}

