using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    bool isFacingRight = false; //For sprite flipping so player sprite faces right direction

    private Rigidbody2D rb;

    Animator animator;

    // Saving an input
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Reading an input
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or left/right arrow keys
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or up/down arrow keys

        FlipSprite();

        // Input normalisoidaan diagonaalista liikettä varten
        movement = movement.normalized;
    }

    // Called fixed time stamps for physics updates
    void FixedUpdate()
    {
        // Player moves
        rb.velocity = movement * Player.Speed;

        // Player walking animation
        animator.SetFloat("xVelocity", Math.Abs(rb.velocity.x));
    }

    void FlipSprite()
    {
        if(isFacingRight && movement.x < 0f || !isFacingRight && movement.x > 0f)
        {
            isFacingRight = false;
            Vector3 sprite = transform.localScale;
            sprite.x *= -1f;
            transform.localScale = sprite;
        }
    }
}
