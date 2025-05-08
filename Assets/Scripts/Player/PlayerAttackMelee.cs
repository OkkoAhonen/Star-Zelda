using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;


public class PlayerAttackMelee : MonoBehaviour
{


    public GameObject attackpoint;
    public float radius;
    public LayerMask enemies;

    public BasicBar slider;

    public Animator animator; // Aseta inspektorissa

    public PlayerStatsManager playerStatsManager;



    // Charge muuttujat

    public float maxChargeTime = 3f; //Tämä korvataan myöhemmin itemien arvoilla
    public float Damage = 5f;
    public float Damagebooster;
    public float currentChargeTime = 0f;
    //private bool UpOrDown = false;

    //[SerializeField] private GameObject chargebar;
    //private bool charging = false;

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
               

                if (Input.GetMouseButtonUp(0) )
                {
                    attack1();

                }

            }


            if(equippedItem.type  == ItemType.Weapon)
            {
                if (equippedItem.name.Contains("_1"))
                {
                    animator.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    animator.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                }
            }
        }
    }

    public void attack1()
    {
        animator.SetTrigger("Attack1");
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, equippedItem.attackRadius, enemies);

        if (enemy.Length >= 0) {
            AudioManager.instance.PlaySFX("AttackMiss1");

            CameraShake.instance.StartShake(0.1f, 0.2f);
        
        }

        foreach (Collider2D enemyGameobje in enemy)
        {

            MageSkeletonController mageController = enemyGameobje.GetComponent<MageSkeletonController>();
            //EnemyController enemyhealth = enemyGameobje.GetComponent<EnemyController>();
            //Debug.Log(enemyhealth.gameObject.name + "Moi");
            Vector2 direction = (enemyGameobje.transform.position - transform.position).normalized;

            AttackAudio();

            StartCoroutine(Freeze());
            
           // AudioManager.instance.PlaySFX("Attack1");

            mageController.TakeDamage(5);
            //enemyhealth.TakeDamage(equippedItem.attackDamage  /*  *(float) PlayerStatsManager.instance.GetStat(StatType.Strength)*/, direction, 2f);

        }

       
        


        currentChargeTime = 0f;

    }

    private void AttackAudio()
    {
        int RandomSFXNumber = UnityEngine.Random.Range(1, 5);
        
        Debug.Log("ATTACKaUDIOnUMBER: " +  RandomSFXNumber);
        switch (RandomSFXNumber)
        {
            case 1:
                AudioManager.instance.PlaySFX("Attack1");

                break;
            case 2:
                AudioManager.instance.PlaySFX("Attack2");
                break;
            case 3:
                AudioManager.instance.PlaySFX("Attack3");
                break;
            case 4:
                AudioManager.instance.PlaySFX("Attack4");
                break;
        }
    }

    private IEnumerator Freeze()
    {
        Time.timeScale = 0f;
        float freezeDuration = 0.15f;

        float elapset = 0f;
        while (elapset < freezeDuration)
        {
            elapset += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1f;
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