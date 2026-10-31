using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerGoWController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] public float walkSpeed = 3f;
    [SerializeField] public float runSpeed = 7f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float rotationSpeed = 10f;

    private float originalWalkSpeed;
    private float originalRunSpeed;

    private CharacterController controller;
    private Animator animator;
    private PlayerCombat combat;

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
        combat = GetComponent<PlayerCombat>();
        cam = Camera.main != null ? Camera.main.transform : null;

        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");

        originalWalkSpeed = walkSpeed;
        originalRunSpeed = runSpeed;
    }

    public bool IsRunning()
    {
        // Если прицеливаемся, бег запрещен
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.isAiming) return false;

        var kb = Keyboard.current;
        return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
    }

    void Update()
    {
        // --- БЛОКИРОВКА ВО ВРЕМЯ ПОЛУЧЕНИЯ УРОНА ---
        if (combat != null && combat.isHit)
        {
            moveInput = Vector2.zero;
            velocity.x = 0f;
            velocity.z = 0f;

            if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

            // Сбрасываем анимацию ходьбы
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveYHash, 0f);
            animator.SetBool(isMovingHash, false);
            return;
        }

        // --- БЛОКИРОВКА ВО ВРЕМЯ АТАКИ И БРОСКА ---
        if (combat != null && (combat.isAttacking || combat.isThrowing || combat.isHit))
        {
            moveInput = Vector2.zero;
            velocity.x = 0f;
            velocity.z = 0f;

            if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

            // Сбрасываем анимацию ходьбы, чтобы не было дерганий
            if (animator != null)
            {
                
            }
            return;
        }

        // --- 2. ВО ВРЕМЯ УДАРА С РАЗБЕГА ---
        if (combat != null && combat.isRunAttacking)
        {
            // Жестко обнуляем скорость, чтобы не было инерции после остановки
            velocity.x = 0f;
            velocity.z = 0f;

            // Применяем только гравитацию
            if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

            // Движение вперед делает скрипт PlayerCombat
            return;
        }

        // --- 3. ОБЫЧНОЕ ДВИЖЕНИЕ (ЧТЕНИЕ ВВОДА) ---
        moveInput = Vector2.zero;
        var kb = Keyboard.current;
        bool isRunning = false;

        if (kb != null)
        {
            if (kb.wKey.isPressed) moveInput.y += 1f;
            if (kb.sKey.isPressed) moveInput.y -= 1f;
            if (kb.aKey.isPressed) moveInput.x -= 1f;
            if (kb.dKey.isPressed) moveInput.x += 1f;
            isRunning = IsRunning(); // Используем наш метод, который учитывает прицеливание
        }

        bool isMoving = moveInput.sqrMagnitude > 0.0001f;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (cam != null)
        {
            camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        }
        Vector3 worldMove = (camForward * moveInput.y + camRight * moveInput.x);

        // --- ПОВОРОТ ---
        if (isMoving && cam != null)
        {
            Vector3 lookDirection = isRunning ? worldMove : camForward;
            Quaternion targetRot = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 10f * Time.deltaTime);
        }

        // --- ДВИЖЕНИЕ ---
        if (isMoving)
            controller.Move(worldMove.normalized * currentSpeed * Time.deltaTime);

        // --- ГРАВИТАЦИЯ ---
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

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
    public void EnableSpeedBoost(float multiplier)
    {
        walkSpeed = originalWalkSpeed * multiplier;
        runSpeed = originalRunSpeed * multiplier;
    }

    public void DisableSpeedBoost()
    {
        walkSpeed = originalWalkSpeed;
        runSpeed = originalRunSpeed;
    }
}