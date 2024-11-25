using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    // Liikkumisen nopeus

    // Rigidbody2D komponentti
    private Rigidbody2D rb;

    // Inputin tallentaminen
    private Vector2 movement;

    // Start kutsutaan ennen ensimmäistä frame-päivitystä
    void Start()
    {
        // Haetaan Rigidbody2D komponentti
        rb = GetComponent<Rigidbody2D>();
    }

    // Update kutsutaan kerran per frame
    void Update()
    {
        // Inputin lukeminen
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D tai nuolinäppäimet vasen/oikea
        movement.y = Input.GetAxisRaw("Vertical");   // W/S tai nuolinäppäimet ylös/alas

        // Input normalisoidaan diagonaalista liikettä varten
        movement = movement.normalized;
    }

    // FixedUpdate kutsutaan kiinteillä aikaväleillä fysiikkapäivityksiä varten
    void FixedUpdate()
    {
        // Liikuta pelaajaa
        rb.velocity = movement * Player.Speed;
    }
}
