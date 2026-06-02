using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Resalta visualmente un slot del puzzle de etiquetas como feedback de acierto/error.
///
/// Va en el GameObject del slot (o en el objeto/texto que quieras teñir). Al llamar
/// Flash(correct), pone verde (correcto) o rojo (incorrecto) con un PULSO durante
/// 'duration' segundos y luego restaura el estado original.
///
/// Soporta dos tipos de visual:
///   • Mallas normales (URP/Built-in): tiñe el color base + pulso de emisión.
///   • Textos TextMeshPro 3D (Text TMP): los TMP NO tienen _BaseColor ni emisión,
///     así que se anima directamente su color (tmp.color) con pulso de brillo.
///
/// Lo dispara LabelMatchPuzzleManager en cada evaluación fallida; también puedes
/// llamar FlashCorrect()/FlashWrong() desde un UnityEvent.
/// </summary>
public class LabelSlotHighlighter : MonoBehaviour
{
    [Header("Objetivos a resaltar")]
    [Tooltip("Renderers (mallas normales). Si está vacío y autoFind está activo, busca en hijos.")]
    public Renderer[] renderers;
    [Tooltip("Textos TMP a resaltar. Si está vacío y autoFind está activo, busca en hijos.")]
    public TMP_Text[] texts;
    public bool autoFindIfEmpty = true;

    [Header("Colores")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    [Tooltip("Cuánto se tiñe el color base de las MALLAS hacia el color de feedback (0 = no tiñe, 1 = pleno).")]
    [Range(0f, 1f)] public float colorBlend = 0.6f;

    [Header("Pulso")]
    [Tooltip("Segundos que dura el resaltado antes de volver a la normalidad.")]
    public float duration = 5f;
    [Tooltip("Pulsos por segundo.")]
    public float pulseSpeed = 2f;
    [Tooltip("Intensidad máxima de emisión del pulso (solo mallas).")]
    [Range(0f, 5f)] public float maxEmission = 2.5f;
    [Tooltip("Brillo mínimo del texto TMP durante el pulso (0 = casi negro, 1 = color pleno).")]
    [Range(0f, 1f)] public float textMinBrightness = 0.25f;

    [Header("Materiales (mallas)")]
    [Tooltip("Propiedad de color base del material (_BaseColor en URP, _Color en Built-in).")]
    public string colorPropertyName = "_BaseColor";

    private Coroutine routine;

    // --- Estado mallas ---
    private Material[] mats;
    private Color[] originalColors;
    private bool[] hadColorProp;
    private Color[] originalEmission;
    private bool[] originalEmissionEnabled;

    // --- Estado TMP ---
    private Color[] originalTextColors;

    private bool captured;

    public void FlashCorrect() => Flash(true);
    public void FlashWrong() => Flash(false);

    /// <summary>Resalta este slot. correct=true → verde, false → rojo. Pulsa 'duration' segundos.</summary>
    public void Flash(bool correct)
    {
        EnsureCaptured();
        bool hasMats = mats != null && mats.Length > 0;
        bool hasTexts = texts != null && texts.Length > 0;
        if (!hasMats && !hasTexts) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine(correct ? correctColor : wrongColor));
    }

    private void EnsureCaptured()
    {
        if (captured) return;

        if (autoFindIfEmpty)
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
            if (texts == null || texts.Length == 0)
                texts = GetComponentsInChildren<TMP_Text>(true);
        }

        // Mallas: instancias de material (no afecta a otros objetos), excluyendo las de TMP.
        var matList = new List<Material>();
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                if (r.GetComponent<TMP_Text>() != null) continue; // los TMP se manejan aparte
                matList.AddRange(r.materials);
            }
        }

        mats = matList.ToArray();
        int n = mats.Length;
        originalColors = new Color[n];
        hadColorProp = new bool[n];
        originalEmission = new Color[n];
        originalEmissionEnabled = new bool[n];

        for (int i = 0; i < n; i++)
        {
            Material m = mats[i];
            if (m == null) continue;

            hadColorProp[i] = m.HasProperty(colorPropertyName);
            if (hadColorProp[i]) originalColors[i] = m.GetColor(colorPropertyName);

            originalEmissionEnabled[i] = m.IsKeywordEnabled("_EMISSION");
            originalEmission[i] = m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.black;
        }

        // TMP: guardar color original.
        if (texts != null)
        {
            originalTextColors = new Color[texts.Length];
            for (int i = 0; i < texts.Length; i++)
                if (texts[i] != null) originalTextColors[i] = texts[i].color;
        }

        captured = true;
    }

    private IEnumerator FlashRoutine(Color feedback)
    {
        // Estado inicial: tinte base de mallas + activar emisión.
        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];
            if (m == null) continue;

            if (hadColorProp[i])
                m.SetColor(colorPropertyName, Color.Lerp(originalColors[i], feedback, colorBlend));

            if (m.HasProperty("_EmissionColor"))
                m.EnableKeyword("_EMISSION");
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            // Pulso 0..1 con seno.
            float k = (Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            // Mallas: pulso de emisión.
            Color emis = feedback * (k * maxEmission);
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m != null && m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", emis);
            }

            // TMP: pulso de brillo del propio color del texto.
            if (texts != null)
            {
                float bright = Mathf.Lerp(textMinBrightness, 1f, k);
                Color textCol = feedback * bright;
                textCol.a = 1f;
                for (int i = 0; i < texts.Length; i++)
                    if (texts[i] != null) texts[i].color = textCol;
            }

            yield return null;
        }

        Restore();
        routine = null;
    }

    /// <summary>Restaura los colores/emisión originales inmediatamente.</summary>
    public void Restore()
    {
        if (mats != null)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m == null) continue;

                if (hadColorProp[i])
                    m.SetColor(colorPropertyName, originalColors[i]);

                if (m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", originalEmission[i]);

                if (originalEmissionEnabled[i])
                    m.EnableKeyword("_EMISSION");
                else
                    m.DisableKeyword("_EMISSION");
            }
        }

        if (texts != null && originalTextColors != null)
        {
            for (int i = 0; i < texts.Length; i++)
                if (texts[i] != null) texts[i].color = originalTextColors[i];
        }
    }
}
