using UnityEngine;

/// <summary>
/// Implementación de Steering Behaviour Complejo: "Hide" (Ocultamiento Dinámico).
/// 
/// REFERENCIA Y JUSTIFICACIÓN TEÓRICA (Craig Reynolds, GDC 1999 - Red3D):
/// -----------------------------------------------------------------------
/// "Hide behavior involves identifying a target location which is on the opposite side
/// of an obstacle from the opponent, and steering toward it using seek [or arrival]."
/// (https://www.red3d.com/cwr/steer/gdc99/)
/// 
/// ALGORITMO:
/// 1. Escanea los obstáculos circundantes dentro de un radio de detección mediante su LayerMask.
/// 2. Para cada obstáculo, calcula el "Hiding Spot" (punto de sombra o cobertura) proyectando
///    un vector desde el cazador (Jugador) a través del centro del obstáculo, desplazándolo
///    el radio del obstáculo más una distancia de holgura (coverOffset).
/// 3. Selecciona el mejor escondite (aquel con menor distancia al agente).
/// 4. Aplica el comportamiento de dirección "Arrival" hacia el punto seleccionado, permitiendo
///    desacelerar suavemente al alcanzar la cobertura para no sobrepasarla ni quedar expuesto.
/// 5. Se combina (blend) con el sensor de Evitación de Obstáculos (Obstacle Avoidance) para rodear
///    la propia estructura hasta alcanzar su punto posterior.
/// 6. En caso de no haber obstáculos disponibles, realiza fallback al comportamiento de Evade predictivo.
/// </summary>
public class HideSteering
{
    private readonly Transform agentTransform;
    private readonly LayerMask obstacleMask;
    private readonly float scanRadius;
    private readonly float coverOffset;
    private readonly float slowingDistance;
    private readonly float arrivalThreshold;

    // Campos de telemetría y debug
    private Vector3 lastChosenSpot;
    private bool hasHidingSpot;

    public bool HasHidingSpot => hasHidingSpot;
    public Vector3 LastChosenSpot => lastChosenSpot;

    public HideSteering(
        Transform agentTransform,
        LayerMask obstacleMask,
        float scanRadius = 15f,
        float coverOffset = 1.5f,
        float slowingDistance = 2.5f,
        float arrivalThreshold = 0.6f)
    {
        this.agentTransform = agentTransform;
        this.obstacleMask = obstacleMask;
        this.scanRadius = scanRadius;
        this.coverOffset = coverOffset;
        this.slowingDistance = slowingDistance;
        this.arrivalThreshold = arrivalThreshold;
    }

    /// <summary>
    /// Calcula la dirección y velocidad deseada para ocultarse respecto al objetivo (Jugador).
    /// Devuelve Vector3 con la dirección y magnitud deseada (entre 0 y 1).
    /// </summary>
    public Vector3 CalculateHideDirection(
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float agentSpeed,
        IAvoidanceSensor avoidanceSensor = null)
    {
        Vector3 agentPos = agentTransform.position;
        agentPos.y = 0f;
        targetPosition.y = 0f;

        // 1. Detección de obstáculos en el entorno del agente
        Collider[] obstacles = Physics.OverlapSphere(agentTransform.position, scanRadius, obstacleMask, QueryTriggerInteraction.Ignore);

        Collider bestObstacle = null;
        Vector3 bestHidingSpot = Vector3.zero;
        float minDistanceToSpot = float.MaxValue;

        for (int i = 0; i < obstacles.Length; i++)
        {
            Collider col = obstacles[i];
            if (col == null || col.transform.root == agentTransform.root) continue;

            // Centro del obstáculo en el plano XZ
            Vector3 obstacleCenter = col.bounds.center;
            obstacleCenter.y = 0f;

            // Radio aproximado del obstáculo en plano horizontal
            float obstacleRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);

            // Vector director desde el jugador hacia el centro del obstáculo
            Vector3 toObstacle = (obstacleCenter - targetPosition);
            if (toObstacle.sqrMagnitude < 0.001f) continue;

            Vector3 hideDir = toObstacle.normalized;

            // Punto de cobertura: detrás del obstáculo respecto a la línea de visión del jugador
            Vector3 spot = obstacleCenter + hideDir * (obstacleRadius + coverOffset);
            spot.y = agentTransform.position.y;

            float distToSpot = Vector3.Distance(agentPos, spot);
            if (distToSpot < minDistanceToSpot)
            {
                minDistanceToSpot = distToSpot;
                bestHidingSpot = spot;
                bestObstacle = col;
            }
        }

        // 2. Si no hay obstáculos disponibles, fallback a Evade predictivo (Reynolds GDC 99)
        if (bestObstacle == null)
        {
            hasHidingSpot = false;
            float lookAheadTime = Mathf.Clamp(Vector3.Distance(agentPos, targetPosition) / Mathf.Max(agentSpeed, 0.1f), 0.1f, 1.5f);
            Vector3 predictedTargetPos = targetPosition + targetVelocity * lookAheadTime;
            predictedTargetPos.y = agentPos.y;

            Vector3 evadeDir = (agentPos - predictedTargetPos).normalized;
            if (avoidanceSensor != null)
            {
                evadeDir = avoidanceSensor.GetSteeredDirection(evadeDir);
            }
            return evadeDir;
        }

        hasHidingSpot = true;
        lastChosenSpot = bestHidingSpot;

        // 3. Comportamiento "Arrival" hacia el Hiding Spot seleccionado
        Vector3 toSpot = bestHidingSpot - agentPos;
        toSpot.y = 0f;
        float distance = toSpot.magnitude;

        // Si ya estamos posicionados detrás de la cobertura, detenerse y mantenerse oculto
        if (distance <= arrivalThreshold)
        {
            return Vector3.zero;
        }

        // Desaceleración proporcional en la zona de frenado (Arrival de Reynolds)
        float speedFactor = (distance > slowingDistance)
            ? 1f
            : Mathf.Clamp01(distance / slowingDistance);

        Vector3 desiredDirection = toSpot.normalized;

        // 4. Combinación con Obstacle Avoidance para contornear obstáculos mientras busca la posición
        Vector3 finalDirection = avoidanceSensor != null
            ? avoidanceSensor.GetSteeredDirection(desiredDirection)
            : desiredDirection;

        return finalDirection * speedFactor;
    }

    /// <summary>
    /// Renderiza Gizmos en el editor para visualización del cálculo en tiempo real.
    /// </summary>
    public void DrawGizmos(Vector3 currentPosition, Vector3 targetPosition)
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        Gizmos.DrawWireSphere(currentPosition, scanRadius);

        if (hasHidingSpot)
        {
            // Dibuja el punto de escondite seleccionado
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastChosenSpot, 0.4f);

            // Línea de proyección desde el jugador a través del escondite
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(targetPosition, lastChosenSpot);

            // Trayectoria deseada del agente hacia el escondite
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(currentPosition, lastChosenSpot);
        }
    }
}
