using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelastaMinut : MonoBehaviour
{

    [Header("Tunniste t‰m‰n objektin uniikkiudelle")]
    [Tooltip("Anna t‰lle objektille uniikki merkkijono. Jos toinen objekti samalla tunnisteella yritt‰‰ s‰ily‰, se tuhotaan.")]
    public string uniqueIdentifier;

    // Staattinen sanakirja (Dictionary) pit‰m‰‰n kirjaa jo olemassaolevista
    // s‰ilytett‰vist‰ objekteista niiden tunnisteiden perusteella.
    // Avain = uniqueIdentifier (string), Arvo = s‰ilytett‰v‰ GameObject
    private static Dictionary<string, GameObject> persistentObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        // --- T‰rke‰ Tarkistus 1: Onko tunniste asetettu? ---
        if (string.IsNullOrEmpty(uniqueIdentifier))
        {
            Debug.LogError($"GameObject '{gameObject.name}' has KeepUniqueObjectOnLoad script but NO uniqueIdentifier set! Object will NOT persist uniquely.", gameObject);
            // Voit p‰‰tt‰‰ tuhotaanko objekti vai sallitaanko sen vain olla normaali (ei-pysyv‰)
            // Destroy(gameObject); // Poista kommentti, jos haluat tuhota objektin ilman tunnistetta
            return; // Lopetetaan Awake-metodin suoritus t‰h‰n
        }

        // --- T‰rke‰ Tarkistus 2: Onko objekti t‰ll‰ tunnisteella JO olemassa ja s‰ilytetty? ---
        if (persistentObjects.ContainsKey(uniqueIdentifier))
        {
            // Kyll‰, objekti t‰ll‰ tunnisteella on jo rekisterˆity.
            // Tarkistetaan, onko se t‰m‰ sama objekti vai joku muu (duplikaatti).
            if (persistentObjects[uniqueIdentifier] != this.gameObject)
            {
                // Se on ERI objekti (eli t‰m‰ on duplikaatti). Tuhotaan t‰m‰ uusi.
                Debug.LogWarning($"Duplicate persistent object with ID '{uniqueIdentifier}' found. Destroying new instance '{gameObject.name}'. Keeping original '{persistentObjects[uniqueIdentifier].name}'.", gameObject);
                Destroy(gameObject);
            }
            // Jos se OLI t‰m‰ sama objekti, ei tehd‰ mit‰‰n (t‰m‰ voi tapahtua harvinaisissa tilanteissa).
        }
        else
        {
            // Ei, objektia t‰ll‰ tunnisteella EI OLE viel‰ rekisterˆity.
            // Tehd‰‰n t‰st‰ se ainokainen ja merkit‰‰n se s‰ilytett‰v‰ksi.
            Debug.Log($"Registering object '{gameObject.name}' with unique ID '{uniqueIdentifier}' to persist.", gameObject);
            persistentObjects.Add(uniqueIdentifier, this.gameObject);
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // (Vapaaehtoinen mutta suositeltava) Siivous, jos persistentti objekti tuhotaan jostain syyst‰
    private void Start()
    {
        //uniqueIdentifier = gameObject.name;
    }
    void OnDestroy()
    {
        // Jos t‰m‰ objekti OLI se, joka oli rekisterˆity sanakirjaan,
        // poistetaan se sielt‰, jotta tunniste vapautuu.
        if (persistentObjects.ContainsKey(uniqueIdentifier) && persistentObjects[uniqueIdentifier] == this.gameObject)
        {
            persistentObjects.Remove(uniqueIdentifier);
            // Debug.Log($"Removed persistent object '{gameObject.name}' with ID '{uniqueIdentifier}' from registry.");
        }
    }
}
