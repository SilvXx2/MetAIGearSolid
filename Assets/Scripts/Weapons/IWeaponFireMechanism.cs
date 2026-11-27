using UnityEngine;

public interface IWeaponFireMechanism
{
    Vector3 GetMuzzlePosition(Transform attacker);
    bool ExecuteFire(Transform attacker, Transform target, Vector3 aimDirection, float damage, LayerMask obstacleMask);
}
