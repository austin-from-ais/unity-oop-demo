using UnityEngine;

// SIGIL CASTERS, PART 4d - A BLOCK THAT SPELLS CAN LAND ON
//
// This is what makes a block a target. A wall has a collider, so the ray stops on it, but it
// has no SpellTarget, so the spell fizzles. Put this component on a block and it joins the game.
//
// What it is: a bag of parts (Scene 6, Composition). The block is a GameObject with a Renderer
// (what it looks like), a Rigidbody (how it moves) and a Collider (what the ray can hit). This
// script finds those parts once, in Awake, and holds them out for spells to use. It also holds
// one piece of its own state: whether it has been powered.
//
// Why not let spells call GetComponent themselves? They could. But then every spell repeats the
// same lookup, and a block with no Rigidbody would break at cast time instead of at Play time.
// Here, RequireComponent makes a block without the right parts impossible to build.
//
// SIMPLE vs ROBUST:
//   - Powered is a bool with a one-way switch. The robust version is an event ("OnPowered") so
//     the door is told once, instead of asking every frame (see PoweredDoor.cs).
//   - The properties have private setters (Scene 3, Encapsulation): anyone can READ Body, Skin
//     and IsPowered, only this script can change them. Public fields would be a word shorter,
//     and would let any script anywhere quietly swap the block's Rigidbody for another one.

namespace SigilCasters
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Renderer))]
    public class SpellTarget : MonoBehaviour
    {
        /// How the block moves. LevitateSpell uses this.
        public Rigidbody Body { get; private set; }

        /// What the block looks like. ElectricSpell uses this.
        public Renderer Skin { get; private set; }

        /// Has an ElectricSpell hit this block? PoweredDoor watches this.
        public bool IsPowered { get; private set; }

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Skin = GetComponent<Renderer>();
        }

        /// The one way to set IsPowered. A method, not a public setter, so there is a place to
        /// put a Debug.Log, a sound, or a sanity check later without touching the callers.
        public void Power()
        {
            IsPowered = true;
            Debug.Log("[Spell Target] " + gameObject.name + " is now powered.");
        }
    }
}
