using UnityEngine;

// Flecha en el piso delante del NPC: muestra hacia dónde mira y su nivel de alerta (color + pulso).
[DisallowMultipleComponent]
public class NpcDirectionIndicator : MonoBehaviour
{
    [SerializeField] private Material indicatorMaterial;
    [SerializeField] private float distanceAhead = 0.35f;
    [SerializeField] private float size = 1f;
    [SerializeField] private float groundOffset = 0.08f;
    [SerializeField] private float hostilePulseSpeed = 10f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private INpcStatusSource status;
    private Collider bodyCollider;
    private GameObject indicator;
    private MeshRenderer indicatorRenderer;
    private Mesh chevronMesh;
    private MaterialPropertyBlock propertyBlock;
    private float bodyRadius;

    private void Awake()
    {
        status = GetComponent<INpcStatusSource>();
        bodyCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (indicatorMaterial == null)
        {
            indicatorMaterial = FeedbackMaterials.CreateTransparentVertexColor("DirectionIndicator_Runtime");
        }

        chevronMesh = ProceduralModelLibrary.Chevron();
        Color[] white = new Color[chevronMesh.vertexCount];
        for (int i = 0; i < white.Length; i++) white[i] = Color.white;
        chevronMesh.colors = white;

        indicator = new GameObject($"{name}_DirectionIndicator");
        indicator.AddComponent<MeshFilter>().sharedMesh = chevronMesh;
        indicatorRenderer = indicator.AddComponent<MeshRenderer>();
        indicatorRenderer.sharedMaterial = indicatorMaterial;
        indicatorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        propertyBlock = new MaterialPropertyBlock();

        if (bodyCollider != null)
        {
            Vector3 extents = bodyCollider.bounds.extents;
            bodyRadius = Mathf.Max(extents.x, extents.z);
        }
        else
        {
            bodyRadius = 0.5f;
        }
    }

    private void LateUpdate()
    {
        if (indicator == null) return;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        float groundY = bodyCollider != null ? bodyCollider.bounds.min.y : transform.position.y;
        Vector3 basePos = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);

        NpcAlertLevel level = status != null ? status.AlertLevel : NpcAlertLevel.Calm;
        float scale = size * Mathf.Max(0.6f, bodyRadius * 1.4f);
        if (level == NpcAlertLevel.Hostile)
        {
            scale *= 1f + 0.15f * Mathf.Sin(Time.time * hostilePulseSpeed);
        }

        indicator.transform.SetPositionAndRotation(basePos + forward * (bodyRadius + distanceAhead), Quaternion.LookRotation(forward, Vector3.up));
        indicator.transform.localScale = Vector3.one * scale;

        Color color = NpcAlertPalette.Get(level);
        color.a = 0.9f;
        propertyBlock.SetColor(ColorId, color);
        indicatorRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnEnable()
    {
        if (indicator != null) indicator.SetActive(true);
    }

    private void OnDisable()
    {
        if (indicator != null) indicator.SetActive(false);
    }

    private void OnDestroy()
    {
        if (indicator != null) Destroy(indicator);
        if (chevronMesh != null) Destroy(chevronMesh);
    }
}
