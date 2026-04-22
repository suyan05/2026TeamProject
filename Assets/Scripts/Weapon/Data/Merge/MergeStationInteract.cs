using UnityEngine;

public class MergeStationInteract : MonoBehaviour
{
    public GameObject mergeUI;
    private bool isOpen = false;

    [Header("상호작용 거리")]
    public float interactDistance = 1.5f;

    private Transform player;

    private void Start()
    {
        player = PlayerMovement.Instance.transform;
    }

    private void Update()
    {
        DrawInteractionRange();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsPlayerInRange())
            {
                ToggleMergeUI();
            }
        }
    }

    bool IsPlayerInRange()
    {
        float dist = Vector2.Distance(player.position, transform.position);
        return dist <= interactDistance;
    }

    void DrawInteractionRange()
    {
        // Scene 뷰에서만 보이는 원형 범위 표시
        Debug.DrawLine(transform.position,
                       transform.position + Vector3.right * interactDistance,
                       Color.yellow);

        Debug.DrawLine(transform.position,
                       transform.position + Vector3.left * interactDistance,
                       Color.yellow);

        Debug.DrawLine(transform.position,
                       transform.position + Vector3.up * interactDistance,
                       Color.yellow);

        Debug.DrawLine(transform.position,
                       transform.position + Vector3.down * interactDistance,
                       Color.yellow);
    }

    public void ToggleMergeUI()
    {
        isOpen = !isOpen;
        mergeUI.SetActive(isOpen);
    }
}
