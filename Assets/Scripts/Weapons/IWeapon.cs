using UnityEngine;

public interface IWeapon
{
    float AttackRange { get; }
    bool CanFire { get; }

    Vector3 GetMuzzlePosition(Transform attacker);
    bool TryAttack(Transform attacker, Transform target);
    bool TryAttack(Transform attacker, Transform target, Vector3 aimDirection);
    void SetEquippedVisual(bool equipped);
}
