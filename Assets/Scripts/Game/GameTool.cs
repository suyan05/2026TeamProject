using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTool : MonoBehaviour
{
    private Dictionary<GameObject, Coroutine> activeCoroutines = new Dictionary<GameObject, Coroutine>();

    public void RotateObjectToPos(GameObject obj, Vector2 target, float offsetAngle = 0f, float duration = 0f)  // obj가 target을 향하도록 회전 (offsetAngle은 target을 향하는 각도에서 더해지는 각도, duration은 회전이 완료되는 데 걸리는 시간)
    {
        if (obj == null) return;

        Vector2 direction = target - (Vector2)obj.transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float finalAngle = targetAngle + offsetAngle;

        if (duration > 0f)
        {
            if (activeCoroutines.ContainsKey(obj) && activeCoroutines[obj] != null)
            {
                StopCoroutine(activeCoroutines[obj]);
            }

            activeCoroutines[obj] = StartCoroutine(RotateRoutine(obj, finalAngle, duration));
        }
        else
        {
            obj.transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
        }
    }
    

    private IEnumerator RotateRoutine(GameObject obj, float targetAngle, float duration)    // RotateObjectToPos에서 호출하는 obj를 targetAngle까지 duration 시간 동안 회전시키는 코루틴
    {
        float startAngle = obj.transform.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (obj == null) yield break;

            elapsedTime += Time.deltaTime;

            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsedTime / duration);
            obj.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            yield return null;
        }

        if (obj != null)
        {
            obj.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            activeCoroutines.Remove(obj);
        }
    }

    int GetRandomInt(int number1, int number2)  // number1과 number2 사이의 랜덤 정수 반환 (소수는 필요 없음)
    {
        if (number1 == number2) return number1;
        else if (number1 > number2) return Random.Range(number2, number1 + 1);
        else return Random.Range(number1, number2 + 1);
    }
}
