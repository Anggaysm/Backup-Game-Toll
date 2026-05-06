using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalMoneyText;

    [Header("Game Over Animation")]
    public CanvasGroup gameOverCanvasGroup;
    public float fadeSpeed = 2f;
    public RectTransform gameOverText;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER!");
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalMoneyText != null && MoneyManager.instance != null)
            finalMoneyText.text = "Money: " + MoneyManager.instance.money;

        if (gameOverText != null)
            gameOverText.localScale = Vector3.zero;

        StartCoroutine(FadeInGameOver());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FadeInGameOver()
    {
        float t = 0f;

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
        }

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * fadeSpeed;

            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = t;

            if (gameOverText != null)
            {
                float scale = Mathf.Lerp(0f, 1f, t);
                gameOverText.localScale = new Vector3(scale, scale, scale);
            }

            yield return null;
        }

        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 1f;

        if (gameOverText != null)
            gameOverText.localScale = Vector3.one;
    }
}