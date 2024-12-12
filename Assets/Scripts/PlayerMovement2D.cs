using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public static event Action OnplayerDamaged;


    [SerializeField] private DialogueUI dialogueUI; // Varmista, että asetat tämän Inspectorissa

    public float health = 8f, MaxHealth = 8f;

    public DialogueUI DialogueUI => dialogueUI;
    public Interactable Interactable { get; set; }

    private bool isFacingRight = true; // Oletuksena pelaaja katsoo oikealle
    private Rigidbody2D rb;

    // Tallennetaan pelaajan syötteet
    private Vector2 movement;

    void Start()
    {
        health = MaxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the Player.");
        }
    }

    void Update()
    {
        movement.x = 0;
        movement.y = 0;

        if (Input.GetKey(KeyCode.W))
            movement.y = 1;
        if (Input.GetKey(KeyCode.S))
            movement.y = -1;
        if (Input.GetKey(KeyCode.A))
            movement.x = -1;
        if (Input.GetKey(KeyCode.D))
            movement.x = 1;

        // Normalisoidaan liike diagonaalisia tilanteita varten
        movement = movement.normalized;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Interactable != null)
            {
                Interactable.Interact(this);
            }
        }
    }

    void FixedUpdate()
    {
        // Pelaajan liike
        rb.velocity = movement * Player.Speed;
    }

    // Tämä metodi vie pelaajalta vahinkoa
    public void TakeDamage(float damage)
    {
        Debug.Log("Player has taken damage" +  damage);
        health -= damage;
        OnplayerDamaged?.Invoke();
        if (health <= 0)
        {
            Die();
        }


    }

    // Pelaajan kuolema
    private void Die()
    {
        Debug.Log("Player has died.");
        // Tässä voit lisätä pelaajan kuoleman käsittelyn (esim. pelin lopetus, animaatiot jne.)
    }

    public void SetInteractable(Interactable interactable)
    {
        Interactable = interactable;
    }
}

/*void FlipSprite()
{
    // Tarkistaa, pitääkö sprite kääntää
    if (isFacingRight && movement.x < 0f || !isFacingRight && movement.x > 0f)
    {
        isFacingRight = !isFacingRight; // Vaihdetaan suunta
        Vector3 sprite = transform.localScale;
        sprite.x *= -1f; // Käännetään sprite
        transform.localScale = sprite;
    }
}*/