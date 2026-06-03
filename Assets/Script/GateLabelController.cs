using TMPro;
using UnityEngine;

public class GateLabelController : MonoBehaviour
{
    public TextMeshPro gateText;

    [Header("Color")]
    public Color unlockedColor =
        Color.cyan;

    public Color lockedColor =
        Color.gray;

    [Range(0f,1f)]
    public float lockedAlpha = 0.3f;

    public void SetUnlocked(bool unlocked)
    {
        if (unlocked)
        {
            Color c =
                unlockedColor;

            c.a = 1f;

            gateText.color = c;
        }
        else
        {
            Color c =
                lockedColor;

            c.a =
                lockedAlpha;

            gateText.color = c;
        }
    }
}