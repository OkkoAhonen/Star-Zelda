using System.Collections;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{

    [SerializeField] GameObject MoneyText;

    public static float PlayerMoney = 100f;    // Nykyinen rahamäärä (julkinen ja staattinen)
    public float targetMoney = 100f;          // Tavoite, johon rahamäärä animoidaan
    [SerializeField] private float lerpSpeed = 3f; // Nopeus, jolla raha liikkuu (säädettävissä Inspectorissa)
    private bool isAnimating = false;          // Seurataanko animaatiota

    void Start()
    {
        MoneyText = GameObject.Find("/Canvas/Money");
        targetMoney = PlayerMoney;              // Asetetaan alkuarvo
        UpdateMoneyUI();                       // Päivitetään UI heti alussa
    }

    void Update()
    {
        // Jos animaatio on käynnissä, lerpataan currentMoney kohti targetMoney-arvoa
        if (isAnimating && PlayerMoney != targetMoney)
        {
            PlayerMoney = Mathf.Lerp(PlayerMoney, targetMoney, Time.deltaTime * lerpSpeed);

            // Pysäytetään animaatio, kun ollaan riittävän lähellä tavoitetta
            if (Mathf.Abs(PlayerMoney - targetMoney) < 0.01f)
            {
                PlayerMoney = targetMoney; // Varmistetaan tarkka arvo
                isAnimating = false;
            }

            UpdateMoneyUI(); // Päivitetään UI joka framella animaation aikana
        }
    }

    // Metodi rahan lisäämiseen tai vähentämiseen
    public void ChangeMoney(float amount)
    {
        targetMoney = PlayerMoney + amount; // Lasketaan uusi tavoite (voi olla + tai -)
        isAnimating = true;                 // Käynnistetään animaatio
    }

    // Päivittää UI-tekstin
    private void UpdateMoneyUI()
    {
        // m_TextMeshPro.text = "$" + PlayerMoney.ToString("F2"); // Näyttää 2 desimaalia, esim. $99.85
        Debug.Log("UpdateMoneyUI");
    }

}