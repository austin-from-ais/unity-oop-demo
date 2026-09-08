using UnityEngine;

// C# IN UNITY - THE CALL CHAIN STARTS WITH MONOBEHAVIOUR
//
// Also plain C#. It compiles. It's in the project. It even has a Start() method.
// It will never run, because nothing calls it and Unity only calls MonoBehaviours.
// Having a method named Start() means nothing outside a MonoBehaviour.

namespace Lifecycle
{
    public class Forgotten
    {
        public void Start()
        {
            Debug.Log("Forgotten.Start ran. (You will never see this line.)");
        }
    }
}
