using UnityEngine;

// Catálogo de modelos low-poly generados por código para reemplazar las primitivas (cubos / cápsulas).
// Todas las medidas están en el espacio local del collider original: cápsula de alto 2 (y de -1 a 1), radio 0.5, frente +Z.
public static class ProceduralModelLibrary
{
    // Submeshes del guardia: 0 = uniforme, 1 = piel, 2 = equipo oscuro, 3 = visor / acento.
    public static Mesh Guard()
    {
        var b = new ProceduralMeshBuilder();
        Quaternion id = Quaternion.identity;

        // Piernas y botas
        for (int side = -1; side <= 1; side += 2)
        {
            float x = 0.14f * side;
            b.Lathe(0, new[] { new Vector2(0f, -0.86f), new Vector2(0.11f, -0.86f), new Vector2(0.12f, -0.5f), new Vector2(0.14f, -0.2f), new Vector2(0f, -0.2f) }, 7, new Vector3(x, 0f, 0f));
            b.Box(2, new Vector3(x, -0.93f, 0.05f), new Vector3(0.2f, 0.14f, 0.34f), id);
        }

        // Torso con hombros anchos y cinturón
        b.Lathe(0, new[]
        {
            new Vector2(0f, -0.28f), new Vector2(0.24f, -0.28f), new Vector2(0.27f, -0.05f),
            new Vector2(0.31f, 0.22f), new Vector2(0.36f, 0.38f), new Vector2(0.22f, 0.5f), new Vector2(0f, 0.52f)
        }, 8, Vector3.zero, id, new Vector3(1f, 1f, 0.72f));
        b.Lathe(2, new[] { new Vector2(0.26f, -0.24f), new Vector2(0.275f, -0.22f), new Vector2(0.275f, -0.12f), new Vector2(0.26f, -0.1f) }, 8, Vector3.zero, id, new Vector3(1f, 1f, 0.74f));
        b.Box(2, new Vector3(0f, 0.16f, 0.2f), new Vector3(0.36f, 0.3f, 0.08f), id); // chaleco táctico
        b.Box(2, new Vector3(0f, 0.12f, -0.27f), new Vector3(0.38f, 0.42f, 0.16f), id); // mochila

        // Brazos
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 shoulder = new Vector3(0.36f * side, 0.36f, 0f);
            Vector3 elbow = new Vector3(0.38f * side, 0.08f, 0.08f);
            Vector3 hand = new Vector3(0.24f * side, -0.04f, 0.3f);
            b.Tube(0, new[] { shoulder, elbow, hand }, new[] { 0.1f, 0.085f, 0.075f }, 6);
            b.Ellipsoid(1, hand, new Vector3(0.07f, 0.07f, 0.07f), id, 3, 6);
        }

        // Cabeza, casco y visor
        b.Lathe(1, new[] { new Vector2(0f, 0.5f), new Vector2(0.08f, 0.52f), new Vector2(0.08f, 0.58f), new Vector2(0f, 0.58f) }, 6, Vector3.zero);
        b.Ellipsoid(1, new Vector3(0f, 0.7f, 0.01f), new Vector3(0.16f, 0.17f, 0.17f), id, 4, 8);
        b.Lathe(0, new[]
        {
            new Vector2(0.21f, 0.68f), new Vector2(0.2f, 0.76f), new Vector2(0.16f, 0.85f), new Vector2(0.08f, 0.9f), new Vector2(0f, 0.91f)
        }, 8, new Vector3(0f, 0f, -0.01f));
        b.Box(3, new Vector3(0f, 0.72f, 0.15f), new Vector3(0.26f, 0.07f, 0.06f), id);

        return b.Build("Guard_LowPoly");
    }

    // Submeshes del jugador: 0 = traje de infiltración, 1 = piel, 2 = equipo, 3 = bandana.
    public static Mesh Player()
    {
        var b = new ProceduralMeshBuilder();
        Quaternion id = Quaternion.identity;

        for (int side = -1; side <= 1; side += 2)
        {
            float x = 0.13f * side;
            b.Lathe(0, new[] { new Vector2(0f, -0.88f), new Vector2(0.1f, -0.88f), new Vector2(0.11f, -0.5f), new Vector2(0.13f, -0.18f), new Vector2(0f, -0.18f) }, 7, new Vector3(x, 0f, 0f));
            b.Box(2, new Vector3(x, -0.94f, 0.05f), new Vector3(0.18f, 0.12f, 0.32f), id);
            b.Box(2, new Vector3(x + 0.05f * side, -0.35f, 0.02f), new Vector3(0.06f, 0.14f, 0.12f), id); // bolsillo de pierna
        }

        b.Lathe(0, new[]
        {
            new Vector2(0f, -0.26f), new Vector2(0.22f, -0.26f), new Vector2(0.24f, -0.02f),
            new Vector2(0.29f, 0.24f), new Vector2(0.32f, 0.38f), new Vector2(0.2f, 0.49f), new Vector2(0f, 0.5f)
        }, 8, Vector3.zero, id, new Vector3(1f, 1f, 0.7f));
        b.Lathe(2, new[] { new Vector2(0.23f, -0.22f), new Vector2(0.245f, -0.2f), new Vector2(0.245f, -0.1f), new Vector2(0.23f, -0.08f) }, 8, Vector3.zero, id, new Vector3(1f, 1f, 0.72f));
        b.Box(2, new Vector3(0.12f, 0.2f, 0.19f), new Vector3(0.12f, 0.16f, 0.07f), id);
        b.Box(2, new Vector3(-0.12f, 0.2f, 0.19f), new Vector3(0.12f, 0.16f, 0.07f), id);
        b.Box(2, new Vector3(0f, -0.17f, -0.2f), new Vector3(0.22f, 0.12f, 0.08f), id);

        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 shoulder = new Vector3(0.32f * side, 0.36f, 0f);
            Vector3 elbow = new Vector3(0.36f * side, 0.06f, -0.02f);
            Vector3 hand = new Vector3(0.3f * side, -0.2f, 0.06f);
            b.Tube(0, new[] { shoulder, elbow, hand }, new[] { 0.09f, 0.08f, 0.07f }, 6);
            b.Ellipsoid(1, hand, new Vector3(0.065f, 0.065f, 0.065f), id, 3, 6);
        }

        b.Lathe(1, new[] { new Vector2(0f, 0.48f), new Vector2(0.075f, 0.5f), new Vector2(0.075f, 0.57f), new Vector2(0f, 0.57f) }, 6, Vector3.zero);
        b.Ellipsoid(1, new Vector3(0f, 0.7f, 0.01f), new Vector3(0.15f, 0.17f, 0.16f), id, 4, 8);
        b.Ellipsoid(2, new Vector3(0f, 0.77f, -0.02f), new Vector3(0.155f, 0.12f, 0.16f), id, 3, 8); // pelo
        b.Lathe(3, new[] { new Vector2(0.158f, 0.72f), new Vector2(0.162f, 0.73f), new Vector2(0.162f, 0.78f), new Vector2(0.158f, 0.79f) }, 8, new Vector3(0f, 0f, 0.01f));

        // Puntas de la bandana flameando hacia atrás
        b.Tube(3, new[] { new Vector3(0.03f, 0.75f, -0.15f), new Vector3(0.08f, 0.68f, -0.32f), new Vector3(0.12f, 0.58f, -0.46f) }, new[] { 0.025f, 0.022f, 0.012f }, 4);
        b.Tube(3, new[] { new Vector3(-0.03f, 0.75f, -0.15f), new Vector3(-0.05f, 0.66f, -0.3f), new Vector3(-0.04f, 0.55f, -0.42f) }, new[] { 0.025f, 0.022f, 0.012f }, 4);

        return b.Build("Player_LowPoly");
    }

    // Submeshes del ratón: 0 = pelaje, 1 = rosa (orejas, nariz, cola, patas), 2 = ojos.
    public static Mesh Mouse()
    {
        var b = new ProceduralMeshBuilder();
        Quaternion id = Quaternion.identity;
        Quaternion alongZ = Quaternion.Euler(90f, 0f, 0f);

        b.Ellipsoid(0, new Vector3(0f, -0.48f, -0.1f), new Vector3(0.52f, 0.95f, 0.46f), alongZ, 5, 9);
        b.Lathe(0, new[] { new Vector2(0f, 0f), new Vector2(0.36f, 0.05f), new Vector2(0.3f, 0.35f), new Vector2(0.16f, 0.62f), new Vector2(0f, 0.78f) }, 8,
            new Vector3(0f, -0.38f, 0.72f), alongZ, new Vector3(1f, 1f, 0.9f));
        b.Ellipsoid(1, new Vector3(0f, -0.36f, 1.5f), new Vector3(0.09f, 0.08f, 0.08f), id, 3, 6);

        for (int side = -1; side <= 1; side += 2)
        {
            b.Ellipsoid(1, new Vector3(0.27f * side, 0.02f, 0.82f), new Vector3(0.21f, 0.23f, 0.05f), Quaternion.Euler(0f, -20f * side, 0f), 4, 8);
            b.Ellipsoid(2, new Vector3(0.17f * side, -0.18f, 1.14f), new Vector3(0.06f, 0.07f, 0.05f), id, 3, 6);
            b.Ellipsoid(1, new Vector3(0.3f * side, -0.92f, 0.5f), new Vector3(0.12f, 0.07f, 0.18f), id, 3, 6);
            b.Ellipsoid(1, new Vector3(0.3f * side, -0.92f, -0.65f), new Vector3(0.12f, 0.07f, 0.2f), id, 3, 6);
        }

        b.Tube(1, new[]
        {
            new Vector3(0f, -0.62f, -0.98f), new Vector3(0.12f, -0.78f, -1.35f), new Vector3(-0.08f, -0.86f, -1.75f),
            new Vector3(0.1f, -0.88f, -2.15f), new Vector3(0.02f, -0.84f, -2.45f)
        }, new[] { 0.09f, 0.07f, 0.05f, 0.035f, 0.015f }, 5);

        return b.Build("Mouse_LowPoly");
    }

    // Submeshes del rifle: 0 = metal oscuro, 1 = culata / guardamanos. Largo ~1 unidad, cañón hacia +Z.
    public static Mesh Rifle()
    {
        var b = new ProceduralMeshBuilder();
        Quaternion sideways = Quaternion.Euler(0f, -90f, 0f); // el perfil XY se dibuja con X = adelante

        Vector2[] receiver =
        {
            new Vector2(-0.18f, 0.02f), new Vector2(0.22f, 0.02f), new Vector2(0.22f, 0.1f), new Vector2(0.16f, 0.13f),
            new Vector2(-0.12f, 0.13f), new Vector2(-0.18f, 0.09f)
        };
        Vector2[] stock =
        {
            new Vector2(-0.5f, -0.06f), new Vector2(-0.18f, 0.03f), new Vector2(-0.18f, 0.1f), new Vector2(-0.5f, 0.1f)
        };
        Vector2[] grip =
        {
            new Vector2(-0.1f, 0.03f), new Vector2(-0.03f, 0.03f), new Vector2(-0.07f, -0.12f), new Vector2(-0.14f, -0.12f)
        };
        Vector2[] magazine =
        {
            new Vector2(0.03f, 0.03f), new Vector2(0.11f, 0.03f), new Vector2(0.14f, -0.15f), new Vector2(0.07f, -0.16f)
        };
        Vector2[] handguard =
        {
            new Vector2(0.22f, 0.03f), new Vector2(0.44f, 0.03f), new Vector2(0.44f, 0.1f), new Vector2(0.22f, 0.11f)
        };

        b.Extrude(0, receiver, 0.07f, Vector3.zero, sideways);
        b.Extrude(1, stock, 0.06f, Vector3.zero, sideways);
        b.Extrude(1, grip, 0.05f, Vector3.zero, sideways);
        b.Extrude(0, magazine, 0.045f, Vector3.zero, sideways);
        b.Extrude(1, handguard, 0.075f, Vector3.zero, sideways);
        b.Tube(0, new[] { new Vector3(0f, 0.07f, 0.44f), new Vector3(0f, 0.07f, 0.62f) }, new[] { 0.022f }, 6);
        b.Box(0, new Vector3(0f, 0.16f, 0.02f), new Vector3(0.03f, 0.04f, 0.16f), Quaternion.identity); // mira

        return b.Build("Rifle_LowPoly");
    }

    // Llave: aro (toro) + vástago + dientes. Un solo submesh para usar el material de color de cada llave.
    public static Mesh Key()
    {
        var b = new ProceduralMeshBuilder();
        Quaternion id = Quaternion.identity;
        b.Torus(0, new Vector3(0f, 0.22f, 0f), id, 0.16f, 0.05f, 12, 6);
        b.Box(0, new Vector3(0f, -0.14f, 0f), new Vector3(0.07f, 0.56f, 0.05f), id);
        b.Box(0, new Vector3(0.07f, -0.34f, 0f), new Vector3(0.1f, 0.06f, 0.05f), id);
        b.Box(0, new Vector3(0.06f, -0.22f, 0f), new Vector3(0.08f, 0.05f, 0.05f), id);
        return b.Build("Key_LowPoly");
    }

    // Armero: base hexagonal con columna y anillo luminoso. 0 = metal, 1 = luz.
    public static Mesh WeaponPedestal()
    {
        var b = new ProceduralMeshBuilder();
        b.Lathe(0, new[]
        {
            new Vector2(0f, 0f), new Vector2(0.6f, 0f), new Vector2(0.6f, 0.08f), new Vector2(0.48f, 0.14f),
            new Vector2(0.3f, 0.18f), new Vector2(0.24f, 0.55f), new Vector2(0.34f, 0.62f), new Vector2(0f, 0.64f)
        }, 6, Vector3.zero);
        b.Lathe(1, new[] { new Vector2(0.5f, 0.1f), new Vector2(0.53f, 0.12f), new Vector2(0.5f, 0.15f), new Vector2(0.45f, 0.14f) }, 6, Vector3.zero);
        return b.Build("WeaponPedestal_LowPoly");
    }

    // Fogonazo de disparo en forma de estrella (efecto breve al atacar).
    public static Mesh MuzzleFlash()
    {
        var b = new ProceduralMeshBuilder();
        const int points = 6;
        Vector2[] star = new Vector2[points * 2];
        for (int i = 0; i < star.Length; i++)
        {
            float angle = i * Mathf.PI / points;
            float radius = (i % 2 == 0) ? 0.32f : 0.12f;
            star[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        b.Extrude(0, star, 0.04f, Vector3.zero, Quaternion.identity);
        b.Lathe(0, new[] { new Vector2(0f, 0f), new Vector2(0.1f, 0.05f), new Vector2(0f, 0.45f) }, 6, Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
        return b.Build("MuzzleFlash_LowPoly");
    }

    // Flecha / chevron plano que indica el frente del NPC.
    public static Mesh Chevron(float length = 0.6f, float width = 0.55f, float thickness = 0.16f)
    {
        var b = new ProceduralMeshBuilder();
        float hw = width * 0.5f;
        Vector2[] shape =
        {
            new Vector2(0f, length), new Vector2(-hw, 0f), new Vector2(-hw + thickness, 0f),
            new Vector2(0f, length - thickness * 1.6f), new Vector2(hw - thickness, 0f), new Vector2(hw, 0f)
        };
        // El polígono está en XY: lo acostamos sobre el piso (XZ) con Y local como adelante.
        b.Extrude(0, shape, 0.03f, Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
        return b.Build("DirectionChevron");
    }
}
