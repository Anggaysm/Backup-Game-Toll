using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    public Slider loadingSlider;
    public TextMeshProUGUI percentText;
    
    [Header("Settings")]
    public float minDisplayTime = 0.5f;
    public float dotAnimationSpeed = 0.5f;
    
    private int activeRequests = 0;
    private float showTime = 0f;
    private bool isShowing = false;
    private Coroutine dotAnimationCoroutine;
    private string currentBaseMessage = "Loading";
    private float targetProgress = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoading(string message = "Loading")
    {
        activeRequests++;
        
        if (loadingPanel != null && !isShowing)
        {
            loadingPanel.SetActive(true);
            isShowing = true;
            showTime = Time.time;
            currentBaseMessage = message;
            
            // Reset UI
            if (loadingSlider != null)
                loadingSlider.value = 0f;

            targetProgress = 0f;
            if (percentText != null)
                percentText.text = "0%";
            
            // Mulai animasi titik
            if (dotAnimationCoroutine != null)
                StopCoroutine(dotAnimationCoroutine);
            dotAnimationCoroutine = StartCoroutine(AnimateDots());
            
            Debug.Log($"✅ Loading panel ditampilkan (Requests: {activeRequests})");
        }
        else if (isShowing)
        {
            // Update message jika sudah showing
            UpdateMessage(message);
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
            // Hentikan animasi
            if (dotAnimationCoroutine != null)
                StopCoroutine(dotAnimationCoroutine);
                
            loadingPanel.SetActive(false);
            isShowing = false;
            Debug.Log("✅ Loading panel disembunyikan");
        }
    }
    
    // Method untuk update message (dipanggil dari TollGate.cs)
    public void UpdateMessage(string message)
    {
        currentBaseMessage = message;
    }
    
    public void ForceHide()
    {
        activeRequests = 0;
        if (isShowing)
        {
            HideLoadingDelayed();
        }
    }

    // Method untuk update progress (0 - 1)
    public void UpdateProgress(float progress)
    {
        if (!isShowing) return;

        progress = Mathf.Clamp01(progress);

        // Simpan target progress
        targetProgress = progress;

        // Update persen text
        if (percentText != null)
        {
            int percentValue = Mathf.RoundToInt(progress * 100f);
            percentText.text = percentValue + "%";
        }
    }
    
    // Animasi titik bergerak (hanya untuk teks "loading...")
    private IEnumerator AnimateDots()
    {
        int dotCount = 0;
        
        while (isShowing)
        {
            if (loadingText != null)
            {
                string dots = new string('.', dotCount);
                loadingText.text = currentBaseMessage + dots;
            }
            
            dotCount = (dotCount + 1) % 4; // 0,1,2,3 lalu ke 0 lagi
            
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }
    
    // Untuk cek apakah loading sedang aktif
    public bool IsShowing()
    {
        return isShowing;
    }

    void Update()
    {
        if (loadingSlider != null)
        {
            loadingSlider.value = Mathf.Lerp(
                loadingSlider.value,
                targetProgress,
                Time.deltaTime * 5f
            );
        }
    }
    
    void OnDestroy()
    {
        if (dotAnimationCoroutine != null)
            StopCoroutine(dotAnimationCoroutine);
    }
}