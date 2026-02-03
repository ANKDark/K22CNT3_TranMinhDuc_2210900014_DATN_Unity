using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    [Tooltip("Gán AudioSource dùng để phát nhạc nền vào đây")]
    public AudioSource musicSource;
    
    [Tooltip("Gán AudioSource dùng để phát hiệu ứng (SFX) vào đây")]
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (audioMixer != null)
        {
            float music = PlayerPrefs.GetFloat("MusicVolume", 100f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 100f);

            float musicdB = Mathf.Lerp(-80f, 0f, music / 100f);
            float sfxdB = Mathf.Lerp(-80f, 0f, sfx / 100f);

            audioMixer.SetFloat("MusicVolume", musicdB);
            audioMixer.SetFloat("SFXVolume", sfxdB);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;

        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}
