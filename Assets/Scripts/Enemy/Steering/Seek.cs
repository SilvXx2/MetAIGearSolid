using UnityEngine;

public class Seek : ISteering
{
    private readonly Transform self;
    private Transform target;
    private Vector3 explicitTargetPosition;
    private bool useExplicitPosition;

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            useExplicitPosition = false;
        }
    }

    public Seek(Transform self, Transform target = null)
    {
        this.self = self;
        this.target = target;
    }

    public void SetTargetPosition(Vector3 worldPosition)
    {
        explicitTargetPosition = worldPosition;
        useExplicitPosition = true;
    }

    public Vector3 GetDir()
    {
        if (self == null) return Vector3.zero;

        Vector3 targetPos;
        if (useExplicitPosition)
        {
            targetPos = explicitTargetPosition;
        }
        else if (target != null)
        {
            targetPos = target.position;
        }
        else
        {
            return Vector3.zero;
        }

        Vector3 dir = targetPos - self.position;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
    }
}

