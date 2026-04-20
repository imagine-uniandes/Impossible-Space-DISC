using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Slot de destino para una etiqueta. Guarda qué ID espera y maneja el snap.
/// </summary>
public class LabelObjectSlot : MonoBehaviour
{
    [Header("Match Data")]
    [Tooltip("ID correcto que debe tener la etiqueta para este objeto")]
    public string expectedLabelId;

    [Tooltip("Objeto fuente para mostrar en resultados cuando este slot sea el incorrecto (opcional)")]
    public GameObject slotResultObjectSource;

    [Header("Snap")]
    [Tooltip("Punto exacto donde se acomodará la etiqueta. Si está vacío usa este transform.")]
    public Transform snapPoint;

    [Tooltip("Permitir reemplazar etiqueta si ya hay una puesta")]
    public bool allowReplacement = true;

    [Tooltip("Si está activo, al entrar una etiqueta al trigger del slot se intentará asignar automáticamente")]
    public bool autoAssignOnTrigger = true;

    [Tooltip("Referencia opcional al manager. Si está vacío, se busca en padres y luego en escena")]
    public LabelMatchPuzzleManager puzzleManagerOverride;

    [Header("Events")]
    public UnityEvent onLabelPlaced;
    public UnityEvent onLabelRemoved;

    [Header("Debug")]
    public bool showLogs = false;

    private LabelTagItem currentLabel;
    private LabelMatchPuzzleManager puzzleManager;

    public LabelTagItem CurrentLabel => currentLabel;
    public bool HasLabel => currentLabel != null;

    private void Awake()
    {
        puzzleManager = puzzleManagerOverride != null
            ? puzzleManagerOverride
            : GetComponentInParent<LabelMatchPuzzleManager>();

        if (puzzleManager == null)
        {
            puzzleManager = FindObjectOfType<LabelMatchPuzzleManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoAssignOnTrigger)
        {
            return;
        }

        TryAutoAssignFromCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!autoAssignOnTrigger)
        {
            return;
        }

        if (currentLabel != null)
        {
            return;
        }

        TryAutoAssignFromCollider(other);
    }

    public Vector3 GetSnapPosition()
    {
        return snapPoint != null ? snapPoint.position : transform.position;
    }

    private void TryAutoAssignFromCollider(Collider other)
    {
        if (other == null)
        {
            return;
        }

        LabelTagItem label = other.GetComponentInParent<LabelTagItem>();
        if (label == null)
        {
            return;
        }

        TryAssignLabel(label);
    }

    public Quaternion GetSnapRotation()
    {
        return snapPoint != null ? snapPoint.rotation : transform.rotation;
    }

    [ContextMenu("Create SnapPoint Child")]
    public void CreateSnapPointChild()
    {
        if (snapPoint != null)
        {
            return;
        }

        GameObject snapPointObject = new GameObject("SnapPoint");
        snapPointObject.transform.SetParent(transform);
        snapPointObject.transform.localPosition = Vector3.zero;
        snapPointObject.transform.localRotation = Quaternion.identity;
        snapPointObject.transform.localScale = Vector3.one;
        snapPoint = snapPointObject.transform;
    }

    public bool TryAssignLabel(LabelTagItem label)
    {
        if (label == null)
        {
            return false;
        }

        if (currentLabel == label)
        {
            SnapLabel(label);
            return true;
        }

        if (currentLabel != null)
        {
            if (!allowReplacement)
            {
                return false;
            }

            ClearCurrentLabel();
        }

        if (label.CurrentSlot != null && label.CurrentSlot != this)
        {
            label.CurrentSlot.ClearCurrentLabel(label);
        }

        currentLabel = label;
        SnapLabel(label);
        label.SetPlacementState(this, true);

        onLabelPlaced?.Invoke();
        puzzleManager?.NotifySlotStateChanged();

        if (showLogs)
        {
            Debug.Log($"[LabelObjectSlot] {name} asignó etiqueta {label.name}");
        }

        return true;
    }

    public void ClearCurrentLabel(LabelTagItem specificLabel = null)
    {
        if (currentLabel == null)
        {
            return;
        }

        if (specificLabel != null && currentLabel != specificLabel)
        {
            return;
        }

        LabelTagItem removedLabel = currentLabel;
        currentLabel = null;
        removedLabel.SetPlacementState(null, false);

        onLabelRemoved?.Invoke();
        puzzleManager?.NotifySlotStateChanged();

        if (showLogs)
        {
            Debug.Log($"[LabelObjectSlot] {name} removió etiqueta {removedLabel.name}");
        }
    }

    public bool IsCorrect()
    {
        if (currentLabel == null)
        {
            return false;
        }

        return string.Equals(currentLabel.LabelId, expectedLabelId, System.StringComparison.Ordinal);
    }

    public string GetCurrentLabelDisplayText()
    {
        return currentLabel != null ? currentLabel.LabelDisplayText : string.Empty;
    }

    private void SnapLabel(LabelTagItem label)
    {
        Transform labelTransform = label.transform;
        labelTransform.SetPositionAndRotation(GetSnapPosition(), GetSnapRotation());
    }
}
