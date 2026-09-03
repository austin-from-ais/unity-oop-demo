using System;
using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// This is the health logic from Enemy, pulled out into its own component.
// It is not an enemy. It is not a player. It's just "has hit points". Put it on anything.
//
// The player has one. Every skeleton has one. The crate does NOT (open BreakableCrate.cs to
// see why). Nothing here knows what it's attached to.

namespace Lecture06
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;

        [Header("Optional wiring (found automatically if left empty)")]
        [SerializeField] private Animator  animator;
        [SerializeField] private HealthBar healthBar;

        private int health;

        public int  Current => health;
        public int  Max     => maxHealth;
        public bool IsDead  => health <= 0;

        /// Fired once, when health hits zero. Other components on this object can listen.
        public event Action Died;

        void Awake()
        {
            if (animator  == null) animator  = GetComponentInChildren<Animator>();
            if (healthBar == null) healthBar = GetComponent<HealthBar>();
        }

        void Start()
        {
            health = maxHealth;
            RefreshBar();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            // Ask every modifier sitting next to us. Shield? Armour? Both? Don't know, don't care.
            foreach (var modifier in GetComponents<IDamageModifier>())
                amount = modifier.Modify(amount);

            if (amount <= 0) return;

            health = Mathf.Clamp(health - amount, 0, maxHealth);
            RefreshBar();

            if (IsDead)
                Die();
            else if (animator != null)
                animator.SetTrigger("Hit");
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            health = Mathf.Clamp(health + amount, 0, maxHealth);
            RefreshBar();
        }

        void Die()
        {
            if (animator != null) animator.SetTrigger("Die");

            foreach (var col in GetComponentsInChildren<Collider>())
                col.enabled = false;

            Died?.Invoke();
        }

        void RefreshBar()
        {
            if (healthBar != null) healthBar.SetFraction((float)health / maxHealth);
        }
    }
}
