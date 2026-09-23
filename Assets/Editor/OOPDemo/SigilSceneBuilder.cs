using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Sigils;

/// <summary>
/// The "Sigil Lab" menu. One scene: an empty GameObject carrying the $1 recogniser and the
/// image tester. Select it, drop a line drawing into the slot, press the button, read the Console.
///
///   Sigil Lab > Build Sigil Scene     Scenes/Sigils.unity
/// </summary>
public static class SigilSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Sigils.unity";
    const string DefaultImage = "Assets/Textures/SigilTests/circle.png";

    [MenuItem("Sigil Lab/Build Sigil Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var go = new GameObject("Sigil Recognizer");
        go.AddComponent<SigilRecognizer>();
        var test = go.AddComponent<SigilImageTest>();
        test.image = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultImage);

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = go;
        Debug.Log("[Sigil Lab] Built " + ScenePath + ". Select 'Sigil Recognizer' and press the button in the Inspector.");
    }
}
