using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionAttack : MonoBehaviour
{
    [SerializeField] private GameObject potionSpash;
    public float potionAttackTimer = 2f;
    private float timer = 0f;

    public float potionDamage = 20f;

    void Update()
    {
        if (InventoryManager.Instance.GetSelectedItem(false) != null)
        {
            Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);

            watchPotionTimer();

            if (equippedItem.type == ItemType.potion)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    potionAttack();
                }
            }
        }
    }

    public void potionAttack()
    {
        Item equippedItem = InventoryManager.Instance.GetSelectedItem(false);
        if (timer >= equippedItem.potionActivetimer)
        {
            // Muutetaan hiiren sijainti pelimaailman koordinaateiksi
            Vector3 potionplace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            potionplace.z = 0f; // Asetetaan z-arvo, jotta se näkyy 2D-pelissä

            Instantiate(potionSpash, potionplace, Quaternion.identity);
            Debug.Log($"Potion spawned at: {potionplace}");

            timer = 0f;
        }
    }

    public void watchPotionTimer()
    {
        timer += Time.deltaTime;
    }
}
