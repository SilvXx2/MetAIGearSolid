using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PathMaker : MonoBehaviour, IPathMaker
{
    public static PathMaker instance;

    [SerializeField] private LayerMask wallMask;
    [SerializeField] private Pathfinding.Grid _grid;
    [SerializeField] private PathfindingAlgorithm algorithm = PathfindingAlgorithm.ThetaStar;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (_grid == null)
        {
            _grid = FindFirstObjectByType<Pathfinding.Grid>();
        }
    }

    private const float LoSClearanceRadius = 0.25f;

    public bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 origin = from.y < 0.8f ? from + Vector3.up * 0.5f : from;
        Vector3 dest = to.y < 0.8f ? to + Vector3.up * 0.5f : to;

        if (Physics.Linecast(origin, dest, wallMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        Vector3 toDest = dest - origin;
        float dist = toDest.magnitude;
        if (dist > 0.001f)
        {
            if (Physics.SphereCast(origin, LoSClearanceRadius, toDest / dist, out _, dist, wallMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }

        return true;
    }

    public List<Node> CreatePath(Vector3 startPos, Vector3 goalPos)
    {
        if (_grid == null || _grid.nodeGrid == null || _grid.nodeGrid.Length == 0)
        {
            _grid = FindFirstObjectByType<Pathfinding.Grid>();
            if (_grid == null || _grid.nodeGrid == null || _grid.nodeGrid.Length == 0)
            {
                return new List<Node>();
            }
        }

        Node closestStart = Closest(startPos);
        Node closestGoal = Closest(goalPos);
        if (closestStart == null || closestGoal == null)
        {
            return new List<Node>();
        }

        return CreatePath(closestStart, closestGoal);
    }

    private List<Node> CreatePath(Node start, Node target)
    {
        if (start == null || target == null) return new List<Node>();

        List<Node> path;
        switch (algorithm)
        {
            case PathfindingAlgorithm.AStarPostSmoothed:
                var rawPath = NodeAlgorithms.Astar(start, target, wallMask);
                path = NodeAlgorithms.PostSmoothPath(rawPath, wallMask);
                break;

            case PathfindingAlgorithm.ThetaStar:
                path = NodeAlgorithms.ThetaStar(start, target, wallMask);
                break;

            case PathfindingAlgorithm.GenericAStar:
            default:
                path = GenericSearchAlgorithms.Astar(
                    start,
                    (n) => n == target,
                    GetNodeNeighbors,
                    GetNodeCost,
                    (n) => GetNodeHeuristic(n, target),
                    IsRecheable);
                break;
        }

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] != null)
            {
                path[i].Color = Color.Lerp(Color.red, Color.yellow, (float)i / Mathf.Max(1, path.Count));
            }
        }

        return path;
    }

    private bool IsRecheable(Node current, Node next)
    {
        if (current == null || next == null || next.Blocked) return false;
        Vector3 from = current.Position + Vector3.up * 0.5f;
        Vector3 to = next.Position + Vector3.up * 0.5f;
        if (Physics.Linecast(from, to, wallMask, QueryTriggerInteraction.Ignore)) return false;
        return true;
    }

    private List<Node> GetNodeNeighbors(Node current) => current.Neighbors;

    private float GetNodeCost(Node current, Node next) => Vector3.Distance(current.Position, next.Position) * next.Cost;

    private float GetNodeHeuristic(Node current, Node target) => Vector3.Distance(current.Position, target.Position);

    private Node Closest(Vector3 pos)
    {
        if (_grid == null || _grid.nodeGrid == null) return null;

        float nodeStep = Mathf.Max(1f, _grid.distance);
        float maxSnapDistance = nodeStep * 4f;
        float maxSnapDistSqr = maxSnapDistance * maxSnapDistance;

        Node closestVisible = null;
        float minVisibleDistSqr = float.MaxValue;

        Node closestFallback = null;
        float minFallbackDistSqr = float.MaxValue;

        foreach (var n in _grid.nodeGrid)
        {
            if (n == null || n.Blocked) continue;

            Vector3 diff = n.transform.position - pos;
            diff.y = 0f;
            float distSqr = diff.sqrMagnitude;
            if (distSqr > maxSnapDistSqr) continue;

            if (distSqr < minFallbackDistSqr)
            {
                closestFallback = n;
                minFallbackDistSqr = distSqr;
            }

            Vector3 fromOrigin = new Vector3(pos.x, n.Position.y + 0.5f, pos.z);
            Vector3 nodePoint = n.Position + Vector3.up * 0.5f;
            if (distSqr < minVisibleDistSqr && !Physics.Linecast(fromOrigin, nodePoint, wallMask, QueryTriggerInteraction.Ignore))
            {
                closestVisible = n;
                minVisibleDistSqr = distSqr;
            }
        }

        return closestVisible != null ? closestVisible : closestFallback;
    }
}

