using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carPrefabs;
    public Transform spawnPoint;
    public Transform[] waypoints;

    [Header("Spawning Settings")]
    public float spawnInterval = 2f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 4f;
    
    [Header("Queue Detection")]
    public float detectionDistance = 5f;
    public int maxCarsInQueue = 3;
    public LayerMask carLayer;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private bool isActive = false;
    
    // Track spawned cars for restore purposes
    private List<GameObject> spawnedCars = new List<GameObject>();

    void Start()
    {
        currentSpawnInterval = spawnInterval;

        if (carLayer == 0)
        {
            carLayer = LayerMask.GetMask("Car");
        }
        
        if (showDebugLogs)
            Debug.Log($"🏭 CarSpawner initialized on {gameObject.name}");
    }

    public void SetActive(bool active)
    {
        isActive = active;
        
        if (showDebugLogs)
            Debug.Log($"🏭 CarSpawner.SetActive({active}) called for {gameObject.name}");

        if (isActive)
        {
            if (spawnCoroutine == null)
            {
                if (showDebugLogs)
                    Debug.Log("🏭 Starting spawn coroutine");
                spawnCoroutine = StartCoroutine(SpawnWithDetection());
            }
        }
        else
        {
            if (spawnCoroutine != null)
            {
                if (showDebugLogs)
                    Debug.Log("🏭 Stopping spawn coroutine");
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }

    public bool IsSafeToSpawn()
    {
        if (!isActive) 
        {
            if (showDebugLogs && Time.frameCount % 120 == 0)
                Debug.Log($"🏭 CarSpawner.IsSafeToSpawn() = FALSE (inactive)");
            return false;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is null!");
            return false;
        }
        
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance, carLayer);
        
        int activeCarsCount = 0;
        
        foreach (Collider car in carsInRange)
        {
            if (car == null) continue;
            
            CarAI carAI = car.GetComponent<CarAI>();
            if (carAI != null)
            {
                if (!carAI.HasReachedDestination() && !carAI.IsInQueue)
                {
                    activeCarsCount++;
                }
            }
            else
            {
                activeCarsCount++;
            }
        }
        
        float distanceToNearestCar = GetDistanceToNearestCar();
        
        bool isQueueNotFull = activeCarsCount < maxCarsInQueue;
        bool hasEnoughSpace = distanceToNearestCar >= 3f;
        
        bool safeToSpawn = isQueueNotFull && hasEnoughSpace;
        
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"🏭 Spawn Check - ActiveCars: {activeCarsCount}/{maxCarsInQueue}, Distance: {distanceToNearestCar:F2}, Safe: {safeToSpawn}");
        }
        
        return safeToSpawn;
    }

    IEnumerator SpawnWithDetection()
    {
        if (showDebugLogs)
            Debug.Log("🏭 SpawnWithDetection coroutine started");
        
        while (isActive)
        {
            bool safeToSpawn = IsSafeToSpawn();
            
            if (safeToSpawn)
            {
                if (showDebugLogs)
                    Debug.Log("🏭 Spawning new car...");
                SpawnCar();
                currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }
            else
            {
                currentSpawnInterval = 0.5f;
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }

        if (showDebugLogs)
            Debug.Log("🏭 SpawnWithDetection coroutine ended");
        spawnCoroutine = null;
    }

    float GetDistanceToNearestCar()
    {
        if (spawnPoint == null) return detectionDistance;
        
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance, carLayer);
        float closestDistance = detectionDistance;
        
        foreach (Collider car in carsInRange)
        {
            if (car != null && car.gameObject != null)
            {
                float distance = Vector3.Distance(spawnPoint.position, car.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }
        
        return closestDistance;
    }

    void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned!");
            return;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint not assigned!");
            return;
        }
        
        int index = Random.Range(0, carPrefabs.Length);
        GameObject car = Instantiate(carPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        CarAI ai = car.GetComponent<CarAI>();
        if (ai != null)
        {
            ai.waypoints = waypoints;
            // Generate unique ID if needed
            if (string.IsNullOrEmpty(ai.carID))
            {
                ai.carID = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            spawnedCars.Add(car);
            
            if (showDebugLogs)
                Debug.Log($"🏭 Car spawned: {ai.carID} (Category: {ai.category})");
        }
        else
        {
            Debug.LogWarning("Spawned car has no CarAI component!");
        }
    }

    // ==================== NEW METHOD FOR SIMPLE RESTORE ====================
    
    /// <summary>
    /// Spawn random car for queue restoration (SIMPLE VERSION)
    /// </summary>
    public void SpawnRandomCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogWarning("No car prefabs assigned for random spawn!");
            return;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is null!");
            return;
        }
        
        int index = Random.Range(0, carPrefabs.Length);
        GameObject car = Instantiate(carPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        CarAI ai = car.GetComponent<CarAI>();
        if (ai != null)
        {
            ai.waypoints = waypoints;
            ai.IsInQueue = true; // IMPORTANT: Set to true so car will be detected as in queue
            
            // Generate unique ID if needed
            if (string.IsNullOrEmpty(ai.carID))
            {
                ai.carID = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            spawnedCars.Add(car);
            
            if (showDebugLogs)
                Debug.Log($"🚗 RANDOM SPAWN FOR QUEUE: {ai.carID} (Category: {ai.category})");
        }
        else
        {
            Debug.LogWarning("Spawned car has no CarAI component!");
        }
    }
    
    /// <summary>
    /// Spawn multiple random cars for queue restoration
    /// </summary>
    public void SpawnMultipleRandomCars(int count)
    {
        if (count <= 0) return;
        
        Debug.Log($"🚗 Spawning {count} random cars for queue restoration");
        
        for (int i = 0; i < count; i++)
        {
            SpawnRandomCar();
        }
    }

    // ==================== EXISTING SPAWN SPECIFIC CAR (KEEP FOR COMPATIBILITY) ====================
    
    public void SpawnSpecificCar(string carID)
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogWarning("No car prefabs assigned!");
            SpawnRandomCar(); // Fallback to random
            return;
        }

        GameObject selectedPrefab = null;
        CarAI selectedAI = null;

        foreach (GameObject prefab in carPrefabs)
        {
            if (prefab == null) continue;
            
            CarAI ai = prefab.GetComponent<CarAI>();

            if (ai != null && ai.carID == carID)
            {
                selectedPrefab = prefab;
                selectedAI = ai;
                break;
            }
        }

        if (selectedPrefab == null)
        {
            // Fallback to random spawn
            Debug.LogWarning($"❌ Prefab tidak ditemukan: {carID}, spawning random car instead");
            SpawnRandomCar();
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint is null!");
            return;
        }

        GameObject car = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        CarAI aiSpawned = car.GetComponent<CarAI>();

        if (aiSpawned != null)
        {
            aiSpawned.waypoints = waypoints;
            aiSpawned.carID = carID;
            aiSpawned.IsInQueue = true;
            aiSpawned.category = selectedAI != null ? selectedAI.category : CarAI.CarCategory.Category1;
            
            spawnedCars.Add(car);
            
            Debug.Log($"🚗 RESTORE SPAWN: {carID} (Category: {aiSpawned.category})");
        }
        else
        {
            Debug.LogWarning($"❌ Spawned car {carID} has no CarAI component!");
        }
    }

    // ==================== CLEANUP METHODS ====================
    
    public void ClearAllSpawnedCars()
    {
        int count = 0;
        foreach (GameObject car in spawnedCars)
        {
            if (car != null)
            {
                Destroy(car);
                count++;
            }
        }
        spawnedCars.Clear();
        
        if (showDebugLogs)
            Debug.Log($"🏭 Cleared {count} spawned cars");
    }

    public int GetSpawnedCarCount()
    {
        // Clean up null references
        spawnedCars.RemoveAll(car => car == null);
        return spawnedCars.Count;
    }
    
    /// <summary>
    /// Get random car prefab (useful for debugging)
    /// </summary>
    public GameObject GetRandomCarPrefab()
    {
        if (carPrefabs == null || carPrefabs.Length == 0) return null;
        return carPrefabs[Random.Range(0, carPrefabs.Length)];
    }
    
    /// <summary>
    /// Get total available car types
    /// </summary>
    public int GetAvailableCarTypes()
    {
        return carPrefabs != null ? carPrefabs.Length : 0;
    }

    // ==================== EXISTING METHODS (UNCHANGED) ====================
    
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, detectionDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.5f);
        }
    }

    public bool IsSpawnAreaClear()
    {
        if (spawnPoint == null) return true;
        
        Collider[] hits = Physics.OverlapSphere(
            spawnPoint.position,
            detectionDistance,
            carLayer
        );

        return hits.Length == 0;
    }

    public void StopSpawner()
    {
        if (showDebugLogs)
            Debug.Log($"🏭 StopSpawner called for {gameObject.name}");
        
        isActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void StartSpawner()
    {
        if (showDebugLogs)
            Debug.Log($"🏭 StartSpawner called for {gameObject.name}, isActive: {isActive}");
        
        if (isActive) 
        {
            if (showDebugLogs)
                Debug.Log("🏭 Spawner already active");
            return;
        }

        isActive = true;

        if (spawnCoroutine == null)
        {
            if (showDebugLogs)
                Debug.Log("🏭 Starting spawn coroutine from StartSpawner");
            spawnCoroutine = StartCoroutine(SpawnWithDetection());
        }
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    void OnDestroy()
    {
        ClearAllSpawnedCars();
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}