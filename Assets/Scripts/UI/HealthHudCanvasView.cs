using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Vista táctica de la vida del jugador en el HUD (barra segmentada + texto + destello rojo al recibir daño).
[RequireComponent(typeof(RectTransform))]
public class HealthHudCanvasView : MonoBehaviour, IHealthHudView
{
    [Header("Referencias Opcionales (si están vacías se crean automáticamente)")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text healthLabel;
    [SerializeField] private Image damageFlashOverlay;

    [Header("Estilo Visual")]
    [SerializeField] private bool showHealthLabel = false;
    [SerializeField] private int visualSegments = 3;
    [SerializeField] private Color highHealthColor = new Color(0.22f, 0.85f, 0.42f, 1f);
    [SerializeField] private Color mediumHealthColor = new Color(1.00f, 0.78f, 0.15f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.95f, 0.22f, 0.18f, 1f);
    [SerializeField] private Color panelBackgroundColor = new Color(0.06f, 0.08f, 0.10f, 0.82f);
    [SerializeField] private Color damageFlashColor = new Color(0.95f, 0.12f, 0.12f, 0.36f);
    [SerializeField] private float damageFlashDuration = 0.35f;

    private Coroutine flashCoroutine;
    private bool visualBuilt;

    private void Awake()
    {
        EnsureVisualHierarchy();
    }

    public void Render(float currentHealth, float maxHealth)
    {
        EnsureVisualHierarchy();

        float safeMax = Mathf.Max(1f, maxHealth);
        float safeCurrent = Mathf.Clamp(currentHealth, 0f, safeMax);
        float ratio = safeCurrent / safeMax;

        if (fillImage != null)
        {
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillImage.color = EvaluateHealthColor(ratio);
        }

        if (healthLabel != null)
        {
            healthLabel.gameObject.SetActive(showHealthLabel);
            if (showHealthLabel)
            {
                healthLabel.text = $"{Mathf.CeilToInt(safeCurrent)}/{Mathf.CeilToInt(safeMax)}";
            }
        }
    }

    public void PlayDamageFlash()
    {
        EnsureVisualHierarchy();
        if (damageFlashOverlay == null || !gameObject.activeInHierarchy) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private Color EvaluateHealthColor(float ratio)
    {
        if (ratio > 0.6f) return highHealthColor;
        if (ratio > 0.3f) return mediumHealthColor;
        return lowHealthColor;
    }

    private IEnumerator DamageFlashRoutine()
    {
        float elapsed = 0f;
        damageFlashOverlay.gameObject.SetActive(true);

        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / damageFlashDuration);
            damageFlashOverlay.color = new Color(
                damageFlashColor.r,
                damageFlashColor.g,
                damageFlashColor.b,
                damageFlashColor.a * t);
            yield return null;
        }

        damageFlashOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);
        damageFlashOverlay.gameObject.SetActive(false);
        flashCoroutine = null;
    }

    private void EnsureVisualHierarchy()
    {
        if (visualBuilt && fillImage != null) return;
        visualBuilt = true;

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            backgroundImage.color = panelBackgroundColor;
            backgroundImage.raycastTarget = false;
        }

        if (showHealthLabel && healthLabel == null)
        {
            GameObject labelGo = new GameObject("HealthLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.layer = gameObject.layer;
            labelGo.transform.SetParent(transform, false);

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.52f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(12f, 0f);
            labelRect.offsetMax = new Vector2(-12f, -4f);

            healthLabel = labelGo.GetComponent<TextMeshProUGUI>();
            healthLabel.fontSize = 20f;
            healthLabel.fontStyle = FontStyles.Bold;
            healthLabel.alignment = TextAlignmentOptions.Left;
            healthLabel.color = Color.white;
            healthLabel.raycastTarget = false;
        }
        else if (!showHealthLabel && healthLabel != null)
        {
            healthLabel.gameObject.SetActive(false);
        }

        if (fillImage == null)
        {
            GameObject barBgGo = new GameObject("BarTrack", typeof(RectTransform), typeof(Image));
            barBgGo.layer = gameObject.layer;
            barBgGo.transform.SetParent(transform, false);

            RectTransform trackRect = barBgGo.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, showHealthLabel ? 0.48f : 1f);
            trackRect.offsetMin = new Vector2(10f, 10f);
            trackRect.offsetMax = new Vector2(-10f, showHealthLabel ? -4f : -10f);

            Image trackImg = barBgGo.GetComponent<Image>();
            trackImg.color = new Color(0.16f, 0.19f, 0.22f, 0.95f);
            trackImg.raycastTarget = false;

            GameObject fillGo = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            fillGo.layer = gameObject.layer;
            fillGo.transform.SetParent(barBgGo.transform, false);

            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            fillImage = fillGo.GetComponent<Image>();
            fillImage.color = highHealthColor;
            fillImage.raycastTarget = false;

            // Divisores visuales para mostrar claramente los 3 golpes de vida
            int dividers = Mathf.Max(1, visualSegments) - 1;
            for (int i = 1; i <= dividers; i++)
            {
                float anchorX = (float)i / visualSegments;
                GameObject divGo = new GameObject($"Divider_{i}", typeof(RectTransform), typeof(Image));
                divGo.layer = gameObject.layer;
                divGo.transform.SetParent(barBgGo.transform, false);

                RectTransform divRect = divGo.GetComponent<RectTransform>();
                divRect.anchorMin = new Vector2(anchorX, 0f);
                divRect.anchorMax = new Vector2(anchorX, 1f);
                divRect.sizeDelta = new Vector2(3f, 0f);
                divRect.anchoredPosition = Vector2.zero;

                Image divImg = divGo.GetComponent<Image>();
                divImg.color = panelBackgroundColor;
                divImg.raycastTarget = false;
            }
        }

        if (damageFlashOverlay == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                GameObject flashGo = new GameObject("DamageFlashOverlay", typeof(RectTransform), typeof(Image));
                flashGo.layer = parentCanvas.gameObject.layer;
                flashGo.transform.SetParent(parentCanvas.transform, false);
                flashGo.transform.SetAsFirstSibling();

                RectTransform flashRect = flashGo.GetComponent<RectTransform>();
                flashRect.anchorMin = Vector2.zero;
                flashRect.anchorMax = Vector2.one;
                flashRect.offsetMin = Vector2.zero;
                flashRect.offsetMax = Vector2.zero;

                damageFlashOverlay = flashGo.GetComponent<Image>();
                damageFlashOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);
                damageFlashOverlay.raycastTarget = false;
                flashGo.SetActive(false);
            }
        }
    }
}
