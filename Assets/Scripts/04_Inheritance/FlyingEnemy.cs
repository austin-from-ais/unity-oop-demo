using UnityEngine;

// SCENE 4 - INHERITANCE
//
// A FlyingEnemy IS an Enemy. It has health, a health bar, TakeDamage, a name, all of it,
// and none of that is written in this file. It's inherited.
//
// This file only says what's DIFFERENT about a flying one: how it moves, and what happens
// when it dies (it drops out of the sky). Everything else is the parent's business.

namespace Lecture04
{
    public class FlyingEnemy : Enemy
    {
        [Header("Flying")]
        [SerializeField] private float hoverHeight  = 1.8f;
        [SerializeField] private float bobAmount    = 0.25f;
        [SerializeField] private float bobSpeed     = 2f;
        [SerializeField] private float keepDistance = 3f;    // hovers this far from the player

        /// `override` means: this replaces the parent's Move(). Enemy.Update() will call this one.
        protected override void Move()
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

        /// The parent's OnDied() does nothing. Ours drops the body to the ground.
        protected override void OnDied()
        {
            var pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
        }
    }
}
