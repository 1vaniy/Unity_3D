using UnityEngine;

public class AxeController : MonoBehaviour
{
    [Header("Настройки полета")]
    public float throwSpeed = 25f;
    public float rotationSpeed = 1500f;
    public float maxDistance = 30f;

    [Header("Ориентация модели")]
    public Vector3 axeVisualRotation = new Vector3(0, 90, 0);

    [Header("Возврат (Боевой)")]
    public float returnSpeed = 40f;
    public Transform axePivot;
    public Transform playerCamera;

    [Header("Возврат в Меню (Кинематографичный)")]
    public float menuReturnSpeed = 10f;
    public float menuRotationSpeed = 500f;

    [Header("Урон")]
    public float damage = 40f;
    public float hitRadius = 0.3f;

    public enum AxeState { Held, Flying, Stuck, Returning }
    public AxeState currentState = AxeState.Held;

    private Vector3 startPosition;
    private Rigidbody rb;
    private Collider col;
    private Vector3 throwDirection;

    private bool isMenuFlight = false;

    // Запоминаем, где топор лежал в меню
    private Vector3 menuStartPosition;
    private Quaternion menuStartRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        currentState = AxeState.Held;
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;

        // Запоминаем позицию на пеньке
        menuStartPosition = transform.position;
        menuStartRotation = transform.rotation;

        if (axePivot != null && currentState == AxeState.Held)
        {
            transform.SetParent(axePivot);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (currentState == AxeState.Flying) Fly();
        else if (currentState == AxeState.Returning) ReturnToHand();
    }

    public void ThrowAxe()
    {
        if (currentState != AxeState.Held) return;

        isMenuFlight = false;
        transform.SetParent(null);
        currentState = AxeState.Flying;
        startPosition = transform.position;
        if (col != null) col.enabled = true;
        if (rb != null) rb.isKinematic = true;

        throwDirection = playerCamera.forward;
        transform.rotation = Quaternion.LookRotation(throwDirection) * Quaternion.Euler(axeVisualRotation);
    }

    private void Fly()
    {
        transform.position += throwDirection * throwSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, hitRadius, throwDirection, out hit, 0.5f))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);

                ZombieAI zombie = hit.collider.GetComponent<ZombieAI>();
                if (zombie != null) zombie.TakeDamageReaction(2);

                StuckInPlace();
            }
            else if (hit.collider.CompareTag("Ground"))
            {
                StuckInPlace();
            }
        }

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
            StuckInPlace();
    }

    private void StuckInPlace()
    {
        currentState = AxeState.Stuck;
        if (col != null) col.enabled = false;
    }

    public void ReturnAxe()
    {
        if (currentState == AxeState.Stuck || currentState == AxeState.Flying)
        {
            isMenuFlight = false;
            currentState = AxeState.Returning;
            if (col != null) col.enabled = false;
        }
    }

    public void StartMenuFlight()
    {
        // Принудительно ставим топор в его стартовую позицию на пеньке
        transform.SetParent(null);
        transform.position = menuStartPosition;
        transform.rotation = menuStartRotation;

        isMenuFlight = true;
        currentState = AxeState.Returning;
        if (col != null) col.enabled = false;
        if (rb != null) rb.isKinematic = true;
    }

    private void ReturnToHand()
    {
        if (axePivot == null) return;

        float currentSpeed = isMenuFlight ? menuReturnSpeed : returnSpeed;
        float currentRotSpeed = isMenuFlight ? menuRotationSpeed : rotationSpeed;

        transform.position = Vector3.MoveTowards(transform.position, axePivot.position, currentSpeed * Time.deltaTime);

        float distanceToHand = Vector3.Distance(transform.position, axePivot.position);

        if (distanceToHand > 1.5f)
        {
            transform.Rotate(Vector3.forward, -currentRotSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            Quaternion targetRot = axePivot.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
        }

        if (distanceToHand < 0.5f)
        {
            transform.SetParent(axePivot);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            currentState = AxeState.Held;
            if (col != null) col.enabled = false;
        }
    }
}