using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class ArrowAttackEnemy : MonoBehaviour, IEnemyCombat
{
    [Header("체력")]
    public float maxHp = 30f;   // 최대 체력

    [Header("적 데이터")]
    public EnemyData enemyData; // EnemyData 에셋을 드래그 앤 드롭

    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform upperGroundCheckPos;    // 땅 위쪽
    public Transform lowerGroundCheckPos;    // 땅 아래쪽

    [Header("공격 시간")]
    public float readyToAttackTime = 0.5f;  // 공격으로 전환 시간
    public float attackChargeTime = 0.42f;  // 공격의 준비 시간
    public float attackCooldown = 0.35f;    // 공격 대기시간 (공격 쿨타임)

    [Header("공격 화살 / 대미지")]
    public EnemyArrowController arrowPrefab;   // 발사할 화살 프리팹
    public Transform firePoint;   // 화살 발사 위치
    public float arrowSpeed = 10f;   // 화살 속도
    public float damage = 0.4f;  // 공격 대미지

    [Header("히트박스")]
    public Vector3 hitboxOffset = Vector3.zero;    
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f); 
    [Header("시야 범위")]
    public Vector3 viewOffset = new Vector3(0f, 0.5f, 0f);
    public Vector3 viewSize = new Vector3(5f, 3f, 5f);     

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

    private bool isFacingRight = true;
    private bool canGoStraight = true;
    private bool isDead = false;

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack, endAttack }
    state currentState;

   
    private Rigidbody rb;
    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
    {
       
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            playerObject = PlayerMovement.Instance.gameObject;
        }

        isFacingRight = true;
        currentHp = maxHp;
        SetState(state.idle);

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.AddEnemy();

        // 중복 코드를 깔끔하게 하나로 합쳤습니다.
        if (hpBarPrefab != null && UIManager.Instance != null && UIManager.Instance.worldCanvas != null)
        {
            GameObject ui = Instantiate(hpBarPrefab, UIManager.Instance.worldCanvas.transform);
            hpBar = ui.GetComponent<EnemyHPBarFollow>();
            hpBar.target = transform;
            hpBar.offset = new Vector3(0, 1.2f, 0); // 머리 위 위치
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateStates();
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

        
        Vector3 originVelocity = rb.linearVelocity;
        originVelocity.x = 0;
        rb.linearVelocity = originVelocity;
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
               
                rb.linearVelocity = new Vector3(sign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y, 0f);
                yield return null;
            }
            Vector3 originVelocity = rb.linearVelocity;
            originVelocity.x = 0;
            rb.linearVelocity = originVelocity;
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
            
            rb.linearVelocity = new Vector3(facingSign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y, 0f);
        }
        else
        {
            Vector3 originVelocity = rb.linearVelocity;
            originVelocity.x = 0;
            rb.linearVelocity = originVelocity;
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

    
    bool IsPlayerInRange()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        // 💥 3D OverlapBox 레이더를 돌려 3D 플레이어를 정상 감지합니다.
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
            yield return new WaitForSeconds(attackChargeTime);
            Attack();
            yield return new WaitForSeconds(attackCooldown);

            if (!IsPlayerInRange()) break;
        }

        SetState(state.track);
    }

    void Attack()
    {
       
        EnemyArrowController arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        arrow.damage = damage;

       
        Vector3 shootDirection = isFacingRight ? Vector3.right : Vector3.left;
        arrow.Shoot(shootDirection, arrowSpeed);
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }

    
    bool IsPlayerInView()
    {
        Vector3 localAdjustedOffset = new Vector3(viewOffset.x * facingSign, viewOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        
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

        
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, distance, obstacleMask))
        {
            if (hit.collider != null) return false;
        }
        return true;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        
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

        if (enemyData != null && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(enemyData.dropGold);
            CurrencyManager.Instance.AddGem(enemyData.dropGem);
        }

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.EnemyDefeated();
        if (hpBar != null) Destroy(hpBar.gameObject);

        Destroy(gameObject);
    }

   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 hitboxLocalAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 hitboxGizmoCenter = transform.position + hitboxLocalAdjustedOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);

        if (firePoint != null) Gizmos.DrawWireSphere(firePoint.position, 0.1f);

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

        if (upperGroundCheckPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(upperGroundCheckPos.position, 0.05f);
        }
        if (lowerGroundCheckPos != null)
        {
            Gizmos.DrawWireSphere(lowerGroundCheckPos.position, 0.05f);
        }
        if (wallCheckPos != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        }
    }
}