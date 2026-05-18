using UnityEngine;
using System.Collections; 

public class CarAI : MonoBehaviour
{
    public enum CarMode
    {
        TollGate,   // Mode untuk scene Toll Gate
        Highway     // Mode untuk scene Highway
    }

    [Header("Car Mode")]
    public CarMode currentMode = CarMode.TollGate;

    [Header("Highway Mode Settings")]
    public bool isBroken = false;
    public float breakdownChance = 0.01f;
    private float breakdownCheckTimer = 0f;


    public enum CarCategory
    {
        Category1,
        Category2,
        Category3,
        Category4
    }

    [Header("Car Identity")]
    public string carID;
    // HAPUS: public int queueNumber;

    [Header("Toll Settings")]
    public CarCategory category;

    public Transform[] waypoints;
    public float speed = 10f;

    [Header("Detection")]
    public float detectionDistance = 3f;
    private float baseSpeed;
    public float stoppingDistance = 1.5f;
    public float raycastOffset = 1.5f;

    [Header("Queue System")]
    [HideInInspector] public bool IsInQueue = false;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private int currentWaypoint = 0;
    private bool isPaying = false;
    private float currentSpeed;
    private bool isDestroyed = false;

    [Header("Lane Reference")]
    public LaneController currentLane;

    [Header("Reward")]
    public int rewardMoney = 100;

    void Start()
    {
        speed = Random.Range(10f, 12f);
        currentSpeed = speed;
        baseSpeed = speed;
        
        if (string.IsNullOrEmpty(carID))
        {
            carID = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        if (gameObject.layer != LayerMask.NameToLayer("Car"))
        {
            gameObject.layer = LayerMask.NameToLayer("Car");
        }
    }

    void Update()
    {
        if (isDestroyed) return;
        
        // ========== HIGHWAY MODE LOGIC ==========
        if (currentMode == CarMode.Highway)
        {
            // Cek mogok (hanya jika tidak sedang mogok dan tidak sedang bayar)
            if (!isBroken && !isPaying)
            {
                CheckForBreakdown();
            }
            
            // Jika sedang mogok, berhenti total
            if (isBroken)
            {
                currentSpeed = 0f;
                return; // Tidak bisa gerak
            }
        }
        
        // ========== TOLL GATE MODE LOGIC ==========
        if (isPaying && currentMode == CarMode.TollGate)
        {
            currentSpeed = 0f;
            return;
        }
        
        // ========== MOVEMENT LOGIC (SAMA UNTUK KEDUA MODE) ==========
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypoint >= waypoints.Length) return;

        Transform target = waypoints[currentWaypoint];
        if (target == null)
        {
            currentWaypoint++;
            return;
        }

        bool isBlocked = false;
        float distanceToObstacle = detectionDistance;

        Vector3 rayOrigin = transform.position + transform.forward * raycastOffset + Vector3.up * 0.5f;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Car") && hit.collider.gameObject != gameObject)
            {
                isBlocked = true;
                distanceToObstacle = hit.distance;
            }
        }

        if (isBlocked)
        {
            if (distanceToObstacle <= stoppingDistance)
            {
                currentSpeed = 0f;
            }
            else if (distanceToObstacle < detectionDistance)
            {
                float speedMultiplier = (distanceToObstacle - stoppingDistance) / (detectionDistance - stoppingDistance);
                currentSpeed = Mathf.Lerp(0f, speed, speedMultiplier);
            }
            else
            {
                currentSpeed = speed;
            }
        }
        else
        {
            currentSpeed = Mathf.Min(currentSpeed + Time.deltaTime * 10f, speed);
        }

        float finalSpeed = currentSpeed;
        if (currentLane != null)
        {
            finalSpeed *=
                currentLane.currentSpeedMultiplier;
        }

        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            finalSpeed * Time.deltaTime
        );

        if (!isBlocked || distanceToObstacle > stoppingDistance + 0.5f)
        {
            transform.position = newPosition;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 5f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                if (IsInQueue)
                {
                    IsInQueue = false;
                }
                GiveReward();
                DestroyCar();
            }
        }
    }

    void DestroyCar()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    public int GetPrice()
    {
        switch (category)
        {
            case CarCategory.Category1: return 0;
            case CarCategory.Category2: return 1000;
            case CarCategory.Category3: return 2500;
            case CarCategory.Category4: return 4000;
            default: return 0;
        }
    }

    public void StartPaying()
    {
        if (isDestroyed) return;
        isPaying = true;
        currentSpeed = 0f;
        if (showDebugLogs)
            Debug.Log($"🚗 {carID} mulai bayar");
    }

    public void StopPaying()
    {
        if (isDestroyed) return;
        isPaying = false;
        currentSpeed = speed;
        if (showDebugLogs)
            Debug.Log($"🚗 {carID} selesai bayar");
    }

    public bool HasReachedDestination()
    {
        return currentWaypoint >= waypoints.Length || isDestroyed;
    }

    public void ResetForRestore()
    {
        if (!isDestroyed)
        {
            isPaying = false;
            IsInQueue = true;
            currentSpeed = speed;
            currentWaypoint = 0;
        }
    }

    void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
        }
    }


    public void SetSpeed(float newSpeed)
    {
        if (!isPaying) // Jangan ubah speed kalow sedang bayar
        {
            speed = newSpeed;
            currentSpeed = speed;
            baseSpeed = speed;
            
            if (showDebugLogs)
                Debug.Log($"🏎️ {carID} speed changed to {speed:F1}");
        }
    }

    // Optional: Get current speed
    public float GetCurrentSpeed()
    {
        return speed;
    }

    public void SetTempRestoreSpeed(float tempSpeed, float duration = 5f)
    {
        float originalSpeed = speed;
        
        speed = tempSpeed;
        currentSpeed = tempSpeed;
        baseSpeed = tempSpeed;
        
        if (showDebugLogs)
            Debug.Log($"🏎️ {carID} RESTORE SPEED: {tempSpeed:F1} (will return to normal in {duration}s)");
        
        StartCoroutine(ResetToNormalSpeedAfterDelay(duration, originalSpeed));
    }

    IEnumerator ResetToNormalSpeedAfterDelay(float delay, float originalSpeed)
    {
        yield return new WaitForSeconds(delay);
        
        if (this != null)
        {
            speed = originalSpeed;
            currentSpeed = originalSpeed;
            baseSpeed = originalSpeed;
            
            if (showDebugLogs)
                Debug.Log($"🏎️ {carID} speed back to normal: {originalSpeed:F1}");
        }
    }

    // ========== HIGHWAY MODE METHODS ==========

    void CheckForBreakdown()
    {
        if (currentMode != CarMode.Highway) return;
        
        breakdownCheckTimer += Time.deltaTime;
        if (breakdownCheckTimer >= 1f)
        {
            breakdownCheckTimer = 0f;
            float finalBreakdownChance = breakdownChance;
            if (currentLane != null)
            {
                finalBreakdownChance *=
                    currentLane.breakdownMultiplier;
            }
            if (Random.value < finalBreakdownChance)
            {
                BreakDown();
            }
        }
    }

    void BreakDown()
    {
        if (currentMode != CarMode.Highway) return;
        if (isBroken) return;

        isBroken = true;
        currentSpeed = 0f;
        gameObject.AddComponent<BrokenCar>();

        Debug.Log($"💥 {carID} mogok!");
    }

    void GiveReward()
    {
        if (MoneyManager.instance != null)
        {
            int finalReward = rewardMoney;
            if (HighwayEfficiencyManager.Instance != null)
            {
                float efficiency =
                    HighwayEfficiencyManager.Instance.currentEfficiency;

                // HIGH EFFICIENCY
                if (efficiency >= 90)
                {
                    finalReward *= 2;
                }

                // LOW EFFICIENCY
                else if (efficiency < 50)
                {
                    finalReward =
                        Mathf.RoundToInt(finalReward * 0.5f);
                }
            }
            MoneyManager.instance.AddMoney(finalReward);

            Debug.Log($"💰 +{finalReward} from {carID}");
            ShowFloatingReward(finalReward);
        }
    }
    void ShowFloatingReward(int amount)
    {
        if (HighwayUIReference.Instance == null) return;

        GameObject prefab =
            HighwayUIReference.Instance.floatingTextPrefab;

        if (prefab == null) return;

        GameObject canvasObj =
            GameObject.Find("Canvas");

        if (canvasObj == null) return;

        GameObject floatingObj =
            Instantiate(
                prefab,
                canvasObj.transform
            );

        Vector3 screenPos =
            Camera.main.WorldToScreenPoint(
                transform.position + Vector3.up * 2f
            );

        floatingObj.transform.position = screenPos;

        FloatingText floatingText =
            floatingObj.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            floatingText.SetText($"+${amount}");
            floatingText.SetColor(Color.green);
        }
    }
}