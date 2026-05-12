using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // =========================
    // 🔹 BASIC LOAD
    // =========================
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is EMPTY!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // =========================
    // 🔹 SHORTCUT BUTTON
    // =========================
    public void GoToHome()
    {
        LoadScene("HomeScreen");
    }

    public void GoToZoneSelect()
    {
        LoadScene("ZoneSelect");
    }

    public void GoToModeSelect()
    {
        LoadScene("ModeSelect");
    }

    // =========================
    // 🔹 GAMEPLAY CONTROL
    // =========================
    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

    // =========================
    // 🔹 Zone Control
    // =========================
    public void PilihCililitan()
    {
        GameData.selectedZone = "Cililitan";
        SceneManager.LoadScene("ModeSelect");
    }

    public void PilihBogor()
    {
        GameData.selectedZone = "Bogor";
        SceneManager.LoadScene("ModeSelect");
    }

    public void PilihGerbangTol()
    {
        if (GameData.selectedZone == "Cililitan")
        {
            SceneManager.LoadScene("GerbangTol_Cililitan");
        }
        else if (GameData.selectedZone == "Bogor")
        {
            SceneManager.LoadScene("GerbangTol_Bogor");
        }
    }

    public void PilihLalinTol()
    {
        if (GameData.selectedZone == "Cililitan")
        {
            SceneManager.LoadScene("LalinTol_Cililitan");
        }
        else if (GameData.selectedZone == "Bogor")
        {
            SceneManager.LoadScene("LalinTol_Bogor");
        }
    }

}