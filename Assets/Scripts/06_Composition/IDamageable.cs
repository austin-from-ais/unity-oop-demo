// SCENE 6 - INTERFACES AND COMPOSITION
//
// An interface is a promise with no code in it. Anything that says
//
//     class Something : MonoBehaviour, IDamageable
//
// is promising: "I have a TakeDamage method and an IsDead property." That's all.
// It says nothing about health, health bars, animations, or what the thing IS.
//
// In this scene the player, every enemy, and a wooden crate all keep this promise, and none of
// them share a parent class. The sword doesn't care. It asks for IDamageable and swings.

namespace Lecture06
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
        bool IsDead { get; }
    }
}
