using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    [Header("Цель (Пивот на персонаже)")]
    public Transform targetPivot;

    [Header("Настройки дистанции")]
    public float distance = 4f;        // Насколько далеко камера от персонажа
    public float height = 1.5f;        // На какой высоте висит камера (на уровне плеч)

    [Header("Смещение через плечо (God of War)")]
    public float shoulderOffsetX = 0.6f; // Сдвиг вправо
    public float shoulderOffsetY = 0.2f; // Сдвиг вверх

    [Header("Чувствительность")]
    public float mouseSensitivity = 0.15f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    private float yaw = 0f;
    private float pitch = 20f; // Начальный наклон сверху вниз

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (targetPivot == null) return;

        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 1. Вычисляем поворот на основе мыши
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 2. Считаем позицию камеры (сзади, сбоку и сверху)
        // Начинаем с точки, поднятой на высоту height
        Vector3 basePosition = targetPivot.position + Vector3.up * height;

        // Отодвигаем камеру назад на distance и применяем поворот мыши
        Vector3 cameraPosition = basePosition - (rotation * Vector3.forward * distance);

        // Добавляем смещение вправо и вверх (в локальных осях камеры)
        cameraPosition += rotation * new Vector3(shoulderOffsetX, shoulderOffsetY, 0f);

        // 3. Применяем позицию
        transform.position = cameraPosition;

        // 4. Камера смотрит не на пивот, а чуть впереди него, чтобы персонаж был сбоку
        Vector3 lookTarget = basePosition + (rotation * new Vector3(shoulderOffsetX, shoulderOffsetY, 0f));
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);
    }
}