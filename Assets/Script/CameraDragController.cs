using UnityEngine;

public class CameraDragController : MonoBehaviour
{
    [Header("Movement")]
    public float dragSpeed = 0.05f;

    [Header("Z Limit")]
    public float minZ = -60f;
    public float maxZ = 0f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;

    public float minFOV = 30f;
    public float maxFOV = 60f;

    private Camera cam;
    private Vector3 lastMousePosition;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMouseDrag();
        HandleTouchDrag();

        HandleMouseZoom();
        HandleTouchZoom();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition =
                Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta =
                Input.mousePosition -
                lastMousePosition;

            Vector3 pos =
                transform.position;

            pos.z +=
                delta.x *
                dragSpeed;

            pos.z =
                Mathf.Clamp(
                    pos.z,
                    minZ,
                    maxZ
                );

            transform.position =
                pos;

            lastMousePosition =
                Input.mousePosition;
        }
    }

    void HandleTouchDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch =
                Input.GetTouch(0);

            if (touch.phase ==
                TouchPhase.Moved)
            {
                Vector3 pos =
                    transform.position;

                pos.z +=
                    touch.deltaPosition.x *
                    dragSpeed;

                pos.z =
                    Mathf.Clamp(
                        pos.z,
                        minZ,
                        maxZ
                    );

                transform.position =
                    pos;
            }
        }
    }

    void HandleMouseZoom()
    {
        float scroll =
            Input.GetAxis(
                "Mouse ScrollWheel"
            );

        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.fieldOfView -=
                scroll * zoomSpeed;

            cam.fieldOfView =
                Mathf.Clamp(
                    cam.fieldOfView,
                    minFOV,
                    maxFOV
                );
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
                Vector2.Distance(
                    prev0,
                    prev1
                );

            float currentDistance =
                Vector2.Distance(
                    touch0.position,
                    touch1.position
                );

            float delta =
                currentDistance -
                prevDistance;

            cam.fieldOfView -=
                delta * 0.01f;

            cam.fieldOfView =
                Mathf.Clamp(
                    cam.fieldOfView,
                    minFOV,
                    maxFOV
                );
        }
    }
}