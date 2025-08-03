using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement2D : MonoBehaviour
{
    public static event Action OnplayerDamaged;

    [SerializeField] private DialogueUI dialogueUI; // Aseta t�m� Inspectorissa
    public float health = 8f, MaxHealth = 8f;

    public DialogueUI DialogueUI => dialogueUI;

    public int money = 0;
    [SerializeField] private Text moneyText;

    private bool isFacingRight = true; // Pelaaja katsoo oletuksena oikealle
    private Rigidbody2D rb;

    // Tallennetaan pelaajan sy�tteet
    private Vector2 movement;

    public bool hasItFlipped = false;

    // Viittaus visuaaliseen osaan (lapsiobjekti)
    [SerializeField] private Transform visuals;

    void Start()
    {
        
        health = MaxHealth;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the Player.");
        }

        if (visuals == null)
        {
            visuals = transform.Find("Visuals"); // Yritet��n l�yt�� lapsi nimelt� "Visuals"
            if (visuals == null)
            {
                Debug.LogError("Visuals child object is missing!");
            }
        }
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = "Money: " + money; // P�ivitt�� UI:n
    }


    void Update()
    {
        movement = Vector2.zero;

        // Lukee pelaajan sy�tteet
        if (Input.GetKey(KeyCode.W)) movement.y = 1;
        if (Input.GetKey(KeyCode.S)) movement.y = -1;
        if (Input.GetKey(KeyCode.A)) movement.x = -1;
        if (Input.GetKey(KeyCode.D)) movement.x = 1;

        // Normalisoidaan liike diagonaalisia tilanteita varten
        movement = movement.normalized;


        if (Input.GetKeyDown(KeyCode.E))
        {
            //Interactable?.Interact(this);
        }
        UpdateMoneyUI();
    }

    void FixedUpdate()
    {
        // Pelaajan liike
        rb.linearVelocity = movement * Player.Speed;
    }


    // Pelaajan vahinkok�sittely
    public void TakeDamage(float damage)
    {
        Debug.Log("Player has taken damage: " + damage);
        health -= damage;

        // Kutsutaan tapahtumaa
        OnplayerDamaged?.Invoke();

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        // Pelaajan kuoleman k�sittely, kuten peli loppuu tai animaatio alkaa
    }

    private void FlipSprite()
    {
        // Tarkistaa, pit��k� sprite k��nt��
        if (visuals != null)
        {
            hasItFlipped = !hasItFlipped;
            isFacingRight = !isFacingRight; // Vaihdetaan suunta

            Vector3 scale = visuals.localScale;
            scale.x *= -1f; // K��nnet��n vain visuals-objekti
            visuals.localScale = scale;
        }
    }
}
