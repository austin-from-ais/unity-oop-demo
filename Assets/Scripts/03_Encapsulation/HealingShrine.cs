using System.Collections.Generic;
using UnityEngine;

// SCENE 3 - ENCAPSULATION  (starting state)
//
// A shrine. Every few seconds, any enemy standing near it gets some health back.
// Written by a third person. Also looks reasonable. Also ships.

namespace Lecture03
{
    public class HealingShrine : MonoBehaviour
    {
        public float radius      = 1.5f;
        public int   healPerTick = 20;
        public float tickSeconds = 2f;

        float nextTick;

        void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + tickSeconds;

            foreach (var enemy in EnemiesInside())
            {
                enemy.health += healPerTick;        // <-- no cap. 100, 120, 140, 160 ... forever.
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
            Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
