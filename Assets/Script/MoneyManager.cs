using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager instance;

    public int startingMoney = 0;
    public int money;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        money = PlayerPrefs.GetInt("Money", startingMoney);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        PlayerPrefs.SetInt("Money", money);
    }

    public void SpendMoney(int amount)
    {
        money -= amount;
        PlayerPrefs.SetInt("Money", money);
        
        if (money < 0 && GameManager.instance != null)
            GameManager.instance.GameOver();
    }
    
    public int GetMoney()
    {
        return money;
    }
    public void ResetMoney()
    {
        money = startingMoney;

        PlayerPrefs.SetInt("Money", money);

        Debug.Log($"💰 Money di-reset ke {money}");
    }
}