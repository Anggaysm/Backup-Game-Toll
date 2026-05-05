using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoMoneyUILoader : MonoBehaviour
{
    public GameObject moneyUIPrefab;
    private static bool isSetup = false;
    
    void Awake()
    {
        if (!isSetup)
        {
            isSetup = true;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadMoneyUI();
    }
    
    void Start()
    {
        LoadMoneyUI();
    }
    
    void LoadMoneyUI()
    {
        // Cari Canvas di scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning($"Scene {SceneManager.GetActiveScene().name} tidak punya Canvas!");
            return;
        }
        
        // Cek apakah sudah ada UI Money di scene ini
        if (FindObjectOfType<UangDisplay>() != null)
        {
            Debug.Log("UI Money sudah ada, skip loading");
            return;
        }
        
        // Buat UI Money baru
        if (moneyUIPrefab != null)
        {
            Instantiate(moneyUIPrefab, canvas.transform);
            Debug.Log($"✅ Auto load MoneyUI ke scene: {SceneManager.GetActiveScene().name}");
        }
        else
        {
            Debug.LogError("❌ moneyUIPrefab belum di-drag ke AutoMoneyUILoader!");
        }
    }
}