using UnityEngine;
using UnityEngine.InputSystem;

// C# IN UNITY - "WHAT HAPPENS WHEN WE CHANGE THE FRAMERATE?"
//
// Press 1 / 2 / 3 / 4 to cap the game at 15 / 30 / 60 / uncapped fps.
// Press R to put every cube back at the start line.
// The label in the corner shows what each cube is doing.

namespace Lifecycle
{
    public class FrameRateSwitch : MonoBehaviour
    {
        public Transform[] cubes;
        public float startX = -9f;

        float smoothedFps;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;          // vsync would override targetFrameRate
            Application.targetFrameRate = 60;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) Application.targetFrameRate = 15;
                if (kb.digit2Key.wasPressedThisFrame) Application.targetFrameRate = 30;
                if (kb.digit3Key.wasPressedThisFrame) Application.targetFrameRate = 60;
                if (kb.digit4Key.wasPressedThisFrame) Application.targetFrameRate = -1;
                if (kb.rKey.wasPressedThisFrame)      ResetCubes();
            }

            float fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            smoothedFps = Mathf.Lerp(smoothedFps, fps, 0.1f);
        }

        void ResetCubes()
        {
            foreach (var c in cubes)
            {
                var p = c.position;
                p.x = startX;
                c.position = p;
            }
        }

        void OnGUI()
        {
            GUI.skin.label.fontSize = 18;
            string cap = Application.targetFrameRate < 0 ? "uncapped" : Application.targetFrameRate + " fps";

            GUI.Label(new Rect(16, 12, 700, 30), $"Target: {cap}    Actual: {smoothedFps:0} fps      [1] 15  [2] 30  [3] 60  [4] uncapped  [R] reset");

            int y = 46;
            foreach (var c in cubes)
            {
                GUI.Label(new Rect(16, y, 700, 30), $"{c.name}:  x = {c.position.x:0.0}");
                y += 26;
            }
        }
    }
}
