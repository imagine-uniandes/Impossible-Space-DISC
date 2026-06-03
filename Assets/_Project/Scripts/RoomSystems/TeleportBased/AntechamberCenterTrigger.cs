using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla cómo se avanza desde la antecámara a la siguiente sala.
///
/// Dos modos (campo advanceByButton):
///   • Cuenta atrás (por defecto): el jugador permanece en el centro X segundos.
///     Si sale antes de terminar, el contador se reinicia.
///   • Botón: cablea el onClick de un botón "Comenzar" a AdvanceNow() y se avanza
///     al pulsarlo (igual que la sala inicial). La cuenta atrás queda desactivada.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AntechamberCenterTrigger : MonoBehaviour
{
    [Header("Modo de avance")]
    [Tooltip("Si está activo, NO se usa la cuenta atrás del centro: se avanza al pulsar un botón cableado a AdvanceNow() (estilo sala inicial).")]
    public bool advanceByButton = false;

    [Header("Countdown")]
    [Tooltip("Segundos que el jugador debe permanecer en el centro (solo si advanceByButton está desactivado)")]
    [Range(1f, 15f)]
    public float countdownSeconds = 5f;

    [Header("References")]
    [Tooltip("TextMeshPro 3D que muestra el contador (hijo de este objeto o asignado manualmente)")]
    public TextMeshPro countdownText;

    [Header("Tags")]
    public string playerTag = "Player";
    public string[] additionalTags = new string[] { "Player" };

    private RoomSpawnManager manager;
    private Coroutine countdownCoroutine;
    private bool hasAdvanced = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        manager = FindObjectOfType<RoomSpawnManager>();

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (advanceByButton) return;
        if (hasAdvanced) return;
        if (!IsPlayer(other)) return;
        if (countdownCoroutine != null) return;

        countdownCoroutine = StartCoroutine(Countdown());
    }

    private void OnTriggerExit(Collider other)
    {
        if (advanceByButton) return;
        if (!IsPlayer(other)) return;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private IEnumerator Countdown()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float remaining = countdownSeconds;

        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(remaining).ToString();

            yield return null;
            remaining -= Time.deltaTime;
        }

        countdownCoroutine = null;
        DoAdvance();
    }

    /// <summary>
    /// Avanza a la siguiente sala de inmediato. Pensado para cablearse al onClick
    /// de un botón "Comenzar" (modo advanceByButton).
    /// </summary>
    public void AdvanceNow()
    {
        if (hasAdvanced) return;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        DoAdvance();
    }

    private void DoAdvance()
    {
        if (hasAdvanced) return;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        hasAdvanced = true;

        if (manager == null)
            manager = FindObjectOfType<RoomSpawnManager>();

        manager?.AdvanceToNextRoom();
    }

    private bool IsPlayer(Collider other)
    {
        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag(playerTag)) return true;
            foreach (string tag in additionalTags)
                if (!string.IsNullOrEmpty(tag) && t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }
}
