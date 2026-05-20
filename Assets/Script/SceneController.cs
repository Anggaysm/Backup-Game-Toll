using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Mode Select UI")]
    public UnityEngine.UI.Button highwayButton;

    public GameObject lockOverlay;

    [Header("Unlock Settings")]
    public int totalGateRequired = 4;


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

    void Start()
    {
        CheckHighwayUnlock();
    }

    void CheckHighwayUnlock()
    {
        if (highwayButton == null || lockOverlay == null)
            return;

        bool allUnlocked = true;

        for (int i = 1; i <= totalGateRequired; i++)
        {
            string saveKey =
                GameData.selectedZone +
                "_Gate_" +
                i +
                "_unlock";

            bool isUnlocked =
                PlayerPrefs.GetInt(saveKey, 0) == 1;

            if (!isUnlocked)
            {
                allUnlocked = false;
                break;
            }
        }

        highwayButton.interactable = allUnlocked;

        lockOverlay.SetActive(!allUnlocked);
    }

}