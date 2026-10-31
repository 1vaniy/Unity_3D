using UnityEngine;

public class AxeShake : MonoBehaviour
{
    public float shakeSpeed = 50f;
    public float shakeAmount = 3f;

    private Quaternion originalRotation;
    private Collider axeCollider;

    void Start()
    {
        originalRotation = transform.rotation;
        axeCollider = GetComponent<Collider>();

        // На старте игры отключаем коллайдер, чтобы топор не толкался с пеньком
        if (axeCollider != null) axeCollider.enabled = false;
    }

    void Update()
    {
        // Дрожание (визуальное)
        float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;
        float offsetZ = Mathf.Sin(Time.time * shakeSpeed * 0.8f) * shakeAmount;

        transform.rotation = originalRotation * Quaternion.Euler(offsetX, offsetY, offsetZ);
    }
}