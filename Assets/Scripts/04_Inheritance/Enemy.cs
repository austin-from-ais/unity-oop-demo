using UnityEngine;

// SCENE 4 - INHERITANCE
//
// Enemy is now a PARENT class. FlyingEnemy (in the next file) is a CHILD of it:
//
//     public class FlyingEnemy : Enemy
//
// The child gets everything in this file for free: health, damage, the health bar, dying.
// It replaces exactly one thing, the thing it does differently: how it moves.
//
// Look at Update() below. Enemy calls Move() every frame. It does not know or care which
// version of Move() runs. On a plain Enemy, the Move() in this file runs. On a FlyingEnemy,
// the override in FlyingEnemy.cs runs instead. The parent calls, the child answers.
//
// Now look at the top of ANY script you have ever written:
//
//     public class Whatever : MonoBehaviour
//
// Same colon. Same idea. MonoBehaviour is the parent, your script is the child, and
// Start() / Update() are MonoBehaviour's version of Move(): hooks the parent calls that you
// fill in. You have been inheriting since your very first script.

namespace Lecture04
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private string enemyName = "Skeleton";
        [SerializeField] private int    maxHealth = 100;
        [SerializeField] private int    defence   = 0;

        [Header("Movement")]
        [SerializeField] protected float moveSpeed    = 1.5f;   // protected: children can see it, outsiders can't
        [SerializeField] protected float aggroRange   = 7f;
        [SerializeField] protected float stopDistance = 1.6f;
        [SerializeField] protected float turnSpeed    = 360f;

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

        void Update()
        {
            if (IsDead) return;

            Move();     // <-- the parent calls. whichever child this is decides what happens.
        }

        // ------------------------------------------------------------ movement

        /// Default movement: walk at the player when they're close, stop at arm's length.
        /// `virtual` means: children are allowed to replace this.
        protected virtual void Move()
        {
            if (Player == null) { Animator.SetBool("Walking", false); return; }

            float dist  = FlatDistanceTo(Player.position);
            bool  chase = dist < aggroRange && dist > stopDistance;

            if (chase)
            {
                FaceToward(Player.position);
                var dir = Flat(Player.position - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
            else if (dist <= stopDistance)
            {
                FaceToward(Player.position);
            }

            Animator.SetBool("Walking", chase);
        }

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

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            health = Mathf.Clamp(health + amount, 0, maxHealth);
            RefreshBar();
        }

        void Die()
        {
            animator.SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
            OnDied();   // another hook. children can react to dying without touching this method.
        }

        /// Called once when this enemy dies. Does nothing by default. Children may override.
        protected virtual void OnDied() { }

        void RefreshBar()
        {
            healthBar.SetFraction((float)health / maxHealth);
        }
    }
}
