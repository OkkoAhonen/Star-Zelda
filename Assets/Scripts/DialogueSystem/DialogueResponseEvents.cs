using UnityEngine;
using System; // Tarvitaan Array.Resize-metodille

// Oletetaan, ett‰ sinulla on jo n‰m‰ luokat m‰‰riteltyn‰ jossain projektissasi
// ja ne ovat [System.Serializable], jotta Unity voi serialisoida ne:
//
// [System.Serializable]
// public class Response
// {
//     public string ResponseText;
//     // ... muita mahdollisia response-tietoja
// }
//
// [System.Serializable]
// public class DialogueObject
// {
//     public Response[] Responses;
//     // ... muita dialogiobjektin tietoja
// }
//
// [System.Serializable]
// public class ResponseEvent
// {
//     public string name;
//     public UnityEngine.Events.UnityEvent onChosen; // Tai mik‰ tahansa UnityEvent-kentt‰si onkaan
// }


public class DialogueResponseEvents : MonoBehaviour
{
    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private ResponseEvent[] events;

    public DialogueObject DialogueObject => dialogueObject;
    public ResponseEvent[] Events => events;

    public void OnValidate()
    {
        if (dialogueObject == null || dialogueObject.Responses == null)
        {
            // Jos dialogueObjectia tai sen vastauksia ei ole,
            // on turvallista tyhjent‰‰ events-array tai j‰tt‰‰ se ennalleen.
            // Tyhjent‰minen voi olla selke‰mp‰‰.
            if (events != null && events.Length > 0)
            {
                Array.Resize(ref events, 0);
            }
            return;
        }

        int requiredSize = dialogueObject.Responses.Length;

        // 1. Varmista, ett‰ events-array on olemassa ja oikean kokoinen.
        // Array.Resize s‰ilytt‰‰ olemassa olevat elementit, jos array kasvaa tai pienenee.
        // Uudet elementit (jos array kasvaa) ovat null (koska ResponseEvent on luokka).
        if (events == null)
        {
            events = new ResponseEvent[requiredSize];
        }
        else if (events.Length != requiredSize)
        {
            Array.Resize(ref events, requiredSize);
        }

        // 2. K‰y l‰pi events-array ja p‰ivit‰/alusta elementit.
        for (int i = 0; i < requiredSize; i++)
        {
            // Hae vastaava vastaus dialogueObjectista.
            // Lis‰t‰‰n tarkistus, ettei menn‰ Responses-arrayn yli, vaikka requiredSize pit‰isi olla oikea.
            Response currentResponse = (i < dialogueObject.Responses.Length) ? dialogueObject.Responses[i] : null;

            // KORJAUS TƒSSƒ:
            // Jos events[i] on null (eli sille ei ole viel‰ oliota, esim. arrayta kasvatettiin),
            // luo uusi ResponseEvent-olio.
            if (events[i] == null)
            {
                events[i] = new ResponseEvent();
            }

            // Nyt events[i]:ss‰ on taatusti olio (joko juuri luotu tai olemassa oleva).
            // P‰ivit‰ sen nimi vastaamaan ResponseTexti‰.
            // TƒMƒ EI YLIKIRJOITA KOKO OLIOTA (ja sen UnityEvent-kentti‰),
            // VAIN SEN NAME-KENTƒN.
            if (currentResponse != null && !string.IsNullOrEmpty(currentResponse.ResponseText))
            {
                events[i].name = currentResponse.ResponseText;
            }
            else
            {
                // Aseta oletusnimi, jos response-dataa ei ole saatavilla
                // tai jos haluat n‰ytt‰‰ jotain muuta.
                events[i].name = $"Response {i} (Data Missing or Empty)";
            }
        }
    }
}