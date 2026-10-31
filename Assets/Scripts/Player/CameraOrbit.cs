using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    [Header("Цель (Пивот на персонаже)")]
    public Transform targetPivot;

    [Header("Настройки дистанции")]
    public float distance = 4f;
    public float height = 1.5f;

    [Header("Смещение через плечо (God of War)")]
    public float shoulderOffsetX = 0.6f;
    public float shoulderOffsetY = 0.2f;

    [Header("Чувствительность")]
    public float mouseSensitivity = 0.15f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Зум (Прицеливание)")]
    public float defaultFov = 60f;
    public float aimFov = 40f;
    public float zoomSpeed = 10f;

    [Header("Коллизии камеры (Стены)")]
    public LayerMask collisionMask; // Сюда выбрать слой, где деревья и стены
    public float collisionRadius = 0.2f; // Толщина луча (чтобы не проваливаться в мелкие щели)
    public float minDistance = 0.5f;     // Минимальная дистанция (чтобы камера не залезла в игрока)

    private float yaw = 0f;
    private float pitch = 20f;
    private float currentDistance; // Текущая плавная дистанция
    private PlayerCombat playerCombat;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (targetPivot != null)
            playerCombat = targetPivot.GetComponentInParent<PlayerCombat>();

        currentDistance = distance;
    }
    public void SetAngles(float newYaw, float newPitch)
    {
        yaw = newYaw;
        pitch = newPitch;
    }
    void LateUpdate()
    {
        if (targetPivot == null) return;
        if (Time.timeScale == 0f) return;
        // 1. Читаем мышь
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 2. Вычисляем поворот
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 3. Точка пивота (голова)
        Vector3 basePosition = targetPivot.position + Vector3.up * height;

        // 4. Направление от камеры к пивоту
        Vector3 cameraDirection = -(rotation * Vector3.forward);

        // Смещение через плечо
        Vector3 shoulderOffset = rotation * new Vector3(shoulderOffsetX, shoulderOffsetY, 0f);

        // 5. ПРОВЕРКА СТОЛКНОВЕНИЙ
        float targetDistance = distance;
        RaycastHit hit;

        // Луч 1: Из центра пивота назад (проверяем спину)
        if (Physics.SphereCast(basePosition, collisionRadius, cameraDirection, out hit, distance, collisionMask))
        {
            targetDistance = hit.distance - collisionRadius;
        }

        // Луч 2: Из смещенной точки (проверяем бок, где висит камера)
        // Мы берем точку пивота + смещение и пускаем луч оттуда
        Vector3 offsetBasePosition = basePosition + shoulderOffset;
        if (Physics.SphereCast(offsetBasePosition, collisionRadius, cameraDirection, out hit, distance, collisionMask))
        {
            // Если бок нашел стену раньше, чем центр — берем это расстояние
            float sideDistance = hit.distance - collisionRadius;
            if (sideDistance < targetDistance) targetDistance = sideDistance;
        }

        // Ограничиваем минимальную дистанцию
        targetDistance = Mathf.Clamp(targetDistance, minDistance, distance);

        // Плавно меняем дистанцию
        float lerpSpeed = targetDistance < currentDistance ? 30f : 5f;
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * lerpSpeed);

        // 6. ПРИМЕНЯЕМ ПОЗИЦИЮ
        Vector3 cameraPosition = basePosition + (cameraDirection * currentDistance) + shoulderOffset;
        transform.position = cameraPosition;

        // 7. Камера смотрит на пивот
        Vector3 lookTarget = basePosition + shoulderOffset;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);

        // 8. ЗУМ ПРИ ПРИЦЕЛИВАНИИ
        if (playerCombat != null && Camera.main != null)
        {
            float targetFov = playerCombat.isAiming ? aimFov : defaultFov;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
        }
    }

}