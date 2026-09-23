using System;
using UnityEngine;

/// <summary>
/// Estado Idle del NPC requerido por la consigna del trabajo práctico:
/// "Idle: Tras cierta cantidad de iteraciones en el estado Patrol o cuando se le agota una energía
/// o luego de un tiempo, el NPC deberá mantenerse en su posición. Tras unos segundos y si nada
/// extraño pasa, deberá continuar su recorrido."
/// 
/// Integra de forma directa el algoritmo de Selección por Ruleta (Roulette Wheel Selection):
/// "Para que el Roulette wheel selection sea aceptado, este debe contar con 3 posibles resultados
/// y de diferentes probabilidades."
/// </summary>
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

    public string CurrentDecisionDescription { get; private set; } = "Ninguna";

    public EnemyIdleState(IEnemyContext context)
    {
        this.context = context;

        // Configuración de la Ruleta de Selección con 3 resultados de probabilidades disímiles (requisito de consigna)
        roulette = new RouletteWheelSelection<IdleBehaviorOption>();

        // 1. Pausa breve de inspección general (50% de probabilidad)
        roulette.AddItem(
            new IdleBehaviorOption(duration: 2.0f, performScanRotation: false),
            weight: 0.50f,
            description: "Inspección Corta (2.0s)");

        // 2. Guardia vigilante con escaneo angular izquierda/derecha (35% de probabilidad)
        roulette.AddItem(
            new IdleBehaviorOption(duration: 4.0f, performScanRotation: true),
            weight: 0.35f,
            description: "Vigilancia Activa con Escaneo (4.0s)");

        // 3. Descanso táctico extendido (15% de probabilidad)
        roulette.AddItem(
            new IdleBehaviorOption(duration: 6.0f, performScanRotation: false),
            weight: 0.15f,
            description: "Descanso Táctico Extendido (6.0s)");
    }

    public void Enter()
    {
        // Detener por completo el movimiento lineal
        context.Mover.Move(Vector3.zero, 0f);

        originalRotation = context.Transform.rotation;
        scanAngleTimer = 0f;

        // Selección estocástica del comportamiento Idle mediante Roulette Wheel
        var chosenItem = roulette.SelectItem();
        currentOption = chosenItem.Value;
        currentTimer = currentOption.Duration;
        CurrentDecisionDescription = chosenItem.Description;

        // Comentario/Log para trazabilidad y justificación del parcial
        Debug.Log($"[{context.Transform.name}] FSM Idle: Seleccionado por Ruleta -> '{chosenItem.Description}' (Peso: {chosenItem.Weight:P0}).");
    }

    public void Update()
    {
        // 1. Si detecta al jugador dentro de su campo visual (Line of Sight), interrumpe el reposo inmediatamente
        if (context.Vision != null && context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.AlertState);
            return;
        }

        // 2. Mantenerlo quieto
        context.Mover.Move(Vector3.zero, 0f);

        // 3. Si la opción seleccionada por la ruleta incluye escaneo, oscilar suavemente la orientación
        if (currentOption.PerformScanRotation)
        {
            scanAngleTimer += Time.deltaTime * 2f;
            float yawOffset = Mathf.Sin(scanAngleTimer) * 35f; // oscila +/- 35 grados
            context.Transform.rotation = originalRotation * Quaternion.Euler(0f, yawOffset, 0f);
        }

        // 4. Temporizador de descanso
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            // Restablecer orientación original antes de retomar la marcha
            context.Transform.rotation = originalRotation;
            context.StateMachine.ChangeState(context.PatrolState);
        }
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
