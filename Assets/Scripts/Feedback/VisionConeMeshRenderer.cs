using UnityEngine;

// Dibuja en el piso el cono de visión real del NPC (el mismo que usa IVisionSensor),
// recortado contra las paredes con raycasts y coloreado según el nivel de alerta del estado actual.
[DisallowMultipleComponent]
public class VisionConeMeshRenderer : MonoBehaviour
{
    [Header("Material (Sprites/Default o cualquier shader con color de vértice y transparencia)")]
    [SerializeField] private Material coneMaterial;

    [Header("Resolución")]
    [SerializeField, Range(8, 96)] private int rayCount = 36;
    [SerializeField] private float refreshInterval = 0.05f;
    [SerializeField] private float groundOffset = 0.06f;

    [Header("Transparencia")]
    [SerializeField, Range(0f, 1f)] private float innerAlpha = 0.55f;
    [SerializeField, Range(0f, 1f)] private float outerAlpha = 0.2f;
    [SerializeField] private float colorLerpSpeed = 8f;

    private IVisionSensor vision;
    private INpcStatusSource status;
    private Collider bodyCollider;

    private GameObject coneObject;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private int[] triangles;
    private Color currentColor;
    private float refreshTimer;

    private void Awake()
    {
        vision = GetComponent<IVisionSensor>();
        status = GetComponent<INpcStatusSource>();
        bodyCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (vision == null)
        {
            Debug.LogWarning($"{name}: VisionConeMeshRenderer necesita un componente que implemente IVisionSensor.", this);
            enabled = false;
            return;
        }

        if (coneMaterial == null)
        {
            coneMaterial = FeedbackMaterials.CreateTransparentVertexColor("VisionCone_Runtime");
        }

        BuildMeshObject();
        currentColor = NpcAlertPalette.Get(status != null ? status.AlertLevel : NpcAlertLevel.Calm);
        Rebuild();
    }

    private void BuildMeshObject()
    {
        // Objeto en la raíz de la escena: así el escalado del NPC no deforma el cono (vértices en coordenadas de mundo).
        coneObject = new GameObject($"{name}_VisionCone");
        coneObject.AddComponent<MeshFilter>().sharedMesh = mesh = new Mesh { name = "VisionCone" };
        var meshRenderer = coneObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = coneMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        mesh.MarkDynamic();

        vertices = new Vector3[rayCount + 2];
        colors = new Color[rayCount + 2];
        triangles = new int[rayCount * 3];
        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
    }

    private void LateUpdate()
    {
        if (mesh == null) return;

        Color targetColor = NpcAlertPalette.Get(status != null ? status.AlertLevel : NpcAlertLevel.Calm);
        if (vision.CanSeeTarget)
        {
            // Pulso cuando el jugador está dentro del cono y sin paredes en el medio.
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 14f);
            targetColor = Color.Lerp(NpcAlertPalette.Hostile, Color.white, pulse * 0.35f);
        }

        currentColor = Color.Lerp(currentColor, targetColor, 1f - Mathf.Exp(-colorLerpSpeed * Time.deltaTime));

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = refreshInterval;
            Rebuild();
        }
        else
        {
            ApplyColors();
        }
    }

    private void Rebuild()
    {
        float groundY = bodyCollider != null ? bodyCollider.bounds.min.y : transform.position.y;
        Vector3 eye = transform.position + vision.EyeOffset;
        Vector3 groundCenter = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);

        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        float halfAngle = vision.VisionAngle * 0.5f;
        float distance = vision.VisionDistance;
        vertices[0] = groundCenter;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)rayCount);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
            float length = distance;
            if (Physics.Raycast(eye, dir, out RaycastHit hit, distance, vision.ObstacleMask, QueryTriggerInteraction.Ignore))
            {
                length = hit.distance;
            }

            vertices[i + 1] = groundCenter + dir * length;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        ApplyColors();
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void ApplyColors()
    {
        Color inner = currentColor;
        inner.a = innerAlpha;
        Color outer = currentColor;
        outer.a = outerAlpha;

        colors[0] = inner;
        for (int i = 1; i < colors.Length; i++) colors[i] = outer;
        mesh.colors = colors;
    }

    private void OnEnable()
    {
        if (coneObject != null) coneObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (coneObject != null) coneObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (coneObject != null) Destroy(coneObject);
        if (mesh != null) Destroy(mesh);
    }
}
