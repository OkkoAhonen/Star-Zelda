using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class testi : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    public int numero = 0;

    public float numero2 = 1f; public float numero3 = 2f;

    // Start is called before the first frame update

    private void Awake()
    {
        
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
 
    }

    // Update is called once per frame
    void Update()
    {
        Annapositio();

    }

    public void Annapositio()
    {
        rb.position = new Vector2(numero2, numero3);
    }
}
