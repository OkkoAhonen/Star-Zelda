using UnityEngine;

public class Managers : MonoBehaviour
{ // This script just keeps this from being deleted (temporary?)
    public gameObject book; 
    private void Awake()
    {
        SetActive(book); // Jos kirja on laitettu pois nenän eestä nii tää aktivoi sen kuitenkin :)
        DontDestroyOnLoad(gameObject);
        PerkSystem perkSystem = new PerkSystem(); // This will set the singleton instance
    }
}
