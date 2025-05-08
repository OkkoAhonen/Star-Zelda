// Laita t�m� skripti p�ly-efekti prefabiin
using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f; // S��d� t�m� vastaamaan efektisi kestoa

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}