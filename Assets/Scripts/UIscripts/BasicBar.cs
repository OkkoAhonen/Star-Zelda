using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Tarvitaan Slideria varten

public class BasicBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [Tooltip("Nopeus, jolla palkki täyttyy tai tyhjenee (yksikköä sekunnissa, suhteessa palkin kokoon 0-1)")]
    [SerializeField] private float fillSpeed = 0.5f; // Säädä tätä nopeutta Unity Editorissa

    private float targetValue = 1f; // Mihin arvoon palkin pitäisi pyrkiä (0-1)

    private void Awake()
    {
        // Hakee Slider-komponentin automaattisesti lapsiobjekteista, jos sitä ei ole asetettu Inspectorissa
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }

        if (slider == null)
        {
            Debug.LogError("BasicBar: Slider-komponenttia ei löytynyt!", this.gameObject);
            this.enabled = false; // Poistetaan skripti käytöstä, jos slideria ei löydy
            return;
        }

        // Asetetaan alkuarvo heti (ilman animaatiota)
        InitializeSliderValue();
    }

    private void InitializeSliderValue()
    {
        // Varmistetaan, että PlayerStatsManager on jo olemassa
        if (PlayerStatsManager.instance != null)
        {
            float currentHealth = (float)PlayerStatsManager.instance.CurrentHealth;
            float maxHealth = (float)PlayerStatsManager.instance.MaxHealth;

            // Vältetään jakaminen nollalla, jos maxHealth on 0 tai pienempi
            if (maxHealth <= 0)
            {
                targetValue = 0f;
            }
            else
            {
                // Lasketaan tavoitearvo (0-1)
                targetValue = Mathf.Clamp01(currentHealth / maxHealth); // Clamp01 varmistaa, että arvo on välillä 0 ja 1
            }
            slider.value = targetValue; // Aseta alkuarvo suoraan
        }
        else
        {
            // Jos PlayerStatsManager ei ole vielä valmis Awake-vaiheessa,
            // yritetään asettaa arvo myöhemmin tai oletetaan täysi terveys aluksi.
            // Tässä oletetaan täysi terveys (arvo 1).
            targetValue = 1f;
            slider.value = 1f;
            // Voit lisätä logiikkaa, joka hakee arvon myöhemmin, jos tarpeen.
        }
    }


    private void Update()
    {
        // Tarkistetaan onko PlayerStatsManager olemassa ja valmis
        if (PlayerStatsManager.instance == null)
        {
            // Jos ei ole, ei päivitetä palkkia vielä
            // Voitaisiin yrittää alustaa arvo uudelleen, jos se ei onnistunut Awaken aikana
            // InitializeSliderValue(); // Voi olla raskas Update-kutsussa, harkitse parempaa paikkaa
            return;
        }

        // Haetaan nykyiset terveysarvot
        float currentHealth = (float)PlayerStatsManager.instance.CurrentHealth;
        float maxHealth = (float)PlayerStatsManager.instance.MaxHealth;

        // Lasketaan uusi tavoitearvo (0-1)
        float newTargetValue;
        if (maxHealth <= 0)
        {
            newTargetValue = 0f;
        }
        else
        {
            newTargetValue = Mathf.Clamp01(currentHealth / maxHealth);
        }

        // Päivitetään tavoitearvo vain, jos se on muuttunut
        // Tämä ei ole välttämätöntä MoveTowards-logiikan kannalta, mutta voi olla selkeämpää
        if (!Mathf.Approximately(targetValue, newTargetValue))
        {
            targetValue = newTargetValue;
            // Debug.Log($"Uusi tavoitearvo: {targetValue}"); // Voi auttaa debugauksessa
        }


        // Liikutetaan sliderin nykyistä arvoa kohti tavoitearvoa sulavasti
        // Mathf.MoveTowards tarvitsee nykyisen arvon, tavoitearvon ja maksimimuutoksen per askel.
        // Kerrotaan fillSpeed Time.deltaTime:lla, jotta liike on tasaista riippumatta frameratesta.
        if (!Mathf.Approximately(slider.value, targetValue)) // Tarkistetaan, onko arvo jo tavoitteessa (pieni optimointi)
        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, fillSpeed * Time.deltaTime);
        }

    }

    // Tämä metodi ei ole enää tarpeellinen Update()-logiikan kanssa,
    // mutta sen voi jättää, jos haluat asettaa arvon suoraan jostain muualta ilman animaatiota.
    /*
    public void SetSliderValueInstantly(float currentValue, float maxValue)
    {
        if (maxValue <= 0)
        {
            targetValue = 0f;
        }
        else
        {
            targetValue = Mathf.Clamp01(currentValue / maxValue);
        }
        slider.value = targetValue; // Asettaa arvon heti
    }
    */
}