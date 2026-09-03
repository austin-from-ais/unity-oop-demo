using UnityEngine;

// SCENE 1 - COLD OPEN
//
// The skeleton. Just a bag of public variables.
// It doesn't DO anything to itself. Whoever hits it reaches in and edits these numbers directly.
// (Open HeroController.cs and look at LandHit() to see that happen.)

namespace Lecture01
{
    public class SkeletonEnemy : MonoBehaviour
    {
        [Header("Stats  (edit these in the Inspector while playing)")]
        public string enemyName = "Skeleton";
        public int    maxHealth = 100;
        public int    health    = 100;
        public int    defence   = 0;      // no armour, no shield. he just takes it.

        [Header("Wiring")]
        public Animator  animator;
        public HealthBar healthBar;

        void Start()
        {
            if (animator  == null) animator  = GetComponent<Animator>();
            if (healthBar == null) healthBar = GetComponent<HealthBar>();

            health = maxHealth;
            healthBar.SetFraction(1f);
        }
    }
}
