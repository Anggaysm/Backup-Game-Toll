using UnityEngine;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    
    [Header("Settings")]
    public float minDisplayTime = 0.5f;
    
    private int activeRequests = 0;
    private float showTime = 0f;
    private bool isShowing = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            loadingPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoading(string message = "Loading...")
    {
        activeRequests++;
        
        if (loadingPanel != null && !isShowing)
        {
            loadingPanel.SetActive(true);
            isShowing = true;
            showTime = Time.time;
            
            if (loadingText != null)
                loadingText.text = message;
                
            Debug.Log($"✅ Loading panel ditampilkan (Requests: {activeRequests})");
        }
        else if (loadingText != null && isShowing)
        {
            loadingText.text = message;
        }
    }

    public void HideLoading()
    {
        activeRequests--;
        
        if (activeRequests <= 0 && isShowing)
        {
            activeRequests = 0;
            
            float elapsed = Time.time - showTime;
            if (elapsed < minDisplayTime)
            {
                Invoke(nameof(HideLoadingDelayed), minDisplayTime - elapsed);
            }
            else
            {
                HideLoadingDelayed();
            }
        }
    }
    
    void HideLoadingDelayed()
    {
        if (loadingPanel != null && isShowing)
        {
            loadingPanel.SetActive(false);
            isShowing = false;
            Debug.Log("✅ Loading panel disembunyikan");
        }
    }
    
    public void UpdateMessage(string message)
    {
        if (loadingText != null && isShowing)
        {
            loadingText.text = message;
        }
    }
    
    public void ForceHide()
    {
        activeRequests = 0;
        if (isShowing)
        {
            HideLoadingDelayed();
        }
    }
}