using UnityEngine;

/// <summary>
/// Trigger en la zona de la puerta. Cuando detecta una CodeKey:
///   - Si el código no está resuelto → feedback negativo.
///   - Si la llave coincide con el color elegido en el código → abre la puerta via ObjectToggler.
///   - Si la llave no coincide → feedback negativo.
/// Al abrir, la llave queda pegada a la puerta y no puede volver a agarrarse.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CodeKeyDoorZone : MonoBehaviour
{
    [Header("References")]
    public CodePuzzleManager puzzleManager;
    public ObjectToggler doorToggler;

    [Header("Key Attachment")]
    [Tooltip("Punto exacto donde la llave queda pegada al abrir. Si está vacío usa el doorTarget del toggler.")]
    public Transform keyAttachPoint;

    [Header("Audio")]
    public AudioClip successSound;
    public AudioClip failSound;
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Debug")]
    public bool showLogs = true;

    private bool hasOpened = false;
    private AudioSource audioSource;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasOpened) return;

        CodeKey key = other.GetComponentInParent<CodeKey>();
        if (key == null) return;

        if (puzzleManager == null)
        {
            Debug.LogError("[CodeKeyDoorZone] No hay CodePuzzleManager asignado.");
            return;
        }

        if (!puzzleManager.IsSolved)
        {
            PlaySound(failSound);
            if (showLogs) Debug.Log("[CodeKeyDoorZone] El código aún no está resuelto.");
            return;
        }

        if (puzzleManager.IsKeyAccepted(key.KeyColor))
        {
            hasOpened = true;
            PlaySound(successSound);
            doorToggler?.Disable();
            AttachKeyToDoor(key);

            if (showLogs)
                Debug.Log($"<color=lime>[CodeKeyDoorZone] ✅ Llave '{key.KeyColor}' correcta. Puerta abierta.</color>");
        }
        else
        {
            PlaySound(failSound);

            if (showLogs)
                Debug.Log($"<color=yellow>[CodeKeyDoorZone] ❌ Llave '{key.KeyColor}' incorrecta. Código: {puzzleManager.GetChosenOperator()} '{puzzleManager.GetCorrectColor()}'.</color>");
        }
    }

    private void AttachKeyToDoor(CodeKey key)
    {
        // Determinar punto de anclaje: keyAttachPoint > doorTarget del toggler > esta zona
        Transform anchor = keyAttachPoint;
        if (anchor == null && doorToggler != null && doorToggler.doorTarget != null)
            anchor = doorToggler.doorTarget.transform;
        if (anchor == null)
            anchor = transform;

        // Desactivar cualquier componente de grab/interactable (independiente del SDK usado)
        foreach (Behaviour b in key.GetComponentsInParent<Behaviour>())
        {
            string typeName = b.GetType().Name;
            if (typeName.Contains("Grab") || typeName.Contains("Interactable"))
                b.enabled = false;
        }

        // Congelar física
        Rigidbody rb = key.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Pegar al ancla
        key.transform.SetParent(anchor, worldPositionStays: true);

        if (keyAttachPoint != null)
        {
            key.transform.SetPositionAndRotation(keyAttachPoint.position, keyAttachPoint.rotation);
        }

        if (showLogs)
            Debug.Log($"<color=cyan>[CodeKeyDoorZone] 🔑 Llave '{key.KeyColor}' pegada a '{anchor.name}'.</color>");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    public void ResetZone() => hasOpened = false;

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "KeyDoorZone");
#endif
    }
}
