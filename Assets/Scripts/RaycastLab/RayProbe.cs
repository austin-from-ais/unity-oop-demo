using UnityEngine;

// RAYCAST LAB 1 - A RAY, BY HAND.
//
// In one line: a raycast is a question you ask the physics engine.
//
//     "Start HERE. Go THAT WAY. What is the first collider you bump into within N metres?"
//
// That is the whole idea. Everything the XR Interaction Toolkit does later (pointing at
// buttons, grabbing from a distance, highlighting what you aim at) is this question, asked
// every frame, with some bookkeeping on top. So before we touch the toolkit, we ask it ourselves.
//
// The three ingredients:
//   origin        a point in the world. For us: where the controller is.
//   direction     which way to look. For us: the way the controller points (transform.forward).
//   max distance  give up after this many metres.
//
// The answer comes back in a RaycastHit. It tells you WHAT you hit (hit.collider), WHERE
// (hit.point), HOW FAR (hit.distance) and which way the surface faces there (hit.normal).
//
// One trap, and it is the big one: a ray only sees COLLIDERS. Not meshes, not what you can see.
// A cube with its collider removed is invisible to a ray. The ray goes straight through it.
// The scene has one of those. Aim at it.

namespace RaycastLab
{
    [RequireComponent(typeof(LineRenderer))]
    public class RayProbe : MonoBehaviour
    {
        [Tooltip("How far the ray reaches, in metres. Past this the ray reports nothing, even if there is a wall.")]
        public float maxDistance = 10f;

        [Tooltip("Which Unity layers the ray can see. 'Everything' for now. Lab 3 is about changing this.")]
        public LayerMask whatTheRaySees = ~0;   // ~0 is every bit set: every layer

        [Tooltip("A small object moved to the hit point, so you can see exactly where the ray landed.")]
        public Transform marker;

        [Tooltip("A floating label that prints what the ray is hitting.")]
        public TextMesh readout;

        public Color missColor = new Color(1f, 0.3f, 0.3f);
        public Color hitColor  = new Color(0.3f, 1f, 0.4f);

        LineRenderer line;      // draws the ray so we can see it. A ray has no visual of its own.
        string lastHitName;     // so we only print to the Console when the answer changes

        void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 2;      // a straight line: a start point and an end point
            line.useWorldSpace = true;   // we give it world positions, not positions relative to the hand
        }

        void Update()
        {
            // 1. The question. Where from, which way.
            Vector3 origin    = transform.position;
            Vector3 direction = transform.forward;

            // 2. Ask physics. Physics.Raycast returns true if it hit something and fills in 'hit'.
            //    If it returns false, do not read 'hit': the fields are just leftovers.
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(origin, direction, out hit, maxDistance, whatTheRaySees);

            // 3. Draw the line. Up to the hit point if there was one, otherwise the full length.
            Vector3 end = hitSomething ? hit.point : origin + direction * maxDistance;
            line.SetPosition(0, origin);
            line.SetPosition(1, end);
            line.startColor = line.endColor = hitSomething ? hitColor : missColor;

            // 4. Put the marker on the surface, facing out along the surface normal.
            if (marker != null)
            {
                marker.gameObject.SetActive(hitSomething);
                if (hitSomething)
                {
                    marker.position = hit.point;
                    marker.rotation = Quaternion.LookRotation(hit.normal);
                }
            }

            // 5. Report. hit.collider is the Collider we hit; its GameObject's name is the readable bit.
            string hitName = hitSomething ? hit.collider.gameObject.name : "(nothing)";
            string text = hitSomething
                ? hitName + "\n" + hit.distance.ToString("0.00") + " m"
                : "(nothing within " + maxDistance + " m)";

            if (readout != null) readout.text = text;

            if (hitName != lastHitName)
            {
                Debug.Log("[Ray By Hand] now hitting: " + hitName
                          + (hitSomething ? "  at " + hit.point + "  distance " + hit.distance.ToString("0.00") : ""));
                lastHitName = hitName;
            }

            // Also draw it in the Scene view (not the Game view). Handy when debugging rays you do not draw.
            Debug.DrawLine(origin, end, hitSomething ? hitColor : missColor);
        }
    }
}
