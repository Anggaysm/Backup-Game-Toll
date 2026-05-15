using UnityEngine;
using TMPro;
using System.Collections;

public class HighwayRushHourManager : MonoBehaviour
{
    public static HighwayRushHourManager Instance;
    
    [Header("Timing Settings")]
    public float normalModeDuration = 60f;
    public float rushHourDuration = 10f;
    
    [Header("Spawner Settings")]
    public HighwayCarSpawner[] allSpawners;
    
    [Header("Car Speed Settings")]
    public float normalCarSpeedMin = 8f;
    public float normalCarSpeedMax = 12f;
    public float rushHourCarSpeedMin = 16f;
    public float rushHourCarSpeedMax = 20f;
    
    [Header("Spawn Interval Settings")]
    public float normalSpawnMin = 1f;
    public float normalSpawnMax = 4f;
    public float rushHourSpawnMin = 0.5f;
    public float rushHourSpawnMax = 2f;

    [Header("Breakdown Settings")]
    public float normalBreakdownChance = 0.01f;
    public float rushHourBreakdownChance = 0.05f;
    
    [Header("UI")]
    public TextMeshProUGUI timerText;  // Cuma 1 text untuk semua
    
    private bool isRushHour = false;
    private float currentTimer;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
        
    void Start()
    {   
        currentTimer = normalModeDuration;
        isRushHour = false;
        
        // Cari semua spawner dulu
        if (allSpawners == null || allSpawners.Length == 0)
        {
            allSpawners = FindObjectsByType<HighwayCarSpawner>(FindObjectsSortMode.None);
        }
        
        // Set ke normal dulu (jangan rush hour)
        SetToNormalMode();
        
        // Matikan update dulu
        enabled = false; // 🔥 IMPORTANT: Rush hour belum aktif
        
        Debug.Log("⏸️ RushHourManager disabled, waiting for loading to complete...");
    }

    public void StartRushHourSystem()
    {
        enabled = true;  // Aktifkan update
        currentTimer = normalModeDuration;
        isRushHour = false;
        
        SetToNormalMode();
        
        Debug.Log("✅ RushHourSystem STARTED! Countdown begins...");
    }
    
    void SetToNormalMode()
    {
        foreach (HighwayCarSpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.minSpawnInterval = normalSpawnMin;
                spawner.maxSpawnInterval = normalSpawnMax;
            }
        }
        
        UpdateAllCarSpeeds();
        
        if (timerText != null)
        {
            timerText.text = $"Next Rush: {Mathf.FloorToInt(normalModeDuration / 60):00}:00";
            timerText.color = Color.white;
        }
    }
    
    void Update()
    {
        // Cuma jalan kalau enabled = true (setelah StartRushHourSystem dipanggil)
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
            
            if (currentTimer <= 0 && !isRushHour)
            {
                StartRushHour();
                UpdateBreakdownChance();
            }
        }
        
        if (isRushHour)
        {
            UpdateAllCarSpeeds();
        }
    }
    
    void StartRushHour()
    {
        isRushHour = true;
        currentTimer = rushHourDuration;
        
        // Ubah spawn interval semua spawner jadi 2x lebih cepat
        foreach (HighwayCarSpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.minSpawnInterval = rushHourSpawnMin;
                spawner.maxSpawnInterval = rushHourSpawnMax;
                Debug.Log($"🚗 Spawner {spawner.name} speed up: {rushHourSpawnMin}-{rushHourSpawnMax}s");
            }
        }
        
        // Ubah kecepatan semua mobil yang sudah ada
        UpdateAllCarSpeeds();
        
        Debug.Log($"🚨 RUSH HOUR STARTED! {rushHourDuration} seconds of chaos!");
    }
    
    void EndRushHour()
    {
        isRushHour = false;
        currentTimer = normalModeDuration;
        
        // Kembalikan spawn interval ke normal
        foreach (HighwayCarSpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.minSpawnInterval = normalSpawnMin;
                spawner.maxSpawnInterval = normalSpawnMax;
                Debug.Log($"🚗 Spawner {spawner.name} back to normal: {normalSpawnMin}-{normalSpawnMax}s");
            }
        }
        
        // Kembalikan kecepatan mobil ke normal
        UpdateAllCarSpeeds();
        
        Debug.Log($"✅ RUSH HOUR ENDED. Back to normal for {normalModeDuration}s");
    }
    
    void UpdateAllCarSpeeds()
    {
        CarAI[] cars = FindObjectsByType<CarAI>(FindObjectsSortMode.None);
        
        foreach (CarAI car in cars)
        {
            if (car != null)
            {
                if (isRushHour)
                {
                    car.SetSpeed(Random.Range(rushHourCarSpeedMin, rushHourCarSpeedMax));
                }
                else
                {
                    car.SetSpeed(Random.Range(normalCarSpeedMin, normalCarSpeedMax));
                }
            }
        }
        
        if (cars.Length > 0)
        {
            Debug.Log($"🏎️ Updated {cars.Length} car speeds (Rush Hour: {isRushHour})");
        }
    }
    
    void UpdateTimerUI()
    {
        if (timerText == null) return;
        
        if (isRushHour)
        {
            // RUSH HOUR MODE
            int seconds = Mathf.CeilToInt(currentTimer);
            timerText.text = $"RUSH HOUR: {seconds}s";
            timerText.color = Color.red;
            timerText.fontSize = 20;
            
            if (currentTimer <= 0)
            {
                EndRushHour();
                UpdateBreakdownChance();
            }
        }
        else
        {
            // NORMAL MODE - Countdown ke Rush Hour
            int minutes = Mathf.FloorToInt(currentTimer / 60);
            int seconds = Mathf.FloorToInt(currentTimer % 60);
            timerText.text = $"Next Rush: {minutes:00}:{seconds:00}";
            timerText.color = Color.white;
            timerText.fontSize = 20;
        }
    }
    
    // Public method untuk reset (panggil dari GameManager saat restart)
    public void ResetRushHour()
    {
        StopAllCoroutines();
        isRushHour = false;
        currentTimer = normalModeDuration;
        
        foreach (HighwayCarSpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.minSpawnInterval = normalSpawnMin;
                spawner.maxSpawnInterval = normalSpawnMax;
            }
        }
        
        UpdateAllCarSpeeds();
        
        Debug.Log("🔄 RushHourManager reset");
    }

    void UpdateBreakdownChance()
    {
        CarAI[] cars =
            FindObjectsByType<CarAI>(FindObjectsSortMode.None);

        foreach (CarAI car in cars)
        {
            if (car != null)
            {
                if (isRushHour)
                {
                    car.breakdownChance =
                        rushHourBreakdownChance;
                }
                else
                {
                    car.breakdownChance =
                        normalBreakdownChance;
                }
            }
        }
    }
}