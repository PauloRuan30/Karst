using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonController : MonoBehaviour, IDamageable
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float pathUpdateTime = 0.1f;
    [SerializeField] private LayerMask obstacleLayer; //Layer for obstacles

    private Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private Collider enemyCollider;
    public Collider swordColider;
    [Header("HealthBar")]
    [SerializeField] float maxHealth = 20;
    float currentHealth;
    [SerializeField] private EnemyHealthBar healthBar;
    [SerializeField] public static int Damage = -1;


    private bool isDying = false;
    void Start()
    {
        gameObject.tag = "skeleton";
        swordColider.enabled = false;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();
        currentHealth = maxHealth;
        healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
    }

    void Update()
    {
        target = GameObject.Find("PlayerModel").transform;
        if (isDying) return;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {

                StopMoving();
                StartCoroutine(Attack());
        }
        else if (!isAttacking)
        {
            MoveTowardsTarget();
        }
    }



    private void StopMoving()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("isRunning", false);
    }

    private void MoveTowardsTarget()
    {
        agent.SetDestination(target.position);
        animator.SetBool("isRunning", true);
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetBool("isRunning", false);
        animator.SetTrigger("isAttacking");
        lastAttackTime = Time.time;
        gameObject.transform.LookAt(target.position);

        // Wait for the attack animation to finish
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            healthBar.UpdateEnemyHealthBar(maxHealth, 0);
            isDying = true;
            Die();
        }
        else
        {
            healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        }
    }
    public void Die()
    {
        agent.enabled = false;
        isDying = true;
        StartCoroutine(DieCoroutine());
    }
    private IEnumerator DieCoroutine()
    {
        animator.SetTrigger("isDead");
        Destroy(enemyCollider);

        yield return new WaitForSeconds(2.2f);

        enabled = false;
        agent.enabled = false;
        Destroy(gameObject);
    }

    public void EnableSwordCollider()
    {
        swordColider.enabled = true;
    }
    public void DisableSwordCollider()
    {
        swordColider.enabled = false;
    }
}