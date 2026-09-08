using UnityEngine;

// C# IN UNITY - WHY THERE IS NO main()
//
// You never call any of these. Unity does. Attach this to something, press Play, and read
// the Console to watch Unity call your code in the order it chooses.

namespace Lifecycle
{
    public class LifecycleProbe : MonoBehaviour
    {
        void Awake()       { Debug.Log($"{name}: Awake"); }
        void OnEnable()    { Debug.Log($"{name}: OnEnable"); }
        void Start()       { Debug.Log($"{name}: Start"); }
        void Update()      { Debug.Log($"{name}: Update  frame {Time.frameCount}"); }
        void FixedUpdate() { Debug.Log($"{name}: FixedUpdate"); }
        void LateUpdate()  { Debug.Log($"{name}: LateUpdate"); }
        void OnDisable()   { Debug.Log($"{name}: OnDisable"); }
        void OnDestroy()   { Debug.Log($"{name}: OnDestroy"); }
    }
}
