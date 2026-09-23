using UnityEngine;

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

    private Vector3 currentVelocity;
    private Vector3 currentLookAhead;
    private Vector3 lookAheadVelocity;

    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        if (target == null && autoFindPlayer)
        {
            FindPlayerTarget();
        }
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(cameraRotation);

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

        Vector3 targetLookAhead = Vector3.zero;
        if (enableLookAhead)
        {
            Vector3 forwardDir = target.forward;
            forwardDir.y = 0f;
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

        Vector3 desiredPosition = ClampToBounds(target.position + offset + currentLookAhead);

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime
        );

        if (enableShake && shakeTimer > 0f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.y *= 0.5f;
            smoothedPosition += shakeOffset;

            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private void FindPlayerTarget()
    {
        var playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    private void SnapToTarget()
    {
        if (target == null) return;

        currentLookAhead = Vector3.zero;
        lookAheadVelocity = Vector3.zero;
        currentVelocity = Vector3.zero;

        transform.position = ClampToBounds(target.position + offset);
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (!useBounds) return position;

        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.z = Mathf.Clamp(position.z, minBounds.y, maxBounds.y);
        return position;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    private void OnDrawGizmosSelected()
    {
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
