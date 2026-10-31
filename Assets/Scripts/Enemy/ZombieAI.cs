using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class ZombieAI : MonoBehaviour
{
    [Header("Цель")]
    public Transform player;

    [Header("Движение")]
    public float moveSpeed = 1.5f;
    public float rotationSpeed = 5f;

    [Header("Атака")]
    public float attackDistance = 1.5f;
    public float attackCooldown = 2f;

    [Header("Урон")]
    public float damage = 15f;

    [Header("Стан (Реакция на удар)")]
    public bool isStunned = false;
    public float stunDuration = 0.8f;

    private bool isDead = false;

    private float lastAttackTime = 0f;
    private CharacterController controller;
    private Animator animator;
    private float gravity = -25f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += Die;
        }
    }

    void Update()
    {
        if (player == null || isDead) return;

        // ИЗМЕНЕНИЕ: Ищем IDamageable
        IDamageable playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable != null && playerDamageable.IsDead())
        {
            controller.SimpleMove(Vector3.zero);
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

        if (isStunned)
        {
            controller.SimpleMove(Vector3.zero);
            velocity.x = 0f;
            velocity.z = 0f;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (distance > attackDistance)
        {
            animator.SetFloat("Speed", 1f);

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (stateInfo.IsName("Walk"))
            {
                controller.Move(transform.forward * moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            controller.SimpleMove(Vector3.zero);

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * 2f * Time.deltaTime);
            }

            if (Time.time >= lastAttackTime + attackCooldown && !stateInfo.IsName("ZombieAttack") && !animator.IsInTransition(0))
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("ZombieAttack");
            }
        }
    }

    private void Die()
    {
        isDead = true;
        isStunned = false;

        if (controller != null) controller.enabled = false;

        animator.SetTrigger("ZombieDeath");
        this.enabled = false;

        ZombieSound sound = GetComponent<ZombieSound>();
        if (sound != null)
        {
            sound.PlayDeathSound();
        }

        Debug.Log("Зомби умер и превратился в труп.");
    }

    public void TakeDamageReaction(int hitType)
    {
        if (isDead) return;

        animator.ResetTrigger("ZombieAttack");
        isStunned = true;

        if (hitType == 1)
            animator.SetTrigger("ZombieHit1");
        else
            animator.SetTrigger("ZombieHit2");

        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    public void DealDamageToPlayer()
    {
        if (isDead || isStunned) return;

        // ИЗМЕНЕНИЕ: Ищем IDamageable
        IDamageable playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable != null && playerDamageable.IsDead()) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackDistance + 0.5f)
        {
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(damage);
            }

            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat != null) playerCombat.PlayHitReaction();
        }
    }
}