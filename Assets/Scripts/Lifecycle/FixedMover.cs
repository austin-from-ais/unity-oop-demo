using UnityEngine;

// C# IN UNITY - FIXEDUPDATE
//
// FixedUpdate runs on a fixed timestep (0.02 s by default), not once per frame.
// On a slow frame it runs several times to catch up. On a very fast frame it may not run at all.
// Time.fixedDeltaTime is always the same number, so this also moves at a steady real-world speed.
// Watch it at 15 fps: it keeps pace with the green cube, but moves in visible steps.

namespace Lifecycle
{
    public class FixedMover : MonoBehaviour
    {
        public float unitsPerSecond = 6f;

        void FixedUpdate()
        {
            transform.Translate(unitsPerSecond * Time.fixedDeltaTime, 0f, 0f);
        }
    }
}
