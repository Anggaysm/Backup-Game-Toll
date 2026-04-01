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
    public float detectionDistance = 5f;      // Jarak deteksi antrian dari spawn point
    public int maxCarsInQueue = 3;             // Maksimal mobil dalam antrian sebelum spawn berhenti
    public LayerMask carLayer;                 // Layer untuk deteksi mobil
    
    private bool isSpawningEnabled = true;
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;

    void Start()
    {
        currentSpawnInterval = spawnInterval;
        
        // Set layer mask untuk deteksi mobil (sesuaikan dengan layer mobil Anda)
        if (carLayer == 0)
        {
            carLayer = LayerMask.GetMask("Car"); // Ganti "Car" dengan layer mobil Anda
        }
        
        // Mulai coroutine untuk spawn dengan deteksi
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnWithDetection());
        }
    }

    IEnumerator SpawnWithDetection()
    {
        while (true)
        {
            // Cek apakah aman untuk spawn
            if (IsSafeToSpawn())
            {
                SpawnCar();
                currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }
            else
            {
                // Jika tidak aman, cek lebih sering (0.5 detik)
                currentSpawnInterval = 0.5f;
                
                // Optional: Debug untuk monitoring
                // Debug.Log("Antrian penuh, menunggu untuk spawn...");
            }
            
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    bool IsSafeToSpawn()
    {
        // Deteksi mobil di area spawn point
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance, carLayer);
        
        // Hitung jumlah mobil yang ada di area spawn (termasuk yang sedang bergerak)
        int activeCarsCount = 0;
        
        foreach (Collider car in carsInRange)
        {
            CarAI carAI = car.GetComponent<CarAI>();
            if (carAI != null)
            {
                // Hanya hitung mobil yang belum mencapai waypoint terakhir
                if (!carAI.HasReachedDestination())
                {
                    activeCarsCount++;
                }
            }
            else
            {
                // Jika tidak ada CarAI component, tetap hitung
                activeCarsCount++;
            }
        }
        
        // Cek juga jarak mobil terdekat dari spawn point
        float distanceToNearestCar = GetDistanceToNearestCar();
        
        // Kondisi aman untuk spawn:
        // 1. Jumlah mobil dalam antrian kurang dari batas maksimal
        // 2. Mobil terdekat minimal 3 meter dari spawn point (memberi ruang)
        bool isQueueNotFull = activeCarsCount < maxCarsInQueue;
        bool hasEnoughSpace = distanceToNearestCar >= 3f;
        
        return isQueueNotFull && hasEnoughSpace;
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
        
        // Assign waypoints ke CarAI
        CarAI ai = car.GetComponent<CarAI>();
        if (ai != null)
        {
            ai.waypoints = waypoints;
            
            // Optional: Set custom speed berdasarkan kemacetan
            // ai.SetSpeedBasedOnTraffic(GetTrafficDensity());
        }
        
        // Optional: Tambahkan efek spawn
        // PlaySpawnEffect();
    }
    
    // Method untuk mendapatkan tingkat kepadatan lalu lintas
    float GetTrafficDensity()
    {
        Collider[] carsInRange = Physics.OverlapSphere(spawnPoint.position, detectionDistance * 2f, carLayer);
        return Mathf.Clamp01((float)carsInRange.Length / maxCarsInQueue);
    }
    
    // Method untuk mengatur ulang spawner (bisa dipanggil dari luar)
    public void ResetSpawner()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnWithDetection());
    }
    
    // Visualisasi di editor untuk debugging
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            // Visualisasi area deteksi
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, detectionDistance);
            
            // Visualisasi spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.5f);
        }
    }
}