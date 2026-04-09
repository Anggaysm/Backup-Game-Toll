using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeDuration = 2.5f;

    public float popScale = 1.5f;
    public float popSpeed = 10f;

    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private Vector3 targetScale;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        transform.localScale = Vector3.zero;
        targetScale = Vector3.one * popScale;
    }

    void Update()
    {
        // 🔥 POP SCALE
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, popSpeed * Time.deltaTime);
        // gerak naik
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // fade out
        canvasGroup.alpha -= Time.deltaTime / fadeDuration;

        // destroy kalau udah hilang
        if (canvasGroup.alpha <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }

    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
}