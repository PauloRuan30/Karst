using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostBeamController : MonoBehaviour
{
    [SerializeField] float liveTime = 2f;
    public float damage = 3f;
    [SerializeField] float rayInterval = 0.2f;
    [SerializeField] float rayDistance = 100f;
    [SerializeField] LayerMask targetLayers;

    GameObject tip;

    void Start()
    {
        gameObject.name = "frostbeam";
        tip = GameObject.Find("tip");
        Destroy(gameObject, liveTime);
        StartCoroutine(ShootRays());
    }

    void Update()
    {
        if (tip != null)
        {
            gameObject.transform.position = tip.transform.position - tip.transform.forward * 0.5f;
            gameObject.transform.forward = tip.transform.forward;
        }
    }

    IEnumerator ShootRays()
    {
        while (true)
        {
            ShootRay();
            yield return new WaitForSeconds(rayInterval);
        }
    }

    void ShootRay()
    {
        RaycastHit hit;
        if (Physics.Raycast(tip.transform.position - tip.transform.forward*0.5f, tip.transform.forward, out hit, rayDistance, targetLayers))
        {
            ApplyDamage(hit.collider);
        }
    }

    private void ApplyDamage(Collider collider)
    {
        // Skip self or non-damageable objects
        if (collider.gameObject == gameObject) return;
        IDamageable damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage); // Apply damage
        }
    }
}
