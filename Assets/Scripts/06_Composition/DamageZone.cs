using System.Collections.Generic;
using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Scene 3's poison puddle, grown up. It hurts ANYTHING damageable standing in it:
// skeletons, the player, a crate. It never asks what they are.

namespace Lecture06
{
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private float radius        = 1.5f;
        [SerializeField] private int   damagePerTick = 5;
        [SerializeField] private float tickSeconds   = 1f;

        float nextTick;

        void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + tickSeconds;

            var hit = new HashSet<IDamageable>();
            foreach (var col in Physics.OverlapSphere(transform.position, radius))
            {
                var d = col.GetComponentInParent<IDamageable>();     // works with interfaces too
                if (d != null) hit.Add(d);
            }

            foreach (var d in hit)
                d.TakeDamage(damagePerTick);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
