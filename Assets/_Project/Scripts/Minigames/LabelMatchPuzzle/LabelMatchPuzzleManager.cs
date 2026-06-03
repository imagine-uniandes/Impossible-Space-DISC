using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Manager del puzzle de etiquetas. Solo valida al final cuando todos los slots tienen etiqueta.
/// Si todo es correcto dispara evento de éxito (útil para abrir/destruir puerta).
/// </summary>
public class LabelMatchPuzzleManager : MonoBehaviour
{
    /// <summary>
    /// Acción aplicada a las etiquetas colocadas al resolver el puzzle.
    /// </summary>
    public enum LabelOnSolvedAction
    {
        None,     // No tocar las etiquetas
        Disable,  // Desactivar (ocultar) la etiqueta completa
        Blacken   // Teñir texto/materiales de negro para que se fundan con la oscuridad
    }

    [Header("References")]
    [Tooltip("Slots del puzzle. Si está vacío puede buscarlos automáticamente en hijos.")]
    public LabelObjectSlot[] slots;

    [Tooltip("Buscar slots en hijos si no fueron asignados")]
    public bool autoFindSlotsIfEmpty = true;

    [Header("Results UI")]
    [Tooltip("Contenedor general del panel de resultados")]
    public GameObject resultsRoot;

    [Tooltip("Objeto visual para resultado correcto")]
    public GameObject successVisual;

    [Tooltip("Objeto visual para resultado incorrecto")]
    public GameObject failVisual;

    [Tooltip("Pool de visuales de fallo. Si hay elementos, se elige uno aleatorio en cada fallo.")]
    public GameObject[] failVisualPool;

    [Header("Contextual Fail Result")]
    [Tooltip("Si está activo, en fallo muestra un resultado basado en un slot mal colocado")]
    public bool useContextualFailResult = true;

    [Tooltip("Punto donde se instancia el objeto del slot incorrecto")]
    public Transform failObjectSpawnPoint;

    [Tooltip("Texto 3D/TMP donde se mostrará la etiqueta incorrecta + '?'")]
    public TMP_Text failLabelText;

    [Tooltip("Si está activo, añade '?' al final del texto mostrado")]
    public bool appendQuestionMarkToFailText = true;

    [Header("Behavior")]
    [Tooltip("Si está activo, al resolver bien no vuelve a evaluar")]
    public bool lockAfterSuccess = true;

    [Header("Etiquetas al resolver (oscuridad)")]
    [Tooltip("Qué hacer con las etiquetas colocadas cuando se resuelve el puzzle (justo cuando se apagan las luces), para que no se sigan viendo blancas.")]
    public LabelOnSolvedAction labelsOnSolved = LabelOnSolvedAction.Blacken;

    [Tooltip("Color al que se tiñe el texto y los materiales de las etiquetas cuando labelsOnSolved = Blacken.")]
    public Color blackenColor = Color.black;

    [Header("Feedback de slots (resaltado con pulso)")]
    [Tooltip("Al fallar, resalta cada slot con su LabelSlotHighlighter (rojo = mal).")]
    public bool highlightSlotsOnFail = true;

    [Tooltip("Además del rojo en los incorrectos, resaltar en verde los slots correctos al fallar.")]
    public bool alsoHighlightCorrectSlots = true;

    [Header("Events")]
    [Tooltip("Se invoca cuando todo está correcto (aquí conectas tu script de puerta)")]
    public UnityEvent onPuzzleSolved;

    [Tooltip("Se invoca cuando hay al menos una etiqueta mal")]
    public UnityEvent onPuzzleFailed;

    [Tooltip("Se invoca en cualquier evaluación final")]
    public UnityEvent onPuzzleChecked;

    [Header("Debug")]
    public bool showLogs = true;

    private bool isSolved;
    private GameObject activeContextFailObjectInstance;

    private void Awake()
    {
        if ((slots == null || slots.Length == 0) && autoFindSlotsIfEmpty)
        {
            slots = GetComponentsInChildren<LabelObjectSlot>(true);
        }

        SetResultsVisible(false, false, false);
    }

    /// <summary>
    /// Lo llaman los slots cuando cambia una etiqueta.
    /// </summary>
    public void NotifySlotStateChanged()
    {
        if (isSolved && lockAfterSuccess)
        {
            return;
        }

        if (!AreAllSlotsFilled())
        {
            SetResultsVisible(false, false, false);
            return;
        }

        EvaluateAll();
    }

    [ContextMenu("Evaluate Now")]
    public void EvaluateAll()
    {
        if (slots == null || slots.Length == 0)
        {
            if (showLogs)
            {
                Debug.LogWarning("[LabelMatchPuzzleManager] No hay slots configurados.");
            }
            return;
        }

        bool allCorrect = true;

        for (int i = 0; i < slots.Length; i++)
        {
            LabelObjectSlot slot = slots[i];
            if (slot == null || !slot.IsCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        onPuzzleChecked?.Invoke();

        if (allCorrect)
        {
            isSolved = true;
            SetResultsVisible(true, true, false);
            onPuzzleSolved?.Invoke();

            // Justo después de apagar las luces (cableado en onPuzzleSolved),
            // ocultamos/oscurecemos las etiquetas para que no se sigan viendo.
            ApplyLabelsOnSolved();

            if (showLogs)
            {
                Debug.Log("<color=lime>[LabelMatchPuzzleManager] ✅ Puzzle resuelto correctamente.</color>");
            }
        }
        else
        {
            SetResultsVisible(true, false, true);

            if (highlightSlotsOnFail)
            {
                HighlightSlotsFeedback();
            }

            onPuzzleFailed?.Invoke();

            if (showLogs)
            {
                Debug.Log("<color=yellow>[LabelMatchPuzzleManager] ⚠️ Hay etiquetas incorrectas. Reacomoda y prueba de nuevo.</color>");
            }
        }
    }

    /// <summary>
    /// Resalta cada slot según si su etiqueta es correcta (verde) o incorrecta (rojo),
    /// usando el LabelSlotHighlighter de cada slot. El pulso y su duración se configuran
    /// en cada highlighter.
    /// </summary>
    private void HighlightSlotsFeedback()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            LabelObjectSlot slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            // Busca el highlighter en el slot (o sus hijos) y, si no, en la etiqueta
            // que tiene puesta (la palabra). Así funciona lo pongas donde lo pongas.
            LabelSlotHighlighter highlighter = slot.GetComponentInChildren<LabelSlotHighlighter>();
            if (highlighter == null && slot.CurrentLabel != null)
            {
                highlighter = slot.CurrentLabel.GetComponentInChildren<LabelSlotHighlighter>();
            }

            if (highlighter == null)
            {
                continue;
            }

            bool correct = slot.IsCorrect();
            if (!correct)
            {
                highlighter.Flash(false);
            }
            else if (alsoHighlightCorrectSlots)
            {
                highlighter.Flash(true);
            }
        }
    }

    /// <summary>
    /// Aplica la acción configurada (ocultar / ennegrecer) a las etiquetas
    /// colocadas en los slots. Se llama al resolver el puzzle.
    /// </summary>
    private void ApplyLabelsOnSolved()
    {
        if (labelsOnSolved == LabelOnSolvedAction.None || slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            LabelObjectSlot slot = slots[i];
            if (slot == null || slot.CurrentLabel == null)
            {
                continue;
            }

            GameObject labelGo = slot.CurrentLabel.gameObject;

            if (labelsOnSolved == LabelOnSolvedAction.Disable)
            {
                labelGo.SetActive(false);
                continue;
            }

            BlackenLabel(labelGo);
        }
    }

    /// <summary>
    /// Tiñe de negro todos los textos TMP y materiales de la etiqueta para que
    /// se funda con la oscuridad cuando se apagan las luces.
    /// </summary>
    private void BlackenLabel(GameObject labelGo)
    {
        TMP_Text[] texts = labelGo.GetComponentsInChildren<TMP_Text>(true);
        for (int t = 0; t < texts.Length; t++)
        {
            if (texts[t] != null)
            {
                texts[t].color = blackenColor;
            }
        }

        Renderer[] renderers = labelGo.GetComponentsInChildren<Renderer>(true);
        for (int r = 0; r < renderers.Length; r++)
        {
            if (renderers[r] == null)
            {
                continue;
            }

            Material[] materials = renderers[r].materials;
            for (int m = 0; m < materials.Length; m++)
            {
                Material mat = materials[m];
                if (mat == null)
                {
                    continue;
                }

                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", blackenColor);
                }

                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", blackenColor);
                }

                // Si el material emite (etiquetas brillantes), apagamos la emisión.
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    public void ResetPuzzleState()
    {
        isSolved = false;
        SetResultsVisible(false, false, false);

        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].ClearCurrentLabel();
            }
        }
    }

    private bool AreAllSlotsFilled()
    {
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || !slots[i].HasLabel)
            {
                return false;
            }
        }

        return true;
    }

    private void SetResultsVisible(bool root, bool success, bool fail)
    {
        if (resultsRoot != null)
        {
            resultsRoot.SetActive(root);
        }

        if (successVisual != null)
        {
            successVisual.SetActive(success);
        }

        SetAllFailVisualsInactive();
        ClearContextFailVisual();

        if (!fail)
        {
            return;
        }

        if (useContextualFailResult && TryShowContextualFailVisual())
        {
            return;
        }

        GameObject randomFail = GetRandomFailVisualFromPool();
        if (randomFail != null)
        {
            randomFail.SetActive(true);
            return;
        }

        if (failVisual != null)
        {
            failVisual.SetActive(fail);
        }
    }

    private void SetAllFailVisualsInactive()
    {
        if (failVisual != null)
        {
            failVisual.SetActive(false);
        }

        if (failVisualPool == null || failVisualPool.Length == 0)
        {
            return;
        }

        for (int i = 0; i < failVisualPool.Length; i++)
        {
            if (failVisualPool[i] != null)
            {
                failVisualPool[i].SetActive(false);
            }
        }
    }

    private bool TryShowContextualFailVisual()
    {
        LabelObjectSlot wrongSlot = GetFirstWrongSlotWithLabel();
        if (wrongSlot == null)
        {
            return false;
        }

        bool hasAnyContextVisual = false;

        if (failLabelText != null)
        {
            string failText = wrongSlot.GetCurrentLabelDisplayText();
            if (!string.IsNullOrWhiteSpace(failText))
            {
                failLabelText.text = appendQuestionMarkToFailText ? failText + "?" : failText;
                failLabelText.gameObject.SetActive(true);
                hasAnyContextVisual = true;
            }
        }

        if (wrongSlot.slotResultObjectSource != null && failObjectSpawnPoint != null)
        {
            activeContextFailObjectInstance = Instantiate(
                wrongSlot.slotResultObjectSource,
                failObjectSpawnPoint.position,
                failObjectSpawnPoint.rotation,
                failObjectSpawnPoint);

            hasAnyContextVisual = true;
        }

        return hasAnyContextVisual;
    }

    private LabelObjectSlot GetFirstWrongSlotWithLabel()
    {
        if (slots == null || slots.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            LabelObjectSlot slot = slots[i];
            if (slot != null && slot.HasLabel && !slot.IsCorrect())
            {
                return slot;
            }
        }

        return null;
    }

    private void ClearContextFailVisual()
    {
        if (activeContextFailObjectInstance != null)
        {
            Destroy(activeContextFailObjectInstance);
            activeContextFailObjectInstance = null;
        }

        if (failLabelText != null)
        {
            failLabelText.text = string.Empty;
            failLabelText.gameObject.SetActive(false);
        }
    }

    private GameObject GetRandomFailVisualFromPool()
    {
        if (failVisualPool == null || failVisualPool.Length == 0)
        {
            return null;
        }

        int validCount = 0;
        for (int i = 0; i < failVisualPool.Length; i++)
        {
            if (failVisualPool[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        int selectedIndex = Random.Range(0, validCount);
        int currentValid = 0;

        for (int i = 0; i < failVisualPool.Length; i++)
        {
            if (failVisualPool[i] == null)
            {
                continue;
            }

            if (currentValid == selectedIndex)
            {
                return failVisualPool[i];
            }

            currentValid++;
        }

        return null;
    }
}
