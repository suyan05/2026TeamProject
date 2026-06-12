using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIController : MonoBehaviour
{
    [Header("UI Targets")]
    public RectTransform titleUI;
    public RectTransform buttonUI;

    [Header("Start Positions")]
    public Vector2 titleStartPos;
    public Vector2 buttonStartPos;

    [Header("End Positions")]
    public Vector2 titleEndPos;
    public Vector2 buttonEndPos;

    [Header("Animation Settings")]
    public float titleMoveDuration = 1.0f;
    public float buttonMoveDuration = 1.0f;

    [Header("Idle Motion Settings")]
    public float idleRange = 10f;
    public float idleSpeed = 1.5f;

    [Header("Scene")]
    public string nextSceneName;

    private bool isIdle = false;

    void Start()
    {
        titleUI.anchoredPosition = titleStartPos;
        buttonUI.anchoredPosition = buttonStartPos;

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // 1) 타이틀 등장
        yield return StartCoroutine(MoveUI(titleUI, titleStartPos, titleEndPos, titleMoveDuration));

        // 2) 버튼 등장 (튀는 효과 포함)
        yield return StartCoroutine(MoveUIBounce(buttonUI, buttonStartPos, buttonEndPos, buttonMoveDuration));

        // 3) Idle motion 시작
        isIdle = true;
    }

    // 기본 Lerp 이동
    IEnumerator MoveUI(RectTransform target, Vector2 startPos, Vector2 endPos, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            target.anchoredPosition = Vector2.Lerp(startPos, endPos, lerp);
            yield return null;
        }
    }

    // 버튼 튕김 효과 (EaseOutBack)
    IEnumerator MoveUIBounce(RectTransform target, Vector2 startPos, Vector2 endPos, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            // EaseOutBack 곡선
            float eased = EaseOutBack(lerp);

            target.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }
    }

    // EaseOutBack 함수
    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    void Update()
    {
        if (!isIdle) return;

        float time = Time.time * idleSpeed;

        // 곡선 기반 Idle motion (부드러운 위아래 움직임)
        float offset1 = Mathf.Sin(time) * idleRange;
        float offset2 = Mathf.Sin(time + Mathf.PI / 2) * idleRange * 0.8f;

        titleUI.anchoredPosition = titleEndPos + new Vector2(0, offset1);
        buttonUI.anchoredPosition = buttonEndPos + new Vector2(0, offset2);
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
