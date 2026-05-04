using UnityEngine;
using UnityEngine.SceneManagement;

public class MapJagorawiUI : MonoBehaviour
{
    public void PilihCililitan()
    {
        GameData.selectedZone = "Cililitan";
        SceneManager.LoadScene("ModeSelect");
    }

    public void PilihCiawi()
    {
        GameData.selectedZone = "Ciawi";
        SceneManager.LoadScene("ModeSelect");
    }

    public void BackToHome()
    {
        SceneManager.LoadScene("HomeScreen");
    }
}