using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// A crate. It is not an Enemy. It has no Health component. It has no health at all: it breaks
// after three hits from anything, no matter how hard.
//
// It still keeps the IDamageable promise, so the sword, the poison, and a mage's fireball can
// all hit it, and none of them know it's a crate. That's what an interface buys you that a
// parent class can't: things with nothing in common can still be treated the same way.

namespace Lecture06
{
    public class BreakableCrate : MonoBehaviour, IDamageable
    {
        [SerializeField] private int hitsToBreak = 3;

        private int hits;

        public bool IsDead => hits >= hitsToBreak;

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            hits++;                                   // the amount is ignored. it's a crate.

            if (IsDead)
                Destroy(gameObject);
            else
                transform.localScale *= 0.85f;        // shrink a little so the hit is visible
        }
    }
}
