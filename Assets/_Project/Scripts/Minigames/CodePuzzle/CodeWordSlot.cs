using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hueco (blank) en el código. Acepta una CodePuzzleWord por snap.
/// Si expectedValue está vacío, acepta cualquier palabra (slot dinámico, ej: color).
/// </summary>
public class CodeWordSlot : MonoBehaviour
{
    [Header("Slot Data")]
    [Tooltip("Identificador interno del slot (ej: objeto, operador, color, funcion, booleano)")]
    public string slotId;

    [Tooltip("Valor correcto exacto. Vacío = acepta cualquier valor (slot dinámico).")]
    public string expectedValue;

    [Header("Snap")]
    [Tooltip("Punto exacto donde se posicionará la palabra. Si vacío usa este Transform.")]
    public Transform snapPoint;
    public bool allowReplacement = true;

    [Tooltip("Si activo, asigna automáticamente cuando una palabra entra al trigger del slot.")]
    public bool autoAssignOnTrigger = true;

    [Header("References")]
    public CodePuzzleManager puzzleManagerOverride;

    [Header("Events")]
    public UnityEvent onWordPlaced;
    public UnityEvent onWordRemoved;

    [Header("Debug")]
    public bool showLogs = false;

    private CodePuzzleWord currentWord;
    private CodePuzzleManager puzzleManager;

    public CodePuzzleWord CurrentWord => currentWord;
    public bool HasWord => currentWord != null;
    /// <summary>True = tiene respuesta fija. False = acepta cualquier valor (slot dinámico).</summary>
    public bool IsFixed => !string.IsNullOrEmpty(expectedValue);

    private void Awake()
    {
        puzzleManager = puzzleManagerOverride != null
            ? puzzleManagerOverride
            : GetComponentInParent<CodePuzzleManager>() ?? FindObjectOfType<CodePuzzleManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoAssignOnTrigger) return;
        TryAutoAssignFromCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!autoAssignOnTrigger || currentWord != null) return;
        TryAutoAssignFromCollider(other);
    }

    private void TryAutoAssignFromCollider(Collider other)
    {
        CodePuzzleWord word = other?.GetComponentInParent<CodePuzzleWord>();
        if (word != null) TryAssignWord(word);
    }

    public bool TryAssignWord(CodePuzzleWord word)
    {
        if (word == null) return false;

        if (currentWord == word)
        {
            SnapWord(word);
            return true;
        }

        if (currentWord != null)
        {
            if (!allowReplacement) return false;
            ClearCurrentWord(sendToOrigin: true);
        }

        if (word.CurrentSlot != null && word.CurrentSlot != this)
            word.CurrentSlot.ClearCurrentWord(word);

        currentWord = word;
        SnapWord(word);
        word.SetPlacementState(this, true);

        onWordPlaced?.Invoke();
        puzzleManager?.NotifySlotStateChanged();

        if (showLogs) Debug.Log($"[CodeWordSlot] '{slotId}' recibió '{word.WordValue}'");
        return true;
    }

    public void ClearCurrentWord(CodePuzzleWord specific = null, bool sendToOrigin = false)
    {
        if (currentWord == null) return;
        if (specific != null && currentWord != specific) return;

        CodePuzzleWord removed = currentWord;
        currentWord = null;
        removed.SetPlacementState(null, false);

        if (sendToOrigin) removed.ReturnToOrigin();

        onWordRemoved?.Invoke();
        puzzleManager?.NotifySlotStateChanged();

        if (showLogs) Debug.Log($"[CodeWordSlot] '{slotId}' liberó '{removed.WordValue}'");
    }

    /// <summary>
    /// Devuelve true si el slot está relleno correctamente.
    /// Slots dinámicos (expectedValue vacío) siempre son válidos si tienen palabra.
    /// </summary>
    public bool IsCorrect()
    {
        if (currentWord == null) return false;
        if (!IsFixed) return true;
        return string.Equals(currentWord.WordValue, expectedValue, System.StringComparison.OrdinalIgnoreCase);
    }

    public string GetCurrentValue() => currentWord != null ? currentWord.WordValue : string.Empty;

    public Vector3 GetSnapPosition() => snapPoint != null ? snapPoint.position : transform.position;
    public Quaternion GetSnapRotation() => snapPoint != null ? snapPoint.rotation : transform.rotation;

    private void SnapWord(CodePuzzleWord word)
    {
        word.transform.SetPositionAndRotation(GetSnapPosition(), GetSnapRotation());
    }
}
