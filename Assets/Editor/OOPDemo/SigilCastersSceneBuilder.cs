using Assignment3;
using SigilCasters;
using Sigils;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

/// <summary>
/// The Sigil Casters demo scene: a small escape room where the four building blocks are wired
/// together. Walk (StickWalk), draw a sigil (SigilDrawer -> SigilRecognizer), aim and cast
/// (SpellCaster), and watch a block react (SpellTarget + a Spell), which opens the door.
///
///   Sigil Lab > Build Sigil Casters Scene     Scenes/SigilCasters.unity
///
/// Same lean rig as Assignment 3 and the Raycast Lab, and the same HeadsetOrSimulator switch.
/// </summary>
public static class SigilCastersSceneBuilder
{
    const string ScenePath   = "Assets/Scenes/SigilCasters.unity";
    const string MaterialDir = "Assets/Materials";
    const string Simulator   = "XR Interaction Simulator";
    const string LeftHand    = "{LeftHand}";
    const string RightHand   = "{RightHand}";

    [MenuItem("Sigil Lab/Build Sigil Casters Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var light = Object.FindAnyObjectByType<Light>();
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.intensity = 1.4f;
        light.shadows = LightShadows.Soft;

        var recognizer = new GameObject("Sigil Recognizer").AddComponent<SigilRecognizer>();
        recognizer.confidenceThreshold = 0.75f;   // hand-drawn strokes in the air are wobblier than traced pictures

        var origin = BuildRig(Camera.main, out var right, out var left, out var head);
        BuildHeadsetOrSimulator();

        // Part 1: walking. On the rig, reads the left stick, needs to know where the head is.
        var walk = origin.AddComponent<StickWalk>();
        walk.head = head;
        walk.moveStick = Action("Move", "<XRController>" + LeftHand + "/{Primary2DAxis}", "Vector2");

        // Part 3: casting. On the right hand. Its LineRenderer is the aim line.
        var aim = right.gameObject.AddComponent<LineRenderer>();
        aim.sharedMaterial = LineMat();
        aim.startWidth = aim.endWidth = 0.008f;
        var caster = right.gameObject.AddComponent<SpellCaster>();
        caster.castButton = Action("Cast", "<XRController>" + RightHand + "/{GripButton}", "Button");

        // Part 2: drawing. Also on the right hand. The trail is a child so it gets its own LineRenderer.
        var trailGo = new GameObject("Sigil Trail");
        trailGo.transform.SetParent(right, false);
        var trail = trailGo.AddComponent<LineRenderer>();
        trail.sharedMaterial = LineMat();
        trail.startWidth = trail.endWidth = 0.015f;
        trail.startColor = trail.endColor = new Color(0.9f, 0.3f, 1f);
        trail.positionCount = 0;

        var drawer = right.gameObject.AddComponent<SigilDrawer>();
        drawer.head       = head;
        drawer.recognizer = recognizer;
        drawer.caster     = caster;
        drawer.trail      = trail;
        drawer.drawButton = Action("Draw", "<XRController>" + RightHand + "/{TriggerButton}", "Button");

        BuildRoom();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = right.gameObject;
        Debug.Log("[Sigil Lab] Built " + ScenePath + ". Trigger = draw, grip = cast, left stick = walk. Read Scripts/SigilCasters/README.md.");
    }

    // ---------------------------------------------------------------- room

    static void BuildRoom()
    {
        var room = new GameObject("Room").transform;

        var floorMat = Mat("SigilCasters_Floor", new Color(0.28f, 0.27f, 0.3f));
        var wallMat  = Mat("SigilCasters_Wall",  new Color(0.5f, 0.52f, 0.58f));

        // Floor: a 12 m square, player starts near the middle facing +z.
        var floor = Primitive(PrimitiveType.Plane, "Floor", room, floorMat, keepCollider: true);
        floor.transform.position   = new Vector3(0f, 0f, 2f);
        floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);

        // Walls: colliders, so the ray stops on them, but no SpellTarget, so spells fizzle on them.
        Wall(room, "Wall (left)",  new Vector3(-4f, 1.5f, 2f), new Vector3(0.2f, 3f, 12f), wallMat);
        Wall(room, "Wall (right)", new Vector3( 4f, 1.5f, 2f), new Vector3(0.2f, 3f, 12f), wallMat);
        Wall(room, "Wall (back)",  new Vector3( 0f, 1.5f, -4f), new Vector3(8f, 3f, 0.2f), wallMat);
        Wall(room, "Wall (front, left of door)",  new Vector3(-2.5f, 1.5f, 6f), new Vector3(3f, 3f, 0.2f), wallMat);
        Wall(room, "Wall (front, right of door)", new Vector3( 2.5f, 1.5f, 6f), new Vector3(3f, 3f, 0.2f), wallMat);

        // The two blocks. Part 4: SpellTarget + Rigidbody + Collider + Renderer.
        var crate = Block(room, "Crate", new Vector3(-1.2f, 0.5f, 2.5f), Mat("SigilCasters_Crate", new Color(0.6f, 0.42f, 0.22f)));
        Caption(crate, "Crate", 0.9f, 0.08f);

        var generator = Block(room, "Generator", new Vector3(1.2f, 0.5f, 2.5f), Mat("SigilCasters_Generator", new Color(0.25f, 0.27f, 0.3f)));
        Caption(generator, "Generator", 0.9f, 0.08f);

        // A wire from the generator to the door, so the connection is visible. Decoration only: no collider.
        var wireMat = Mat("SigilCasters_Wire", new Color(0.15f, 0.15f, 0.15f));
        var wireA = Primitive(PrimitiveType.Cube, "Wire", room, wireMat);
        wireA.transform.position   = new Vector3(1.2f, 0.02f, 4.2f);
        wireA.transform.localScale = new Vector3(0.05f, 0.04f, 3.4f);
        var wireB = Primitive(PrimitiveType.Cube, "Wire", room, wireMat);
        wireB.transform.position   = new Vector3(0.6f, 0.02f, 5.9f);
        wireB.transform.localScale = new Vector3(1.25f, 0.04f, 0.05f);

        // The door. A collider (the ray stops on it) but NOT a SpellTarget. PoweredDoor watches the generator.
        var door = Primitive(PrimitiveType.Cube, "Door", room, Mat("SigilCasters_Door", new Color(0.55f, 0.2f, 0.15f)), keepCollider: true);
        door.transform.position   = new Vector3(0f, 1.5f, 6f);
        door.transform.localScale = new Vector3(2f, 3f, 0.2f);
        door.isStatic = false;
        var poweredDoor = door.AddComponent<PoweredDoor>();
        poweredDoor.generator = generator.GetComponent<SpellTarget>();
        Sign(room, "Sign (door)", new Vector3(0f, 0.45f, 5.85f), "DOOR\nopens when the generator is powered");

        // Beyond the door: the way out.
        var exit = Primitive(PrimitiveType.Sphere, "Exit", room, Mat("SigilCasters_Exit", new Color(1f, 0.85f, 0.3f)));
        exit.transform.position   = new Vector3(0f, 1.2f, 8f);
        exit.transform.localScale = Vector3.one * 0.6f;
        Caption(exit, "You escaped", 0.7f, 0.12f);

        // Signs on the front wall, readable from the start position.
        Sign(room, "Sign (spells)",   new Vector3(-2.5f, 2.2f, 5.85f), "SIGILS\n\nCircle  =  Levitate\nZigzag  =  Electric");
        Sign(room, "Sign (controls)", new Vector3( 2.5f, 2.2f, 5.85f), "CONTROLS\n\nLeft stick: walk\nTrigger (hold): draw\nGrip: cast");
    }

    static GameObject Wall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var wall = Primitive(PrimitiveType.Cube, name, parent, mat, keepCollider: true);
        wall.transform.position   = pos;
        wall.transform.localScale = scale;
        return wall;
    }

    static GameObject Block(Transform parent, string name, Vector3 pos, Material mat)
    {
        var block = Primitive(PrimitiveType.Cube, name, parent, mat, keepCollider: true);
        block.transform.position = pos;
        block.isStatic = false;
        var rb = block.AddComponent<Rigidbody>();
        rb.mass = 2f;
        block.AddComponent<SpellTarget>();
        return block;
    }

    // ---------------------------------------------------------------- rig (same as Assignment 3)

    static GameObject BuildRig(Camera cam, out Transform right, out Transform left, out Transform head)
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
        head = cam.transform;

        xrOrigin.Camera = cam;
        xrOrigin.CameraFloorOffsetObject = offset.gameObject;
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        xrOrigin.CameraYOffset = 1.36144f;
        offset.localPosition = new Vector3(0f, xrOrigin.CameraYOffset, 0f);

        left  = Hand("Left Controller",  LeftHand,  offset, Mat("SigilCasters_LeftHand",  new Color(0.2f, 0.45f, 0.95f)));
        right = Hand("Right Controller", RightHand, offset, Mat("SigilCasters_RightHand", new Color(0.9f, 0.25f, 0.2f)));

        return origin;
    }

    static Transform Hand(string name, string usage, Transform parent, Material mat)
    {
        var hand = new GameObject(name);
        hand.transform.SetParent(parent, false);
        hand.transform.localPosition = new Vector3(usage == LeftHand ? -0.2f : 0.2f, -0.3f, 0.3f);
        Track(hand, "<XRController>" + usage, "devicePosition", "deviceRotation");

        var visual = Primitive(PrimitiveType.Cube, "Visual", hand.transform, mat);
        visual.transform.localScale = new Vector3(0.05f, 0.04f, 0.13f);
        visual.isStatic = false;

        return hand.transform;
    }

    static void Track(GameObject go, string device, string position, string rotation)
    {
        var driver = go.AddComponent<TrackedPoseDriver>();
        driver.positionInput      = Action("Position",       device + "/" + position,     "Vector3");
        driver.rotationInput      = Action("Rotation",       device + "/" + rotation,     "Quaternion");
        driver.trackingStateInput = Action("Tracking State", device + "/trackingState",   "Integer");
    }

    // An action that carries its own single binding, so the scene needs no .inputactions asset.
    static InputActionProperty Action(string name, string binding, string controlType)
        => new InputActionProperty(new InputAction(name, binding: binding, expectedControlType: controlType));

    static void BuildHeadsetOrSimulator()
    {
        var switcher = new GameObject("Headset Or Simulator").AddComponent<HeadsetOrSimulator>();

        foreach (var guid in AssetDatabase.FindAssets("\"" + Simulator + "\" t:Prefab"))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null || prefab.name != Simulator) continue;

            var sim = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            sim.SetActive(false);
            switcher.simulator = sim;
            return;
        }

        Debug.LogWarning("[Sigil Lab] '" + Simulator + "' prefab not found. Import it from Package Manager > XR Interaction Toolkit > Samples, then build again.");
    }

    // ---------------------------------------------------------------- helpers

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

    static void Caption(GameObject target, string text, float above, float size = 0.05f)
    {
        var label = Label(null, target.name + " caption", text, size);
        label.transform.position = target.transform.position + Vector3.up * above;
    }

    static void Sign(Transform parent, string name, Vector3 pos, string text)
    {
        var label = Label(parent, name, text, 0.12f);
        label.transform.position = pos;
    }

    static TextMesh Label(Transform parent, string name, string text, float size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;
        tm.characterSize = size * 0.1f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return tm;
    }

    static Material LineMat()
    {
        var path = MaterialDir + "/SigilCasters_Line.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));   // shows the LineRenderer's own colours
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
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
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
