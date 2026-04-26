using UnityEngine;

/// <summary>
/// Trigger en la zona de la puerta. Cuando detecta una CodeKey:
///   - Si el código no está resuelto → feedback negativo.
///   - Si la llave coincide con el color elegido en el código → abre la puerta via ObjectToggler.
///   - Si la llave no coincide → feedback negativo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CodeKeyDoorZone : MonoBehaviour
{
    [Header("References")]
    public CodePuzzleManager puzzleManager;
    public ObjectToggler doorToggler;

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

        string correct = puzzleManager.GetCorrectColor();

        if (string.Equals(key.KeyColor, correct, System.StringComparison.OrdinalIgnoreCase))
        {
            hasOpened = true;
            PlaySound(successSound);
            doorToggler?.Disable();

            if (showLogs)
                Debug.Log($"<color=lime>[CodeKeyDoorZone] ✅ Llave '{key.KeyColor}' correcta. Puerta abierta.</color>");
        }
        else
        {
            PlaySound(failSound);

            if (showLogs)
                Debug.Log($"<color=yellow>[CodeKeyDoorZone] ❌ Llave '{key.KeyColor}' incorrecta. El código dice '{correct}'.</color>");
        }
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
