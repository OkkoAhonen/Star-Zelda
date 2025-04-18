using UnityEngine;

public class Managers : MonoBehaviour
{ // This script just keeps this from being deleted (temporary?)
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PerkSystem perkSystem = new PerkSystem(); // This will set the singleton instance
    }
}
