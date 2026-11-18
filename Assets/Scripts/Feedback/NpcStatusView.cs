using TMPro;
using UnityEngine;

// HUD flotante sobre el NPC: estado actual de la FSM / árbol de decisión, munición o progreso de recarga,
// y un ícono "!" / "?" al cambiar el nivel de alerta (feedback inmediato de detección).
[DisallowMultipleComponent]
public class NpcStatusView : MonoBehaviour
{
    [SerializeField] private float heightAboveHead = 0.6f;
    [SerializeField] private float labelFontSize = 2.6f;
    [SerializeField] private float alertIconFontSize = 9f;
    [SerializeField] private float alertIconDuration = 1.2f;
    [SerializeField] private bool showWeaponLine = true;

    private INpcStatusSource status;
    private IWeaponStatusSource weapon;
    private Collider bodyCollider;
    private Camera mainCamera;

    private GameObject root;
    private TextMeshPro label;
    private TextMeshPro alertIcon;
    private NpcAlertLevel lastLevel;
    private float alertIconTimer;
    private string lastText;

    private void Awake()
    {
        status = GetComponent<INpcStatusSource>();
        weapon = GetComponent<IWeaponStatusSource>();
        bodyCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (status == null)
        {
            Debug.LogWarning($"{name}: NpcStatusView necesita un componente que implemente INpcStatusSource.", this);
            enabled = false;
            return;
        }

        root = new GameObject($"{name}_StatusView");
        label = CreateText("Label", labelFontSize, Vector3.zero);
        alertIcon = CreateText("AlertIcon", alertIconFontSize, new Vector3(0f, 1.1f, 0f));
        alertIcon.fontStyle = FontStyles.Bold;
        alertIcon.text = string.Empty;
        lastLevel = status.AlertLevel;
    }

    private TextMeshPro CreateText(string objectName, float fontSize, Vector3 localOffset)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(root.transform, false);
        go.transform.localPosition = localOffset;
        var text = go.AddComponent<TextMeshPro>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Bottom;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color32(0, 0, 0, 220);
        text.rectTransform.sizeDelta = new Vector2(12f, 3f);
        text.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return text;
    }

    private void LateUpdate()
    {
        if (root == null) return;
        if (mainCamera == null) mainCamera = Camera.main;

        float topY = bodyCollider != null ? bodyCollider.bounds.max.y : transform.position.y + 1f;
        root.transform.position = new Vector3(transform.position.x, topY + heightAboveHead, transform.position.z);
        if (mainCamera != null) root.transform.rotation = mainCamera.transform.rotation;

        NpcAlertLevel level = status.AlertLevel;
        UpdateAlertIcon(level);
        UpdateLabel(level);
    }

    private void UpdateLabel(NpcAlertLevel level)
    {
        string color = ColorUtility.ToHtmlStringRGB(NpcAlertPalette.Get(level));
        string text = $"<color=#{color}><b>{status.StatusLabel}</b></color>";

        if (showWeaponLine && weapon != null)
        {
            text += "\n<size=75%>" + BuildWeaponLine() + "</size>";
        }

        if (text != lastText)
        {
            label.text = text;
            lastText = text;
        }
    }

    private string BuildWeaponLine()
    {
        if (!weapon.HasWeapon) return "<color=#FF8C1A>SIN ARMA</color>";

        if (weapon.IsReloading)
        {
            int percent = Mathf.RoundToInt(weapon.ReloadProgress01 * 100f);
            return $"<color=#FF8C1A>RECARGA {percent}%</color>";
        }

        string ammoColor = weapon.CurrentAmmo > 1 ? "FFFFFF" : "FF8C1A";
        return $"<color=#{ammoColor}>MUNICIÓN {weapon.CurrentAmmo}/{weapon.MaxAmmo}</color>";
    }

    private void UpdateAlertIcon(NpcAlertLevel level)
    {
        if (level != lastLevel)
        {
            if (level == NpcAlertLevel.Hostile) ShowAlertIcon("!", NpcAlertPalette.Hostile);
            else if (level == NpcAlertLevel.Suspicious) ShowAlertIcon("?", NpcAlertPalette.Suspicious);
            else if (level == NpcAlertLevel.Fleeing) ShowAlertIcon("!!", NpcAlertPalette.Fleeing);
            lastLevel = level;
        }

        if (alertIconTimer > 0f)
        {
            alertIconTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(alertIconTimer / alertIconDuration);
            alertIcon.transform.localScale = Vector3.one * (1f + 0.4f * Mathf.Clamp01((t - 0.8f) * 5f));
            if (alertIconTimer <= 0f) alertIcon.text = string.Empty;
        }
    }

    private void ShowAlertIcon(string icon, Color color)
    {
        alertIcon.text = icon;
        alertIcon.color = color;
        alertIconTimer = alertIconDuration;
    }

    private void OnEnable()
    {
        if (root != null) root.SetActive(true);
    }

    private void OnDisable()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root);
    }
}
