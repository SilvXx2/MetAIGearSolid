using System.Collections.Generic;
using UnityEngine;

// Construye mallas low-poly a partir de operaciones geométricas (torno, extrusión, tubo, elipsoide, caja).
// Cada pieza se asigna a un submesh para poder usar varios materiales por modelo, sin depender de primitivas de Unity.
public class ProceduralMeshBuilder
{
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<List<int>> submeshes = new List<List<int>>();

    private List<int> Tris(int submesh)
    {
        while (submeshes.Count <= submesh) submeshes.Add(new List<int>());
        return submeshes[submesh];
    }

    private void AddTriangle(int submesh, Vector3 a, Vector3 b, Vector3 c)
    {
        // Vértices sin compartir: sombreado plano (flat shading) de estilo low-poly.
        List<int> tris = Tris(submesh);
        int index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);
    }

    private void AddQuad(int submesh, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        AddTriangle(submesh, a, b, c);
        AddTriangle(submesh, a, c, d);
    }

    // Torno: revoluciona un perfil (x = radio, y = altura) alrededor del eje Y local de la pieza.
    public ProceduralMeshBuilder Lathe(int submesh, Vector2[] profile, int segments, Vector3 center, Quaternion rotation, Vector3 scale)
    {
        segments = Mathf.Max(3, segments);
        for (int p = 0; p < profile.Length - 1; p++)
        {
            for (int s = 0; s < segments; s++)
            {
                float a0 = (s / (float)segments) * Mathf.PI * 2f;
                float a1 = ((s + 1) / (float)segments) * Mathf.PI * 2f;
                Vector3 v00 = LathePoint(profile[p], a0, center, rotation, scale);
                Vector3 v01 = LathePoint(profile[p], a1, center, rotation, scale);
                Vector3 v10 = LathePoint(profile[p + 1], a0, center, rotation, scale);
                Vector3 v11 = LathePoint(profile[p + 1], a1, center, rotation, scale);

                if (profile[p].x <= 0.0001f)
                {
                    AddTriangle(submesh, v00, v11, v10);
                }
                else if (profile[p + 1].x <= 0.0001f)
                {
                    AddTriangle(submesh, v00, v01, v10);
                }
                else
                {
                    AddQuad(submesh, v00, v01, v11, v10);
                }
            }
        }

        return this;
    }

    private static Vector3 LathePoint(Vector2 profilePoint, float angle, Vector3 center, Quaternion rotation, Vector3 scale)
    {
        Vector3 local = new Vector3(Mathf.Sin(angle) * profilePoint.x, profilePoint.y, Mathf.Cos(angle) * profilePoint.x);
        local = Vector3.Scale(local, scale);
        return center + rotation * local;
    }

    public ProceduralMeshBuilder Lathe(int submesh, Vector2[] profile, int segments, Vector3 center)
    {
        return Lathe(submesh, profile, segments, center, Quaternion.identity, Vector3.one);
    }

    // Elipsoide low-poly construido con un torno de media circunferencia.
    public ProceduralMeshBuilder Ellipsoid(int submesh, Vector3 center, Vector3 radii, Quaternion rotation, int rings = 5, int segments = 8)
    {
        rings = Mathf.Max(2, rings);
        Vector2[] profile = new Vector2[rings + 1];
        for (int i = 0; i <= rings; i++)
        {
            float t = Mathf.PI * i / rings;
            profile[i] = new Vector2(Mathf.Sin(t), -Mathf.Cos(t));
        }

        return Lathe(submesh, profile, segments, center, rotation, radii);
    }

    // Prisma rectangular propio (8 vértices, 6 caras) para piezas duras como botas, mochila o visor.
    public ProceduralMeshBuilder Box(int submesh, Vector3 center, Vector3 size, Quaternion rotation)
    {
        Vector3 h = size * 0.5f;
        Vector3[] c = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 local = new Vector3((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y, (i & 4) == 0 ? -h.z : h.z);
            c[i] = center + rotation * local;
        }

        AddQuad(submesh, c[0], c[2], c[3], c[1]); // -Z
        AddQuad(submesh, c[4], c[5], c[7], c[6]); // +Z
        AddQuad(submesh, c[0], c[4], c[6], c[2]); // -X
        AddQuad(submesh, c[1], c[3], c[7], c[5]); // +X
        AddQuad(submesh, c[2], c[6], c[7], c[3]); // +Y
        AddQuad(submesh, c[0], c[1], c[5], c[4]); // -Y
        return this;
    }

    // Extruye un polígono simple (sentido antihorario, plano XY local) con un espesor en Z.
    public ProceduralMeshBuilder Extrude(int submesh, Vector2[] polygon, float depth, Vector3 center, Quaternion rotation)
    {
        float hz = depth * 0.5f;
        List<int> triangles = Triangulate(polygon);

        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector2 a = polygon[triangles[i]];
            Vector2 b = polygon[triangles[i + 1]];
            Vector2 c = polygon[triangles[i + 2]];
            AddTriangle(submesh, P(a, hz, center, rotation), P(b, hz, center, rotation), P(c, hz, center, rotation));
            AddTriangle(submesh, P(a, -hz, center, rotation), P(c, -hz, center, rotation), P(b, -hz, center, rotation));
        }

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];
            AddQuad(submesh, P(a, -hz, center, rotation), P(b, -hz, center, rotation), P(b, hz, center, rotation), P(a, hz, center, rotation));
        }

        return this;
    }

    private static Vector3 P(Vector2 p, float z, Vector3 center, Quaternion rotation)
    {
        return center + rotation * new Vector3(p.x, p.y, z);
    }

    // Tubo que sigue una curva de puntos con radio variable (brazos, colas, anillos).
    public ProceduralMeshBuilder Tube(int submesh, Vector3[] path, float[] radii, int segments = 6, bool capEnds = true)
    {
        segments = Mathf.Max(3, segments);
        Vector3[][] rings = new Vector3[path.Length][];

        for (int i = 0; i < path.Length; i++)
        {
            Vector3 forward = i < path.Length - 1 ? path[i + 1] - path[i] : path[i] - path[i - 1];
            if (forward.sqrMagnitude < 0.000001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;
            float radius = radii[Mathf.Min(i, radii.Length - 1)];

            rings[i] = new Vector3[segments];
            for (int s = 0; s < segments; s++)
            {
                float angle = (s / (float)segments) * Mathf.PI * 2f;
                rings[i][s] = path[i] + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
            }
        }

        for (int i = 0; i < path.Length - 1; i++)
        {
            for (int s = 0; s < segments; s++)
            {
                int n = (s + 1) % segments;
                AddQuad(submesh, rings[i][s], rings[i][n], rings[i + 1][n], rings[i + 1][s]);
            }
        }

        if (capEnds)
        {
            for (int s = 0; s < segments; s++)
            {
                int n = (s + 1) % segments;
                AddTriangle(submesh, path[0], rings[0][n], rings[0][s]);
                AddTriangle(submesh, path[path.Length - 1], rings[path.Length - 1][s], rings[path.Length - 1][n]);
            }
        }

        return this;
    }

    // Anillo (toro) low-poly; útil para el aro de las llaves.
    public ProceduralMeshBuilder Torus(int submesh, Vector3 center, Quaternion rotation, float majorRadius, float minorRadius, int majorSegments = 10, int minorSegments = 6)
    {
        Vector3[] path = new Vector3[majorSegments + 1];
        float[] radii = { minorRadius };
        for (int i = 0; i <= majorSegments; i++)
        {
            float angle = (i / (float)majorSegments) * Mathf.PI * 2f;
            path[i] = center + rotation * new Vector3(Mathf.Cos(angle) * majorRadius, Mathf.Sin(angle) * majorRadius, 0f);
        }

        return Tube(submesh, path, radii, minorSegments, false);
    }

    public Mesh Build(string name)
    {
        Mesh mesh = new Mesh { name = name };
        if (vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.subMeshCount = Mathf.Max(1, submeshes.Count);
        for (int i = 0; i < submeshes.Count; i++)
        {
            mesh.SetTriangles(submeshes[i], i);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    // Triangulación por "ear clipping" para polígonos simples (sin agujeros).
    private static List<int> Triangulate(Vector2[] polygon)
    {
        List<int> result = new List<int>();
        List<int> indices = new List<int>();
        for (int i = 0; i < polygon.Length; i++) indices.Add(i);

        if (SignedArea(polygon) < 0f) indices.Reverse();

        int guard = 0;
        while (indices.Count > 3 && guard++ < 1000)
        {
            bool clipped = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prev = indices[(i - 1 + indices.Count) % indices.Count];
                int curr = indices[i];
                int next = indices[(i + 1) % indices.Count];
                Vector2 a = polygon[prev];
                Vector2 b = polygon[curr];
                Vector2 c = polygon[next];

                if (Cross(b - a, c - b) <= 0f) continue;

                bool containsPoint = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    int k = indices[j];
                    if (k == prev || k == curr || k == next) continue;
                    if (PointInTriangle(polygon[k], a, b, c))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (containsPoint) continue;

                result.Add(prev);
                result.Add(curr);
                result.Add(next);
                indices.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped) break;
        }

        if (indices.Count == 3)
        {
            result.Add(indices[0]);
            result.Add(indices[1]);
            result.Add(indices[2]);
        }

        return result;
    }

    private static float SignedArea(Vector2[] polygon)
    {
        float area = 0f;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];
            area += a.x * b.y - b.x * a.y;
        }

        return area * 0.5f;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(b - a, p - a);
        float d2 = Cross(c - b, p - b);
        float d3 = Cross(a - c, p - c);
        bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
        return !(hasNeg && hasPos);
    }
}
