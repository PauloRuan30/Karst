using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 15f;
    public float damage = 50;
    public float explosionRadius = 3f; // Radius of the explosion effect
    GameObject tip;
    [SerializeField] float lifeTime = 2f;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject hit;
    [SerializeField] float hitOffset = 0f;
    [SerializeField] bool UseFirePointRotation;
    [SerializeField] GameObject[] detachedObjects;
    [SerializeField] Vector3 rotationOffset = new Vector3(0, 0, 0);
    private Rigidbody rb;
    private bool hasExploded = false;
    void Start()
    {
        gameObject.name = "fireball";
        tip = GameObject.Find("tip");
        rb = GetComponent<Rigidbody>();
        if (flash != null)
        {
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = tip.transform.forward * speed;
            //transform.position += transform.forward * (speed * Time.deltaTime);         
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return; // Prevent multiple explosions

        hasExploded = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        speed = 0;

        // Handle explosion effects
        Explode();
    }

    private void Explode()
    {
        // Get all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var collider in colliders)
        {
            // Skip self or non-damageable objects
            if (collider.gameObject == gameObject) continue;

            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage); // Apply damage
            }
        }

        // Handle hit effects
        HandleHitEffect();
        HandleDetachedObjects();

        // Destroy the projectile after explosion effects
        Destroy(gameObject);
    }

    private void HandleHitEffect()
    {
        if (hit != null)
        {
            Quaternion rot = Quaternion.identity;
            Vector3 pos = transform.position;

            var hitInstance = Instantiate(hit, pos, rot);
            if (UseFirePointRotation)
            {
                hitInstance.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
            }
            else if (rotationOffset != Vector3.zero)
            {
                hitInstance.transform.rotation = Quaternion.Euler(rotationOffset);
            }
            else
            {
                hitInstance.transform.LookAt(pos + transform.forward);
            }

            var hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration - 0.2f);
            }
        }
    }

    private void HandleDetachedObjects()
    {
        foreach (var detachedPrefab in detachedObjects)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }
    }
}
