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
    public int maxPenaltyCount = 3; // Maksimal kena penalty sebelum game over
    private int currentPenaltyCount = 0; // Counter penalty saat ini

    float jamDetectTimer = 0f;
    public float jamThreshold = 2f; // harus macet 2 detik baru dianggap macet

    [Header("Traffic UI")]
    public TextMeshProUGUI trafficText;
    public CanvasGroup trafficCanvasGroup;


    void Start()
    {
        UpdateSpawnerState();
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
    }

    void UpdatePayButtonState()
    {
        if (payButtonComponent == null) return;

        bool hasCar = carQueue.Count > 0;
        bool canPay = hasCar && !isProcessing;

        payButtonComponent.interactable = canPay;

        CanvasGroup cg = payButtonComponent.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = canPay ? 1f : 0.5f;
        }
    }

    void HandleTrafficPressure()
    {
        if (!isUnlocked)
        {
            // 🔒 GATE MASIH TUTUP
            if (trafficText != null)
            {
                trafficText.text = "TUTUP";
                trafficText.color = Color.gray;

                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 1f;
            }

            return;
        }
        
        // CEK APAKAH SPAWNER LAGI MACET
        bool unsafeToSpawn = !spawner.IsSafeToSpawn();

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
        
        // DEBUG
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🚦 Status: {(isTrafficJam ? "MACET" : "LANCAR")} | Timer: {(isTrafficJam ? currentCountdown.ToString("F1") : "0")} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
        }
        
        // =========================
        // 🔥 UI MACET (INI YANG DITAMBAHIN)
        // =========================
        if (trafficText != null)
        {
            if (isTrafficJam)
            {
                trafficText.text = "MACET!\n" + Mathf.Ceil(currentCountdown).ToString();

                // warna berubah
                if (currentCountdown <= 1.5f)
                    trafficText.color = Color.red;
                else
                    trafficText.color = Color.yellow;

                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 1f;
            }
            else
            {
                if (trafficCanvasGroup != null)
                    trafficCanvasGroup.alpha = 0f;
            }
        }
        // =========================
        
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
        // TAMBAH COUNTER PENALTY
        currentPenaltyCount++;
        
        Debug.Log($"⚠️ PENALTY KE-{currentPenaltyCount} dari {maxPenaltyCount}");
        
        // CEK APAKAH UDAH MELEWATI BATAS PENALTY
        if (currentPenaltyCount >= maxPenaltyCount)
        {
            Debug.Log($"💀 GAME OVER! Sudah kena penalty {currentPenaltyCount} kali (maksimal {maxPenaltyCount})");
            ShowWarningText($"PENALTY KE-{currentPenaltyCount}! GAME OVER!", Color.red);
            GameManager.instance.GameOver();;
            return;
        }
        
        // KALO BELUM SAMPE BATAS, KURANGIN DUIT
        if (GameManager.instance.money >= penaltyMoney)
        {
            GameManager.instance.SpendMoney(penaltyMoney);
            Debug.Log($"💸 DENDA! -{penaltyMoney} | Sisa uang: {GameManager.instance.money} | Penalty: {currentPenaltyCount}/{maxPenaltyCount}");
            
            // Tampilkan penalty text
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
            
            // CEK JUGA KALO DUIT HABIS
            if (GameManager.instance.money <= 0)
            {
                Debug.Log($"💀 GAME OVER! Uang habis!");
                GameManager.instance.GameOver();;
            }
        }
        else
        {
            Debug.Log($"💀 GAME OVER! Uang tidak cukup bayar denda {penaltyMoney}");
            GameManager.instance.GameOver();;
        }
    }


    // RESET PENALTY COUNTER (bisa dipanggil kalo misalkan upgrade atau unlock)
    public void ResetPenaltyCounter()
    {
        currentPenaltyCount = 0;
        Debug.Log($"🔄 Penalty counter di-reset! Sekarang: {currentPenaltyCount}/{maxPenaltyCount}");
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
                maxLevelText.SetActive(true);
            }
            else
            {
                upgradeButton.SetActive(true);
                maxLevelText.SetActive(false);

                int cost = GetUpgradeCost();
                upgradeText.text = "Upgrade\n(" + cost + ")";

                bool canUpgrade = GameManager.instance.money >= cost;
                SetButtonState(upgradeButton.GetComponent<Button>(), canUpgrade);
            }

            payButton.SetActive(level == 1);
        }
        else
        {
            unlockButton.SetActive(true);
            upgradeButton.SetActive(false);
            payButton.SetActive(false);
            maxLevelText.SetActive(false);

            levelText.text = "Locked";
            unlockText.text = "Buka Pintu\n(" + unlockCost + ")";

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

        if (GameManager.instance.money >= unlockCost)
        {
            GameManager.instance.SpendMoney(unlockCost);
            isUnlocked = true;
            
            // Reset penalty counter pas unlock gate
            ResetPenaltyCounter();
            
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

            Debug.Log($"🔓 GATE DIBUKA! Uang tersisa: {GameManager.instance.money}");
            UpdateSpawnerState();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Uang tidak cukup! Butuh: {unlockCost}, Punya: {GameManager.instance.money}");
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

        if (GameManager.instance.money >= cost)
        {
            GameManager.instance.SpendMoney(cost);
            level++;
            
            // Reset penalty counter pas upgrade (opsional, bisa dihapus kalo ga mau)
            ResetPenaltyCounter();
            
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
                ftScript.SetColor(Color.green);
            }

            Debug.Log($"⬆️ GATE UPGRADE ke Level {level} | Uang tersisa: {GameManager.instance.money}");
            UpdateUI();

            if (IsAuto())
            {
                TryProcessNextCar();
            }
        }
        else
        {
            Debug.Log($"Uang tidak cukup upgrade! Butuh: {cost}, Punya: {GameManager.instance.money}");
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
                car.StartPaying();
                carQueue.Enqueue(car);
                UpdatePayButtonState();
                
                Debug.Log($"🚗 MOBIL MASUK | Total antrian: {carQueue.Count}");

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
        GameManager.instance.AddMoney(money);
        audioSource.PlayOneShot(moneySound, 0.5f);
        
        Debug.Log($"💰 +{money} | Total: {GameManager.instance.money}");

        Vector3 worldPos = transform.position + Vector3.up * 2f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        GameObject ft = Instantiate(floatingTextPrefab, canvas.transform);
        RectTransform rt = ft.GetComponent<RectTransform>();
        rt.position = screenPos;

        FloatingText ftScript = ft.GetComponent<FloatingText>();
        if (ftScript != null)
        {
            ftScript.SetText("+" + money);
        }

        car.StopPaying();
        
        yield return new WaitForSeconds(0.3f);

        isProcessing = false;
        UpdatePayButtonState();

        if (IsAuto() && carQueue.Count > 0)
        {
            TryProcessNextCar();
        }
    }

    void UpdateTrafficUIPosition()
    {
        if (trafficText == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 4f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        trafficText.transform.position = screenPos;
    }

    
}