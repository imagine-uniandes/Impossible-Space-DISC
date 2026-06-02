using System.Collections;
using UnityEngine;

/// <summary>
/// Reproductor de música de fondo GLOBAL para la experiencia de una sola escena.
///
/// Es un objeto único y persistente (igual que GameTimer): la música suena en
/// bucle y NO se reinicia al cambiar de sala (el pool activa/desactiva las salas,
/// pero este objeto vive aparte). Pensado para reutilizar la música que ya usas
/// en la sala de código.
///
/// Si alguna zona necesita cambiar de pista, llama PlayTrack(clip) y hace un
/// crossfade suave sin cortes.
///
/// Wiring: ponlo en un GameObject suelto de la escena (no dentro de un prefab de
/// sala) y asígnale la música de fondo.
/// </summary>
public class AmbientMusicPlayer : MonoBehaviour
{
    public static AmbientMusicPlayer Instance { get; private set; }

    [Header("Música")]
    [Tooltip("Pista de fondo por defecto (reutiliza la de la sala de código).")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float volume = 0.25f;

    [Header("Transiciones")]
    [Tooltip("Duración del crossfade al cambiar de pista (segundos).")]
    public float crossfadeDuration = 1.5f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource active;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sourceA = CreateSource();
        sourceB = CreateSource();
        active = sourceA;
    }

    private AudioSource CreateSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D: se oye igual en toda la escena
        src.volume = 0f;
        return src;
    }

    private void Start()
    {
        if (backgroundMusic != null)
            PlayTrack(backgroundMusic, instant: true);
    }

    /// <summary>Cambia (o inicia) la pista de fondo con crossfade. instant=true para arrancar sin fundido.</summary>
    public void PlayTrack(AudioClip clip, bool instant = false)
    {
        if (clip == null) return;
        if (active.clip == clip && active.isPlaying) return;

        AudioSource next = active == sourceA ? sourceB : sourceA;
        next.clip = clip;
        next.volume = 0f;
        next.Play();

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        if (instant)
        {
            active.Stop();
            next.volume = volume;
            active = next;
        }
        else
        {
            fadeRoutine = StartCoroutine(Crossfade(active, next));
        }
    }

    /// <summary>Ajusta el volumen de la música actual (0-1).</summary>
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (active != null) active.volume = volume;
    }

    public void Stop() => active?.Stop();

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float t = 0f;
        float startFrom = from.volume;
        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / crossfadeDuration;
            from.volume = Mathf.Lerp(startFrom, 0f, k);
            to.volume = Mathf.Lerp(0f, volume, k);
            yield return null;
        }
        from.volume = 0f;
        from.Stop();
        to.volume = volume;
        active = to;
        fadeRoutine = null;
    }
}
