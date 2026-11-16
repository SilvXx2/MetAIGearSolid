using UnityEngine;

public class Seek : ISteering
{
    private readonly Transform self;

    public Transform Target { get; set; }

    public Seek(Transform self, Transform target = null)
    {
        this.self = self;
        Target = target;
    }

    public Vector3 GetDir()
    {
        if (self == null || Target == null) return Vector3.zero;

        Vector3 dir = Target.position - self.position;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;
    }
}

