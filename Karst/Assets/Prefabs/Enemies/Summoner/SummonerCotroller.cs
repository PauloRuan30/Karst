using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class SummonerController : MonoBehaviour, IDamageable
{
    private Transform target;
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private float summonCooldown = 5f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject minion;
    [SerializeField] private GameObject portal;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;

    [Header("HealthBar")]
    [SerializeField] float maxHealth = 100;
    float currentHealth;
    [SerializeField] private EnemyHealthBar healthBar;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isInCombat = false;
    private bool isSummoning = false;
    private bool isWandering = false;
    private Vector3 startPosition;
    private float nextWanderTime;
    private const float UPDATE_INTERVAL = 0.2f;
    private Collider enemyCollider;
    private bool tookDamage = false;
    // Animation parameters
    private static readonly int WalkSpeedParam = Animator.StringToHash("walkSpeed");
    private static readonly int IsSummoningParam = Animator.StringToHash("isSummoning");
    private bool isDying = false;

    private void Start()
    {
        gameObject.tag = "summoner";
        try
        {
            target = GameObject.Find("PlayerModel").transform;
        }
        catch
        {
            Debug.Log("player model not found!");
        }
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        enemyCollider = GetComponent<Collider>();
        
        currentHealth = maxHealth;
        healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);

        SetNextWanderTime();
        InvokeRepeating(nameof(SlowUpdate), 0f, UPDATE_INTERVAL);
    }

    private void SlowUpdate()
    {
        if (isDying) return;
        float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
        float sqrDetectionRange = detectionRange * detectionRange;
        float sqrAttackRange = attackRange * attackRange;

        if (sqrDistanceToTarget <= sqrDetectionRange || tookDamage)
        {
            isInCombat = true;
            if (sqrDistanceToTarget <= sqrAttackRange && CanSeeTarget())
            {
                StopMovingAndAttack();
            }
            else if(!isSummoning)
            {
                agent.SetDestination(target.position);
            }
        }
        UpdateAnimations();
    }

    private void Wander()
    {
        if (Time.time >= nextWanderTime)
        {
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            SetNextWanderTime();
        }
    }
    private void StopMovingAndAttack()
    {
        agent.ResetPath();
        if (!isSummoning)
        {
            transform.LookAt(target);
            StartCoroutine(AttackCoroutine());
        }
    }
    private IEnumerator AttackCoroutine()
    {
        isSummoning = true;
        animator.SetTrigger(IsSummoningParam);
        GameObject portal = OpenPortal();
        yield return new WaitForSeconds(summonCooldown);
        Destroy(portal);
        isSummoning = false;
        agent.SetDestination(target.position);
    }


    public void Summon() {
        Vector3 spawnPoint = transform.position + transform.forward * 1.1f;
        GameObject minionToSpawn = Instantiate(minion, spawnPoint, transform.rotation);
    }

    public GameObject OpenPortal() {
        Vector3 spawnPoint = transform.position + transform.forward * 1.1f + Vector3.up;
        return Instantiate(portal, spawnPoint, transform.rotation);
    }

    private void UpdateAnimations()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(WalkSpeedParam, speed);
    }

    private bool CanSeeTarget()
    {
        if (isInCombat) {
            Vector3 direction = target.position - transform.position;
            return !Physics.SphereCast(
                transform.position,
                0.3f,
                direction.normalized,
                out RaycastHit hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            ) || hit.transform == target;
        }
        return false;
    }

    private void SetNextWanderTime()
    {
        nextWanderTime = Time.time + Random.Range(minWanderWaitTime, maxWanderWaitTime);
    }

    public void TakeDamage(float damage)
    {
        tookDamage = true;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            healthBar.UpdateEnemyHealthBar(maxHealth, 0);
            Die();
        }
        else
        {
            animator.SetTrigger("isDamage");
            healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        }
    }
    public void Die()
    {
        isDying = true;
        agent.enabled = false;
        StartCoroutine(DieCoroutine());
    }
    private IEnumerator DieCoroutine()
    {
        animator.SetTrigger("isDead");
        Destroy(enemyCollider);

        yield return new WaitForSeconds(5.5f);

        enabled = false;
        agent.enabled = false;
        Destroy(gameObject);
    }
}