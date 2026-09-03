using UnityEngine;

/// <summary>Elevated three-quarter camera that trails a target. RuneScape-ish.</summary>
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3   offset      = new Vector3(0f, 7f, -7f);
    public float     lookHeight  = 0.8f;
    public float     smoothTime  = 0.15f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        var wanted = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
