using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sigils;

// SIGIL CASTERS, PART 2 - DRAWING A SIGIL IN THE AIR
//
// In one line: while the trigger is held, remember where the hand is every frame. When it is
// let go, flatten that 3D path onto an imaginary sheet of paper in front of the player and ask
// the $1 recogniser what shape it is.
//
// The recogniser (Scripts/Sigils/DollarOneRecognizer.cs) has NOT changed. In the Sigil Lab its
// points came from tracing a picture. Here they come from a controller. The recogniser cannot
// tell the difference, and that is exactly why it was written as its own plain C# class.
//
// A stroke has three moments, and the Input System has a question for each:
//
//   the trigger went down this frame   WasPressedThisFrame()    -> start a new stroke
//   the trigger is down                IsPressed()              -> add a point
//   the trigger came up this frame     WasReleasedThisFrame()   -> finish and recognise
//
// THE FLATTENING. $1 wants 2D points, (x, y) on a page. The hand gives 3D points, (x, y, z) in
// the room. So we have to decide what "the page" is. The simple answer: a sheet held up in
// front of the player's face at the moment they start drawing. Its sideways axis is the head's
// right, its up axis is the head's up. Then, for every 3D point:
//
//     x on the page = how far the point is along head-right  = Dot(point - head, headRight)
//     y on the page = how far the point is along head-up     = Dot(point - head, headUp)
//
// and the depth (towards or away from the face) is simply dropped. Dot(a, b), when b is one
// metre long, means "how much of a points along b". The whole flattening is two dot products.
//
// SIMPLE vs ROBUST:
//   - One point per frame, no filtering. At 72 fps a two-second stroke is about 144 points,
//     some of them on top of each other when the hand pauses. $1 resamples anyway, so this is
//     fine. The robust version only adds a point when the hand has moved at least a centimetre.
//   - The page is fixed when the trigger goes down. Turn your head mid-stroke and the page does
//     not follow. Good enough: nobody spins while drawing a circle.
//   - The result goes straight to one SpellCaster through a direct reference. The robust
//     version raises an event, so sound, particles or a scoreboard could listen too without
//     this script knowing they exist.
//   - A tiny stroke (an accidental tap of the trigger) is thrown out by point count, not length.
//   - We call Recognize, not RecognizeAnyStartOrDirection. A picture has no start point, so the
//     Sigil Lab had to try all of them. A real stroke starts where the trigger went down.

namespace SigilCasters
{
    public class SigilDrawer : MonoBehaviour
    {
        [Tooltip("The Main Camera. Where the player's eyes are. The page we draw on faces them.")]
        public Transform head;

        [Tooltip("The $1 recogniser component from the Sigil Lab, unchanged.")]
        public SigilRecognizer recognizer;

        [Tooltip("Who gets told what we drew. See SpellCaster.cs (Part 3).")]
        public SpellCaster caster;

        [Tooltip("Draws the stroke in the air so everyone can see it. A child of the controller.")]
        public LineRenderer trail;

        [Tooltip("The trigger, as an Input System action. The scene builder binds it to <XRController>{RightHand}/{TriggerButton}.")]
        public InputActionProperty drawButton;

        [Tooltip("Strokes with fewer points than this are ignored. Stops a tap of the trigger from casting something.")]
        public int minPoints = 10;

        // The stroke so far, in world space: (x, y, z) in the room.
        readonly List<Vector3> worldPoints = new List<Vector3>();

        // The page: where it is and which way its x and y axes point. Fixed when the trigger goes down.
        Vector3 pageOrigin, pageRight, pageUp;

        void Awake()
        {
            trail.useWorldSpace = true;   // we give it room positions, not positions relative to the hand
            trail.positionCount = 0;
        }

        void OnEnable()  => drawButton.action.Enable();
        void OnDisable() => drawButton.action.Disable();

        void Update()
        {
            if (drawButton.action.WasPressedThisFrame())  StartStroke();
            if (drawButton.action.IsPressed())            AddPoint();
            if (drawButton.action.WasReleasedThisFrame()) FinishStroke();
        }

        void StartStroke()
        {
            worldPoints.Clear();
            trail.positionCount = 0;

            // Hold up the page. Three arrows copied from the head RIGHT NOW, so the page stays
            // put for the rest of the stroke even if the head moves.
            pageOrigin = head.position;
            pageRight  = head.right;
            pageUp     = head.up;
        }

        void AddPoint()
        {
            // transform is the right controller; this script sits on it.
            worldPoints.Add(transform.position);

            // Redraw the trail. positionCount tells the LineRenderer how many points to expect.
            trail.positionCount = worldPoints.Count;
            trail.SetPosition(worldPoints.Count - 1, transform.position);
        }

        void FinishStroke()
        {
            if (worldPoints.Count < minPoints)
            {
                Debug.Log("[Sigil Drawer] Too short (" + worldPoints.Count + " points). Ignored.");
                return;
            }

            // 1. Flatten: room points -> page points. The two dot products from the header.
            var pagePoints = new List<Vector2>(worldPoints.Count);
            foreach (Vector3 p in worldPoints)
            {
                Vector3 fromHead = p - pageOrigin;
                float x = Vector3.Dot(fromHead, pageRight);
                float y = Vector3.Dot(fromHead, pageUp);
                pagePoints.Add(new Vector2(x, y));
            }

            // 2. Recognise. The same call the image tester made in the Sigil Lab.
            DollarOneRecognizer.Result result = recognizer.Recognize(pagePoints);

            // 3. Say what we saw: the whole leaderboard, so a near miss is visible in the Console.
            string report = "[Sigil Drawer] " + worldPoints.Count + " points -> " + result.Name
                          + " (" + result.Score.ToString("0.00") + ")";
            foreach (var m in result.Matches)
                report += "\n    " + m.Name.PadRight(10) + m.Score.ToString("0.00");
            Debug.Log(report);

            // 4. Only a confident match counts. Every scribble is closest to SOMETHING.
            if (!result.IsConfident(recognizer.confidenceThreshold))
            {
                Debug.Log("[Sigil Drawer] Not confident enough (needs " + recognizer.confidenceThreshold + "). No spell.");
                return;
            }

            // 5. Hand the NAME to Part 3. This script knows shapes. It knows nothing about spells.
            caster.Load(result.Name);
        }
    }
}
