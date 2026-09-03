using System.Collections.Generic;
using UnityEngine;

// SCENE 3 - ENCAPSULATION  (starting state)
//
// A poison puddle. Every tick, any enemy standing in it loses some health.
// Written by "someone else on the team". Looks reasonable. Ships.

namespace Lecture03
{
    public class PoisonCloud : MonoBehaviour
    {
        public float radius        = 1.5f;
        public int   damagePerTick = 5;
        public float tickSeconds   = 1f;

        float nextTick;

        void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + tickSeconds;

            foreach (var enemy in EnemiesInside())
            {
                enemy.health -= damagePerTick;      // <-- straight into the field. no bar update. no death.
            }
        }

        HashSet<Enemy> EnemiesInside()
        {
            var found = new HashSet<Enemy>();
            foreach (var col in Physics.OverlapSphere(transform.position, radius))
            {
                var enemy = col.GetComponentInParent<Enemy>();
                if (enemy != null) found.Add(enemy);
            }
            return found;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
