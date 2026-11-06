using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour, ISteering
{
    public int maxBoids = 20;
    public float radius = 6f;
    public LayerMask maskBoids = ~0;

    private Collider[] _colls;
    private List<IBoid> _boids;
    private IBoid _self;
    private IFlockingBehaviour[] _behaviours;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        int capacity = Mathf.Max(4, maxBoids);
        _colls = new Collider[capacity];
        _self = GetComponent<IBoid>();
        _behaviours = GetComponents<IFlockingBehaviour>();
        _boids = new List<IBoid>(capacity);
    }

    public Vector3 GetDir()
    {
        if (_colls == null || _boids == null || _behaviours == null)
        {
            Initialize();
        }

        _boids.Clear();
        int mask = maskBoids.value != 0 ? maskBoids.value : ~0;
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _colls, mask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (_colls[i] == null) continue;
            var boid = _colls[i].GetComponent<IBoid>() ?? _colls[i].GetComponentInParent<IBoid>();
            if (boid == null || boid == _self) continue;
            if (!_boids.Contains(boid))
            {
                _boids.Add(boid);
            }
        }

        Vector3 dir = Vector3.zero;
        for (int i = 0; i < _behaviours.Length; i++)
        {
            if (_behaviours[i] == null) continue;
            dir += _behaviours[i].GetDir(_boids, _self);
        }

        dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

