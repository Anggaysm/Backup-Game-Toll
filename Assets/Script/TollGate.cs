using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TollGate : MonoBehaviour
{
    public int level = 1;

    [Header("Upgrade Settings")]
    public int baseUpgradeCost = 2000;
    public float costMultiplier = 2.5f;
    public int maxLevel = 4;

    private Queue<CarAI> carQueue = new Queue<CarAI>();
    private bool isProcessing = false;

    int GetUpgradeCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(costMultiplier, level - 1));
    }

    bool IsAuto()
    {
        return level >= 2;
    }

    float GetDelay()
    {
        if (level == 1) return 1.5f;
        if (level == 2) return 1.0f;
        if (level == 3) return 0.5f;
        if (level == 4) return 0.1f;

        return 2f;
    }

    public void UpgradeGate()
    {
        int cost = GetUpgradeCost();

        if (level >= maxLevel)
        {
            Debug.Log("SUDAH MAX LEVEL");
            return;
        }

        if (GameManager.instance.money >= cost)
        {
            GameManager.instance.SpendMoney(cost);
            level++;

            Debug.Log("Gate upgraded ke level " + level);
            Debug.Log("Next upgrade cost: " + GetUpgradeCost());

            // 🔥 FIX: langsung jalan kalau jadi auto
            if (IsAuto())
            {
                TryProcessNextCar();
            }
        }
        else
        {
            Debug.Log("UANG GA CUKUP");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null)
            {
                car.StartPaying();
                carQueue.Enqueue(car);

                if (IsAuto())
                {
                    TryProcessNextCar();
                }
            }
        }
    }

    void TryProcessNextCar()
    {
        if (!isProcessing && carQueue.Count > 0)
        {
            StartCoroutine(ProcessCar());
        }
    }

    public void PayAndRelease()
    {
        if (level == 1)
        {
            TryProcessNextCar();
        }
    }

    IEnumerator ProcessCar()
    {
        isProcessing = true;

        CarAI car = carQueue.Dequeue();

        yield return new WaitForSeconds(GetDelay());

        GameManager.instance.AddMoney(1000);

        car.StopPaying();

        isProcessing = false;

        if (IsAuto())
        {
            TryProcessNextCar();
        }
    }
}