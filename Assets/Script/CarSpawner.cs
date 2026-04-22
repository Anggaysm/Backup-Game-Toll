using UnityEngine;
using System.Collections;

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
    
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;

    private bool isActive = false;

    void Start()
    {
        currentSpawnInterval = spawnInterval;

        if (carLayer == 0)
        {
            carLayer = LayerMask.GetMask("Car");
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (isActive)
        {
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnWithDetection());
            }
        }
        else
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }

    // 🔥 METHOD PUBLIC BUAT TOLL GATE - NGEECEK APAKAH AMAN UNTUK SPAWN
    public bool IsSafeToSpawn()
    {
        if (!isActive) return false;
        
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance, carLayer);
        
        int activeCarsCount = 0;
        
        foreach (Collider car in carsInRange)
        {
            CarAI carAI = car.GetComponent<CarAI>();
            if (carAI != null)
            {
                if (!carAI.HasReachedDestination())
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
        
        return isQueueNotFull && hasEnoughSpace;
    }

    IEnumerator SpawnWithDetection()
    {
        while (isActive)
        {
            if (IsSafeToSpawn())
            {
                SpawnCar();
                currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }
            else
            {
                currentSpawnInterval = 0.5f;
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }

        spawnCoroutine = null;
    }

    float GetDistanceToNearestCar()
    {
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance, carLayer);
        float closestDistance = detectionDistance;
        
        foreach (Collider car in carsInRange)
        {
            if (car.gameObject != null)
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
        if (carPrefabs == null || carPrefabs.Length == 0) return;
        if (spawnPoint == null) return;
        
        int index = Random.Range(0, carPrefabs.Length);
        GameObject car = Instantiate(carPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        CarAI ai = car.GetComponent<CarAI>();
        if (ai != null)
        {
            ai.waypoints = waypoints;
        }
    }

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
}