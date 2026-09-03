using UnityEngine;

// SCENE 5 - POLYMORPHISM
// Walks at the player, stops at arm's length.

namespace Lecture05
{
    public class GroundEnemy : Enemy
    {
        [Header("Ground")]
        [SerializeField] protected float stopDistance = 1.6f;

        public override void Move()
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
    }
}
