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
    private TextMeshProUGUI rescueButtonText;
    private UnityEngine.UI.Button rescueButton;
    private UnityEngine.UI.Image rescueButtonImage;
    private bool isButtonAnimating = false;

    private bool isBeingRescued = false;

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
                    transform.position + Vector3.up * 7f
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

        if (!isBeingRescued)
        {
            countdown -= Time.deltaTime;
        }
        UpdateCountdownUI();
        if (rescueButtonObject != null)
        {
            Vector3 desiredPos =
                Camera.main.WorldToScreenPoint(
                    transform.position + Vector3.up * 7f
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

        if (FailureManager.Instance != null)
        {
            FailureManager.Instance.AddStrike();
        }

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

        countdownText.fontSize = 28;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.color = Color.red;

        countdownText.text = Mathf.CeilToInt(countdown).ToString();
    }

    void UpdateCountdownUI()
    {
        if (countdownTextObject == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 1.8f;

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

        rescueButton =
            rescueButtonObject.GetComponent<UnityEngine.UI.Button>();

        rescueButtonImage =
            rescueButtonObject.GetComponent<UnityEngine.UI.Image>();

        rescueButtonText =
            rescueButtonObject.GetComponentInChildren<TextMeshProUGUI>();

        rescueButton.onClick.AddListener(RescueCar);
    }

    void RescueCar()
    {
        if (isResolved) return;
        if (isBeingRescued) return;

        if (RescueManager.Instance == null)
        {
            Debug.LogWarning("RescueManager tidak ditemukan!");
            return;
        }

        if (!RescueManager.Instance.CanRescue())
        {
            Debug.Log($"❌ Semua rescue sedang sibuk!");

            if (countdownText != null)
            {
                countdownText.color = Color.yellow;
            }
            return;
        }

        isBeingRescued = true;
        if (countdownTextObject != null)
        {
            countdownTextObject.SetActive(false);
        }

        Debug.Log($"🚑 Rescue process started for {carAI.carID}");
        if (rescueButtonText != null)
        {
            rescueButtonText.text = "RESCUING...";
        }

        if (rescueButton != null)
        {
            rescueButton.interactable = false;
        }

        if (rescueButtonImage != null)
        {
            Color color = rescueButtonImage.color;
            color.a = 0.5f;

            rescueButtonImage.color = color;
        }

        RescueManager.Instance.StartRescue(this);
    }

    public void FinishRescue()
    {
        if (isResolved) return;

        isResolved = true;
        if (FailureManager.Instance != null)
        {
            FailureManager.Instance.ResetStrike();
        }

        Debug.Log($"✅ {carAI.carID} rescued successfully!");

        carAI.isBroken = false;

        if (countdownTextObject != null)
            Destroy(countdownTextObject);

        if (rescueButtonObject != null)
            Destroy(rescueButtonObject);

        Destroy(this);
    }
}