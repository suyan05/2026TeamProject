using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float walkSpeed = 3.0f;       // 걷기 속도
    public float runSpeed = 6.0f;        // 뛰기 속도
    public float rotationSpeed = 720.0f; // 회전 속도

    private CharacterController controller;
    private Animator animator;
    private string currentAnimation = "";

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 마우스 좌클릭 시 공격 애니메이션 실행
        if (Input.GetMouseButtonDown(0))
        {
            PlayAnimation("Attack");
            return; // 공격 시작 순간에는 아래 이동 로직을 타지 않음
        }

        // 2. 현재 공격 애니메이션이 재생 중이라면 이동 및 다른 애니메이션 차단
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack") && stateInfo.normalizedTime < 1.0f)
            {
                return; // 공격이 완전히 끝날 때까지 아래 이동 코드를 실행하지 않음
            }
        }

        // 3. 이동 입력 받기 (W, A, S, D 또는 방향키)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // 4. 이동 및 애니메이션 처리
        if (moveDirection.magnitude >= 0.1f)
        {
            // Shift 키를 누르고 있으면 달리기, 아니면 걷기
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            string targetAnimation = isRunning ? "Run" : "Walk";

            // 캐릭터 회전
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // 캐릭터 이동
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            // 이동 애니메이션 (Walk 또는 Run) 재생
            PlayAnimation(targetAnimation);
        }
        else
        {
            // 입력이 없으면 대기(Idle) 애니메이션 재생
            PlayAnimation("Idle");
        }
    }

    /// <summary>
    /// 화살표 없이 코드로 애니메이션을 부드럽게 바꾸는 메서드
    /// </summary>
    void PlayAnimation(string animName)
    {
        if (animator == null) return;

        // 이미 해당 애니메이션이 재생 중이거나 전환 중이면 중복 실행 방지
        if (currentAnimation == animName || animator.IsInTransition(0)) return;

        // 0.1초 동안 부드럽게 섞이면서 전환
        animator.CrossFadeInFixedTime(animName, 0.1f);
        currentAnimation = animName;
    }
}