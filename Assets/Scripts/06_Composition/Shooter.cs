using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Lobs a projectile at the player from range. Works on a flying mage, a ground skeleton,
// a turret, a crate. It's a behaviour, not a kind of enemy.

namespace Lecture06
{
    public class Shooter : MonoBehaviour
    {
        [SerializeField] private int   damage          = 6;
        [SerializeField] private float range           = 9f;
        [SerializeField] private float interval        = 2.5f;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float muzzleHeight    = 1.3f;

        Animator    animator;
        IDamageable self;
        Transform   player;
        IDamageable playerDamageable;
        float       nextShot;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            self     = GetComponent<IDamageable>();
        }

        void Start()
        {
            var hero = FindAnyObjectByType<Hero>();
            if (hero == null) return;

            player           = hero.transform;
            playerDamageable = hero.GetComponent<IDamageable>();
            nextShot         = Time.time + Random.Range(0f, interval);   // so a group doesn't fire in lockstep
        }

        void Update()
        {
            if (player == null || playerDamageable == null || playerDamageable.IsDead) return;
            if (self != null && self.IsDead) return;

            var to = player.position - transform.position;
            to.y = 0f;
            if (to.magnitude > range) return;
            if (Time.time < nextShot) return;

            nextShot = Time.time + interval;
            if (animator != null) animator.SetTrigger("Attack");

            var from = transform.position + Vector3.up * muzzleHeight + transform.forward * 0.5f;
            Projectile.Spawn(from, player, playerDamageable, damage, projectileSpeed);
        }
    }
}
