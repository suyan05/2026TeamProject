using UnityEngine;

public class CameraFollowOnStart : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector3 rotation;

    void Start()
    {
        // Player 자동 참조
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target != null)
        {
            CameraMovement.TargetTracking(target, offset);
            CameraMovement.RotateTo(rotation, 0);
        }
        else
        {
            Debug.LogWarning("CameraFollowOnStart: Player를 찾을 수 없습니다.");
        }
    }
}
