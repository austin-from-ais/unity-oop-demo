using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sigils;

// SIGIL CASTERS, PART 2 - DRAWING A SIGIL IN THE AIR
//
// In one line: while the trigger is held, remember where the hand is every frame. When it is
// let go, flatten that 3D path onto an imaginary sheet of paper in front of the player and ask
// the $1 recognizer what shape it is.
//
// The recognizer (Scripts/Sigils/DollarOneRecognizer.cs) has NOT changed. In the Sigil Lab its
// points came from tracing a picture. Here they come from a controller. The recognizer cannot
// tell the difference, and that is exactly why it was written as its own plain C# class.
//
// A stroke has three moments, and the Input System has a question for each:
//
//   the trigger went down this frame   WasPressedThisFrame()    -> start a new stroke
//   the trigger is down                IsPressed()              -> add a point
//   the trigger came up this frame     WasReleasedThisFrame()   -> finish and recognize
//
// THE FLATTENING, AND THE "PAGE" METAPHOR. $1 wants 2D points: (x, y), like pen on paper. The
// hand gives 3D points: (x, y, z) in the room. To get from one to the other we have to pick a
// flat surface and squash the stroke onto it, and we have to decide WHICH surface.
//
// "Page" is the metaphor this script reaches for. Imagine a sheet of paper held up in front of
// the player's face at the moment the trigger goes down. It is NOT a Unity object; nothing is
// drawn on it; you will not find it in the Hierarchy. It is three stored arrows (pageOrigin,
// pageRight, pageUp) and a way of thinking about what they mean: where the sheet is, which way
// its x axis points (the head's right), which way its y axis points (the head's up).
//
// Why the head and not the room's walls? Because the player can face any direction. Flatten
// onto a wall and a circle drawn by someone facing that wall squashes to a line. A sheet that
// always faces the player catches whatever they draw in front of themselves. Then, for every
// 3D point:
//
//     x on the page = how far the point is along head-right  = Dot(point - head, headRight)
//     y on the page = how far the point is along head-up     = Dot(point - head, headUp)
//
// and the depth (towards or away from the face) is simply dropped. Dot(a, b), when b is one
// meter long, means "how much of a points along b". The whole flattening is two dot products.
//
// SIMPLE vs ROBUST:
//   - One point per frame, no filtering. At 72 fps a two-second stroke is about 144 points,
//     some of them on top of each other when the hand pauses. $1 resamples anyway, so this is
//     fine. The robust version only adds a point when the hand has moved at least a centimeter.
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

        [Tooltip("The $1 recognizer component from the Sigil Lab, unchanged.")]
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

        // The "page" (see the header): the imaginary sheet we flatten the stroke onto. Where it
        // is, which way its x axis points, which way its y axis points. Fixed when the trigger
        // goes down. Not public, so not in the Inspector: nothing outside this script needs them.
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

            // Hold up the "page": copy three arrows from the head RIGHT NOW, so the imaginary
            // sheet stays put for the rest of the stroke even if the head moves.
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

            // 2. Recognize. The same call the image tester made in the Sigil Lab.
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
