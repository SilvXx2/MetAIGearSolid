using System;
using UnityEngine;

/// <summary>
/// Estado de Ataque del enemigo requerido por la consigna de IA.
/// Cuando el NPC está a distancia del jugador, se detiene, lo encara y ataca.
/// Puede invocar un callback para consumir munición y notificar el fin del juego / Game Over.
/// </summary>
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
        cooldownTimer = 0f; // Atacar inmediatamente al entrar en rango
    }

    public void Update()
    {
        // En estado de ataque el enemigo se mantiene firme
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
            ExecuteAttack();
            cooldownTimer = attackCooldown;
        }
    }

    private void ExecuteAttack()
    {
        Debug.Log($"[EnemyAttackState] ¡{context.Transform.name} ejecutó un ATAQUE hacia el jugador!");
        onAttackAction?.Invoke();
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
