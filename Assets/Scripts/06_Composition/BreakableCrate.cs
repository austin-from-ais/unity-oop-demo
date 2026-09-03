using System.Collections;
using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// A crate. It is not an Enemy. It has no Health component. It has no health at all: it breaks
// after three hits from anything, no matter how hard.
//
// It still keeps the IDamageable promise, so the sword, the poison, and a mage's fireball can
// all hit it, and none of them know it's a crate. That's what an interface buys you that a
// parent class can't: things with nothing in common can still be treated the same way.

namespace Lecture06
{
    public class BreakableCrate : MonoBehaviour, IDamageable
    {
        [SerializeField] private int hitsToBreak = 3;

        private int hits;

        public bool IsDead => hits >= hitsToBreak;

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            hits++;                                   // the amount is ignored. it's a crate.
            StopAllCoroutines();

            if (IsDead)
            {
                GetComponent<Collider>().enabled = false;
                StartCoroutine(Break());
            }
            else
            {
                StartCoroutine(Wobble());
            }
        }

        IEnumerator Wobble()
        {
            var start = transform.localScale;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t / 0.25f * Mathf.PI) * 0.15f;
                transform.localScale = new Vector3(start.x * s, start.y / s, start.z * s);
                yield return null;
            }
            transform.localScale = start;
        }

        IEnumerator Break()
        {
            var start = transform.localScale;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 0.3f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
