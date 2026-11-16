using UnityEngine;

public class EnemyIdleState : IState
{
    public enum IdleActionType
    {
        NarrowScan,
        AlertWideScan,
        GuardStep
    }

    public struct IdleBehaviorOption
    {
        public float Duration;
        public IdleActionType ActionType;
        public float ScanAngleAmplitude;
        public float ScanSpeed;

        public IdleBehaviorOption(
            float duration,
            IdleActionType actionType = IdleActionType.NarrowScan,
            float scanAngleAmplitude = 35f,
            float scanSpeed = 2f)
        {
            Duration = duration;
            ActionType = actionType;
            ScanAngleAmplitude = scanAngleAmplitude;
            ScanSpeed = scanSpeed;
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

        // Opción 1 (Peso fijo): Escaneo rápido de sector frontal
        roulette.AddItem(
            new IdleBehaviorOption(
                duration: 2.0f,
                actionType: IdleActionType.NarrowScan,
                scanAngleAmplitude: 28f,
                scanSpeed: 2.6f),
            weight: 0.45f,
            description: "Inspección Corta de Sector (2.0s)");

        // Opción 2 (PESO DINÁMICO): Vigilancia amplia de alerta (aumenta su probabilidad si el jugador está cerca)
        roulette.AddItem(
            new IdleBehaviorOption(
                duration: 4.0f,
                actionType: IdleActionType.AlertWideScan,
                scanAngleAmplitude: 105f,
                scanSpeed: 2.2f),
            weightProvider: EvaluateDynamicAlertScanWeight,
            description: "Vigilancia Activa 360° (4.0s)");

        // Opción 3 (Peso fijo): Ronda corta de pasos de guardia en el puesto
        roulette.AddItem(
            new IdleBehaviorOption(
                duration: 3.5f,
                actionType: IdleActionType.GuardStep,
                scanAngleAmplitude: 45f,
                scanSpeed: 1.8f),
            weight: 0.25f,
            description: "Ronda Corta de Guardia (3.5s)");
    }

    private float EvaluateDynamicAlertScanWeight()
    {
        const float baseWeight = 0.25f;
        const float maxAlertWeight = 0.90f;

        if (context?.Vision?.Target == null || context.Transform == null)
        {
            return baseWeight;
        }

        Vector3 toTarget = context.Vision.Target.position - context.Transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        float maxSenseRange = Mathf.Max(8f, context.Vision.VisionDistance * 1.8f);

        if (distance >= maxSenseRange)
        {
            return baseWeight;
        }

        float proximityFactor = 1f - Mathf.Clamp01(distance / maxSenseRange);
        return Mathf.Lerp(baseWeight, maxAlertWeight, proximityFactor);
    }

    public void Enter()
    {
        context.Mover.Move(Vector3.zero, 0f);

        originalRotation = context.Transform.rotation;
        scanAngleTimer = 0f;

        var chosenItem = roulette.SelectItem();
        currentOption = chosenItem.Value;
        currentTimer = currentOption.Duration;

        Debug.Log($"[{context.Transform.name}] FSM Idle: Seleccionado por Ruleta -> '{chosenItem.Description}' (Peso evaluado: {chosenItem.Weight:F2}).");
    }

    public void Update()
    {
        if (context.Vision != null && context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.AlertState);
            return;
        }

        scanAngleTimer += Time.deltaTime * currentOption.ScanSpeed;

        if (currentOption.ActionType == IdleActionType.GuardStep)
        {
            // Da pasos cortos de avance y retroceso vigilando el área
            float marchWave = Mathf.Sin(scanAngleTimer);
            Vector3 desiredDir = (originalRotation * Vector3.forward) * Mathf.Sign(marchWave);
            desiredDir.y = 0f;

            Vector3 steeredDir = context.Avoidance != null
                ? context.Avoidance.GetSteeredDirection(desiredDir.normalized)
                : desiredDir.normalized;

            context.Mover.Move(steeredDir, context.PatrolSpeed * 0.45f);
            context.Mover.Rotate(steeredDir);
        }
        else
        {
            context.Mover.Move(Vector3.zero, 0f);
            float yawOffset = Mathf.Sin(scanAngleTimer) * currentOption.ScanAngleAmplitude;
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
