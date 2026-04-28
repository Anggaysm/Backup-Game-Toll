using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider bgmSlider;

    void Start()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 1f);
        bgmSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        BGMManager.instance.SetVolume(value);
    }
}