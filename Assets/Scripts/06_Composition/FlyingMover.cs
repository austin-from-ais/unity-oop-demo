using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
// Hovers, bobs, keeps its distance, drops when whatever it's attached to dies.

namespace Lecture06
{
    public class FlyingMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed    = 1.5f;
        [SerializeField] private float aggroRange   = 9f;
        [SerializeField] private float keepDistance = 4f;
        [SerializeField] private float hoverHeight  = 1.8f;
        [SerializeField] private float bobAmount    = 0.25f;
        [SerializeField] private float bobSpeed     = 2f;
        [SerializeField] private float turnSpeed    = 360f;

        IDamageable self;
        Transform   player;

        void Awake()
        {
            self = GetComponent<IDamageable>();
        }

        void Start()
        {
            var hero = FindAnyObjectByType<Hero>();
            if (hero != null) player = hero.transform;
        }

        void Update()
        {
            var pos = transform.position;

            if (self != null && self.IsDead)
            {
                pos.y = Mathf.MoveTowards(pos.y, 0f, 6f * Time.deltaTime);   // fall
                transform.position = pos;
                return;
            }

            pos.y = hoverHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmount;

            if (player != null)
            {
                var to = player.position - transform.position;
                to.y = 0f;
                float dist = to.magnitude;

                if (dist < aggroRange && dist > keepDistance)
                    pos += to.normalized * moveSpeed * Time.deltaTime;

                if (to.sqrMagnitude > 0.0001f)
                {
                    var wanted = Quaternion.LookRotation(to, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, turnSpeed * Time.deltaTime);
                }
            }

            transform.position = pos;
        }
    }
}
