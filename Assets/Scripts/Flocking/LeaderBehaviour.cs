using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBehaviour : MonoBehaviour, IFlockingBehaviour
{
    public Transform target;
    public float timePrediction = 0.6f;
    public float multiplier = 2.5f;
    public float slowingDistance = 2.5f;
    public float followBehindOffset = 1.4f;

    private Seek _seek;
    private Pursuit _pursuit;
    private Arrive _arrive;
    private bool _isSeek;

    private void Awake()
    {
        _seek = new Seek(transform, target);
        _pursuit = new Pursuit(transform, (Rigidbody)null, timePrediction);
        _arrive = new Arrive(transform, slowingDistance, 0.5f);
        SetTarget(target);
    }

    public Vector3 GetDir(List<IBoid> boids, IBoid self)
    {
        if (target == null) return Vector3.zero;
        if (_arrive == null)
        {
            _seek = new Seek(transform, target);
            _pursuit = new Pursuit(transform, (Rigidbody)null, timePrediction);
            _arrive = new Arrive(transform, slowingDistance, 0.5f);
            SetTarget(target);
        }

        // Punto de seguimiento ligeramente detrás del líder, con fallback a la posición exacta del líder
        Vector3 followPoint = target.position - target.forward * followBehindOffset;
        Vector3 toFollow = followPoint - transform.position;
        toFollow.y = 0f;

        Vector3 toLeaderExact = target.position - transform.position;
        toLeaderExact.y = 0f;

        // Si ya está dentro del radio de llegada del líder, orienta suavemente hacia el punto de formación
        if (toLeaderExact.sqrMagnitude <= slowingDistance * slowingDistance)
        {
            return toFollow.sqrMagnitude > 0.04f ? toFollow.normalized * multiplier : Vector3.zero;
        }

        if (_isSeek) return _seek.GetDir() * multiplier;
        return _pursuit.GetDir() * multiplier;
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;
        target = newTarget;
        var rb = newTarget.GetComponent<Rigidbody>();
        if (rb)
        {
            _pursuit.Target = rb;
            _isSeek = false;
        }
        else
        {
            _seek.Target = newTarget;
            _isSeek = true;
        }
    }
}

