using UnityEngine;

public class HighwayUIReference : MonoBehaviour
{
    public static HighwayUIReference Instance;

    [Header("UI Prefabs")]
    public GameObject floatingRescueButtonPrefab;
    public GameObject floatingTextPrefab;

    void Awake()
    {
        Instance = this;
    }
}