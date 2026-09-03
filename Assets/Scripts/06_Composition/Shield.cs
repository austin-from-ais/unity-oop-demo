using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
//
// Soaks up damage until it breaks. Then it's gone.
// Drop this on any GameObject that has a Health and it just works. Nothing else needs editing.

namespace Lecture06
{
    public class Shield : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private int        strength = 30;
        [SerializeField] private GameObject visual;     // optional: hidden when the shield breaks

        private int remaining;

        public int  Remaining => remaining;
        public bool IsBroken  => remaining <= 0;

        void Awake()
        {
            remaining = strength;
        }

        public int Modify(int amount)
        {
            if (IsBroken) return amount;

            int absorbed = Mathf.Min(remaining, amount);
            remaining -= absorbed;

            if (IsBroken && visual != null)
                visual.SetActive(false);

            return amount - absorbed;
        }
    }
}
