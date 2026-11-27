using System.Collections.Generic;
using UnityEngine;

// Sensor dedicado al esquive dinámico de ratas (SRP + 0 GC con OverlapSphereNonAlloc).
public class EnemyRatAvoidanceSensor : MonoBehaviour, ISteeringModifier
{
    [Header("Detección de Ratas")]
    [SerializeField] private float detectionRadius = 3.8f;
    [SerializeField] private float personalClearance = 1.15f;
    [SerializeField] private float lateralDodgeWeight = 1.85f;
    [SerializeField] private float closeRepulsionWeight = 2.4f;
    [SerializeField] private LayerMask agentMask = ~0;
    [SerializeField] private LayerMask wallCheckMask;
    [SerializeField] private Vector3 sensorOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private int maxBufferHits = 24;

    private Collider[] hitBuffer;
    private readonly HashSet<IAvoidableAgent> detectedAgents = new HashSet<IAvoidableAgent>();
    private IVisionSensor visionSensor;
    private Vector3 lastAvoidanceForce;
    private bool detectedRatLastFrame;

    private void Awake()
    {
        hitBuffer = new Collider[Mathf.Max(8, maxBufferHits)];
        visionSensor = GetComponent<IVisionSensor>();
    }

    public Vector3 ModifyDirection(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 0.0001f)
        {
            detectedRatLastFrame = false;
            lastAvoidanceForce = Vector3.zero;
            return Vector3.zero;
        }

        if (hitBuffer == null || hitBuffer.Length == 0)
        {
            hitBuffer = new Collider[Mathf.Max(8, maxBufferHits)];
        }

        desiredDirection.y = 0f;
        Vector3 normalizedDesired = desiredDirection.normalized;
        Vector3 origin = transform.position + sensorOffset;

        int mask = agentMask.value != 0 ? agentMask.value : ~0;
        int hitCount = Physics.OverlapSphereNonAlloc(origin, detectionRadius, hitBuffer, mask, QueryTriggerInteraction.Ignore);

        detectedAgents.Clear();
        Vector3 totalDodgeForce = Vector3.zero;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = hitBuffer[i];
            if (col == null || col.transform.root == transform.root) continue;

            IAvoidableAgent avoidable = col.GetComponent<IAvoidableAgent>() ?? col.GetComponentInParent<IAvoidableAgent>();
            if (avoidable == null) continue;
            if (avoidable.AgentTransform != null && avoidable.AgentTransform.root == transform.root) continue;
            if (!detectedAgents.Add(avoidable)) continue;

            Vector3 toRat = avoidable.Position - transform.position;
            toRat.y = 0f;
            float distance = toRat.magnitude;
            if (distance <= 0.001f || distance > detectionRadius) continue;

            Vector3 dirToRat = toRat / distance;
            float forwardDot = Vector3.Dot(dirToRat, normalizedDesired);
            float effectiveClearance = personalClearance + avoidable.AvoidanceRadius;

            // 1. Repulsión suave si la rata está muy pegada
            if (distance < effectiveClearance)
            {
                float closeness = 1f - Mathf.Clamp01(distance / effectiveClearance);
                totalDodgeForce += (-dirToRat) * (closeness * closeRepulsionWeight);
            }

            // 2. Desvío tangencial lateral si la rata está en el camino frontal
            if (forwardDot > -0.15f)
            {
                Vector3 lateralToRat = toRat - normalizedDesired * (forwardDot * distance);
                if (lateralToRat.magnitude <= effectiveClearance)
                {
                    float urgency = 1f - Mathf.Clamp01(distance / detectionRadius);
                    float crossY = Vector3.Cross(normalizedDesired, dirToRat).y;
                    float preferredSign = crossY >= 0f ? -1f : 1f;

                    Vector3 lateralRight = Vector3.Cross(Vector3.up, normalizedDesired).normalized;
                    Vector3 dodgeDir = ChooseClearLateralSide(origin, lateralRight, preferredSign);
                    totalDodgeForce += dodgeDir * (urgency * lateralDodgeWeight);
                }
            }
        }

        if (totalDodgeForce.sqrMagnitude <= 0.0001f)
        {
            detectedRatLastFrame = false;
            lastAvoidanceForce = Vector3.zero;
            return normalizedDesired;
        }

        detectedRatLastFrame = true;
        lastAvoidanceForce = totalDodgeForce;

        Vector3 combined = normalizedDesired + totalDodgeForce;
        combined.y = 0f;
        return combined.sqrMagnitude > 0.0001f ? combined.normalized : normalizedDesired;
    }

    private Vector3 ChooseClearLateralSide(Vector3 origin, Vector3 lateralRight, float preferredSign)
    {
        LayerMask walls = wallCheckMask.value != 0
            ? wallCheckMask
            : (visionSensor != null ? visionSensor.ObstacleMask : default);

        Vector3 preferredDir = lateralRight * preferredSign;
        if (walls.value == 0) return preferredDir;

        float probeDistance = personalClearance + 0.8f;
        bool preferredBlocked = Physics.Raycast(origin, preferredDir, probeDistance, walls, QueryTriggerInteraction.Ignore);
        if (!preferredBlocked) return preferredDir;

        Vector3 oppositeDir = -preferredDir;
        bool oppositeBlocked = Physics.Raycast(origin, oppositeDir, probeDistance, walls, QueryTriggerInteraction.Ignore);
        return oppositeBlocked ? preferredDir : oppositeDir;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + sensorOffset;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.25f);
        Gizmos.DrawWireSphere(origin, detectionRadius);

        if (Application.isPlaying && detectedRatLastFrame)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, lastAvoidanceForce.normalized * 2.2f);
        }
    }
}
