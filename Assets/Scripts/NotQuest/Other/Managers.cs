using UnityEngine;

public class Managers : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PerkSystem perkSystem = new PerkSystem(); // This will set the singleton instance
    }
}
