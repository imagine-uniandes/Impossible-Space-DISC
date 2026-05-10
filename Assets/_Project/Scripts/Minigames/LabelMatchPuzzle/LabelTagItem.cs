using UnityEngine;

/// <summary>
/// Etiqueta agarrable para el minijuego de emparejar objeto-etiqueta.
/// Funciona con cualquier sistema de grab/release (XR o custom) llamando
/// OnGrabbed() y OnReleasedTryPlace() desde eventos.
/// </summary>
public class LabelTagItem : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("ID de la etiqueta (debe coincidir con expectedLabelId del slot correcto)")]
    public string labelId;

    [Tooltip("Texto visible de la etiqueta para mostrar en resultados (si está vacío usa labelId)")]
    public string labelDisplayText;

    [Header("Auto Place")]
    [Tooltip("Radio para buscar un slot cercano al soltar")]
    [Min(0.01f)]
    public float autoPlaceRadius = 0.2f;

    [Tooltip("Capas donde están los slots. Si no se configura, usa todas.")]
    public LayerMask slotLayerMask = ~0;

    [Tooltip("Si está activo, al colocarse se bloquea físicamente la etiqueta")]
    public bool lockRigidbodyWhenPlaced = true;

    [Header("Debug")]
    public bool showLogs = false;

    private Rigidbody cachedRigidbody;
    private LabelObjectSlot currentSlot;
    private bool isPlaced;
    private RigidbodyConstraints originalConstraints;
    private bool originalUseGravity;
    private bool originalIsKinematic;

    public string LabelId => labelId;
    public string LabelDisplayText => string.IsNullOrWhiteSpace(labelDisplayText) ? labelId : labelDisplayText;
    public bool IsPlaced => isPlaced;
    public LabelObjectSlot CurrentSlot => currentSlot;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();

        if (cachedRigidbody != null)
        {
            originalConstraints = cachedRigidbody.constraints;
            originalUseGravity = cachedRigidbody.useGravity;
            originalIsKinematic = cachedRigidbody.isKinematic;
        }
    }

    private void LateUpdate()
    {
        if (!isPlaced || currentSlot == null)
        {
            return;
        }

        transform.SetPositionAndRotation(currentSlot.GetSnapPosition(), currentSlot.GetSnapRotation());
    }

    /// <summary>
    /// Llamar cuando el jugador toma la etiqueta.
    /// (Ejemplo XR: Select Entered)
    /// </summary>
    public void OnGrabbed()
    {
        if (currentSlot != null)
        {
            currentSlot.ClearCurrentLabel(this);
        }
    }

    /// <summary>
    /// Llamar cuando el jugador suelta la etiqueta.
    /// (Ejemplo XR: Select Exited)
    /// </summary>
    public void OnReleasedTryPlace()
    {
        TryPlaceInNearestSlot();
    }

    [ContextMenu("Try Place In Nearest Slot")]
    public bool TryPlaceInNearestSlot()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            autoPlaceRadius,
            slotLayerMask,
            QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0)
        {
            if (showLogs)
            {
                Debug.LogWarning($"[LabelTagItem] {name} no encontró colliders de slot dentro del radio {autoPlaceRadius}.");
            }
            return false;
        }

        LabelObjectSlot nearestSlot = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            LabelObjectSlot slot = hits[i].GetComponentInParent<LabelObjectSlot>();
            if (slot == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, slot.GetSnapPosition());
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestSlot = slot;
            }
        }

        if (nearestSlot == null)
        {
            if (showLogs)
            {
                Debug.LogWarning($"[LabelTagItem] {name} encontró colliders, pero ninguno tenía LabelObjectSlot.");
            }
            return false;
        }

        bool assigned = nearestSlot.TryAssignLabel(this);

        if (showLogs)
        {
            Debug.Log($"[LabelTagItem] {name} intento de snap en {nearestSlot.name}: {assigned}");
        }

        return assigned;
    }

    public void SetPlacementState(LabelObjectSlot slot, bool placed)
    {
        currentSlot = slot;
        isPlaced = placed;

        if (cachedRigidbody != null && lockRigidbodyWhenPlaced)
        {
            cachedRigidbody.isKinematic = placed;
            cachedRigidbody.useGravity = !placed;
            cachedRigidbody.constraints = placed ? RigidbodyConstraints.FreezeAll : originalConstraints;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;

            if (!placed)
            {
                cachedRigidbody.useGravity = originalUseGravity;
                cachedRigidbody.isKinematic = originalIsKinematic;
            }
        }

        if (showLogs)
        {
            Debug.Log($"[LabelTagItem] {name} -> placed={placed}, slot={(slot != null ? slot.name : "None")}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, autoPlaceRadius);
    }
}
