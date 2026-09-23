using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyDefinition", menuName = "MetAIGearSolid/Key Definition")]
public class KeyDefinition : ScriptableObject
{
    [Header("Identidad de la Llave")]
    [SerializeField] private string displayName = "Llave";
    [SerializeField] private Color color = Color.yellow;
    [SerializeField] private Sprite icon;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Color Color => color;
    public Sprite Icon => icon;
}
