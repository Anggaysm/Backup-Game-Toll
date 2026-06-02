using UnityEngine;
using UnityEngine.UI;

public class RushHourAlertManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject alertPanel;

    [Header("UI")]
    public Image alertImage;

    public Button readyButton;

    [Header("Images")]
    public Sprite rushInfoSprite;
    public Sprite rushWarningSprite;
    public Sprite rushStartedSprite;

    private const string INFO_KEY =
        "RushHour_Info_Shown";

    private const string WARNING_KEY =
        "RushHour_Warning_Shown";

    private const string STARTED_KEY =
        "RushHour_Started_Shown";

    private void Start()
    {
        if (alertPanel != null)
            alertPanel.SetActive(false);

        readyButton.onClick.AddListener(CloseAlert);
    }

    public void ShowRushInfo()
    {
        if (PlayerPrefs.GetInt(INFO_KEY, 0) == 1)
            return;

        PlayerPrefs.SetInt(INFO_KEY, 1);
        PlayerPrefs.Save();

        ShowAlert(rushInfoSprite);
    }

    public void ShowRushWarning()
    {
        if (PlayerPrefs.GetInt(WARNING_KEY, 0) == 1)
            return;

        PlayerPrefs.SetInt(WARNING_KEY, 1);
        PlayerPrefs.Save();

        ShowAlert(rushWarningSprite);
    }

    public void ShowRushStarted()
    {
        if (PlayerPrefs.GetInt(STARTED_KEY, 0) == 1)
            return;

        PlayerPrefs.SetInt(STARTED_KEY, 1);
        PlayerPrefs.Save();

        ShowAlert(rushStartedSprite);
    }

    void ShowAlert(Sprite sprite)
    {
        if (alertPanel == null)
            return;

        alertImage.sprite = sprite;

        alertPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseAlert()
    {
        alertPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}