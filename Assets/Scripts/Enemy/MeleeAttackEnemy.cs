using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class MeleeAttackEnemy : MonoBehaviour, IEnemyCombat
{
    [Header("체력")]
    public float maxHp = 30f;   

    [Header("적 데이터")]
    public EnemyData enemyData;

    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform upperGroundCheckPos;    // 땅 위쪽
    public Transform lowerGroundCheckPos;    // 땅 아래쪽

    [Header("공격")]
    public float readyToAttackTime = 0.5f;  // 공격으로 전환 시간
    public float attackChargeTime = 0.42f;  // 공격의 준비 시간
    public float attackCooldown = 0.35f;    // 공격 대기시간 (공격 쿨타임)

    [Header("대미지")]
    public float damage = 0.4f;  // 공격 대미지

    [Header("히트박스")]
    public Vector3 hitboxOffset = Vector3.zero;    //  3D 오프셋(Vector3)으로 변경
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f); //  3D 크기(Vector3)로 변경

    [Header("시야 범위")]
    public Vector3 viewOffset = new Vector3(0f, 0.5f, 0f); //  3D 시야 오프셋
    public Vector3 viewSize = new Vector3(5f, 3f, 5f);     //  3D 시야 영역 크기

    [Header("감지, 레이어")]
    public float detectionDecayTime = 3f;   // 플레이어 감지 후 감지율이 0으로 떨어지는 시간
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 감지 레이어

    [Header("적 HP바 자동 생성")]
    public GameObject hpBarPrefab;
    private EnemyHPBarFollow hpBar;

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float detectionRate = 0;    // 플레이어 감지율 (0~1)
    public float currentHp;    // 현재 체력
    private const float layerCheckRadius = 0.05f;  // 감지 위치 반경

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack, endAttack }
    state currentState;

    // 3D 리지드바디로 컴포넌트 타입을 바꿈
    private Rigidbody rb;
    GameObject playerObject;    // 플레이어 오브젝트
    public float dashTime;
    public float dashSpeed;

    private Animator anim;

    private void Awake()
    {
        //  Rigidbody를 가져옵니다.
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        //  싱글톤이 안전하게 들어왔는지 체크 후 대입
        if (PlayerMovement.Instance != null)
        {
            playerObject = PlayerMovement.Instance.gameObject;
        }

        isFacingRight = true;
        currentHp = maxHp;
        SetState(state.idle);

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.AddEnemy();

        if (hpBarPrefab != null && UIManager.Instance != null && UIManager.Instance.worldCanvas != null)
        {
            GameObject ui = Instantiate(hpBarPrefab, UIManager.Instance.worldCanvas.transform);
            hpBar = ui.GetComponent<EnemyHPBarFollow>();
            hpBar.target = transform;
            hpBar.offset = new Vector3(0, 1.2f, 0);
        }
        else
        {
            if (hpBarPrefab == null) Debug.LogError("HP Bar 프리팹이 연결되지 않았습니다!");
            if (UIManager.Instance == null) Debug.LogError("씬에 UIManager가 없습니다!");
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateStates();

        if (anim != null && currentState != state.attack)
        {
            // ] 3D 리지드바디의 X축 속도(velocity.x)를 반영합니다.
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        }

        if (playerObject != null)
        {
            switch (currentState)
            {
                case state.idle:
                    if (IsPlayerInView())
                    {
                        SetState(state.track);
                    }
                    break;

                case state.track:
                    TrackHandler();
                    break;
            }
        }
        else if (currentState != state.idle)
        {
            SetState(state.idle);
        }
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        //  3D 속도 0 제어
        Vector3 originVelocity = rb.velocity;
        originVelocity.x = 0;
        rb.velocity = originVelocity;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle || playerObject == null)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.attack)
        {
            StartCoroutine(AttackHandler());
        }
        else if (targetState == state.endAttack)
        {
            StartCoroutine(EndAttack());
        }
    }

    void SwitchPos()
    {
        Flip();
        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived(targetPos) && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
                //  3D Rigidbody 속도 반영 (Z축은 0으로 유지하여 라인 고정)
                rb.velocity = new Vector3(sign * currentNormalizedSpeed * maxSpeed, rb.velocity.y, 0f);
                yield return null;
            }
            Vector3 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;

            yield return new WaitForSeconds(trunDuration);
            SwitchPos();
            yield return null;
        }
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }

    void TrackHandler()
    {
        if (playerObject == null) return;
        LookPos(playerObject.transform.position);

        Vector3 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        if (canGoStraight && !HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.505f, 1f);
            // 3D Rigidbody 속도 반영
            rb.velocity = new Vector3(facingSign * currentNormalizedSpeed * maxSpeed, rb.velocity.y, 0f);
        }
        else
        {
            Vector3 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;
        }

        if (IsPlayerInRange())
        {
            SetState(state.attack);
            return;
        }

        if (IsPlayerInView())
        {
            detectionRate += 1f;
        }
        else
        {
            detectionRate -= Time.deltaTime / detectionDecayTime;
        }

        detectionRate = Mathf.Clamp01(detectionRate);

        if (detectionRate == 0)
        {
            SetState(state.endAttack);
        }
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        UpdateStates();
    }

    //  3D 사정거리 감지 (Physics.OverlapBox)
    bool IsPlayerInRange()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        // 💥 Physics2D.OverlapBoxAll를 3D용 Physics.OverlapBox로 변경!
        Collider[] hitTargets = Physics.OverlapBox(worldCenter, hitboxSize / 2f, Quaternion.identity, playerLayer);

        if (hitTargets.Length > 0)
        {
            foreach (Collider targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator AttackHandler()
    {
        while (true)
        {
            if (anim != null)
            {
                anim.Play("Armature|Armature|Attack", 0, 0f);
            }

            yield return new WaitForSeconds(attackChargeTime);

            Attack();

            yield return new WaitForSeconds(attackCooldown);

            if (!IsPlayerInRange()) break;
        }

        if (anim != null) anim.Play("Armature|Armature|Idle", 0, 0f);

        SetState(state.track);
    }

    //  3D 진짜 데미지 처리 (Physics.OverlapBox)
    void Attack()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        // 💥 3D 공간의 플레이어 캡슐/박스 콜라이더를 정확히 탐색합니다.
        Collider[] hitTargets = Physics.OverlapBox(worldCenter, hitboxSize / 2f, Quaternion.identity, playerLayer);

        if (hitTargets.Length > 0)
        {
            foreach (Collider targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    PlayerMovement.Instance.GetDamage(damage, transform);
                    break;
                }
            }
        }
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }

    //  3D 시야 감지 (Physics.OverlapBox + 3D Raycast)
    bool IsPlayerInView()
    {
        Vector3 localAdjustedOffset = new Vector3(viewOffset.x * facingSign, viewOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        // 💥 시야 감지 영역 역시 3D로 바꿉니다.
        Collider[] hitTargets = Physics.OverlapBox(worldCenter, viewSize / 2f, Quaternion.identity, playerLayer);

        bool isPlayerInView = false;

        if (hitTargets.Length > 0)
        {
            foreach (Collider targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    isPlayerInView = true;
                    break;
                }
            }
        }

        if (!isPlayerInView) return false;

        Vector3 startPos = transform.position;
        Vector3 endPos = playerObject.transform.position;
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        //  3D 물리(Physics.Raycast)를 쓰도록 변경합니다.
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, distance, obstacleMask))
        {
            if (hit.collider != null) return false;
        }
        return true;
    }

    //  지형 체크 레이캐스트 (Physics.OverlapSphere)
    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        // 💥 바닥 감지 및 벽 감지 센서 레이더들을 전부 3D(Physics.OverlapSphere)로 교체합니다.
        bool upperGroundDetect = Physics.CheckSphere(upperGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool lowerGroundDetect = Physics.CheckSphere(lowerGroundCheckPos.position, layerCheckRadius, obstacleMask);

        bool isGrounded = upperGroundDetect || lowerGroundDetect;
        bool isTouchingAnyWall = Physics.CheckSphere(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;
    }

    public void GetDamage(float damage, Transform attackerTransform)
    {
        currentHp -= damage;

        if (hpBar != null)
            hpBar.SetHP(currentHp, maxHp);

        if (currentHp <= 0f)
        {
            Dead();
        }
    }

    void Dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (enemyData != null && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(enemyData.dropGold);
            CurrencyManager.Instance.AddGem(enemyData.dropGem);
        }

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.EnemyDefeated();
        if (hpBar != null) Destroy(hpBar.gameObject);

        Destroy(gameObject, 1.0f);
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 hitboxLocalAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 hitboxGizmoCenter = transform.position + hitboxLocalAdjustedOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);

        Gizmos.color = Color.blue;
        Vector3 viewLocalAdjustedOffset = new Vector3(viewOffset.x * facingSign, viewOffset.y, 0f);
        Vector3 viewGizmoCenter = transform.position + viewLocalAdjustedOffset;
        Gizmos.DrawWireCube(viewGizmoCenter, viewSize);

        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(movePosRight, 0.25f);
                Gizmos.DrawWireSphere(movePosLeft, 0.25f);
                Gizmos.DrawLine(movePosRight, movePosLeft);
            }
        }
        else
        {
            Vector3 gizmosMovePosRight = transform.position;
            Vector3 gizmosMovePosLeft = transform.position;
            gizmosMovePosRight.x += moveRadius;
            gizmosMovePosLeft.x -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosRight, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosLeft, 0.25f);
            Gizmos.DrawLine(gizmosMovePosRight, gizmosMovePosLeft);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(upperGroundCheckPos.position, 0.05f);
        Gizmos.DrawWireSphere(lowerGroundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
    }
}