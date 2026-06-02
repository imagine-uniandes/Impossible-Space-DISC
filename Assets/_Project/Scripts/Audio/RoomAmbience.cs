using System.Collections;
using UnityEngine;

/// <summary>
/// Sonido ambiente PROPIO de una sala. Va en cada prefab de sala/subsala.
///
/// Reproduce un colchón ambiental en bucle (zumbido de servidores, drone sci-fi,
/// ruido de oficina...) mientras la sala está activa, y opcionalmente sonidos
/// puntuales aleatorios (un beep lejano, un goteo, una alarma suave) cada cierto
/// intervalo. Arranca con OnEnable y se detiene con OnDisable, así el pool lo
/// gestiona solo al activar/desactivar la sala.
///
/// La música de fondo continua NO va aquí: esa la lleva AmbientMusicPlayer
/// (global, no se reinicia al cambiar de sala).
/// </summary>
public class RoomAmbience : MonoBehaviour
{
    [Header("Colchón ambiental (loop)")]
    [Tooltip("Sonido continuo de fondo de esta sala. Se reproduce en bucle.")]
    public AudioClip ambienceLoop;
    [Range(0f, 1f)] public float ambienceVolume = 0.4f;
    [Tooltip("0 = se oye igual en toda la sala (2D). 1 = totalmente posicional (3D, sale del objeto).")]
    [Range(0f, 1f)] public float spatialBlend = 0f;

    [Header("Sonidos puntuales aleatorios (opcional)")]
    [Tooltip("Se elige uno al azar cada intervalo. Déjalo vacío si no quieres puntuales.")]
    public AudioClip[] randomOneShots;
    [Range(0f, 1f)] public float oneShotVolume = 0.6f;
    [Tooltip("Segundos mínimos entre sonidos puntuales.")]
    public float minInterval = 8f;
    [Tooltip("Segundos máximos entre sonidos puntuales.")]
    public float maxInterval = 20f;

    private AudioSource loopSource;
    private AudioSource oneShotSource;
    private Coroutine randomRoutine;

    private void Awake()
    {
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = spatialBlend;
        loopSource.volume = ambienceVolume;

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.loop = false;
        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = spatialBlend;
    }

    private void OnEnable()
    {
        if (ambienceLoop != null)
        {
            loopSource.clip = ambienceLoop;
            loopSource.volume = ambienceVolume;
            loopSource.Play();
        }

        if (randomOneShots != null && randomOneShots.Length > 0)
            randomRoutine = StartCoroutine(RandomOneShotLoop());
    }

    private void OnDisable()
    {
        if (loopSource != null) loopSource.Stop();
        if (randomRoutine != null)
        {
            StopCoroutine(randomRoutine);
            randomRoutine = null;
        }
    }

    private IEnumerator RandomOneShotLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            AudioClip clip = randomOneShots[Random.Range(0, randomOneShots.Length)];
            if (clip != null)
                oneShotSource.PlayOneShot(clip, oneShotVolume);
        }
    }
}
