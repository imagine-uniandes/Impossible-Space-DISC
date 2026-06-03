using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Muestra una pista contextual cuando el jugador falla el puzzle de código.
/// Conectar ShowHint() al evento onPuzzleFailed del CodePuzzleManager.
/// Conectar HideHint() al evento onPuzzleSolved del CodePuzzleManager.
/// </summary>
public class CodePuzzleHintDisplay : MonoBehaviour
{
    [Header("Referencias")]
    public CodePuzzleManager puzzleManager;

    [Header("UI")]
    [Tooltip("Texto TMP donde aparece la pista (3D o Canvas)")]
    public TMP_Text hintText;

    [Tooltip("Panel raíz a mostrar/ocultar (opcional, puede ser el mismo GO que el texto)")]
    public GameObject hintPanel;

    [Header("Mensajes por slot")]
    [Tooltip("Slot objeto incorrecto (se espera 'llave')")]
    public string hintObjeto = "¿Qué tipo de objeto abre puertas?";

    [Tooltip("Slot operador incorrecto (se esperan '==' o '!=')")]
    public string hintOperador = "El operador compara valores. Prueba con == o !=.";

    [Tooltip("Slot color: no hay llave de ese color en la sala")]
    public string hintColor = "¿Hay una llave de ese color en la sala?";

    [Tooltip("Slot función incorrecto (se espera 'abrir')")]
    public string hintFuncion = "¿Qué función hace que algo se abra?";

    [Tooltip("Slot booleano incorrecto (se espera 'true')")]
    public string hintBooleano = "¿Verdadero o falso? Piénsalo bien.";

    [Tooltip("Mensaje genérico si ningún slot está claramente mal")]
    public string hintGeneral = "Algo en el código no está bien. Revísalo con calma.";

    [Header("Comportamiento")]
    [Tooltip("Segundos antes de ocultarse automáticamente (0 = no se oculta solo)")]
    [Min(0f)]
    public float autoHideDelay = 5f;

    [Header("Debug")]
    public bool showLogs = false;

    private Coroutine hideCoroutine;

    private void Start()
    {
        SetPanelActive(false);
    }

    /// <summary>
    /// Llamar desde onPuzzleFailed del CodePuzzleManager.
    /// Detecta el primer slot incorrecto y muestra la pista correspondiente.
    /// </summary>
    public void ShowHint()
    {
        if (puzzleManager == null)
        {
            if (showLogs) Debug.LogWarning("[HintDisplay] No hay CodePuzzleManager asignado.");
            return;
        }

        string message = DetermineHintMessage();
        Display(message);
    }

    /// <summary>
    /// Llamar desde onPuzzleSolved del CodePuzzleManager para ocultar la pista.
    /// </summary>
    public void HideHint()
    {
        CancelHideTimer();
        SetPanelActive(false);
    }

    private string DetermineHintMessage()
    {
        // Comprobar slots fijos en el orden en que aparecen en la línea de código
        if (SlotFilledAndWrong(puzzleManager.objetoSlot))
            return hintObjeto;

        // El operador es válido si es uno de los aceptados ('==' o '!='), no un valor fijo.
        if (puzzleManager.operadorSlot != null && puzzleManager.operadorSlot.HasWord
            && !puzzleManager.IsValidOperator(puzzleManager.operadorSlot.GetCurrentValue()))
            return hintOperador;

        // Color: el slot acepta cualquier valor, pero debe existir llave de ese color
        if (puzzleManager.colorSlot != null && puzzleManager.colorSlot.HasWord)
        {
            string chosen = puzzleManager.colorSlot.GetCurrentValue();
            if (!IsValidColor(chosen))
                return hintColor;
        }

        if (SlotFilledAndWrong(puzzleManager.funcionSlot))
            return hintFuncion;

        if (SlotFilledAndWrong(puzzleManager.booleanoSlot))
            return hintBooleano;

        return hintGeneral;
    }

    private static bool SlotFilledAndWrong(CodeWordSlot slot)
    {
        return slot != null && slot.HasWord && slot.IsFixed && !slot.IsCorrect();
    }

    private bool IsValidColor(string color)
    {
        if (string.IsNullOrEmpty(color) || puzzleManager.validColors == null)
            return false;

        foreach (string valid in puzzleManager.validColors)
        {
            if (string.Equals(valid, color, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private void Display(string message)
    {
        if (hintText != null)
            hintText.text = message;

        SetPanelActive(true);

        if (showLogs)
            Debug.Log($"[HintDisplay] Pista: {message}");

        if (autoHideDelay > 0f)
        {
            CancelHideTimer();
            hideCoroutine = StartCoroutine(AutoHide(autoHideDelay));
        }
    }

    private IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPanelActive(false);
        hideCoroutine = null;
    }

    private void CancelHideTimer()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private void SetPanelActive(bool active)
    {
        if (hintPanel != null)
            hintPanel.SetActive(active);
        else if (hintText != null)
            hintText.gameObject.SetActive(active);
    }
}
