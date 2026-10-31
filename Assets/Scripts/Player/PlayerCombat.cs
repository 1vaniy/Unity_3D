using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Õ‡ÒÚÓÈÍË ÍÓÏ·Ó (ÒÚÓˇ / ıÓ‰¸·‡)")]
    public bool isAttacking = false;
    public float inputBufferWindow = 0.5f;
    public float attackForwardSpeed = 2f;
    public float attackMoveDelay = 0.2f;

    [Header("”ÓÌ ÍÓÏ·Ó")]
    public float damage = 25f;
    public float attackRadius = 2f;

    [Header("”‰‡ Ò ‡Á·Â„‡ (Shift + À Ã)")]
    public bool isRunAttacking = false;
    public float runAttackForwardSpeed = 8f;
    [Range(0f, 1f)]
    public float runAttackStopProgress = 0.6f;
    public float runAttackDamage = 100f;

    [Header("œÓÎÛ˜ÂÌËÂ ÛÓÌ‡")]
    public bool isHit = false;
    public float hitStunDuration = 0.5f;

    [Header("—ÏÂÚ¸")]
    public bool isDead = false;

    [Header("“ÓÔÓ (œËˆÂÎË‚‡ÌËÂ)")]
    public AxeController axe;
    public bool isAiming = false;
    public bool isThrowing = false;

    private Animator animator;
    private PlayerGoWController movementScript;
    private Transform cam;

    private int comboStep = 0;
    private float attackStartTime;
    private bool queuedNextAttack = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<PlayerGoWController>();
        cam = Camera.main.transform;

        Health health = GetComponent<Health>();
        if (health != null) health.OnDeath += Die;
    }

    void Update()
    {
        if (isDead) return;

        // —·ÓÒ ÙÎ‡„‡ ·ÓÒÍ‡
        if (isThrowing)
        {
            AnimatorStateInfo throwState = animator.GetCurrentAnimatorStateInfo(0);
            if (!throwState.IsName("Throw") && !animator.IsInTransition(0))
            {
                isThrowing = false;
            }
        }

        // --- ÀŒ√» ¿ œ–»÷≈À»¬¿Õ»ﬂ » ¡–Œ— ¿ (œ Ã) ---
        if (Mouse.current != null)
        {
            bool isCurrentlyRunning = movementScript != null && movementScript.IsRunning();

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (axe != null)
                {
                    if (axe.currentState == AxeController.AxeState.Held)
                    {
                        if (!isCurrentlyRunning && !isAttacking && !isRunAttacking && !isThrowing && !animator.IsInTransition(0))
                        {
                            isAiming = true;
                        }
                    }
                    else if (axe.currentState == AxeController.AxeState.Stuck || axe.currentState == AxeController.AxeState.Flying)
                    {
                        axe.ReturnAxe();
                    }
                }
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                if (isAiming && axe != null && axe.currentState == AxeController.AxeState.Held)
                {
                    isThrowing = true;
                    animator.SetTrigger("Throw");
                    isAiming = false;
                }
                else
                {
                    isAiming = false;
                }
            }
        }

        if (isHit) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool inAttackState = stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2") || stateInfo.IsName("Attack3");
        bool inRunAttackState = stateInfo.IsName("RunAttack");

        // --- —¡–Œ— ‘À¿√Œ¬ ---
        if (isAttacking && !inAttackState && !animator.IsInTransition(0))
        {
            isAttacking = false;
            comboStep = 0;
            queuedNextAttack = false;
        }

        if (isRunAttacking)
        {
            float progress = stateInfo.normalizedTime % 1f;
            if ((inRunAttackState && progress >= 0.99f) || (!inRunAttackState && !animator.IsInTransition(0)))
            {
                isRunAttacking = false;
            }
        }

        // --- ÀŒ√» ¿ Œ¡€◊Õ€’ ”ƒ¿–Œ¬ ---
        if (isAttacking)
        {
            if (Time.time - attackStartTime < 0.15f && cam != null)
            {
                Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                if (camForward != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(camForward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360f * Time.deltaTime);
                }
            }

            if (Time.time - attackStartTime > attackMoveDelay)
            {
                GetComponent<CharacterController>().Move(transform.forward * attackForwardSpeed * Time.deltaTime);
            }
        }
        // --- ÀŒ√» ¿ ”ƒ¿–¿ — –¿«¡≈√¿ ---
        else if (isRunAttacking)
        {
            float progress = stateInfo.normalizedTime % 1f;
            if (progress < runAttackStopProgress)
            {
                GetComponent<CharacterController>().Move(transform.forward * runAttackForwardSpeed * Time.deltaTime);
            }
        }

        // --- ◊“≈Õ»≈ ¬¬Œƒ¿ (À Ã) ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isAiming || isThrowing) return;

            bool isRunning = movementScript != null && movementScript.IsRunning();

            if (!animator.IsInTransition(0) && !isAttacking && !isRunAttacking)
            {
                if (isRunning)
                {
                    StartRunAttack();
                }
                else
                {
                    StartAttack(1);
                }
            }
            else if (isAttacking && inAttackState && comboStep < 3)
            {
                float progress = stateInfo.normalizedTime % 1f;
                if (progress >= inputBufferWindow)
                {
                    queuedNextAttack = true;
                }
            }
        }

        // --- ¡”‘≈–  ŒÃ¡Œ ---
        if (isAttacking && queuedNextAttack && inAttackState)
        {
            float progress = stateInfo.normalizedTime % 1f;
            if (progress >= 0.99f)
            {
                if (comboStep == 1) { StartAttack(2); queuedNextAttack = false; }
                else if (comboStep == 2) { StartAttack(3); queuedNextAttack = false; }
            }
        }
    }

    void StartAttack(int step)
    {
        isAttacking = true;
        comboStep = step;
        attackStartTime = Time.time;
        queuedNextAttack = false;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("AttackRun");

        animator.SetTrigger("Attack" + step);
    }

    void StartRunAttack()
    {
        isRunAttacking = true;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        animator.SetTrigger("AttackRun");
    }

    public void ReleaseAxe()
    {
        if (axe != null && axe.currentState == AxeController.AxeState.Held)
        {
            axe.ThrowAxe();
        }
    }

    public void PlayHitReaction()
    {
        PlayerRage rage = GetComponent<PlayerRage>();
        if (rage != null && rage.IsRaging) return;

        if (rage != null) rage.AddRage(rage.rageOnTakeDamage);

        isAttacking = false;
        isRunAttacking = false;
        isThrowing = false;
        comboStep = 0;
        queuedNextAttack = false;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("AttackRun");
        animator.ResetTrigger("Throw");

        isHit = true;
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");

        StopAllCoroutines();
        StartCoroutine(HitStun());
    }

    private System.Collections.IEnumerator HitStun()
    {
        yield return new WaitForSeconds(hitStunDuration);
        isHit = false;
    }

    public void DealDamageToEnemy()
    {
        if (isRunAttacking || isDead || isAiming) return;

        float currentDamage = damage;
        if (axe != null && axe.currentState != AxeController.AxeState.Held)
        {
            currentDamage = 1f;
        }

        PlayerRage rage = GetComponent<PlayerRage>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward, attackRadius);

        foreach (var hitCollider in hitColliders)
        {
            // »«Ã≈Õ≈Õ»≈: »˘ÂÏ IDamageable
            IDamageable target = hitCollider.GetComponent<IDamageable>();
            if (target != null && hitCollider.gameObject != gameObject)
            {
                target.TakeDamage(currentDamage);

                if (rage != null) rage.AddRage(rage.rageOnHit);

                ZombieAI zombie = hitCollider.GetComponent<ZombieAI>();
                if (zombie != null)
                {
                    zombie.TakeDamageReaction(comboStep);
                }

                if (rage != null && rage.IsRaging)
                {
                    IDamageable myHealth = GetComponent<IDamageable>();
                    if (myHealth != null) myHealth.Heal(rage.healPerHit);
                }
            }
        }
    }

    public void DealDamageToEnemyRunAttack()
    {
        if (isDead) return;

        float currentDamage = runAttackDamage;
        if (axe != null && axe.currentState != AxeController.AxeState.Held)
        {
            currentDamage = 1f;
        }

        PlayerRage rage = GetComponent<PlayerRage>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward, attackRadius);

        foreach (var hitCollider in hitColliders)
        {
            // »«Ã≈Õ≈Õ»≈: »˘ÂÏ IDamageable
            IDamageable target = hitCollider.GetComponent<IDamageable>();
            if (target != null && hitCollider.gameObject != gameObject)
            {
                target.TakeDamage(currentDamage);

                if (rage != null) rage.AddRage(rage.rageOnHit);

                if (rage != null && rage.IsRaging)
                {
                    IDamageable myHealth = GetComponent<IDamageable>();
                    if (myHealth != null) myHealth.Heal(rage.healPerHit);
                }
            }
        }
    }

    private void Die()
    {
        isDead = true;
        isAiming = false;
        isThrowing = false;
        isAttacking = false;
        isRunAttacking = false;
        isHit = false;

        if (movementScript != null) movementScript.enabled = false;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        this.enabled = false;
        animator.SetTrigger("Death");

        DeathScreen deathScreen = FindObjectOfType<DeathScreen>();
        if (deathScreen != null)
        {
            Invoke(nameof(ShowDeathMenuDelayed), 4f);
        }
    }

    private void ShowDeathMenuDelayed()
    {
        DeathScreen deathScreen = FindObjectOfType<DeathScreen>();
        if (deathScreen != null) deathScreen.ShowDeathScreen();
    }
}