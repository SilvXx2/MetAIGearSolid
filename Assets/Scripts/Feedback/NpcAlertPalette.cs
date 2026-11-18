using UnityEngine;

// Paleta única de colores por nivel de alerta, compartida por todas las vistas de feedback.
public static class NpcAlertPalette
{
    public static readonly Color Calm = new Color(0.30f, 0.85f, 1.00f);
    public static readonly Color Suspicious = new Color(1.00f, 0.80f, 0.15f);
    public static readonly Color Hostile = new Color(1.00f, 0.22f, 0.18f);
    public static readonly Color Fleeing = new Color(0.75f, 0.45f, 1.00f);
    public static readonly Color Vulnerable = new Color(1.00f, 0.55f, 0.10f);

    public static Color Get(NpcAlertLevel level)
    {
        switch (level)
        {
            case NpcAlertLevel.Suspicious: return Suspicious;
            case NpcAlertLevel.Hostile: return Hostile;
            case NpcAlertLevel.Fleeing: return Fleeing;
            case NpcAlertLevel.Vulnerable: return Vulnerable;
            default: return Calm;
        }
    }
}
