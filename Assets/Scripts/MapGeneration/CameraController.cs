using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("How quickly the camera moves to the target position. Smaller value = faster.")]
    public float smoothTime = 0.3f; // Voit s��t�� t�t� Inspectorissa

    [SerializeField] private Vector3 targetPosition; // Sijainti, johon kamera pyrkii
    private Vector3 currentVelocity = Vector3.zero; // Tarvitaan SmoothDamp-funktiolle
    private float initialZ; // Kameran alkuper�inen Z-koordinaatti

    void Start()
    {
        // Tallennetaan kameran Z-koordinaatti, jotta se ei muutu 2D-peliss�.
        // Oletetaan, ett� kamera on jo asetettu oikealle Z-et�isyydelle.
        initialZ = transform.position.z;

        // Asetetaan aluksi targetiksi kameran nykyinen sijainti,
        // jotta se ei hypp�� pelin alussa.
        targetPosition = transform.position;
    }

    // T�m� metodi p�ivitt�� kameran kohdesijainnin.
    // Muut skriptit (kuten ChangeRooms) kutsuvat t�t�.
    public void MoveToTarget(Vector3 newTargetCenter)
    {
        // P�ivitet��n X ja Y, mutta pidet��n alkuper�inen Z.
        targetPosition = new Vector3(newTargetCenter.x, newTargetCenter.y, initialZ);
        Debug.Log($"CameraController: New target position set to {targetPosition}");
    }

    // LateUpdate ajetaan kaikkien Update-funktioiden j�lkeen joka framessa.
    // Se on paras paikka kameran liikuttamiselle, jotta se seuraa
    // p�ivitettyj� hahmojen sijainteja sulavasti.
    void LateUpdate()
    {
        // Jos kamera ei ole viel� kohteessa...
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f) // Pieni toleranssi
        {
            // Liikuta kameraa sulavasti kohti targetPositionia k�ytt�en SmoothDampia.
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            // Kun ollaan tarpeeksi l�hell�, napsauta tarkasti paikalleen ja nollaa nopeus.
            transform.position = targetPosition;
            currentVelocity = Vector3.zero;
        }
    }

    // Lis�metodi, jolla kameran voi siirt�� v�litt�m�sti kohteeseen.
    // Hy�dyllinen esim. pelin alussa tai tason latauksen j�lkeen.
    public void SnapToTarget(Vector3 newTargetCenter)
    {
        targetPosition = new Vector3(newTargetCenter.x, newTargetCenter.y, initialZ);
        transform.position = targetPosition;
        currentVelocity = Vector3.zero; // Nollaa nopeus, ettei tule outoa liikett� seuraavassa framessa
        Debug.Log($"CameraController: Snapped to position {targetPosition}");
    }
}