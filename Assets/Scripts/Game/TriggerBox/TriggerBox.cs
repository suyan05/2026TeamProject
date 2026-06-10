using System.Collections;
using UnityEngine;

public interface ITriggerBox
{
    void TriggerIn();
    void TriggerOut();
}

public class TriggerBox : MonoBehaviour
{
    public enum TriggerBoxRangeType { cancellation, maintain, notUse }

    [Header("해제 범위")]
    public TriggerBoxRangeType rangeType = TriggerBoxRangeType.notUse;
    // 이 영역이 어떻게 사용될지에 대한 열거형
    // cancellation - 트리거박스의 해제 영역
    // maintain - 트리거박스의 유지 영역 (영역을 벗어났을 경우 해제)
    // notUse - 일회용. 스크립트가 한 번 작동 후 꺼짐. TriggerOut() 은 호출될 수 없음.

    public Vector3 hitboxOffset = Vector3.zero;    // 오프셋
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f); // 크기 (width, height, depth)
    public LayerMask playerLayer;

    ITriggerBox[] triggerBoxParts;

    bool isEnabled = false;
    Vector3 boxCenter;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }
        col.isTrigger = true;

        triggerBoxParts = gameObject.GetComponents<ITriggerBox>();

        if (triggerBoxParts.Length <= 0)
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        boxCenter = transform.position + hitboxOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnabled) return;

        if (other.gameObject == PlayerMovement.Instance.gameObject)
        {
            Debug.Log("플레이어가 TriggerBox에 들어왔습니다.");
            TriggerParts(true);

            if (rangeType == TriggerBoxRangeType.notUse)
            {
                this.enabled = false;
                return;
            }
            else
            {
                StartCoroutine(TriggerCancellation());
            }
        }
    }

    IEnumerator TriggerCancellation()
    {
        if (rangeType == TriggerBoxRangeType.cancellation)
        {
            while (!IsPlayerInRange()) yield return null;
        }
        else
        {
            while (IsPlayerInRange()) yield return null;
        }

        TriggerParts(false);
    }

    void TriggerParts(bool In)
    {
        // 성능을 개선하고 가독성을 줄이는 것을 택하는 미친 속박을 걸어버림
        // 명명백백한 속박의 왕

        isEnabled = In;
        if (In)
        {
            foreach (ITriggerBox part in triggerBoxParts)
            {
                part.TriggerIn();
            }
        }
        else
        {
            foreach (ITriggerBox part in triggerBoxParts)
            {
                part.TriggerOut();
            }
        }
    }

    public bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            hitboxSize * 0.5f,
            Quaternion.identity,
            playerLayer
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == PlayerMovement.Instance.gameObject)
                return true;
        }

        return false;
        // 이거 왜 if안에 넣었지? 이렇게 해도 되는거잖아 정신차려정신차려정신차려정신차려정신차려정신차려정신차려정신차려정신차려정신차려정신차려
    }

    private void OnDrawGizmosSelected()
    {
        if (rangeType == TriggerBoxRangeType.notUse) return;
        Gizmos.color = Color.violet;

        Vector3 hitboxGizmoCenter = transform.position + hitboxOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}
