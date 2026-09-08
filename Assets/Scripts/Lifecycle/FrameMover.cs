using UnityEngine;

// C# IN UNITY - UPDATE TRACKS FRAMES, NOT TIME
//
// Moves 0.1 units every time Update runs. Update runs once per frame.
// So at 60 fps this moves 6 units a second. At 30 fps, 3. At 15 fps, 1.5.
// Same code, different speed, depending on how fast the machine is.

namespace Lifecycle
{
    public class FrameMover : MonoBehaviour
    {
        public float unitsPerFrame = 0.1f;

        void Update()
        {
            transform.Translate(unitsPerFrame, 0f, 0f);
        }
    }
}
