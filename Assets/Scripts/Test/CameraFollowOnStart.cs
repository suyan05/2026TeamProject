using UnityEngine;

public class CameraFollowOnStart : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector3 rotation;

    void Start()
    {
        CameraMovement.TargetTracking(target, offset);
        CameraMovement.RotateTo(rotation, 0);
    }
}
