using UnityEngine;
using UnityEngine.InputSystem;

// SCENE 3 - ENCAPSULATION
//
// Unchanged from scene 2. The hero was already going through TakeDamage().
// It's the *other* scripts that weren't.

namespace Lecture03
{
    public class Hero : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 3f;
        public float turnSpeed = 720f;

        [Header("Combat")]
        public int   damage         = 10;
        public float attackRange    = 1.6f;
        public float attackInterval = 1.2f;

        [Header("Wiring")]
        public Animator animator;
        public Camera   cam;

        Enemy   target;
        Vector3 destination;
        bool    hasDestination;
        float   nextAttackTime;

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
                if (target.IsDead)
                {
                    target = null;
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

        void HandleClick()
        {
            var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 200f)) return;

            var enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                target = enemy;
                hasDestination = false;
            }
            else
            {
                target = null;
                destination = hit.point;
                hasDestination = true;
            }
        }

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

        void TryAttack()
        {
            if (Time.time < nextAttackTime) return;

            nextAttackTime = Time.time + attackInterval;
            animator.SetTrigger("Attack");
            target.TakeDamage(damage);
        }
    }
}
