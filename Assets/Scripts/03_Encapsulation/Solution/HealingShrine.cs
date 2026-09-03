using System.Collections.Generic;
using UnityEngine;

// SCENE 3 - ENCAPSULATION  (solution)
// One line changed. Heal() caps at maxHealth, so the shrine can't overheal even if it wanted to.

namespace Lecture03.Solution
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
                enemy.Heal(healPerTick);            // <-- through the gate
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
