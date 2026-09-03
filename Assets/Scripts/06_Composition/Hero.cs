using UnityEngine;
using UnityEngine.InputSystem;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Two changes from scene 5.
//
// 1. The hero attacks IDamageable, not Enemy. Look at HandleClick(): GetComponentInParent<IDamageable>()
//    finds a skeleton's Health, a crate's BreakableCrate, anything that made the promise.
//    The hero has never heard of either class.
//
// 2. The hero can be hurt. Not because Hero inherits from something with health, but because the
//    Adventurer GameObject also carries a Health component. The hero HAS health; it isn't a kind of health.
//    That's composition.

namespace Lecture06
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

        IDamageable ownHealth;          // whatever on this object can be damaged. probably a Health.
        IDamageable target;
        Transform   targetTransform;
        Vector3     destination;
        bool        hasDestination;
        float       nextAttackTime;

        void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (cam == null)      cam      = Camera.main;
            ownHealth = GetComponent<IDamageable>();
        }

        void Update()
        {
            if (ownHealth != null && ownHealth.IsDead)
            {
                animator.SetBool("Walking", false);
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                HandleClick();

            bool walking = false;

            if (target != null)
            {
                if (target.IsDead || targetTransform == null)
                {
                    target = null;
                }
                else
                {
                    var toTarget = Flat(targetTransform.position - transform.position);

                    if (toTarget.magnitude > attackRange)
                    {
                        walking = MoveToward(targetTransform.position);
                    }
                    else
                    {
                        FaceToward(targetTransform.position);
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

            var damageable = hit.collider.GetComponentInParent<IDamageable>();   // <-- an interface, not a class
            var asComponent = damageable as Component;                            // every implementor here is a component

            bool clickedSelf = asComponent != null && asComponent.transform.IsChildOf(transform);

            if (damageable != null && !damageable.IsDead && !clickedSelf)
            {
                target          = damageable;
                targetTransform = asComponent.transform;
                hasDestination  = false;
            }
            else
            {
                target          = null;
                destination     = hit.point;
                hasDestination  = true;
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
            target.TakeDamage(damage);   // skeleton, crate, whatever. same line.
        }
    }
}
