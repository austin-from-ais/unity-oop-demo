using UnityEngine;

// C# IN UNITY - LATEUPDATE
//
// LateUpdate runs after every script's Update has finished. This one looks at where the cube
// ended up this frame and, if it ran off the right edge, puts it back on the left.
// It doesn't care which mover pushed it. It just sees the final position.

namespace Lifecycle
{
    public class WrapAround : MonoBehaviour
    {
        public float rightEdge = 9f;
        public float leftEdge  = -9f;

        void LateUpdate()
        {
            if (transform.position.x > rightEdge)
            {
                var p = transform.position;
                p.x = leftEdge;
                transform.position = p;
            }
        }
    }
}
