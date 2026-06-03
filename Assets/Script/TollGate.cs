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

    [Header("Traffic Pressure")]
    public float maxCountdown = 5f;
    private float currentCountdown;
    public int penaltyMoney = 1000;
    private bool isTrafficJam = false;
    private bool wasTrafficJam = false;
    public int maxPenaltyCount = 3;
    private int currentPenaltyCount = 0;
    float jamDetectTimer = 0f;
    public float jamThreshold = 2f;

    [Header("Traffic UI")]
    public TextMeshProUGUI trafficText;
    public CanvasGroup trafficCanvasGroup;

    [Header("Save System - SIMPLE")]
    public string gateID;
    private int savedQueueCount = 0; // SIMPAN JUMLAH SAJA
    
    [Header("Queue Detector")]
    public QueueDetector queueDetector;
    private bool isRestoringTraffic = false;

    [Header("Layer Settings")]
    public LayerMask carLayer;

    private float autoSaveTimer = 0f;
    public float autoSaveInterval = 5f;

    [Header("Restore Settings")]
    public float restoreSpawnInterval = 0.5f;
    public float restoreYOffset = 0.8f;
    public float restoreSpeedMultiplier = 2.5f;

    string GetSaveKey()
    {
        return GameData.selectedZone + "_" + gateID;
    }

    void Start()
    {
        LoadProgress();

        if (spawner != null)
        {
            spawner.SetActive(isUnlocked);
            if (isUnlocked) spawner.StartSpawner();
        }

        if (spawner != null)
        {
            spawner.SetSpawnRateByLevel(level);
        }

        if (savedQueueCount > 0)
        {
            LoadingManager.instance.ShowLoading("Loading Traffic...");

            StartCoroutine(StartRestoreDelay());
        }else
        {
            // 🔥 TIDAK ADA QUEUE, LANGSUNG START RUSH HOUR
            StartRushHourSystemIfReady();
        }
        
        UpdateUI();
        UpdatePayButtonState();

        Debug.Log($"=== TOLL GATE {gateID} READY | Level: {level} | Unlocked: {isUnlocked} ===");
    }

    IEnumerator StartRestoreDelay()
    {
        yield return null;
        yield return new WaitForSeconds(0.2f);

        StartCoroutine(RestoreQueueSimple());
    }

    void Update()
    {
        UpdateUI();
        HandleTrafficPressure();
        UpdateTrafficUIPosition();
        
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveProgressSimple(); // SAVE SIMPLE dan CEPAT
        }
    }

    // ==================== SAVE SYSTEM SIMPLE ====================
    
    void SaveProgressSimple()
    {
        if (string.IsNullOrEmpty(gateID)) return;

        // Simpan data dasar
        PlayerPrefs.SetInt(GetSaveKey() + "_level", level);
        PlayerPrefs.SetInt(
            GetSaveKey() + "_unlock",
            isUnlocked ? 1 : 0
        );
        PlayerPrefs.SetInt(
            GetSaveKey() + "_penaltyCount",
            currentPenaltyCount
        );        
        // SIMPLE: Simpan jumlah mobil di queue area
        int queueCount = GetQueueCount();
        PlayerPrefs.SetInt(
            GetSaveKey() + "_queueCount",
            queueCount
        );        
        if (MoneyManager.instance != null)
        {
            PlayerPrefs.SetInt("TotalMoney", MoneyManager.instance.money);
        }

        PlayerPrefs.Save();
        
        Debug.Log($"💾 SAVE SIMPLE {gateID} | Level: {level} | Queue Count: {queueCount}");
    }

    void LoadProgress()
    {
        if (string.IsNullOrEmpty(gateID)) return;

        level =
            PlayerPrefs.GetInt(
                GetSaveKey() + "_level",
                level
            );
        isUnlocked =
            PlayerPrefs.GetInt(
                GetSaveKey() + "_unlock",
                isUnlocked ? 1 : 0
            ) == 1;
        currentPenaltyCount =
            PlayerPrefs.GetInt(
                GetSaveKey() + "_penaltyCount",
                0
            );        
        // SIMPLE: Load jumlah queue
        savedQueueCount =
            PlayerPrefs.GetInt(
                GetSaveKey() + "_queueCount",
                0
            );
        Debug.Log($"📥 LOAD SIMPLE {gateID} | Level: {level} | Saved Queue Count: {savedQueueCount}");
    }

    int GetQueueCount()
    {
        // Hitung dari queueDetector (lebih akurat)
        if (queueDetector != null && queueDetector.queuedCars != null)
        {
            return queueDetector.queuedCars.Count;
        }
        
        // Fallback ke carQueue
        if (carQueue != null)
        {
            return carQueue.Count;
        }
        
        return 0;
    }
    
    IEnumerator RestoreQueueSimple()
    {
        isRestoringTraffic = true;
        
        if (savedQueueCount <= 0)
        {
            Debug.Log($"✅ Tidak ada queue untuk direstore");
            StartRushHourSystemIfReady();
            yield break;
        }
        
        if (LoadingManager.instance != null)
        {
            yield return null;
        }
        
        Debug.Log($"🚦 MULAI RESTORE: {savedQueueCount} mobil (dengan pengecekan jarak aman)");

        IncreaseAllExistingCarsSpeed(restoreSpeedMultiplier);

        if (spawner != null)
            spawner.SetActive(false);
        
        int successfullySpawned = 0;
        int retryCount = 0;
        int maxRetries = savedQueueCount * 3; // Batas maksimal retry
        
        while (successfullySpawned < savedQueueCount && retryCount < maxRetries)
        {
            if (spawner != null)
            {
                bool spawnSuccess = spawner.SpawnRandomCarFast(restoreYOffset, restoreSpeedMultiplier);
                
                if (spawnSuccess)
                {
                    successfullySpawned++;
                    retryCount = 0; // Reset retry counter
                    
                    // UPDATE PROGRESS UI
                    if (LoadingManager.instance != null)
                    {
                        float progress = (float)successfullySpawned / savedQueueCount;
                        int percent = Mathf.RoundToInt(progress * 100f);
                        LoadingManager.instance.UpdateProgress(progress);
                    }
                    
                    Debug.Log($"🚗 RESTORE MOBIL KE-{successfullySpawned} dari {savedQueueCount}");
                }
                else
                {
                    retryCount++;
                    if (retryCount % 5 == 0)
                    {
                        Debug.Log($"⏳ Menunggu jarak aman... (retry {retryCount})");
                    }
                }
            }
            
            // Tunggu sebentar sebelum cek lagi (lebih pendek dari interval spawn normal)
            yield return new WaitForSeconds(0.2f);
        }
        
        if (spawner != null)
        {
            spawner.SetActive(true);
            spawner.ClearLastSpawnedCar(); // Clear tracking setelah selesai
        }
        
        if (isUnlocked && spawner != null)
        {
            spawner.StartSpawner();
        }
        
        PlayerPrefs.SetInt(
            GetSaveKey() + "_queueCount",
            0
        );
        PlayerPrefs.Save();
        
        Debug.Log($"✅ RESTORE SELESAI! {successfullySpawned}/{savedQueueCount} mobil dispawan");
        
        isRestoringTraffic = false;
        
        if (LoadingManager.instance != null)
        {
            LoadingManager.instance.HideLoading();
        }
        
        StartRushHourSystemIfReady();
    }

    // Method baru di TollGate.cs
    void StartRushHourSystemIfReady()
    {
        if (RushHourManager.Instance != null)
        {
            Debug.Log("🚦 Starting Rush Hour System after loading complete!");
            RushHourManager.Instance.StartRushHourSystem();
        }
        else
        {
            Debug.LogWarning("⚠️ RushHourManager.Instance not found!");
        }
    }

    // ==================== TOLL GATE LOGIC (SAMA) ====================
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isUnlocked) return;

        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null && !carQueue.Contains(car))
            {
                car.IsInQueue = true;

                car.StartPaying();
                carQueue.Enqueue(car);
                
                if (queueDetector != null && !queueDetector.queuedCars.Contains(car))
                {
                    queueDetector.queuedCars.Add(car);
                }
                
                UpdatePayButtonState();
                SaveProgressSimple(); // Langsung save simple
                
                Debug.Log($"🚗 MOBIL MASUK | Total: {carQueue.Count}");
                
                if (IsAuto())
                {
                    TryProcessNextCar();
                }
            }
        }
    }

    IEnumerator ProcessCar()
    {
        isProcessing = true;
        UpdatePayButtonState();

        CarAI car = carQueue.Dequeue();
        car.IsInQueue = false;
        
        if (queueDetector != null && queueDetector.queuedCars.Contains(car))
        {
            queueDetector.queuedCars.Remove(car);
        }
        
        Debug.Log($"🔄 PROSES MOBIL | Sisa: {carQueue.Count}");

        yield return new WaitForSeconds(GetDelay());

        int money = car.GetPrice();
        
        if (MoneyManager.instance != null)
            MoneyManager.instance.AddMoney(money);
        
        if (audioSource != null && moneySound != null)
            audioSource.PlayOneShot(
                moneySound,
                0.5f * BGMManager.SFXVolume
            );
        
        Debug.Log($"💰 +{money} | Total: {(MoneyManager.instance != null ? MoneyManager.instance.money : 0)}");

        if (mainCamera != null && canvas != null && floatingTextPrefab != null)
        {
            Vector3 worldPos = transform.position + Vector3.up * 2f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
            ft.GetComponent<RectTransform>().position = screenPos;
            ft.GetComponent<FloatingText>().SetText("+" + money);
        }

        car.StopPaying();
        
        yield return new WaitForSeconds(0.3f);

        isProcessing = false;
        UpdatePayButtonState();
        SaveProgressSimple(); // Save setelah proses

        if (IsAuto() && carQueue.Count > 0)
        {
            TryProcessNextCar();
        }
    }

    // ==================== METHOD LAIN (TIDAK BERUBAH) ====================
    
    void UpdatePayButtonState()
    {
        if (payButtonComponent == null) return;
        bool hasCar = carQueue.Count > 0;
        bool canPay = hasCar && !isProcessing && level == 1;
        payButtonComponent.interactable = canPay;
    }

    void HandleTrafficPressure()
    {
        if (isRestoringTraffic)
            return;

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
        if (spawner != null && spawner.isActiveAndEnabled)
        {
            bool hasActiveCars = CheckIfAnyCarsExist();
            if (hasActiveCars)
            {
                unsafeToSpawn = !spawner.IsSafeToSpawn();
            }
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
            if (isTrafficJam)
            {
                isTrafficJam = false;
                wasTrafficJam = false;
                currentCountdown = maxCountdown;
                currentPenaltyCount = 0;
            }
        }
        
        // DEBUG LOG SETIAP 60 FRAME
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🚦 Status: {(isTrafficJam ? "MACET" : "LANCAR")} | Timer: {(isTrafficJam ? currentCountdown.ToString("F1") : "0")} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
        }
        
        // UPDATE UI TRAFFIC
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
        
        // LOGIKA COUNTDOWN MACET
        if (isTrafficJam)
        {
            if (!wasTrafficJam)
            {
                currentCountdown = maxCountdown;
                Debug.Log($"⚠️ MACET DETEKSI! Countdown {maxCountdown} detik dimulai");
                // TAMBAHKAN WARNING FLOATING TEXT SAAT MACET MULAI
                wasTrafficJam = true;
            }
            
            currentCountdown -= Time.deltaTime;
            
            
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
                Debug.Log($"✅ MACET SELESAI! LANCAR LAGI");
                ShowWarningText("✅ TRAFFIC LANCAR", Color.green);
                wasTrafficJam = false;
                currentCountdown = maxCountdown;
                currentPenaltyCount = 0;
                SaveProgressSimple();
            }
        }
    }

    bool CheckIfAnyCarsExist()
    {
        if (carQueue.Count > 0) return true;
        if (queueDetector != null && queueDetector.queuedCars.Count > 0) return true;
        return FindObjectsByType<CarAI>(FindObjectsSortMode.None).Length > 0;
    }

    void ShowWarningText(string message, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("FloatingTextPrefab is null!");
            return;
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("MainCamera is null!");
            return;
        }
        
        if (canvas == null)
        {
            Debug.LogWarning("Canvas is null!");
            return;
        }
        
        Vector3 worldPos = transform.position + Vector3.up * 4f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        
        GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
        
        RectTransform rt = ft.GetComponent<RectTransform>();
        if (rt != null)
            rt.position = screenPos;
        
        FloatingText ftScript = ft.GetComponent<FloatingText>();
        if (ftScript != null)
        {
            ftScript.SetText(message);
            ftScript.SetColor(color);
        }
        else
        {
            Debug.LogWarning("FloatingText component not found on prefab!");
        }
        
        Destroy(ft, 1.5f);
    }

    public void ApplyPenalty()
    {
        currentPenaltyCount++;
        
        Debug.Log($"⚠️ PENALTY KE-{currentPenaltyCount} dari {maxPenaltyCount}");
        
        // CEK APAKAH SUDAH MENCAPAI BATAS MAX PENALTY
        if (currentPenaltyCount >= maxPenaltyCount)
        {
            Debug.Log($"💀 GAME OVER! Sudah kena penalty {currentPenaltyCount} kali (maksimal {maxPenaltyCount})");
            // GABUNG JADI SATU: GAME OVER + Penalty
            ShowWarningText($"GAME OVER!\nPenalty {currentPenaltyCount}/{maxPenaltyCount}", Color.red);
            
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
            return;
        }
        
        // CEK APAKAH UANG CUKUP
        if (MoneyManager.instance != null && MoneyManager.instance.money >= penaltyMoney)
        {
            MoneyManager.instance.SpendMoney(penaltyMoney);
            Debug.Log($"💸 DENDA! -{penaltyMoney} | Sisa uang: {MoneyManager.instance.money} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
            
            // GABUNG JADI SATU TEKS: -1000 di atas, Penalty (1/3) di bawah
            ShowWarningText($"-{penaltyMoney}\nPenalty {currentPenaltyCount}/{maxPenaltyCount}", Color.red);
            
            SaveProgressSimple();
            
            if (MoneyManager.instance.money <= 0)
            {
                Debug.Log($"💀 GAME OVER! Uang habis!");
                ShowWarningText($"UANG HABIS!\nGAME OVER!", Color.red);
                if (GameManager.instance != null)
                    GameManager.instance.GameOver();
            }
        }
        else
        {
            Debug.Log($"💀 GAME OVER! Uang tidak cukup bayar denda {penaltyMoney}");
            ShowWarningText($"UANG TIDAK CUKUP!\nButuh {penaltyMoney}", Color.red);
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
        }
    }

    string GetGateTypeName()
    {
        switch (level)
        {
            case 1: return "";
            case 2: return "E-Money";
            case 3: return "RFID";
            case 4: return "MLFF";
            default: return "";
        }
    }

    void UpdateUI()
    {
        if (isUnlocked)
        {
            unlockButton.SetActive(false);
            levelText.text = GetGateTypeName();

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
                upgradeText.text = "(" + cost + ")";
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
            levelText.text = "Locked";
            unlockText.text = "(" + unlockCost + ")";
            bool canUnlock = MoneyManager.instance != null && MoneyManager.instance.money >= unlockCost;
            SetButtonState(unlockButton.GetComponent<Button>(), canUnlock);
        }
        UpdatePayButtonState();
    }

    void SetButtonState(Button btn, bool canAfford)
    {
        if (btn == null) return;
        btn.interactable = canAfford;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = canAfford ? 1f : 0.5f;
    }

    int GetUpgradeCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(costMultiplier, level - 1));
    bool IsAuto() => level >= 2;
    float GetDelay() => level == 1 ? 1.5f : (level == 2 ? 1.0f : (level == 3 ? 0.5f : 0.1f));

    public void UnlockGate()
    {
        if (isUnlocked) return;
        
        if (MoneyManager.instance != null && MoneyManager.instance.money >= unlockCost)
        {
            MoneyManager.instance.SpendMoney(unlockCost);
            isUnlocked = true;
            GateVisualManager visual =
                FindFirstObjectByType<GateVisualManager>();

            if (visual != null)
            {
                visual.RefreshGateVisual();
            }
            
            // MAINTAIN SOUND
            if (audioSource != null && unlockSound != null)
                audioSource.PlayOneShot(
                    unlockSound,
                    BGMManager.SFXVolume
                );
            
            // TAMPILKAN FLOATING TEXT (SUDAH OK)
            ShowWarningText("GATE OPEN", Color.cyan);
            
            SaveProgressSimple();
            spawner?.SetActive(true);
            spawner?.StartSpawner();
            UpdateUI();
            Debug.Log($"🔓 GATE DIBUKA!");
        }
        else
        {
            ShowWarningText($"UANG TIDAK CUKUP!\nButuh {unlockCost}", Color.red);
        }
    }

    public void UpgradeGate()
    {
        if (level >= maxLevel)
        {
            ShowWarningText("MAX LEVEL!", Color.yellow);
            return;
        }
        
        int cost = GetUpgradeCost();
        
        if (MoneyManager.instance != null && MoneyManager.instance.money >= cost)
        {
            MoneyManager.instance.SpendMoney(cost);
            level++;

            if (spawner != null)
            {
                spawner.SetSpawnRateByLevel(level);
            }
            
            if (audioSource != null && upgradeSound != null)
                audioSource.PlayOneShot(
                    upgradeSound,
                    BGMManager.SFXVolume
                );
            
            // TAMPILKAN FLOATING TEXT
            ShowWarningText($"UPGRADE\nLEVEL {level}", Color.green);
            
            SaveProgressSimple();
            UpdateUI();
            Debug.Log($"⬆️ UPGRADE ke Level {level}");
            
            if (IsAuto() && carQueue.Count > 0) 
                TryProcessNextCar();
        }
        else
        {
            ShowWarningText($"UANG TIDAK CUKUP!\nButuh {cost}", Color.red);
        }
    }

    void TryProcessNextCar()
    {
        if (!isProcessing && carQueue.Count > 0)
            StartCoroutine(ProcessCar());
    }

    public void PayAndRelease()
    {
        if (level == 1) TryProcessNextCar();
    }

    void UpdateTrafficUIPosition()
    {
        if (trafficText == null || mainCamera == null)
            return;

        Vector3 worldPos = transform.position + Vector3.up * 4f;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        trafficText.transform.position = screenPos;
    }

    void IncreaseAllExistingCarsSpeed(float multiplier)
    {
        CarAI[] allCars = FindObjectsByType<CarAI>(FindObjectsSortMode.None);
        int count = 0;
        
        foreach (CarAI car in allCars)
        {
            if (car != null && !car.IsInQueue) // Mobil yang belum di queue
            {
                float currentSpeed = car.GetCurrentSpeed();
                float newSpeed = currentSpeed * multiplier;
                newSpeed = Mathf.Min(newSpeed, 35f); // Batas maksimal speed 35
                car.SetTempRestoreSpeed(newSpeed, 4f); // Cepat selama 4 detik
                count++;
            }
        }
        
        Debug.Log($"🏎️ Increased speed of {count} existing cars (multiplier: {multiplier}x)");
    }
}