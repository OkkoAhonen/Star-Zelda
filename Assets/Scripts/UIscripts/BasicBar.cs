using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [Tooltip("Nopeus, jolla palkki täyttyy tai tyhjenee (yksikköä sekunnissa, suhteessa palkin kokoon 0-1)")]
    [SerializeField] private float fillSpeed = 0.5f;

    private float targetValue = 1f;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }

        if (slider == null)
        {
            Debug.LogError("BasicBar: Slider-komponenttia ei löytynyt!", this.gameObject);
            this.enabled = false;
            return;
        }

        InitializeSliderValue();
    }

    private void InitializeSliderValue()
    {
        if (PlayerStatsManager.instance != null)
        {
            float currentHealth = (float)PlayerStatsManager.instance.CurrentHealth;
            float maxHealth = (float)PlayerStatsManager.instance.MaxHealth;

            if (maxHealth <= 0)
            {
                targetValue = 0f;
            }
            else
            {
                targetValue = Mathf.Clamp01(currentHealth / maxHealth);
            }
            slider.value = targetValue;
        }
        else
        {
            targetValue = 1f;
            slider.value = 1f;
        }
    }


    private void Update()
    {
        if (PlayerStatsManager.instance == null)
        {
            return;
        }

        float currentHealth = (float)PlayerStatsManager.instance.CurrentHealth;
        float maxHealth = (float)PlayerStatsManager.instance.MaxHealth;

        float newTargetValue;
        if (maxHealth <= 0)
        {
            newTargetValue = 0f;
        }
        else
        {
            newTargetValue = Mathf.Clamp01(currentHealth / maxHealth);
        }

        if (!Mathf.Approximately(targetValue, newTargetValue))
        {
            targetValue = newTargetValue;
        }

        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, fillSpeed * Time.deltaTime);
        }

    }
}