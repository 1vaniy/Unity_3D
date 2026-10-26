using UnityEngine;
using UnityEngine.InputSystem; // Нужно только для чтения клавиатуры

[RequireComponent(typeof(CharacterController))]
public class PlayerGoWController : MonoBehaviour
{
    [Header("Настройки")]
    
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 7f;

    private CharacterController controller;
    private Animator animator;
    private Vector2 moveInput;
    private Vector3 velocity;
    private Transform cam;

    private int moveXHash;
    private int moveYHash;
    private int isMovingHash;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cam = Camera.main != null ? Camera.main.transform : null;

        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");
    }

    void Update()
    {
        moveInput = Vector2.zero;
        var kb = Keyboard.current;
        bool isRunning = false;

        if (kb != null)
        {
            if (kb.wKey.isPressed) moveInput.y += 1f;
            if (kb.sKey.isPressed) moveInput.y -= 1f;
            if (kb.aKey.isPressed) moveInput.x -= 1f;
            if (kb.dKey.isPressed) moveInput.x += 1f;

            isRunning = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.0001f;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Берем направление от Main Camera (теперь это камера-орбита)
        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (cam != null)
        {
            camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        }
        Vector3 worldMove = (camForward * moveInput.y + camRight * moveInput.x);

        // --- ПОВОРОТ ПЕРСОНАЖА ---
        // Если есть ввод (мы идем или бежим), персонаж поворачивается.
        // Если стоим на месте, он НЕ поворачивается за мышкой (смотрим ему в лицо).
        if (isMoving && cam != null)
        {
            Vector3 lookDirection = isRunning ? worldMove : camForward;
            Quaternion targetRot = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 10f * Time.deltaTime);
        }

        if (isMoving)
            controller.Move(worldMove.normalized * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- АНИМАТОР ---
        if (animator != null)
        {
            Vector3 local = transform.InverseTransformDirection(worldMove);
            float targetX = isRunning ? 0f : local.x;
            float targetY = local.z;

            float currentX = animator.GetFloat(moveXHash);
            float currentY = animator.GetFloat(moveYHash);

            float smoothX = Mathf.Lerp(currentX, targetX, Time.deltaTime * 15f);
            float smoothY = Mathf.Lerp(currentY, targetY, Time.deltaTime * 15f);

            animator.SetFloat(moveXHash, smoothX);
            animator.SetFloat(moveYHash, smoothY);
            animator.SetBool(isMovingHash, isMoving);
            animator.SetBool("IsRunning", isRunning);
        }
    }
}

