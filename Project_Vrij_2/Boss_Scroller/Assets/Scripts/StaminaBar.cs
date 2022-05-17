using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    private Slider staminaBar;

    private void Awake()
    {
        staminaBar = gameObject.GetComponentInChildren<Slider>();
    }

    public void SetUpStaminaBar(float maxStamina)
    {
        staminaBar.maxValue = maxStamina;
        staminaBar.value = maxStamina;
    }

    public void DecreaseStamina(float stamina)
    {
        staminaBar.value -= stamina;
    }
    public void IncreaseStamina(float stamina)
    {
        staminaBar.value += stamina;
    }

    public void SetStamina(float stamina)
    {
        staminaBar.value = stamina;
    }
}
