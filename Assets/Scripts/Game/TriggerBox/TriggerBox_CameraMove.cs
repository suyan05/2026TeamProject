using UnityEngine;

public class TriggerBox_CameraMove : MonoBehaviour, ITriggerBox
{
    public enum TriggerBoxPartActionType { atIn, atOut, both }
    [Header("작동 시간")]
    public TriggerBoxPartActionType actionType = TriggerBoxPartActionType.atIn;

    [Header("카메라 위치")]
    public float cameraZoom = 17;
    public float cameraMoveDuration = 0.5f;

    public void TriggerIn()
    {
        if (actionType == TriggerBoxPartActionType.atOut) return;

        Trigger();
    }

    public void TriggerOut()
    {
        if (actionType == TriggerBoxPartActionType.atIn) return;

        Trigger();
    }

    void Trigger()
    {
        CameraMovement.PositionZoom(cameraZoom, cameraMoveDuration);
    }
}
