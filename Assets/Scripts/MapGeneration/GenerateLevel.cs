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

    // Nykyiset k�yt�ss� olevat asetukset t�lle tasolle.
    // HUOM: T�m� on public, jotta esim. ChangeRooms voi lukea roomChangeTime.
    // Parempi vaihtoehto voisi olla julkinen property: public LevelGenerationSettings CurrentSettings => currentSettings;
    public LevelGenerationSettings currentSettings { get; private set; } // Muutettu propertyksi, jossa public get, private set

    // --- Private State ---
    private List<Room> generatedRooms = new List<Room>();
    private System.Random randomGenerator;
    private bool isGenerating = false; // Nimi muutettu selke�mm�ksi
    private int currentRecursionDepthFailsafe = 0; // Nimi muutettu selke�mm�ksi


    void Awake()
    {
        Debug.Log("GenerateLevel Awake: Initializing...");

        // --- LIS�TTY OSA ---
        // Hae GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found! GenerateLevel cannot determine level depth. Disabling script.");
            enabled = false; // Poista t�m� komponentti k�yt�st�
            return;
        }

        // Hae nykyinen syvyys
        int depthIndex = GameManager.Instance.CurrentDepth - 1; // Listan indeksi = syvyys - 1

        // Tarkista onko asetuksia m��ritetty ja onko indeksi validi
        if (depthSettingsAssets == null || depthSettingsAssets.Count == 0)
        {
            Debug.LogError("No Depth Settings Assets assigned in GenerateLevel script Inspector! Disabling script.");
            enabled = false;
            return;
        }
        if (depthIndex < 0 || depthIndex >= depthSettingsAssets.Count)
        {
            Debug.LogError($"Invalid depth index ({depthIndex}) for current depth {GameManager.Instance.CurrentDepth}. Check depthSettingsAssets list size ({depthSettingsAssets.Count}). Falling back to index 0.");
            depthIndex = 0; // Yrit� k�ytt�� ensimm�ist� asetusta h�t�tapauksessa
        }

        // Lataa oikea asetus-asset nykyiselle syvyydelle
        // K�ytet��n propertyn setteri�
        currentSettings = depthSettingsAssets[depthIndex];

        if (currentSettings == null)
        {
            Debug.LogError($"LevelGenerationSettings asset for depth {GameManager.Instance.CurrentDepth} (index {depthIndex}) is assigned as NULL in the list! Disabling script.");
            enabled = false;
            return;
        }
        Debug.Log($"Using Level Settings for Depth: {GameManager.Instance.CurrentDepth} (Asset: {currentSettings.name})");
        // --- LIS�TTY OSA LOPPUU ---


        // Varmistetaan ett� parentit on m��ritetty
        if (minimapIconParent == null)
        {
            Debug.LogError("Minimap Icon Parent is not assigned in GenerateLevel script Inspector! Disabling script.");
            enabled = false;
            return;
        }
        if (generatedRoomsParent == null)
        {
            Debug.LogError("Generated Rooms Parent is not assigned in GenerateLevel script Inspector! Disabling script.");
            enabled = false;
            return;
        }

        // Alustetaan satunnaislukugeneraattori VAIN Awake-vaiheessa
        InitializeRandomGenerator();
        Debug.Log("GenerateLevel Awake: Initialization complete.");
    }

    // MUOKATTU: K�yt� 'currentSettings'
    private void InitializeRandomGenerator()
    {
        if (currentSettings.useSeed)
        {
            randomGenerator = new System.Random(currentSettings.seed);
            Debug.Log($"Initialized random generator with seed: {currentSettings.seed}");
        }
        else
        {
            // K�ytet��n j�rjestelm�n oletussiement� (yleens� aikaan perustuva)
            int seed = (int)System.DateTime.Now.Ticks;
            randomGenerator = new System.Random(seed);
            Debug.Log($"Initialized random generator with time-based seed (approx): {seed}");
        }
    }

    // *** LIS�TTY Start() METODI KUTSUMAAN GENERATENEWLEVEL ***
    void Start()
    {
        // Varmistetaan viel� kerran, ett� kaikki on ok ennen generointia
        if (currentSettings != null && !isGenerating)
        {
            Debug.Log("GenerateLevel Start: Calling GenerateNewLevel...");
            GenerateNewLevel();
        }
        else if (currentSettings == null)
        {
            Debug.LogError("GenerateLevel Start: Cannot generate level because currentSettings is null (check Awake errors).");
        }
        else if (isGenerating)
        {
            Debug.LogWarning("GenerateLevel Start: Generation seems to be already in progress?");
        }
    }


    private void GenerateNewLevel(int attempt = 1)
    {
        // Est� p��llekk�inen generointi
        if (isGenerating)
        {
            Debug.LogWarning("GenerateNewLevel called while already generating. Aborting new call.");
            return;
        }
        isGenerating = true;

        Debug.Log($"--- Starting Level Generation (Attempt: {attempt}) ---");

        // 1. Siivoa edellinen taso (jos on)
        ClearLevel();

        // 2. Nollaa failsafe-laskuri
        currentRecursionDepthFailsafe = 0;

        // *** LIS�TTY KRIITTINEN TARKISTUS ***
        // 3. Luo l�ht�huone (Tarkista ensin onko template olemassa!)
        if (currentSettings.startRoomTemplate == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Start Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'! Cannot generate level.");
            isGenerating = false;
            return; // Lopetetaan generointi t�h�n
        }
        if (currentSettings.startRoomTemplate.prefab == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Start Room Template ('{currentSettings.startRoomTemplate.name}') PREFAB is NOT ASSIGNED! Cannot generate level.");
            isGenerating = false;
            return; // Lopetetaan generointi t�h�n
        }

        Debug.Log("Creating Start Room...");
        Room startRoom = new Room(currentSettings.startRoomTemplate, Vector2.zero, 0);
        startRoom.IsExplored = true; // L�ht�huone on aina tutkittu
        DrawMinimapIconAndInstantiateRoom(startRoom); // T�m� lis�� my�s generatedRooms-listaan
        Debug.Log($"Start Room added. Generated rooms count: {generatedRooms.Count}");

        // 4. Aloita rekursiivinen generointi l�ht�huoneesta
        Debug.Log("Starting recursive generation from Start Room...");
        GenerateRecursive(startRoom);
        Debug.Log("Recursive generation finished.");

        // 5. Tarkista minimihuonem��r� (Generointi Takuu - Osa 1)
        Debug.Log("Ensuring minimum room count...");
        EnsureMinimumRooms();

        // *** LIS�TTY KRIITTINEN TARKISTUS ***
        // 6. Sijoita pomohuone (Tarkista template)
        if (currentSettings.bossRoomTemplate == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Boss Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'! Cannot place boss room.");
            // Vaikka t�m� on kriittinen, ei v�ltt�m�tt� lopeteta koko generointia, mutta varoitetaan isosti.
            // Voit p��tt�� haluatko palauttaa false tai yritt�� jatkaa ilman bossia.
            HandleGenerationFailure("Boss Room (Template Missing)", attempt); // K�sitell��n virheen�
            isGenerating = false; // Merkit��n generointi valmiiksi (vaikkakin ep�onnistuneesti)
            return;
        }
        if (currentSettings.bossRoomTemplate.prefab == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Boss Room Template ('{currentSettings.bossRoomTemplate.name}') PREFAB is NOT ASSIGNED! Cannot place boss room.");
            HandleGenerationFailure("Boss Room (Prefab Missing)", attempt);
            isGenerating = false;
            return;
        }

        Debug.Log("Attempting to place Boss Room...");
        bool bossPlaced = GenerateSpecialRoomPlacement(currentSettings.bossRoomTemplate);
        if (!bossPlaced)
        {
            // HandleGenerationFailure hoitaa jo virhelokin ja mahdollisen uuden yrityksen
            Debug.LogError($"Failed to place Boss Room even after fallback attempts (or template was missing).");
            isGenerating = false;
            return; // Lopetetaan t�m�n yrityksen generointi
        }
        Debug.Log("Boss Room placed successfully.");

        // *** LIS�TTY KRIITTINEN TARKISTUS ***
        // 7. Sijoita aarrehuone (Tarkista template)
        if (currentSettings.treasureRoomTemplate == null)
        {
            Debug.LogWarning($"Treasure Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'. Skipping treasure room placement.");
            // T�m� ei ole yht� kriittinen kuin boss, joten voidaan ehk� jatkaa varoituksella.
        }
        else if (currentSettings.treasureRoomTemplate.prefab == null)
        {
            Debug.LogWarning($"Treasure Room Template ('{currentSettings.treasureRoomTemplate.name}') PREFAB is NOT ASSIGNED. Skipping treasure room placement.");
        }
        else
        {
            Debug.Log("Attempting to place Treasure Room...");
            bool treasurePlaced = GenerateSpecialRoomPlacement(currentSettings.treasureRoomTemplate);
            if (!treasurePlaced)
            {
                // HandleGenerationFailure hoitaa jo virhelokin ja mahdollisen uuden yrityksen
                Debug.LogWarning($"Failed to place Treasure Room even after fallback attempts.");
                // Ei v�ltt�m�tt� lopeteta generointia, jos aarrehuone ep�onnistuu
            }
            else
            {
                Debug.Log("Treasure Room placed successfully.");
            }
        }


        // 8. Aktivoi l�ht�huoneen GameObject
        if (startRoom.RoomInstance != null)
        {
            startRoom.RoomInstance.SetActive(true);
            Debug.Log("Activating Start Room instance.");
            // HUOM: Player.CurrentRoom ja kameran siirto pit�isi tehd� ChangeRooms.Start():ssa
            // tai erillisess� LevelManagerissa, ei t�ss�.
        }
        else
        {
            // T�m�n ei pit�isi tapahtua, jos prefab-tarkistus meni l�pi
            Debug.LogError("Start room instance was null after generation, even though template prefab seemed okay!");
        }

        Debug.Log($"--- Level Generation Successful (Attempt: {attempt}) ---");
        Debug.Log($"Total rooms generated: {generatedRooms.Count}");
        PrintRoomList(); // Tulostetaan huonelista debuggausta varten
        isGenerating = false; // Merkit��n generointi valmiiksi
    }

    private void HandleGenerationFailure(string reason, int currentAttempt)
    {
        Debug.LogError($"Generation Failure (Attempt {currentAttempt}): {reason}"); // Muutettu LogErroriksi
        if (currentAttempt < currentSettings.maxRegenerationAttempts)
        {
            Debug.LogWarning($"Retrying generation (Attempt {currentAttempt + 1} of {currentSettings.maxRegenerationAttempts})...");
            // Resetoidaan tila ennen uutta yrityst� (ClearLevel kutsutaan GenerateNewLevelin alussa)
            generatedRooms.Clear(); // Varmuuden vuoksi tyhjennet��n lista t��ll�kin
            // Aloita uusi generointi (�l� tee t�t� silmukassa, vaan anna kutsun palata ja Startin hoitaa se?)
            // Itse asiassa, rekursiivinen kutsu t�ss� on ok, koska ClearLevel siivoaa edellisen yrityksen.
            GenerateNewLevel(currentAttempt + 1); // Yrit� uudelleen
        }
        else
        {
            Debug.LogError($"Failed to generate level after {currentSettings.maxRegenerationAttempts} attempts. Reason: {reason}. Stopping generation for this level.");
            // T�ss� kohtaa pit�isi ehk� n�ytt�� virheilmoitus pelaajalle tai ladata p��valikko.
            isGenerating = false; // Merkit��n generointi ep�onnistuneeksi ja lopetetuksi
        }
    }


    // Rekursiivinen p��funktio huoneiden generointiin
    private void GenerateRecursive(Room parentRoom)
    {
        // Failsafe: Est� liian syv� rekursio
        if (parentRoom.recursionDepth >= currentSettings.maxRecursionDepth)
        {
            return;
        }

        // P�ivit� globaali failsafe-syvyys (debuggausta varten)
        if (parentRoom.recursionDepth > currentRecursionDepthFailsafe)
        {
            currentRecursionDepthFailsafe = parentRoom.recursionDepth;
        }

        List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        ShuffleList(directions);

        foreach (Vector2 direction in directions)
        {
            // Todenn�k�isyys jatkaa haaraa
            if (randomGenerator.NextDouble() < currentSettings.branchingChance)
            {
                Vector2 newLocation = parentRoom.Location + direction;

                // Tarkista rajat
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit)
                {
                    // Tarkista olemassaolo
                    if (!CheckIfRoomExists(newLocation))
                    {
                        // Tarkista "Isaac-s��nt�" (ei 2x2)
                        if (!CheckIfCausesCrowding(newLocation, direction))
                        {
                            RoomTemplate newTemplate = GetRandomNormalRoomTemplate();
                            if (newTemplate != null)
                            {
                                if (newTemplate.prefab == null)
                                { // *** LIS�TTY TARKISTUS ***
                                    Debug.LogWarning($"Skipping room at {newLocation} because Normal Room Template ('{newTemplate.name}') PREFAB is NOT ASSIGNED.");
                                    continue; // Siirry seuraavaan suuntaan
                                }
                                Room newRoom = new Room(newTemplate, newLocation, parentRoom.recursionDepth + 1);
                                DrawMinimapIconAndInstantiateRoom(newRoom);
                                GenerateRecursive(newRoom); // Jatka t�st� uudesta huoneesta
                            }
                            else
                            {
                                // Virhe logattiin jo GetRandomNormalRoomTemplate-metodissa
                            }
                        }
                    }
                }
            }
        }
    }

    // Generointi Takuu - Osa 1: Varmistaa minimihuonem��r�n
    private void EnsureMinimumRooms()
    {
        // Laske nykyinen normaalihuoneiden m��r�
        int currentNormalRooms = generatedRooms.Count(r => r.template.type == RoomType.Normal);
        if (currentNormalRooms >= currentSettings.minRooms)
        {
            Debug.Log($"Minimum room count ({currentSettings.minRooms}) already met ({currentNormalRooms} normal rooms).");
            return; // Ei tarvitse tehd� mit��n
        }

        Debug.Log($"Current normal rooms ({currentNormalRooms}) is less than minimum ({currentSettings.minRooms}). Attempting to add more...");

        int attempts = 0;
        // K�ytet��n turvallisempaa maksimiyritysm��r��
        int maxAttempts = currentSettings.minRooms * 5 + 10; // Heuristinen arvo

        while (currentNormalRooms < currentSettings.minRooms && attempts < maxAttempts)
        {
            attempts++;
            // Etsi potentiaalisia paikkoja laajentaa
            List<Room> potentialExpandParents = generatedRooms
                .Where(r => r.template.type == RoomType.Normal || r.template.type == RoomType.Start) // Laajenna normaaleista tai startista
                .Where(r => CountNeighbors(r.Location) < 4) // Joilla on tilaa
                .ToList();

            if (potentialExpandParents.Count == 0)
            {
                Debug.LogWarning($"EnsureMinimumRooms: Could not find any room to expand from (Attempt {attempts}). Stopping expansion.");
                break; // Ei voi laajentaa enemp��
            }

            ShuffleList(potentialExpandParents);
            Room expandFrom = potentialExpandParents[0]; // Valitse satunnainen laajennuskohde

            List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
            ShuffleList(directions);

            foreach (Vector2 direction in directions)
            {
                Vector2 newLocation = expandFrom.Location + direction;
                // Tarkista kaikki ehdot uudelleen
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit &&
                    Mathf.Abs(newLocation.y) <= currentSettings.roomLimit &&
                    !CheckIfRoomExists(newLocation) &&
                    !CheckIfCausesCrowding(newLocation, direction)) // Varmista ettei aiheuta crowdingia
                {
                    RoomTemplate newTemplate = GetRandomNormalRoomTemplate();
                    if (newTemplate != null)
                    {
                        if (newTemplate.prefab == null)
                        { // *** LIS�TTY TARKISTUS ***
                            Debug.LogWarning($"EnsureMinimumRooms: Skipping room at {newLocation} because Normal Room Template ('{newTemplate.name}') PREFAB is NOT ASSIGNED.");
                            continue;
                        }
                        Room newRoom = new Room(newTemplate, newLocation, expandFrom.recursionDepth + 1);
                        DrawMinimapIconAndInstantiateRoom(newRoom);
                        Debug.Log($"EnsureMinimumRooms: Added extra room at {newLocation} (Attempt {attempts}). New count: {generatedRooms.Count(r => r.template.type == RoomType.Normal)}");
                        currentNormalRooms++; // P�ivit� laskuri
                        break; // Lis�ttiin yksi t�ll� kierroksella, poistu direction-loopista
                    }
                }
            }
            // Jos ei onnistuttu lis��m��n t�st� vanhemmasta, while-loop yritt�� seuraavaa (tai samaa uudelleen sekoituksen j�lkeen)
        }

        if (currentNormalRooms < currentSettings.minRooms)
        {
            Debug.LogWarning($"EnsureMinimumRooms: Failed to generate minimum number of rooms ({currentSettings.minRooms}) after {attempts} attempts. Generated: {currentNormalRooms} normal rooms.");
        }
        else
        {
            Debug.Log($"EnsureMinimumRooms: Minimum room count ({currentSettings.minRooms}) met or exceeded ({currentNormalRooms} normal rooms).");
        }
    }


    // --- Special Room Placement ---
    private bool GenerateSpecialRoomPlacement(RoomTemplate specialTemplate)
    {
        // Template ja prefab tarkistukset tehd��n jo GenerateNewLevel-metodissa ennen t�m�n kutsumista,
        // mutta varmistetaan viel� prefab t�ss�kin, jos t�t� kutsuttaisiin muualta.
        if (specialTemplate == null || specialTemplate.prefab == null)
        {
            Debug.LogError($"GenerateSpecialRoomPlacement called with null template or null prefab for type {specialTemplate?.type.ToString() ?? "Unknown"}!");
            return false;
        }

        Debug.Log($"Attempting to place Special Room: {specialTemplate.type}");

        List<Room> potentialParents = new List<Room>();
        Vector2 placementLocation = Vector2.zero;
        bool foundSpot = false;

        // --- M��rit� Potentiaaliset Vanhemmat ---
        if (specialTemplate.type == RoomType.Boss)
        {
            // Pomohuone: Etsi kauimmainen huone l�ht�huoneesta.
            // Otetaan huomioon KAIKKI generoidut huoneet (paitsi ehk� aarre/muut erikoishuoneet, jos niin halutaan).
            float maxDist = -1f;
            List<Room> farthestRooms = new List<Room>(); // Ker�t��n kaikki yht� kaukana olevat

            foreach (Room r in generatedRooms.Where(room => room.template.type != RoomType.Treasure)) // Rajataan pois esim. aarrehuoneet
            {
                float dist = Vector2.Distance(r.Location, Vector2.zero); // K�ytet��n et�isyytt� origosta
                // Tai Manhattan et�isyys: float dist = Mathf.Abs(r.Location.x) + Mathf.Abs(r.Location.y);

                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthestRooms.Clear(); // L�ytyi uusi kauimmainen
                    farthestRooms.Add(r);
                }
                else if (dist == maxDist)
                {
                    farthestRooms.Add(r); // Lis�� yht� kaukana oleva listaan
                }
            }

            if (farthestRooms.Count > 0)
            {
                ShuffleList(farthestRooms); // Arvotaan yksi yht� kaukana olevista
                potentialParents.Add(farthestRooms[0]);
                Debug.Log($"Selected farthest room at {farthestRooms[0].Location} (Distance: {maxDist}) as potential parent for Boss.");
            }
            else
            {
                Debug.LogError("Could not find any potential parent room (farthest room) for Boss Room!");
                return false; // Kriittinen virhe
            }
        }
        else // Muut erikoishuoneet (esim. Treasure)
        {
            // Aarrehuoneelle tms.: Etsi normaalit huoneet, joilla on vain YKSI naapuri (dead ends).
            potentialParents = generatedRooms.Where(r => r.template.type == RoomType.Normal && CountNeighbors(r.Location) == 1).ToList();
            Debug.Log($"Found {potentialParents.Count} potential dead end parent rooms for {specialTemplate.type}.");

            if (potentialParents.Count == 0)
            {
                // Fallback: Jos ei dead endej�, yrit� mist� tahansa normaalista huoneesta, jolla on vapaa naapuri.
                // V�ltet��n start-huonetta, jos mahdollista.
                potentialParents = generatedRooms
                                   .Where(r => r.template.type == RoomType.Normal && CountNeighbors(r.Location) < 4)
                                   .ToList();
                Debug.Log($"No dead ends found. Found {potentialParents.Count} potential non-full normal parent rooms for {specialTemplate.type}.");

                if (potentialParents.Count == 0)
                {
                    // Viel� fallback: Yrit� start-huoneesta jos sill�k��n on tilaa
                    Room start = GetRoomAt(Vector2.zero);
                    if (start != null && CountNeighbors(start.Location) < 4)
                    {
                        potentialParents.Add(start);
                        Debug.LogWarning($"Using Start room as fallback parent for {specialTemplate.type}.");
                    }
                }
            }
        }

        // Tarkistetaan onko yht��n potentiaalista vanhempaa l�ytynyt
        if (potentialParents.Count == 0)
        {
            Debug.LogError($"Could not find ANY suitable parent rooms to attach Special Room: {specialTemplate.type}.");
            return false; // Ei voida sijoittaa
        }

        // --- Yrit� Sijoittaa Huone ---
        ShuffleList(potentialParents); // Sekoita j�rjestys

        foreach (Room parent in potentialParents)
        {
            List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
            ShuffleList(directions);

            foreach (Vector2 direction in directions)
            {
                Vector2 newLocation = parent.Location + direction;

                // Tarkista rajat
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit)
                {
                    // Tarkista olemassaolo
                    if (!CheckIfRoomExists(newLocation))
                    {
                        // Tarkista ISAAC-s��nt�: Erikoishuoneella saa olla VAIN YKSI naapuri (se vanhempi).
                        // Eli lasketaan montako naapuria TULEVALLA sijainnilla olisi.
                        if (CountNeighbors(newLocation) == 0) // T�m�n pit�� olla 0, koska parent tulee ainoaksi naapuriksi
                        {
                            placementLocation = newLocation;
                            foundSpot = true;
                            Debug.Log($"Found valid placement for {specialTemplate.type} at {placementLocation} adjacent to parent at {parent.Location}.");
                            break; // L�ytyi paikka t�lle vanhemmalle
                        }
                    }
                }
            }
            if (foundSpot) break; // L�ytyi paikka, lopeta vanhempien l�pik�ynti
        }

        // --- Jos Normaali Sijoitus Ep�onnistui (Fallback) ---
        if (!foundSpot)
        {
            Debug.LogWarning($"Could not find placement for {specialTemplate.type} using standard rules (only 1 neighbor allowed). Attempting fallback placement (any free adjacent spot)...");

            // Rentoutetaan s��nt��: Etsi MIK� TAHANSA vapaa paikka MIK� TAHANSA generoidun huoneen vierest� (pois lukien muut erikoishuoneet?).
            List<Room> fallbackParents = generatedRooms
                                        .Where(r => r.template.type == RoomType.Normal || r.template.type == RoomType.Start) // Normaali tai start
                                        .ToList();
            ShuffleList(fallbackParents);

            foreach (Room parent in fallbackParents)
            {
                List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
                ShuffleList(directions);

                foreach (Vector2 direction in directions)
                {
                    Vector2 newLocation = parent.Location + direction;
                    if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit &&
                        Mathf.Abs(newLocation.y) <= currentSettings.roomLimit &&
                        !CheckIfRoomExists(newLocation))
                    {
                        // Rento s��nt�: Kunhan paikka on vapaa ja rajojen sis�ll�, sijoita t�h�n.
                        placementLocation = newLocation;
                        foundSpot = true;
                        Debug.LogWarning($"FALLBACK PLACEMENT used for {specialTemplate.type} at {placementLocation} adjacent to {parent.Location}. Rules relaxed (multiple neighbors possible).");
                        break;
                    }
                }
                if (foundSpot) break;
            }
        }


        // --- Luo Huone Jos Paikka L�ytyi ---
        if (foundSpot)
        {
            Room specialRoom = new Room(specialTemplate, placementLocation, 0); // Recursion depth 0 tai muu merkint�
            DrawMinimapIconAndInstantiateRoom(specialRoom);
            return true;
        }
        else
        {
            Debug.LogError($"PLACEMENT FAILED for {specialTemplate.type}: Could not find any suitable spot, even with fallback methods.");
            return false; // Ei onnistunut
        }
    }

    // --- Utility Functions ---

    private bool CheckIfRoomExists(Vector2 location)
    {
        // K�yt� FirstOrDefault nopeampaan hakuun, jos lista on suuri,
        // mutta Exists on selke�mpi pienill� listoilla. Pidet��n Exists.
        return generatedRooms.Exists(room => room.Location == location);
    }

    // Tarkistaa, aiheuttaisiko huoneen lis��minen annettuun paikkaan
    // kielletyn 2x2-rypp��n muodostumisen, KUN ollaan tulossa suunnasta 'directionFromParent'.
    private bool CheckIfCausesCrowding(Vector2 location, Vector2 directionFromParent)
    {
        // Isaac-s��nt�: Uudella huoneella saa olla vain yksi naapuri (se mist� tultiin),
        // kun se lis�t��n. Eli muut kolme suuntaa pit�� olla tyhji�.
        int neighborCountExcludingParent = 0;
        Vector2[] checkDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        foreach (Vector2 checkDir in checkDirections)
        {
            // �l� laske sit� suuntaa, josta vanhempi tulee
            if (checkDir == -directionFromParent) continue;

            if (CheckIfRoomExists(location + checkDir))
            {
                neighborCountExcludingParent++;
            }
        }
        // Jos on YKSIKIN muu naapuri kuin vanhempi, se rikkoo s��nt��.
        return neighborCountExcludingParent > 0;
    }

    // Laskee olemassa olevien naapurien m��r�n annetulle sijainnille.
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
        if (currentSettings.normalRoomTemplates == null || currentSettings.normalRoomTemplates.Count == 0)
        {
            Debug.LogError("No normal room templates defined in LevelGenerationSettings! Cannot generate normal rooms.");
            return null;
        }
        // Varmista ettei listassa ole nulleja
        List<RoomTemplate> validTemplates = currentSettings.normalRoomTemplates.Where(t => t != null).ToList();
        if (validTemplates.Count == 0)
        {
            Debug.LogError("Normal room templates list is defined, but contains only NULL entries!");
            return null;
        }

        int randomIndex = randomGenerator.Next(0, validTemplates.Count);
        return validTemplates[randomIndex];
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = randomGenerator.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Luo minikarttaikonin ja instansioi huone-prefabin.
    private void DrawMinimapIconAndInstantiateRoom(Room room)
    {
        // Tarkistetaan prefab viel� kerran (vaikka kutsuvissa metodeissa on tarkistuksia)
        if (room == null || room.template == null || room.template.prefab == null)
        {
            Debug.LogError($"DrawMinimapIconAndInstantiateRoom called with invalid room or template (Prefab is null for template: {room?.template?.name ?? "Unknown"})");
            return;
        }

        // --- Instansioi Huone Prefab ---
        try
        {
            // Laske maailman positio (voit s��t�� kerrointa tarpeen mukaan)
            float roomSpacing = 20f; // Esimerkki huoneiden v�list�
            Vector3 worldPosition = new Vector3(room.Location.x * roomSpacing, room.Location.y * roomSpacing, 0);

            room.RoomInstance = Instantiate(room.template.prefab, worldPosition, Quaternion.identity, generatedRoomsParent);
            room.RoomInstance.name = $"{room.template.type} Room ({room.Location.x},{room.Location.y})";
            // Deaktivoi huone oletuksena (paitsi start room aktivoidaan GenerateNewLevelin lopussa)
            room.RoomInstance.SetActive(false);
            // T�ss� kohtaa voisi my�s v�litt�� huoneelle sen datan (Room-olio), jos tarpeen
            // RoomController controller = room.RoomInstance.GetComponent<RoomController>();
            // if (controller != null) controller.Initialize(room);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error instantiating room prefab for template '{room.template.name}' at {room.Location}: {e.Message}");
            // �l� jatka ikonin luontiin, jos instansiointi ep�onnistui
            return;
        }


        // --- Luo Minikarttaikoni ---
        try
        {
            GameObject minimapTile = new GameObject($"Minimap_{room.template.type}_{room.Location.x}_{room.Location.y}");
            minimapTile.transform.SetParent(minimapIconParent, false);

            Image image = minimapTile.AddComponent<Image>();
            room.RoomImage = image;

            // P�ivit� ikoni heti oikeaksi
            UpdateMinimapIcon(room);

            RectTransform rectTransform = image.GetComponent<RectTransform>();
            if (rectTransform == null)
            { // Varmuuden vuoksi
                rectTransform = minimapTile.AddComponent<RectTransform>();
            }

            // Laske koko ja sijainti minikartalla
            float iconSize = currentSettings.minimapIconBaseSize * currentSettings.minimapIconScale;
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);

            float paddedSize = iconSize * (1f + currentSettings.minimapPadding);
            // Varmistetaan ett� k�ytet��n Vector2:ta anchoredPositioniin
            rectTransform.anchoredPosition = new Vector2(room.Location.x * paddedSize, room.Location.y * paddedSize);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating minimap icon for room at {room.Location}: {e.Message}");
            // Huoneen instanssi on jo luotu, mutta ikoni puuttuu
        }

        // Lis�� generoitu huone listaan vasta kun kaikki onnistui
        generatedRooms.Add(room);
        // Debug.Log($"Added room {room.template.type} at {room.Location}. Total rooms: {generatedRooms.Count}");
    }

    // P�ivitt�� yksitt�isen huoneen minikarttaikonin sen tilan mukaan
    public void UpdateMinimapIcon(Room room)
    {
        if (room == null || room.RoomImage == null || currentSettings == null) return; // Lis�tty currentSettings tarkistus

        Sprite iconToShow = null;
        Color iconColor = Color.white;
        bool isCurrent = (Player.CurrentRoom == room); // K�yt� Player.CurrentRoomia jos se on asetettu oikein

        if (isCurrent)
        {
            iconToShow = currentSettings.currentRoomIcon;
        }
        else if (room.IsExplored)
        {
            switch (room.template.type)
            {
                case RoomType.Boss: iconToShow = currentSettings.bossRoomIcon; break;
                case RoomType.Treasure: iconToShow = currentSettings.treasureRoomIcon; break;
                // Lis�� caseja muille erikoistyypeille tarvittaessa (Shop, Secret, jne.)
                // default: // Start ja Normal tutkittuna
                case RoomType.Start: // N�yt� normaali ikoni startille kun se ei ole current
                case RoomType.Normal:
                    iconToShow = currentSettings.normalRoomIcon; break;
                default: // Tuntematon tyyppi? N�yt� normaali.
                    iconToShow = currentSettings.normalRoomIcon; break;
            }
        }
        else // Huone on olemassa, mutta sit� ei ole tutkittu
        {
            // Haluatko n�ytt�� tutkimattomille aina saman ikonin vai paljastaa erikoishuoneet?
            // Binding of Isaac ei yleens� paljasta erikoishuoneita ennen kuin niiss� k�y.
            iconToShow = currentSettings.unexploredRoomIcon;
            // Voit himment�� tutkimattomia:
            // iconColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }

        // Varmista ett� sprite on olemassa ennen asetusta
        if (iconToShow != null)
        {
            room.RoomImage.sprite = iconToShow;
            room.RoomImage.color = iconColor;
        }
        else
        {
            // Jos sopivaa ikonia ei l�ytynyt asetuksista, piilota tai n�yt� placeholder?
            Debug.LogWarning($"Minimap icon sprite is NULL for room type {room.template.type}, explored state: {room.IsExplored}, isCurrent: {isCurrent}. Check LevelGenerationSettings ('{currentSettings.name}').");
            // Voit piilottaa ikonin tai asettaa jonkin oletusikoni/v�rin
            room.RoomImage.enabled = false; // Piilotetaan jos ikoni puuttuu
        }
        if (room.RoomImage.sprite != null) room.RoomImage.enabled = true; // Varmista ett� n�kyy jos sprite l�ytyi
    }


    public void UpdateAllMinimapIcons()
    {
        // Debug.Log("Updating all minimap icons...");
        foreach (Room room in generatedRooms)
        {
            UpdateMinimapIcon(room);
        }
    }

    private void ClearLevel()
    {
        Debug.Log("Clearing previous level data and instances...");
        // Poista vanhat huone-instanssit scenest� generatedRoomsParentin alta
        if (generatedRoomsParent != null)
        {
            for (int i = generatedRoomsParent.childCount - 1; i >= 0; i--)
            {
                // Tarkista null ennen tuhoamista
                if (generatedRoomsParent.GetChild(i) != null)
                {
                    Destroy(generatedRoomsParent.GetChild(i).gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("generatedRoomsParent is null in ClearLevel, cannot clear old room instances.");
        }


        // Poista vanhat minikarttaikonit UI:sta minimapIconParentin alta
        if (minimapIconParent != null)
        {
            for (int i = minimapIconParent.childCount - 1; i >= 0; i--)
            {
                if (minimapIconParent.GetChild(i) != null)
                {
                    Destroy(minimapIconParent.GetChild(i).gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("minimapIconParent is null in ClearLevel, cannot clear old minimap icons.");
        }


        // Tyhjenn� huonelista
        generatedRooms.Clear();

        // Nollaa Playerin tila? (Tehd��n mieluiten Player-skriptiss� tai GameManagerissa)
        // Player.CurrentRoom = null; // �L� TEE T�T� T��LL�, ChangeRooms hoitaa Player.CurrentRoomin asetuksen
    }

    // Kutsu t�m� metodi esim. napista tai debug-komennolla
    public void Regenerate()
    {
        if (isGenerating)
        {
            Debug.LogWarning("Cannot Regenerate: Generation already in progress.");
            return;
        }
        Debug.Log("Regenerate called. Re-initializing random generator and starting new level generation...");
        // Varmista ett� oikeat asetukset ladataan uudelleen, jos syvyys on voinut muuttua
        // T�ss� vaiheessa oletetaan, ett� Awake on jo ajettu ja currentSettings on oikea
        // Mutta jos haluat vaihtaa siement� lennosta, Initialize t�ytyy kutsua uudelleen:
        if (currentSettings != null)
        {
            InitializeRandomGenerator(); // Varmista ett� uusi siemen otetaan k�ytt��n JOS asetusta muutettu lennosta
            GenerateNewLevel();
        }
        else
        {
            Debug.LogError("Cannot Regenerate: currentSettings is null. Check for errors during Awake.");
        }

    }

    // Debug-metodi huonelistan tulostamiseen
    [ContextMenu("Print Generated Room List")] // Voit kutsua t�m�n Inspectorista
    private void PrintRoomList()
    {
        if (generatedRooms == null || generatedRooms.Count == 0)
        {
            Debug.Log("Room List is empty.");
            return;
        }
        string log = $"--- Generated Room List ({generatedRooms.Count} rooms) ---\n";
        // J�rjestet��n sijainnin mukaan selkeyden vuoksi
        foreach (Room r in generatedRooms.OrderBy(room => room.Location.y).ThenBy(room => room.Location.x))
        {
            string instanceStatus = r.RoomInstance != null ? (r.RoomInstance.activeSelf ? "Active" : "Inactive") : "NULL";
            log += $"Loc: ({r.Location.x},{r.Location.y}) | Type: {r.template?.type.ToString() ?? "N/A"} | Explored: {r.IsExplored} | Neighbors: {CountNeighbors(r.Location)} | Instance: {instanceStatus}\n";
        }
        log += "--------------------------------------";
        Debug.Log(log);
    }

    // Julkinen metodi huoneen hakemiseen sijainnin perusteella
    public Room GetRoomAt(Vector2 location)
    {
        // K�ytet��n FirstOrDefault, joka palauttaa null jos huonetta ei l�ydy.
        return generatedRooms.FirstOrDefault(r => r.Location == location);
    }
}