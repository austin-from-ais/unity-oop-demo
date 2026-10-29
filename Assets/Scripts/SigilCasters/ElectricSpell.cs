using UnityEngine;

// SIGIL CASTERS, PART 4c - ELECTRIC
//
// Turn the block yellow and mark it as powered. Something else in the room (PoweredDoor.cs) is
// watching for exactly that mark.
//
// Compare with LevitateSpell.cs: same Name, same Apply(SpellTarget), completely different body.
// SpellCaster calls both with the same line of code. That is polymorphism, and it is the reason
// the caster does not need an if/else per spell.
//
// SIMPLE vs ROBUST:
//   - Recolouring with .material.color. '.material' quietly gives this block its own copy of the
//     material, which is what we want: only THIS block turns yellow. '.sharedMaterial' would
//     recolour every block that uses it. The robust version has an emissive "glow" material
//     made in advance and swaps it in.
//   - No sparks, no sound, no timer. Powered is forever.

namespace SigilCasters
{
    public class ElectricSpell : Spell
    {
        public override string Name => "Electric";

        static readonly Color ElectricYellow = new Color(1f, 0.9f, 0.2f);

        public override void Apply(SpellTarget target)
        {
            target.Skin.material.color = ElectricYellow;   // the visible effect
            target.Power();                                // the state other things can react to
        }
    }
}
