using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMeleeController : MonoBehaviour, IDamageable
{
    private Transform target;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float runSpeedThreshold = 1.5f;
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private LayerMask obstacleLayer; //Layer for obstacles


    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isInCombat = false;
    private Vector3 startPosition;
    private float nextWanderTime;
    private bool isWandering = false;
    private bool isIdle = false;
    private Collider enemyCollider;
    public Collider rightHandCollider;
    public Collider leftHandCollider;

    [SerializeField] public static int Damage = -3;

    [Header("HealthBar")]
    [SerializeField] float maxHealth = 100;
    float currentHealth;
    [SerializeField] private EnemyHealthBar healthBar;

    private bool isDying = false;

    private void Start()
    {
        gameObject.tag = "melee";
        try
        {
            target = GameObject.Find("PlayerModel").transform;
        }
        catch
        {
            Debug.Log("player model not found!");
        }
        rightHandCollider.enabled = false;
        leftHandCollider.enabled = false;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        enemyCollider = GetComponent<Collider>();

        currentHealth = maxHealth;
        healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);

        SetNextWanderTime();
        StartCoroutine(UpdatePathRoutine());
        StartCoroutine(WanderRoutine());
    }

    private void Update()
    {
        if (isDying) return;

        if (!isInCombat && Vector3.Distance(target.position, transform.position) <= detectionRange)
        {
            isInCombat = true;
            StopCoroutine(WanderRoutine());
            isWandering = false;
        }

        if (isInCombat)
        {
            ChaseOrAttackTarget();
        }

        UpdateAnimationState();
    }

    private IEnumerator UpdatePathRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        while (!isDying && enabled)
        {
            if (isInCombat && !isAttacking)
            {
                agent?.SetDestination(target.position);
            }
            yield return wait;
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (!isInCombat)
        {
            if (Time.time >= nextWanderTime)
            {
                // Move to a random point
                Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    isWandering = true;
                    agent.SetDestination(hit.position);

                    // Wait until reaching the destination or getting close enough
                    while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                    {
                        if (isInCombat) yield break; // Exit if combat starts
                        yield return null;
                    }

                    isWandering = false;
                }

                // Idle for a random duration
                float idleDuration = Random.Range(minWanderWaitTime, maxWanderWaitTime);
                yield return new WaitForSeconds(idleDuration);

                SetNextWanderTime();
            }
            yield return null;
        }
    }

    private void ChaseOrAttackTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange && !isAttacking)
        {
          StartCoroutine(AttackTarget());
        }
    }


    private void SetNextWanderTime()
    {
        nextWanderTime = Time.time + Random.Range(minWanderWaitTime, maxWanderWaitTime);
    }

    private void UpdateAnimationState()
    {
        float speed = agent.velocity.magnitude;
        if (speed > 0.1f)
        {
            animator.SetBool("Idle", false);
            isIdle = false;
            if (!isWalking)
            {
                animator.SetBool("Walk", true);
                isWalking = true;
            }
            if (speed > runSpeedThreshold)
            {
                if (!isRunning)
                {
                    animator.SetBool("Run", true);
                    isRunning = true;
                }
            }
            else if (isRunning)
            {
                animator.SetBool("Run", false);
                isRunning = false;
            }
        }
        else if (speed <= 0.1f && !isIdle && !isAttacking) {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            animator.SetBool("Idle", true);
            isIdle = true;
            isWalking = false;
            isRunning = false;
        }
        else if (isWalking || !isWandering)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            isWalking = false;
            isRunning = false;
        }
    }

    private IEnumerator AttackTarget()
    {
        isAttacking = true;
        agent.ResetPath();
        transform.LookAt(target);
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(float damage)
    {
        isInCombat = true;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            healthBar.UpdateEnemyHealthBar(maxHealth, 0);
            Die();
        }
        else
        {
            //animator.SetTrigger("Hit");
            healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        }
    }

    public void CastSpell()
    {
        animator.SetTrigger("Cast Spell");
    }

    public void Die()
    {
        isDying = true;
        agent.enabled = false;
        StartCoroutine(DieCoroutine());
    }
    private IEnumerator DieCoroutine()
    {
        animator.SetTrigger("Death");
        Destroy(enemyCollider);

        yield return new WaitForSeconds(2.4f);

        enabled = false;
        Destroy(gameObject);
    }
    public void EnableHandsCollider()
    {
        rightHandCollider.enabled = true;
        leftHandCollider.enabled = true;
    }

    public void DisableHandsCollider()
    {
        rightHandCollider.enabled = false;
        leftHandCollider.enabled = false;
    }
}