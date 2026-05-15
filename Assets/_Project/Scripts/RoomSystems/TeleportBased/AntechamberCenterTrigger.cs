using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Sustituye al SpawnTransitionTrigger en el centro de la antecámara.
/// El jugador debe permanecer dentro del área X segundos para avanzar.
/// Si sale antes de que termine, el contador se reinicia.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AntechamberCenterTrigger : MonoBehaviour
{
    [Header("Countdown")]
    [Tooltip("Segundos que el jugador debe permanecer en el centro")]
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
        if (hasAdvanced) return;
        if (!IsPlayer(other)) return;
        if (countdownCoroutine != null) return;

        countdownCoroutine = StartCoroutine(Countdown());
    }

    private void OnTriggerExit(Collider other)
    {
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

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        countdownCoroutine = null;
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
