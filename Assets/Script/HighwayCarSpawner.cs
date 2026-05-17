using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HighwayCarSpawner : MonoBehaviour
{
    [Header("Car Prefabs")]
    public GameObject[] carPrefabs;
    
    [Header("Spawn Point & Waypoints")]
    public Transform spawnPoint;
    public Transform[] waypoints;
    
    [Header("Spawning Settings")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 4f;
    public bool isActive = true;
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Spawn Safety")]
    public float detectionDistance = 5f;
    public LayerMask carLayer;
    
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedCars = new List<GameObject>();

    [Header("Lane")]
    public LaneController laneController;
    
    void Start()
    {
        currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

        if (carLayer == 0)
        {
            carLayer = LayerMask.GetMask("Car");
        }
        
        if (isActive)
        {
            StartSpawner();
        }
    }
    
    public void StartSpawner()
    {
        if (spawnCoroutine != null) return;
        spawnCoroutine = StartCoroutine(SpawnLoop());
        
        if (showDebugLogs)
            Debug.Log($"🚗 HighwayCarSpawner started on {gameObject.name}");
    }
    
    public void StopSpawner()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            
            if (showDebugLogs)
                Debug.Log($"🛑 HighwayCarSpawner stopped on {gameObject.name}");
        }
    }
    
    IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(currentSpawnInterval);
            if (IsSafeToSpawn())
            {
                SpawnCar();
            }
            currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    bool IsSafeToSpawn()
    {
        if (spawnPoint == null) return false;

        Collider[] hits = Physics.OverlapSphere(
            spawnPoint.position,
            detectionDistance,
            carLayer
        );

        return hits.Length == 0;
    }
    
    void SpawnCar()
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogWarning("No car prefabs assigned!");
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
        ai.currentLane = laneController;
        if (ai != null)
        {
            ai.waypoints = waypoints;
            ai.currentMode = CarAI.CarMode.Highway;
            
            spawnedCars.Add(car);
            
            if (showDebugLogs)
                Debug.Log($"🚗 Car spawned: {ai.carID}");
        }
        else
        {
            Debug.LogWarning("Spawned car has no HighwayCarAI component! Adding one...");
            HighwayCarAI newAI = car.AddComponent<HighwayCarAI>();
            newAI.waypoints = waypoints;
            spawnedCars.Add(car);
        }
    }
    
    public void ClearAllCars()
    {
        foreach (GameObject car in spawnedCars)
        {
            if (car != null) Destroy(car);
        }
        spawnedCars.Clear();
    }
}