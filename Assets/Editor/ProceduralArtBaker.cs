#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Herramienta de editor: genera los modelos low-poly (ProceduralModelLibrary) como assets,
// crea sus materiales y los aplica a prefabs y a la escena abierta reemplazando cubos y cápsulas.
// También agrega los componentes de feedback visual (cono de visión, flecha de dirección y HUD flotante) a los NPCs.
public static class ProceduralArtBaker
{
    private const string MeshFolder = "Assets/Art/Meshes";
    private const string MaterialFolder = "Assets/Art/Materials";

    private const string GuardPrefab = "Assets/Prefabs/NPCS/Guardian.prefab";
    private const string MousePrefab = "Assets/Prefabs/NPCS/Mouse.prefab";
    private const string PlayerPrefab = "Assets/Prefabs/PlayerThings/Player.prefab";
    private const string WeaponStationPrefab = "Assets/Prefabs/WeaponStation.prefab";

    private class ArtSet
    {
        public Mesh Guard, Player, Mouse, Rifle, Key, Pedestal, Flash;
        public Material[] GuardMats, PlayerMats, MouseMats, LeaderMouseMats, RifleMats, PedestalMats;
        public Material FlashMat, FeedbackMat;
    }

    [MenuItem("MetAIGearSolid/Arte procedural/Generar y aplicar (prefabs + escena)")]
    public static void BakeAndApplyAll()
    {
        ArtSet art = Bake();
        ApplyToPrefabs(art);
        ApplyToOpenScene(art);
        AssetDatabase.SaveAssets();
        Debug.Log("[ProceduralArtBaker] Modelos procedurales generados y aplicados a prefabs y escena.");
    }

    private static ArtSet Bake()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder(MeshFolder);
        EnsureFolder(MaterialFolder);

        var art = new ArtSet
        {
            Guard = SaveMesh(ProceduralModelLibrary.Guard()),
            Player = SaveMesh(ProceduralModelLibrary.Player()),
            Mouse = SaveMesh(ProceduralModelLibrary.Mouse()),
            Rifle = SaveMesh(ProceduralModelLibrary.Rifle()),
            Key = SaveMesh(ProceduralModelLibrary.Key()),
            Pedestal = SaveMesh(ProceduralModelLibrary.WeaponPedestal()),
            Flash = SaveMesh(ProceduralModelLibrary.MuzzleFlash())
        };

        Material skin = LitMaterial("M_Skin", new Color(0.86f, 0.66f, 0.52f), 0.2f);
        Material darkGear = LitMaterial("M_DarkGear", new Color(0.13f, 0.14f, 0.15f), 0.35f);

        art.GuardMats = new[]
        {
            LitMaterial("M_GuardUniform", new Color(0.36f, 0.42f, 0.34f), 0.15f),
            skin,
            darkGear,
            LitMaterial("M_GuardVisor", new Color(0.9f, 0.15f, 0.1f), 0.8f, new Color(1.2f, 0.15f, 0.05f))
        };

        art.PlayerMats = new[]
        {
            LitMaterial("M_SneakingSuit", new Color(0.16f, 0.2f, 0.3f), 0.45f),
            skin,
            LitMaterial("M_PlayerGear", new Color(0.32f, 0.25f, 0.17f), 0.2f),
            LitMaterial("M_Bandana", new Color(0.08f, 0.08f, 0.1f), 0.1f)
        };

        Material pink = LitMaterial("M_MousePink", new Color(0.95f, 0.6f, 0.65f), 0.3f);
        Material eyes = LitMaterial("M_MouseEyes", new Color(0.02f, 0.02f, 0.02f), 0.9f);
        art.MouseMats = new[] { LitMaterial("M_MouseFur", new Color(0.55f, 0.55f, 0.58f), 0.1f), pink, eyes };
        art.LeaderMouseMats = new[] { LitMaterial("M_MouseFurLeader", new Color(0.45f, 0.3f, 0.18f), 0.1f), pink, eyes };

        art.RifleMats = new[]
        {
            LitMaterial("M_GunMetal", new Color(0.18f, 0.19f, 0.2f), 0.6f, default, 0.7f),
            LitMaterial("M_GunPolymer", new Color(0.3f, 0.24f, 0.16f), 0.25f)
        };

        art.PedestalMats = new[]
        {
            LitMaterial("M_PedestalMetal", new Color(0.32f, 0.34f, 0.37f), 0.5f, default, 0.5f),
            LitMaterial("M_PedestalLight", new Color(0.2f, 0.9f, 1f), 0.6f, new Color(0.3f, 1.6f, 2f))
        };

        art.FlashMat = LitMaterial("M_MuzzleFlash", new Color(1f, 0.85f, 0.3f), 0.2f, new Color(4f, 2.6f, 0.6f));
        art.FeedbackMat = FeedbackMaterial();
        return art;
    }

    // ---------- Aplicación ----------

    private static void ApplyToPrefabs(ArtSet art)
    {
        EditPrefab(GuardPrefab, root => ApplyGuard(root, art));
        EditPrefab(MousePrefab, root => ApplyMouse(root, art, false));
        EditPrefab(PlayerPrefab, root => ApplyPlayer(root, art));
        EditPrefab(WeaponStationPrefab, root => ApplyWeaponStation(root, art));
    }

    private static void ApplyToOpenScene(ArtSet art)
    {
        foreach (var guard in Object.FindObjectsByType<EnemyDecisionTreeController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ApplyGuard(guard.gameObject, art);
        }

        foreach (var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (enemy is EnemyDecisionTreeController) continue;
            bool isLeader = enemy is FleeingEnemyController;
            ApplyMouse(enemy.gameObject, art, isLeader);
        }

        foreach (var player in Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ApplyPlayer(player.gameObject, art);
        }

        foreach (var station in Object.FindObjectsByType<WeaponPickupStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ApplyWeaponStation(station.gameObject, art);
        }

        foreach (var key in Object.FindObjectsByType<KeyPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ApplyKey(key.gameObject, art);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void ApplyGuard(GameObject root, ArtSet art)
    {
        GameObject body = FindBody<EnemyDecisionTreeController>(root);
        if (body == null) return;

        SetMesh(body, art.Guard, art.GuardMats);

        // Rifle visible en mano mientras HasWeapon == true (EnemyWeapon.equippedHolsterVisual).
        Transform held = FindOrCreateChild(body.transform, "HeldRifle");
        held.localPosition = new Vector3(0.24f, -0.02f, 0.32f);
        held.localRotation = Quaternion.identity;
        held.localScale = Vector3.one * 0.7f;
        SetMesh(held.gameObject, art.Rifle, art.RifleMats);

        var weapon = body.GetComponent<EnemyWeapon>();
        if (weapon != null)
        {
            var so = new SerializedObject(weapon);
            so.FindProperty("equippedHolsterVisual").objectReferenceValue = held.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // El objeto "Gun" es el efecto de ataque (AttackVisualEffect + DamageAura): pasa a ser un fogonazo en la boca del cañón.
        Transform flash = body.transform.Find("Gun");
        if (flash != null)
        {
            flash.localPosition = new Vector3(0.24f, 0.03f, 0.78f);
            flash.localRotation = Quaternion.identity;
            flash.localScale = Vector3.one * 0.9f;
            SetMesh(flash.gameObject, art.Flash, new[] { art.FlashMat });
        }

        AddFeedback(body, art, true);
    }

    private static void ApplyMouse(GameObject root, ArtSet art, bool isLeader)
    {
        GameObject body = FindBody<EnemyController>(root);
        if (body == null) return;

        SetMesh(body, art.Mouse, isLeader ? art.LeaderMouseMats : art.MouseMats);
        AddFeedback(body, art, false);
    }

    private static void ApplyPlayer(GameObject root, ArtSet art)
    {
        GameObject body = FindBody<PlayerMovement>(root);
        if (body == null) return;
        SetMesh(body, art.Player, art.PlayerMats);
    }

    private static void ApplyWeaponStation(GameObject root, ArtSet art)
    {
        Transform pedestal = root.transform.Find("Pedestal");
        if (pedestal != null)
        {
            pedestal.localPosition = new Vector3(0f, -0.5f, 0f);
            pedestal.localScale = Vector3.one;
            SetMesh(pedestal.gameObject, art.Pedestal, art.PedestalMats);
        }

        Transform floating = root.transform.Find("FloatingWeaponVisual");
        if (floating == null) return;

        foreach (string oldPart in new[] { "RifleBody", "RifleBarrel", "RifleGrip" })
        {
            Transform part = floating.Find(oldPart);
            if (part != null) Object.DestroyImmediate(part.gameObject, true);
        }

        Transform rifle = FindOrCreateChild(floating, "RifleModel");
        rifle.localPosition = Vector3.zero;
        rifle.localRotation = Quaternion.Euler(0f, 0f, 0f);
        rifle.localScale = Vector3.one * 1.1f;
        SetMesh(rifle.gameObject, art.Rifle, art.RifleMats);
    }

    private static void ApplyKey(GameObject key, ArtSet art)
    {
        var renderer = key.GetComponent<MeshRenderer>();
        Material keyMaterial = renderer != null ? renderer.sharedMaterial : null;
        SetMesh(key, art.Key, keyMaterial != null ? new[] { keyMaterial } : null);
    }

    private static void AddFeedback(GameObject body, ArtSet art, bool showWeaponLine)
    {
        var cone = GetOrAdd<VisionConeMeshRenderer>(body);
        SetObject(cone, "coneMaterial", art.FeedbackMat);

        var arrow = GetOrAdd<NpcDirectionIndicator>(body);
        SetObject(arrow, "indicatorMaterial", art.FeedbackMat);

        var view = GetOrAdd<NpcStatusView>(body);
        var so = new SerializedObject(view);
        so.FindProperty("showWeaponLine").boolValue = showWeaponLine;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- Utilidades ----------

    private static GameObject FindBody<T>(GameObject root) where T : Component
    {
        T component = root.GetComponentInChildren<T>(true);
        return component != null ? component.gameObject : null;
    }

    private static void SetMesh(GameObject go, Mesh mesh, Material[] materials)
    {
        var filter = go.GetComponent<MeshFilter>();
        if (filter == null) filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) renderer = go.AddComponent<MeshRenderer>();
        if (materials != null) renderer.sharedMaterials = materials;

        EditorUtility.SetDirty(go);
    }

    private static Transform FindOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) return child;

        var go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void SetObject(Object target, string property, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(property).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EditPrefab(string path, System.Action<GameObject> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning($"[ProceduralArtBaker] No se encontró el prefab {path}.");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Mesh SaveMesh(Mesh mesh)
    {
        string path = $"{MeshFolder}/{mesh.name}.asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        // Mantiene el GUID para no romper referencias existentes.
        EditorUtility.CopySerialized(mesh, existing);
        existing.name = mesh.name;
        EditorUtility.SetDirty(existing);
        Object.DestroyImmediate(mesh);
        return existing;
    }

    private static Material LitMaterial(string materialName, Color color, float smoothness, Color emission = default, float metallic = 0f)
    {
        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", metallic);

        if (emission.maxColorComponent > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material FeedbackMaterial()
    {
        string path = $"{MaterialFolder}/M_FeedbackVertexColor.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, path);
        }

        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
