using UnityEngine;

/// <summary>
/// Palabra 3D agarrable para el puzzle de código.
/// Conectar OnGrabbed() y OnReleasedTryPlace() desde los eventos XR (Select Entered / Select Exited).
/// </summary>
public class CodePuzzleWord : MonoBehaviour
{
    [Header("Data")]
    public string wordValue;

    [Header("Auto Place")]
    [Min(0.01f)]
    public float autoPlaceRadius = 0.25f;
    public LayerMask slotLayerMask = ~0;
    public bool lockWhenPlaced = true;

    [Header("Debug")]
    public bool showLogs = false;

    private Rigidbody rb;
    private CodeWordSlot currentSlot;
    private bool isPlaced;
    private RigidbodyConstraints originalConstraints;
    private bool originalUseGravity;
    private bool originalIsKinematic;

    public string WordValue => wordValue;
    public bool IsPlaced => isPlaced;
    public CodeWordSlot CurrentSlot => currentSlot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalConstraints = rb.constraints;
            originalUseGravity = rb.useGravity;
            originalIsKinematic = rb.isKinematic;
        }
    }

    private void LateUpdate()
    {
        if (!isPlaced || currentSlot == null) return;
        transform.SetPositionAndRotation(currentSlot.GetSnapPosition(), currentSlot.GetSnapRotation());
    }

    /// <summary>Llamar desde Select Entered del XR Interactable.</summary>
    public void OnGrabbed()
    {
        currentSlot?.ClearCurrentWord(this);
    }

    /// <summary>Llamar desde Select Exited del XR Interactable.</summary>
    public void OnReleasedTryPlace()
    {
        TryPlaceInNearestSlot();
    }

    [ContextMenu("Try Place In Nearest Slot")]
    public bool TryPlaceInNearestSlot()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, autoPlaceRadius, slotLayerMask, QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0) return false;

        CodeWordSlot nearest = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            CodeWordSlot slot = hits[i].GetComponentInParent<CodeWordSlot>();
            if (slot == null) continue;

            float dist = Vector3.Distance(transform.position, slot.GetSnapPosition());
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = slot;
            }
        }

        if (nearest == null) return false;

        bool assigned = nearest.TryAssignWord(this);
        if (showLogs) Debug.Log($"[CodePuzzleWord] {name} snap en {nearest.slotId}: {assigned}");
        return assigned;
    }

    public void SetPlacementState(CodeWordSlot slot, bool placed)
    {
        currentSlot = slot;
        isPlaced = placed;

        if (rb != null && lockWhenPlaced)
        {
            rb.isKinematic = placed;
            rb.useGravity = !placed;
            rb.constraints = placed ? RigidbodyConstraints.FreezeAll : originalConstraints;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (!placed)
            {
                rb.useGravity = originalUseGravity;
                rb.isKinematic = originalIsKinematic;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, autoPlaceRadius);
    }
}
