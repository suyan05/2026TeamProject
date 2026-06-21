using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;       // �̵� �ӵ�
    public float rotationSpeed = 720.0f; // ȸ�� �ӵ�

    private CharacterController controller;
    private Animator animator;

    void Start()
    {
        // ĳ���Ϳ� �پ��ִ� ������Ʈ���� �����ɴϴ�.
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // W, A, S, D �Ǵ� ����Ű �Է��� �□��ϴ�. (���� ����: -1.0f ~ 1.0f)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // �̵� ���� ���� ���
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // �Է��� ���� ���� ĳ���□� �����̰� ȸ����ŵ�ϴ�.
        if (moveDirection.magnitude >= 0.1f)
        {
            // 1. �ٶ□� �������� ĳ���� ȸ��
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // 2. ĳ���� �̵�
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // 3. �ִϸ������� Speed �Ķ���Ϳ� ���� �̵� �ӵ�(ũ��)�� �����մϴ�.
        // �Է��� ������ 0, ������ 1�� ����� ���� ��□ �ִϸ��̼��� �ٲ�ϴ�.
        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
    }
}