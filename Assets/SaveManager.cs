using UnityEngine;
using System.IO; // Tarvitaan tiedosto-operaatioihin

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Est‰‰ t‰m‰n tuhoutumisen scenen vaihdossa (jos tarpeen)
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // M‰‰rit‰ tallennuspolku. Application.persistentDataPath on turvallinen paikka eri alustoilla.
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");
        // Voit tulostaa polun n‰hd‰ksesi miss‰ se on:
        // Debug.Log("Save file path: " + saveFilePath);
    }

    public void SavePlayerData(PlayerSaveData dataToSave)
    {
        try
        {
            string json = JsonUtility.ToJson(dataToSave, true); // 'true' tekee JSONista n‰timm‰n (pretty print)
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Player data saved to: " + saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save data: " + e.Message);
        }
    }

    public PlayerSaveData LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
                Debug.Log("Player data loaded from: " + saveFilePath);
                return loadedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load data: " + e.Message);
                return null;
            }
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + saveFilePath);
            return null; // Palauta null, jos tiedostoa ei ole (tai voit palauttaa oletusarvot)
        }
    }

    public bool HasSaveData()
    {
        return File.Exists(saveFilePath);
    }
}