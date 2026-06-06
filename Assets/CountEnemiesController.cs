using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountEnemiesController : MonoBehaviour
{
    public static bool canSpawnEnemy;
    public int maxEnemiesAmount = 10;
    // Start is called before the first frame update
    void Start()
    {
        canSpawnEnemy = false;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] meleeEnemies = GameObject.FindGameObjectsWithTag("melee");
        GameObject[] casterEnemies = GameObject.FindGameObjectsWithTag("caster");
        GameObject[] summonerEnemies = GameObject.FindGameObjectsWithTag("summoner");
        GameObject[] skeletonEnemies = GameObject.FindGameObjectsWithTag("skeleton");
        if(meleeEnemies.Length + casterEnemies.Length + summonerEnemies.Length + skeletonEnemies.Length < maxEnemiesAmount)
        {
            canSpawnEnemy = true;
        }
        else
        {
            //Debug.Log("enemies reached 100 cant spawn more");
            canSpawnEnemy = false;
        }
    }
}
