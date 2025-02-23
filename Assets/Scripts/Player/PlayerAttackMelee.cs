using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerAttackMelee : MonoBehaviour
{


    public GameObject attackpoint;
    public float radius;
    public LayerMask enemies;

    public BasicBar slider;


    //Charge muuttujat

    public float maxChargeTime = 3f; //Tämä korvataan myöhemmin itemien arvoilla
    public float Damage = 5f;
    public float Damagebooster;
    public float currentChargeTime = 0f;
    private bool UpOrDown = false;

 

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
            if (equippedItem.isWeapon == true) { 
            chargeSword();

            if (Input.GetMouseButtonUp(0) )
            {
                attack1();
            }
        }
    }

    public void attack1()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        Damagebooster = equippedItem.damageBooster;
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, equippedItem.attackRadius, enemies );
        Damagebooster = Damagebooster + (Damagebooster * currentChargeTime);
        foreach (Collider2D enemyGameobje in enemy)
        {
            Debug.Log(equippedItem.attackDamage * Damagebooster);

            EnemyAI enemyhealth = enemyGameobje.GetComponent<EnemyAI>();
            enemyhealth.enemyTakeDamage(equippedItem.attackDamage * Damagebooster);

            
        }


        Damagebooster = 1f;
        currentChargeTime = 0f;
    }

    private void chargeSword()
    {
        if(Input.GetMouseButton(0))
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
            currentChargeTime += Time.deltaTime * 2;

                if ( currentChargeTime > equippedItem.maxChargeTime ) // Vaihdetaan chargen suuntaa
                {  
                    currentChargeTime = equippedItem.maxChargeTime;
        
                }

            

            slider.UpdateSlider(currentChargeTime, equippedItem.maxChargeTime);

            

            
        }
    }

    /*private void OnDrawGizmos()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        if ( equippedItem != null ) { 
        Gizmos.DrawWireSphere(attackpoint.transform.position, equippedItem.attackRadius);
        }
    }*/
}
