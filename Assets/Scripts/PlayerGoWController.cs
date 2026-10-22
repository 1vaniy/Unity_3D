using UnityEngine;
using UnityEngine.InputSystem; // Нужно только для чтения клавиатуры

[RequireComponent(typeof(CharacterController))]
public class PlayerGoWController : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float rotationSpeed = 10f;

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
        // --- ЧТЕНИЕ КЛАВИАТУРЫ НАПРЯМУЮ ---
        moveInput = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed) moveInput.y += 1f;
            if (kb.sKey.isPressed) moveInput.y -= 1f;
            if (kb.aKey.isPressed) moveInput.x -= 1f;
            if (kb.dKey.isPressed) moveInput.x += 1f;
        }
        // -----------------------------------

        bool isMoving = moveInput.sqrMagnitude > 0.0001f;

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (cam != null)
        {
            camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        }
        Vector3 worldMove = (camForward * moveInput.y + camRight * moveInput.x);

        // Поворот
        if (isMoving && cam != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * 10f * Time.deltaTime);
        }

        // Движение
        if (isMoving)
            controller.Move(worldMove.normalized * moveSpeed * Time.deltaTime);

        // Гравитация
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Аниматор
        if (animator != null)
        {
            Vector3 local = transform.InverseTransformDirection(worldMove);

            float currentX = animator.GetFloat(moveXHash);
            float currentY = animator.GetFloat(moveYHash);

            float smoothX = Mathf.Lerp(currentX, local.x, Time.deltaTime * 15f);
            float smoothY = Mathf.Lerp(currentY, local.z, Time.deltaTime * 15f);

            animator.SetFloat(moveXHash, smoothX);
            animator.SetFloat(moveYHash, smoothY);
            animator.SetBool(isMovingHash, isMoving);
        }
    }
}