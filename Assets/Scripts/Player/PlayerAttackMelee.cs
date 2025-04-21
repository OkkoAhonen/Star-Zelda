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

    public Animator animator; //Aseta inspektorissa

    public PlayerStatsManager playerStatsManager;

    //Charge muuttujat

    public float maxChargeTime = 3f; //Tämä korvataan myöhemmin itemien arvoilla
    public float Damage = 5f;
    public float Damagebooster;
    public float currentChargeTime = 0f;
    private bool UpOrDown = false;

    //[SerializeField] private GameObject chargebar;
    private bool charging = false;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(PlayerStatsManager.instance.GetStat(StatType.Strength));
        Debug.Log("Kuka oot?");

        if (animator != null)
        
        { 
            animator.speed = 2f;
            Debug.Log("animation speed: " + animator.speed);
        }
    }

    // Update is called once per frame
    void Update()
    {


        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
            if (equippedItem.isWeapon == true)
            {
               

                if (Input.GetMouseButtonUp(0))
                {
                    attack1();

                }

            }


            if(equippedItem.type  == ItemType.Weapon)
            {
                if (equippedItem.name.Contains("_1"))
                {
                    animator.gameObject.transform.localScale = new Vector3(40, 40, 1);
                }
                else
                {
                    animator.gameObject.transform.localScale = new Vector3(60, 60, 1);
                }
            }
        }
    }

    public void attack1()
    {
        animator.SetTrigger("Attack1");
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, equippedItem.attackRadius, enemies);
        foreach (Collider2D enemyGameobje in enemy)
        {

            EnemyController enemyhealth = enemyGameobje.GetComponent<EnemyController>();
            Debug.Log(enemyhealth.gameObject.name + "Moi");
            Vector2 direction = (enemyGameobje.transform.position - transform.position).normalized;
            enemyhealth.TakeDamage( equippedItem.attackDamage  /*  *(float) PlayerStatsManager.instance.GetStat(StatType.Strength)*/, direction, 2f);


        }


        currentChargeTime = 0f;

    }

    /*private void chargeSword()
    {
        if (Input.GetMouseButton(0))
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
            if (equippedItem == null || slider == null)
            {
                Debug.LogError("Cannot charge sword: equippedItem or slider is null!");
                return;
            }

            currentChargeTime += Time.deltaTime * 2;

            if (currentChargeTime > equippedItem.maxChargeTime)
            {
                currentChargeTime = equippedItem.maxChargeTime;
            }

            slider.UpdateSlider(currentChargeTime, equippedItem.maxChargeTime);
        }
    }*/

    private void OnDrawGizmos()
    {
        if (attackpoint != null)
        {
            // Haetaan varustettu esine, jos se on olemassa
            Item equippedItem = InventoryManager.Instance != null ? InventoryManager.Instance.GetSelectedItem(false) : null;
            float currentRadius = equippedItem != null && equippedItem.isWeapon ? equippedItem.attackRadius : radius;

            // Asetetaan Gizmos-väri (esim. punainen)
            Gizmos.color = Color.red;

            // Piirretään hyökkäysalueen ympyrä
            Gizmos.DrawWireSphere(attackpoint.transform.position, currentRadius);
        }
    }

}