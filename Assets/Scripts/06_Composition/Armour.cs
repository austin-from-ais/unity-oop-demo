using UnityEngine;

// SCENE 6 - INTERFACES AND COMPOSITION
// Knocks a flat amount off every hit. This is the old `defence` field, as a component.

namespace Lecture06
{
    public class Armour : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private int reduction = 3;

        public int Modify(int amount)
        {
            return Mathf.Max(0, amount - reduction);
        }
    }
}
