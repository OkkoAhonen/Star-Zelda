using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAction : MonoBehaviour
{
    public PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        playerStats.playerMaxHealth = 100;
        playerStats.playerCurrentHealth = playerStats.playerMaxHealth;

       
    }
    public void playerTakeDamage(float damage)
    {
        playerStats.playerCurrentHealth -= damage;
    }
}
