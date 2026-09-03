using UnityEngine;

// SCENE 3 - ENCAPSULATION  (solution)
//
// `health` is private. The ONLY ways in are TakeDamage() and Heal(), and the clamp lives inside them.
// It is now impossible for health to be -50 or 999, no matter who writes the next script.
//
// About [SerializeField] private:
//   private          = other SCRIPTS can't touch it.
//   [SerializeField] = the INSPECTOR still can, and Unity still saves it in the scene/prefab.
// You want both: designers tune numbers in the Inspector, code goes through the gate.
// That's the whole reason every Unity tutorial writes it that way.

namespace Lecture03.Solution
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private string enemyName = "Skeleton";
        [SerializeField] private int    maxHealth = 100;
        [SerializeField] private int    defence   = 0;

        [Header("Wiring")]
        [SerializeField] private Animator  animator;
        [SerializeField] private HealthBar healthBar;

        // Not serialized, not public. Nobody outside this file can set it.
        private int health;

        // Read-only windows. Anyone can LOOK.
        public string EnemyName => enemyName;
        public int    Health    => health;
        public int    MaxHealth => maxHealth;
        public bool   IsDead    => health <= 0;

        void Start()
        {
            if (animator  == null) animator  = GetComponent<Animator>();
            if (healthBar == null) healthBar = GetComponent<HealthBar>();

            health = maxHealth;
            RefreshBar();
        }

        // ---- the gates ----

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (amount <= 0) return;                       // damage is never negative. use Heal().

            int dealt = Mathf.Max(0, amount - defence);
            health = Mathf.Clamp(health - dealt, 0, maxHealth);
            RefreshBar();

            if (IsDead)
                Die();
            else
                animator.SetTrigger("Hit");
        }

        public void Heal(int amount)
        {
            if (IsDead) return;                            // the dead stay dead
            if (amount <= 0) return;

            health = Mathf.Clamp(health + amount, 0, maxHealth);
            RefreshBar();
        }

        public void Kill()
        {
            if (IsDead) return;

            health = 0;
            RefreshBar();
            Die();
        }

        // ---- private helpers ----

        void RefreshBar()
        {
            healthBar.SetFraction((float)health / maxHealth);
        }

        void Die()
        {
            animator.SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
        }
    }
}
