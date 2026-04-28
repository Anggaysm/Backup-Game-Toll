using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioSource bgmSource;

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
        bgmSource.loop = true;
        bgmSource.Play();
    }
    
    public void SetVolume(float value)
    {
        bgmSource.volume = value;
        PlayerPrefs.SetFloat("BGM", value);
    }
}