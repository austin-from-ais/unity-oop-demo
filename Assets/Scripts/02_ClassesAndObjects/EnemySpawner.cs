using UnityEngine;

// SCENE 2 - CLASSES AND OBJECTS
//
// The payoff. In scene 1, "add an enemy" meant copying every variable and every block.
// Now it's one line inside a loop, because Enemy is a blueprint we can stamp out.
//
// Every enemy this spawns is a separate object: own position, own health, own name.

namespace Lecture02
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("What to build, and how many")]
        public Enemy enemyPrefab;
        public int   count  = 10;
        public float radius = 3.5f;

        [Header("Give each object its own numbers")]
        public int minHealth = 60;
        public int maxHealth = 160;

        void Start()
        {
            for (int i = 0; i < count; i++)
            {
                // spread them in a ring around this spawner
                float angle = i * Mathf.PI * 2f / count;
                var offset  = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                var pos     = transform.position + offset;

                Enemy enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, transform);   // <-- the one line

                enemy.name      = "Skeleton " + (i + 1);
                enemy.enemyName = enemy.name;
                enemy.maxHealth = Random.Range(minHealth, maxHealth + 1);
                enemy.transform.LookAt(transform.position);
            }
        }
    }
}
