using UnityEngine;

public class FailureManager : MonoBehaviour
{
    public static FailureManager Instance;

    [Header("Strike Settings")]
    public int currentStrike = 0;
    public int maxStrike = 3;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddStrike()
    {
        currentStrike++;

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

        currentStrike = 0;
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