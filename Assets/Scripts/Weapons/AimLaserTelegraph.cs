using UnityEngine;

// Aviso visual (mira láser) durante la fase de apuntado previo al disparo a distancia.
public class AimLaserTelegraph : MonoBehaviour, IAimTelegraph
{
    [Header("Configuración de Mira Láser")]
    [SerializeField] private Color aimingColor = new Color(1f, 0.85f, 0.15f, 0.75f);
    [SerializeField] private Color lockedColor = new Color(1f, 0.12f, 0.12f, 0.95f);
    [SerializeField] private float baseWidth = 0.055f;
    [SerializeField] private float lockedWidth = 0.095f;
    [SerializeField] private float pulseFrequency = 24f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Material laserMaterial;

    private LineRenderer lineRenderer;
    private IVisionSensor visionSensor;
    private Material runtimeMaterial;

    private void Awake()
    {
        visionSensor = GetComponent<IVisionSensor>();
        EnsureLineRenderer();
        StopAim();
    }

    public void BeginAim(Vector3 origin, Vector3 targetPoint)
    {
        EnsureLineRenderer();
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        UpdateAim(origin, targetPoint, 0f, false);
    }

    public void UpdateAim(Vector3 origin, Vector3 targetPoint, float progress01, bool isLocked)
    {
        EnsureLineRenderer();
        if (lineRenderer == null) return;

        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
        }

        Vector3 endPoint = ResolveEndPoint(origin, targetPoint);
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPoint);

        Color currentColor = Color.Lerp(aimingColor, lockedColor, Mathf.Clamp01(progress01));
        float currentWidth = Mathf.Lerp(baseWidth, lockedWidth, Mathf.Clamp01(progress01));

        if (isLocked)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseFrequency);
            currentColor = Color.Lerp(lockedColor, Color.white, pulse * 0.35f);
            currentWidth = lockedWidth * Mathf.Lerp(0.85f, 1.3f, pulse);
        }

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * 0.7f);
        lineRenderer.startWidth = currentWidth;
        lineRenderer.endWidth = currentWidth * 0.75f;
    }

    public void StopAim()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private Vector3 ResolveEndPoint(Vector3 origin, Vector3 desiredEndPoint)
    {
        Vector3 toTarget = desiredEndPoint - origin;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f) return origin;

        LayerMask mask = obstacleMask.value != 0
            ? obstacleMask
            : (visionSensor != null ? visionSensor.ObstacleMask : default);

        if (mask.value != 0 && Physics.Raycast(origin, toTarget / distance, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return desiredEndPoint;
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer != null) return;

        GameObject lineObject = new GameObject($"{name}_AimLaser");
        lineObject.transform.SetParent(transform, false);

        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.numCapVertices = 2;

        if (laserMaterial != null)
        {
            lineRenderer.sharedMaterial = laserMaterial;
        }
        else
        {
            runtimeMaterial = FeedbackMaterials.CreateTransparentVertexColor("AimLaserMaterial");
            lineRenderer.sharedMaterial = runtimeMaterial;
        }
    }

    private void OnDisable()
    {
        StopAim();
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
