using System.Collections.Generic;
using UnityEngine;

// SCENE 5 - POLYMORPHISM
//
// One list. Ground enemies, flying enemies, and a boss all go in it, because every one of them
// IS an Enemy. One loop in Update() calls Move() on each. The loop has no idea which is which.
// Each object runs its own Move(). That is polymorphism: one call, many shapes.
//
// This is also exactly what Unity does to you. Unity keeps a list of every MonoBehaviour in the
// scene and calls Update() on each one, every frame, without knowing or caring what any of them
// are. Your Hero, your Enemy, your camera script: all "just MonoBehaviours" to the loop.

namespace Lecture05
{
    public class Horde : MonoBehaviour
    {
        [Header("Blueprints")]
        [SerializeField] private GroundEnemy groundPrefab;
        [SerializeField] private FlyingEnemy flyingPrefab;
        [SerializeField] private BossEnemy   bossPrefab;

        [Header("How many of each")]
        [SerializeField] private int   groundCount = 3;
        [SerializeField] private int   flyingCount = 3;
        [SerializeField] private int   bossCount   = 1;
        [SerializeField] private float radius      = 6f;

        [Header("Everything spawned, whatever kind it is")]
        [SerializeField] private List<Enemy> enemies = new List<Enemy>();

        void Start()
        {
            int total = groundCount + flyingCount + bossCount;
            int slot  = 0;

            for (int i = 0; i < groundCount; i++) Spawn(groundPrefab, "Ground " + (i + 1), slot++, total);
            for (int i = 0; i < flyingCount; i++) Spawn(flyingPrefab, "Flyer "  + (i + 1), slot++, total);
            for (int i = 0; i < bossCount;   i++) Spawn(bossPrefab,   "Boss "   + (i + 1), slot++, total);
        }

        void Spawn(Enemy prefab, string name, int slot, int total)
        {
            if (prefab == null) return;

            float angle = slot * Mathf.PI * 2f / total;
            var   pos   = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

            Enemy enemy = Instantiate(prefab, pos, Quaternion.identity, transform);   // a GroundEnemy IS an Enemy, so this is allowed
            enemy.name = name;
            enemy.transform.LookAt(transform.position);

            enemies.Add(enemy);
        }

        void Update()
        {
            // ---- the loop ----
            foreach (Enemy e in enemies)
            {
                if (!e.IsDead)
                    e.Move();       // ground walks. flyer hovers. boss charges. same line.
            }
        }

        public int Alive
        {
            get
            {
                int n = 0;
                foreach (var e in enemies) if (!e.IsDead) n++;
                return n;
            }
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Enemies alive: " + Alive + " / " + enemies.Count);
        }
    }
}
