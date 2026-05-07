using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private int nextQueueNumber = 1;

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

    [Header("Traffic Pressure")]
    public float maxCountdown = 5f;
    private float currentCountdown;
    public int penaltyMoney = 1000;
    private bool isTrafficJam = false;
    private bool wasTrafficJam = false;

    [Header("Penalty Counter")]
    public int maxPenaltyCount = 3;
    private int currentPenaltyCount = 0;

    float jamDetectTimer = 0f;
    public float jamThreshold = 2f;

    [Header("Traffic UI")]
    public TextMeshProUGUI trafficText;
    public CanvasGroup trafficCanvasGroup;

    [Header("Save System")]
    public string gateID;
    private string loadedQueueData;
    private bool isRestoringTraffic = false;

    [Header("Queue Detector")]
    public QueueDetector queueDetector;

    [Header("Loading UI")]
    public GameObject loadingPanel;

    // NEW: Auto-save timer
    private float autoSaveTimer = 0f;
    public float autoSaveInterval = 5f; // Save setiap 5 detik

    void Start()
    {
        LoadProgress();

        if (spawner != null)
            spawner.StopSpawner();

        StartCoroutine(RestoreQueueTraffic());

        UpdateUI();
        UpdatePayButtonState();

        Debug.Log($"=== TOLL GATE READY ===");
        Debug.Log($"Gate Level: {level}, Unlocked: {isUnlocked}");
        Debug.Log($"Max Countdown: {maxCountdown}, Penalty: {penaltyMoney}");
        Debug.Log($"Max Penalty Count: {maxPenaltyCount} kali");
    }

    void Update()
    {
        UpdateUI();
        HandleTrafficPressure();
        UpdateTrafficUIPosition();
        
        // AUTO-SAVE setiap beberapa detik
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveProgress();
        }
        
        // Manual save with Q key (tetap jalan)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SaveProgress();
            string savedQueue = PlayerPrefs.GetString(gateID + "_queue");
            Debug.Log("QUEUE SAVED: " + savedQueue);
        }
    }

    // FIX 1: Save otomatis pas keluar dari scene/object di-destroy
    void OnDestroy()
    {
        Debug.Log($"🔄 {gateID} - OnDestroy called! Saving before destruction...");
        SaveProgress();
    }

    // FIX 2: Save pas aplikasi ditutup
    void OnApplicationQuit()
    {
        Debug.Log($"🔄 {gateID} - Application quitting! Saving final state...");
        SaveProgress();
    }

    // FIX 3: Save pas script di-disable
    void OnDisable()
    {
        Debug.Log($"🔄 {gateID} - OnDisable called! Saving before disable...");
        SaveProgress();
    }

    void UpdatePayButtonState()
    {
        if (payButtonComponent == null) return;

        bool hasCar = carQueue.Count > 0;
        bool canPay = hasCar && !isProcessing && level == 1;

        payButtonComponent.interactable = canPay;

        CanvasGroup cg = payButtonComponent.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = canPay ? 1f : 0.5f;
        }
    }

    void HandleTrafficPressure()
    {
        if (isRestoringTraffic)
        {
            return;
        }
        if (!isUnlocked)
        {
            if (trafficText != null)
            {
                trafficText.text = "TUTUP";
                trafficText.color = Color.gray;

                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 1f;
            }
            return;
        }
        
        bool unsafeToSpawn = false;
        if (spawner != null)
        {
            // Pastikan method IsSafeToSpawn ada
            unsafeToSpawn = !spawner.IsSafeToSpawn();
        }
        else
        {
            Debug.LogWarning("Spawner not assigned!");
        }

        if (unsafeToSpawn)
        {
            jamDetectTimer += Time.deltaTime;

            if (jamDetectTimer >= jamThreshold)
            {
                isTrafficJam = true;
            }
        }
        else
        {
            jamDetectTimer = 0f;
            isTrafficJam = false;
        }
        
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🚦 Status: {(isTrafficJam ? "MACET" : "LANCAR")} | Timer: {(isTrafficJam ? currentCountdown.ToString("F1") : "0")} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
        }
        
        if (trafficText != null)
        {
            if (isTrafficJam)
            {
                trafficText.text = "MACET!\n" + Mathf.Ceil(currentCountdown).ToString();
                trafficText.color = (currentCountdown <= 1.5f) ? Color.red : Color.yellow;

                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 1f;
            }
            else
            {
                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 0f;
            }
        }
        
        if (isTrafficJam)
        {
            if (!wasTrafficJam)
            {
                currentCountdown = maxCountdown;
                Debug.Log($"⚠️ MACET DETEKSI! Countdown {maxCountdown} detik dimulai");
                wasTrafficJam = true;
            }
            
            currentCountdown -= Time.deltaTime;
            
            if (currentCountdown <= 1f && currentCountdown > 0)
            {
                Debug.Log($"⚠️ {currentCountdown:F1} DETIK LAGI KENA DENDA!");
            }
            
            if (currentCountdown <= 0)
            {
                ApplyPenalty();
                currentCountdown = maxCountdown;
            }
        }
        else
        {
            if (wasTrafficJam)
            {
                Debug.Log($"✅ MACET SELESAI! LANCAR LAGI - Penalty counter di-reset ke 0");
                wasTrafficJam = false;
                currentCountdown = maxCountdown;
                currentPenaltyCount = 0;
                SaveProgress(); // FIX: Save saat macet selesai
            }
        }
    }

    void ShowWarningText(string message, Color color)
    {
        if (floatingTextPrefab == null || mainCamera == null || canvas == null) return;
        
        Vector3 worldPos = transform.position + Vector3.up * 4f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        
        GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
        RectTransform rt = ft.GetComponent<RectTransform>();
        rt.position = screenPos;
        
        FloatingText ftScript = ft.GetComponent<FloatingText>();
        if (ftScript != null)
        {
            ftScript.SetText(message);
            ftScript.SetColor(color);
        }
        
        Destroy(ft, 1.5f);
    }

    void ApplyPenalty()
    {
        currentPenaltyCount++;
        
        Debug.Log($"⚠️ PENALTY KE-{currentPenaltyCount} dari {maxPenaltyCount}");
        
        if (currentPenaltyCount >= maxPenaltyCount)
        {
            Debug.Log($"💀 GAME OVER! Sudah kena penalty {currentPenaltyCount} kali (maksimal {maxPenaltyCount})");
            ShowWarningText($"PENALTY KE-{currentPenaltyCount}! GAME OVER!", Color.red);
            
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
            else
                Debug.LogError("GameManager.instance is NULL! Cannot call GameOver.");
            
            return;
        }
        
        if (MoneyManager.instance != null && MoneyManager.instance.money >= penaltyMoney)
        {
            MoneyManager.instance.SpendMoney(penaltyMoney);
            Debug.Log($"💸 DENDA! -{penaltyMoney} | Sisa uang: {MoneyManager.instance.money} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
            SaveProgress(); // FIX: Save setelah kena penalty
            
            if (floatingTextPrefab != null && mainCamera != null && canvas != null)
            {
                Vector3 worldPos = transform.position + Vector3.up * 3f;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                
                GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
                RectTransform rt = ft.GetComponent<RectTransform>();
                rt.position = screenPos;
                
                FloatingText ftScript = ft.GetComponent<FloatingText>();
                if (ftScript != null)
                {
                    ftScript.SetText($"-{penaltyMoney}\n({currentPenaltyCount}/{maxPenaltyCount})");
                    ftScript.SetColor(Color.red);
                }
            }
            
            if (MoneyManager.instance.money <= 0)
            {
                Debug.Log($"💀 GAME OVER! Uang habis!");
                if (GameManager.instance != null)
                    GameManager.instance.GameOver();
            }
        }
        else
        {
            Debug.Log($"💀 GAME OVER! Uang tidak cukup bayar denda {penaltyMoney}");
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
        }
    }

    public void ResetPenaltyCounter()
    {
        currentPenaltyCount = 0;
        Debug.Log($"🔄 Penalty counter di-reset! Sekarang: {currentPenaltyCount}/{maxPenaltyCount}");
        SaveProgress(); // FIX: Save saat reset penalty
    }

    void UpdateUI()
    {
        if (isUnlocked)
        {
            unlockButton.SetActive(false);

            levelText.text = "Gate Lv." + level;

            if (level >= maxLevel)
            {
                upgradeButton.SetActive(false);
                if (maxLevelText != null) maxLevelText.SetActive(true);
            }
            else
            {
                upgradeButton.SetActive(true);
                if (maxLevelText != null) maxLevelText.SetActive(false);

                int cost = GetUpgradeCost();
                upgradeText.text = "Upgrade\n(" + cost + ")";

                bool canUpgrade = MoneyManager.instance != null && MoneyManager.instance.money >= cost;
                SetButtonState(upgradeButton.GetComponent<Button>(), canUpgrade);
            }

            if (payButton != null) payButton.SetActive(level == 1);
        }
        else
        {
            if (unlockButton != null) unlockButton.SetActive(true);
            if (upgradeButton != null) upgradeButton.SetActive(false);
            if (payButton != null) payButton.SetActive(false);
            if (maxLevelText != null) maxLevelText.SetActive(false);

            if (levelText != null) levelText.text = "Locked";
            if (unlockText != null) unlockText.text = "Buka Pintu\n(" + unlockCost + ")";

            bool canUnlock = MoneyManager.instance != null && MoneyManager.instance.money >= unlockCost;
            if (unlockButton != null) SetButtonState(unlockButton.GetComponent<Button>(), canUnlock);
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
            cg.alpha = canAfford ? 1f : 0.5f;
        }
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

    public void UnlockGate()
    {
        if (isUnlocked)
        {
            Debug.Log("Gate sudah terbuka");
            return;
        }

        if (MoneyManager.instance != null && MoneyManager.instance.money >= unlockCost)
        {
            MoneyManager.instance.SpendMoney(unlockCost);
            isUnlocked = true;
            SaveProgress();
            ResetPenaltyCounter();
            
            if (mainCamera != null && canvas != null)
            {
                Vector3 worldPos = transform.position + Vector3.up * 2f;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                
                if (audioSource != null && unlockSound != null)
                    audioSource.PlayOneShot(unlockSound);

                if (floatingTextPrefab != null)
                {
                    GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
                    RectTransform rt = ft.GetComponent<RectTransform>();
                    if (rt != null) rt.position = screenPos;

                    FloatingText ftScript = ft.GetComponent<FloatingText>();
                    if (ftScript != null)
                    {
                        ftScript.SetText("Terbuka");
                        ftScript.SetColor(Color.cyan);
                    }
                }
            }

            Debug.Log($"🔓 GATE DIBUKA! Uang tersisa: {MoneyManager.instance.money}");
            UpdateSpawnerState();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Uang tidak cukup! Butuh: {unlockCost}, Punya: {(MoneyManager.instance != null ? MoneyManager.instance.money : 0)}");
        }
    }

    public void UpgradeGate()
    {
        int cost = GetUpgradeCost();

        if (level >= maxLevel)
        {
            Debug.Log("Gate sudah level MAX!");
            return;
        }

        if (MoneyManager.instance != null && MoneyManager.instance.money >= cost)
        {
            MoneyManager.instance.SpendMoney(cost);
            level++;
            SaveProgress();
            ResetPenaltyCounter();
            
            if (mainCamera != null && canvas != null)
            {
                Vector3 worldPos = transform.position + Vector3.up * 3f;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                
                if (audioSource != null && upgradeSound != null)
                    audioSource.PlayOneShot(upgradeSound);

                if (floatingTextPrefab != null)
                {
                    GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
                    RectTransform rt = ft.GetComponent<RectTransform>();
                    if (rt != null) rt.position = screenPos;

                    FloatingText ftScript = ft.GetComponent<FloatingText>();
                    if (ftScript != null)
                    {
                        ftScript.SetText("Upgrade Lv." + level);
                        ftScript.SetColor(Color.green);
                    }
                }
            }

            Debug.Log($"⬆️ GATE UPGRADE ke Level {level} | Uang tersisa: {MoneyManager.instance.money}");
            UpdateUI();

            if (IsAuto())
            {
                TryProcessNextCar();
            }
        }
        else
        {
            Debug.Log($"Uang tidak cukup upgrade! Butuh: {cost}, Punya: {(MoneyManager.instance != null ? MoneyManager.instance.money : 0)}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isUnlocked) return;

        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null)
            {
                car.queueNumber = nextQueueNumber;
                nextQueueNumber++;

                car.StartPaying();
                carQueue.Enqueue(car);
                UpdatePayButtonState();
                SaveProgress(); // FIX: Save saat ada mobil masuk antrian
                
                Debug.Log($"🚗 MOBIL MASUK | Queue Number: {car.queueNumber} | Total: {carQueue.Count}");
                
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
        UpdatePayButtonState();

        CarAI car = carQueue.Dequeue();
        
        Debug.Log($"🔄 PROSES MOBIL | Sisa: {carQueue.Count}");

        yield return new WaitForSeconds(GetDelay());

        int money = car.GetPrice();
        
        if (MoneyManager.instance != null)
            MoneyManager.instance.AddMoney(money);
        
        if (audioSource != null && moneySound != null)
            audioSource.PlayOneShot(moneySound, 0.5f);
        
        Debug.Log($"💰 +{money} | Total: {(MoneyManager.instance != null ? MoneyManager.instance.money : 0)}");

        if (mainCamera != null && canvas != null && floatingTextPrefab != null)
        {
            Vector3 worldPos = transform.position + Vector3.up * 2f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
            RectTransform rt = ft.GetComponent<RectTransform>();
            if (rt != null) rt.position = screenPos;

            FloatingText ftScript = ft.GetComponent<FloatingText>();
            if (ftScript != null)
            {
                ftScript.SetText("+" + money);
            }
        }

        car.StopPaying();
        
        yield return new WaitForSeconds(0.3f);

        isProcessing = false;
        UpdatePayButtonState();
        SaveProgress(); // FIX: Save setelah proses mobil selesai

        if (IsAuto() && carQueue.Count > 0)
        {
            TryProcessNextCar();
        }
    }

    void UpdateTrafficUIPosition()
    {
        if (trafficText == null || mainCamera == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 4f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        trafficText.transform.position = screenPos;
    }

    // FIX: Improved Save function with better queue saving
    void SaveProgress()
    {
        if (string.IsNullOrEmpty(gateID))
        {
            Debug.LogWarning("GateID kosong!");
            return;
        }

        PlayerPrefs.SetInt(gateID + "_level", level);
        PlayerPrefs.SetInt(gateID + "_unlock", isUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(gateID + "_penaltyCount", currentPenaltyCount);
        PlayerPrefs.SetInt(gateID + "_nextQueueNumber", nextQueueNumber);
        
        // FIX: Also save current money state if needed
        if (MoneyManager.instance != null)
        {
            PlayerPrefs.SetInt("TotalMoney", MoneyManager.instance.money);
        }
        
        // Save queue data with better format
        string queueData = GetQueueData();
        PlayerPrefs.SetString(gateID + "_queue", queueData);
        
        // Save car queue data separately
        SaveCarQueueData();

        PlayerPrefs.Save();

        Debug.Log($"💾 SAVE {gateID} | Level: {level} | Unlock: {isUnlocked} | Penalty: {currentPenaltyCount} | Queue Length: {carQueue.Count}");
    }

    // FIX: New method to save car queue properly
    void SaveCarQueueData()
    {
        List<string> carDataList = new List<string>();
        CarAI[] cars = carQueue.ToArray();
        
        foreach (CarAI car in cars)
        {
            if (car != null)
            {
                string data = $"{car.carID}|{car.category}|{car.queueNumber}";
                carDataList.Add(data);
            }
        }
        
        string allCarData = string.Join(";", carDataList);
        PlayerPrefs.SetString(gateID + "_carQueue", allCarData);
        Debug.Log($"Saved {carDataList.Count} cars in queue");
    }

    void LoadProgress()
    {
        if (string.IsNullOrEmpty(gateID))
        {
            Debug.LogWarning("GateID kosong!");
            return;
        }

        level = PlayerPrefs.GetInt(gateID + "_level", level);
        isUnlocked = PlayerPrefs.GetInt(gateID + "_unlock", isUnlocked ? 1 : 0) == 1;
        currentPenaltyCount = PlayerPrefs.GetInt(gateID + "_penaltyCount", 0);
        nextQueueNumber = PlayerPrefs.GetInt(gateID + "_nextQueueNumber", 1);
        loadedQueueData = PlayerPrefs.GetString(gateID + "_queue", "");

        Debug.Log($"📥 LOAD {gateID} | Level: {level} | Unlock: {isUnlocked} | Penalty: {currentPenaltyCount}");
        Debug.Log($"🚗 LOAD QUEUE: {loadedQueueData}");
    }

    string GetQueueData()
    {
        if (queueDetector == null || queueDetector.queuedCars == null)
            return "";

        if (queueDetector.queuedCars.Count == 0)
            return "";

        List<string> data = new List<string>();

        foreach (CarAI car in queueDetector.queuedCars)
        {
            if (car != null)
            {
                string carData = car.carID + "|" + car.category.ToString();
                data.Add(carData);
            }
        }

        return string.Join(",", data);
    }

    IEnumerator RestoreQueueTraffic()
    {
        isRestoringTraffic = true;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (string.IsNullOrEmpty(loadedQueueData))
        {
            Debug.Log("Tidak ada queue untuk direstore");
            isRestoringTraffic = false;
            if (loadingPanel != null) loadingPanel.SetActive(false);
            yield break;
        }

        if (spawner != null)
            spawner.SetActive(false);

        Debug.Log("🚦 MULAI RESTORE TRAFFIC");

        string[] carsData = loadedQueueData.Split(',');

        foreach (string data in carsData)
        {
            if (string.IsNullOrEmpty(data))
                continue;

            string[] splitData = data.Split('|');

            if (splitData.Length >= 1)
            {
                string carID = splitData[0];
                Debug.Log("🚗 RESTORE MOBIL: " + carID);

                if (spawner != null)
                    spawner.SpawnSpecificCar(carID);

                yield return new WaitForSeconds(0.75f);
            }
        }

        if (spawner != null)
            spawner.SetActive(true);

        Debug.Log("✅ RESTORE TRAFFIC SELESAI");
        yield return new WaitForSeconds(1f);

        if (isUnlocked && spawner != null)
        {
            spawner.StartSpawner();
        }
        
        isRestoringTraffic = false;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
}