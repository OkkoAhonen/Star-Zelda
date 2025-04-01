using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    public float playerMoveSpeed = 10f;
    public float playerMaxHealth = 100f;
    public float playerCurrentHealth = 100f;

    public int Startpoint = 0;

    public void PlusStartPoints()
    {
        Startpoint = Startpoint + 1;
        Debug.Log(Startpoint);
    }
    
}
