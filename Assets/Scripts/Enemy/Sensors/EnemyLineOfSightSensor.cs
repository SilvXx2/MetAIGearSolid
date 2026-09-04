using UnityEngine;

public class EnemyLineOfSightSensor : MonoBehaviour, IVisionSensor
{
    [SerializeField] private Transform target;
    [SerializeField] private float visionDistance = 10f;
    [Range(0f, 360f)]
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private LayerMask obstacleMask;

    public bool CanSeeTarget => CheckLineOfSight();
    public Transform Target => target;
    public float VisionDistance => visionDistance;
    public float VisionAngle => visionAngle;

    private void Awake()
    {
        if (target == null) target = FindFirstObjectByType<PlayerMovement>()?.transform;
    }

    private bool CheckLineOfSight()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<PlayerMovement>()?.transform;
            if (target == null) return false;
        }

        Vector3 origin = transform.position + eyeOffset;
        Vector3 targetPos = target.position + eyeOffset;
        Vector3 toTarget = targetPos - origin;
        float distance = toTarget.magnitude;

        if (distance > visionDistance) return false;

        Vector3 horizontalDir = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
        Vector3 horizontalForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        if (horizontalDir != Vector3.zero && horizontalForward != Vector3.zero)
        {
            if (Vector3.Angle(horizontalForward, horizontalDir) > visionAngle / 2f) return false;
        }

        if (obstacleMask != 0)
        {
            if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, distance, obstacleMask))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target)) return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + eyeOffset;

        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
        Gizmos.DrawWireSphere(origin, visionDistance);

        Vector3 forward = transform.forward;
        Vector3 leftRayDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, leftRayDirection * visionDistance);
        Gizmos.DrawRay(origin, rightRayDirection * visionDistance);

        if (target != null)
        {
            Gizmos.color = CheckLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(origin, target.position + eyeOffset);
        }
    }
}
