using UnityEngine;
using TMPro;

public class UangDisplay : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    
    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        if (MoneyManager.instance != null && textComponent != null)
        {
            textComponent.text = $"💰 {MoneyManager.instance.money}";
        }
    }
}