using TMPro;
using UnityEngine;

/// <summary>
/// Muestra el tiempo del GameTimer en un TextMeshPro (3D world space).
/// Pon este componente en el mismo GameObject que el TextMeshPro 3D.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class GameTimerDisplay : MonoBehaviour
{
    /// <summary>Referencia única al display global de la escena. Los GlobalTimerAnchor lo usan para reubicarlo.</summary>
    public static GameTimerDisplay Instance { get; private set; }

    [Tooltip("Prefijo visible antes del tiempo (ej: 'TIEMPO  ')")]
    public string prefix = "TIEMPO  ";

    [Tooltip("Si está activo, el contador se coloca/sigue al anchor de la sala activa (GlobalTimerAnchor). Si no hay ninguna activa, se queda donde esté.")]
    public bool followActiveAnchor = true;

    private TextMeshPro label;

    private void Awake()
    {
        Instance = this;
        label = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        if (GameTimer.Instance == null) return;
        label.text = prefix + GameTimer.FormatTime(GameTimer.Instance.Elapsed);
    }

    private void LateUpdate()
    {
        // Sigue al anchor de la sala activa SIN emparentarse (evita que una sala
        // precargada/inactiva se lleve el contador y lo deje invisible).
        if (!followActiveAnchor) return;

        var anchor = GlobalTimerAnchor.ActiveAnchor;
        if (anchor == null) return;

        Transform target = anchor.Target;
        transform.SetPositionAndRotation(target.position, target.rotation);
        transform.localScale = anchor.worldScale; // objeto suelto en raíz → escala local == escala mundo
    }
}
