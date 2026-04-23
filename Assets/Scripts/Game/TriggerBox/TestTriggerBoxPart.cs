using UnityEngine;

public class TestTriggerBoxPart : MonoBehaviour, ITriggerBox
{
    public void TriggerIn()
    {
        Debug.Log("Trigger In");
    }
    public void TriggerOut()
    {
        Debug.Log("Trigger Out");
    }
}
