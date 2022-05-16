using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider healthBar;

    private void Start()
    {
        healthBar = gameObject.GetComponentInChildren<Slider>();
    }

    public void SetUpHealthBar(float maxHealth)
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
    }

    public void DecreaseHealth(float health)
    {
        healthBar.value -= health;
    }
    public void IncreaseHealth(float health)
    {
        healthBar.value += health;
    }

    public void SetHealth(float health)
    {
        healthBar.value = health;
    }
}
