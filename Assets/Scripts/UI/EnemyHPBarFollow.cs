using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarFollow : MonoBehaviour
{
    public Transform target;      // 따라갈 적
    public Vector3 offset;        // 머리 위 위치
    public Image fillImage;       // HP바 Fill 이미지

    Camera cam;

    float currentFill = 1f;       // 실제 표시되는 fillAmount
    float targetFill = 1f;        // 목표 fillAmount
    float smoothSpeed = 5f;       // 부드럽게 줄어드는 속도

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // HP바 위치 업데이트
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + offset);
        transform.position = screenPos;

        // 부드러운 HP 감소
        //currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        //fillImage.fillAmount = currentFill;

        //UpdateColor();
    }

    public void SetHP(float current, float max)
    {
        targetFill = Mathf.Clamp01(current / max);
    }

    /*void UpdateColor()
    {
        if (currentFill > 0.7f)
        {
            fillImage.color = Color.green;
        }
        else if (currentFill > 0.3f)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }*/
}
