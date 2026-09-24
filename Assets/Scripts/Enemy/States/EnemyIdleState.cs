using UnityEngine;

public class EnemyIdleState : IState
{
    public struct IdleBehaviorOption
    {
        public float Duration;
        public bool PerformScanRotation;

        public IdleBehaviorOption(float duration, bool performScanRotation)
        {
            Duration = duration;
            PerformScanRotation = performScanRotation;
        }
    }

    private readonly IEnemyContext context;
    private readonly RouletteWheelSelection<IdleBehaviorOption> roulette;
    private float currentTimer;
    private IdleBehaviorOption currentOption;
    private Quaternion originalRotation;
    private float scanAngleTimer;

    public EnemyIdleState(IEnemyContext context)
    {
        this.context = context;

        roulette = new RouletteWheelSelection<IdleBehaviorOption>();

        roulette.AddItem(
            new IdleBehaviorOption(duration: 2.0f, performScanRotation: false),
            weight: 0.50f,
            description: "Inspección Corta (2.0s)");

        roulette.AddItem(
            new IdleBehaviorOption(duration: 4.0f, performScanRotation: true),
            weight: 0.35f,
            description: "Vigilancia Activa con Escaneo (4.0s)");

        roulette.AddItem(
            new IdleBehaviorOption(duration: 6.0f, performScanRotation: false),
            weight: 0.15f,
            description: "Descanso Táctico Extendido (6.0s)");
    }

    public void Enter()
    {
        context.Mover.Move(Vector3.zero, 0f);

        originalRotation = context.Transform.rotation;
        scanAngleTimer = 0f;

        var chosenItem = roulette.SelectItem();
        currentOption = chosenItem.Value;
        currentTimer = currentOption.Duration;

        Debug.Log($"[{context.Transform.name}] FSM Idle: Seleccionado por Ruleta -> '{chosenItem.Description}' (Peso: {chosenItem.Weight:P0}).");
    }

    public void Update()
    {
        if (context.Vision != null && context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.AlertState);
            return;
        }

        context.Mover.Move(Vector3.zero, 0f);

        if (currentOption.PerformScanRotation)
        {
            scanAngleTimer += Time.deltaTime * 2f;
            float yawOffset = Mathf.Sin(scanAngleTimer) * 35f;
            context.Transform.rotation = originalRotation * Quaternion.Euler(0f, yawOffset, 0f);
        }

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            context.Transform.rotation = originalRotation;
            context.StateMachine.ChangeState(context.PatrolState);
        }
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
