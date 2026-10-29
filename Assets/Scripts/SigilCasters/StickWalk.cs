using UnityEngine;
using UnityEngine.InputSystem;

// SIGIL CASTERS, PART 1 - WALKING WITH THE THUMBSTICK
//
// In one line: read the left thumbstick, work out which way "forward" is from the headset, and
// slide the whole rig that way a little every frame.
//
// This is Assignment 3 (Flying) with one change. There, the hands decided the direction. Here
// the thumbstick does, and we stay on the floor. Same rig, same rule:
//
//   XR Origin (XR Rig)        <- this script is here. Moving THIS moves the player.
//     Camera Offset
//       Main Camera           <- the head. READ it. Never move it; the headset overwrites it.
//       Left Controller       <- the thumbstick we read is on this controller
//       Right Controller
//
// SIMPLE vs ROBUST. Every script in this folder makes the simple, readable choice and says so.
// In this one:
//   - No collision. The player can walk straight through walls. The robust version puts a
//     CharacterController on the rig and calls its Move() instead of adding to position.
//   - No dead zone. A thumbstick at rest reports something like (0.01, -0.02), not (0, 0), so
//     the player drifts a hair. The robust version ignores anything shorter than about 0.1.
//   - No turning. You turn by turning your head and body. Snap turn would be a second script
//     of exactly this shape: read the right stick's x, rotate the rig.
//   - No comfort options (vignette, teleport). Real VR games need them. A demo does not.

namespace SigilCasters
{
    public class StickWalk : MonoBehaviour
    {
        [Tooltip("The Main Camera. 'Forward' means the way the head is facing.")]
        public Transform head;

        [Tooltip("The left thumbstick, as an Input System action. The scene builder binds it to <XRController>{LeftHand}/{Primary2DAxis}.")]
        public InputActionProperty moveStick;

        [Tooltip("Walking speed in metres per second. 2 is a brisk walk.")]
        public float speed = 2f;

        // An InputAction does nothing until it is enabled. Think of it as a component that
        // starts switched off. OnEnable / OnDisable are the "switched on / switched off" pair
        // from the Lifecycle Lab, so this is the natural place to flip it.
        void OnEnable()  => moveStick.action.Enable();
        void OnDisable() => moveStick.action.Disable();

        void Update()
        {
            // 1. Read the stick. A Vector2: x is left/right (-1..1), y is back/forward (-1..1).
            Vector2 stick = moveStick.action.ReadValue<Vector2>();

            // 2. Which way is "forward"? The way the head faces... but flattened. If you look at
            //    the floor and push the stick, you want to walk, not sink into the ground. So
            //    throw away the up/down part of the head's forward arrow, then make it one
            //    metre long again (Normalize), so looking down does not also make you slower.
            Vector3 forward = head.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = head.right;
            right.y = 0f;
            right.Normalize();

            // 3. Combine. Stick up = forward, stick right = right. Half up and half right gives
            //    a diagonal. Scaling an arrow by a number and adding arrows: that is all this is.
            Vector3 direction = forward * stick.y + right * stick.x;

            // 4. Move the rig. Times Time.deltaTime, because Update runs once per FRAME and
            //    frames are not all the same length (Lifecycle Lab 3: the red and green cubes).
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}
