using UnityEngine;

public class DebugAutoReset : MonoBehaviour
{
    public bool resetOnPlay = true;

    void Awake()
    {
#if UNITY_EDITOR
        if (resetOnPlay)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("🔥 AUTO RESET (EDITOR ONLY)");
        }
#endif
    }
}