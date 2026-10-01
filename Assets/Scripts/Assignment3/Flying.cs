using UnityEngine;

// ASSIGNMENT 3 - FLYING
//
// This is the file you edit and hand in. Fill in Update() so the player flies where their
// hands point: both hands up, go up. Both hands down, go down. Reach forward, go forward.
//
// This script sits on "XR Origin (XR Rig)" in the scene Scenes/Assignment3_Flying.
// The XR Origin is the player's body. The headset and both controllers are its children:
//
//   XR Origin (XR Rig)        <- this script is here. `transform` means THIS object.
//     Camera Offset
//       Main Camera           <- the headset. In VR the camera is the player's head.
//       Left Controller       <- the left hand
//       Right Controller      <- the right hand
//
// Unity moves those three children for you, every frame, to match the real headset and the
// real controllers. You never set their positions. You only READ them.
//
// What you move is the XR Origin. Move the parent and the head and both hands come along,
// the same way a passenger moves when the car does. That is what flying is here.
//
// TRAP: do not try to fly by moving the Main Camera. The headset writes the camera's position
// every frame, so whatever you write gets overwritten before you ever see it.

namespace Assignment3
{
    public class Flying : MonoBehaviour
    {
        // The three things Unity tracks. These slots are already filled in for you.
        // Select "XR Origin (XR Rig)" in the Hierarchy and look at this component in the
        // Inspector: each slot shows which object it points at. Click a slot and Unity
        // highlights that object.
        public Transform leftController;
        public Transform rightController;
        public Transform cameraPos;          // the head (the Main Camera)

        // Flying speed. It is public, so it shows up in the Inspector and you can change it
        // while the game is running. Find a value that feels good, then type it in here.
        public float speed = 2f;

        // Two arrows for you to fill in each frame: head -> left hand, and head -> right hand.
        // A Vector3 can be a place (x, y, z) or an arrow (how far along x, y and z). Same type,
        // two meanings. `position` is a place. These two are arrows.
        private Vector3 leftDir, rightDir;

        void Update()
        {
            // complete the script
            //
            // Questions to answer, in order. Each one is a line or two of code.
            //
            // 1. Where is the left hand, measured from the head?
            //    You have two places: cameraPos.position and leftController.position.
            //    What do you do with two places to get the arrow from one to the other?
            //    Put it in leftDir.
            //
            // 2. The same question for the right hand. Put it in rightDir.
            //
            // 3. You have two arrows and you want one flying direction.
            //    How do you combine two arrows into one?
            //
            // 4. Move the XR Origin (transform) along that direction.
            //    Remember the red cube and the green cube in Lifecycle Lab scene 3. Update runs
            //    once per FRAME, and frames are not all the same length. Which of those two
            //    cubes do you want the player to be?
            //
            // Check your work: Debug.Log(leftDir) prints the arrow to the Console every frame.
            // Hold your left hand above your head. Which of the three numbers is positive?
        }
    }
}
