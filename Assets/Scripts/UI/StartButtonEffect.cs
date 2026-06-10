using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleStartButton : MonoBehaviour
{
    public Graphic uiGraphic;      // 버튼의 Text 또는 Image
    public float blinkSpeed = 2f;  // 깜빡이는 속도
    public string nextSceneName;   // 이동할 씬 이름

    void Update()
    {
        // 알파값을 0~1로 반복
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));

        Color c = uiGraphic.color;
        c.a = alpha;
        uiGraphic.color = c;
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
