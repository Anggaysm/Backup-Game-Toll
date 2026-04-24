using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour
{
    private int targetMoney;
    private float displayMoney;
    public float moneyLerpSpeed = 5f;
    public static GameManager instance;
    public TextMeshProUGUI moneyText;

    public int money = 0;

    public GameObject gameOverPanel;
    public TextMeshProUGUI finalMoneyText;

    [Header("Game Over Animation")]
    public CanvasGroup gameOverCanvasGroup;
    public float fadeSpeed = 2f;
    public RectTransform gameOverText;


    void Awake()
    {
        instance = this;
        targetMoney = money;
        displayMoney = money;
    }

    public void SpendMoney(int amount)
    {
        money -= amount;
        targetMoney = money;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Money: " + money);
        targetMoney = money;
    }

    void Update()
    {
        displayMoney = Mathf.Lerp(displayMoney, targetMoney, moneyLerpSpeed * Time.deltaTime);

        if (Mathf.Abs(displayMoney - targetMoney) < 0.5f)
        {
            displayMoney = targetMoney;
        }

        moneyText.text = "Money: " + Mathf.RoundToInt(displayMoney);
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER!");

        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalMoneyText != null)
            finalMoneyText.text = "Money: " + money;

        // 🔥 reset scale dulu
        if (gameOverText != null)
            gameOverText.localScale = Vector3.zero;

        // 🔥 JALANKAN COROUTINE
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

            // fade panel
            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = t;

            // pop text
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