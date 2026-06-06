using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image enemyHealthBar;
    [SerializeField] private float reduceSpeed = 2;
    private float target = 1;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }

    public void UpdateEnemyHealthBar(float maxHealth, float currentHealth)
    {
        enemyHealthBar.fillAmount = Mathf.MoveTowards(currentHealth / maxHealth, target, reduceSpeed * Time.deltaTime);
    }
}
