using System.Collections;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshPro;
    [SerializeField] GameObject MoneyText;

    public static float PlayerMoney = 100f;    // Nykyinen raham‰‰r‰ (julkinen ja staattinen)
    public float targetMoney = 100f;          // Tavoite, johon raham‰‰r‰ animoidaan
    [SerializeField] private float lerpSpeed = 3f; // Nopeus, jolla raha liikkuu (s‰‰dett‰viss‰ Inspectorissa)
    private bool isAnimating = false;          // Seurataanko animaatiota

    void Start()
    {
        MoneyText = GameObject.Find("/Canvas/Money");
        m_TextMeshPro = MoneyText.GetComponent<TextMeshProUGUI>();
        targetMoney = PlayerMoney;              // Asetetaan alkuarvo
        UpdateMoneyUI();                       // P‰ivitet‰‰n UI heti alussa
    }

    void Update()
    {
        // Jos animaatio on k‰ynniss‰, lerpataan currentMoney kohti targetMoney-arvoa
        if (isAnimating && PlayerMoney != targetMoney)
        {
            PlayerMoney = Mathf.Lerp(PlayerMoney, targetMoney, Time.deltaTime * lerpSpeed);

            // Pys‰ytet‰‰n animaatio, kun ollaan riitt‰v‰n l‰hell‰ tavoitetta
            if (Mathf.Abs(PlayerMoney - targetMoney) < 0.01f)
            {
                PlayerMoney = targetMoney; // Varmistetaan tarkka arvo
                isAnimating = false;
            }

            UpdateMoneyUI(); // P‰ivitet‰‰n UI joka framella animaation aikana
        }
    }

    // Metodi rahan lis‰‰miseen tai v‰hent‰miseen
    public void ChangeMoney(float amount)
    {
        targetMoney = PlayerMoney + amount; // Lasketaan uusi tavoite (voi olla + tai -)
        isAnimating = true;                 // K‰ynnistet‰‰n animaatio
    }

    // P‰ivitt‰‰ UI-tekstin
    private void UpdateMoneyUI()
    {
        // m_TextMeshPro.text = "$" + PlayerMoney.ToString("F2"); // N‰ytt‰‰ 2 desimaalia, esim. $99.85
        Debug.Log("UpdateMoneyUI");
    }

}