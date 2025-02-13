using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public float playerCoins = 10.0f;

    public float MoneySubract(float coins, float subtraction)
    {
        coins -= subtraction;
        return coins;
    }

    public float MoneyAddition(float coins,float addition)
    {
        coins += addition;
        return coins;
    }
}
