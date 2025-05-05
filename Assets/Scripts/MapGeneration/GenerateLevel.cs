using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // Tarvitaan Image

public class GenerateLevel : MonoBehaviour
{
    [Header("Depth Settings Assets")]
    [Tooltip("List of settings assets, index 0 = Depth 1, index 1 = Depth 2, etc.")]
    public List<LevelGenerationSettings> depthSettingsAssets;

    [Header("Scene References")]
    [Tooltip("Parent Transform for minimap UI icons.")]
    public Transform minimapIconParent;
    [Tooltip("Parent Transform for instantiated room GameObjects in the scene.")]
    public Transform generatedRoomsParent;

    // Nykyiset asetukset t‰lle tasolle (luetaan Awaken aikana)
    public LevelGenerationSettings currentSettings { get; private set; }

    // --- Private State ---
    private List<Room> generatedRooms = new List<Room>();
    private System.Random randomGenerator;
    private bool isGenerating = false;
    private int currentRecursionDepthFailsafe = 0;

    void Awake()
    {
        Debug.Log("GenerateLevel Awake: Initializing...");
        if (!LoadSettings()) return; // Yrit‰ ladata asetukset, lopeta jos ep‰onnistuu
        if (!CheckSceneReferences()) return; // Tarkista viittaukset, lopeta jos puuttuu

        InitializeRandomGenerator(); // Alusta generaattori vain kerran
        Debug.Log("GenerateLevel Awake: Initialization complete.");
    }

    bool LoadSettings()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found! Disabling script.");
            enabled = false; return false;
        }
        int depthIndex = GameManager.Instance.CurrentDepth - 1;
        if (depthSettingsAssets == null || depthSettingsAssets.Count == 0)
        {
            Debug.LogError("No Depth Settings Assets assigned! Disabling script.");
            enabled = false; return false;
        }
        if (depthIndex < 0 || depthIndex >= depthSettingsAssets.Count)
        {
            Debug.LogError($"Invalid depth index ({depthIndex}). Using index 0.");
            depthIndex = 0;
        }
        currentSettings = depthSettingsAssets[depthIndex];
        if (currentSettings == null)
        {
            Debug.LogError($"LevelGenerationSettings asset at index {depthIndex} is NULL! Disabling script.");
            enabled = false; return false;
        }
        Debug.Log($"Using Level Settings for Depth: {GameManager.Instance.CurrentDepth} (Asset: {currentSettings.name})");

        if (currentSettings.minTotalRooms > currentSettings.maxTotalRooms)
        {
            Debug.LogWarning($"Settings '{currentSettings.name}': minTotalRooms ({currentSettings.minTotalRooms}) > maxTotalRooms ({currentSettings.maxTotalRooms}). Clamping min to max.");
            currentSettings.minTotalRooms = currentSettings.maxTotalRooms;
        }
        return true;
    }

    bool CheckSceneReferences()
    {
        if (minimapIconParent == null) { Debug.LogError("Minimap Icon Parent not assigned! Disabling script."); enabled = false; return false; }
        if (generatedRoomsParent == null) { Debug.LogError("Generated Rooms Parent not assigned! Disabling script."); enabled = false; return false; }
        return true;
    }

    private void InitializeRandomGenerator()
    {
        if (currentSettings.useSeed)
        {
            randomGenerator = new System.Random(currentSettings.seed);
            Debug.Log($"Initialized random generator with seed: {currentSettings.seed}");
        }
        else
        {
            int seed = (int)System.DateTime.Now.Ticks;
            randomGenerator = new System.Random(seed);
           // Debug.Log($"Initialized random generator with time-based seed (approx): {seed}");
        }
    }

    void Start()
    {
        if (currentSettings != null && !isGenerating)
        {
            Debug.Log("GenerateLevel Start: Calling GenerateNewLevel...");
            GenerateNewLevel();
        }
        else if (currentSettings == null)
        {
            Debug.LogError("GenerateLevel Start: Cannot generate level because currentSettings is null.");
        }
    }

    private void GenerateNewLevel(int attempt = 1)
    {
        if (isGenerating && attempt == 1)
        { // Est‰ ulkoinen kutsu jos jo generoi, salli rekursiiviset yritykset
            Debug.LogWarning("GenerateNewLevel called externally while already generating. Ignoring.");
            return;
        }
        isGenerating = true; // Merkitse generointi alkaneeksi

        Debug.Log($"--- Starting Level Generation (Attempt: {attempt}) ---");
        ClearLevel();
        currentRecursionDepthFailsafe = 0;

        // Tarkista KRIITTISET templatet heti
        if (!CheckRequiredTemplates())
        {
            Debug.LogError($"CRITICAL FAILURE: Required templates missing/invalid in '{currentSettings.name}'. Aborting generation.");
            isGenerating = false;
            // T‰ss‰ ei yleens‰ kannata yritt‰‰ regenerointia, koska asetukset on rikki.
            return;
        }

        // 1. Luo Start-huone
        Room startRoom = new Room(currentSettings.startRoomTemplate, Vector2.zero, 0);
        startRoom.IsExplored = true;
        DrawMinimapIconAndInstantiateRoom(startRoom);

        // 2. Generoi normaalit (kunnioittaa maxTotalRooms)
        if (generatedRooms.Count < currentSettings.maxTotalRooms)
        {
            GenerateRecursive(startRoom);
        }

        // 3. T‰yt‰ minimi (kunnioittaa maxTotalRooms)
        EnsureMinimumTotalRooms();

        // 4. SIJOITA BOSSHUONE (PRIORITEETTI)
        Debug.Log("Attempting to place REQUIRED Boss Room...");
        bool bossPlaced = GenerateSpecialRoomPlacement(currentSettings.bossRoomTemplate);

        // Jos bossin sijoitus ep‰onnistui TƒYSIN (jopa pakotuksen j‰lkeen), yrit‰ koko generointia uudelleen
        if (!bossPlaced)
        {
            Debug.LogError($"CRITICAL FAILURE: Boss Room placement FAILED completely (Attempt {attempt}). Retrying generation if possible...");
            HandleGenerationFailure("Boss Room Placement Completely Failed", attempt);
            // HandleGenerationFailure hoitaa joko uuden kutsun tai lopetuksen, joten palataan t‰st‰
            return;
        }
        Debug.Log($"Boss Room placed successfully. Room count: {generatedRooms.Count}");

        // 5. Sijoita aarrehuone (jos m‰‰ritelty ja tilaa - TƒMƒ KUNNIOITTAA MAXIA)
        if (currentSettings.treasureRoomTemplate != null && currentSettings.treasureRoomTemplate.prefab != null)
        {
            Debug.Log("Attempting to place Treasure Room (if space allows)...");
            bool treasurePlaced = GenerateSpecialRoomPlacement(currentSettings.treasureRoomTemplate);
            if (treasurePlaced) { Debug.Log($"Treasure Room placed successfully. Room count: {generatedRooms.Count}"); }
            // Virhe/ohitus logataan GenerateSpecialRoomPlacementissa
        }
        else { Debug.Log("Treasure Room Template/Prefab not assigned. Skipping."); }


        // 6. Aktivoi l‰htˆhuone
        if (startRoom.RoomInstance != null) { startRoom.RoomInstance.SetActive(true); }
        else { Debug.LogError("Start room instance was null after generation!"); }

        Debug.Log($"--- Level Generation Successful (Attempt: {attempt}) ---");
        Debug.Log($"Final total rooms generated: {generatedRooms.Count}");
        PrintRoomList();
        isGenerating = false; // Merkitse generointi valmiiksi
    }

    bool CheckRequiredTemplates()
    {
        bool ok = true;
        if (currentSettings.startRoomTemplate == null || currentSettings.startRoomTemplate.prefab == null)
        {
            Debug.LogError($"Start Room Template or its Prefab is NULL in '{currentSettings.name}'!"); ok = false;
        }
        if (currentSettings.bossRoomTemplate == null || currentSettings.bossRoomTemplate.prefab == null)
        {
            Debug.LogError($"Boss Room Template or its Prefab is NULL in '{currentSettings.name}'!"); ok = false;
        }
        if (currentSettings.normalRoomTemplates == null || currentSettings.normalRoomTemplates.Count == 0 || currentSettings.normalRoomTemplates.All(t => t == null))
        {
            Debug.LogError($"No valid Normal Room Templates defined in '{currentSettings.name}'!"); ok = false;
        }
        else if (currentSettings.normalRoomTemplates.Any(t => t != null && t.prefab == null))
        {
            Debug.LogError($"One or more Normal Room Templates in '{currentSettings.name}' have a NULL Prefab!"); ok = false;
        }
        return ok;
    }

    private void HandleGenerationFailure(string reason, int currentAttempt)
    {
        Debug.LogError($"Generation Failure (Attempt {currentAttempt}): {reason}");
        if (currentAttempt < currentSettings.maxRegenerationAttempts)
        {
            Debug.LogWarning($"Retrying generation (Attempt {currentAttempt + 1} of {currentSettings.maxRegenerationAttempts})...");
            // Kutsu GenerateNewLevel uudelleen, isGenerating pysyy true
            GenerateNewLevel(currentAttempt + 1);
        }
        else
        {
            Debug.LogError($"Failed to generate level after {currentSettings.maxRegenerationAttempts} attempts. Reason: {reason}. Stopping generation.");
            isGenerating = false; // Lopetetaan lopullisesti
        }
    }

    private void GenerateRecursive(Room parentRoom)
    {
        if (parentRoom.recursionDepth >= currentSettings.maxRecursionDepth) return;
        if (generatedRooms.Count >= currentSettings.maxTotalRooms) return; // Lopeta jos max t‰ynn‰

        if (parentRoom.recursionDepth > currentRecursionDepthFailsafe)
        {
            currentRecursionDepthFailsafe = parentRoom.recursionDepth;
        }

        List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        ShuffleList(directions);

        foreach (Vector2 direction in directions)
        {
            if (generatedRooms.Count >= currentSettings.maxTotalRooms) break; // Tarkista ennen joka suuntaa

            if (randomGenerator.NextDouble() < currentSettings.branchingChance)
            {
                Vector2 newLocation = parentRoom.Location + direction;
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit)
                {
                    if (!CheckIfRoomExists(newLocation))
                    {
                        if (!CheckIfCausesCrowding(newLocation, direction))
                        {
                            RoomTemplate newTemplate = GetRandomNormalRoomTemplate();
                            if (newTemplate != null && newTemplate.prefab != null)
                            {
                                Room newRoom = new Room(newTemplate, newLocation, parentRoom.recursionDepth + 1);
                                DrawMinimapIconAndInstantiateRoom(newRoom);
                                GenerateRecursive(newRoom); // Jatka rekursiota
                            }
                            else if (newTemplate != null)
                            {
                                Debug.LogWarning($"Skipping room: Normal Template '{newTemplate.name}' prefab is null.");
                            }
                        }
                    }
                }
            }
            if (generatedRooms.Count >= currentSettings.maxTotalRooms) break; // Tarkista joka suunnan j‰lkeen
        }
    }

    private void EnsureMinimumTotalRooms()
    {
        if (generatedRooms.Count >= currentSettings.minTotalRooms) return;

        Debug.Log($"Current total rooms ({generatedRooms.Count}) < minimum ({currentSettings.minTotalRooms}). Adding more...");
        int attempts = 0;
        int maxAttempts = (currentSettings.minTotalRooms - generatedRooms.Count) * 5 + 20;

        while (generatedRooms.Count < currentSettings.minTotalRooms && attempts < maxAttempts)
        {
            attempts++;
            if (generatedRooms.Count >= currentSettings.maxTotalRooms)
            { // Tarkista max raja heti alussa
                Debug.Log("EnsureMinimumTotalRooms: Reached maxTotalRooms. Stopping expansion.");
                break;
            }

            List<Room> potentialParents = generatedRooms
                .Where(r => (r.template.type == RoomType.Normal || r.template.type == RoomType.Start) && CountNeighbors(r.Location) < 4)
                .ToList();

            if (potentialParents.Count == 0) { Debug.LogWarning($"EnsureMinimumTotalRooms: No room to expand from (Attempt {attempts})."); break; }

            ShuffleList(potentialParents); Room expandFrom = potentialParents[0];
            List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down }; ShuffleList(directions);
            bool roomAdded = false;
            foreach (Vector2 direction in directions)
            {
                if (generatedRooms.Count >= currentSettings.maxTotalRooms)
                { // Tarkista max raja silmukan sis‰ll‰
                    Debug.Log("EnsureMinimumTotalRooms: Reached maxTotalRooms during direction check. Stopping expansion.");
                    goto MaxReachedInEnsureMin; // Hyp‰t‰‰n ulos
                }

                Vector2 newLocation = expandFrom.Location + direction;
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit &&
                    !CheckIfRoomExists(newLocation) && !CheckIfCausesCrowding(newLocation, direction))
                {
                    RoomTemplate newTemplate = GetRandomNormalRoomTemplate();
                    if (newTemplate != null && newTemplate.prefab != null)
                    {
                        Room newRoom = new Room(newTemplate, newLocation, expandFrom.recursionDepth + 1);
                        DrawMinimapIconAndInstantiateRoom(newRoom);
                        //Debug.Log($"EnsureMinimumTotalRooms: Added room at {newLocation}. New total: {generatedRooms.Count}");
                        roomAdded = true;
                        break; // Lis‰ttiin yksi, riitt‰‰ t‰lle attemptille
                    }
                    else if (newTemplate != null)
                    {
                        Debug.LogWarning($"EnsureMinimumTotalRooms: Skipping room: Normal Template '{newTemplate.name}' prefab is null.");
                    }
                }
            }
            // Jos ei lis‰tty, while jatkuu
        } // end while

    MaxReachedInEnsureMin:; // Goto kohde

        if (generatedRooms.Count < currentSettings.minTotalRooms)
        {
            Debug.LogWarning($"EnsureMinimumTotalRooms: Failed to reach minimum ({currentSettings.minTotalRooms}) after {attempts} attempts. Generated: {generatedRooms.Count}.");
        }
        else
        {
            //Debug.Log($"EnsureMinimumTotalRooms: Minimum met or exceeded ({generatedRooms.Count}).");
        }
    }

    private bool GenerateSpecialRoomPlacement(RoomTemplate specialTemplate)
    {
        if (specialTemplate == null || specialTemplate.prefab == null)
        {
            Debug.LogError($"GenerateSpecialRoomPlacement: Invalid template or prefab ({specialTemplate?.name ?? "NULL"})");
            return false;
        }

        // Ohita max huoneiden tarkistus VAIN bossille
        if (specialTemplate.type != RoomType.Boss && generatedRooms.Count >= currentSettings.maxTotalRooms)
        {
            Debug.Log($"Skipping placement of {specialTemplate.type}: maxTotalRooms ({currentSettings.maxTotalRooms}) reached.");
            return false;
        }
        else if (specialTemplate.type == RoomType.Boss && generatedRooms.Count >= currentSettings.maxTotalRooms)
        {
            Debug.LogWarning($"Placing REQUIRED Boss Room even though maxTotalRooms ({currentSettings.maxTotalRooms}) is reached/exceeded!");
        }

        Debug.Log($"Attempting to place Special Room: {specialTemplate.type}");

        List<Room> potentialParents = new List<Room>();
        Vector2 placementLocation = Vector2.zero;
        bool foundSpot = false;
        Room designatedBossParent = null;

        // M‰‰rit‰ potentiaaliset vanhemmat
        if (specialTemplate.type == RoomType.Boss)
        {
            float maxDist = -1f; List<Room> farthestRooms = new List<Room>();
            foreach (Room r in generatedRooms.Where(room => room.template.type != RoomType.Treasure))
            {
                float dist = Vector2.Distance(r.Location, Vector2.zero);
                if (dist > maxDist) { maxDist = dist; farthestRooms.Clear(); farthestRooms.Add(r); }
                else if (dist == maxDist) { farthestRooms.Add(r); }
            }
            if (farthestRooms.Count > 0) { ShuffleList(farthestRooms); designatedBossParent = farthestRooms[0]; potentialParents.Add(designatedBossParent); Debug.Log($"Selected farthest room: {designatedBossParent.Location} for Boss."); }
            else { Debug.LogError("Cannot place Boss: Could not find farthest room!"); return false; }
        }
        else
        {
            potentialParents = generatedRooms.Where(r => r.template.type == RoomType.Normal && CountNeighbors(r.Location) == 1).ToList();
            if (potentialParents.Count == 0)
            {
                potentialParents = generatedRooms.Where(r => r.template.type == RoomType.Normal && CountNeighbors(r.Location) < 4).ToList();
                if (potentialParents.Count == 0) { Room start = GetRoomAt(Vector2.zero); if (start != null && CountNeighbors(start.Location) < 4) potentialParents.Add(start); }
            }
            Debug.Log($"Found {potentialParents.Count} potential parents for {specialTemplate.type}.");
        }

        if (potentialParents.Count == 0 && specialTemplate.type != RoomType.Boss) { Debug.LogWarning($"Could not find parent for {specialTemplate.type}."); return false; }

        // Yrit‰ normaalia sijoitusta
        ShuffleList(potentialParents);
        foreach (Room parent in potentialParents)
        {
            List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down }; ShuffleList(directions);
            foreach (Vector2 direction in directions)
            {
                Vector2 newLocation = parent.Location + direction;
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit)
                {
                    if (!CheckIfRoomExists(newLocation))
                    {
                        if (CountNeighbors(newLocation) == 0)
                        { // Isaac Rule
                            placementLocation = newLocation; foundSpot = true; Debug.Log($"Found valid (standard) placement for {specialTemplate.type} at {placementLocation}"); break;
                        }
                    }
                }
            }
            if (foundSpot) break;
        }

        // Pakotettu sijoitus bossille, jos standardi ep‰onnistui
        if (!foundSpot && specialTemplate.type == RoomType.Boss)
        {
            Debug.LogWarning($"Standard placement failed for Boss. Attempting FORCED placement near {designatedBossParent?.Location}...");
            if (designatedBossParent != null)
            {
                List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down }; ShuffleList(directions);
                foreach (Vector2 direction in directions)
                {
                    Vector2 newLocation = designatedBossParent.Location + direction;
                    // OHITA SƒƒNN÷T, tarkista vain rajat ja tyhj‰ paikka
                    if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit && !CheckIfRoomExists(newLocation))
                    {
                        placementLocation = newLocation; foundSpot = true; Debug.LogWarning($"FORCED Boss Room placement at {placementLocation}. Rules ignored."); break;
                    }
                }
                if (!foundSpot) Debug.LogError($"FORCE PLACEMENT FAILED for Boss Room near {designatedBossParent.Location}! Very rare.");
            }
            else { Debug.LogError("Cannot force boss placement: designated parent was null!"); }
        }

        // Yleinen fallback (vain jos ei bossi JA standardi ep‰onnistui)
        if (!foundSpot && specialTemplate.type != RoomType.Boss)
        {
            Debug.LogWarning($"Standard placement failed for {specialTemplate.type}. Trying general fallback...");
            List<Room> fallbackParents = generatedRooms.Where(r => r.template.type == RoomType.Normal || r.template.type == RoomType.Start).ToList(); ShuffleList(fallbackParents);
            foreach (Room parent in fallbackParents)
            {
                List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down }; ShuffleList(directions);
                foreach (Vector2 direction in directions)
                {
                    Vector2 newLocation = parent.Location + direction;
                    if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit && !CheckIfRoomExists(newLocation))
                    {
                        placementLocation = newLocation; foundSpot = true; Debug.LogWarning($"GENERAL FALLBACK used for {specialTemplate.type} at {placementLocation}"); break;
                    }
                }
                if (foundSpot) break;
            }
        }

        // Luo huone, jos paikka lˆytyi
        if (foundSpot)
        {
            // Ei en‰‰ max checki‰ bossille t‰ss‰
            Room specialRoom = new Room(specialTemplate, placementLocation, 0);
            DrawMinimapIconAndInstantiateRoom(specialRoom); // Lis‰‰ listaan
            return true;
        }
        else
        {
            // T‰m‰n pit‰isi tapahtua vain jos bossin pakotuskin ep‰onnistui tai muun fallback ep‰onnistui
            Debug.LogError($"PLACEMENT FAILED COMPLETELY for {specialTemplate.type}.");
            return false;
        }
    }

    // --- Utility Functions ---
    private bool CheckIfRoomExists(Vector2 location) { return generatedRooms.Exists(room => room.Location == location); }

    private bool CheckIfCausesCrowding(Vector2 location, Vector2 directionFromParent)
    {
        int neighborCountExcludingParent = 0;
        Vector2[] checkDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        foreach (Vector2 checkDir in checkDirections)
        {
            if (checkDir == -directionFromParent) continue;
            if (CheckIfRoomExists(location + checkDir)) { neighborCountExcludingParent++; }
        }
        return neighborCountExcludingParent > 0;
    }

    private int CountNeighbors(Vector2 location)
    {
        int count = 0;
        if (CheckIfRoomExists(location + Vector2.left)) count++;
        if (CheckIfRoomExists(location + Vector2.right)) count++;
        if (CheckIfRoomExists(location + Vector2.up)) count++;
        if (CheckIfRoomExists(location + Vector2.down)) count++;
        return count;
    }

    private RoomTemplate GetRandomNormalRoomTemplate()
    {
        if (currentSettings.normalRoomTemplates == null || currentSettings.normalRoomTemplates.Count == 0) { Debug.LogError("No normal room templates defined!"); return null; }
        List<RoomTemplate> validTemplates = currentSettings.normalRoomTemplates.Where(t => t != null && t.prefab != null).ToList(); // Lis‰tty prefab check
        if (validTemplates.Count == 0) { Debug.LogError("Normal room templates list has no valid entries (null or prefab missing)!"); return null; }
        int randomIndex = randomGenerator.Next(0, validTemplates.Count);
        return validTemplates[randomIndex];
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--; int k = randomGenerator.Next(n + 1);
            T value = list[k]; list[k] = list[n]; list[n] = value;
        }
    }

    private void DrawMinimapIconAndInstantiateRoom(Room room)
    {
        if (room == null || room.template == null || room.template.prefab == null) { Debug.LogError($"DrawMinimap: Invalid room/template/prefab"); return; }
        try
        { // Instansioi huone
            float roomSpacing = 20f; // Voit s‰‰t‰‰ t‰t‰
            Vector3 worldPosition = new Vector3(room.Location.x * roomSpacing, room.Location.y * roomSpacing, 0);
            room.RoomInstance = Instantiate(room.template.prefab, worldPosition, Quaternion.identity, generatedRoomsParent);
            room.RoomInstance.name = $"{room.template.type} Room ({room.Location.x},{room.Location.y})";
            room.RoomInstance.SetActive(false); // Aktivoidaan myˆhemmin tarvittaessa
        }
        catch (System.Exception e) { Debug.LogError($"Error instantiating room '{room.template.name}': {e.Message}"); return; }

        try
        { // Luo minikarttaikoni
            GameObject minimapTile = new GameObject($"Minimap_{room.template.type}_{room.Location.x}_{room.Location.y}");
            minimapTile.transform.SetParent(minimapIconParent, false);
            Image image = minimapTile.AddComponent<Image>();
            room.RoomImage = image;
            UpdateMinimapIcon(room); // Aseta oikea ikoni heti
            RectTransform rectTransform = image.GetComponent<RectTransform>() ?? minimapTile.AddComponent<RectTransform>();
            float iconSize = currentSettings.minimapIconBaseSize * currentSettings.minimapIconScale;
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            float paddedSize = iconSize * (1f + currentSettings.minimapPadding);
            rectTransform.anchoredPosition = new Vector2(room.Location.x * paddedSize, room.Location.y * paddedSize);
        }
        catch (System.Exception e) { Debug.LogError($"Error creating minimap icon for room at {room.Location}: {e.Message}"); }

        // Lis‰‰ listaan vasta lopuksi
        generatedRooms.Add(room);
    }

    public void UpdateMinimapIcon(Room room)
    {
        if (room == null || room.RoomImage == null || currentSettings == null) return;
        Sprite iconToShow = null; Color iconColor = Color.white; bool isCurrent = (Player.CurrentRoom == room); // Olettaa Player.CurrentRoom asetettu oikein
        if (isCurrent) { iconToShow = currentSettings.currentRoomIcon; }
        else if (room.IsExplored)
        {
            switch (room.template.type)
            {
                case RoomType.Boss: iconToShow = currentSettings.bossRoomIcon; break;
                case RoomType.Treasure: iconToShow = currentSettings.treasureRoomIcon; break;
                case RoomType.Start: case RoomType.Normal: default: iconToShow = currentSettings.normalRoomIcon; break;
            }
        }
        else { iconToShow = currentSettings.unexploredRoomIcon; }

        if (iconToShow != null)
        {
            room.RoomImage.sprite = iconToShow; room.RoomImage.color = iconColor; room.RoomImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Minimap icon sprite is NULL for room type {room.template.type}, state explored={room.IsExplored}, current={isCurrent}. Check settings '{currentSettings.name}'.");
            room.RoomImage.enabled = false; // Piilota jos ikonia ei ole
        }
    }

    public void UpdateAllMinimapIcons() { foreach (Room room in generatedRooms) { UpdateMinimapIcon(room); } }

    private void ClearLevel()
    {
        Debug.Log("Clearing previous level...");
        if (generatedRoomsParent != null) { for (int i = generatedRoomsParent.childCount - 1; i >= 0; i--) { if (generatedRoomsParent.GetChild(i) != null) Destroy(generatedRoomsParent.GetChild(i).gameObject); } }
        else { Debug.LogWarning("generatedRoomsParent is null in ClearLevel."); }
        if (minimapIconParent != null) { for (int i = minimapIconParent.childCount - 1; i >= 0; i--) { if (minimapIconParent.GetChild(i) != null) Destroy(minimapIconParent.GetChild(i).gameObject); } }
        else { Debug.LogWarning("minimapIconParent is null in ClearLevel."); }
        generatedRooms.Clear();
    }

    public void Regenerate()
    {
        if (isGenerating) { Debug.LogWarning("Cannot Regenerate: Generation in progress."); return; }
        Debug.Log("Regenerate called...");
        if (currentSettings != null) { InitializeRandomGenerator(); GenerateNewLevel(); }
        else { Debug.LogError("Cannot Regenerate: currentSettings is null."); }
    }

    [ContextMenu("Print Generated Room List")]
    private void PrintRoomList()
    {
        if (generatedRooms == null || generatedRooms.Count == 0) { Debug.Log("Room List empty."); return; }
        string log = $"--- Generated Room List ({generatedRooms.Count} rooms, Min: {currentSettings.minTotalRooms}, Max: {currentSettings.maxTotalRooms}) ---\n";
        foreach (Room r in generatedRooms.OrderBy(room => room.Location.y).ThenBy(room => room.Location.x))
        {
            string instanceStatus = r.RoomInstance != null ? (r.RoomInstance.activeSelf ? "Active" : "Inactive") : "NULL";
            log += $"Loc: ({r.Location.x},{r.Location.y}) | Type: {r.template?.type.ToString() ?? "N/A"} | Explored: {r.IsExplored} | Neighbors: {CountNeighbors(r.Location)} | Instance: {instanceStatus}\n";
        }
        Debug.Log(log + "--------------------------------------");
    }

    public Room GetRoomAt(Vector2 location)
    {
        return generatedRooms.FirstOrDefault(r => r.Location == location);
    }
} // End of GenerateLevel class