using UnityEngine;

// C# IN UNITY - THE CALL CHAIN STARTS WITH MONOBEHAVIOUR
//
// This IS a MonoBehaviour, so Unity calls Start() and Update() on it.
// From inside those, it calls a plain C# object. That's the whole chain:
//
//     Unity  ->  CallChainDemo.Update()  ->  greeter.Greet()
//
// Greeter runs because this script called it. Forgotten never runs because nobody does.

namespace Lifecycle
{
    public class CallChainDemo : MonoBehaviour
    {
        public int everyNFrames = 120;

        Greeter   greeter   = new Greeter();     // plain C# object, lives inside this component
        Forgotten forgotten = new Forgotten();   // exists. never used. its Start() is just a method.

        void Start()
        {
            Debug.Log(greeter.Greet(name));
        }

        void Update()
        {
            if (Time.frameCount % everyNFrames == 0)
                Debug.Log(greeter.Greet(name));
        }
    }
}
