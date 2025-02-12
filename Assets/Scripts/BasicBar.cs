using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicBar : MonoBehaviour
{

    [SerializeField] private Slider slider;
    // Start is called before the first frame update
    private void Awake()
    {
        // Hakee Slider-komponentin automaattisesti, jos sitä ei ole asetettu
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }




    }

    public void UpdateSlider(float currentValue, float maxValue)
    {
        slider.value = currentValue/maxValue;  
    }
}
