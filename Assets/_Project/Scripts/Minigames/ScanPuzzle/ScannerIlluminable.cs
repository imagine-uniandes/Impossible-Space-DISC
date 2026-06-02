using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hace que un objeto se "encienda" (emisión) mientras el HandheldScanner lo
/// apunta, como si la linterna lo iluminara, y se apague suavemente al dejar de
/// apuntarlo. Pensado para el modelo de Séneca o cualquier prop iluminable.
///
/// Independiente del puzzle: NO cuenta como objetivo (eso lo hace ScanTarget).
/// Requiere un Collider en el objeto (o en un hijo) dentro de una capa incluida
/// en el LayerMask del escáner para que el haz lo detecte.
///
/// El HandheldScanner llama SetIlluminated(true/false) automáticamente al
/// entrar/salir el haz (illuminateOnAim).
/// </summary>
[DisallowMultipleComponent]
public class ScannerIlluminable : MonoBehaviour
{
    [Header("Renderers a iluminar")]
    [Tooltip("Si está vacío y autoFind está activo, busca renderers en hijos (sirve para modelos con muchas mallas).")]
    public Renderer[] renderers;
    public bool autoFindRenderersIfEmpty = true;

    [Header("Emisión")]
    [Tooltip("Color de la luz emitida al ser apuntado (un blanco cálido funciona como linterna).")]
    [ColorUsage(false, true)] public Color emissionColor = Color.white;

    [Tooltip("Intensidad máxima de la emisión cuando está totalmente iluminado.")]
    [Range(0f, 10f)] public float maxIntensity = 2f;

    [Tooltip("Velocidad de encendido/apagado del brillo (mayor = más rápido).")]
    [Min(0.1f)] public float fadeSpeed = 6f;

    private Material[] mats;
    private Color[] originalEmission;
    private bool[] originalEmissionEnabled;
    private bool captured;

    private float current;   // 0..1 actual
    private float target;    // 0..1 objetivo
    private float lastApplied = -1f;

    private void Awake()
    {
        Capture();
    }

    private void Capture()
    {
        if (captured) return;

        if ((renderers == null || renderers.Length == 0) && autoFindRenderersIfEmpty)
            renderers = GetComponentsInChildren<Renderer>(true);

        var matList = new List<Material>();
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                matList.AddRange(r.materials); // instancias por renderer
            }
        }

        mats = matList.ToArray();
        originalEmission = new Color[mats.Length];
        originalEmissionEnabled = new bool[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];
            if (m == null) continue;
            originalEmissionEnabled[i] = m.IsKeywordEnabled("_EMISSION");
            originalEmission[i] = m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.black;
        }

        captured = true;
    }

    /// <summary>Lo llama el HandheldScanner al entrar/salir el haz.</summary>
    public void SetIlluminated(bool on)
    {
        target = on ? 1f : 0f;
        if (on) // asegura que el keyword esté activo para ver el cambio de inmediato
        {
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null && mats[i].HasProperty("_EmissionColor"))
                    mats[i].EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (!captured) return;

        current = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
        if (Mathf.Approximately(current, lastApplied)) return;

        Apply();
        lastApplied = current;
    }

    private void Apply()
    {
        for (int i = 0; i < mats.Length; i++)
        {
            Material m = mats[i];
            if (m == null) continue;

            if (m.HasProperty("_EmissionColor"))
            {
                Color glow = emissionColor * (maxIntensity * current);
                m.SetColor("_EmissionColor", originalEmission[i] + glow);
            }

            // Al apagarse del todo, restaura el estado original del keyword.
            if (current <= 0f && !originalEmissionEnabled[i])
                m.DisableKeyword("_EMISSION");
        }
    }
}
