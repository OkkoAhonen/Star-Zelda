using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Slider healthBar;  // Viittaus UI-slideriin
    public EnemyStats bossStats;  // Viittaus EnemyStats-komponenttiin
    private float currentHealth;  // K‰ytet‰‰n t‰t‰ seuraamaan pomon terveytt‰
    public CameraFollow cameraFollow;  // Viittaus CameraFollow-komponenttiin

    void Start()
    {
        // Tarkistetaan, ett‰ bossStats ja cameraFollow on liitetty oikein
        if (bossStats == null)
        {
            Debug.LogError("BossStats is not assigned to BossHealthBar!");
            return;
        }

        if (cameraFollow == null)
        {
            Debug.LogError("CameraFollow is not assigned to BossHealthBar!");
            return;
        }

        // Alustetaan currentHealth maxHealth-arvolla
        currentHealth = bossStats.maxHealth;

        // Asetetaan healthbarin maksimiarvo ja alkuarvo
        if (healthBar != null)
        {
            healthBar.maxValue = bossStats.maxHealth;  // Healthbarin maksimiarvo
            healthBar.value = currentHealth;  // Alkuarvo (terveys alussa)
        }
        else
        {
            Debug.LogError("HealthBar is not assigned!");
        }

        // Tarkista alkuarvo debugilla
        Debug.Log("Initial Health: " + currentHealth);
    }

    void Update()
    {
        // Tarkistetaan, onko kamera seuramassa pomoa
        if (cameraFollow.IsitBoss)
        {
            // N‰ytet‰‰n health bar
            healthBar.gameObject.SetActive(true);

            // P‰ivitet‰‰n healthbar pomon nykyisen terveyden perusteella
            healthBar.value = Mathf.Clamp(currentHealth, 0, bossStats.maxHealth);
        }
        else
        {
            // Piilotetaan health bar, jos kamera ei seuraa pomoa
            healthBar.gameObject.SetActive(false);
        }

        // Debuggaus: Varmista, ett‰ arvot ovat oikeat
        Debug.Log("HealthBar Value: " + healthBar.value + ", Current Health: " + currentHealth);
    }

    // T‰m‰ funktio voi olla esimerkiksi pomon ottama vahinko, jota kutsutaan muista skripteist‰
    public void TakeDamage(float damage)
    {
        // Tarkista, ettei vahinko ole negatiivinen
        if (damage < 0)
        {
            damage = 0;
        }

        // V‰hennet‰‰n vahinko pomon nykyterveydest‰
        currentHealth -= damage;

        // Varmistetaan, ett‰ terveys ei mene alle nollan
        currentHealth = Mathf.Max(currentHealth, 0);

        // Debuggaa vahinko ja uusi terveys
        Debug.Log("Took damage: " + damage + ", New Health: " + currentHealth);

        // P‰ivit‰ healthbarin arvo heti
        healthBar.value = Mathf.Clamp(currentHealth, 0, bossStats.maxHealth);
    }
}
