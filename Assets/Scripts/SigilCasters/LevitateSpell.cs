using UnityEngine;

// SIGIL CASTERS, PART 4b - LEVITATE
//
// Switch gravity off on the block and give it a nudge upwards. With some air resistance
// (damping) it rises, slows, and hangs there.
//
// Everything in Apply() is plain Rigidbody. Nothing here knows about VR, rays or sigils. These
// three lines would do the same thing in any Unity project.
//
// SIMPLE vs ROBUST:
//   - It never comes back down. Cast it again and the block just gets another nudge. The robust
//     version remembers the block and restores gravity after N seconds (a coroutine, or a
//     timer in Update on the SpellTarget).
//   - Fixed numbers. The robust version reads the strength from a settings asset.

namespace SigilCasters
{
    public class LevitateSpell : Spell
    {
        public override string Name => "Levitate";

        public override void Apply(SpellTarget target)
        {
            Rigidbody body = target.Body;

            body.useGravity     = false;             // stop falling
            body.linearDamping  = 2f;                // air resistance: the nudge dies out instead of carrying on forever
            body.linearVelocity = Vector3.up * 2f;   // the nudge: 2 meters per second, straight up
        }
    }
}
