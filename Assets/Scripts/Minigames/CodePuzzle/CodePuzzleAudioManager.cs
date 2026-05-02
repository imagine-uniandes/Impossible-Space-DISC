using UnityEngine;

/// <summary>
/// Maneja todos los sonidos del CodePuzzle.
/// Se auto-suscribe a los eventos de CodePuzzleManager, CodeWordSlot y CodePuzzleWord al iniciar.
/// Asignar los AudioClips en el Inspector; si alguno queda vacío ese sonido se omite silenciosamente.
/// </summary>
public class CodePuzzleAudioManager : MonoBehaviour
{
    [Header("Música de fondo")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundVolume = 0.25f;

    [Header("Interacción con palabras")]
    [Tooltip("Al agarrar una palabra")]
    public AudioClip wordGrabbedSound;
    [Tooltip("Al colocar una palabra en un slot")]
    public AudioClip wordPlacedSound;
    [Tooltip("Al quitar/desplazar una palabra de un slot")]
    public AudioClip wordRemovedSound;

    [Header("Resultado del puzzle")]
    [Tooltip("Al resolver el código correctamente")]
    public AudioClip puzzleSolvedSound;
    [Tooltip("Al ingresar código incorrecto")]
    public AudioClip puzzleFailedSound;

    [Header("Referencias")]
    [Tooltip("Si está vacío, se busca automáticamente en escena")]
    public CodePuzzleManager puzzleManager;

    [Header("Debug")]
    public bool showLogs = false;

    private AudioSource bgSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        bgSource = gameObject.AddComponent<AudioSource>();
        bgSource.loop = true;
        bgSource.playOnAwake = false;
        bgSource.spatialBlend = 0f;
        bgSource.volume = backgroundVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        if (puzzleManager == null)
            puzzleManager = FindObjectOfType<CodePuzzleManager>();
    }

    private void Start()
    {
        if (puzzleManager != null)
        {
            puzzleManager.onPuzzleSolved.AddListener(PlaySolved);
            puzzleManager.onPuzzleFailed.AddListener(PlayFailed);
        }

        foreach (CodeWordSlot slot in FindObjectsOfType<CodeWordSlot>())
        {
            slot.onWordPlaced.AddListener(PlayWordPlaced);
            slot.onWordRemoved.AddListener(PlayWordRemoved);
        }

        foreach (CodePuzzleWord word in FindObjectsOfType<CodePuzzleWord>())
        {
            word.onGrabbed.AddListener(PlayWordGrabbed);
        }

        if (backgroundMusic != null)
        {
            bgSource.clip = backgroundMusic;
            bgSource.Play();
            if (showLogs) Debug.Log("[CodePuzzleAudioManager] Música de fondo iniciada.");
        }
    }

    private void OnDestroy()
    {
        if (puzzleManager != null)
        {
            puzzleManager.onPuzzleSolved.RemoveListener(PlaySolved);
            puzzleManager.onPuzzleFailed.RemoveListener(PlayFailed);
        }
    }

    public void PlayWordGrabbed() => PlaySFX(wordGrabbedSound);
    public void PlayWordPlaced()   => PlaySFX(wordPlacedSound);
    public void PlayWordRemoved()  => PlaySFX(wordRemovedSound);
    public void PlaySolved()       => PlaySFX(puzzleSolvedSound);
    public void PlayFailed()       => PlaySFX(puzzleFailedSound);

    public void StopBackground()              => bgSource?.Stop();
    public void SetBackgroundVolume(float v)  => bgSource.volume = v;

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
        if (showLogs) Debug.Log($"[CodePuzzleAudioManager] SFX: {clip.name}");
    }
}
