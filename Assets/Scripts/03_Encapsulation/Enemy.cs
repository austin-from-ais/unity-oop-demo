using UnityEngine;

// SCENE 3 - ENCAPSULATION  (starting state: the bug is live)
//
// This is the Enemy from scene 2. It has a nice TakeDamage() method with a clamp in it.
// So why is a skeleton standing there with -50 health and a full bar?
//
// Because `health` is public. Three other scripts in this folder edit it directly and skip
// TakeDamage entirely. Find them. (Hint: the compiler will find them for you the moment
// you make health private.)

namespace Lecture03
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        public string enemyName = "Skeleton";
        public int    maxHealth = 100;
        public int    defence   = 0;

        [Header("State")]
        public int health;              // <-- anyone can write to this. anyone.

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
