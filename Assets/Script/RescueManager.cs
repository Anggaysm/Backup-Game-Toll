using UnityEngine;
using System.Collections;

public class RescueManager : MonoBehaviour
{
    public static RescueManager Instance;

    [Header("Rescue Settings")]
    public int totalRescue = 1;

    [SerializeField]
    private int busyRescue = 0;

    public float rescueDuration = 5f;

    [Header("Purchase Settings")]
    public int maxRescue = 4;

    [Header("Purchase Settings")]
    public int baseCost = 5000;
    public float costMultiplier = 2f;

    private int purchaseCount = 0;

    [SerializeField]
    private int availableRescue = 1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (HighwayDataManager.Instance != null)
        {
            availableRescue =
                HighwayDataManager.Instance.savedRescue;

            totalRescue = availableRescue;
        }

        if (HighwayDataManager.Instance != null)
        {
            purchaseCount =
                HighwayDataManager.Instance.savedPurchaseCount;
        }
    }

    public int GetAvailableRescue()
    {
        return availableRescue - busyRescue;
    }

    public bool CanRescue()
    {
        return GetAvailableRescue() > 0;
    }

    public void StartRescue(BrokenCar targetCar)
    {
        if (!CanRescue()) return;

        StartCoroutine(RescueProcess(targetCar));
    }

    IEnumerator RescueProcess(BrokenCar targetCar)
    {
        busyRescue++;
        Debug.Log($"🚑 Available Rescue: {GetAvailableRescue()}");

        Debug.Log($"🚑 Rescue started! Busy: {busyRescue}");

        yield return new WaitForSeconds(rescueDuration);

        if (targetCar != null)
        {
            targetCar.FinishRescue();
        }

        busyRescue--;

        Debug.Log($"✅ Rescue finished! Available: {GetAvailableRescue()}");
    }

    public int GetRescueCost()
    {
        return Mathf.RoundToInt(
            baseCost * Mathf.Pow(costMultiplier, purchaseCount)
        );
    }
    public bool BuyRescue()
    {
        if (availableRescue >= maxRescue)
        {
            Debug.Log("❌ Rescue sudah max!");
            return false;
        }

        int cost = GetRescueCost();

        if (MoneyManager.instance == null)
            return false;

        if (MoneyManager.instance.money < cost)
        {
            Debug.Log($"❌ Uang tidak cukup! Need: {cost}");
            return false;
        }

        MoneyManager.instance.SpendMoney(cost);

        availableRescue++;
        totalRescue++;

        purchaseCount++;
        if (HighwayDataManager.Instance != null)
        {
            HighwayDataManager.Instance.savedPurchaseCount =
                purchaseCount;
        }

        if (HighwayDataManager.Instance != null)
        {
            HighwayDataManager.Instance.savedRescue =
                availableRescue;
        }

        Debug.Log($"✅ Rescue purchased! Total: {totalRescue}");

        return true;
    }
}