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
    }

    public void attack1()
    {
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, radius, enemies );

        foreach (Collider2D enemyGameobje in enemy)
        {
            Debug.Log("Hit enemy");
            enemyGameobje.GetComponent<EnemyStats>().maxHealth = 0;

            
        }
    }

    private void chargeSword()
    {
        if(Input.GetMouseButton(0))
        {

            if(UpOrDown == false) {
                currentChargeTime += Time.deltaTime * 2;

                if ( currentChargeTime > maxChargeTime ) // Vaihdetaan chargen suuntaa
                {
                    UpOrDown = !UpOrDown;
        
                }
            }

            else 
            {
                currentChargeTime -= Time.deltaTime * 2;
                if (currentChargeTime < 0) // Vaihdetaan chargen suuntaa
                {
                    UpOrDown = !UpOrDown;

                }

            }


            slider.UpdateSlider(currentChargeTime, maxChargeTime);

            

            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackpoint.transform.position, radius);
    }
}
