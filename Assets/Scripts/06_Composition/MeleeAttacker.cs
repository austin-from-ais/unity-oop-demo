using System.Collections;
using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Swings at the player when close. Asks for the player's IDamageable and calls TakeDamage.
// It doesn't know the player has a Health component. It doesn't need to.

namespace Lecture06
{
    public class MeleeAttacker : MonoBehaviour
    {
        [SerializeField] private int   damage   = 8;
        [SerializeField] private float range    = 1.9f;
        [SerializeField] private float interval = 2f;
        [SerializeField] private float hitDelay = 0.45f;

        Animator    animator;
        IDamageable self;
        Transform   player;
        IDamageable playerDamageable;
        float       nextAttack;

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
        }

        void Update()
        {
            if (player == null || playerDamageable == null || playerDamageable.IsDead) return;
            if (self != null && self.IsDead) return;

            var to = player.position - transform.position;
            to.y = 0f;
            if (to.magnitude > range) return;
            if (Time.time < nextAttack) return;

            nextAttack = Time.time + interval;
            if (animator != null) animator.SetTrigger("Attack");
            StartCoroutine(LandHit());
        }

        IEnumerator LandHit()
        {
            yield return new WaitForSeconds(hitDelay);
            if (self != null && self.IsDead) yield break;

            playerDamageable.TakeDamage(damage);
        }
    }
}
