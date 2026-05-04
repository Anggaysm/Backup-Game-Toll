using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ModeSelectUI : MonoBehaviour
{
    public TextMeshProUGUI zoneText;

    void Start()
    {
        zoneText.text = "Zona: " + GameData.selectedZone;
    }

    public void PilihGerbangTol()
    {
        if (GameData.selectedZone == "Cililitan")
        {
            SceneManager.LoadScene("GerbangTol_Cililitan");
        }
        else if (GameData.selectedZone == "Ciawi")
        {
            SceneManager.LoadScene("GerbangTol_Ciawi");
        }
    }

    public void PilihLalinTol()
    {
        if (GameData.selectedZone == "Cililitan")
        {
            SceneManager.LoadScene("LalinTol_Cililitan");
        }
        else if (GameData.selectedZone == "Ciawi")
        {
            SceneManager.LoadScene("LalinTol_Ciawi");
        }
    }

    public void BackToMap()
    {
        SceneManager.LoadScene("ZoneSelect");
    }
}