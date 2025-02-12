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
    public float Damagebooster = 1f;
    public float currentChargeTime = 0f;
    private bool UpOrDown = false;

 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        chargeSword();

        if (Input.GetMouseButtonUp(0))
        {
            attack1();
        }
    }

    public void attack1()
    {
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, radius, enemies );
        Damagebooster = Damagebooster + (Damagebooster * currentChargeTime);
        foreach (Collider2D enemyGameobje in enemy)
        {
            Debug.Log(Damage * Damagebooster);

            EnemyAI enemyhealth = enemyGameobje.GetComponent<EnemyAI>();
            enemyhealth.enemyTakeDamage(Damage * Damagebooster);

            
        }


        Damagebooster = 1f;
        currentChargeTime = 0f;
    }

    private void chargeSword()
    {
        if(Input.GetMouseButton(0))
        {
            
                currentChargeTime += Time.deltaTime * 2;

                if ( currentChargeTime > maxChargeTime ) // Vaihdetaan chargen suuntaa
                {  
                    currentChargeTime = maxChargeTime;
        
                }

            

            slider.UpdateSlider(currentChargeTime, maxChargeTime);

            

            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackpoint.transform.position, radius);
    }
}
