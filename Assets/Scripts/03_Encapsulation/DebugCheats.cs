using UnityEngine;
using UnityEngine.InputSystem;

// SCENE 3 - ENCAPSULATION  (starting state)
//
// Dev hotkeys "for testing". Everybody has a file like this.
//   K  =  kill every enemy
//   H  =  full-heal every enemy

namespace Lecture03
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
                    enemy.health = -50;             // <-- "-50 so it's *definitely* dead"
            }

            if (kb.hKey.wasPressedThisFrame)
            {
                foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                    enemy.health = 999;             // <-- more than max. why not.
            }
        }
    }
}
