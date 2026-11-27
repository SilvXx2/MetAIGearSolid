using System;
using UnityEngine;

public class EnemyAttackState : IState, IDescribedState
{
    private enum AttackPhase
    {
        Aiming,
        Cooldown
    }

    private readonly IEnemyContext context;
    private readonly Action<Vector3> onAttackAction;
    private readonly float attackCooldown;
    private readonly float aimDuration;
    private readonly float aimLockDuration;
    private readonly float telegraphMaxLength;
    private readonly IAimTelegraph aimTelegraph;
    private readonly Func<Vector3> muzzlePositionProvider;

    private AttackPhase phase;
    private float phaseTimer;
    private Vector3 lockedAimDirection;

    public EnemyAttackState(
        IEnemyContext context,
        Action<Vector3> onAttackAction = null,
        float attackCooldown = 0.85f,
        float aimDuration = 0.85f,
        float aimLockDuration = 0.22f,
        float telegraphMaxLength = 12f,
        IAimTelegraph aimTelegraph = null,
        Func<Vector3> muzzlePositionProvider = null)
    {
        this.context = context;
        this.onAttackAction = onAttackAction;
        this.attackCooldown = Mathf.Max(0.1f, attackCooldown);
        this.aimDuration = Mathf.Max(0.1f, aimDuration);
        this.aimLockDuration = Mathf.Clamp(aimLockDuration, 0f, this.aimDuration * 0.6f);
        this.telegraphMaxLength = Mathf.Max(2f, telegraphMaxLength);
        this.aimTelegraph = aimTelegraph;
        this.muzzlePositionProvider = muzzlePositionProvider;
    }

    public bool IsCommittedToShot => phase == AttackPhase.Aiming && (aimDuration - phaseTimer) <= aimLockDuration;

    public string StatusLabel
    {
        get
        {
            if (phase == AttackPhase.Aiming)
            {
                return IsCommittedToShot ? "DISPARANDO!" : "APUNTANDO...";
            }

            return "ATAQUE";
        }
    }

    public NpcAlertLevel AlertLevel => NpcAlertLevel.Hostile;

    public void Enter()
    {
        context.Mover.Move(Vector3.zero, 0f);
        BeginAimPhase();
    }

    public void Update()
    {
        context.Mover.Move(Vector3.zero, 0f);

        Transform target = context.Vision?.Target;

        if (phase == AttackPhase.Aiming)
        {
            UpdateAimPhase(target);
            return;
        }

        UpdateCooldownPhase(target);
    }

    public void Exit()
    {
        aimTelegraph?.StopAim();
        context.Mover.Move(Vector3.zero, 0f);
    }

    private void BeginAimPhase()
    {
        phase = AttackPhase.Aiming;
        phaseTimer = 0f;
        lockedAimDirection = GetDirectionToTarget(context.Vision?.Target);

        Vector3 origin = GetMuzzleOrigin();
        Vector3 endPoint = GetTelegraphEndPoint(origin, context.Vision?.Target);
        aimTelegraph?.BeginAim(origin, endPoint);
    }

    private void UpdateAimPhase(Transform target)
    {
        phaseTimer += Time.deltaTime;
        float remaining = aimDuration - phaseTimer;
        bool isLocked = remaining <= aimLockDuration;

        if (!isLocked)
        {
            lockedAimDirection = GetDirectionToTarget(target);
        }

        if (lockedAimDirection.sqrMagnitude > 0.0001f)
        {
            context.Mover.Rotate(lockedAimDirection);
        }

        Vector3 origin = GetMuzzleOrigin();
        Vector3 endPoint = GetTelegraphEndPoint(origin, isLocked ? null : target);
        float progress01 = Mathf.Clamp01(phaseTimer / aimDuration);
        aimTelegraph?.UpdateAim(origin, endPoint, progress01, isLocked);

        if (phaseTimer >= aimDuration)
        {
            aimTelegraph?.StopAim();
            onAttackAction?.Invoke(lockedAimDirection);
            phase = AttackPhase.Cooldown;
            phaseTimer = attackCooldown;
        }
    }

    private void UpdateCooldownPhase(Transform target)
    {
        Vector3 dirToTarget = GetDirectionToTarget(target);
        if (dirToTarget.sqrMagnitude > 0.0001f)
        {
            context.Mover.Rotate(dirToTarget);
        }

        phaseTimer -= Time.deltaTime;
        if (phaseTimer <= 0f)
        {
            BeginAimPhase();
        }
    }

    private Vector3 GetMuzzleOrigin()
    {
        if (muzzlePositionProvider != null)
        {
            return muzzlePositionProvider.Invoke();
        }

        return context.Transform.position + Vector3.up * 1.1f;
    }

    private Vector3 GetDirectionToTarget(Transform target)
    {
        if (target != null)
        {
            Vector3 toTarget = target.position - context.Transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                return toTarget.normalized;
            }
        }

        Vector3 forward = context.Transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }

    private Vector3 GetTelegraphEndPoint(Vector3 origin, Transform liveTarget)
    {
        float length = telegraphMaxLength;
        if (liveTarget != null)
        {
            Vector3 flatDelta = liveTarget.position - origin;
            flatDelta.y = 0f;
            length = Mathf.Clamp(flatDelta.magnitude + 1.5f, 1.5f, telegraphMaxLength);
        }

        return origin + lockedAimDirection * length;
    }
}
