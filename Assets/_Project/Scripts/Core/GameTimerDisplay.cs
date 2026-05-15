using TMPro;
using UnityEngine;

/// <summary>
/// Muestra el tiempo del GameTimer en un TextMeshPro (3D world space).
/// Pon este componente en el mismo GameObject que el TextMeshPro 3D.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class GameTimerDisplay : MonoBehaviour
{
    [Tooltip("Prefijo visible antes del tiempo (ej: 'TIEMPO  ')")]
    public string prefix = "TIEMPO  ";

    private TextMeshPro label;

    private void Awake()
    {
        label = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        if (GameTimer.Instance == null) return;
        label.text = prefix + GameTimer.FormatTime(GameTimer.Instance.Elapsed);
    }
}
