using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SummonMinionController : MonoBehaviour
{
    public static float damage = 5f;
    public float searchRadius = 10f;
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public LayerMask enemyLayerMask;
    public GameObject spellPrefab;
    public float spellSpeed = 10f;
    public float followPlayerDistance = 10f;
    public LayerMask obstacleLayer;
    public float lifeTime = 15f;
    public float minDistanceFromPlayer = 3f; //  minimum distance from player

    private float summonedTime;
    private Transform playerTransform;
    private GameObject playerModel;
    private NavMeshAgent agent;
    private Transform currentTarget;
    private float lastAttackTime;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDying = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        playerModel = GameObject.Find("PlayerModel");
        summonedTime = Time.time;
    }

    void Update()
    {
        if (isDying)
            return;
        if (Time.time - summonedTime >= lifeTime)
        {
            isDying = true;
            animator.SetTrigger("isDead");
        }
        else
        {
            UpdateAnimator();

            if (isAttacking)
            {
                agent.isStopped = true;
                return;
            }
            playerTransform = playerModel.transform;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer > followPlayerDistance)
            {
                FollowPlayer();
            }
            else
            {
                SearchAndEngageEnemy();
            }
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
    }

    void SearchAndEngageEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius, enemyLayerMask);

        if (hitColliders.Length > 0)
        {
            // Initialize variables to track the closest enemy
            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider collider in hitColliders)
            {
                // Calculate the distance from the player to the current enemy
                float distanceToEnemy = Vector3.Distance(playerTransform.position, collider.transform.position);

                // Update the closest enemy if this one is closer
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = collider.transform;
                }
            }

            // Set the closest enemy as the current target
            currentTarget = closestEnemy;

            if (currentTarget != null)
            {
                float distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.position);
                transform.LookAt(currentTarget.position);

                if (CanSeeTarget())
                {
                    if (distanceToCurrentTarget <= attackRange)
                    {
                        AttackEnemy();
                    }
                    else
                    {
                        MoveTowardsEnemy();
                    }
                }
                else
                {
                    MoveTowardsEnemy();  // Move towards the target even if it cannot be seen
                }
            }
        }
        else
        {
            currentTarget = null;
            FollowPlayer();
        }
    }

    bool CanSeeTarget()
    {
    Vector3 direction = currentTarget.position - transform.position;
            return !Physics.SphereCast(
                transform.position,
                0.3f,
                direction.normalized,
                out RaycastHit hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            ) || hit.transform == currentTarget;

    }


    void MoveTowardsEnemy()
    {
        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
    }

    void AttackEnemy()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            TriggerAttack();
            lastAttackTime = Time.time;
        }
    }

    void TriggerAttack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");
    }

    public void CastSpell()
    {
        if (isDying) return;
        if(currentTarget?.GetComponent<Collider>() != null)
        {
            Collider targetCollider = currentTarget.GetComponent<Collider>();
            if (targetCollider != null)
            {
                // Get the center of the target's collider
                Vector3 spellTargetPosition = targetCollider.bounds.center;

                // Calculate the position to spawn the spell
                Vector3 spellSpawnPoint = transform.position + transform.forward * 1.1f + Vector3.up;
                GameObject spell = Instantiate(spellPrefab, spellSpawnPoint, Quaternion.identity);

                // Determine the direction to cast the spell
                Vector3 direction = (spellTargetPosition - spellSpawnPoint).normalized;
                spell.transform.forward = direction;
            }
        }
    }

    public void FinishAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    void FollowPlayer()
    {
        if (isDying) return;
        currentTarget = null;
        agent.isStopped = false;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > minDistanceFromPlayer)
        {
            // Calculate a position that's minDistanceFromPlayer away from the player
            Vector3 targetPosition = playerTransform.position - directionToPlayer.normalized * minDistanceFromPlayer;
            agent.SetDestination(targetPosition);
        }
        else
        {
            // If already within the minimum distance, stop moving
            agent.SetDestination(transform.position);
        }
    }

    public void DestroyMinion()
    {
        Destroy(gameObject);
    }
}