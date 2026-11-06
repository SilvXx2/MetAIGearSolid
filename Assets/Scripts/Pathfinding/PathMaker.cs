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

    public PathfindingAlgorithm Algorithm
    {
        get => algorithm;
        set => algorithm = value;
    }

    public LayerMask WallMask => wallMask;

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

    public bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        return !Physics.Linecast(from, to, wallMask, QueryTriggerInteraction.Ignore);
    }

    public List<Node> CreatePath(Vector3 startPos, Vector3 goalPos, Func<Node, bool> itFulfills = null)
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

        if (itFulfills == null)
        {
            itFulfills = (n) => n == closestGoal;
        }

        return CreatePath(closestStart, closestGoal, itFulfills);
    }

    public List<Node> CreatePath(Node start, Node target, Func<Node, bool> itFulfills)
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
                if (itFulfills == null) itFulfills = (n) => n == target;
                path = GenericSearchAlgorithms.Astar(
                    start,
                    itFulfills,
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
        if (Physics.Linecast(current.Position, next.Position, wallMask, QueryTriggerInteraction.Ignore)) return false;
        return true;
    }

    private List<Node> GetNodeNeighbors(Node current) => current.Neighbors;

    private float GetNodeCost(Node current, Node next) => Vector3.Distance(current.Position, next.Position) * next.Cost;

    private float GetNodeHeuristic(Node current, Node target) => Vector3.Distance(current.Position, target.Position);

    private Node Closest(Vector3 pos)
    {
        if (_grid == null || _grid.nodeGrid == null) return null;

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

            if (distSqr < minFallbackDistSqr)
            {
                closestFallback = n;
                minFallbackDistSqr = distSqr;
            }

            if (distSqr < minVisibleDistSqr && HasLineOfSight(pos, n.Position))
            {
                closestVisible = n;
                minVisibleDistSqr = distSqr;
            }
        }

        return closestVisible != null ? closestVisible : closestFallback;
    }
}

