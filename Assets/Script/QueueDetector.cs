using UnityEngine;
using System.Collections.Generic;

public class QueueDetector : MonoBehaviour
{
    public List<CarAI> queuedCars = new List<CarAI>();
    
    [Header("Settings")]
    public bool showDebugLogs = true;
    public float debugLogInterval = 1f;
    private float lastDebugLogTime = 0f;
    
    [Header("Queue Limits")]
    public int maxQueueSize = 10;
    
    public System.Action<int> OnQueueChanged;
    public System.Action<CarAI> OnCarEntered;
    public System.Action<CarAI> OnCarExited;
    
    private BoxCollider boxCollider;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (!isInitialized)
        {
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogWarning($"⚠️ QueueDetector on {gameObject.name} tidak memiliki BoxCollider! Menambahkan otomatis...");
                boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
            isInitialized = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;
        
        CarAI car = other.GetComponentInParent<CarAI>();
        
        if (car == null) return;
        if (car.HasReachedDestination()) return;
        if (queuedCars.Contains(car)) return;
        if (queuedCars.Count >= maxQueueSize) return;
        
        queuedCars.Add(car);
        car.IsInQueue = true;
        
        if (showDebugLogs && Time.time - lastDebugLogTime > debugLogInterval)
        {
            // HAPUS queueNumber dari sini
            Debug.Log($"🚗 MASUK ANTRIAN: {car.carID} | Total: {queuedCars.Count}/{maxQueueSize}");
            lastDebugLogTime = Time.time;
        }
        
        OnQueueChanged?.Invoke(queuedCars.Count);
        OnCarEntered?.Invoke(car);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Car")) return;
        
        CarAI car = other.GetComponentInParent<CarAI>();
        
        if (car == null) return;
        
        if (queuedCars.Contains(car))
        {
            queuedCars.Remove(car);
            car.IsInQueue = false;
            
            if (showDebugLogs && Time.time - lastDebugLogTime > debugLogInterval)
            {
                Debug.Log($"🚙 KELUAR ANTRIAN: {car.carID} | Sisa: {queuedCars.Count}/{maxQueueSize}");
                lastDebugLogTime = Time.time;
            }
            
            OnQueueChanged?.Invoke(queuedCars.Count);
            OnCarExited?.Invoke(car);
        }
    }
    
    public CarAI GetFirstInQueue()
    {
        CleanNullReferences();
        if (queuedCars.Count > 0 && queuedCars[0] != null)
            return queuedCars[0];
        return null;
    }
    
    public CarAI GetLastInQueue()
    {
        CleanNullReferences();
        if (queuedCars.Count > 0 && queuedCars[queuedCars.Count - 1] != null)
            return queuedCars[queuedCars.Count - 1];
        return null;
    }
    
    public void ClearQueue()
    {
        foreach (var car in queuedCars)
        {
            if (car != null)
                car.IsInQueue = false;
        }
        queuedCars.Clear();
        
        OnQueueChanged?.Invoke(0);
        
        if (showDebugLogs)
            Debug.Log("🧹 Queue detector cleared");
    }
    
    public bool RemoveFromQueue(CarAI car)
    {
        if (car == null) return false;
        
        if (queuedCars.Contains(car))
        {
            queuedCars.Remove(car);
            car.IsInQueue = false;
            
            OnQueueChanged?.Invoke(queuedCars.Count);
            OnCarExited?.Invoke(car);
            
            return true;
        }
        return false;
    }
    
    public bool IsInQueue(CarAI car)
    {
        return car != null && queuedCars.Contains(car);
    }
    
    public int GetQueuePosition(CarAI car)
    {
        if (car == null) return -1;
        return queuedCars.IndexOf(car);
    }
    
    public List<CarAI> GetAllQueuedCars()
    {
        CleanNullReferences();
        return new List<CarAI>(queuedCars);
    }
    
    public int GetQueueCount()
    {
        CleanNullReferences();
        return queuedCars.Count;
    }
    
    public bool IsQueueFull()
    {
        return queuedCars.Count >= maxQueueSize;
    }
    
    public bool IsQueueEmpty()
    {
        return queuedCars.Count == 0;
    }
    
    public void CleanNullReferences()
    {
        int removedCount = queuedCars.RemoveAll(car => car == null);
        if (removedCount > 0 && showDebugLogs)
            Debug.Log($"🧹 Dibersihkan {removedCount} referensi null dari antrian");
    }
    
    public int GetQueueCountForSave()
    {
        CleanNullReferences();
        return queuedCars.Count;
    }
    
    public string GetQueueDataForSave()
    {
        CleanNullReferences();
        
        List<string> carDataList = new List<string>();
        
        foreach (CarAI car in queuedCars)
        {
            if (car != null)
            {
                // HAPUS queueNumber dari sini
                string carData = $"{car.carID}|{(int)car.category}";
                carDataList.Add(carData);
            }
        }
        
        return string.Join(",", carDataList);
    }
    
    public void PrintQueueStatus()
    {
        CleanNullReferences();
        
        Debug.Log($"=== QUEUE STATUS ({gameObject.name}) ===");
        Debug.Log($"Total mobil: {queuedCars.Count}/{maxQueueSize}");
        
        for (int i = 0; i < queuedCars.Count; i++)
        {
            if (queuedCars[i] != null)
            {
                // HAPUS queueNumber dari sini
                Debug.Log($"  [{i}] {queuedCars[i].carID} | Category: {queuedCars[i].category}");
            }
            else
            {
                Debug.Log($"  [{i}] NULL REFERENCE");
            }
        }
        Debug.Log($"=================================");
    }
    
    private void Update()
    {
        if (Time.frameCount % 600 == 0)
        {
            CleanNullReferences();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();
            
        if (boxCollider != null)
        {
            float fillPercent = queuedCars.Count / (float)maxQueueSize;
            Color gizmoColor = Color.green;
            
            if (fillPercent > 0.7f)
                gizmoColor = Color.red;
            else if (fillPercent > 0.4f)
                gizmoColor = Color.yellow;
            
            gizmoColor.a = 0.3f;
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(transform.position, boxCollider.size);
        }
    }
    
    private void OnValidate()
    {
        if (maxQueueSize <= 0)
            maxQueueSize = 1;
    }
}