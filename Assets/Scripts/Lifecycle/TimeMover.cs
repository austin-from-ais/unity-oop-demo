using UnityEngine;

// C# IN UNITY - TIME.DELTATIME
//
// Time.deltaTime is how many seconds the last frame took. Multiply a per-second speed by it
// and you get the right distance for THIS frame, however long it was.
// 6 units a second at 60 fps. 6 units a second at 15 fps. Same code, same speed.

namespace Lifecycle
{
    public class TimeMover : MonoBehaviour
    {
        public float unitsPerSecond = 6f;

        void Update()
        {
            transform.Translate(unitsPerSecond * Time.deltaTime, 0f, 0f);
        }
    }
}
