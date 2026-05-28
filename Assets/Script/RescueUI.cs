using UnityEngine;
using TMPro;

public class RescueUI : MonoBehaviour
{
    public TextMeshProUGUI rescueText;

    public TextMeshProUGUI buyButtonText;
    public UnityEngine.UI.Button buyButton;
    [Header("Penalty UI")]
    public TextMeshProUGUI penaltyText;

    void Update()
    {
        if (RescueManager.Instance == null) return;

        int available =
            RescueManager.Instance.GetAvailableRescue();

        int total =
            RescueManager.Instance.totalRescue;

        rescueText.text =
            $"Rescue: {available}/{total}";

        int cost =
            RescueManager.Instance.GetRescueCost();

        buyButtonText.text =
            $"{cost}";
        if (RescueManager.Instance.totalRescue >=
            RescueManager.Instance.maxRescue)
        {
            buyButton.interactable = false;

            buyButtonText.text = "MAX RESCUE";
        }
        
        if (FailureManager.Instance != null)
        {
            int current =
                FailureManager.Instance.currentStrike;

            int max =
                FailureManager.Instance.maxStrike;

            penaltyText.text =
                $"⚠️ Penalty: {current}/{max}";

            if (current == 0)
            {
                penaltyText.color = Color.white;
            }
            else if (current == 1)
            {
                penaltyText.color = Color.yellow;
            }
            else
            {
                penaltyText.color = Color.red;
            }
        }
        
    }
    public void OnBuyRescue()
    {
        if (RescueManager.Instance != null)
        {
            RescueManager.Instance.BuyRescue();
        }
    }
}