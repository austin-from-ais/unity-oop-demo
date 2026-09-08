// C# IN UNITY - THE CALL CHAIN STARTS WITH MONOBEHAVIOUR
//
// This is plain C#. No MonoBehaviour, no `using UnityEngine`. Unity does not know it exists.
// It runs only when something that Unity IS calling decides to call it.
// (See CallChainDemo.cs for the thing that calls it.)

namespace Lifecycle
{
    public class Greeter
    {
        public int timesCalled;

        public string Greet(string who)
        {
            timesCalled++;
            return $"Hello, {who}. (call #{timesCalled})";
        }
    }
}
