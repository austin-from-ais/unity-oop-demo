using UnityEditor;
using UnityEngine;
using Sigils;

/// <summary>
/// Custom Inspector for SigilImageTest: shows the "what kind of image" note and a button that
/// runs the recogniser and prints to the Console. Editor-only; the component works without it.
/// </summary>
[CustomEditor(typeof(SigilImageTest))]
public class SigilImageTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            SigilImageTest.ImageNote +
            "\n\nPress the button below (or use the component's context menu > Recognize Image). " +
            "What the recogniser sees is printed to the Console.",
            MessageType.Info);

        DrawDefaultInspector();

        var test = (SigilImageTest)target;
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(test.image == null))
        {
            if (GUILayout.Button("Recognize image  ->  Console", GUILayout.Height(28)))
                test.RecognizeImage();
        }

        if (test.image == null)
            EditorGUILayout.HelpBox("Assign an image first. Try one from Assets/Textures/SigilTests.", MessageType.Warning);
    }
}
