using UnityEngine;
using UnityEngine.InputSystem;

// SCENE 3 - ENCAPSULATION  (solution)
// The cheats still exist. They just can't put the enemy into a state that isn't real.

namespace Lecture03.Solution
{
    public class DebugCheats : MonoBehaviour
    {
        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.kKey.wasPressedThisFrame)
            {
                foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                    enemy.Kill();
            }

            if (kb.hKey.wasPressedThisFrame)
            {
                foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                    enemy.Heal(enemy.MaxHealth);
            }
        }
    }
}
