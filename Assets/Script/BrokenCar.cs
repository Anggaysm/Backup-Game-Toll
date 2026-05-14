using UnityEngine;
using TMPro;

public class BrokenCar : MonoBehaviour
{
    private CarAI carAI;

    [Header("Breakdown Settings")]
    public float countdown = 5f;

    private bool isResolved = false;
    
    [Header("UI")]
    public GameObject countdownTextObject;
    private TextMeshProUGUI countdownText;

    [Header("Rescue Button")]
    public GameObject rescueButtonPrefab;

    private GameObject rescueButtonObject;
    private bool isButtonAnimating = false;

    void Start()
    {
        carAI = GetComponent<CarAI>();

        CreateCountdownText();

        rescueButtonPrefab =
            GameObject.Find("GameManager")
            .GetComponent<HighwayUIReference>()
            .floatingRescueButtonPrefab;

        CreateRescueButton();

        if (rescueButtonObject != null)
        {
            Vector3 startPos =
                Camera.main.WorldToScreenPoint(
                    transform.position + Vector3.up * 5f
                );

            rescueButtonObject.transform.position = startPos;

            isButtonAnimating = true;
        }

        isButtonAnimating = true;

        Debug.Log($"🚨 {carAI.carID} breakdown started!");
    }

    void Update()
    {
        if (isResolved) return;

        countdown -= Time.deltaTime;
        UpdateCountdownUI();
        if (rescueButtonObject != null)
        {
            Vector3 desiredPos =
                Camera.main.WorldToScreenPoint(
                    transform.position + Vector3.up * 3f
                );

            if (isButtonAnimating)
            {
                rescueButtonObject.transform.position =
                    Vector3.Lerp(
                        rescueButtonObject.transform.position,
                        desiredPos,
                        Time.deltaTime * 8f
                    );

                float distance =
                    Vector3.Distance(
                        rescueButtonObject.transform.position,
                        desiredPos
                    );

                if (distance < 1f)
                {
                    isButtonAnimating = false;
                }
            }
            else
            {
                rescueButtonObject.transform.position = desiredPos;
            }
        }

        if (countdown <= 0)
        {
            AutoRecover();
        }
    }

    void AutoRecover()
    {
        isResolved = true;

        if (countdownTextObject != null)
        {
            Destroy(countdownTextObject);
        }

        Debug.Log($"✅ {carAI.carID} recovered automatically!");

        carAI.isBroken = false;

        Destroy(this);
    }

    private void OnDestroy()
    {
        if (rescueButtonObject != null)
        {
            Destroy(rescueButtonObject);
        }
        if (countdownTextObject != null)
        {
            Destroy(countdownTextObject);
        }
        if (carAI != null)
        {
            carAI.isBroken = false;
        }
    }

    void CreateCountdownText()
    {
        GameObject canvasObj = GameObject.Find("Canvas");

        if (canvasObj == null)
        {
            Debug.LogWarning("Canvas tidak ditemukan!");
            return;
        }

        countdownTextObject = new GameObject("BreakdownCountdown");

        countdownTextObject.transform.SetParent(canvasObj.transform);

        countdownText = countdownTextObject.AddComponent<TextMeshProUGUI>();

        countdownText.fontSize = 36;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.color = Color.red;

        countdownText.text = Mathf.CeilToInt(countdown).ToString();
    }

    void UpdateCountdownUI()
    {
        if (countdownTextObject == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 2f;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        countdownTextObject.transform.position = screenPos;

        countdownText.text = Mathf.CeilToInt(countdown).ToString();
    }

    void CreateRescueButton()
    {
        if (rescueButtonPrefab == null)
        {
            Debug.LogWarning("Rescue button prefab tidak ditemukan!");
            return;
        }

        GameObject canvasObj = GameObject.Find("Canvas");

        rescueButtonObject = Instantiate(
            rescueButtonPrefab,
            canvasObj.transform
        );

        UnityEngine.UI.Button btn =
            rescueButtonObject.GetComponent<UnityEngine.UI.Button>();

        btn.onClick.AddListener(RescueCar);
    }

    void RescueCar()
    {
        if (isResolved) return;

        isResolved = true;

        Debug.Log($"🚑 {carAI.carID} rescued!");

        carAI.isBroken = false;

        if (countdownTextObject != null)
            Destroy(countdownTextObject);

        if (rescueButtonObject != null)
            Destroy(rescueButtonObject);

        Destroy(this);
    }
}