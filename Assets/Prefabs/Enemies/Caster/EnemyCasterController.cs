using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyCasterController : MonoBehaviour, IDamageable
{
    [Header("References")]
    private GameObject target;
    [SerializeField] private GameObject skillToCast;
    [SerializeField] private Transform enemyCasterCastPoint;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;

    [Header("Raycasting")]
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("HealthBar")]
    [SerializeField] float maxHealth = 100;
    float currentHealth;
    [SerializeField] private EnemyHealthBar healthBar;

    [Header("Path Update")]
    [SerializeField] private float pathUpdateDelay = 0.2f;

    [Header("Debugging")]
    [SerializeField] private bool showVisualization = false;

    private Collider enemyCollider;
    private Vector3 startPosition;
    private bool isInCombat = false;
    private float nextWanderTime;
    private bool isAttacking = false;
    private bool tookDamage = false;
    private float nextUpdateTime;
    [SerializeField] public static int Damage = -5;

    // Animation parameters
    private static readonly int IsWalkingParam = Animator.StringToHash("isWalking");
    private static readonly int IsAttackingParam = Animator.StringToHash("isAttacking");
    private static readonly int IsInCombatParam = Animator.StringToHash("isInCombat");

    private LineRenderer lineRenderer;
    private GameObject hitSphere;

    private bool isDying = false;

    private Coroutine updatePathCoroutine;

    private void Start()
    {
        gameObject.tag = "caster";
        try
        {
            target = GameObject.Find("PlayerModel");
        }
        catch
        {
            Debug.Log("player model not found!");
        }
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        enemyCollider = GetComponent<Collider>();
        currentHealth = maxHealth;
        healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        try
        {
            updatePathCoroutine = StartCoroutine(UpdatePathCoroutine());
        }
        catch
        {
           
        }
        // Set up LineRenderer to draw the ray
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.enabled = false; // Initially disable the LineRenderer

        // Create a sphere to visualize the hit point
        hitSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitSphere.transform.localScale = Vector3.one * sphereCastRadius * 2;
        hitSphere.GetComponent<Collider>().enabled = false; // Disable the collider
        hitSphere.GetComponent<Renderer>().material.color = Color.red;
        hitSphere.SetActive(false); // Hide initially
    }

    private IEnumerator UpdatePathCoroutine()
    {
        while (true)
        {
            UpdateBehavior();
            yield return new WaitForSeconds(pathUpdateDelay);
        }
    }

    private void UpdateBehavior()
    {
        if (isDying || target == null) return;

        float distanceToTarget = (target.transform.position - transform.position).magnitude;
        if (!isInCombat)
        {
            if (tookDamage)
            {
                isInCombat = true;
            }
            else
            {
                
                if (distanceToTarget <= attackRange)
                    isInCombat = true;
            }
        }
        if (isInCombat) {
            if (distanceToTarget > attackRange)
            {
                ChaseTarget();
            }
            else {
                bool canCastSpell = CanSeeTarget();
                if (canCastSpell)
                {
                    StopMovingAndAttack();
                }
                else {
                    ChaseTarget();
                }
            }
        }
        UpdateAnimations();
    }

    private void Update()
    {
        // Update visualization if enabled
        if (showVisualization)
        {
            UpdateVisualization();
        }
    }

    private void UpdateVisualization()
    {
        if (showVisualization && isInCombat && target != null && enemyCasterCastPoint != null)
        {
            Vector3 direction = target.transform.position - enemyCasterCastPoint.position + 0.2f * Vector3.up;
            RaycastHit hit;

            bool hitDetected = Physics.SphereCast(
                enemyCasterCastPoint.position,
                sphereCastRadius,
                direction.normalized,
                out hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );

            Vector3 endPosition = hitDetected ? hit.point : enemyCasterCastPoint.position + direction.normalized * attackRange;
            lineRenderer.SetPosition(0, enemyCasterCastPoint.position);
            lineRenderer.SetPosition(1, endPosition);

            if (hitDetected)
            {
                hitSphere.transform.position = hit.point;
                hitSphere.SetActive(true);
            }
            else
            {
                hitSphere.SetActive(false);
            }

            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
            hitSphere.SetActive(false);
        }
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

    private void SetNextWanderTime()
    {
        nextWanderTime = Time.time + Random.Range(minWanderWaitTime, maxWanderWaitTime);
    }

    private void StopMovingAndAttack()
    {
        agent.ResetPath(); // Stops the agent from moving
        animator.SetBool(IsInCombatParam, true);
        if (!isAttacking)
        {
            transform.LookAt(target.transform.position);
            StartCoroutine(AttackCoroutine());
        }
    }

    private void ChaseTarget()
    {
        if (isDying) return;
        agent.SetDestination(target.transform.position);
    }

    private bool CanSeeTarget()
    {
        if (isInCombat && target != null && enemyCasterCastPoint != null)
        {
            Vector3 direction = target.transform.position - enemyCasterCastPoint.position + 0.2f * Vector3.up;
            RaycastHit hit;

            // Perform the sphere cast
            bool hitDetected = Physics.SphereCast(
                enemyCasterCastPoint.position,
                sphereCastRadius,
                direction.normalized,
                out hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );

            if (showVisualization)
            {
                // Update the LineRenderer to show the ray
                Vector3 endPosition = hitDetected ? hit.point : enemyCasterCastPoint.position + direction.normalized * attackRange;
                lineRenderer.SetPosition(0, enemyCasterCastPoint.position);
                lineRenderer.SetPosition(1, endPosition);

                // Update the hit sphere position
                if (hitDetected)
                {
                    hitSphere.transform.position = hit.point;
                    hitSphere.SetActive(true);
                }
                else
                {
                    hitSphere.SetActive(false);
                }
            }

            // Return whether the target is seen
            return !hitDetected || hit.transform == target;
        }
        return false;
    }


private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        animator.SetBool(IsAttackingParam, true);
        yield return new WaitForSeconds(attackCooldown);
        animator.SetBool(IsAttackingParam, false);
        isAttacking = false;
    }

    public void CastSpell()
    {
        if (skillToCast != null && enemyCasterCastPoint != null)
        {
            GameObject spellInstance = Instantiate(skillToCast, enemyCasterCastPoint.position, enemyCasterCastPoint.rotation);
            if (spellInstance.TryGetComponent(out Rigidbody spellRb))
            {
                Vector3 direction = (target.transform.position - 0.1f * Vector3.up - enemyCasterCastPoint.position).normalized;
                spellRb.AddForce(direction * 500f);
            }
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool(IsWalkingParam, agent.velocity.sqrMagnitude > 0.01f);
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
        StartCoroutine(DieCoroutine());
    }
    private IEnumerator DieCoroutine()
    {
        agent.enabled = false;
        animator.SetTrigger("isDead");
        Destroy(enemyCollider);

        yield return new WaitForSeconds(4.8f);

        enabled = false;
        Destroy(gameObject);
    }
}