using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TollGate : MonoBehaviour
{
    public Button payButtonComponent;

    [Header("Spawner")]
    public CarSpawner spawner;

    [Header("Unlock Settings")]
    public bool isUnlocked = true;
    public int unlockCost = 5000;

    [Header("Upgrade Settings")]
    public int level = 1;
    public int baseUpgradeCost = 2000;
    public float costMultiplier = 2.5f;
    public int maxLevel = 4;

    public GameObject unlockButton;
    public GameObject upgradeButton;
    public GameObject payButton;

    public TextMeshProUGUI unlockText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI levelText;

    private Queue<CarAI> carQueue = new Queue<CarAI>();
    private bool isProcessing = false;

    public GameObject maxLevelText;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;
    public Camera mainCamera;
    public Canvas canvas;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip moneySound;
    public AudioClip upgradeSound;
    public AudioClip unlockSound;

    void UpdatePayButtonState()
    {
        if (payButtonComponent == null) return;

        bool hasCar = carQueue.Count > 0;

        // enable/disable button
        payButtonComponent.interactable = hasCar;

        // efek visual (optional biar lebih jelas)
        CanvasGroup cg = payButtonComponent.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = hasCar ? 1f : 0.5f;
        }
    }
        

    void UpdateUI()
    {
        if (isUnlocked)
        {
            unlockButton.SetActive(false);

            // 🔥 LEVEL TEXT
            levelText.text = "Gate Lv." + level;

            // 🔥 MAX LEVEL CHECK
            if (level >= maxLevel)
            {
                upgradeButton.SetActive(false);
                maxLevelText.SetActive(true);
            }
            else
            {
                upgradeButton.SetActive(true);
                maxLevelText.SetActive(false);

                int cost = GetUpgradeCost();
                upgradeText.text = "Upgrade\n(" + cost + ")";

                // 🔥 CEK UANG
                bool canUpgrade = GameManager.instance.money >= cost;
                SetButtonState(upgradeButton.GetComponent<Button>(), canUpgrade);
            }

            // PAY BUTTON
            payButton.SetActive(level == 1);
        }
        else
        {
            unlockButton.SetActive(true);
            upgradeButton.SetActive(false);
            payButton.SetActive(false);
            maxLevelText.SetActive(false);

            // 🔒 TEXT
            levelText.text = "Locked";
            unlockText.text = "Buka Pintu\n(" + unlockCost + ")";

            // 🔥 CEK UANG
            bool canUnlock = GameManager.instance.money >= unlockCost;
            SetButtonState(unlockButton.GetComponent<Button>(), canUnlock);
        }

        UpdatePayButtonState();
    }

    void SetButtonState(Button btn, bool canAfford)
    {
        if (btn == null) return;

        btn.interactable = canAfford;

        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = canAfford ? 1f : 0.5f; // redup kalau ga mampu
        }
    }

    void Start()
    {
        UpdateSpawnerState();
        UpdateUI();
        UpdatePayButtonState();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateSpawnerState()
    {
        if (spawner != null)
        {
            spawner.SetActive(isUnlocked);
        }
    }

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

    // 🔓 UNLOCK GATE
    public void UnlockGate()
    {
        if (isUnlocked)
        {
            Debug.Log("Gate sudah terbuka");
            return;
        }

        if (GameManager.instance.money >= unlockCost)
        {
            GameManager.instance.SpendMoney(unlockCost);
            isUnlocked = true;
            // 🔥 SPAWN TEXT "GATE DIBUKA"
            Vector3 worldPos = transform.position + Vector3.up * 2f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            audioSource.PlayOneShot(unlockSound);

            GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
            RectTransform rt = ft.GetComponent<RectTransform>();
            rt.position = screenPos;

            FloatingText ftScript = ft.GetComponent<FloatingText>();
            if (ftScript != null)
            {
                ftScript.SetText("Terbuka");
                ftScript.SetColor(Color.cyan); 
            }

            Debug.Log("Gate berhasil dibuka!");

            UpdateSpawnerState(); // 🔥 aktifin spawner
            UpdateUI();

        }
        else
        {
            Debug.Log("Uang tidak cukup!");
        }
    }

    // ⬆️ UPGRADE
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
            // 🔥 SPAWN TEXT DI ATAS GATE
            Vector3 worldPos = transform.position + Vector3.up * 3f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            audioSource.PlayOneShot(upgradeSound);

            GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
            RectTransform rt = ft.GetComponent<RectTransform>();
            rt.position = screenPos;

            FloatingText ftScript = ft.GetComponent<FloatingText>();
            if (ftScript != null)
            {
                ftScript.SetText("Upgrade Lv." + level);
                ftScript.SetColor(Color.green); // optional biar beda
            }

            Debug.Log("Gate upgraded ke level " + level);
            UpdateUI();

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
        if (!isUnlocked) return; // ⛔ gate masih lock

        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null)
            {
                car.StartPaying();
                carQueue.Enqueue(car);
                UpdatePayButtonState();

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
        UpdatePayButtonState();

        yield return new WaitForSeconds(GetDelay());

        // 🔥 AMBIL MONEY
        int money = car.GetPrice();

        // 🔥 TAMBAH UANG (CUMA SEKALI!)
        GameManager.instance.AddMoney(money);
        audioSource.PlayOneShot(moneySound, 0.5f);

        // 🔥 POSISI DI ATAS GATE (WORLD)
        Vector3 worldPos = transform.position + Vector3.up * 2f;

        // 🔥 CONVERT KE SCREEN
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 🔥 SPAWN DI CANVAS
        GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
        RectTransform rt = ft.GetComponent<RectTransform>();

        rt.position = screenPos;

        // 🔥 SET TEXT
        FloatingText ftScript = ft.GetComponent<FloatingText>();
        if (ftScript != null)
        {
            ftScript.SetText("+" + money);
        }

        car.StopPaying();

        isProcessing = false;
        UpdatePayButtonState();

        if (IsAuto())
        {
            TryProcessNextCar();
        }
    }
}