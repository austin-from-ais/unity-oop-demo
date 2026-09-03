using System.Collections;
using UnityEngine;

// SCENE 5 - POLYMORPHISM
// Hovers, bobs, keeps its distance. Falls when killed.

namespace Lecture05
{
    public class FlyingEnemy : Enemy
    {
        [Header("Flying")]
        [SerializeField] private float hoverHeight  = 1.8f;
        [SerializeField] private float bobAmount    = 0.25f;
        [SerializeField] private float bobSpeed     = 2f;
        [SerializeField] private float keepDistance = 3f;

        public override void Move()
        {
            var pos = transform.position;
            pos.y = hoverHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmount;

            if (Player != null)
            {
                float dist = FlatDistanceTo(Player.position);

                if (dist < aggroRange && dist > keepDistance)
                {
                    var dir = Flat(Player.position - transform.position).normalized;
                    pos += dir * moveSpeed * Time.deltaTime;
                }

                FaceToward(Player.position);
            }

            transform.position = pos;
        }

        protected override void OnDied()
        {
            StartCoroutine(FallToGround());
        }

        IEnumerator FallToGround()
        {
            while (transform.position.y > 0.01f)
            {
                var pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, 0f, 6f * Time.deltaTime);
                transform.position = pos;
                yield return null;
            }
        }
    }
}
