using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class ChangeRooms : MonoBehaviour
{
    private CameraController cameraController;
    private GenerateLevel levelGenerator;
    private bool changeRoomCooldown = false;
    private float cooldownTime;

    void Start()
    {
        levelGenerator = FindObjectOfType<GenerateLevel>();
        if (levelGenerator == null)
        {
            Debug.LogError("GenerateLevel script not found in the scene!");
            enabled = false;
            return;
        }
        if (levelGenerator.currentSettings == null)
        {
            Debug.LogError("GenerateLevel script does not have LevelGenerationSettings assigned!");
            enabled = false;
            return;
        }

        cooldownTime = levelGenerator.currentSettings.roomChangeTime;

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController script not found in the scene! Camera movement will not work.");
        }

        // Varmista että Player.Transform asetetaan jossain pelaajan omassa skriptissä!
        // Esimerkiksi pelaajan liikuskriptin Awake():ssa: Player.Transform = this.transform;
        // Tässä vain varmistus, ettei se ole null Startissa.
        if (Player.Transform == null)
        {
            // Jos tämä on pelaajan skripti, voit asettaa sen tässä:
            // Player.Transform = this.transform;
            // Mutta jos tämä on eri skripti, tämä ei toimi oikein.
            Debug.LogWarning("Player.Transform was null in ChangeRooms.Start(). Make sure it's set elsewhere.");
            // Yritetään varmuuden vuoksi:
            if (gameObject.CompareTag("Player")) // Varmista että pelaajalla on "Player"-tägi
            {
                Player.Transform = this.transform;
                Debug.Log("Set Player.Transform in ChangeRooms as fallback.");
            }
        }


        Room startRoom = levelGenerator.GetRoomAt(Vector2.zero);
        if (startRoom != null)
        {
            Player.CurrentRoom = startRoom;
            EnableDoors(Player.CurrentRoom);
            levelGenerator.UpdateAllMinimapIcons();

            if (cameraController != null && startRoom.RoomInstance != null)
            {
                cameraController.SnapToTarget(startRoom.RoomInstance.transform.position);
            }
        }
        else
        {
            Debug.LogError("Start room (0,0) not found after level generation!");
        }
    }

    void EndChangeRoomCooldown()
    {
        changeRoomCooldown = false;
    }

    void EnableDoors(Room room)
    {
        if (room == null || room.RoomInstance == null)
        {
            Debug.LogWarning($"EnableDoors called with null room or room instance.");
            return;
        }

        Transform doorsParent = room.RoomInstance.transform.Find("Doors");
        if (doorsParent == null)
        {
            Debug.LogError($"'Doors' child object not found in room instance: {room.RoomInstance.name}");
            return;
        }

        ActivateDoor(doorsParent, "LeftDoor", false);
        ActivateDoor(doorsParent, "RightDoor", false);
        ActivateDoor(doorsParent, "TopDoor", false);
        ActivateDoor(doorsParent, "BottomDoor", false);

        if (levelGenerator.GetRoomAt(room.Location + Vector2.left) != null) ActivateDoor(doorsParent, "LeftDoor", true);
        if (levelGenerator.GetRoomAt(room.Location + Vector2.right) != null) ActivateDoor(doorsParent, "RightDoor", true);
        if (levelGenerator.GetRoomAt(room.Location + Vector2.up) != null) ActivateDoor(doorsParent, "TopDoor", true);
        if (levelGenerator.GetRoomAt(room.Location + Vector2.down) != null) ActivateDoor(doorsParent, "BottomDoor", true);
    }

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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (changeRoomCooldown || levelGenerator == null || Player.CurrentRoom == null || cameraController == null)
        {
            return;
        }

        string doorName = collision.gameObject.name;
        Vector2 direction = Vector2.zero;
        string oppositeDoorName = "";
        Vector3 playerSpawnOffset = Vector3.zero;

        // --- TÄMÄ SWITCH-LAUSEKE PUUTTUI ---
        switch (doorName)
        {
            case "LeftDoor":
                direction = Vector2.left;
                oppositeDoorName = "RightDoor";
                playerSpawnOffset = new Vector3(-1.5f, 0, 0); // Säädä offset tarpeen mukaan
                break;
            case "RightDoor":
                direction = Vector2.right;
                oppositeDoorName = "LeftDoor";
                playerSpawnOffset = new Vector3(1.5f, 0, 0);
                break;
            case "TopDoor":
                direction = Vector2.up;
                oppositeDoorName = "BottomDoor";
                playerSpawnOffset = new Vector3(0, 1.5f, 0);
                break;
            case "BottomDoor":
                direction = Vector2.down;
                oppositeDoorName = "TopDoor";
                playerSpawnOffset = new Vector3(0, -1.5f, 0);
                break;
            default:
                //Debug.Log($"Collided with non-door trigger: {doorName}");
                return; // Ei ollut tunnettu ovi
        }
        // --- SWITCH LOPPUU ---

        // Tarkistus lisätty varmuudeksi, ettei jatketa jos suuntaa ei määritetty
        if (direction == Vector2.zero)
        {
            Debug.LogWarning($"Direction was zero after switch for door: {doorName}");
            return;
        }

        Debug.Log($"Player entered door: {doorName}, moving towards {direction}");

        changeRoomCooldown = true;
        Invoke(nameof(EndChangeRoomCooldown), cooldownTime);

        Vector2 newLocation = Player.CurrentRoom.Location + direction;
        Room newRoomData = levelGenerator.GetRoomAt(newLocation);

        if (newRoomData != null)
        {
            if (Player.CurrentRoom.RoomInstance != null)
            {
                Player.CurrentRoom.RoomInstance.SetActive(false);
            }

            if (newRoomData.RoomInstance != null)
            {
                newRoomData.RoomInstance.SetActive(true);

                bool wasAlreadyExplored = newRoomData.IsExplored;
                newRoomData.IsExplored = true;

                Transform entryDoor = newRoomData.RoomInstance.transform.Find("Doors")?.Find(oppositeDoorName);

                // Tarkista Player.Transform ennen käyttöä!
                if (Player.Transform == null)
                {
                    Debug.LogError("Player.Transform is NULL when trying to move player!");
                }
                else if (entryDoor != null) // Jos entryDoor löytyi JA Player.Transform EI ole null
                {
                    Player.Transform.position = entryDoor.position + playerSpawnOffset;
                }
                else // Jos entryDoor EI löytynyt, mutta Player.Transform ON olemassa
                {
                    Debug.LogError($"Could not find opposite door '{oppositeDoorName}' in new room instance. Placing player at room center.");
                    Player.Transform.position = newRoomData.RoomInstance.transform.position;
                }


                if (cameraController != null)
                {
                    cameraController.MoveToTarget(newRoomData.RoomInstance.transform.position);
                }

                Room previousRoom = Player.CurrentRoom;
                Player.CurrentRoom = newRoomData;

                levelGenerator.UpdateMinimapIcon(previousRoom);
                levelGenerator.UpdateMinimapIcon(newRoomData);
                if (!wasAlreadyExplored) { UpdateNeighborMinimapIcons(newRoomData); } // Päivitä naapurit jos tutkittiin ekaa kertaa

                EnableDoors(newRoomData);

                Debug.Log($"Successfully changed to room at {newRoomData.Location}");
            }
            else
            {
                Debug.LogError($"New room data found at {newLocation}, but its RoomInstance is null!");
                CancelInvoke(nameof(EndChangeRoomCooldown)); EndChangeRoomCooldown();
            }
        }
        else
        {
            Debug.LogWarning($"Player collided with door '{doorName}' but no room data found at target location {newLocation}.");
            CancelInvoke(nameof(EndChangeRoomCooldown)); EndChangeRoomCooldown();
        }
    } // <-- TÄMÄ AALTOSULJE PUUTTUI OnTriggerEnter2D:ltä

    // (Valinnainen) Päivittää annetun huoneen naapurien minikarttaikonit
    void UpdateNeighborMinimapIcons(Room centerRoom)
    {
        Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        foreach (Vector2 dir in directions)
        {
            Room neighbor = levelGenerator.GetRoomAt(centerRoom.Location + dir);
            if (neighbor != null)
            {
                // Päivitä vain jos naapuri ei ole vielä tutkittu? Tai aina?
                // Tässä päivitetään aina, jos halutaan Isaac-tyyli missä naapurit paljastuu
                if (!neighbor.IsExplored)
                {
                    // Voitaisiin merkitä naapuri paljastetuksi, mutta ei tutkituksi
                    // tai vain päivittää sen ikoni. Tässä päivitetään ikoni.
                    levelGenerator.UpdateMinimapIcon(neighbor);
                }
            }
        }
    }

} // <-- Tämä on luokan päättävä aaltosulje


public static class Player
{
    public static Transform Transform { get; set; }

    public static float Speed = 12f;

    public static Room CurrentRoom;

    public static CharacterController CharacterController;
}



