using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireBallController : MonoBehaviour
{
    [SerializeField] float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private int EnemyCasterSkillDamage = 5;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collisionObject = collision.gameObject;
        if (collision != null)
        {
            if (collisionObject.CompareTag("PlayerTag"))
            {
                Player playerComponent = collisionObject?.GetComponent<Player>();
                playerComponent?.PlayerUpdateHealth(-EnemyCasterSkillDamage);
                
            }
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log(this + "destroyed");

    }

}
