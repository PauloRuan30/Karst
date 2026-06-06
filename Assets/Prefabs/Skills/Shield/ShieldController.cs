using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float growthDuration = 0.5f;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] float liveTime = 4f;
    private SphereCollider shieldCollider;
    GameObject player;

    private int[] savedDamages;
    private void Awake()
    {
        shieldCollider = gameObject.AddComponent<SphereCollider>();
        shieldCollider.isTrigger = true;
        shieldCollider.radius = 0f;
    }

    // Start is called before the first frame update


    void Start()
    {
        savedDamages = new int[3];
        savedDamages[0] = EnemyMeleeController.Damage;
        savedDamages[1] = SkeletonController.Damage;
        savedDamages[2] = EnemyCasterController.Damage;
        EnemyMeleeController.Damage = SkeletonController.Damage = EnemyCasterController.Damage = 0;
        player = GameObject.Find("PlayerModel");
        StartCoroutine(ShieldSequence());
        Destroy(gameObject, liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 2 * player.transform.localScale.y, player.transform.position.z);
        gameObject.transform.position = player.transform.position - Vector3.up;
        gameObject.transform.rotation = player.transform.rotation;
    }

    private IEnumerator ShieldSequence() {
        // Grow the shield
        float elapsedTime = 0f;
        while (elapsedTime < growthDuration)
        {
            float t = elapsedTime / growthDuration;
            shieldCollider.radius = Mathf.Lerp(0f, maxRadius, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        shieldCollider.radius = maxRadius;

        // Keep the shield active
        yield return new WaitForSeconds(liveTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Calculate direction from shield center to enemy
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;

            // Apply force to push enemy away
            Rigidbody enemyRb = other.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }
    }

    private void OnDestroy()
    {
        EnemyMeleeController.Damage = savedDamages[0];
        SkeletonController.Damage = savedDamages[1];
        EnemyCasterController.Damage = savedDamages[2];
    }
}
