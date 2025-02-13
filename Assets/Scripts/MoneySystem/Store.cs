using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Progress;

public class Store : MonoBehaviour
{
    [SerializeField] float itemPrice = 3f;
    public PlayerMoney playerMoney;
    public float coins;
    

    // Start is called before the first frame update
    void Start()
    {
        playerMoney = GetComponent<PlayerMoney>();

        coins = playerMoney.playerCoins;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuyItem()
    {
        if (coins >= itemPrice)
        {
            coins = playerMoney.MoneySubract(coins, itemPrice);

            Debug.Log("Item bought successfully");

            // Koodi jossa lis‰t‰‰ item invii
        }

        else
        {
            Debug.LogError("Not enough money left");
        }
    }
}
