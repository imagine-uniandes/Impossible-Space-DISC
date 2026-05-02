using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Trigger para avanzar al siguiente prefab de habitación usando RoomSpawnManager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpawnTransitionTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Manager de aparición de habitaciones")]
    public RoomSpawnManager spawnManager;

    [Tooltip("Buscar automáticamente un RoomSpawnManager en escena si la referencia está vacía")]
    public bool autoFindSpawnManager = true;

    [Header("Trigger Settings")]
    [Tooltip("Tag principal que activa el trigger")]
    public string playerTag = "Player";

    [Tooltip("Tags adicionales válidos (ej: Hand, HandLeft, HandRight)")]
    public string[] additionalActivationTags = new string[] { "Hand", "Player" };

    [Tooltip("También validar tags en padres del collider que entra")]
    public bool checkParentTags = true;

    [Tooltip("Permitir solo una activación")]
    public bool triggerOnlyOnce = true;

    [Tooltip("Delay antes de ejecutar la transición")]
    [Range(0f, 3f)]
    public float transitionDelay = 0f;

    [Tooltip("Deshabilitar collider luego de activarse")]
    public bool disableColliderAfterUse = true;

    [Header("Events")]
    public UnityEvent onBeforeTransition;
    public UnityEvent onAfterTransition;

    [Header("Debug")]
    public bool showLogs = true;

    private Collider triggerCollider;
    private bool hasTriggered;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        TryResolveSpawnManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidActivator(other)) return;
        if (triggerOnlyOnce && hasTriggered) return;

        hasTriggered = true;

        if (transitionDelay > 0f)
        {
            StartCoroutine(ExecuteTransitionDelayed());
        }
        else
        {
            ExecuteTransition();
        }
    }

    private IEnumerator ExecuteTransitionDelayed()
    {
        yield return new WaitForSeconds(transitionDelay);
        ExecuteTransition();
    }

    private void ExecuteTransition()
    {
        TryResolveSpawnManager();

        if (spawnManager == null)
        {
            Debug.LogError("<color=red>[SpawnTransitionTrigger] ❌ spawnManager no asignado.</color>");
            return;
        }

        onBeforeTransition?.Invoke();
        spawnManager.AdvanceToNextRoom();
        onAfterTransition?.Invoke();

        if (disableColliderAfterUse && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (showLogs)
        {
            Debug.Log($"<color=lime>[SpawnTransitionTrigger] ✅ Trigger ejecutado en '{gameObject.name}'</color>");
        }
    }

    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

    public void SetSpawnManager(RoomSpawnManager manager)
    {
        spawnManager = manager;
    }

    private void TryResolveSpawnManager()
    {
        if (spawnManager != null || !autoFindSpawnManager)
        {
            return;
        }

        spawnManager = FindObjectOfType<RoomSpawnManager>();

        if (showLogs && spawnManager != null)
        {
            Debug.Log($"<color=cyan>[SpawnTransitionTrigger] 🔗 Manager auto-asignado en '{gameObject.name}'</color>");
        }
    }

    private bool IsValidActivator(Collider other)
    {
        if (other == null) return false;

        if (HasAnyValidTag(other.transform))
        {
            return true;
        }

        if (!checkParentTags)
        {
            return false;
        }

        Transform current = other.transform.parent;
        while (current != null)
        {
            if (HasAnyValidTag(current))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool HasAnyValidTag(Transform target)
    {
        if (target == null) return false;

        string targetTag = target.tag;

        if (!string.IsNullOrEmpty(playerTag) && targetTag == playerTag)
        {
            return true;
        }

        if (additionalActivationTags == null || additionalActivationTags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < additionalActivationTags.Length; i++)
        {
            string tag = additionalActivationTags[i];
            if (string.IsNullOrEmpty(tag)) continue;

            if (targetTag == tag)
            {
                return true;
            }
        }

        return false;
    }
}
