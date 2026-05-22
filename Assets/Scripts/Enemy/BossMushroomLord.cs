using UnityEngine;
using System.Collections;

public class BossMushroomLord : BossBase
{
    [Header("Mushroom Lord Settings")]
    public GameObject sporeMinionPrefab;  // 자폭 포자 프리팹
    public GameObject poisonAreaPrefab;  // 독 폭탄 장판 프리팹
    public GameObject smashIndicator;    // 발 구르기 범위 표시 UI

    private float patternCooldown = 3.5f; // 1페이즈 기본 쿨타임

    private void Update()
    {
        if (isDead || isPhaseTransitioning) return;
        // 실제 게임에서는 여기서 타이머를 돌려 패턴을 무작위로 시전합니다.
    }

    // [패턴 1] 포자 소환 (미니언 2~4마리)
    public void SummonSpores()
    {
        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = transform.position.y; // 바닥 높이 맞추기
            Instantiate(sporeMinionPrefab, spawnPos, Quaternion.identity);
        }
        Debug.Log($"버섯군주 패턴: {count}마리의 자폭 포자 소환!");
    }

    // [패턴 2] 독 폭탄 던지기 (지속 데미지 장판)
    public IEnumerator ThrowPoisonBomb(Vector3 targetPos)
    {
        Debug.Log("버섯군주 패턴: 독 폭탄 투하 투척!");
        yield return new WaitForSeconds(1.2f); // 날아가는 시간
        Instantiate(poisonAreaPrefab, targetPos, Quaternion.identity);
    }

    // [패턴 3] 발 구르기 (원형 충격파 + 넉백)
    public IEnumerator FootSmash()
    {
        Debug.Log("버섯군주 패턴: 발 구르기 시전 (범위 예고)");
        if (smashIndicator != null) smashIndicator.SetActive(true);

        yield return new WaitForSeconds(1.5f); // 전조 증상 시간

        // 주변 플레이어 판정 및 넉백 로직 들어갈 자리
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 6f);
        foreach (var player in hitPlayers)
        {
            // if (player가 플레이어라면) 넉백 및 데미지 처리
        }

        if (smashIndicator != null) smashIndicator.SetActive(false);
    }

    // 2페이즈 전환 (HP 50% 이하 시 자동 발동)
    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        currentPhase = 2;

        // 문서 기준: 이동 속도 1.4배 ~ 1.6배 증가 및 패턴 쿨타임 단축
        moveSpeed *= 1.5f;
        patternCooldown = 1.8f; // 더욱 빠르게 패턴을 몰아침

        Debug.Log("<color=red>?? 돌연변이 버섯군주 2페이즈 진입! 광폭화 상태 (속도 증가/쿨타임 단축)</color>");

        // 페이즈 전환 애니메이션이나 포효 연출 시간 대기
        yield return new WaitForSeconds(2f);
        isPhaseTransitioning = false;
    }

    protected override void Die()
    {
        isDead = true;
        Debug.Log("돌연변이 버섯군주 무찔렀습니다! 클리어!");
    }
}