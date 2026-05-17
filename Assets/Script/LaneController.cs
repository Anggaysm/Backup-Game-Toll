using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LaneController : MonoBehaviour
{
    [Header("Lane Info")]
    public string laneName = "Lane";
    public int laneIndex;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Tooltip("Semakin besar, semakin cepat rusak")]
    public float healthDecayRate = 1f;

    [Header("UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Button maintenanceButton;

    [Header("Maintenance")]
    public float repairAmount = 30f;
    public int maintenanceCost = 1000;

    [Header("Gameplay Effect")]
    public float currentSpeedMultiplier = 1f;

    public float breakdownMultiplier = 1f;

    private bool hasLoadedData = false;
    private float decayTimer = 0f;

    void Start()
    {
        if (HighwayDataManager.Instance != null)
        {
            float savedHealth =
                HighwayDataManager.Instance
                .savedLaneHealth[laneIndex];

            if (savedHealth > 0)
            {
                currentHealth = savedHealth;
            }
            else
            {
                currentHealth = maxHealth;
            }
        }
        
        else
        {
            currentHealth = maxHealth;
        }

        UpdateUI();
        hasLoadedData = true;

        if (maintenanceButton != null)
        {
            maintenanceButton.onClick.AddListener(RepairLane);
        }
    }

    void Update()
    {
        DecayHealth();
        UpdateUI();
        UpdateGameplayEffect();
    }

    void DecayHealth()
    {
        decayTimer += Time.deltaTime;

        if (decayTimer >= 1f)
        {
            decayTimer = 0f;

            currentHealth -= healthDecayRate;

            currentHealth =
                Mathf.Clamp(currentHealth, 0, maxHealth);

            SaveLaneHealth();
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value =
                currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.RoundToInt(currentHealth)}%";
        }

        if (healthSlider != null)
        {
            Image fill =
                healthSlider.fillRect.GetComponent<Image>();

            if (fill != null)
            {
                if (currentHealth > 70)
                {
                    fill.color = Color.green;
                }
                else if (currentHealth > 40)
                {
                    fill.color = Color.yellow;
                }
                else
                {
                    fill.color = Color.red;
                }
            }
        }
    }

    public void RepairLane()
    {
        if (MoneyManager.instance == null) return;

        if (MoneyManager.instance.money < maintenanceCost)
        {
            Debug.Log("❌ Uang maintenance tidak cukup!");
            return;
        }

        MoneyManager.instance.SpendMoney(maintenanceCost);

        currentHealth += repairAmount;

        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);
            SaveLaneHealth();

        Debug.Log($"🛠️ {laneName} repaired!");
    }

    void UpdateGameplayEffect()
    {
        float healthPercent =
            currentHealth / maxHealth;

        // HEALTH BAGUS
        if (healthPercent > 0.7f)
        {
            currentSpeedMultiplier = 1f;
            breakdownMultiplier = 1f;
        }

        // HEALTH SEDANG
        else if (healthPercent > 0.4f)
        {
            currentSpeedMultiplier = 0.8f;
            breakdownMultiplier = 2f;
        }

        // HEALTH KRITIS
        else
        {
            currentSpeedMultiplier = 0.5f;
            breakdownMultiplier = 4f;
        }
    }
    void SaveLaneHealth()
    {
        if (!hasLoadedData) return;

        if (HighwayDataManager.Instance != null)
        {
            HighwayDataManager.Instance
                .savedLaneHealth[laneIndex] =
                currentHealth;
        }
    }
}