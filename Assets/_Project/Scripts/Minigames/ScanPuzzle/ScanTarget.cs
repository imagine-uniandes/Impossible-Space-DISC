using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Marcador para un objeto escaneable de la fase de oscuridad.
///
/// Hay dos tipos:
///   • isCorrectTarget = true  → uno de los objetos a encontrar. Al escanearlo
///       se ilumina en verde (color + emisión), suena acierto y avisa al manager.
///   • isCorrectTarget = false → distractor. No cuenta para completar el puzzle.
///
/// Requiere un Collider para que el raycast del HandheldScanner lo detecte,
/// y estar en una capa incluida en el LayerMask del escáner.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ScanTarget : MonoBehaviour
{
    [Header("Tipo")]
    [Tooltip("¿Es uno de los objetos que el jugador debe encontrar? (false = distractor)")]
    public bool isCorrectTarget = true;

    [Header("Resaltado en verde")]
    [Tooltip("Renderers a iluminar. Si está vacío, busca en hijos al escanear.")]
    public Renderer[] renderersToHighlight;

    [Tooltip("Si está activo y no hay renderers asignados, busca renderers en hijos")]
    public bool autoFindRenderersIfEmpty = true;

    [Tooltip("Color al encontrarse correcto")]
    public Color highlightColor = Color.green;

    [Tooltip("Nombre de la propiedad de color del material (_BaseColor para URP, _Color para Built-in)")]
    public string colorPropertyName = "_BaseColor";

    [Header("Emisión")]
    [Tooltip("Activar emisión al resaltar (efecto 'brilla en la oscuridad')")]
    public bool useEmission = true;

    [Tooltip("Color de emisión")]
    public Color emissionColor = Color.green;

    [Tooltip("Intensidad de emisión")]
    [Range(0f, 5f)]
    public float emissionIntensity = 2f;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando este objetivo correcto es encontrado")]
    public UnityEvent onScannedCorrect;

    [Tooltip("Se dispara cuando se escanea un distractor (feedback opcional)")]
    public UnityEvent onScannedWrong;

    [Header("Debug")]
    public bool showLogs = false;

    private bool found;

    public bool IsFound => found;
    public bool IsCorrectTarget => isCorrectTarget;

    // Asignado por el ScanPuzzleManager al inicializar (para avisar al encontrarse).
    [HideInInspector] public ScanPuzzleManager manager;

    /// <summary>
    /// Llamado por el HandheldScanner cuando el haz apunta a este objeto.
    /// Devuelve true si fue un acierto NUEVO (para que el escáner suene acierto),
    /// false en cualquier otro caso (distractor o ya encontrado).
    /// </summary>
    public bool OnScanned(HandheldScanner scanner)
    {
        if (!isCorrectTarget)
        {
            if (showLogs)
                Debug.Log($"<color=yellow>[ScanTarget] {name} es distractor.</color>");
            onScannedWrong?.Invoke();
            return false;
        }

        if (found)
            return false;

        found = true;
        Highlight();

        if (showLogs)
            Debug.Log($"<color=lime>[ScanTarget] ✅ {name} encontrado.</color>");

        onScannedCorrect?.Invoke();

        if (manager != null)
            manager.NotifyTargetFound(this);

        return true;
    }

    private void Highlight()
    {
        if ((renderersToHighlight == null || renderersToHighlight.Length == 0) && autoFindRenderersIfEmpty)
        {
            renderersToHighlight = GetComponentsInChildren<Renderer>(true);
        }

        if (renderersToHighlight == null) return;

        foreach (Renderer rend in renderersToHighlight)
        {
            if (rend == null) continue;

            // material (no sharedMaterial) crea una instancia para no afectar otros objetos.
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                if (mat.HasProperty(colorPropertyName))
                    mat.SetColor(colorPropertyName, highlightColor);
                // Fallback Built-in
                if (colorPropertyName != "_Color" && mat.HasProperty("_Color"))
                    mat.SetColor("_Color", highlightColor);

                if (useEmission)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                }
            }
        }
    }

    [ContextMenu("Test Highlight")]
    private void TestHighlight()
    {
        Highlight();
    }
}
