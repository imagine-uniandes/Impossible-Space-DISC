using UnityEngine;
using TMPro;

/// <summary>
/// Mini puzzle visual: cambia el color de quads y oculta un texto TMP.
/// Diseñado para llamarse desde un botón/evento de Unity (por ejemplo en VR).
/// </summary>
public class MiniPuzzleVisualSwitch : MonoBehaviour
{
    [Header("Quad Targets")]
    [Tooltip("Renderers de los quads que cambiarán de color")]
    public Renderer[] quadsToColor;

    [Tooltip("Si está activo y no hay quads asignados, busca renderers en hijos")]
    public bool autoFindChildrenIfEmpty = true;

    [Header("TMP Target")]
    [Tooltip("Texto TMP a ocultar cuando se ejecute el puzzle")]
    public TMP_Text textToHide;

    [Header("Visual Result")]
    [Tooltip("Color final de todos los quads")]
    public Color solvedColor = Color.green;

    [Header("Behavior")]
    [Tooltip("Permitir ejecutar solo una vez")]
    public bool executeOnlyOnce = true;

    [Tooltip("Mostrar logs en consola")]
    public bool showLogs = true;

    private bool hasExecuted;

    /// <summary>
    /// Ejecuta el resultado del mini puzzle.
    /// </summary>
    public void ExecutePuzzle()
    {
        if (executeOnlyOnce && hasExecuted)
        {
            return;
        }

        if ((quadsToColor == null || quadsToColor.Length == 0) && autoFindChildrenIfEmpty)
        {
            quadsToColor = GetComponentsInChildren<Renderer>(true);
        }

        int changedCount = 0;

        if (quadsToColor != null)
        {
            foreach (Renderer renderer in quadsToColor)
            {
                if (renderer == null) continue;

                ColorizeRenderer(renderer);
                changedCount++;
            }
        }

        if (textToHide != null)
        {
            textToHide.gameObject.SetActive(false);
        }

        hasExecuted = true;

        if (showLogs)
        {
            Debug.Log($"<color=lime>[MiniPuzzleVisualSwitch] ✅ Ejecutado. Quads cambiados: {changedCount}. Texto oculto: {(textToHide != null)}</color>");
        }
    }

    [ContextMenu("Execute Puzzle (Test)")]
    public void TestExecutePuzzle()
    {
        ExecutePuzzle();
    }

    private void ColorizeRenderer(Renderer renderer)
    {
        Material[] materials = renderer.materials;

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null) continue;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", solvedColor);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", solvedColor);
            }
        }
    }
}
