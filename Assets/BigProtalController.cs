using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigProtalController : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyHealthBar portHealthBar;
    [SerializeField] private GameObject enemyPorts;
    [SerializeField] float maxHealth = 100;
    float currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            portHealthBar.UpdateEnemyHealthBar(maxHealth, 0);
            Destroy(gameObject);
            enemyPorts.SetActive(false);
            DestroyAllEnemies();
            WinningMenuCanvas.isWin = true;
        }
        else
        {
            portHealthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        }
    }
    private void DestroyAllEnemies()
    {
        GameObject[] meleeEnemies = GameObject.FindGameObjectsWithTag("melee");
        GameObject[] casterEnemies = GameObject.FindGameObjectsWithTag("caster");
        GameObject[] summonerEnemies = GameObject.FindGameObjectsWithTag("summoner");
        GameObject[] skeletonEnemies = GameObject.FindGameObjectsWithTag("skeleton");

        List<GameObject> allEnemies = new List<GameObject>();
        allEnemies.AddRange(meleeEnemies);
        allEnemies.AddRange(casterEnemies);
        allEnemies.AddRange(summonerEnemies);
        allEnemies.AddRange(skeletonEnemies);

        foreach (GameObject enemy in allEnemies)
        {
            Destroy(enemy);
        }
    }
}
