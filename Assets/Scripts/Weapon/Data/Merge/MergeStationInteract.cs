using UnityEngine;

public class MergeStationInteract : MonoBehaviour
{
    public GameObject mergeUI;          // 머지 UI
    public GameObject interactUI;       // E키 UI
    public float interactDistance = 2f; // 상호작용 거리

    public bool isOpen;

    private Transform player;
    private Camera cam;
    private RectTransform interactRT;

    private MergeStationUI stationUI;

    private void Awake()
    {
        // mergeUI 자동 할당
        if (mergeUI == null)
        {
            // UIManager에서 MergePanel을 찾거나, 씬 전체에서 탐색
            Transform t = UIManager.Instance != null ? UIManager.Instance.transform.Find("MergePanel") : null;
            if (t != null)
                mergeUI = t.gameObject;
            else
            {
                GameObject go = GameObject.Find("MergePanel");
                if (go != null)
                    mergeUI = go;
            }
        }

        // interactUI 자동 할당
        if (interactUI == null)
        {
            Transform t = UIManager.Instance != null ? UIManager.Instance.transform.Find("E") : null;
            if (t != null)
                interactUI = t.gameObject;
            else
            {
                GameObject go = GameObject.Find("E");
                if (go != null)
                    interactUI = go;
            }
        }

        stationUI = FindObjectOfType<MergeStationUI>();
        player = GameObject.FindWithTag("Player")?.transform;
        cam = Camera.main;

        if (interactUI != null)
        {
            interactRT = interactUI.GetComponent<RectTransform>();
            interactUI.SetActive(false);
        }
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        // 플레이어가 가까이 왔을 때
        if (dist <= interactDistance)
        {
            if (!isOpen)
            {
                interactUI.SetActive(true);
                UpdateInteractUIPosition();
            }

            if (Input.GetKeyDown(KeyCode.E))
                ToggleMergeUI();
        }
        else
        {
            interactUI.SetActive(false);
        }
    }

    // E키 UI를 머지 스테이션 위치에 맞춰 화면에 표시
    private void UpdateInteractUIPosition()
    {
        if (interactRT == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 1.5f; // 스테이션 위쪽에 표시
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        interactRT.position = screenPos;
    }

    public void ToggleMergeUI()
    {
        if (isOpen)
            CloseMergeUI();
        else
            OpenMergeUI();
    }

    public void OpenMergeUI()
    {
        isOpen = true;
        mergeUI.SetActive(true);

        PlayerMovement.Instance.controlLocked = true;
        UIManager.Instance.inventoryUI.SetActive(true);

        interactUI.SetActive(false);
    }

    public void CloseMergeUI()
    {
        isOpen = false;
        mergeUI.SetActive(false);

        PlayerMovement.Instance.controlLocked = false;
        UIManager.Instance.inventoryUI.SetActive(false);

        if (stationUI != null)
            stationUI.ClearAll();
    }
}
