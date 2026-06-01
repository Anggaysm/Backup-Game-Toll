using UnityEngine;

public class TollCameraOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform cameraTransform;

    [Header("Rotation")]
    public float rotationSpeed = 0.2f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minDistance = 15f;
    public float maxDistance = 50f;

    private float currentDistance;

    void Start()
    {
        currentDistance =
            Vector3.Distance(
                transform.position,
                cameraTransform.position
            );
    }

    void Update()
    {
        HandleMouseRotation();
        HandleMouseZoom();
        HandleTouchRotation();
        HandleTouchZoom();
    }

    void HandleMouseRotation()
    {
        if (Input.GetMouseButton(0))
        {
            float deltaX =
                Input.GetAxis("Mouse X");

            transform.Rotate(
                Vector3.up,
                deltaX * rotationSpeed * 100f,
                Space.World
            );
        }
    }

    void HandleMouseZoom()
    {
        float scroll =
            Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Zoom(scroll * zoomSpeed);
        }
    }

    void HandleTouchRotation()
    {
        if (Input.touchCount == 1)
        {
            Touch touch =
                Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                transform.Rotate(
                    Vector3.up,
                    touch.deltaPosition.x *
                    rotationSpeed,
                    Space.World
                );
            }
        }
    }

    void HandleTouchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 =
                Input.GetTouch(0);

            Touch touch1 =
                Input.GetTouch(1);

            Vector2 prev0 =
                touch0.position -
                touch0.deltaPosition;

            Vector2 prev1 =
                touch1.position -
                touch1.deltaPosition;

            float prevDistance =
                Vector2.Distance(prev0, prev1);

            float currentDistanceTouch =
                Vector2.Distance(
                    touch0.position,
                    touch1.position
                );

            float delta =
                currentDistanceTouch -
                prevDistance;

            Zoom(delta * 0.01f);
        }
    }

    void Zoom(float amount)
    {
        Vector3 dir =
            (cameraTransform.position -
             transform.position).normalized;

        currentDistance -= amount;

        currentDistance =
            Mathf.Clamp(
                currentDistance,
                minDistance,
                maxDistance
            );

        cameraTransform.position =
            transform.position +
            dir * currentDistance;
    }
}