using UnityEngine;

public class SummonController : MonoBehaviour
{
    [SerializeField] float lifeTime = 4f;
    [SerializeField] float spawnInterval = 1f;
    [SerializeField] GameObject minion;
    [SerializeField] float spawnRadius = 2f; // Radius of the spawn area

    // Start is called before the first frame update
    void Start()
    {
        // Destroy the game object after a specified lifetime
        Destroy(gameObject, lifeTime);

        // Start spawning minions every second
        InvokeRepeating("SpawnMinion", 1f, spawnInterval);
    }

    // Method to spawn a minion
    void SpawnMinion()
    {
        // Generate a random position within a circle
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        Instantiate(minion, spawnPosition, transform.rotation);

    }

    // This method is called when the game object is destroyed
    void OnDestroy()
    {
        // Cancel the InvokeRepeating when the object is destroyed
        CancelInvoke("SpawnMinion");
    }
}
