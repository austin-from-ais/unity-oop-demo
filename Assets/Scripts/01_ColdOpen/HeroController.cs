using UnityEngine;
using UnityEngine.InputSystem;

// SCENE 1 - COLD OPEN
//
// Click the ground: the hero walks there.
// Click the skeleton: the hero walks up to it and swings every attackInterval seconds until it's dead.
//
// Read LandHit() carefully. The hero reaches into the skeleton and edits its health, plays ITS
// animations, updates ITS health bar, and turns off ITS collider. The skeleton has no say.

namespace Lecture01
{
    public class HeroController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 3f;
        public float turnSpeed = 720f;   // degrees per second

        [Header("Combat")]
        public int   damage         = 10;
        public float attackRange    = 1.6f;
        public float attackInterval = 1.2f;  // seconds between swings

        [Header("Wiring")]
        public Animator animator;
        public Camera   cam;

        SkeletonEnemy target;
        Vector3       destination;
        bool          hasDestination;
        float         nextAttackTime;

        void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (cam == null)      cam      = Camera.main;
        }

        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                HandleClick();

            bool walking = false;

            if (target != null)
            {
                if (target.health <= 0)
                {
                    target = null; // it's dead, stop caring about it
                }
                else
                {
                    var toTarget = Flat(target.transform.position - transform.position);

                    if (toTarget.magnitude > attackRange)
                    {
                        walking = MoveToward(target.transform.position);
                    }
                    else
                    {
                        FaceToward(target.transform.position);
                        TryAttack();
                    }
                }
            }
            else if (hasDestination)
            {
                walking = MoveToward(destination);
                if (!walking) hasDestination = false;
            }

            animator.SetBool("Walking", walking);
        }

        // ------------------------------------------------------------------ input

        void HandleClick()
        {
            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 200f)) return;

            var skeleton = hit.collider.GetComponentInParent<SkeletonEnemy>();
            if (skeleton != null && skeleton.health > 0)
            {
                target = skeleton;
                hasDestination = false;
            }
            else
            {
                target = null;
                destination = hit.point;
                hasDestination = true;
            }
        }

        // --------------------------------------------------------------- movement

        /// Returns true while still moving.
        bool MoveToward(Vector3 point)
        {
            var to = Flat(point - transform.position);
            if (to.magnitude < 0.05f) return false;

            FaceToward(point);
            transform.position += to.normalized * walkSpeed * Time.deltaTime;
            return true;
        }

        void FaceToward(Vector3 point)
        {
            var to = Flat(point - transform.position);
            if (to.sqrMagnitude < 0.0001f) return;

            var wanted = Quaternion.LookRotation(to, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, turnSpeed * Time.deltaTime);
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        // ----------------------------------------------------------------- combat

        void TryAttack()
        {
            if (Time.time < nextAttackTime) return;

            nextAttackTime = Time.time + attackInterval;
            animator.SetTrigger("Attack");
            LandHit(target);
        }

        void LandHit(SkeletonEnemy skeleton)
        {
            if (skeleton == null || skeleton.health <= 0) return;

            // ---- the hero does the skeleton's bookkeeping for it ----
            int dealt = Mathf.Max(0, damage - skeleton.defence);
            skeleton.health -= dealt;

            skeleton.healthBar.SetFraction((float)skeleton.health / skeleton.maxHealth);

            if (skeleton.health <= 0)
            {
                skeleton.health = 0;
                skeleton.animator.SetTrigger("Die");
                skeleton.GetComponent<Collider>().enabled = false;
            }
            else
            {
                skeleton.animator.SetTrigger("Hit");
            }
        }
    }
}
