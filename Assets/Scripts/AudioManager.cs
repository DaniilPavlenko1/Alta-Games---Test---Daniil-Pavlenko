using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float clipVolume = 0.9f;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource || !sfxSource)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                if (!musicSource) musicSource = sources[0];
                if (!sfxSource) sfxSource = sources[1];
            }
        }
    }

    private void Start()
    {
        PlayBackground();
    }

    public void PlayBackground()
    {
        if (!musicSource || !backgroundMusic) return;

        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (musicSource.clip != backgroundMusic)
            musicSource.clip = backgroundMusic;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopBackground()
    {
        if (musicSource && musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PlayWin()
    {
        PlayClip(winClip);
    }

    public void PlayLose()
    {
        PlayClip(loseClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (!sfxSource || !clip) return;

        if (sfxSource.isPlaying) sfxSource.Stop();

        sfxSource.loop = false;
        sfxSource.volume = clipVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void ResetToMenu()
    {
        if (sfxSource && sfxSource.isPlaying) sfxSource.Stop();
        PlayBackground();
    }
}
