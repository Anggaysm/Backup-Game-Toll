using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioSource bgmSource;
    public static float SFXVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("BGM", 1f);
        bgmSource.volume = volume;

        SFXVolume = PlayerPrefs.GetFloat("SFX", 1f);
        bgmSource.loop = true;
        bgmSource.Play();
    }
    
    public void SetVolume(float value)
    {
        bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGM", value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;

        PlayerPrefs.SetFloat("SFX", value);
        PlayerPrefs.Save();

        Debug.Log("SFX Volume : " + value);
    }
}