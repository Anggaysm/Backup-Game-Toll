using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Load saved values
        bgmSlider.value =
            PlayerPrefs.GetFloat("BGM", 1f);

        sfxSlider.value =
            PlayerPrefs.GetFloat("SFX", 1f);

        // Register listeners
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void SetBGMVolume(float value)
    {
        BGMManager.instance.SetVolume(value);
    }

    void SetSFXVolume(float value)
    {
        BGMManager.instance.SetSFXVolume(value);
    }
}