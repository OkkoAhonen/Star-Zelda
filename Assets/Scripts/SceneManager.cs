using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneManager : MonoBehaviour
{
    // Vaihda scene nimellä
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Vaihda scene indeksiä käyttäen
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Lataa uudelleen nykyinen scene
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Lopeta peli (toimii vain buildatussa pelissä)
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void loadCaveScene1()
    {
        SceneManager.LoadScene("Okon kokeilu Scene");
    }

    public void LoadVillageScene()
    {
        SceneManager.LoadScene("Town kokeilu");
    }
}
