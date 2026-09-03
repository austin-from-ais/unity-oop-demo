using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
// A glowing ball that homes in on a target and calls TakeDamage on arrival. Built at runtime, no prefab.

namespace Lecture06
{
    public class Projectile : MonoBehaviour
    {
        Transform   target;
        IDamageable damageable;
        int         damage;
        float       speed;
        float       dieAt;

        public static Projectile Spawn(Vector3 from, Transform target, IDamageable damageable, int damage, float speed)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Projectile";
            Destroy(go.GetComponent<Collider>());                  // visual only; never blocks a click or a raycast
            go.transform.position   = from;
            go.transform.localScale = Vector3.one * 0.25f;
            go.GetComponent<Renderer>().material.color = new Color(0.65f, 0.25f, 1f);

            var p = go.AddComponent<Projectile>();
            p.target     = target;
            p.damageable = damageable;
            p.damage     = damage;
            p.speed      = speed;
            p.dieAt      = Time.time + 5f;
            return p;
        }

        void Update()
        {
            if (target == null || damageable == null || damageable.IsDead || Time.time > dieAt)
            {
                Destroy(gameObject);
                return;
            }

            var aim = target.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, aim, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, aim) < 0.3f)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
