using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TextMeshProUGUI moneyText;

    public int money = 0;

    void Awake()
    {
        instance = this;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Money: " + money);

        moneyText.text = "Money: " + money;
    }
}