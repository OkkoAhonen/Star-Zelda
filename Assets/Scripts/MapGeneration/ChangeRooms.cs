using UnityEngine;
using UnityEngine.Tilemaps; // <<< Varmista t�m� using-lauseke
using System.Linq;         // <<< Varmista t�m� using-lauseke
using Unity.VisualScripting;

// RequireComponentia ei v�ltt�m�tt� tarvita, jos skripti on pelaajassa itsess��n
// [RequireComponent(typeof(Collider2D))]
public class ChangeRooms : MonoBehaviour
{
    private CameraController cameraController;
    private GenerateLevel levelGenerator;
    private bool changeRoomCooldown = false;
    private float cooldownTime;

    void Start()
    {
        // K�yt� modernia tapaa etsi� (tai vanhaa FindObjectOfType jos Unity-versio vaatii)
        levelGenerator = FindFirstObjectByType<GenerateLevel>();
        if (levelGenerator == null)
        {
            Debug.LogError("GenerateLevel script not found in the scene!");
            enabled = false; return;
        }
        if (levelGenerator.currentSettings == null)
        {
            // T�m� tarkistus on nyt t�rke�mpi, koska GenerateLevel voi poistaa itsens� k�yt�st� Awaken aikana
            if (levelGenerator.enabled)
            { // Tarkista onko generaattori viel� p��ll�
                Debug.LogError("GenerateLevel script does not have LevelGenerationSettings loaded! Check for errors in GenerateLevel's Awake.");
            }
            else
            {
                Debug.Log("GenerateLevel was disabled, likely due to initialization errors. ChangeRooms cannot function.");
            }
            enabled = false; return;
        }

        cooldownTime = levelGenerator.currentSettings.roomChangeTime;

        cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController script not found in the scene! Camera movement will not work.");
            // Ei v�ltt�m�tt� poisteta skripti� k�yt�st�, peli voi toimia ilman kameran liikett�kin
        }

        // Pelaajan Transformin asetus (Paras paikka on pelaajan omassa skriptiss�!)
        if (Player.Transform == null)
        {
            if (gameObject.CompareTag("Player"))
            {
                Player.Transform = this.transform;
                Debug.LogWarning("Fallback: Set Player.Transform in ChangeRooms. Set this in Player's own Awake/Start script.");
            }
            else
            {
                Debug.LogWarning("Player.Transform is null in ChangeRooms.Start() and this is not the player. Ensure Player.Transform is set elsewhere.");
            }
        }

        // Alkuasetukset l�ht�huoneelle
        Room startRoom = levelGenerator.GetRoomAt(Vector2.zero);
        if (startRoom != null)
        {
            // Varmista ett� start roomin instanssi on olemassa (GenerateLevel aktivoi sen)
            if (startRoom.RoomInstance == null && levelGenerator.enabled)
            {
                Debug.LogError("Start room data found, but its RoomInstance is NULL! Check GenerateLevel logs.");
                // Yritet��n l�yt�� se uudelleen varmuuden vuoksi? Ei yleens� auta.
                startRoom.RoomInstance = GameObject.Find($"{startRoom.template.type} Room (0,0)"); // H�t�ratkaisu, voi ep�onnistua
            }


            Player.CurrentRoom = startRoom; // Aseta nykyinen huone
            levelGenerator.UpdateAllMinimapIcons(); // P�ivit� kaikki ikonit alussa

            if (startRoom.RoomInstance != null)
            { // Tarkista uudelleen, jos se l�ytyi
                EnableDoors(Player.CurrentRoom); // Aktivoi ovet start-huoneessa

                if (cameraController != null)
                {
                    // Aseta kameran alkutila
                    bool follow = startRoom.template.followPlayerCamera;
                    Vector3 targetPos = GetRoomCenterTarget(startRoom);
                    Bounds bounds = GetRoomBounds(startRoom);

                    // Aseta tila ja snappaa kamera paikalleen
                    cameraController.SetCameraMode(follow, bounds, targetPos);
                    cameraController.SnapToTarget(targetPos); // Snapataan joka tapauksessa alkuun
                }
            }
            else if (levelGenerator.enabled)
            {
                // Jos generaattori on p��ll� mutta instanssia ei ole, jotain on pahasti pieless�
                Debug.LogError("Start Room instance could not be found or created properly!");
            }

        }
        else if (levelGenerator.enabled)
        {
            // Jos generaattori on p��ll� mutta start roomia ei l�ydy, se on generointivirhe
            Debug.LogError("Start room (0,0) data not found after level generation!");
        }
    }

    void EndChangeRoomCooldown()
    {
        changeRoomCooldown = false;
    }

    // Aktivoi/Deaktivoi ovet huoneen naapurien perusteella
    void EnableDoors(Room room)
    {
        if (room == null || room.RoomInstance == null)
        {
            Debug.LogWarning($"EnableDoors called with null room or instance.");
            return;
        }
        Transform doorsParent = room.RoomInstance.transform.Find("Doors");
        if (doorsParent == null)
        {
            Debug.LogError($"'Doors' child object not found in room instance: {room.RoomInstance.name}");
            return;
        }

        ActivateDoor(doorsParent, "LeftDoor", levelGenerator.GetRoomAt(room.Location + Vector2.left) != null);
        ActivateDoor(doorsParent, "RightDoor", levelGenerator.GetRoomAt(room.Location + Vector2.right) != null);
        ActivateDoor(doorsParent, "TopDoor", levelGenerator.GetRoomAt(room.Location + Vector2.up) != null);
        ActivateDoor(doorsParent, "BottomDoor", levelGenerator.GetRoomAt(room.Location + Vector2.down) != null);
    }

    // Apumetodi yksitt�isen oven aktivointiin
    void ActivateDoor(Transform doorsParent, string doorName, bool active)
    {
        Transform door = doorsParent.Find(doorName);
        if (door != null)
        {
            door.gameObject.SetActive(active);
        }
        else
        {
            Debug.LogWarning($"Door '{doorName}' not found under '{doorsParent.name}' in room '{doorsParent.parent.name}'");
        }
    }

    // P��logiikka huoneen vaihtoon oven triggerist�
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (changeRoomCooldown || levelGenerator == null || Player.CurrentRoom == null || !levelGenerator.enabled) return;

        string doorName = collision.gameObject.name;
        Vector2 direction; string oppositeDoorName; Vector3 playerSpawnOffset;

        switch (doorName)
        {
            case "LeftDoor": direction = Vector2.left; oppositeDoorName = "RightDoor"; playerSpawnOffset = new Vector3(-1.5f, 0, 0); break;
            case "RightDoor": direction = Vector2.right; oppositeDoorName = "LeftDoor"; playerSpawnOffset = new Vector3(1.5f, 0, 0); break;
            case "TopDoor": direction = Vector2.up; oppositeDoorName = "BottomDoor"; playerSpawnOffset = new Vector3(0, 1.5f, 0); break;
            case "BottomDoor": direction = Vector2.down; oppositeDoorName = "TopDoor"; playerSpawnOffset = new Vector3(0, -1.5f, 0); break;
            default: return;
        }

        Debug.Log($"Player entered door: {doorName}, moving towards {direction}");
        changeRoomCooldown = true;
        Invoke(nameof(EndChangeRoomCooldown), cooldownTime);

        Vector2 newLocation = Player.CurrentRoom.Location + direction;
        Room newRoomData = levelGenerator.GetRoomAt(newLocation);

        if (newRoomData != null)
        {
            if (Player.CurrentRoom?.RoomInstance != null) { Player.CurrentRoom.RoomInstance.SetActive(false); }

            if (newRoomData.RoomInstance != null)
            {
                newRoomData.RoomInstance.SetActive(true);
                bool wasAlreadyExplored = newRoomData.IsExplored;
                newRoomData.IsExplored = true;

                // Siirr� pelaaja
                if (Player.Transform != null)
                {
                    Transform entryDoor = newRoomData.RoomInstance.transform.Find("Doors")?.Find(oppositeDoorName);
                    if (entryDoor != null) Player.Transform.position = entryDoor.position + playerSpawnOffset;
                    else { Debug.LogError($"Opposite door '{oppositeDoorName}' not found!"); Player.Transform.position = newRoomData.RoomInstance.transform.position; }
                }
                else { Debug.LogError("Player.Transform is NULL!"); }

                // --- Kameran K�sittely ---
                bool follow = newRoomData.template.followPlayerCamera;
                Vector3 targetPos = GetRoomCenterTarget(newRoomData);
                Bounds bounds = GetRoomBounds(newRoomData); // <<< M��RITELL��N NYT T��LL� YLEMP�N�

                if (cameraController != null)
                { // Tarkistetaan kamera edelleen
                  // Tarkista bounds vain jos aiotaan seurata
                    if (follow && (bounds == default || bounds.size == Vector3.zero))
                    {
                        Debug.LogError($"Cannot set camera to FOLLOW mode for room {newRoomData.RoomInstance.name} because valid bounds could not be determined! Falling back to STATIC.");
                        follow = false; // Pakota staattiseen
                        targetPos = GetRoomCenterTarget(newRoomData); // Hae keskipiste uudelleen
                        bounds = default; // Nollaa bounds, koska sit� ei k�ytet� staattisessa
                    }
                    cameraController.SetCameraMode(follow, bounds, targetPos);
                }
                // --- Kameran k�sittely loppuu ---

                // P�ivit� pelin tila
                Room previousRoom = Player.CurrentRoom;
                Player.CurrentRoom = newRoomData;

                // P�ivit� UI
                levelGenerator.UpdateMinimapIcon(previousRoom);
                levelGenerator.UpdateMinimapIcon(newRoomData);
                if (!wasAlreadyExplored) { UpdateNeighborMinimapIcons(newRoomData); }

                // Aktivoi ovet
                EnableDoors(newRoomData);

                // NYT 'bounds' on edelleen olemassa t�ss� debug-lauseessa
                // Lis�t��n tarkistus, ett� bounds on validi, jotta 'Follow'-tila n�ytet��n oikein
                bool boundsAreValid = (bounds != default && bounds.size != Vector3.zero);
                Debug.Log($"Successfully changed to room at {newRoomData.Location}. Camera Mode: {(cameraController != null && follow && boundsAreValid ? "Follow" : "Static")}");

            }
            else
            {
                Debug.LogError($"New room data found at {newLocation}, but its RoomInstance is null!");
                CancelInvoke(nameof(EndChangeRoomCooldown)); EndChangeRoomCooldown();
            }
        }
        else
        {
            Debug.LogWarning($"No room data found at target location {newLocation}.");
            CancelInvoke(nameof(EndChangeRoomCooldown)); EndChangeRoomCooldown();
        }
    }

    // Hakee keskipisteen, johon staattisen kameran tulisi t�hd�t�
    Vector3 GetRoomCenterTarget(Room room)
    {
        if (room == null || room.RoomInstance == null) return Vector3.zero;

        // Yrit� ensin l�yt�� CompositeCollider2D (tarkin tilemapeille)
        CompositeCollider2D composite = room.RoomInstance.GetComponentInChildren<CompositeCollider2D>(true);
        if (composite != null) return composite.bounds.center;

        // Sitten TilemapRenderer
        TilemapRenderer tmRenderer = room.RoomInstance.GetComponentInChildren<TilemapRenderer>(true);
        if (tmRenderer != null && tmRenderer.gameObject.activeInHierarchy) return tmRenderer.bounds.center;

        // Viimeisen� RoomInstancen oma sijainti (prefabin pivot)
        Debug.LogWarning($"GetRoomCenterTarget: Could not find CompositeCollider or TilemapRenderer for {room.RoomInstance.name}. Using transform.position as center.");
        return room.RoomInstance.transform.position;
    }

    // Hakee huoneen rajat (Bounds) kameran rajausta varten
    Bounds GetRoomBounds(Room room)
    {
        if (room == null || room.RoomInstance == null)
        {
            Debug.LogError("GetRoomBounds called with null room or instance!"); return default;
        }
        // Debug.Log($"DEBUG: Attempting to get bounds for {room.RoomInstance.name}..."); // Voit poistaa t�m�n jos toimii

        // ETUSIJALLA: Yrit� l�yt�� CompositeCollider2D
        {
            // ... (null checkit) ...
            Debug.Log($"DEBUG: Getting bounds for {room.RoomInstance.name}...");

            CompositeCollider2D composite = room.RoomInstance.GetComponentInChildren<CompositeCollider2D>(true);
            if (composite != null)
            {
                Debug.Log($"DEBUG: Found CompositeCollider '{composite.name}'. Center={composite.bounds.center}, Size={composite.bounds.size}"); // T�RKE� LOGI
                return composite.bounds;
            }

            TilemapRenderer tmRenderer = room.RoomInstance.GetComponentInChildren<TilemapRenderer>(true);
            if (tmRenderer != null && tmRenderer.gameObject.activeInHierarchy)
            {
                Debug.Log($"DEBUG: Found TilemapRenderer '{tmRenderer.name}'. Center={tmRenderer.bounds.center}, Size={tmRenderer.bounds.size}"); // T�RKE� LOGI
                return tmRenderer.bounds;
            }

            // ... (fallback colliderin etsint� ja logit) ...
            Collider2D[] allColliders = room.RoomInstance.GetComponentsInChildren<Collider2D>(true);
            Collider2D fallbackCollider = allColliders.FirstOrDefault(c => c.transform.parent == null || c.transform.parent.name != "Doors");
            if (fallbackCollider != null)
            {
                Debug.LogWarning($"DEBUG: Using fallback collider '{fallbackCollider.name}'. Center={fallbackCollider.bounds.center}, Size={fallbackCollider.bounds.size}"); // T�RKE� LOGI
                return fallbackCollider.bounds;
            }


            Debug.LogError($"COULD NOT DETERMINE ANY BOUNDS for {room.RoomInstance.name}!");
            return default;
        }
    }

        // P�ivitt�� viereisten huoneiden minikarttaikonit (jos ne eiv�t ole jo tutkittuja)
        void UpdateNeighborMinimapIcons(Room centerRoom)
    {
        if (levelGenerator == null || centerRoom == null) return; // Varmistus

        Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        foreach (Vector2 dir in directions)
        {
            Room neighbor = levelGenerator.GetRoomAt(centerRoom.Location + dir);
            // P�ivit� naapurin ikoni VAIN jos se on olemassa EIK� sit� ole viel� tutkittu
            if (neighbor != null && !neighbor.IsExplored)
            {
                levelGenerator.UpdateMinimapIcon(neighbor);
            }
        }
    }

} // End of ChangeRooms class


public static class Player
{
    public static Transform Transform { get; set; }

    public static float Speed = 12f;

    public static Room CurrentRoom;

    public static CharacterController CharacterController;
}



