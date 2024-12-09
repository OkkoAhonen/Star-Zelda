using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI; // Varmista, että asetat tämän Inspectorissa

    //public float speed = Player.Speed;
    

    public DialogueUI DialogueUI => dialogueUI;
    public Interactable Interactable { get; set; }

    private bool isFacingRight = true; // Oletuksena pelaaja katsoo oikealle
    private Rigidbody2D rb;
    //private Animator animator;

    // Tallennetaan pelaajan syötteet
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the Player.");
        }

        /*animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing from the Player.");
        }*/
    }

    void Update()
    {
        

        // Lukee pelaajan syötteet
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D tai nuolinäppäimet vasen/oikea
        movement.y = Input.GetAxisRaw("Vertical");   // W/S tai nuolinäppäimet ylös/alas

        // Normalisoidaan liike diagonaalisia tilanteita varten
        movement = movement.normalized;

        // Päivitetään spritejen kääntö
        FlipSprite();

        if (dialogueUI.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(Interactable != null)
            {
                Interactable.Interact(this);
            }
        }
        
    }

    void FixedUpdate()
    {
        // Pelaajan liike
        rb.velocity = movement * Player.Speed;

        // Pelaajan kävelyanimaatio
        //animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
    }

    void FlipSprite()
    {
        // Tarkistaa, pitääkö sprite kääntää
        if (isFacingRight && movement.x < 0f || !isFacingRight && movement.x > 0f)
        {
            isFacingRight = !isFacingRight; // Vaihdetaan suunta
            Vector3 sprite = transform.localScale;
            sprite.x *= -1f; // Käännetään sprite
            transform.localScale = sprite;
        }
    }
    public void SetInteractable(Interactable interactable)
    {
        Interactable = interactable;
    }

}
