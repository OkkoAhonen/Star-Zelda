using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("How quickly the camera moves to the target position. Smaller value = faster.")]
    public float smoothTime = 0.3f; // Voit säätää tätä Inspectorissa

    [SerializeField] private Vector3 targetPosition; // Sijainti, johon kamera pyrkii
    private Vector3 currentVelocity = Vector3.zero; // Tarvitaan SmoothDamp-funktiolle
    private float initialZ; // Kameran alkuperäinen Z-koordinaatti

    void Start()
    {
        // Tallennetaan kameran Z-koordinaatti, jotta se ei muutu 2D-pelissä.
        // Oletetaan, että kamera on jo asetettu oikealle Z-etäisyydelle.
        initialZ = transform.position.z;

        // Asetetaan aluksi targetiksi kameran nykyinen sijainti,
        // jotta se ei hyppää pelin alussa.
        targetPosition = transform.position;
    }

    // Tämä metodi päivittää kameran kohdesijainnin.
    // Muut skriptit (kuten ChangeRooms) kutsuvat tätä.
    public void MoveToTarget(Vector3 newTargetCenter)
    {
        // Päivitetään X ja Y, mutta pidetään alkuperäinen Z.
        targetPosition = new Vector3(newTargetCenter.x, newTargetCenter.y, initialZ);
        Debug.Log($"CameraController: New target position set to {targetPosition}");
    }

    // LateUpdate ajetaan kaikkien Update-funktioiden jälkeen joka framessa.
    // Se on paras paikka kameran liikuttamiselle, jotta se seuraa
    // päivitettyjä hahmojen sijainteja sulavasti.
    void LateUpdate()
    {
        // Jos kamera ei ole vielä kohteessa...
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f) // Pieni toleranssi
        {
            // Liikuta kameraa sulavasti kohti targetPositionia käyttäen SmoothDampia.
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            // Kun ollaan tarpeeksi lähellä, napsauta tarkasti paikalleen ja nollaa nopeus.
            transform.position = targetPosition;
            currentVelocity = Vector3.zero;
        }
    }

    // Lisämetodi, jolla kameran voi siirtää välittömästi kohteeseen.
    // Hyödyllinen esim. pelin alussa tai tason latauksen jälkeen.
    public void SnapToTarget(Vector3 newTargetCenter)
    {
        targetPosition = new Vector3(newTargetCenter.x, newTargetCenter.y, initialZ);
        transform.position = targetPosition;
        currentVelocity = Vector3.zero; // Nollaa nopeus, ettei tule outoa liikettä seuraavassa framessa
        Debug.Log($"CameraController: Snapped to position {targetPosition}");
    }
}