using System.Collections.Generic;
using Assignment3;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

/// <summary>
/// The "Assignments" menu. Builds the starting scene for Assignment 3: a VR rig with the
/// unfinished Flying script already attached and wired, and a forest to fly over.
///
///   Assignments > Build Assignment 3 - Flying     Scenes/Assignment3_Flying.unity
///
/// The rig is the plain XR Origin from GameObject > XR > XR Origin (VR), plus two tracked
/// controllers. No locomotion, no interactors: the student's script is the only thing that moves it.
/// </summary>
public static class Assignment3SceneBuilder
{
    const string ScenePath   = "Assets/Scenes/Assignment3_Flying.unity";
    const string MaterialDir = "Assets/Materials";
    const string Forest      = "Assets/KayKit/Forest";
    const string Simulator   = "XR Interaction Simulator";

    [MenuItem("Assignments/Build Assignment 3 - Flying")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var light = Object.FindAnyObjectByType<Light>();
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.color = new Color(1f, 0.96f, 0.88f);
        light.intensity = 1.6f;
        light.shadows = LightShadows.Soft;

        var origin = BuildRig(Camera.main);
        BuildHeadsetOrSimulator();
        BuildWorld();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = origin;
        Debug.Log("[Assignments] Built " + ScenePath + ". Open Scripts/Assignment3/Flying.cs and fill in Update().");
    }

    // ------------------------------------------------------------------ rig

    static GameObject BuildRig(Camera cam)
    {
        var origin = new GameObject("XR Origin (XR Rig)");
        var xrOrigin = origin.AddComponent<XROrigin>();

        var offset = new GameObject("Camera Offset").transform;
        offset.SetParent(origin.transform, false);

        cam.transform.SetParent(offset, false);
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
        cam.nearClipPlane = 0.01f;
        Track(cam.gameObject, "<XRHMD>", "centerEyePosition", "centerEyeRotation");

        xrOrigin.Camera = cam;
        xrOrigin.CameraFloorOffsetObject = offset.gameObject;
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        xrOrigin.CameraYOffset = 1.36144f;

        // A headset in Floor mode resets this to 0 on Play. The simulator has no floor, so it keeps it as eye height.
        offset.localPosition = new Vector3(0f, xrOrigin.CameraYOffset, 0f);

        var left  = Hand("Left Controller",  "{LeftHand}",  offset, Mat("Flying_LeftHand",  new Color(0.2f, 0.45f, 0.95f)));
        var right = Hand("Right Controller", "{RightHand}", offset, Mat("Flying_RightHand", new Color(0.9f, 0.25f, 0.2f)));

        var flying = origin.AddComponent<Flying>();
        flying.leftController  = left;
        flying.rightController = right;
        flying.cameraPos       = cam.transform;

        return origin;
    }

    static Transform Hand(string name, string usage, Transform parent, Material mat)
    {
        var hand = new GameObject(name);
        hand.transform.SetParent(parent, false);

        // Only where they sit in the Scene view before Play. Tracking overwrites this every frame.
        hand.transform.localPosition = new Vector3(usage == "{LeftHand}" ? -0.2f : 0.2f, -0.3f, 0.3f);
        Track(hand, "<XRController>" + usage, "devicePosition", "deviceRotation");

        var visual = Primitive(PrimitiveType.Cube, "Visual", hand.transform, mat);
        visual.transform.localScale = new Vector3(0.05f, 0.04f, 0.13f);
        visual.isStatic = false;

        return hand.transform;
    }

    // Same bindings XRI's own "XR Origin (VR)" menu item writes for the camera.
    static void Track(GameObject go, string device, string position, string rotation)
    {
        var driver = go.AddComponent<TrackedPoseDriver>();
        driver.positionInput      = new InputActionProperty(new InputAction("Position",       binding: device + "/" + position,   expectedControlType: "Vector3"));
        driver.rotationInput      = new InputActionProperty(new InputAction("Rotation",       binding: device + "/" + rotation,   expectedControlType: "Quaternion"));
        driver.trackingStateInput = new InputActionProperty(new InputAction("Tracking State", binding: device + "/trackingState", expectedControlType: "Integer"));
    }

    static void BuildHeadsetOrSimulator()
    {
        var switcher = new GameObject("Headset Or Simulator").AddComponent<HeadsetOrSimulator>();

        foreach (var guid in AssetDatabase.FindAssets("\"" + Simulator + "\" t:Prefab"))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null || prefab.name != Simulator) continue;

            var sim = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            sim.SetActive(false);   // HeadsetOrSimulator switches it on when there is no headset
            switcher.simulator = sim;
            return;
        }

        Debug.LogWarning("[Assignments] '" + Simulator + "' prefab not found. Import it from Package Manager > XR Interaction Toolkit > Samples, then build again. The scene will only work with a headset until then.");
    }

    // ---------------------------------------------------------------- world

    static void BuildWorld()
    {
        KeepClear.Clear();
        var world = new GameObject("World").transform;
        var white = Mat("Flying_White", new Color(0.93f, 0.93f, 0.9f));

        var ground = Primitive(PrimitiveType.Plane, "Ground", world, Mat("Flying_Ground", new Color(0.36f, 0.55f, 0.27f)), keepCollider: true);
        ground.transform.localScale = new Vector3(100f, 1f, 100f);   // 1 km x 1 km, so the edge stays out of sight

        var pad = Primitive(PrimitiveType.Cylinder, "Launch Pad", world, white);
        pad.transform.localScale = new Vector3(3f, 0.02f, 3f);

        // Three hoops to fly through: ahead and up, then right and higher, then left and back down.
        Hoop(world, "Hoop 1", new Vector3(  0f,  6f, 16f), new Vector3( 0f, 1.4f,  0f), Mat("Flying_Hoop1", new Color(1f, 0.8f, 0.1f)));
        Hoop(world, "Hoop 2", new Vector3( 14f, 14f, 32f), new Vector3( 0f,   6f, 16f), Mat("Flying_Hoop2", new Color(1f, 0.45f, 0.1f)));
        Hoop(world, "Hoop 3", new Vector3(-12f,  9f, 50f), new Vector3(14f,  14f, 32f), Mat("Flying_Hoop3", new Color(0.85f, 0.2f, 0.75f)));

        // Striped towers: each band is 5 m, so you can read your height off them.
        var red = Mat("Flying_Red", new Color(0.85f, 0.2f, 0.15f));
        Tower(world, "Height Tower L (5 m per band)", new Vector3(-18f, 0f, 22f), red, white);
        Tower(world, "Height Tower R (5 m per band)", new Vector3( 24f, 0f, 44f), red, white);

        var rng = new System.Random(3);

        var clouds = new GameObject("Clouds").transform;
        clouds.SetParent(world, false);
        for (int i = 0; i < 28; i++)
        {
            var cloud = Primitive(PrimitiveType.Sphere, "Cloud", clouds, white);
            cloud.transform.position = OnRing(rng, 25f, 170f, Rand(rng, 35f, 80f));
            cloud.transform.localScale = new Vector3(Rand(rng, 8f, 16f), Rand(rng, 2f, 4f), Rand(rng, 6f, 12f));
        }

        var trees  = Load("Tree_1_A", "Tree_1_B", "Tree_2_A", "Tree_2_C", "Tree_3_A", "Tree_4_B");
        var bushes = Load("Bush_1_A", "Bush_1_C", "Bush_2_B", "Bush_3_A", "Bush_4_A");
        var rocks  = Load("Rock_1_A", "Rock_1_D", "Rock_2_B", "Rock_3_C", "Rock_3_G");

        // Keep the launch pad clear so the first thing the player sees is open sky and the first hoop.
        Scatter(world, rng, trees,  260, 12f, 190f, 1.2f, 2.2f, "Trees");
        Scatter(world, rng, bushes,  90,  4f, 120f, 1.0f, 2.0f, "Bushes");
        Scatter(world, rng, rocks,   70,  5f, 150f, 1.0f, 4.0f, "Rocks");
    }

    static void Hoop(Transform parent, string name, Vector3 pos, Vector3 seenFrom, Material mat)
    {
        const int   segments = 24;
        const float radius = 2.5f, thickness = 0.25f;

        var hoop = new GameObject(name).transform;
        hoop.SetParent(parent, false);
        hoop.position = pos;
        KeepClear.Add(pos);
        hoop.rotation = Quaternion.LookRotation(pos - seenFrom);   // face the direction you arrive from

        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            var seg = Primitive(PrimitiveType.Cube, "Segment", hoop, mat);
            seg.transform.localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
            seg.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg);
            seg.transform.localScale = new Vector3(thickness, Mathf.PI * 2f * radius / segments * 1.1f, thickness);
        }
    }

    static void Tower(Transform parent, string name, Vector3 pos, Material a, Material b)
    {
        var tower = new GameObject(name).transform;
        tower.SetParent(parent, false);
        tower.position = pos;
        KeepClear.Add(pos);

        for (int i = 0; i < 8; i++)
        {
            var band = Primitive(PrimitiveType.Cube, (i * 5) + " to " + (i * 5 + 5) + " m", tower, i % 2 == 0 ? a : b);
            band.transform.localPosition = new Vector3(0f, 2.5f + i * 5f, 0f);
            band.transform.localScale = new Vector3(1.5f, 5f, 1.5f);
        }
    }

    static void Scatter(Transform parent, System.Random rng, GameObject[] set, int count,
                        float minR, float maxR, float minScale, float maxScale, string group)
    {
        if (set.Length == 0) return;

        var g = new GameObject(group).transform;
        g.SetParent(parent, false);

        for (int i = 0; i < count; i++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(set[rng.Next(set.Length)]);
            go.transform.SetParent(g);
            go.transform.position = ClearSpot(rng, minR, maxR);
            go.transform.rotation = Quaternion.Euler(0f, Rand(rng, 0f, 360f), 0f);
            go.transform.localScale = Vector3.one * Rand(rng, minScale, maxScale);
            go.isStatic = true;
        }
    }

    // ------------------------------------------------------------- helpers

    static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material mat, bool keepCollider = false)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // A random point between two radii, so things spread evenly over the area instead of bunching at the centre.
    static Vector3 OnRing(System.Random rng, float minR, float maxR, float y)
    {
        float angle = Rand(rng, 0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(Rand(rng, minR * minR, maxR * maxR));
        return new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r);
    }

    // Hoops and towers register here so nothing gets planted underneath or inside them.
    static readonly List<Vector3> KeepClear = new List<Vector3>();

    static Vector3 ClearSpot(System.Random rng, float minR, float maxR)
    {
        for (int tries = 0; ; tries++)
        {
            var p = OnRing(rng, minR, maxR, 0f);
            bool blocked = false;
            foreach (var c in KeepClear)
                if (new Vector2(p.x - c.x, p.z - c.z).magnitude < 6f) blocked = true;
            if (!blocked || tries > 20) return p;
        }
    }

    static float Rand(System.Random rng, float a, float b) => (float)(a + rng.NextDouble() * (b - a));

    static GameObject[] Load(params string[] stems)
    {
        var list = new List<GameObject>();
        foreach (var stem in stems)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Forest + "/" + stem + "_Color1.fbx");
            if (go != null) list.Add(go);
            else Debug.LogWarning("[Assignments] Foliage missing: " + stem);
        }
        return list.ToArray();
    }

    static Material Mat(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MaterialDir)) AssetDatabase.CreateFolder("Assets", "Materials");
        var path = MaterialDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
