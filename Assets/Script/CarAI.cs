using UnityEngine;
using System.Collections; 

public class CarAI : MonoBehaviour
{
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

    void Start()
    {
        speed = Random.Range(8f, 12f);
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
        
        if (isPaying) 
        {
            currentSpeed = 0f;
            return;
        }

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

        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
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
                StartCoroutine(DestroyWithDelay());
            }
        }
    }

    System.Collections.IEnumerator DestroyWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
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
    
}