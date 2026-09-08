using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lifecycle;

/// <summary>
/// The "Lifecycle Lab" menu. Three tiny scenes for the "C# in Unity: why there is no main()" lecture.
///
///   Lifecycle Lab > 1 - Probe            one cube with LifecycleProbe; read the Console
///   Lifecycle Lab > 2 - Call Chain       a MonoBehaviour calling plain C#
///   Lifecycle Lab > 3 - Frames vs Time   three cubes: per-frame, per-second, FixedUpdate. Keys 1-4 cap fps.
/// </summary>
public static class LifecycleSceneBuilder
{
    const string SceneDir    = "Assets/Scenes";
    const string MaterialDir = "Assets/Materials";

    [MenuItem("Lifecycle Lab/1 - Probe")]
    public static void BuildProbe()
    {
        var scene = Fresh(new Vector3(0f, 1.5f, -5f), Vector3.zero);

        var cube = Cube("Cube", Vector3.zero, Mat("Lifecycle_Probe", new Color(0.85f, 0.85f, 0.85f)));
        cube.AddComponent<LifecycleProbe>();

        Save(scene, "Lifecycle_1_Probe");
    }

    [MenuItem("Lifecycle Lab/2 - Call Chain")]
    public static void BuildCallChain()
    {
        var scene = Fresh(new Vector3(0f, 1.5f, -5f), Vector3.zero);

        var cube = Cube("Caller", Vector3.zero, Mat("Lifecycle_Probe", new Color(0.85f, 0.85f, 0.85f)));
        cube.AddComponent<CallChainDemo>();

        Save(scene, "Lifecycle_2_CallChain");
    }

    [MenuItem("Lifecycle Lab/3 - Frames vs Time")]
    public static void BuildFramesVsTime()
    {
        var scene = Fresh(new Vector3(0f, 7f, -11f), new Vector3(0f, 0f, 0f));

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(2.2f, 1f, 1f);
        ground.GetComponent<Renderer>().sharedMaterial = Mat("Lifecycle_Ground", new Color(0.22f, 0.24f, 0.28f));

        var lineMat = Mat("Lifecycle_Line", new Color(0.95f, 0.95f, 0.95f));
        Line("Start line",  -9f, lineMat);
        Line("Finish line",  9f, lineMat);

        var frame = Cube("Frame cube  (0.1 per Update)",     new Vector3(-9f, 0.5f,  2f), Mat("Lifecycle_Frame", new Color(0.85f, 0.3f, 0.25f)));
        var time  = Cube("Time cube  (6 per second)",        new Vector3(-9f, 0.5f,  0f), Mat("Lifecycle_Time",  new Color(0.3f, 0.75f, 0.35f)));
        var fixd  = Cube("Fixed cube  (6 per second, FixedUpdate)", new Vector3(-9f, 0.5f, -2f), Mat("Lifecycle_Fixed", new Color(0.3f, 0.5f, 0.9f)));

        frame.AddComponent<FrameMover>();
        time.AddComponent<TimeMover>();
        fixd.AddComponent<FixedMover>();
        frame.AddComponent<WrapAround>();
        time.AddComponent<WrapAround>();
        fixd.AddComponent<WrapAround>();

        var sw = new GameObject("Frame Rate Switch").AddComponent<FrameRateSwitch>();
        sw.cubes = new[] { frame.transform, time.transform, fixd.transform };

        Save(scene, "Lifecycle_3_FramesVsTime");
    }

    // ------------------------------------------------------------- helpers

    static UnityEngine.SceneManagement.Scene Fresh(Vector3 camPos, Vector3 lookAt)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        cam.transform.position = camPos;
        cam.transform.LookAt(lookAt);
        return scene;
    }

    static void Save(UnityEngine.SceneManagement.Scene scene, string name)
    {
        if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder("Assets", "Scenes");
        var path = SceneDir + "/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[Lifecycle Lab] Built " + path);
    }

    static GameObject Cube(string name, Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void Line(string name, float x, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = new Vector3(x, 0.01f, 0f);
        go.transform.localScale = new Vector3(0.08f, 0.02f, 8f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
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
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
