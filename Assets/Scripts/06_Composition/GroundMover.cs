using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Walks toward the player. That's it. It doesn't know whether it's on a skeleton, a boss, or a
// haunted crate. Put it next to a Health and a MeleeAttacker and you have a ground enemy.
// Put it next to a Health and a Shooter and you have a different one. No new class.

namespace Lecture06
{
    public class GroundMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed    = 1.5f;
        [SerializeField] private float aggroRange   = 8f;
        [SerializeField] private float stopDistance = 1.6f;
        [SerializeField] private float turnSpeed    = 360f;

        Animator    animator;
        IDamageable self;
        Transform   player;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            self     = GetComponent<IDamageable>();
        }

        void Start()
        {
            var hero = FindAnyObjectByType<Hero>();
            if (hero != null) player = hero.transform;
        }

        void Update()
        {
            if (player == null) return;
            if (self != null && self.IsDead) return;

            var   to    = player.position - transform.position;
            to.y = 0f;
            float dist  = to.magnitude;
            bool  chase = dist < aggroRange && dist > stopDistance;

            if (dist < aggroRange) FaceToward(to);
            if (chase) transform.position += to.normalized * moveSpeed * Time.deltaTime;

            if (animator != null) animator.SetBool("Walking", chase);
        }

        void FaceToward(Vector3 flatDir)
        {
            if (flatDir.sqrMagnitude < 0.0001f) return;
            var wanted = Quaternion.LookRotation(flatDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, turnSpeed * Time.deltaTime);
        }
    }
}
