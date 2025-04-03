using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAttack : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject bow;

    public float bowAttackTimer = 1f;
    private float timer = 0f;
    public float bowDamage = 15f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

            watchArrowTimer();

            if (equippedItem.isPotion == true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    bowAttack();
                }
            }
        }
    }

    public void bowAttack()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        if (timer >= equippedItem.potionActivetimer)
        {
            // Muutetaan hiiren sijainti pelimaailman koordinaateiksi
            Vector3 arrowPlace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            arrowPlace.z = 0f; // Asetetaan z-arvo, jotta se näkyy 2D-pelissä

            Instantiate(arrow, arrowPlace, Quaternion.identity);
            Debug.Log($"Potion spawned at: {arrowPlace}");

            timer = 0f;
        }
    }

    public void watchArrowTimer()
    {
        timer += Time.deltaTime;
    }
}
