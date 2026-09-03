using System.Collections.Generic;
using UnityEngine;

// SCENE 3 - ENCAPSULATION  (solution)
// One line changed. The bar updates and the skeleton dies, because TakeDamage does that.

namespace Lecture03.Solution
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
                enemy.TakeDamage(damagePerTick);    // <-- through the gate
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
