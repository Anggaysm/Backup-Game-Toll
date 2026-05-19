using UnityEngine;

public class FailureManager : MonoBehaviour
{
    public static FailureManager Instance;

    [Header("Strike Settings")]
    public int currentStrike = 0;
    public int maxStrike = 3;

    string GetSaveKey()
    {
        return GameData.selectedZone + "_Strike";
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        currentStrike =
            PlayerPrefs.GetInt(
                GetSaveKey(),
                0
            );
    }

    public void AddStrike()
    {
        currentStrike++;
        PlayerPrefs.SetInt(
            GetSaveKey(),
            currentStrike
        );

        PlayerPrefs.Save();

        Debug.Log($"⚠️ STRIKE {currentStrike}/{maxStrike}");

        if (currentStrike >= maxStrike)
        {
            GameOver();
        }
    }

    public void ResetStrike()
    {
        if (currentStrike > 0)
        {
            Debug.Log("✅ Strike streak reset!");
        }

        PlayerPrefs.SetInt(
            GetSaveKey(),
            0
        );

        PlayerPrefs.Save();
    }

    void GameOver()
    {
        Debug.Log("💀 GAME OVER!");

        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
    }
}