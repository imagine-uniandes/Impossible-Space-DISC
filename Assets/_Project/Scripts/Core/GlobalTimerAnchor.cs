using UnityEngine;

/// <summary>
/// Colócalo en cada PREFAB de sala / subsala (sistema de teleport + pool).
///
/// El contador (GameTimerDisplay) es ÚNICO en la escena y la lógica del tiempo
/// (GameTimer) vive en un objeto persistente aparte, así que el tiempo nunca se
/// reinicia ni se pausa al cambiar de sala. Este componente solo marca DÓNDE
/// debe verse el contador dentro de la sala activa.
///
/// IMPORTANTE — por qué NO se emparenta el display:
///   El display NO se hace hijo del anchor. Si lo fuera, una sala PRECARGADA
///   (que se instancia activa y se desactiva en el mismo frame) le robaría el
///   contador y se lo llevaría a su jerarquía inactiva → el texto desaparece.
///   En su lugar, el display SIGUE en cada LateUpdate al anchor de la sala que
///   está activa (ver <see cref="ActiveAnchor"/> y GameTimerDisplay). Así nunca
///   se lo traga una sala inactiva, no hereda escalas raras del prefab y sigue
///   bien aunque la sala se teletransporte.
///
/// Wiring:
///   1. Crea un GameObject vacío dentro del prefab, en la posición/rotación
///      (mirando hacia el jugador) donde quieres que aparezca el contador.
///   2. Añade este componente y ponlo como 'anchor' (o déjalo en el mismo objeto
///      y se usa su propio transform). El gizmo muestra dónde aparecerá el texto.
/// </summary>
public class GlobalTimerAnchor : MonoBehaviour
{
    [Header("Posición destino")]
    [Tooltip("Punto donde se colocará el contador en esta sala. Si se deja vacío se usa el transform de este mismo objeto.")]
    public Transform anchor;

    [Tooltip("Escala EN MUNDO que se aplicará al contador en esta sala (no depende de la jerarquía del prefab).")]
    public Vector3 worldScale = Vector3.one;

    /// <summary>Anchor de la sala actualmente activa. El display lo sigue cada frame.</summary>
    public static GlobalTimerAnchor ActiveAnchor { get; private set; }

    /// <summary>Transform al que debe ir el display (el 'anchor' asignado o este mismo objeto).</summary>
    public Transform Target => anchor != null ? anchor : transform;

    private void OnEnable()
    {
        // Esta sala pasa a ser la dueña del contador.
        ActiveAnchor = this;
    }

    private void OnDisable()
    {
        // Si esta sala era la dueña y se desactiva (precarga / cambio de sala),
        // busca otra sala activa que tome el relevo para que el display no quede huérfano.
        if (ActiveAnchor != this) return;

        ActiveAnchor = null;
        var all = FindObjectsByType<GlobalTimerAnchor>(FindObjectsSortMode.None);
        foreach (var a in all)
        {
            if (a != this && a.isActiveAndEnabled)
            {
                ActiveAnchor = a;
                break;
            }
        }
    }

    // Dibuja en el editor dónde aparecerá el contador, para colocarlo sin adivinar.
    private void OnDrawGizmos()
    {
        Transform t = Target;
        Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
        Gizmos.DrawWireSphere(t.position, 0.08f);
        Gizmos.DrawLine(t.position, t.position + t.forward * 0.3f); // hacia dónde "mira" el texto
    }
}
