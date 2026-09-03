using UnityEngine;

// SCENE 2 - CLASSES AND OBJECTS
//
// This file is the BLUEPRINT. Nothing in here is a skeleton.
// It says what every enemy HAS (fields) and what every enemy CAN DO (methods).
//
// Every GameObject this is attached to is an OBJECT built from the blueprint:
// same fields, its own values. Two skeletons, one class. That's an instance.
//
// Compare with Lecture01/SkeletonEnemy.cs: the fields are the same,
// but the bookkeeping that used to live inside the hero now lives here, in TakeDamage().

namespace Lecture02
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats  (the blueprint's fields)")]
        public string enemyName = "Skeleton";
        public int    maxHealth = 100;
        public int    defence   = 0;

        [Header("State  (changes while playing)")]
        public int health;

        [Header("Wiring")]
        public Animator  animator;
        public HealthBar healthBar;

        public bool IsDead => health <= 0;

        void Start()
        {
            if (animator  == null) animator  = GetComponent<Animator>();
            if (healthBar == null) healthBar = GetComponent<HealthBar>();

            health = maxHealth;
            healthBar.SetFraction(1f);
        }

        // ---- the blueprint's methods ----

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            int dealt = Mathf.Max(0, amount - defence);
            health = Mathf.Max(0, health - dealt);

            healthBar.SetFraction((float)health / maxHealth);

            if (IsDead)
                Die();
            else
                animator.SetTrigger("Hit");
        }

        void Die()
        {
            animator.SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
        }
    }
}
