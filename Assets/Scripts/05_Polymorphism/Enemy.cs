using UnityEngine;

// SCENE 5 - POLYMORPHISM
//
// Enemy is now ABSTRACT. You can't make a plain Enemy any more; you can only make one of its
// children (GroundEnemy, FlyingEnemy, BossEnemy). And Move() is abstract: no default at all.
// Every child MUST write its own.
//
// Also: there's no Update() in here any more. Nobody in this file calls Move().
// Open Horde.cs to see who does, and how it manages to without knowing what anything is.

namespace Lecture05
{
    public abstract class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private string enemyName = "Skeleton";
        [SerializeField] private int    maxHealth = 100;
        [SerializeField] private int    defence   = 0;

        [Header("Movement")]
        [SerializeField] protected float moveSpeed  = 1.5f;
        [SerializeField] protected float aggroRange = 8f;
        [SerializeField] protected float turnSpeed  = 360f;

        [Header("Wiring")]
        [SerializeField] private Animator  animator;
        [SerializeField] private HealthBar healthBar;

        private int health;

        public string EnemyName => enemyName;
        public int    Health    => health;
        public int    MaxHealth => maxHealth;
        public bool   IsDead    => health <= 0;

        protected Transform Player   { get; private set; }
        protected Animator  Animator => animator;

        void Start()
        {
            if (animator  == null) animator  = GetComponent<Animator>();
            if (healthBar == null) healthBar = GetComponent<HealthBar>();

            var hero = FindAnyObjectByType<Hero>();
            if (hero != null) Player = hero.transform;

            health = maxHealth;
            RefreshBar();
        }

        /// Every kind of enemy moves. HOW is up to the kind. Public, because the Horde calls it.
        public abstract void Move();

        // ------------------------------------------------------------ helpers for children

        protected void FaceToward(Vector3 point)
        {
            var to = Flat(point - transform.position);
            if (to.sqrMagnitude < 0.0001f) return;

            var wanted = Quaternion.LookRotation(to, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, turnSpeed * Time.deltaTime);
        }

        protected float FlatDistanceTo(Vector3 point) => Flat(point - transform.position).magnitude;

        protected static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        // -------------------------------------------------------------- damage

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            int dealt = Mathf.Max(0, amount - defence);
            health = Mathf.Clamp(health - dealt, 0, maxHealth);
            RefreshBar();

            if (IsDead)
                Die();
            else
                animator.SetTrigger("Hit");
        }

        void Die()
        {
            animator.SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
            OnDied();
        }

        protected virtual void OnDied() { }

        void RefreshBar()
        {
            healthBar.SetFraction((float)health / maxHealth);
        }
    }
}
