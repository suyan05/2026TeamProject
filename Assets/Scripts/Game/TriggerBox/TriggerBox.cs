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

    public Vector3 hitboxOffset = Vector3.zero;    // 오프셋
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f); // 크기 (width, height, depth)
    public LayerMask playerLayer;

    ITriggerBox[] triggerBoxParts;
    bool isEnabled = false;

    // 실시간 위치 반영을 위한 프로퍼티
    Vector3 BoxCenter => transform.position + hitboxOffset;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnabled) return;

        // [수정] 충돌한 오브젝트가 PlayerMovement 이거나 PlayerController 인지 둘 다 검사
        if (IsTargetPlayer(other.gameObject))
        {
            Debug.Log($"플레이어가 TriggerBox에 들어왔습니다. (오브젝트: {other.gameObject.name})");
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
        isEnabled = In;
        foreach (ITriggerBox part in triggerBoxParts)
        {
            if (In) part.TriggerIn();
            else part.TriggerOut();
        }
    }

    public bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapBox(
            BoxCenter,
            hitboxSize * 0.5f,
            Quaternion.identity,
            playerLayer
        );

        foreach (var hit in hits)
        {
            // [수정] 범위 체크할 때도 두 컴포넌트 모두 대응하도록 변경
            if (IsTargetPlayer(hit.gameObject))
                return true;
        }

        return false;
    }

    /// <summary>
    /// [추가] 해당 오브젝트가 PlayerMovement 또는 PlayerController의 인스턴스인지 확인하는 메서드
    /// </summary>
    private bool IsTargetPlayer(GameObject targetObj)
    {
        // 1. PlayerMovement 컴포넌트 검사 (싱글톤이 존재하고, 그 오브젝트와 일치하는지)
        // 클래스명 유실을 대비해 컴포넌트가 붙어있는지 확인하는 방식으로도 이중 체크합니다.
        if (targetObj.GetComponent<PlayerMovement>() != null) return true;

        // 2. PlayerController 컴포넌트 검사
        if (targetObj.GetComponent<PlayerController>() != null) return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (rangeType == TriggerBoxRangeType.notUse) return;
        Gizmos.color = Color.violet;

        Gizmos.DrawWireCube(BoxCenter, hitboxSize);
    }
}