using UnityEngine;

// SIGIL CASTERS, PART 4e - THE ESCAPE ROOM PAYOFF
//
// A door that slides up once a particular block has been powered. This is the whole "escape
// room" in one script: a puzzle is just a thing in the room that watches another thing.
//
// Notice what this script does NOT know about: spells, sigils, rays, controllers. It knows one
// SpellTarget and one bool on it. You could power the generator from a keyboard key, a timer or
// a second door, and this script would open exactly the same way.
//
// SIMPLE vs ROBUST:
//   - It asks "are you powered yet?" every single frame (polling). Cheap for one door, silly for
//     a thousand. The robust version subscribes to an OnPowered event on the SpellTarget and is
//     told once.
//   - It moves by sliding the Transform. If the door had a Rigidbody, the robust version would
//     move it through the physics system so it pushed things out of the way properly.

namespace SigilCasters
{
    public class PoweredDoor : MonoBehaviour
    {
        [Tooltip("The block that has to be powered (hit with the Electric spell) for this door to open.")]
        public SpellTarget generator;

        [Tooltip("How far up the door slides, in metres.")]
        public float openHeight = 2.4f;

        [Tooltip("How fast it slides, in metres per second.")]
        public float speed = 1f;

        Vector3 closedPosition;   // remembered on Awake, so 'open' can be measured from it

        void Awake() => closedPosition = transform.position;

        void Update()
        {
            if (!generator.IsPowered) return;   // not yet. Ask again next frame.

            // MoveTowards: step at most speed * deltaTime closer to the target, and stop exactly
            // on it. No overshoot, and no need for an "is it open yet" flag.
            Vector3 openPosition = closedPosition + Vector3.up * openHeight;
            transform.position = Vector3.MoveTowards(transform.position, openPosition, speed * Time.deltaTime);
        }
    }
}
