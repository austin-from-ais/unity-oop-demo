using UnityEngine;

// SIGIL CASTERS, PART 4a - WHAT A SPELL IS
//
// A spell is two things: a name, and something it does to a block. That is the whole contract.
// Every concrete spell (LevitateSpell.cs, ElectricSpell.cs) fills in those two things and
// nothing else, and SpellCaster calls Apply() without knowing which one it is holding.
//
// Why a plain C# class and not a MonoBehaviour? A spell has no position, nothing to draw, no
// Update. It is an idea, not a thing in the scene. Same reasoning as DollarOneRecognizer.
// (Lifecycle Lab: if Unity does not need to call it every frame, it does not need to be a component.)
//
// Why abstract? So nobody can write `new Spell()`: there is no such thing as a spell in general,
// only particular spells. And so a concrete spell that forgets to write Apply() is a compile
// error, not a spell that silently does nothing. Scene 4 (Inheritance), Scene 5 (Polymorphism).
//
// SIMPLE vs ROBUST:
//   - An abstract class, not an interface. An interface (Scene 6) would let a MonoBehaviour be a
//     spell too. We do not need that, and "a LevitateSpell IS A Spell" reads well as a class.
//   - Apply() gets the whole SpellTarget and helps itself to the block's parts. The robust
//     version would ask the block first ("CanReceive(spell)?") so a stone block could refuse
//     to levitate.

namespace SigilCasters
{
    public abstract class Spell
    {
        /// Shown in the Console and in SpellCaster's Inspector.
        public abstract string Name { get; }

        /// Do the thing. The target is the block the ray landed on. See SpellTarget.cs for what
        /// a block hands over.
        public abstract void Apply(SpellTarget target);
    }
}
