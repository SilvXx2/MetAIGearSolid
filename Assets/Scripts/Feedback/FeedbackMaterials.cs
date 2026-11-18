using UnityEngine;

// Fallback para crear materiales de feedback si no se asignaron desde el Inspector.
// En la escena se asignan los materiales de Assets/Art/Materials para que el shader quede incluido en el build.
public static class FeedbackMaterials
{
    public static Material CreateTransparentVertexColor(string materialName)
    {
        Shader shader = Shader.Find("Sprites/Default");
        return new Material(shader) { name = materialName };
    }
}
