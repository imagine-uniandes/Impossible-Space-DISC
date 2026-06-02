using UnityEngine;

/// <summary>
/// Maneja todo el audio de la SALA DE IA (fase de escaneo en oscuridad + puzzle
/// de etiquetas). Mismo patrón que CodePuzzleAudioManager / ServerRoomAudioManager:
/// se auto-suscribe a los eventos de los managers de la sala al iniciar.
///
/// Los sonidos de acierto/error del propio HandheldScanner (al apuntar a un
/// objeto) se siguen configurando en el escáner; aquí va el ambiente de la sala
/// y los hitos del puzzle. Si algún clip queda vacío, ese sonido se omite.
///
/// La música de fondo continua la lleva AmbientMusicPlayer (global).
/// </summary>
public class IARoomAudioManager : MonoBehaviour
{
    [Header("Ambiente de la sala (loop)")]
    [Tooltip("Drone/zumbido sci-fi de la sala de IA. Bucle mientras la sala está activa.")]
    public AudioClip ambienceLoop;
    [Range(0f, 1f)] public float ambienceVolume = 0.4f;
    [Tooltip("0 = se oye igual en toda la sala (2D). 1 = posicional.")]
    [Range(0f, 1f)] public float ambienceSpatialBlend = 0f;

    [Header("Fase de escaneo")]
    [Tooltip("Blip al encontrar un objetivo correcto (progreso). Opcional: el escáner ya reproduce su propio acierto, déjalo vacío para no duplicar.")]
    public AudioClip scanProgressSound;
    [Tooltip("Sonido al completar TODO el escaneo (se encienden las luces).")]
    public AudioClip scanCompleteSound;

    [Header("Puzzle de etiquetas")]
    [Tooltip("Al colocar una etiqueta en un slot.")]
    public AudioClip labelPlacedSound;
    [Tooltip("Al quitar una etiqueta de un slot.")]
    public AudioClip labelRemovedSound;
    [Tooltip("Al resolver el puzzle correctamente.")]
    public AudioClip puzzleSolvedSound;
    [Tooltip("Al evaluar con alguna etiqueta mal.")]
    public AudioClip puzzleFailedSound;

    [Header("Referencias (si están vacías, se buscan en escena)")]
    public ScanPuzzleManager scanPuzzle;
    public LabelMatchPuzzleManager labelPuzzle;

    [Header("Debug")]
    public bool showLogs = false;

    private AudioSource ambienceSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.spatialBlend = ambienceSpatialBlend;
        ambienceSource.volume = ambienceVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = ambienceSpatialBlend;

        if (scanPuzzle == null) scanPuzzle = FindObjectOfType<ScanPuzzleManager>();
        if (labelPuzzle == null) labelPuzzle = FindObjectOfType<LabelMatchPuzzleManager>();
    }

    private void Start()
    {
        if (scanPuzzle != null)
        {
            scanPuzzle.onProgress.AddListener(OnScanProgress);
            scanPuzzle.onAllTargetsFound.AddListener(PlayScanComplete);
        }

        if (labelPuzzle != null)
        {
            labelPuzzle.onPuzzleSolved.AddListener(PlayPuzzleSolved);
            labelPuzzle.onPuzzleFailed.AddListener(PlayPuzzleFailed);

            if (labelPuzzle.slots != null)
            {
                foreach (LabelObjectSlot slot in labelPuzzle.slots)
                {
                    if (slot == null) continue;
                    slot.onLabelPlaced.AddListener(PlayLabelPlaced);
                    slot.onLabelRemoved.AddListener(PlayLabelRemoved);
                }
            }
        }

        StartAmbience(ambienceLoop);
    }

    private void OnDestroy()
    {
        if (scanPuzzle != null)
        {
            scanPuzzle.onProgress.RemoveListener(OnScanProgress);
            scanPuzzle.onAllTargetsFound.RemoveListener(PlayScanComplete);
        }

        if (labelPuzzle != null)
        {
            labelPuzzle.onPuzzleSolved.RemoveListener(PlayPuzzleSolved);
            labelPuzzle.onPuzzleFailed.RemoveListener(PlayPuzzleFailed);

            if (labelPuzzle.slots != null)
            {
                foreach (LabelObjectSlot slot in labelPuzzle.slots)
                {
                    if (slot == null) continue;
                    slot.onLabelPlaced.RemoveListener(PlayLabelPlaced);
                    slot.onLabelRemoved.RemoveListener(PlayLabelRemoved);
                }
            }
        }
    }

    private void OnScanProgress(int found, int total) => PlaySFX(scanProgressSound);

    public void PlayScanComplete()  => PlaySFX(scanCompleteSound);
    public void PlayLabelPlaced()   => PlaySFX(labelPlacedSound);
    public void PlayLabelRemoved()  => PlaySFX(labelRemovedSound);
    public void PlayPuzzleSolved()  => PlaySFX(puzzleSolvedSound);
    public void PlayPuzzleFailed()  => PlaySFX(puzzleFailedSound);

    public void SetAmbienceVolume(float v)
    {
        ambienceVolume = Mathf.Clamp01(v);
        if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
    }

    public void StopAmbience() => ambienceSource?.Stop();

    private void StartAmbience(AudioClip clip)
    {
        if (clip == null || ambienceSource == null) return;
        ambienceSource.clip = clip;
        ambienceSource.volume = ambienceVolume;
        ambienceSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
        if (showLogs) Debug.Log($"[IARoomAudioManager] SFX: {clip.name}");
    }
}
