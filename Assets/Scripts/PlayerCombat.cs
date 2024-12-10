using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int hitDamage = 3; // Miekan tekem� vahinko.
    public GameObject sword; // Pelaajan miekka.
    public float swingSpeed = 200f; // Miekan heilautuksen nopeus (asteet per sekunti).
    private bool isSwinging = false; // Onko miekka parhaillaan heilautuksessa.
    private Vector2 attackDirection = Vector2.right; // Ly�nnin oletussuunnan vektori.
    private Vector2 swordOffset; // Miekan alkuper�inen sijainti pelaajasta (offset).

    [SerializeField] private Transform UpSwing;
    [SerializeField] private Transform DownSwing;
    [SerializeField] private Transform LeftSwing;
    [SerializeField] private Transform RightSwing;

    void Start()
    {
        if (sword == null)
        {
            Debug.LogError("Sword GameObject is not assigned to PlayerCombat!");
        }

        // Alustetaan miekan alkuper�inen sijainti (offset) pelaajasta
        swordOffset = sword.transform.localPosition;
    }

    void Update()
    {
        HandleAttackDirection(); // P�ivitt�� ly�nnin suunnan nuolin�pp�imist�.
        if (Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartCoroutine(SwingSword());
        }
    }

    // P�ivitt�� hy�kk�yksen suunnan nuolin�pp�inten perusteella.
    void HandleAttackDirection()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            attackDirection = Vector2.up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            attackDirection = Vector2.down;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            attackDirection = Vector2.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            attackDirection = Vector2.right;
        }

        // Vaihdetaan miekan sijainti oikealle paikalle, riippuen nuolin�pp�imest�
        UpdateSwordPosition();
    }

    // P�ivitt�� miekan sijainnin sen mukaan, mik� suunta on valittu
    void UpdateSwordPosition()
    {
        if (attackDirection == Vector2.up)
        {
            sword.transform.position = UpSwing.position;
        }
        else if (attackDirection == Vector2.down)
        {
            sword.transform.position = DownSwing.position;
        }
        else if (attackDirection == Vector2.left)
        {
            sword.transform.position = LeftSwing.position;
        }
        else if (attackDirection == Vector2.right)
        {
            sword.transform.position = RightSwing.position;
        }
    }

    // Miekan heilautus kaarena.
    IEnumerator SwingSword()
    {
        isSwinging = true;

        // Lasketaan perussuunnan kulma hy�kk�yssuunnan perusteella.
        float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - 45f; // Aloita vasemmalta.
        float endAngle = baseAngle + 45f;  // Lopeta oikealle.
        float currentAngle = startAngle;

        // Asetetaan miekan paikka suhteessa pelaajaan
        sword.transform.localPosition = swordOffset;

        // Aloitetaan heilautus.
        while (currentAngle <= endAngle)
        {
            // P�ivitet��n miekan rotaatio suhteessa pelaajaan.
            sword.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

            // Siirryt��n seuraavaan kulmaan swingSpeedin perusteella.
            currentAngle += swingSpeed * Time.deltaTime;

            // Siirret��n miekka pelaajan eteenp�in hy�kk�yssuunnan mukaan
            sword.transform.localPosition = swordOffset + attackDirection * 0.5f; // 0.5f on et�isyys pelaajasta

            yield return null; // Odotetaan seuraavaa framea.
        }

        // Palautetaan miekka alkuasentoon.
        sword.transform.localRotation = Quaternion.Euler(0, 0, 0);
        sword.transform.localPosition = swordOffset; // Miekan alkuper�inen sijainti
        isSwinging = false;
    }

    // T�m� metodi kutsutaan, kun miekka osuu viholliseen.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("ly�nti viholliseen " + other.name);

            // Haetaan EnemyStats-komponentti viholliselta.
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();

            if (enemyStats != null)
            {
                // V�hennet��n vihollisen terveytt�.
                enemyStats.maxHealth = Mathf.Max(0, enemyStats.maxHealth - hitDamage);
                Debug.Log("Hit enemy! Remaining health: " + enemyStats.maxHealth);

                // Tarkistetaan, kuoleeko vihollinen.
                if (enemyStats.maxHealth == 0)
                {
                    other.GetComponent<EnemyController>()?.Death();
                }
            }
            else
            {
                Debug.LogWarning("Enemy does not have an EnemyStats component.");
            }
        }
    }
}
