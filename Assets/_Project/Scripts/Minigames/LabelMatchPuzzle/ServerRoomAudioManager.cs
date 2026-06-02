using UnityEngine;

/// <summary>
/// Maneja todo el audio de la SALA DE SERVIDORES.
/// Sigue el mismo patrón que CodePuzzleAudioManager: se auto-suscribe a los
/// eventos del puzzle al iniciar y reproduce los SFX correspondientes.
///
/// La música de fondo continua la lleva AmbientMusicPlayer (global); aquí solo
/// va el colchón ambiental de servidores y los sonidos del switch.
///
/// Si algún AudioClip queda vacío, ese sonido se omite silenciosamente.
/// </summary>
public class ServerRoomAudioManager : MonoBehaviour
{
    [Header("Ambiente de la sala (loop)")]
    [Tooltip("Zumbido continuo de la sala de servidores. Se reproduce en bucle mientras la sala está activa.")]
    public AudioClip ambienceLoop;
    [Range(0f, 1f)] public float ambienceVolume = 0.4f;
    [Tooltip("0 = se oye igual en toda la sala (2D). 1 = posicional (sale del objeto).")]
    [Range(0f, 1f)] public float ambienceSpatialBlend = 0f;

    [Header("Interacción con el switch")]
    [Tooltip("Al pasar la mano / apuntar al botón (hover).")]
    public AudioClip buttonHoverSound;
    [Tooltip("Al pulsar el botón.")]
    public AudioClip buttonPressSound;

    [Header("Resultado: servidor encendido")]
    [Tooltip("Sonido de encendido del servidor (power-up) al resolver el puzzle.")]
    public AudioClip serverPoweredOnSound;
    [Tooltip("Opcional: ambiente que sustituye al colchón cuando el servidor arranca (servidores en marcha). Si está vacío se mantiene 'ambienceLoop'.")]
    public AudioClip ambienceAfterPowerOn;

    [Header("Referencias")]
    [Tooltip("Si está vacío, se busca automáticamente en la escena.")]
    public MiniPuzzleVisualSwitch puzzle;

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

        if (puzzle == null)
            puzzle = FindObjectOfType<MiniPuzzleVisualSwitch>();
    }

    private void Start()
    {
        if (puzzle != null)
            puzzle.onPuzzleExecuted.AddListener(PlayServerPoweredOn);

        StartAmbience(ambienceLoop);
    }

    private void OnDestroy()
    {
        if (puzzle != null)
            puzzle.onPuzzleExecuted.RemoveListener(PlayServerPoweredOn);
    }

    // --- Métodos públicos para wirear a los eventos del botón en el Inspector ---
    public void PlayButtonHover() => PlaySFX(buttonHoverSound);
    public void PlayButtonPress() => PlaySFX(buttonPressSound);

    public void PlayServerPoweredOn()
    {
        PlaySFX(serverPoweredOnSound);
        if (ambienceAfterPowerOn != null)
            StartAmbience(ambienceAfterPowerOn);
        if (showLogs) Debug.Log("[ServerRoomAudioManager] Servidor encendido.");
    }

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
        if (showLogs) Debug.Log($"[ServerRoomAudioManager] SFX: {clip.name}");
    }
}
