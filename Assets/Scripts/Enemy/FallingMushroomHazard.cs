using UnityEngine;

public class FallingMushroomHazard : MonoBehaviour
{
    [Header("데미지 설정")]
    public float damage = 15f;            // 플레이어에게 줄 데미지
    public float explosionRadius = 1.5f;  // 바닥에 충돌했을 때 폭발 범위
    public bool damageOnCollision = true; // 플레이어에게 직접 부딪혔을 때도 데미지를 줄지 여부

    [Header("레이어 설정")]
    public LayerMask playerLayer;         // Player 레이어
    public LayerMask groundMask;          // Ground 레이어

    private bool hasTriggered = false;    // 중복 데미지 방지용 플래그

    // Trigger 콜라이더를 사용할 경우 작동
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // 1. 떨어지다가 플레이어와 직접 부딪힌 경우
        if (damageOnCollision && ((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            TriggerDamage();
        }
        // 2. 바닥에 부딪힌 경우
        else if (((1 << other.gameObject.layer) & groundMask) != 0)
        {
            Explode();
        }
    }

    // 일반 물리 콜라이더(Is Trigger 체크 안됨)를 사용할 경우 작동
    private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;

        if (damageOnCollision && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            TriggerDamage();
        }
        else if (((1 << collision.gameObject.layer) & groundMask) != 0)
        {
            Explode();
        }
    }

    // 플레이어 직접 타격 시 데미지 처리
    void TriggerDamage()
    {
        hasTriggered = true;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.GetDamage(damage, transform);
        }

        // 데미지를 준 후 버섯 오브젝트 삭제
        Destroy(gameObject);
    }

    // 바닥 충돌 시 주변 범위 폭발 처리
    void Explode()
    {
        hasTriggered = true;

        // 주변 범위 내의 플레이어 감지 (OverlapSphere)
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);

        foreach (Collider player in hitPlayers)
        {
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.GetDamage(damage, transform);
            }
        }

        // ?? 이펙트가 있다면 여기에 추가 (예: Instantiate(폭발이펙트, transform.position, Quaternion.identity);)

        // 폭발 후 버섯 오브젝트 삭제
        Destroy(gameObject);
    }

    // 에디터 화면에서 폭발 범위를 주황색 선으로 미리 보기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}