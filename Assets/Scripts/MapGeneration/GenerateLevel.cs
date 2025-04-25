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

    // Nykyiset käytössä olevat asetukset tälle tasolle.
    // HUOM: Tämä on public, jotta esim. ChangeRooms voi lukea roomChangeTime.
    // Parempi vaihtoehto voisi olla julkinen property: public LevelGenerationSettings CurrentSettings => currentSettings;
    public LevelGenerationSettings currentSettings { get; private set; } // Muutettu propertyksi, jossa public get, private set

    // --- Private State ---
    private List<Room> generatedRooms = new List<Room>();
    private System.Random randomGenerator;
    private bool isGenerating = false; // Nimi muutettu selkeämmäksi
    private int currentRecursionDepthFailsafe = 0; // Nimi muutettu selkeämmäksi


    void Awake()
    {
        Debug.Log("GenerateLevel Awake: Initializing...");

        // --- LISÄTTY OSA ---
        // Hae GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found! GenerateLevel cannot determine level depth. Disabling script.");
            enabled = false; // Poista tämä komponentti käytöstä
            return;
        }

        // Hae nykyinen syvyys
        int depthIndex = GameManager.Instance.CurrentDepth - 1; // Listan indeksi = syvyys - 1

        // Tarkista onko asetuksia määritetty ja onko indeksi validi
        if (depthSettingsAssets == null || depthSettingsAssets.Count == 0)
        {
            Debug.LogError("No Depth Settings Assets assigned in GenerateLevel script Inspector! Disabling script.");
            enabled = false;
            return;
        }
        if (depthIndex < 0 || depthIndex >= depthSettingsAssets.Count)
        {
            Debug.LogError($"Invalid depth index ({depthIndex}) for current depth {GameManager.Instance.CurrentDepth}. Check depthSettingsAssets list size ({depthSettingsAssets.Count}). Falling back to index 0.");
            depthIndex = 0; // Yritä käyttää ensimmäistä asetusta hätätapauksessa
        }

        // Lataa oikea asetus-asset nykyiselle syvyydelle
        // Käytetään propertyn setteriä
        currentSettings = depthSettingsAssets[depthIndex];

        if (currentSettings == null)
        {
            Debug.LogError($"LevelGenerationSettings asset for depth {GameManager.Instance.CurrentDepth} (index {depthIndex}) is assigned as NULL in the list! Disabling script.");
            enabled = false;
            return;
        }
        Debug.Log($"Using Level Settings for Depth: {GameManager.Instance.CurrentDepth} (Asset: {currentSettings.name})");
        // --- LISÄTTY OSA LOPPUU ---


        // Varmistetaan että parentit on määritetty
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

    // MUOKATTU: Käytä 'currentSettings'
    private void InitializeRandomGenerator()
    {
        if (currentSettings.useSeed)
        {
            randomGenerator = new System.Random(currentSettings.seed);
            Debug.Log($"Initialized random generator with seed: {currentSettings.seed}");
        }
        else
        {
            // Käytetään järjestelmän oletussiementä (yleensä aikaan perustuva)
            int seed = (int)System.DateTime.Now.Ticks;
            randomGenerator = new System.Random(seed);
            Debug.Log($"Initialized random generator with time-based seed (approx): {seed}");
        }
    }

    // *** LISÄTTY Start() METODI KUTSUMAAN GENERATENEWLEVEL ***
    void Start()
    {
        // Varmistetaan vielä kerran, että kaikki on ok ennen generointia
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
        // Estä päällekkäinen generointi
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

        // *** LISÄTTY KRIITTINEN TARKISTUS ***
        // 3. Luo lähtöhuone (Tarkista ensin onko template olemassa!)
        if (currentSettings.startRoomTemplate == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Start Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'! Cannot generate level.");
            isGenerating = false;
            return; // Lopetetaan generointi tähän
        }
        if (currentSettings.startRoomTemplate.prefab == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Start Room Template ('{currentSettings.startRoomTemplate.name}') PREFAB is NOT ASSIGNED! Cannot generate level.");
            isGenerating = false;
            return; // Lopetetaan generointi tähän
        }

        Debug.Log("Creating Start Room...");
        Room startRoom = new Room(currentSettings.startRoomTemplate, Vector2.zero, 0);
        startRoom.IsExplored = true; // Lähtöhuone on aina tutkittu
        DrawMinimapIconAndInstantiateRoom(startRoom); // Tämä lisää myös generatedRooms-listaan
        Debug.Log($"Start Room added. Generated rooms count: {generatedRooms.Count}");

        // 4. Aloita rekursiivinen generointi lähtöhuoneesta
        Debug.Log("Starting recursive generation from Start Room...");
        GenerateRecursive(startRoom);
        Debug.Log("Recursive generation finished.");

        // 5. Tarkista minimihuonemäärä (Generointi Takuu - Osa 1)
        Debug.Log("Ensuring minimum room count...");
        EnsureMinimumRooms();

        // *** LISÄTTY KRIITTINEN TARKISTUS ***
        // 6. Sijoita pomohuone (Tarkista template)
        if (currentSettings.bossRoomTemplate == null)
        {
            Debug.LogError($"CRITICAL FAILURE: Boss Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'! Cannot place boss room.");
            // Vaikka tämä on kriittinen, ei välttämättä lopeteta koko generointia, mutta varoitetaan isosti.
            // Voit päättää haluatko palauttaa false tai yrittää jatkaa ilman bossia.
            HandleGenerationFailure("Boss Room (Template Missing)", attempt); // Käsitellään virheenä
            isGenerating = false; // Merkitään generointi valmiiksi (vaikkakin epäonnistuneesti)
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
            return; // Lopetetaan tämän yrityksen generointi
        }
        Debug.Log("Boss Room placed successfully.");

        // *** LISÄTTY KRIITTINEN TARKISTUS ***
        // 7. Sijoita aarrehuone (Tarkista template)
        if (currentSettings.treasureRoomTemplate == null)
        {
            Debug.LogWarning($"Treasure Room Template is NOT ASSIGNED in LevelGenerationSettings asset '{currentSettings.name}'. Skipping treasure room placement.");
            // Tämä ei ole yhtä kriittinen kuin boss, joten voidaan ehkä jatkaa varoituksella.
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
                // Ei välttämättä lopeteta generointia, jos aarrehuone epäonnistuu
            }
            else
            {
                Debug.Log("Treasure Room placed successfully.");
            }
        }


        // 8. Aktivoi lähtöhuoneen GameObject
        if (startRoom.RoomInstance != null)
        {
            startRoom.RoomInstance.SetActive(true);
            Debug.Log("Activating Start Room instance.");
            // HUOM: Player.CurrentRoom ja kameran siirto pitäisi tehdä ChangeRooms.Start():ssa
            // tai erillisessä LevelManagerissa, ei tässä.
        }
        else
        {
            // Tämän ei pitäisi tapahtua, jos prefab-tarkistus meni läpi
            Debug.LogError("Start room instance was null after generation, even though template prefab seemed okay!");
        }

        Debug.Log($"--- Level Generation Successful (Attempt: {attempt}) ---");
        Debug.Log($"Total rooms generated: {generatedRooms.Count}");
        PrintRoomList(); // Tulostetaan huonelista debuggausta varten
        isGenerating = false; // Merkitään generointi valmiiksi
    }

    private void HandleGenerationFailure(string reason, int currentAttempt)
    {
        Debug.LogError($"Generation Failure (Attempt {currentAttempt}): {reason}"); // Muutettu LogErroriksi
        if (currentAttempt < currentSettings.maxRegenerationAttempts)
        {
            Debug.LogWarning($"Retrying generation (Attempt {currentAttempt + 1} of {currentSettings.maxRegenerationAttempts})...");
            // Resetoidaan tila ennen uutta yritystä (ClearLevel kutsutaan GenerateNewLevelin alussa)
            generatedRooms.Clear(); // Varmuuden vuoksi tyhjennetään lista täälläkin
            // Aloita uusi generointi (älä tee tätä silmukassa, vaan anna kutsun palata ja Startin hoitaa se?)
            // Itse asiassa, rekursiivinen kutsu tässä on ok, koska ClearLevel siivoaa edellisen yrityksen.
            GenerateNewLevel(currentAttempt + 1); // Yritä uudelleen
        }
        else
        {
            Debug.LogError($"Failed to generate level after {currentSettings.maxRegenerationAttempts} attempts. Reason: {reason}. Stopping generation for this level.");
            // Tässä kohtaa pitäisi ehkä näyttää virheilmoitus pelaajalle tai ladata päävalikko.
            isGenerating = false; // Merkitään generointi epäonnistuneeksi ja lopetetuksi
        }
    }


    // Rekursiivinen pääfunktio huoneiden generointiin
    private void GenerateRecursive(Room parentRoom)
    {
        // Failsafe: Estä liian syvä rekursio
        if (parentRoom.recursionDepth >= currentSettings.maxRecursionDepth)
        {
            return;
        }

        // Päivitä globaali failsafe-syvyys (debuggausta varten)
        if (parentRoom.recursionDepth > currentRecursionDepthFailsafe)
        {
            currentRecursionDepthFailsafe = parentRoom.recursionDepth;
        }

        List<Vector2> directions = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        ShuffleList(directions);

        foreach (Vector2 direction in directions)
        {
            // Todennäköisyys jatkaa haaraa
            if (randomGenerator.NextDouble() < currentSettings.branchingChance)
            {
                Vector2 newLocation = parentRoom.Location + direction;

                // Tarkista rajat
                if (Mathf.Abs(newLocation.x) <= currentSettings.roomLimit && Mathf.Abs(newLocation.y) <= currentSettings.roomLimit)
                {
                    // Tarkista olemassaolo
                    if (!CheckIfRoomExists(newLocation))
                    {
                        // Tarkista "Isaac-sääntö" (ei 2x2)
                        if (!CheckIfCausesCrowding(newLocation, direction))
                        {
                            RoomTemplate newTemplate = GetRandomNormalRoomTemplate();
                            if (newTemplate != null)
                            {
                                if (newTemplate.prefab == null)
                                { // *** LISÄTTY TARKISTUS ***
                                    Debug.LogWarning($"Skipping room at {newLocation} because Normal Room Template ('{newTemplate.name}') PREFAB is NOT ASSIGNED.");
                                    continue; // Siirry seuraavaan suuntaan
                                }
                                Room newRoom = new Room(newTemplate, newLocation, parentRoom.recursionDepth + 1);
                                DrawMinimapIconAndInstantiateRoom(newRoom);
                                GenerateRecursive(newRoom); // Jatka tästä uudesta huoneesta
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

    // Generointi Takuu - Osa 1: Varmistaa minimihuonemäärän
    private void EnsureMinimumRooms()
    {
        // Laske nykyinen normaalihuoneiden määrä
        int currentNormalRooms = generatedRooms.Count(r => r.template.type == RoomType.Normal);
        if (currentNormalRooms >= currentSettings.minRooms)
        {
            Debug.Log($"Minimum room count ({currentSettings.minRooms}) already met ({currentNormalRooms} normal rooms).");
            return; // Ei tarvitse tehdä mitään
        }

        Debug.Log($"Current normal rooms ({currentNormalRooms}) is less than minimum ({currentSettings.minRooms}). Attempting to add more...");

        int attempts = 0;
        // Käytetään turvallisempaa maksimiyritysmäärää
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
                break; // Ei voi laajentaa enempää
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
                        { // *** LISÄTTY TARKISTUS ***
                            Debug.LogWarning($"EnsureMinimumRooms: Skipping room at {newLocation} because Normal Room Template ('{newTemplate.name}') PREFAB is NOT ASSIGNED.");
                            continue;
                        }
                        Room newRoom = new Room(newTemplate, newLocation, expandFrom.recursionDepth + 1);
                        DrawMinimapIconAndInstantiateRoom(newRoom);
                        Debug.Log($"EnsureMinimumRooms: Added extra room at {newLocation} (Attempt {attempts}). New count: {generatedRooms.Count(r => r.template.type == RoomType.Normal)}");
                        currentNormalRooms++; // Päivitä laskuri
                        break; // Lisättiin yksi tällä kierroksella, poistu direction-loopista
                    }
                }
            }
            // Jos ei onnistuttu lisäämään tästä vanhemmasta, while-loop yrittää seuraavaa (tai samaa uudelleen sekoituksen jälkeen)
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
        // Template ja prefab tarkistukset tehdään jo GenerateNewLevel-metodissa ennen tämän kutsumista,
        // mutta varmistetaan vielä prefab tässäkin, jos tätä kutsuttaisiin muualta.
        if (specialTemplate == null || specialTemplate.prefab == null)
        {
            Debug.LogError($"GenerateSpecialRoomPlacement called with null template or null prefab for type {specialTemplate?.type.ToString() ?? "Unknown"}!");
            return false;
        }

        Debug.Log($"Attempting to place Special Room: {specialTemplate.type}");

        List<Room> potentialParents = new List<Room>();
        Vector2 placementLocation = Vector2.zero;
        bool foundSpot = false;

        // --- Määritä Potentiaaliset Vanhemmat ---
        if (specialTemplate.type == RoomType.Boss)
        {
            // Pomohuone: Etsi kauimmainen huone lähtöhuoneesta.
            // Otetaan huomioon KAIKKI generoidut huoneet (paitsi ehkä aarre/muut erikoishuoneet, jos niin halutaan).
            float maxDist = -1f;
            List<Room> farthestRooms = new List<Room>(); // Kerätään kaikki yhtä kaukana olevat

            foreach (Room r in generatedRooms.Where(room => room.template.type != RoomType.Treasure)) // Rajataan pois esim. aarrehuoneet
            {
                float dist = Vector2.Distance(r.Location, Vector2.zero); // Käytetään etäisyyttä origosta
                // Tai Manhattan etäisyys: float dist = Mathf.Abs(r.Location.x) + Mathf.Abs(r.Location.y);

                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthestRooms.Clear(); // Löytyi uusi kauimmainen
                    farthestRooms.Add(r);
                }
                else if (dist == maxDist)
                {
                    farthestRooms.Add(r); // Lisää yhtä kaukana oleva listaan
                }
            }

            if (farthestRooms.Count > 0)
            {
                ShuffleList(farthestRooms); // Arvotaan yksi yhtä kaukana olevista
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
                // Fallback: Jos ei dead endejä, yritä mistä tahansa normaalista huoneesta, jolla on vapaa naapuri.
                // Vältetään start-huonetta, jos mahdollista.
                potentialParents = generatedRooms
                                   .Where(r => r.template.type == RoomType.Normal && CountNeighbors(r.Location) < 4)
                                   .ToList();
                Debug.Log($"No dead ends found. Found {potentialParents.Count} potential non-full normal parent rooms for {specialTemplate.type}.");

                if (potentialParents.Count == 0)
                {
                    // Vielä fallback: Yritä start-huoneesta jos silläkään on tilaa
                    Room start = GetRoomAt(Vector2.zero);
                    if (start != null && CountNeighbors(start.Location) < 4)
                    {
                        potentialParents.Add(start);
                        Debug.LogWarning($"Using Start room as fallback parent for {specialTemplate.type}.");
                    }
                }
            }
        }

        // Tarkistetaan onko yhtään potentiaalista vanhempaa löytynyt
        if (potentialParents.Count == 0)
        {
            Debug.LogError($"Could not find ANY suitable parent rooms to attach Special Room: {specialTemplate.type}.");
            return false; // Ei voida sijoittaa
        }

        // --- Yritä Sijoittaa Huone ---
        ShuffleList(potentialParents); // Sekoita järjestys

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
                        // Tarkista ISAAC-sääntö: Erikoishuoneella saa olla VAIN YKSI naapuri (se vanhempi).
                        // Eli lasketaan montako naapuria TULEVALLA sijainnilla olisi.
                        if (CountNeighbors(newLocation) == 0) // Tämän pitää olla 0, koska parent tulee ainoaksi naapuriksi
                        {
                            placementLocation = newLocation;
                            foundSpot = true;
                            Debug.Log($"Found valid placement for {specialTemplate.type} at {placementLocation} adjacent to parent at {parent.Location}.");
                            break; // Löytyi paikka tälle vanhemmalle
                        }
                    }
                }
            }
            if (foundSpot) break; // Löytyi paikka, lopeta vanhempien läpikäynti
        }

        // --- Jos Normaali Sijoitus Epäonnistui (Fallback) ---
        if (!foundSpot)
        {
            Debug.LogWarning($"Could not find placement for {specialTemplate.type} using standard rules (only 1 neighbor allowed). Attempting fallback placement (any free adjacent spot)...");

            // Rentoutetaan sääntöä: Etsi MIKÄ TAHANSA vapaa paikka MIKÄ TAHANSA generoidun huoneen vierestä (pois lukien muut erikoishuoneet?).
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
                        // Rento sääntö: Kunhan paikka on vapaa ja rajojen sisällä, sijoita tähän.
                        placementLocation = newLocation;
                        foundSpot = true;
                        Debug.LogWarning($"FALLBACK PLACEMENT used for {specialTemplate.type} at {placementLocation} adjacent to {parent.Location}. Rules relaxed (multiple neighbors possible).");
                        break;
                    }
                }
                if (foundSpot) break;
            }
        }


        // --- Luo Huone Jos Paikka Löytyi ---
        if (foundSpot)
        {
            Room specialRoom = new Room(specialTemplate, placementLocation, 0); // Recursion depth 0 tai muu merkintä
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
        // Käytä FirstOrDefault nopeampaan hakuun, jos lista on suuri,
        // mutta Exists on selkeämpi pienillä listoilla. Pidetään Exists.
        return generatedRooms.Exists(room => room.Location == location);
    }

    // Tarkistaa, aiheuttaisiko huoneen lisääminen annettuun paikkaan
    // kielletyn 2x2-ryppään muodostumisen, KUN ollaan tulossa suunnasta 'directionFromParent'.
    private bool CheckIfCausesCrowding(Vector2 location, Vector2 directionFromParent)
    {
        // Isaac-sääntö: Uudella huoneella saa olla vain yksi naapuri (se mistä tultiin),
        // kun se lisätään. Eli muut kolme suuntaa pitää olla tyhjiä.
        int neighborCountExcludingParent = 0;
        Vector2[] checkDirections = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        foreach (Vector2 checkDir in checkDirections)
        {
            // Älä laske sitä suuntaa, josta vanhempi tulee
            if (checkDir == -directionFromParent) continue;

            if (CheckIfRoomExists(location + checkDir))
            {
                neighborCountExcludingParent++;
            }
        }
        // Jos on YKSIKIN muu naapuri kuin vanhempi, se rikkoo sääntöä.
        return neighborCountExcludingParent > 0;
    }

    // Laskee olemassa olevien naapurien määrän annetulle sijainnille.
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
        // Tarkistetaan prefab vielä kerran (vaikka kutsuvissa metodeissa on tarkistuksia)
        if (room == null || room.template == null || room.template.prefab == null)
        {
            Debug.LogError($"DrawMinimapIconAndInstantiateRoom called with invalid room or template (Prefab is null for template: {room?.template?.name ?? "Unknown"})");
            return;
        }

        // --- Instansioi Huone Prefab ---
        try
        {
            // Laske maailman positio (voit säätää kerrointa tarpeen mukaan)
            float roomSpacing = 20f; // Esimerkki huoneiden välistä
            Vector3 worldPosition = new Vector3(room.Location.x * roomSpacing, room.Location.y * roomSpacing, 0);

            room.RoomInstance = Instantiate(room.template.prefab, worldPosition, Quaternion.identity, generatedRoomsParent);
            room.RoomInstance.name = $"{room.template.type} Room ({room.Location.x},{room.Location.y})";
            // Deaktivoi huone oletuksena (paitsi start room aktivoidaan GenerateNewLevelin lopussa)
            room.RoomInstance.SetActive(false);
            // Tässä kohtaa voisi myös välittää huoneelle sen datan (Room-olio), jos tarpeen
            // RoomController controller = room.RoomInstance.GetComponent<RoomController>();
            // if (controller != null) controller.Initialize(room);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error instantiating room prefab for template '{room.template.name}' at {room.Location}: {e.Message}");
            // Älä jatka ikonin luontiin, jos instansiointi epäonnistui
            return;
        }


        // --- Luo Minikarttaikoni ---
        try
        {
            GameObject minimapTile = new GameObject($"Minimap_{room.template.type}_{room.Location.x}_{room.Location.y}");
            minimapTile.transform.SetParent(minimapIconParent, false);

            Image image = minimapTile.AddComponent<Image>();
            room.RoomImage = image;

            // Päivitä ikoni heti oikeaksi
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
            // Varmistetaan että käytetään Vector2:ta anchoredPositioniin
            rectTransform.anchoredPosition = new Vector2(room.Location.x * paddedSize, room.Location.y * paddedSize);

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating minimap icon for room at {room.Location}: {e.Message}");
            // Huoneen instanssi on jo luotu, mutta ikoni puuttuu
        }

        // Lisää generoitu huone listaan vasta kun kaikki onnistui
        generatedRooms.Add(room);
        // Debug.Log($"Added room {room.template.type} at {room.Location}. Total rooms: {generatedRooms.Count}");
    }

    // Päivittää yksittäisen huoneen minikarttaikonin sen tilan mukaan
    public void UpdateMinimapIcon(Room room)
    {
        if (room == null || room.RoomImage == null || currentSettings == null) return; // Lisätty currentSettings tarkistus

        Sprite iconToShow = null;
        Color iconColor = Color.white;
        bool isCurrent = (Player.CurrentRoom == room); // Käytä Player.CurrentRoomia jos se on asetettu oikein

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
                // Lisää caseja muille erikoistyypeille tarvittaessa (Shop, Secret, jne.)
                // default: // Start ja Normal tutkittuna
                case RoomType.Start: // Näytä normaali ikoni startille kun se ei ole current
                case RoomType.Normal:
                    iconToShow = currentSettings.normalRoomIcon; break;
                default: // Tuntematon tyyppi? Näytä normaali.
                    iconToShow = currentSettings.normalRoomIcon; break;
            }
        }
        else // Huone on olemassa, mutta sitä ei ole tutkittu
        {
            // Haluatko näyttää tutkimattomille aina saman ikonin vai paljastaa erikoishuoneet?
            // Binding of Isaac ei yleensä paljasta erikoishuoneita ennen kuin niissä käy.
            iconToShow = currentSettings.unexploredRoomIcon;
            // Voit himmentää tutkimattomia:
            // iconColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }

        // Varmista että sprite on olemassa ennen asetusta
        if (iconToShow != null)
        {
            room.RoomImage.sprite = iconToShow;
            room.RoomImage.color = iconColor;
        }
        else
        {
            // Jos sopivaa ikonia ei löytynyt asetuksista, piilota tai näytä placeholder?
            Debug.LogWarning($"Minimap icon sprite is NULL for room type {room.template.type}, explored state: {room.IsExplored}, isCurrent: {isCurrent}. Check LevelGenerationSettings ('{currentSettings.name}').");
            // Voit piilottaa ikonin tai asettaa jonkin oletusikoni/värin
            room.RoomImage.enabled = false; // Piilotetaan jos ikoni puuttuu
        }
        if (room.RoomImage.sprite != null) room.RoomImage.enabled = true; // Varmista että näkyy jos sprite löytyi
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
        // Poista vanhat huone-instanssit scenestä generatedRoomsParentin alta
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


        // Tyhjennä huonelista
        generatedRooms.Clear();

        // Nollaa Playerin tila? (Tehdään mieluiten Player-skriptissä tai GameManagerissa)
        // Player.CurrentRoom = null; // ÄLÄ TEE TÄTÄ TÄÄLLÄ, ChangeRooms hoitaa Player.CurrentRoomin asetuksen
    }

    // Kutsu tämä metodi esim. napista tai debug-komennolla
    public void Regenerate()
    {
        if (isGenerating)
        {
            Debug.LogWarning("Cannot Regenerate: Generation already in progress.");
            return;
        }
        Debug.Log("Regenerate called. Re-initializing random generator and starting new level generation...");
        // Varmista että oikeat asetukset ladataan uudelleen, jos syvyys on voinut muuttua
        // Tässä vaiheessa oletetaan, että Awake on jo ajettu ja currentSettings on oikea
        // Mutta jos haluat vaihtaa siementä lennosta, Initialize täytyy kutsua uudelleen:
        if (currentSettings != null)
        {
            InitializeRandomGenerator(); // Varmista että uusi siemen otetaan käyttöön JOS asetusta muutettu lennosta
            GenerateNewLevel();
        }
        else
        {
            Debug.LogError("Cannot Regenerate: currentSettings is null. Check for errors during Awake.");
        }

    }

    // Debug-metodi huonelistan tulostamiseen
    [ContextMenu("Print Generated Room List")] // Voit kutsua tämän Inspectorista
    private void PrintRoomList()
    {
        if (generatedRooms == null || generatedRooms.Count == 0)
        {
            Debug.Log("Room List is empty.");
            return;
        }
        string log = $"--- Generated Room List ({generatedRooms.Count} rooms) ---\n";
        // Järjestetään sijainnin mukaan selkeyden vuoksi
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
        // Käytetään FirstOrDefault, joka palauttaa null jos huonetta ei löydy.
        return generatedRooms.FirstOrDefault(r => r.Location == location);
    }
}