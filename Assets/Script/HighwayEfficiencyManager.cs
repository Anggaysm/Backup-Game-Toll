using UnityEngine;
using TMPro;

public class HighwayEfficiencyManager : MonoBehaviour
{
    public static HighwayEfficiencyManager Instance;

    [Header("Efficiency")]
    [Range(0, 100)]
    public float currentEfficiency = 100f;

    [Header("References")]
    public LaneController[] lanes;

    [Header("UI")]
    public TextMeshProUGUI efficiencyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (lanes == null || lanes.Length == 0)
        {
            lanes =
                FindObjectsByType<LaneController>(
                    FindObjectsSortMode.None
                );
        }
    }

    void Update()
    {
        CalculateEfficiency();
        UpdateUI();
    }

    void CalculateEfficiency()
    {
        float totalHealth = 0f;

        foreach (LaneController lane in lanes)
        {
            if (lane != null)
            {
                totalHealth += lane.currentHealth;
            }
        }

        float averageHealth =
            totalHealth / lanes.Length;

        int activeBreakdowns =
            FindObjectsByType<BrokenCar>(
                FindObjectsSortMode.None
            ).Length;

        int strikeCount = 0;

        if (FailureManager.Instance != null)
        {
            strikeCount =
                FailureManager.Instance.currentStrike;
        }

        currentEfficiency =
            averageHealth
            - (activeBreakdowns * 5f)
            - (strikeCount * 15f);

        currentEfficiency =
            Mathf.Clamp(currentEfficiency, 0, 100);
    }

    void UpdateUI()
    {
        if (efficiencyText == null) return;

        efficiencyText.text =
            $"Efficiency: {Mathf.RoundToInt(currentEfficiency)}%";

        if (currentEfficiency >= 90)
        {
            efficiencyText.color = Color.green;
        }
        else if (currentEfficiency >= 70)
        {
            efficiencyText.color = Color.yellow;
        }
        else
        {
            efficiencyText.color = Color.red;
        }
    }
}