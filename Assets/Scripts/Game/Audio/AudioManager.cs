using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource sfx;      // PlayOneShot for one-shots
    public AudioSource music;    // looping bed (optional)
    public AudioClip successClip, failClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DispatchEvents.Succeeded += OnWin;
        DispatchEvents.Failed    += OnLose;
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        DispatchEvents.Succeeded -= OnWin;
        DispatchEvents.Failed    -= OnLose;
    }

    void OnWin(EventData job, EventOutcome o)  => Play(successClip);
    void OnLose(EventData job, EventOutcome o) => Play(failClip);

    // direct API so anything can trigger audio without events
    public void Play(AudioClip clip) { if (clip && sfx) sfx.PlayOneShot(clip); }
    public void PlayMusic(AudioClip clip) { if (music) { music.clip = clip; music.loop = true; music.Play(); } }
}