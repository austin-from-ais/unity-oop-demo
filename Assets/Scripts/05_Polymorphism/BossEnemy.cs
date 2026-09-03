using UnityEngine;

// SCENE 5 - POLYMORPHISM
//
// A boss is a ground enemy (note the parent: GroundEnemy, not Enemy) that stands still until
// you get close, then charges, then has to catch its breath.
//
// It's a grandchild: BossEnemy -> GroundEnemy -> Enemy -> MonoBehaviour.
// When it's not charging it just calls base.Move(), the GroundEnemy version. Reuse, not copy-paste.

namespace Lecture05
{
    public class BossEnemy : GroundEnemy
    {
        [Header("Boss")]
        [SerializeField] private float chargeRange     = 5f;
        [SerializeField] private float chargeSpeed     = 7f;
        [SerializeField] private float chargeSeconds   = 0.7f;
        [SerializeField] private float recoverSeconds  = 2f;

        float chargeUntil;
        float recoverUntil;

        public override void Move()
        {
            if (Player == null) { base.Move(); return; }

            // catching its breath: stand there, face the player, do nothing else
            if (Time.time < recoverUntil)
            {
                Animator.SetBool("Walking", false);
                FaceToward(Player.position);
                return;
            }

            // mid-charge: go fast in a straight line
            if (Time.time < chargeUntil)
            {
                var dir = Flat(Player.position - transform.position).normalized;
                transform.position += dir * chargeSpeed * Time.deltaTime;
                Animator.SetBool("Walking", true);

                if (FlatDistanceTo(Player.position) <= stopDistance)
                {
                    chargeUntil  = 0f;
                    recoverUntil = Time.time + recoverSeconds;
                    Animator.SetTrigger("Attack");      // the roar
                }
                return;
            }

            // close enough to start a charge?
            float dist = FlatDistanceTo(Player.position);
            if (dist < chargeRange && dist > stopDistance)
            {
                chargeUntil = Time.time + chargeSeconds;
                return;
            }

            // otherwise behave like any ground enemy
            base.Move();
        }
    }
}
