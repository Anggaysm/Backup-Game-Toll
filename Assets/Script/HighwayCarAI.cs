using UnityEngine;
using System.Collections;

public class HighwayCarAI : MonoBehaviour
{
    [Header("Car Identity")]
    public string carID;
    public int price = 1000; // Default price
    
    [Header("Waypoints")]
    public Transform[] waypoints;
    
    [Header("Movement")]
    public float speed = 10f;
    private float currentSpeed;
    private int currentWaypoint = 0;
    private bool isDestroyed = false;
    
    [Header("Detection")]
    public float detectionDistance = 3f;
    public float stoppingDistance = 1.5f;
    public float raycastOffset = 1.5f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    void Start()
    {
        // Random speed antara 8-12
        speed = Random.Range(8f, 12f);
        currentSpeed = speed;
        
        // Generate unique ID
        if (string.IsNullOrEmpty(carID))
        {
            carID = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        // Set layer ke Car
        if (gameObject.layer != LayerMask.NameToLayer("Car"))
        {
            gameObject.layer = LayerMask.NameToLayer("Car");
        }
    }
    
    void Update()
    {
        if (isDestroyed) return;
        
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypoint >= waypoints.Length) return;
        
        Transform target = waypoints[currentWaypoint];
        if (target == null)
        {
            currentWaypoint++;
            return;
        }
        
        // Deteksi mobil di depan
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
        
        // Atur kecepatan berdasarkan jarak
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
        
        // Gerakkan mobil
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
        );
        
        if (!isBlocked || distanceToObstacle > stoppingDistance + 0.5f)
        {
            transform.position = newPosition;
        }
        
        // Rotasi menghadap target
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 5f * Time.deltaTime);
        }
        
        // Pindah ke waypoint berikutnya
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentWaypoint++;
            
            if (currentWaypoint >= waypoints.Length)
            {
                StartCoroutine(DestroyWithDelay());
            }
        }
    }
    
    IEnumerator DestroyWithDelay()
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
        return price;
    }
    
    public void SetPrice(int newPrice)
    {
        price = newPrice;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        currentSpeed = speed;
    }
    
    public float GetCurrentSpeed()
    {
        return speed;
    }
}