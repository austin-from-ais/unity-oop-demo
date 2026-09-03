using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// THIS FILE IS A WARNING, NOT AN EXAMPLE.
//
// It compiles. Nothing in the scene uses it. It shows what happens if you keep answering
// "add a feature" with "add a subclass" after scene 5.
//
// Two ways to move. Two ways to attack. Shield or no shield. That's 2 x 2 x 2 = 8 classes,
// and the same Shoot() and shield code is pasted into four of them each, because C# only lets
// a class have ONE parent. Add "boss" and it's 16. Add "poisonous" and it's 32.
//
// Scene 1's copy-paste problem is back, wearing a tie.
//
// (These are plain classes, not MonoBehaviours, so they don't clutter the Add Component menu.)

namespace Lecture06.Explosion
{
    abstract class Enemy
    {
        public int health = 100;
        public abstract void Move();
    }

    // ---- movement: two kinds ----

    class GroundEnemy : Enemy
    {
        public override void Move() { Debug.Log("walk toward player"); }
    }

    class FlyingEnemy : Enemy
    {
        public override void Move() { Debug.Log("hover toward player"); }
    }

    // ---- add shooting: one copy per movement kind ----

    class GroundShootingEnemy : GroundEnemy
    {
        public float cooldown = 2f;
        public void Shoot()
        {
            Debug.Log("spawn projectile, aim at player, start cooldown");
        }
    }

    class FlyingShootingEnemy : FlyingEnemy
    {
        public float cooldown = 2f;
        public void Shoot()
        {
            Debug.Log("spawn projectile, aim at player, start cooldown");   // copy #2. can't inherit from both.
        }
    }

    // ---- add shields: one copy per everything above ----

    class GroundShieldedEnemy : GroundEnemy
    {
        public int shield = 30;
        public int Absorb(int amount) { int a = Mathf.Min(shield, amount); shield -= a; return amount - a; }
    }

    class FlyingShieldedEnemy : FlyingEnemy
    {
        public int shield = 30;
        public int Absorb(int amount) { int a = Mathf.Min(shield, amount); shield -= a; return amount - a; }   // copy #2
    }

    class GroundShootingShieldedEnemy : GroundShootingEnemy
    {
        public int shield = 30;
        public int Absorb(int amount) { int a = Mathf.Min(shield, amount); shield -= a; return amount - a; }   // copy #3
    }

    class FlyingShootingShieldedEnemy : FlyingShootingEnemy
    {
        public int shield = 30;
        public int Absorb(int amount) { int a = Mathf.Min(shield, amount); shield -= a; return amount - a; }   // copy #4
    }

    // Now the designer asks for a shielded boss that flies and shoots, and a ground one that
    // doesn't shoot but is poisonous. Where do they go? How many files do you touch to fix a
    // bug in Absorb()?
    //
    // Compare: in the real scene, "flying shooting shielded" is a GameObject with
    // FlyingMover + Shooter + Shield + Health on it. Zero new classes.
}
