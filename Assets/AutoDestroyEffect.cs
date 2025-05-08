// Laita tämä skripti pöly-efekti prefabiin
using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f; // Säädä tämä vastaamaan efektisi kestoa

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}