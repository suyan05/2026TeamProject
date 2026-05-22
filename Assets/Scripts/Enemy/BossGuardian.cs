using UnityEngine;
using System.Collections;

public class BossGuardian : BossBase
{
    [Header("Guardian Settings")]
    public GameObject vineProjectilePrefab;
    public Transform shootPoint;
    public GameObject smashIndicator; // 내리찍기 범위 표시용 UI/오브젝트
    private float defenseModifier = 1f;

    private void Update()
    {
        if (isDead || isPhaseTransitioning) return;
        // 여기에 패턴 타이머 및 플레이어 추적 로직 추가
    }

    // 패턴 1: 덩굴 도약 (Leap)
    public IEnumerator LeapAttack(Vector3 targetPos)
    {
        Debug.Log("패턴: 덩굴 도약 시작");
        // 도약 중에는 무기 데미지 일부 무시 로직
        yield return new WaitForSeconds(1f);
        transform.position = targetPos; // 실제로는 포물선 이동 연출 필요
    }

    // 패턴 2: 내리찍기 (Smash)
    public IEnumerator SmashAttack()
    {
        smashIndicator.SetActive(true); // 경고 표시
        yield return new WaitForSeconds(1.5f);

        // 범위 데미지 판정
        Collider[] hit = Physics.OverlapSphere(transform.position, 5f);
        foreach (var h in hit) { /* 플레이어 데미지 처리 */ }

        smashIndicator.SetActive(false);
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        currentPhase = 2;
        defenseModifier = 0.3f; // 받는 피해 70% 감소 (문서 내용)

        Debug.Log("덩굴의 수호자 2페이즈 진입: 방어력 대폭 상승!");
        // 카메라 줌인 연출 (Cinemachine 호출 등)
        yield return new WaitForSeconds(2f);
        isPhaseTransitioning = false;
    }

    protected override void Die()
    {
        isDead = true;
        Debug.Log("덩굴의 수호자 처치 완료");
    }
}
