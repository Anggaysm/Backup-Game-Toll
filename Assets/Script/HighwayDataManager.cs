using UnityEngine;

public class HighwayDataManager : MonoBehaviour
{
    public static HighwayDataManager Instance;

    [Header("Lane Health Save")]
    public float[] savedLaneHealth = new float[4];

    [Header("Penalty")]
    public int savedStrike = 0;

    [Header("Rescue")]
    public int savedRescue = 1;

    [Header("Rush Hour")]
    public bool wasRushHour = false;
    public float savedRushTimer = 0f;
    
    [Header("Rescue Purchase")]
    public int savedPurchaseCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetAllData()
    {
        savedStrike = 0;
        savedRescue = 1;
        savedPurchaseCount = 0;

        for (int i = 0; i < savedLaneHealth.Length; i++)
        {
            savedLaneHealth[i] = 100f;
        }

        Debug.Log("🔥 Highway data reset!");
    }
}