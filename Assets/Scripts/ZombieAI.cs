using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ZombieAI : MonoBehaviour
{
    [Header("Цель")]
    public Transform player; // Перетащи сюда игрока

    [Header("Настройки движения")]
    public float moveSpeed = 1.5f;        // Скорость зомби
    public float rotationSpeed = 5f;      // Скорость разворота
    public float stopDistance = 1.5f;     // На каком расстоянии останавливаться

    private CharacterController controller;
    private Animator animator;
    private float gravity = -25f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        // Гравитация (чтобы не проваливался)
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Если мы дальше, чем stopDistance, идем к игроку
        if (distanceToPlayer > stopDistance)
        {
            // Направление на игрока
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Чтобы не наклонялся вперед

            // Поворот к игроку
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Движение
            controller.Move(transform.forward * moveSpeed * Time.deltaTime);

            // Анимация ходьбы
            animator.SetFloat("Speed", 1f);
        }
        else
        {
            // Если близко — стоим
            animator.SetFloat("Speed", 0f);
        }
    }
}