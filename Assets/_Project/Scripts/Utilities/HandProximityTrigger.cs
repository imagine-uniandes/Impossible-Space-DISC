using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Activa un ObjectToggler cuando la mano del jugador entra al trigger.
/// Úsalo como botón físico VR con un collider invisible.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HandProximityTrigger : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("ObjectToggler a ejecutar cuando la mano entre al trigger")]
    public ObjectToggler objectToggler;

    [Tooltip("Acción a ejecutar: Disable, Enable o Toggle")]
    public ObjectToggler.ToggleAction actionToCall = ObjectToggler.ToggleAction.Disable;

    [Header("Trigger Settings")]
    [Tooltip("Tags que se consideran 'mano del jugador'")]
    public string[] handTags = new string[] { "Hand", "HandLeft", "HandRight", "Player" };

    [Tooltip("Buscar también en objetos padre del collider que entra")]
    public bool checkParentTags = true;

    [Tooltip("Solo activarse una vez")]
    public bool triggerOnlyOnce = true;

    [Header("Events")]
    public UnityEvent onHandEnter;

    [Header("Debug")]
    public bool showLogs = true;

    private Collider triggerCollider;
    private bool hasTriggered;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHand(other)) return;
        if (triggerOnlyOnce && hasTriggered) return;

        hasTriggered = true;

        onHandEnter?.Invoke();
        ExecuteAction();

        if (disableAfterUse)
            triggerCollider.enabled = false;

        if (showLogs)
            Debug.Log($"<color=lime>[HandProximityTrigger] ✅ Mano detectada en '{gameObject.name}' — acción: {actionToCall}</color>");
    }

    [Header("Behaviour")]
    [Tooltip("Desactivar el collider después de usarse (evita doble activación)")]
    public bool disableAfterUse = true;

    private void ExecuteAction()
    {
        if (objectToggler == null)
        {
            Debug.LogError("<color=red>[HandProximityTrigger] ❌ ObjectToggler no asignado</color>");
            return;
        }

        switch (actionToCall)
        {
            case ObjectToggler.ToggleAction.Disable:
                objectToggler.Disable();
                break;
            case ObjectToggler.ToggleAction.Enable:
                objectToggler.Enable();
                break;
            case ObjectToggler.ToggleAction.Toggle:
                objectToggler.Toggle();
                break;
        }
    }

    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    private bool IsHand(Collider other)
    {
        if (other == null) return false;

        if (HasAnyHandTag(other.transform)) return true;

        if (!checkParentTags) return false;

        Transform current = other.transform.parent;
        while (current != null)
        {
            if (HasAnyHandTag(current)) return true;
            current = current.parent;
        }

        return false;
    }

    private bool HasAnyHandTag(Transform t)
    {
        if (t == null || handTags == null) return false;
        string tag = t.tag;
        for (int i = 0; i < handTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(handTags[i]) && tag == handTags[i])
                return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.05f, "HandProximityTrigger");
        #endif
    }
}
