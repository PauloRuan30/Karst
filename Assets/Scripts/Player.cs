using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxMana = 100;
    private float currentHealth;
    private float currentMana;
    //private float addedHealth;
    [SerializeField] private int ManaRegenerationAmount = 5;
    [SerializeField] private int HealthRegenerationAmount = 0;
    [SerializeField] private Bar healthBar;
    [SerializeField] private Bar manaBar;

    [Header("DeathScreenObjects")]
    [SerializeField] GameObject DeathScreenCanvas;
    [SerializeField] GameObject rayInteractor;
    [SerializeField] Button restartBtn;
    [SerializeField] Button mainMenuBtn;

    public static Player instance;
    private bool playerIsDead;
    void Start()
    {
        playerIsDead = false;
        if (instance == null) { instance = this; };
        currentHealth = maxHealth;
        currentMana = maxMana;
        healthBar.UpdateHealthBar(maxHealth, currentHealth, HealthRegenerationAmount);
        manaBar.UpdateManaBar(maxMana, currentMana, ManaRegenerationAmount);

        StartCoroutine(RegenerateRoutine());

        restartBtn.onClick.AddListener(RestartSceneBtnHandler);
        mainMenuBtn.onClick.AddListener(ExitToMenuButtonHandler);
    }


    private IEnumerator RegenerateRoutine() {
        while (true)
        {
            yield return new WaitForSeconds(1);
            PlayerUpdateMana(ManaRegenerationAmount);
            PlayerUpdateHealth(HealthRegenerationAmount);
        }
    }

    public void PlayerUpdateHealth(float amount)
    {
        if (playerIsDead) return;
        currentHealth += amount;
        if (currentHealth <= 0)
        {
            healthBar.UpdateHealthBar(maxHealth, 0, 0);
            PlayerDie();
        }
        else
        {
            currentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;
            currentHealth = currentHealth < 0 ? 0 : currentHealth;
            healthBar.UpdateHealthBar(maxHealth, currentHealth, HealthRegenerationAmount);
        }
    }
    public void PlayerUpdateMana(float manaAmount)
    {
        currentMana += manaAmount;
        currentMana = currentMana > maxMana ? maxMana : currentMana;
        currentMana = currentMana < 0 ? 0 : currentMana;
        manaBar.UpdateManaBar(maxMana, currentMana, ManaRegenerationAmount);
    }

    public int GetCurrentMana() {
        return (int)currentMana;
    }

    public void PlayerUpdateRegenerationSpeed(float amount) {
        HealthRegenerationAmount = (int)amount;
    }
    
    public void PlayerDie()
    {
        playerIsDead = true;
        Time.timeScale = 0;
        DeathScreenCanvas.SetActive(true);
        Draw drawscript = GameObject.Find("Player").GetComponent<Draw>();
        drawscript.enabled = false;
        rayInteractor.SetActive(true);
    }

    public void ExitToMenuButtonHandler()
    {
        Time.timeScale = 1;
        SceneTransitionManager.singleton.GoToScene(0);
    }

    public void RestartSceneBtnHandler() {
        Time.timeScale = 1;
        SceneTransitionManager.singleton.GoToScene(1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("got hit by " + collision.gameObject.name);

        switch (collision.gameObject.name)
        {
            case "ColiderSword":
                PlayerUpdateHealth(SkeletonController.Damage);
                break;
            case "EnemyFireBall(Clone)":
                PlayerUpdateHealth(EnemyCasterController.Damage);
                break;
            case "LeftHandSphere":
                PlayerUpdateHealth(EnemyMeleeController.Damage);
                break;
            case "RightHandSphere":
                PlayerUpdateHealth(EnemyMeleeController.Damage);
                break;
            default:
                break;
        }
    }
}
