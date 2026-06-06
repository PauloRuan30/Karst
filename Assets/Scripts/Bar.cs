using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image healBarSprite;
    [SerializeField] private float reduceSpeed;
    [SerializeField] private TMPro.TMP_Text healthValue;
    [SerializeField] private TMPro.TMP_Text addedHealthValue;
    [SerializeField] private TMPro.TMP_Text manaValue;
    [SerializeField] private TMPro.TMP_Text addedManaValue;
    private float target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        healBarSprite.fillAmount = Mathf.MoveTowards(healBarSprite.fillAmount, target, reduceSpeed * Time.deltaTime);
    }
    public void UpdateHealthBar(float maxHealth, float currentHealth, float addedHealth)
    {
        target = currentHealth / maxHealth;
        healthValue.text = $"{currentHealth:F0}/{maxHealth:F0}";
        addedHealthValue.text = addedHealthValue != null ? (addedHealth != 0 ? $"+{addedHealth:F0}" : "") : "";
    }
    public void UpdateManaBar(float maxMana, float currentMana, float addedMana)
    {
        target = currentMana / maxMana;
        manaValue.text = $"{currentMana:F0}/{maxMana:F0}";
        addedManaValue.text = addedManaValue != null ? (addedMana != 0 ? $"+{addedMana:F0}" : "") : "";
    }
}
