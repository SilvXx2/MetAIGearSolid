using UnityEngine;

public interface IAimTelegraph
{
    void BeginAim(Vector3 origin, Vector3 targetPoint);
    void UpdateAim(Vector3 origin, Vector3 targetPoint, float progress01, bool isLocked);
    void StopAim();
}
