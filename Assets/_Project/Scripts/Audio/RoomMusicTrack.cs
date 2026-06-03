using System.Collections;
using UnityEngine;

/// <summary>
/// Selector de música por sala. Va en el prefab de cada sala (incluida la
/// antecámara/sala intermedia y la inicial).
///
/// Al activarse la sala (OnEnable, gestionado por el pool de salas) le pide al
/// AmbientMusicPlayer global que reproduzca SU pista con un crossfade suave.
/// Como AmbientMusicPlayer ignora la petición si ya está sonando esa misma pista,
/// dos salas seguidas con la misma música no provocan corte ni reinicio.
///
/// Así la música "alterna" entre salas (inicial, código, etiquetado, servidores,
/// intermedias) usando una sola fuente de música, sin solapamientos.
///
/// Reparto:
///   • Música de fondo que cambia por sala → este componente + AmbientMusicPlayer.
///   • Colchón ambiental propio (drone/zumbido) → RoomAmbience.
///   • SFX de puzzles → managers de cada minijuego.
/// </summary>
public class RoomMusicTrack : MonoBehaviour
{
    [Header("Música de esta sala")]
    [Tooltip("Pista de fondo que sonará mientras esta sala esté activa. Vacío = no cambia la música actual.")]
    public AudioClip track;

    [Tooltip("Volumen para esta pista (0-1). Si es negativo, se mantiene el volumen actual del player.")]
    [Range(-1f, 1f)]
    public float overrideVolume = -1f;

    [Tooltip("Si está activo, la primera vez arranca sin fundido (útil para la sala inicial).")]
    public bool instantOnFirstPlay = false;

    [Header("Debug")]
    public bool showLogs = false;

    private static bool anyTrackPlayed;

    private void OnEnable()
    {
        // Importante: NO ponemos la pista directamente en OnEnable. El pool precarga
        // la sala siguiente instanciándola activa y desactivándola en el MISMO frame
        // (Instantiate → SetActive(false)), lo que dispara este OnEnable aunque la
        // sala no sea la que el jugador ve. Si reaccionáramos al instante, esa sala
        // precargada "secuestraría" la música de la sala actual.
        //
        // En su lugar esperamos un frame: si esta activación fue solo el parpadeo de
        // un preload, el objeto ya estará desactivado y Unity cancela esta corrutina
        // antes de tocar la música. Solo la sala que de verdad permanece activa llega
        // a reproducir su pista.
        StartCoroutine(ApplyWhenSettled());
    }

    private IEnumerator ApplyWhenSettled()
    {
        yield return null; // sobrevivir al parpadeo del preload (ver OnEnable)

        // Esperar a que exista el player global (por orden de inicialización).
        int safety = 0;
        while (AmbientMusicPlayer.Instance == null && safety < 300)
        {
            safety++;
            yield return null;
        }

        if (AmbientMusicPlayer.Instance == null)
        {
            if (showLogs)
                Debug.LogWarning($"[RoomMusicTrack] No se encontró AmbientMusicPlayer en escena para '{name}'.");
            yield break;
        }

        ApplyTrack();
    }

    private void ApplyTrack()
    {
        if (track == null)
        {
            if (showLogs) Debug.Log($"[RoomMusicTrack] '{name}' sin pista asignada; se mantiene la música actual.");
            return;
        }

        AmbientMusicPlayer player = AmbientMusicPlayer.Instance;

        if (overrideVolume >= 0f)
            player.SetVolume(overrideVolume);

        bool instant = instantOnFirstPlay && !anyTrackPlayed;
        player.PlayTrack(track, instant);
        anyTrackPlayed = true;

        if (showLogs)
            Debug.Log($"<color=cyan>[RoomMusicTrack] '{name}' pidió pista '{track.name}' (instant={instant}).</color>");
    }
}
