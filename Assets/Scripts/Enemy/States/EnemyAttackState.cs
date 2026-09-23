using System;
using UnityEngine;

public class EnemyAttackState : IState
{
    private readonly IEnemyContext context;
    private readonly Action onAttackAction;
    private readonly float attackCooldown;
    private float cooldownTimer;

    public EnemyAttackState(IEnemyContext context, Action onAttackAction = null, float attackCooldown = 1.2f)
    {
        this.context = context;
        this.onAttackAction = onAttackAction;
        this.attackCooldown = attackCooldown;
    }

    public void Enter()
    {
        context.Mover.Move(Vector3.zero, 0f);
        cooldownTimer = 0f;
    }

    public void Update()
    {
        context.Mover.Move(Vector3.zero, 0f);

        Transform target = context.Vision?.Target;
        if (target != null)
        {
            Vector3 toTarget = target.position - context.Transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                context.Mover.Rotate(toTarget.normalized);
            }
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            onAttackAction?.Invoke();
            cooldownTimer = attackCooldown;
        }
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
