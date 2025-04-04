using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MoneyManager : MonoBehaviour
{
     [SerializeField] TextMeshProUGUI m_TextMeshPro;

    // Start is called before the first frame update
    public static float PlayerMoney = 100f;
    [SerializeField] GameObject MoneyText;
    


    void Start()
    {
        MoneyText = GameObject.Find("/Canvas/Money");
        m_TextMeshPro = MoneyText.GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        m_TextMeshPro.text = "$" + PlayerMoney.ToString("F2");
    }

}
