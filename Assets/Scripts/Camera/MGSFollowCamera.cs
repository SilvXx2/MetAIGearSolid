using UnityEngine;

/// <summary>
/// Cámara de seguimiento estilo Metal Gear Solid 1 (Top-Down con ángulo cenital inclinado).
/// Proporciona:
/// - Perspectiva cenital inclinada (~60°) con rotación fija característica de MGS1.
/// - Suavizado cinematográfico (SmoothDamp) sin tirones (LateUpdate).
/// - Anticipación de visión (Look-Ahead) hacia donde mira el jugador para ver enemigos por delante.
/// - Límites de escenario/habitación (Room Bounds) opcionales.
/// - Efecto de temblor (Screen Shake) para impactos o explosiones.
/// </summary>
[AddComponentMenu("Camera/MGS Follow Camera")]
public class MGSFollowCamera : MonoBehaviour
{
    [Header("--- Objetivo ---")]
    [Tooltip("Transform del jugador a seguir. Si se deja vacío, buscará automáticamente al Player.")]
    [SerializeField] private Transform target;

    [Tooltip("Buscar automáticamente al jugador si el campo target está vacío.")]
    [SerializeField] private bool autoFindPlayer = true;

    [Header("--- Posición y Distancia (Estilo MGS1) ---")]
    [Tooltip("Desplazamiento relativo respecto al jugador (X: lateral, Y: altura, Z: distancia detrás).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 13f, -7f);

    [Tooltip("Ángulo de inclinación fijo de la cámara en grados (Pitch: 60° suele ser el estándar MGS1).")]
    [SerializeField] private Vector3 cameraRotation = new Vector3(60f, 0f, 0f);

    [Header("--- Suavizado de Seguimiento ---")]
    [Tooltip("Tiempo de amortiguación (SmoothDamp). Valores menores son más rígidos, mayores más suaves.")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("--- Anticipación de Visión (Look-Ahead) ---")]
    [Tooltip("Si está activo, la cámara se desplaza ligeramente hacia donde mira el jugador (ideal para sigilo).")]
    [SerializeField] private bool enableLookAhead = true;

    [Tooltip("Distancia que se adelanta la cámara en la dirección frontal del jugador.")]
    [SerializeField] private float lookAheadDistance = 2.5f;

    [Tooltip("Tiempo de suavizado para los cambios de dirección de la anticipación.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float lookAheadSmoothTime = 0.3f;

    [Header("--- Límites de Habitación (Room Bounds) ---")]
    [Tooltip("Activa la restricción de movimiento de la cámara a un área determinada.")]
    [SerializeField] private bool useBounds = false;

    [Tooltip("Límites mínimos en X (horizontal) y Z (profundidad).")]
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);

    [Tooltip("Límites máximos en X (horizontal) y Z (profundidad).")]
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("--- Temblor de Pantalla (Screen Shake) ---")]
    [SerializeField] private bool enableShake = true;

    // Variables internas de estado
    private Vector3 currentVelocity;
    private Vector3 currentLookAhead;
    private Vector3 lookAheadVelocity;
    private Vector3 targetOffset;
    private float offsetTransitionSpeed = 5f;

    // Estado del Screen Shake
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    public Transform Target => target;
    public bool UseBounds { get => useBounds; set => useBounds = value; }

    private void Awake()
    {
        targetOffset = offset;

        if (target == null && autoFindPlayer)
        {
            FindPlayerTarget();
        }
    }

    private void Start()
    {
        // Aplicar la rotación fija clásica de MGS1
        transform.rotation = Quaternion.Euler(cameraRotation);

        // Posicionar instantáneamente en el primer frame para evitar que viaje desde el origen
        if (target != null)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer)
            {
                FindPlayerTarget();
            }

            if (target == null) return;
        }

        // 1. Transición suave de offset si fue modificado (por ejemplo, al pegarse a una pared)
        offset = Vector3.Lerp(offset, targetOffset, Time.deltaTime * offsetTransitionSpeed);

        // 2. Calcular la anticipación de visión (Look-Ahead)
        Vector3 targetLookAhead = Vector3.zero;
        if (enableLookAhead)
        {
            Vector3 forwardDir = target.forward;
            forwardDir.y = 0f; // Mantener la anticipación en el plano del suelo
            if (forwardDir.sqrMagnitude > 0.001f)
            {
                targetLookAhead = forwardDir.normalized * lookAheadDistance;
            }
        }

        currentLookAhead = Vector3.SmoothDamp(
            currentLookAhead,
            targetLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        // 3. Calcular posición deseada
        Vector3 desiredPosition = target.position + offset + currentLookAhead;

        // 4. Aplicar límites de habitación/escenario si están activos
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        // 5. Suavizado de seguimiento hacia la posición deseada
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime
        );

        // 6. Aplicar temblor de pantalla (Screen Shake) si está activo
        if (enableShake && shakeTimer > 0f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.y *= 0.5f; // Reducir movimiento vertical para mantener estabilidad visual
            smoothedPosition += shakeOffset;

            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothedPosition;

        // Mantener la rotación fija
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    /// <summary>
    /// Busca automáticamente el Transform del jugador en la escena.
    /// </summary>
    public void FindPlayerTarget()
    {
        // Buscar primero por el script PlayerMovement existente en el proyecto
        var playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
            return;
        }

        // Alternativa: buscar por Tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    /// <summary>
    /// Teletransporta inmediatamente la cámara a la posición del jugador sin suavizado.
    /// Útil al reaparecer, cambiar de zona o iniciar la partida.
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        currentLookAhead = Vector3.zero;
        lookAheadVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;

        Vector3 initialPos = target.position + offset;
        if (useBounds)
        {
            initialPos.x = Mathf.Clamp(initialPos.x, minBounds.x, maxBounds.x);
            initialPos.z = Mathf.Clamp(initialPos.z, minBounds.y, maxBounds.y);
        }

        transform.position = initialPos;
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    /// <summary>
    /// Asigna dinámicamente un nuevo objetivo para la cámara.
    /// </summary>
    public void SetTarget(Transform newTarget, bool snapImmediately = false)
    {
        target = newTarget;
        if (snapImmediately && target != null)
        {
            SnapToTarget();
        }
    }

    /// <summary>
    /// Permite modificar el offset de forma dinámica (por ejemplo, zoom o encuadre de pared estilo MGS).
    /// </summary>
    public void SetCustomOffset(Vector3 newOffset, float transitionSpeed = 5f)
    {
        targetOffset = newOffset;
        offsetTransitionSpeed = transitionSpeed;
    }

    /// <summary>
    /// Restablece el offset al valor original configurado en el inspector.
    /// </summary>
    public void ResetOffset(Vector3 defaultOffset, float transitionSpeed = 5f)
    {
        targetOffset = defaultOffset;
        offsetTransitionSpeed = transitionSpeed;
    }

    /// <summary>
    /// Configura los límites del área/habitación dinámicamente (por ejemplo al entrar a una nueva sala).
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    /// <summary>
    /// Desactiva los límites de la cámara.
    /// </summary>
    public void DisableBounds()
    {
        useBounds = false;
    }

    /// <summary>
    /// Dispara un temblor de pantalla por una duración e intensidad determinada.
    /// </summary>
    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar límites del área en la escena de Unity si están activados
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) * 0.5f,
                transform.position.y,
                (minBounds.y + maxBounds.y) * 0.5f
            );
            Vector3 size = new Vector3(
                Mathf.Abs(maxBounds.x - minBounds.x),
                1f,
                Mathf.Abs(maxBounds.y - minBounds.y)
            );
            Gizmos.DrawWireCube(center, size);
        }

        // Dibujar línea guía hacia el objetivo y punto de anticipación
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);

            if (enableLookAhead)
            {
                Gizmos.color = Color.green;
                Vector3 lookPoint = target.position + (target.forward * lookAheadDistance);
                Gizmos.DrawWireSphere(lookPoint, 0.4f);
                Gizmos.DrawLine(target.position, lookPoint);
            }
        }
    }
}
