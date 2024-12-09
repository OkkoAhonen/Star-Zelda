using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI; // Varmista, ett� asetat t�m�n Inspectorissa

    //public float speed = Player.Speed;
    

    public DialogueUI DialogueUI => dialogueUI;
    public Interactable Interactable { get; set; }

    private bool isFacingRight = true; // Oletuksena pelaaja katsoo oikealle
    private Rigidbody2D rb;
    //private Animator animator;

    // Tallennetaan pelaajan sy�tteet
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
        

        // Lukee pelaajan sy�tteet
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D tai nuolin�pp�imet vasen/oikea
        movement.y = Input.GetAxisRaw("Vertical");   // W/S tai nuolin�pp�imet yl�s/alas

        // Normalisoidaan liike diagonaalisia tilanteita varten
        movement = movement.normalized;

        // P�ivitet��n spritejen k��nt�
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

        // Pelaajan k�velyanimaatio
        //animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
    }

    void FlipSprite()
    {
        // Tarkistaa, pit��k� sprite k��nt��
        if (isFacingRight && movement.x < 0f || !isFacingRight && movement.x > 0f)
        {
            isFacingRight = !isFacingRight; // Vaihdetaan suunta
            Vector3 sprite = transform.localScale;
            sprite.x *= -1f; // K��nnet��n sprite
            transform.localScale = sprite;
        }
    }
    public void SetInteractable(Interactable interactable)
    {
        Interactable = interactable;
    }

}
