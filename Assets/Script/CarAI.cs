using UnityEngine;

public class CarAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;

    [Header("Detection")]
    public float detectionDistance = 3f;
    private float baseSpeed; // Tambahkan variabel baseSpeed
    public float stoppingDistance = 1.5f; // Jarak aman berhenti dari mobil depan
    public float raycastOffset = 1.5f;    // Offset raycast dari depan mobil

    private int currentWaypoint = 0;
    private bool isPaying = false;
    private float currentSpeed;

    void Start()
    {
        speed = Random.Range(8f, 12f);
        currentSpeed = speed;
    }

    void Update()
    {
        // ⛔ STOP kalau lagi bayar
        if (isPaying) 
        {
            currentSpeed = 0f;
            return;
        }

        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypoint >= waypoints.Length) return;

        Transform target = waypoints[currentWaypoint];

        // ===== DETEKSI MOBIL DEPAN DENGAN STOPPING DISTANCE =====
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

        // ===== PERBAIKAN: AJUST SPEED BERDASARKAN JARAK =====
        if (isBlocked)
        {
            // Jika terlalu dekat, berhenti total
            if (distanceToObstacle <= stoppingDistance)
            {
                currentSpeed = 0f;
            }
            // Jika masih agak jauh, pelan-pelan
            else if (distanceToObstacle < detectionDistance)
            {
                // Gradual slowdown berdasarkan jarak
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
            // Accelerate kembali ke speed normal
            currentSpeed = Mathf.Min(currentSpeed + Time.deltaTime * 10f, speed);
        }

        // ===== GERAK DENGAN SMOOTH FOLLOW =====
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
        );

        // ===== PERBAIKAN: CEK TABRAKAN SEBELUM PINDAH =====
        // Cek apakah posisi baru akan menabrak mobil depan
        if (isBlocked && distanceToObstacle <= stoppingDistance + 0.5f)
        {
            // Jangan bergerak maju jika terlalu dekat
            // Tetap di posisi sekarang
        }
        else
        {
            transform.position = newPosition;
        }

        // ===== ROTASI =====
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 5f * Time.deltaTime);
        }

        // ===== WAYPOINT =====
        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                Destroy(gameObject);
            }
        }
    }

    // ===== TOLL SYSTEM =====
    public void StopPaying()
    {
        isPaying = false;
        currentSpeed = speed; // Reset speed setelah selesai bayar
    }

    public void StartPaying()
    {
        isPaying = true;
        currentSpeed = 0f;
    }

    // Optional: Visualisasi untuk debugging
    void OnDrawGizmosSelected()
    {
        // Visualisasi raycast
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + transform.forward * raycastOffset + Vector3.up * 0.5f;
        Gizmos.DrawRay(rayOrigin, transform.forward * detectionDistance);
        
        // Visualisasi stopping distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rayOrigin + transform.forward * stoppingDistance, 0.3f);
    }

    // Tambahkan ke CarAI.cs
    public bool HasReachedDestination()
    {
        return currentWaypoint >= waypoints.Length;
    }

    // Optional: Method untuk menyesuaikan speed berdasarkan kepadatan
    public void SetSpeedBasedOnTraffic(float trafficDensity)
    {
        // Kurangi speed jika lalu lintas padat
        float speedMultiplier = Mathf.Lerp(1f, 0.5f, trafficDensity);
        speed = baseSpeed * speedMultiplier;
    }
}