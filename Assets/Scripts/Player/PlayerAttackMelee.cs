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
        animator.speed = 2f;
    }

    // Update is called once per frame
    void Update()
    {


        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
            if (equippedItem.isWeapon == true)
            {
                chargeSword();

                if (Input.GetMouseButtonUp(0))
                {
                    attack1();

                }

            }
        }
    }

    public void attack1()
    {
        animator.SetTrigger("Attack1");
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        Damagebooster = equippedItem.damageBooster;
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, equippedItem.attackRadius, enemies);
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
    }

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