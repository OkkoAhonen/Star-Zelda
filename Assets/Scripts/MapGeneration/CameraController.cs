using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How quickly the camera moves to the target position. Smaller value = faster.")]
    public float smoothTime = 0.3f;

    [Header("Following Settings")]
    [Tooltip("Reference to the player's transform. If null, tries to find Player.Transform.")]
    [SerializeField] private Transform playerTransform; // Voit asettaa Inspectorissa tai se hakee Player.Transform

    // --- Sis‰iset tilat ---
    private Vector3 targetPosition;
    private Vector3 currentVelocity = Vector3.zero;
    private float initialZ;
    private bool isFollowingPlayer = false; // Onko kamera seurantatilassa?
    private Bounds currentRoomBounds;       // Nykyisen huoneen rajat (jos seurataan)
    private Camera cam; // Viittaus omaan Camera-komponenttiin

    void Start()
    {
        cam = GetComponent<Camera>(); // Hae Camera-komponentti
        if (cam == null)
        {
            Debug.LogError("CameraController requires a Camera component on the same GameObject!");
            enabled = false;
            return;
        }
        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraController is designed for an Orthographic camera.");
        }

        initialZ = transform.position.z;
        targetPosition = transform.position;

        // Yrit‰ hakea pelaaja, jos ei asetettu Inspectorissa
        if (playerTransform == null)
        {
            // Yrit‰ k‰ytt‰‰ staattista Player-luokkaa (muista sen rajoitteet)
            if (Player.Transform != null)
            {
                playerTransform = Player.Transform;
                Debug.Log("CameraController found player via Player.Transform.");
            }
            else
            {
                // Viimeinen yritys tagilla (vaatii pelaajalta "Player"-tagin)
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                    Debug.Log("CameraController found player via FindWithTag('Player').");
                }
                else
                {
                    Debug.LogError("CameraController could not find Player Transform! Following will not work.");
                }
            }
        }
    }

    // Asettaa kameran tilan (staattinen tai seuraava) ja mahdolliset rajat
    public void SetCameraMode(bool follow, Bounds roomBounds = default, Vector3 staticTarget = default)
    {
        isFollowingPlayer = follow;
        if (isFollowingPlayer)
        {
            currentRoomBounds = roomBounds;
            Debug.Log($"CameraController set to FOLLOW mode. Bounds: Center={currentRoomBounds.center}, Size={currentRoomBounds.size}");
            // T‰ss‰ ei tarvitse asettaa targetPositionia, koska se lasketaan LateUpdatessa pelaajasta
        }
        else
        {
            targetPosition = new Vector3(staticTarget.x, staticTarget.y, initialZ);
            Debug.Log($"CameraController set to STATIC mode. Target: {targetPosition}");
        }
        // Nollaa nopeus tilan vaihtuessa est‰‰ksesi oudon hypyn
        currentVelocity = Vector3.zero;
    }

    void LateUpdate()
    {
        Vector3 finalTargetPosition;

        if (isFollowingPlayer)
        {
            if (playerTransform == null) return;

            // 1. Haluttu kohde = pelaajan sijainti
            Vector3 desiredPosition = new Vector3(playerTransform.position.x, playerTransform.position.y, initialZ);

            // --- KOMMENTOI RAJAUSLOGIIKKA POIS TESTAUKSEN AJAKSI ---
            
            // 2. Laske kameran n‰kym‰n koko
            float camHeight = cam.orthographicSize * 2;
            float camWidth = camHeight * cam.aspect;

            // 3. Laske sallitut rajat
            if (currentRoomBounds != default)
            {
                 float minX = currentRoomBounds.min.x + camWidth / 2;
                 float maxX = currentRoomBounds.max.x - camWidth / 2;
                 float minY = currentRoomBounds.min.y + camHeight / 2;
                 float maxY = currentRoomBounds.max.y - camHeight / 2;

                 if (minX > maxX) { minX = maxX = currentRoomBounds.center.x; }
                 if (minY > maxY) { minY = maxY = currentRoomBounds.center.y; }

                 // 4. Rajoita haluttu sijainti
                 float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
                 float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
                 finalTargetPosition = new Vector3(clampedX, clampedY, initialZ);
            }
            else
            {
                 Debug.LogWarning("Camera following but room bounds not set!");
                 finalTargetPosition = desiredPosition; // K‰yt‰ rajoittamatonta sijaintia
            }
            
            // --- K‰yt‰ AINA rajoittamatonta sijaintia testiss‰ ---
            finalTargetPosition = desiredPosition;
            // --- KOMMENTOINTI LOPPUU ---

        }
        else // Staattinen tila
        {
            finalTargetPosition = targetPosition;
        }

        // Liikuta kameraa kohti lopullista kohdetta
        if (Vector3.Distance(transform.position, finalTargetPosition) > 0.01f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, finalTargetPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            transform.position = finalTargetPosition;
            currentVelocity = Vector3.zero;
        }
    }

    // SnapToTarget pysyy samana, mutta k‰ytet‰‰n sit‰ vain staattiseen keskitykseen
    public void SnapToTarget(Vector3 newTargetCenter)
    {
        targetPosition = new Vector3(newTargetCenter.x, newTargetCenter.y, initialZ);
        transform.position = targetPosition;
        currentVelocity = Vector3.zero;
        isFollowingPlayer = false; // Oletetaan, ett‰ snap on aina staattiseen tilaan
        Debug.Log($"CameraController: Snapped to static position {targetPosition}");
    }
}