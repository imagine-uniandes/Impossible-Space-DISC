using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manager del puzzle de código.
/// El usuario completa 5 huecos: objeto, operador, color (dinámico), función y booleano.
/// El color elegido determina qué llave abre la puerta — sin respuesta única prefijada.
/// </summary>
public class CodePuzzleManager : MonoBehaviour
{
    [Header("Slots fijos (respuesta definida)")]
    [Tooltip("Slot del objeto (espera 'llave'). Enseña el concepto de variable/referencia.")]
    public CodeWordSlot objetoSlot;

    [Tooltip("Slot del operador (espera '==').")]
    public CodeWordSlot operadorSlot;

    [Tooltip("Slot de la función (espera 'abrir').")]
    public CodeWordSlot funcionSlot;

    [Tooltip("Slot del booleano (espera 'true').")]
    public CodeWordSlot booleanoSlot;

    [Header("Slot dinámico")]
    [Tooltip("Slot del color: el usuario elige libremente. Determina qué llave abre la puerta.")]
    public CodeWordSlot colorSlot;

    [Header("Colores válidos")]
    [Tooltip("Solo estos colores tienen llave física. Cualquier otro color será distractor.")]
    public string[] validColors = { "azul", "rojo", "verde" };

    [Header("Operadores válidos")]
    [Tooltip("Operadores aceptados en el slot de operador. '==' exige la llave del color elegido; '!=' exige una llave de cualquier OTRO color válido.")]
    public string[] validOperators = { "==", "!=" };

    [Header("Feedback de pantalla")]
    public GameObject codeSuccessVisual;
    public GameObject codeErrorVisual;

    [Header("Behavior")]
    public bool lockAfterSuccess = true;

    [Header("Events")]
    [Tooltip("Cuando el código es 100% correcto y el color elegido tiene una llave.")]
    public UnityEvent onPuzzleSolved;
    [Tooltip("Cuando hay algún error (slot fijo incorrecto o color sin llave).")]
    public UnityEvent onPuzzleFailed;

    [Header("Debug")]
    public bool showLogs = true;

    private bool isSolved;
    private string correctColor;
    private string chosenOperator;

    public bool IsSolved => isSolved;

    /// <summary>Retorna el color que el usuario eligió en el código.</summary>
    public string GetCorrectColor() => correctColor;

    /// <summary>Retorna el operador que el usuario eligió ('==' o '!=').</summary>
    public string GetChosenOperator() => chosenOperator;

    /// <summary>Llamado automáticamente por los slots al cambiar de estado.</summary>
    public void NotifySlotStateChanged()
    {
        if (isSolved && lockAfterSuccess) return;

        SetFeedbackVisible(false, false);

        if (!AreAllSlotsFilled()) return;

        Evaluate();
    }

    [ContextMenu("Evaluate Now")]
    public void Evaluate()
    {
        bool fixedCorrect =
            CheckFixed(objetoSlot) &&
            CheckFixed(funcionSlot) &&
            CheckFixed(booleanoSlot);

        // El operador es semi-dinámico: aceptamos '==' o '!=' (configurable en validOperators).
        string chosenOp = operadorSlot != null ? operadorSlot.GetCurrentValue() : string.Empty;
        bool operatorValid = IsValidOperator(chosenOp);

        string chosenColor = colorSlot != null ? colorSlot.GetCurrentValue() : string.Empty;
        bool colorValid = IsValidColor(chosenColor);

        if (fixedCorrect && operatorValid && colorValid)
        {
            correctColor = chosenColor.ToLower();
            chosenOperator = chosenOp.Trim();
            isSolved = true;
            SetFeedbackVisible(true, false);
            onPuzzleSolved?.Invoke();

            if (showLogs)
                Debug.Log($"<color=lime>[CodePuzzleManager] ✅ Código correcto. Operador '{chosenOperator}', color '{correctColor}'.</color>");
        }
        else
        {
            SetFeedbackVisible(false, true);
            onPuzzleFailed?.Invoke();

            if (showLogs)
            {
                if (!fixedCorrect)
                    Debug.Log("<color=yellow>[CodePuzzleManager] ⚠️ Error de sintaxis en el código.</color>");
                else if (!operatorValid)
                    Debug.Log($"<color=yellow>[CodePuzzleManager] ⚠️ Operador '{chosenOp}' no válido. Usa == o !=.</color>");
                else
                    Debug.Log($"<color=yellow>[CodePuzzleManager] ⚠️ Color '{chosenColor}' no tiene llave en la escena.</color>");
            }
        }
    }

    private bool CheckFixed(CodeWordSlot slot)
    {
        if (slot == null) return true; // slot no configurado, se ignora
        return slot.IsCorrect();
    }

    public bool IsValidOperator(string op)
    {
        if (string.IsNullOrEmpty(op) || validOperators == null) return false;
        string trimmed = op.Trim();
        for (int i = 0; i < validOperators.Length; i++)
        {
            if (string.Equals(validOperators[i], trimmed, System.StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Decide si una llave de color dado abre la puerta, según el operador elegido:
    ///   '==' → solo la llave del color elegido.
    ///   '!=' → cualquier llave de color válido EXCEPTO el elegido.
    /// </summary>
    public bool IsKeyAccepted(string keyColor)
    {
        if (!isSolved || string.IsNullOrEmpty(keyColor)) return false;

        bool matchesChosen = string.Equals(keyColor, correctColor, System.StringComparison.OrdinalIgnoreCase);

        if (chosenOperator == "!=")
            return IsValidColor(keyColor) && !matchesChosen;

        // Por defecto / '==': la llave debe coincidir con el color elegido.
        return matchesChosen;
    }

    private bool IsValidColor(string color)
    {
        if (string.IsNullOrEmpty(color) || validColors == null) return false;
        for (int i = 0; i < validColors.Length; i++)
        {
            if (string.Equals(validColors[i], color, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool AreAllSlotsFilled()
    {
        return SlotFilled(objetoSlot)
            && SlotFilled(operadorSlot)
            && SlotFilled(colorSlot)
            && SlotFilled(funcionSlot)
            && SlotFilled(booleanoSlot);
    }

    private static bool SlotFilled(CodeWordSlot slot) => slot == null || slot.HasWord;

    private void SetFeedbackVisible(bool success, bool error)
    {
        if (codeSuccessVisual != null) codeSuccessVisual.SetActive(success);
        if (codeErrorVisual != null) codeErrorVisual.SetActive(error);
    }

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle()
    {
        isSolved = false;
        correctColor = string.Empty;
        chosenOperator = string.Empty;
        SetFeedbackVisible(false, false);

        objetoSlot?.ClearCurrentWord(sendToOrigin: true);
        operadorSlot?.ClearCurrentWord(sendToOrigin: true);
        colorSlot?.ClearCurrentWord(sendToOrigin: true);
        funcionSlot?.ClearCurrentWord(sendToOrigin: true);
        booleanoSlot?.ClearCurrentWord(sendToOrigin: true);
    }
}
