using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject heartPrefab;
    public PlayerHeart playerHealth;
    public GameObject healthHearts;

    List<GameObject> hearts;

    public void Start()
    {
        hearts = new List<GameObject>(healthHearts.GetComponentsInChildren<GameObject>());
    }

    public void CreateHearts()
    {

    }
}