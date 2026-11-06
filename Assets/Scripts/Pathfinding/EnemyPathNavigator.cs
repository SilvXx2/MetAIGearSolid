using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class EnemyPathNavigator : MonoBehaviour, IPathNavigator
{
    [Header("Configuración de Seguimiento de Nodos")]
    [SerializeField] private float nodeReachThreshold = 0.6f;
    [SerializeField] private float repathInterval = 0.4f;
    [SerializeField] private float targetMovedRepathDistance = 1.2f;
    [SerializeField] private PathMaker customPathMaker;

    private readonly List<Node> path = new List<Node>();
    private IVisionSensor visionSensor;
    private IAvoidanceSensor avoidanceSensor;
    private float repathTimer;
    private Vector3 lastGoalPosition;
    private bool hasLastGoal;

    public bool IsFollowingPath => path.Count > 0;
    public IReadOnlyList<Node> CurrentPath => path;

    private IPathMaker ActivePathMaker => customPathMaker != null ? customPathMaker : PathMaker.instance;

    private void Awake()
    {
        visionSensor = GetComponent<IVisionSensor>();
        avoidanceSensor = GetComponent<IAvoidanceSensor>();
    }

    public void Initialize(IVisionSensor vision, IAvoidanceSensor avoidance, IPathMaker pathMakerOverride = null)
    {
        if (vision != null) visionSensor = vision;
        if (avoidance != null) avoidanceSensor = avoidance;
        if (pathMakerOverride is PathMaker pm) customPathMaker = pm;
    }

    public bool HasLineOfSightTo(Vector3 targetPosition)
    {
        if (visionSensor != null)
        {
            return visionSensor.CanSeePosition(targetPosition);
        }

        if (ActivePathMaker != null)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 dest = targetPosition + Vector3.up * 0.5f;
            return ActivePathMaker.HasLineOfSight(origin, dest);
        }

        return true;
    }

    public Vector3 GetMoveDirection(Vector3 targetPosition, ISteering directSteering = null)
    {
        // Regla TP2: Si tiene línea de visión con su objetivo, usa Steering Behaviors.
        if (HasLineOfSightTo(targetPosition))
        {
            ClearPath();
            Vector3 desiredDir = directSteering != null
                ? directSteering.GetDir()
                : GetFlatDirection(transform.position, targetPosition);

            return ApplyAvoidance(desiredDir);
        }

        // Regla TP2: Si NO tiene línea de visión con su objetivo, pasa a utilizar Pathfinding.
        IPathMaker pathMaker = ActivePathMaker;
        if (pathMaker == null)
        {
            Vector3 fallbackDir = directSteering != null
                ? directSteering.GetDir()
                : GetFlatDirection(transform.position, targetPosition);
            return ApplyAvoidance(fallbackDir);
        }

        repathTimer -= Time.deltaTime;
        float goalMovedSqr = hasLastGoal ? (targetPosition - lastGoalPosition).sqrMagnitude : float.MaxValue;

        if (path.Count == 0 || repathTimer <= 0f || goalMovedSqr >= targetMovedRepathDistance * targetMovedRepathDistance)
        {
            RebuildPath(pathMaker, targetPosition);
        }

        AdvanceCompletedNodes();

        if (path.Count > 0 && path[0] != null)
        {
            Vector3 nextNodePosition = path[0].Position;
            Vector3 dirToNode = GetFlatDirection(transform.position, nextNodePosition);
            return ApplyAvoidance(dirToNode);
        }

        Vector3 defaultDir = directSteering != null
            ? directSteering.GetDir()
            : GetFlatDirection(transform.position, targetPosition);
        return ApplyAvoidance(defaultDir);
    }

    public void ClearPath()
    {
        path.Clear();
        hasLastGoal = false;
    }

    private void RebuildPath(IPathMaker pathMaker, Vector3 targetPosition)
    {
        repathTimer = repathInterval;
        lastGoalPosition = targetPosition;
        hasLastGoal = true;

        List<Node> calculated = pathMaker.CreatePath(transform.position, targetPosition);
        path.Clear();
        if (calculated != null && calculated.Count > 0)
        {
            path.AddRange(calculated);
            AdvanceCompletedNodes();
        }
    }

    private void AdvanceCompletedNodes()
    {
        float sqrThreshold = nodeReachThreshold * nodeReachThreshold;

        while (path.Count > 0)
        {
            if (path[0] == null)
            {
                path.RemoveAt(0);
                continue;
            }

            Vector3 toNode = path[0].Position - transform.position;
            toNode.y = 0f;

            if (toNode.sqrMagnitude <= sqrThreshold)
            {
                path.RemoveAt(0);
                continue;
            }

            // Si el siguiente nodo en la lista ya es visible directamente y está más cerca del objetivo, avanzamos (Theta* smoothing)
            if (path.Count > 1 && path[1] != null && HasLineOfSightTo(path[1].Position))
            {
                Vector3 toCurrent = path[0].Position - path[1].Position;
                toCurrent.y = 0f;
                Vector3 agentToNext = path[1].Position - transform.position;
                agentToNext.y = 0f;

                if (agentToNext.sqrMagnitude < toCurrent.sqrMagnitude)
                {
                    path.RemoveAt(0);
                    continue;
                }
            }

            break;
        }
    }

    private Vector3 ApplyAvoidance(Vector3 desiredDirection)
    {
        if (desiredDirection == Vector3.zero) return Vector3.zero;
        return avoidanceSensor != null
            ? avoidanceSensor.GetSteeredDirection(desiredDirection)
            : desiredDirection.normalized;
    }

    private static Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 diff = to - from;
        diff.y = 0f;
        return diff.sqrMagnitude > 0.0001f ? diff.normalized : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.magenta;
        Vector3 prev = transform.position;
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == null) continue;
            Vector3 nodePos = path[i].Position;
            Gizmos.DrawLine(prev, nodePos);
            Gizmos.DrawSphere(nodePos, 0.22f);
            prev = nodePos;
        }
    }
}

