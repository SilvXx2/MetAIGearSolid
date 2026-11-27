using UnityEngine;

// Marca a cualquier rata (seguidores de manada o líder) como agente que los guardias deben esquivar.
public class RatAvoidanceMarker : MonoBehaviour, IAvoidableAgent
{
    [SerializeField] private float avoidanceRadius = 1.25f;

    public Vector3 Position => transform.position;
    public float AvoidanceRadius => Mathf.Max(0.3f, avoidanceRadius);
    public Transform AgentTransform => transform;
}
