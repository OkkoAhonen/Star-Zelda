using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ResponseEvent
{
    [HideInInspector] public string name; // Nimi vastaukselle, näkyy vain editorissa
    [SerializeField] private UnityEvent onPickedResponse; // Yksityinen kenttä UnityEventille

    // Julkinen getter, joka palauttaa UnityEvent-instanssin
    public UnityEvent OnPickedResponse => onPickedResponse;
}
