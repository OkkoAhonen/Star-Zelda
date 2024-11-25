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

    // Start kutsutaan ennen ensimm�ist� frame-p�ivityst�
    void Start()
    {
        // Haetaan Rigidbody2D komponentti
        rb = GetComponent<Rigidbody2D>();
    }

    // Update kutsutaan kerran per frame
    void Update()
    {
        // Inputin lukeminen
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D tai nuolin�pp�imet vasen/oikea
        movement.y = Input.GetAxisRaw("Vertical");   // W/S tai nuolin�pp�imet yl�s/alas

        // Input normalisoidaan diagonaalista liikett� varten
        movement = movement.normalized;
    }

    // FixedUpdate kutsutaan kiinteill� aikav�leill� fysiikkap�ivityksi� varten
    void FixedUpdate()
    {
        // Liikuta pelaajaa
        rb.velocity = movement * Player.Speed;
    }
}
