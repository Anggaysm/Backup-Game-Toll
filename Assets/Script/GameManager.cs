using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private int targetMoney;
    private float displayMoney;
    public float moneyLerpSpeed = 5f;
    public static GameManager instance;
    public TextMeshProUGUI moneyText;

    public int money = 0;

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
}