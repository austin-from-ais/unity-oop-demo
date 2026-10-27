using Assignment3;
using RaycastLab;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// The "Raycast Lab" menu. Five small VR scenes for the raycasts + XR Interaction Toolkit practical.
///
///   Raycast Lab > 1 - Ray By Hand              Physics.Raycast written out, drawn as a line
///   Raycast Lab > 2 - Ray Interactor           XRRayInteractor + line visual + interactables
///   Raycast Lab > 3 - Some Things Not Others   Unity layer the ray skips vs interaction layer it can't select
///   Raycast Lab > 4 - Canvas Buttons           world-space canvas, TrackedDeviceGraphicRaycaster, XRUIInputModule
///   Raycast Lab > 5 - Grab                     XRGrabInteractable: ray grab (right) and direct grab (left)
///   Raycast Lab > Build All
///
/// Same lean rig as Assignment 3: XR Origin, tracked camera, two tracked block hands, and
/// HeadsetOrSimulator to pick a real headset or the keyboard simulator on Play.
/// Simulator keys: hold ] to take the right hand, right mouse + move to aim, G = grip, T = trigger.
/// </summary>
public static class RaycastLabSceneBuilder
{
    const string SceneDir     = "Assets/Scenes";
    const string MaterialDir  = "Assets/Materials";
    const string Simulator    = "XR Interaction Simulator";

    const string GhostLayer   = "Ghost";    // Unity layer. The ray's raycast mask skips it: the ray passes through.
    const string LockedLayer  = "Locked";   // XRI interaction layer. The ray stops on it but can't hover or select.
    const int    GhostIndex   = 6;          // first user-defined Unity layer slot
    const int    LockedIndex  = 1;          // first free XRI interaction layer slot

    const string RightHand = "{RightHand}";
    const string LeftHand  = "{LeftHand}";

    // ------------------------------------------------------------------ menu

    [MenuItem("Raycast Lab/1 - Ray By Hand")]
    public static void Build1()
    {
        var scene = Fresh(out var right, out var left);

        // No toolkit here. A LineRenderer, a marker sphere, and our own script asking physics.
        var line = right.gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = LineMat();
        line.startWidth = line.endWidth = 0.01f;

        var marker = Primitive(PrimitiveType.Sphere, "Hit Marker", null, Mat("Raycast_Marker", new Color(1f, 0.95f, 0.3f)));
        marker.transform.localScale = Vector3.one * 0.05f;
        marker.isStatic = false;

        var probe = right.gameObject.AddComponent<RayProbe>();
        probe.marker  = marker.transform;
        probe.readout = Label(right, "Readout", "", 0.1f);

        var solid = Target(PrimitiveType.Cube,   "Solid cube (has a collider)",  new Vector3( 0f, 1.2f, 3f), Mat("Raycast_Solid", new Color(0.85f, 0.85f, 0.85f)));
        var ghost = Target(PrimitiveType.Cube,   "See-through cube (NO collider)", new Vector3(-1f, 1.2f, 3f), Mat("Raycast_NoCollider", new Color(0.75f, 0.6f, 0.95f)));
        var ball  = Target(PrimitiveType.Sphere, "Sphere (has a collider)",       new Vector3( 1f, 1.2f, 3f), Mat("Raycast_Solid", new Color(0.85f, 0.85f, 0.85f)));
        Object.DestroyImmediate(ghost.GetComponent<Collider>());   // the whole point of this cube

        Caption(solid, "collider");
        Caption(ghost, "no collider:\nthe ray goes through");
        Caption(ball,  "collider");

        BackWall();
        Save(scene, "Raycast_1_RayByHand");
    }

    [MenuItem("Raycast Lab/2 - Ray Interactor")]
    public static void Build2()
    {
        var scene = Fresh(out var right, out var left);

        Manager();
        var ray = RayInteractor(right, uiToo: false);
        Readout(right, ray);

        var plain = Target(PrimitiveType.Cube, "Plain cube (collider only)", new Vector3(-1f, 1.2f, 3f), Mat("Raycast_Solid", new Color(0.85f, 0.85f, 0.85f)));
        var cube  = Interactable(PrimitiveType.Cube,   "Interactable cube",   new Vector3(0f, 1.2f, 3f), Mat("Raycast_Interactable", new Color(0.35f, 0.65f, 0.95f)));
        var ball  = Interactable(PrimitiveType.Sphere, "Interactable sphere", new Vector3(1f, 1.2f, 3f), Mat("Raycast_Interactable", new Color(0.35f, 0.65f, 0.95f)));

        Caption(plain, "collider, no interactable:\nthe ray stops, line goes red");
        Caption(cube,  "XRSimpleInteractable:\naim = hover, G = select");
        Caption(ball,  "XRSimpleInteractable");

        BackWall();
        Save(scene, "Raycast_2_RayInteractor");
    }

    [MenuItem("Raycast Lab/3 - Some Things Not Others")]
    public static void Build3()
    {
        EnsureLayers();
        var scene = Fresh(out var right, out var left);

        Manager();
        var ray = RayInteractor(right, uiToo: false);
        Readout(right, ray);

        // Filter 1: the physics raycast. Everything except the Ghost layer. Ghost objects do not exist to this ray.
        ray.raycastMask = ~(1 << LayerMask.NameToLayer(GhostLayer));

        // Filter 2: the toolkit. Only interactables on the Default interaction layer may hover/select.
        ray.interactionLayers = 1 << 0;

        var ghost  = Interactable(PrimitiveType.Cube, "Ghost cube (Unity layer: Ghost)", new Vector3(-1f, 1.2f, 2.4f), Mat("Raycast_Ghost", new Color(0.8f, 0.8f, 0.8f, 1f)));
        ghost.layer = LayerMask.NameToLayer(GhostLayer);
        ghost.transform.localScale = Vector3.one * 0.6f;

        var behind = Interactable(PrimitiveType.Cube, "Cube behind the ghost", new Vector3(-1f, 1.2f, 4f), Mat("Raycast_Interactable", new Color(0.35f, 0.65f, 0.95f)));
        var normal = Interactable(PrimitiveType.Cube, "Normal cube",           new Vector3( 0f, 1.2f, 3f), Mat("Raycast_Interactable", new Color(0.35f, 0.65f, 0.95f)));

        var locked = Interactable(PrimitiveType.Cube, "Locked cube (interaction layer: Locked)", new Vector3(1f, 1.2f, 3f), Mat("Raycast_Locked", new Color(0.55f, 0.3f, 0.3f)));
        locked.GetComponent<XRBaseInteractable>().interactionLayers = 1 << LockedIndex;

        Caption(ghost,  "Unity layer 'Ghost':\nnot in the Raycast Mask.\nThe ray passes through it", 0.45f);
        Caption(normal, "Default layers:\nhover and select work");
        Caption(locked, "Interaction layer 'Locked':\nthe ray stops here but\ncan't hover or select");

        BackWall();
        Save(scene, "Raycast_3_SomeThingsNotOthers");
    }

    [MenuItem("Raycast Lab/4 - Canvas Buttons")]
    public static void Build4()
    {
        var scene = Fresh(out var right, out var left);

        Manager();

        // The UI side of the toolkit: an EventSystem whose input module understands tracked devices.
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<XRUIInputModule>();

        var ray = RayInteractor(right, uiToo: true);   // UI Interaction on, trigger = UI press
        Readout(right, ray);

        var target = Target(PrimitiveType.Cube, "Paint me", new Vector3(0f, 0.95f, 2.4f), Mat("Raycast_PaintMe", new Color(0.85f, 0.85f, 0.85f)));
        target.transform.localScale = Vector3.one * 0.3f;
        target.isStatic = false;

        var paint = target.AddComponent<PaintTarget>();
        paint.target = target.GetComponent<Renderer>();

        // Right canvas: the one that works.
        var good = Canvas("VR Canvas (TrackedDeviceGraphicRaycaster)", new Vector3(0.75f, 1.35f, 2.4f));
        good.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        UIText(good, "Title", "TrackedDeviceGraphicRaycaster", new Vector2(0f, 230f), 30);
        Button(good, "Red",   "Red",   new Vector2(0f,  120f), paint.PaintRed);
        Button(good, "Green", "Green", new Vector2(0f,    0f), paint.PaintGreen);
        Button(good, "Blue",  "Blue",  new Vector2(0f, -120f), paint.PaintBlue);
        paint.counter = UIText(good, "Counter", "Clicks: 0", new Vector2(0f, -230f), 34);

        // Left canvas: looks identical, has only the ordinary GraphicRaycaster. The ray can't see it.
        var bad = Canvas("Plain Canvas (GraphicRaycaster only)", new Vector3(-0.75f, 1.35f, 2.4f));
        bad.gameObject.AddComponent<GraphicRaycaster>();
        UIText(bad, "Title", "GraphicRaycaster only", new Vector2(0f, 230f), 30);
        Button(bad, "Red",   "Red",   new Vector2(0f,  120f), paint.PaintRed);
        Button(bad, "Green", "Green", new Vector2(0f,    0f), paint.PaintGreen);
        Button(bad, "Blue",  "Blue",  new Vector2(0f, -120f), paint.PaintBlue);
        UIText(bad, "Note", "(mouse only: the VR ray\ndoes not see this canvas)", new Vector2(0f, -230f), 24);

        BackWall();
        Save(scene, "Raycast_4_CanvasButtons");
    }

    [MenuItem("Raycast Lab/5 - Grab")]
    public static void Build5()
    {
        var scene = Fresh(out var right, out var left);

        Manager();

        // Right hand: ray grab. Force Grab pulls the object to the hand instead of leaving it at the end of the ray.
        var ray = RayInteractor(right, uiToo: false);
        ray.useForceGrab = true;
        Readout(right, ray);

        // Left hand: direct grab. No ray at all: a trigger sphere around the hand, grip when touching.
        var direct = left.gameObject.AddComponent<XRDirectInteractor>();
        direct.selectInput = Button("Select", LeftHand, "GripButton", "Grip");
        direct.handedness = InteractorHandedness.Left;
        var touch = left.gameObject.AddComponent<SphereCollider>();
        touch.isTrigger = true;
        touch.radius = 0.12f;

        var table = Primitive(PrimitiveType.Cube, "Table", null, Mat("Raycast_Table", new Color(0.45f, 0.32f, 0.22f)), keepCollider: true);
        table.transform.position   = new Vector3(0f, 0.85f, 2f);
        table.transform.localScale = new Vector3(1.6f, 0.06f, 0.7f);

        Grabbable(PrimitiveType.Cube,   "Grab cube",  new Vector3(-0.45f, 1.0f, 2f), Mat("Raycast_Grab",  new Color(0.95f, 0.6f, 0.2f)), 0.18f);
        Grabbable(PrimitiveType.Sphere, "Throw ball", new Vector3( 0f,    1.0f, 2f), Mat("Raycast_Ball",  new Color(0.3f, 0.8f, 0.4f)),  0.16f);
        Grabbable(PrimitiveType.Cube,   "Grab cube 2", new Vector3(0.45f, 1.0f, 2f), Mat("Raycast_Grab",  new Color(0.95f, 0.6f, 0.2f)), 0.18f);

        var bolted = Target(PrimitiveType.Cube, "Bolted-down cube (not grabbable)", new Vector3(0.9f, 1.0f, 2f), Mat("Raycast_Solid", new Color(0.85f, 0.85f, 0.85f)));
        bolted.transform.localScale = Vector3.one * 0.18f;
        Caption(bolted, "collider but no\nXRGrabInteractable", 0.2f);

        Caption(table, "Right ray + G: grab from here.  Left hand touching + G: grab up close.", 0.6f, 0.07f);

        BackWall();
        Save(scene, "Raycast_5_Grab");
    }

    [MenuItem("Raycast Lab/Build All")]
    public static void BuildAll()
    {
        Build1(); Build2(); Build3(); Build4(); Build5();
    }

    // ------------------------------------------------------------- XRI bits

    static void Manager()
    {
        // Interactors find or create one on their own, but an explicit one is easier to point at in class.
        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
    }

    static XRRayInteractor RayInteractor(Transform hand, bool uiToo)
    {
        var ray = hand.gameObject.AddComponent<XRRayInteractor>();
        ray.handedness = InteractorHandedness.Right;
        ray.maxRaycastDistance = 10f;
        ray.lineType = XRRayInteractor.LineType.StraightLine;
        ray.enableUIInteraction = uiToo;

        // Grip = select (hover + grip = grab/press). Trigger = UI press (click a button). Same split as XRI's defaults.
        ray.selectInput  = Button("Select",   RightHand, "GripButton",    "Grip");
        ray.uiPressInput = Button("UI Press", RightHand, "TriggerButton", "Trigger");

        // The visual. XRInteractorLineVisual reads the interactor on the same object and drives a LineRenderer.
        var line = hand.gameObject.GetComponent<LineRenderer>();
        if (line == null) line = hand.gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial = LineMat();

        var visual = hand.gameObject.AddComponent<XRInteractorLineVisual>();
        visual.lineWidth = 0.01f;
        visual.setLineColorGradient = true;
        visual.validColorGradient   = Solid(new Color(0.3f, 1f, 0.4f));    // hovering something it may select
        visual.invalidColorGradient = Solid(new Color(1f, 0.3f, 0.3f));    // hitting something, or nothing, it can't use
        visual.stopLineAtFirstRaycastHit = true;

        return ray;
    }

    // An input reader that carries its own action, bound by usage so it works on the simulator and on real controllers.
    static XRInputButtonReader Button(string name, string hand, string buttonUsage, string valueUsage)
    {
        var reader = new XRInputButtonReader(name, inputSourceMode: XRInputButtonReader.InputSourceMode.InputAction);
        reader.inputActionPerformed.AddBinding("<XRController>" + hand + "/{" + buttonUsage + "}");
        reader.inputActionValue.AddBinding("<XRController>" + hand + "/{" + valueUsage + "}");
        return reader;
    }

    static void Readout(Transform hand, XRRayInteractor ray)
    {
        var readout = hand.gameObject.AddComponent<RayReadout>();
        readout.interactor = ray;
        readout.readout = Label(hand, "Readout", "", 0.1f);
    }

    static GameObject Interactable(PrimitiveType type, string name, Vector3 pos, Material mat)
    {
        var go = Target(type, name, pos, mat);
        go.isStatic = false;
        go.AddComponent<XRSimpleInteractable>();
        go.AddComponent<HoverHighlight>();
        return go;
    }

    static GameObject Grabbable(PrimitiveType type, string name, Vector3 pos, Material mat, float size)
    {
        var go = Target(type, name, pos, mat);
        go.isStatic = false;
        go.transform.localScale = Vector3.one * size;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.5f;

        var grab = go.AddComponent<XRGrabInteractable>();
        grab.movementType  = XRBaseInteractable.MovementType.Kinematic;   // follows the hand, still pushes other physics objects
        grab.throwOnDetach = true;                                         // let go while moving = it keeps the velocity

        go.AddComponent<HoverHighlight>();
        return go;
    }

    static Canvas Canvas(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        go.AddComponent<CanvasScaler>();

        var rt = canvas.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(600f, 600f);
        rt.localScale = Vector3.one * 0.001f;   // 600 units -> 0.6 m
        rt.position   = pos;

        var bg = new GameObject("Background", typeof(Image)).GetComponent<Image>();
        bg.transform.SetParent(go.transform, false);
        bg.color = new Color(0.12f, 0.12f, 0.15f, 0.9f);
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;

        return canvas;
    }

    static Button Button(Canvas canvas, string name, string label, Vector2 pos, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);

        var image = go.GetComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = new Color(0.9f, 0.9f, 0.9f);

        var rt = image.rectTransform;
        rt.sizeDelta = new Vector2(360f, 90f);
        rt.anchoredPosition = pos;

        var button = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, onClick);

        var text = UIText(canvas, "Label", label, Vector2.zero, 36);
        text.transform.SetParent(go.transform, false);
        text.color = new Color(0.1f, 0.1f, 0.1f);
        text.rectTransform.sizeDelta = rt.sizeDelta;
        text.rectTransform.anchoredPosition = Vector2.zero;

        return button;
    }

    static Text UIText(Canvas canvas, string name, string content, Vector2 pos, int size)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(canvas.transform, false);

        var text = go.GetComponent<Text>();
        text.text = content;
        text.font = DefaultFont();
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.rectTransform.sizeDelta = new Vector2(560f, 100f);
        text.rectTransform.anchoredPosition = pos;
        return text;
    }

    // ------------------------------------------------------------- layers

    // Adds the Unity layer "Ghost" and the XRI interaction layer "Locked" if they are not there yet.
    // Also runs once when the editor loads (see RaycastLabLayers below), so a fresh clone gets the
    // layer names without anyone building the scenes.
    public static void EnsureLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        if (LayerMask.NameToLayer(GhostLayer) < 0)
        {
            var slot = layers.GetArrayElementAtIndex(GhostIndex);
            if (string.IsNullOrEmpty(slot.stringValue)) slot.stringValue = GhostLayer;
            else Debug.LogWarning("[Raycast Lab] Unity layer slot " + GhostIndex + " is taken by '" + slot.stringValue + "'. Add a layer called " + GhostLayer + " yourself.");
            tagManager.ApplyModifiedProperties();
        }

        // The interaction layer names live in an asset XRI generates per machine (Assets/XRI/Settings/...).
        // It is deliberately not in git, so find it wherever it is. The scenes only store the layer's bit,
        // so Lab 3 works even if the name is missing; the name just makes the Inspector readable.
        if (InteractionLayerMask.NameToLayer(LockedLayer) >= 0) return;

        foreach (var guid in AssetDatabase.FindAssets("t:InteractionLayerSettings"))
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (settings == null) continue;

            var so = new SerializedObject(settings);
            var names = so.FindProperty("m_LayerNames");
            if (names == null || names.arraySize <= LockedIndex) continue;

            var slot = names.GetArrayElementAtIndex(LockedIndex);
            if (!string.IsNullOrEmpty(slot.stringValue))
            {
                Debug.LogWarning("[Raycast Lab] Interaction layer " + LockedIndex + " is already named '" + slot.stringValue + "'. Lab 3 calls it " + LockedLayer + ".");
                return;
            }
            slot.stringValue = LockedLayer;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
            return;
        }

        Debug.Log("[Raycast Lab] No InteractionLayerSettings asset yet. Open Edit > Project Settings > XR Interaction Toolkit once and XRI will create it; the Locked layer name is added the next time the editor loads.");
    }

    // ------------------------------------------------------------- rig

    static UnityEngine.SceneManagement.Scene Fresh(out Transform right, out Transform left)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var light = Object.FindAnyObjectByType<Light>();
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.intensity = 1.4f;
        light.shadows = LightShadows.Soft;

        BuildRig(Camera.main, out right, out left);
        BuildHeadsetOrSimulator();

        var ground = Primitive(PrimitiveType.Plane, "Ground", null, Mat("Raycast_Ground", new Color(0.3f, 0.32f, 0.36f)), keepCollider: true);
        ground.transform.localScale = new Vector3(3f, 1f, 3f);

        return scene;
    }

    static void BuildRig(Camera cam, out Transform right, out Transform left)
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
        offset.localPosition = new Vector3(0f, xrOrigin.CameraYOffset, 0f);

        left  = Hand("Left Controller",  LeftHand,  offset, Mat("Raycast_LeftHand",  new Color(0.2f, 0.45f, 0.95f)));
        right = Hand("Right Controller", RightHand, offset, Mat("Raycast_RightHand", new Color(0.9f, 0.25f, 0.2f)));
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
            sim.SetActive(false);
            switcher.simulator = sim;
            return;
        }

        Debug.LogWarning("[Raycast Lab] '" + Simulator + "' prefab not found. Import it from Package Manager > XR Interaction Toolkit > Samples, then build again.");
    }

    // ------------------------------------------------------------- props

    static GameObject Target(PrimitiveType type, string name, Vector3 pos, Material mat)
    {
        var go = Primitive(type, name, null, mat, keepCollider: true);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.5f;
        return go;
    }

    static void BackWall()
    {
        var wall = Primitive(PrimitiveType.Cube, "Back wall", null, Mat("Raycast_Wall", new Color(0.55f, 0.58f, 0.62f)), keepCollider: true);
        wall.transform.position   = new Vector3(0f, 2f, 7f);
        wall.transform.localScale = new Vector3(12f, 4f, 0.2f);
    }

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

    // A 3D label below an object, facing the player.
    static void Caption(GameObject target, string text, float below = 0.4f, float size = 0.05f)
    {
        var label = Label(null, target.name + " caption", text, 0f, size);
        label.transform.position = target.transform.position + Vector3.down * below;
    }

    static TextMesh Label(Transform parent, string name, string text, float above, float size = 0.04f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, above, 0f);

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = DefaultFont();
        tm.fontSize = 64;
        tm.characterSize = size * 0.1f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return tm;
    }

    static Font DefaultFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    static Gradient Solid(Color c)
    {
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    // Lines need a shader that shows vertex colours, so the line visual's red/green shows up.
    static Material LineMat()
    {
        var path = MaterialDir + "/Raycast_Line.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
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
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void Save(UnityEngine.SceneManagement.Scene scene, string name)
    {
        if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder("Assets", "Scenes");
        var path = SceneDir + "/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[Raycast Lab] Built " + path);
    }
}

/// <summary>
/// Names the Lab 3 layers on every editor load, so a student who clones the repo and opens
/// Raycast_3 sees "Ghost" and "Locked" in the Inspector without building anything.
/// </summary>
[InitializeOnLoad]
static class RaycastLabLayers
{
    static RaycastLabLayers()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            RaycastLabSceneBuilder.EnsureLayers();
        };
    }
}
