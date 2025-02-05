using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Viholliset 
{

    public int maxhealt;
    public int minhealt;
    public int attackDamage;
    public string name;


    public void attack()
    {
        Debug.Log($"You have been attacked {attackDamage} amouth \t you have this much left {maxhealt}");
    }

   

}
