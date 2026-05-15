using UnityEngine;

/// <summary>
/// Aplica el material de estrellas a los renderers del techo al iniciar.
/// Ponlo en el root del prefab de habitación o en cualquier GameObject padre de los techos.
/// </summary>
public class CeilingSkybox : MonoBehaviour
{
    [Header("Material")]
    [Tooltip("Material de estrellas (StarCeiling.mat)")]
    public Material starMaterial;

    [Header("Targets")]
    [Tooltip("Renderers del techo a los que aplicar el material. Si está vacío, se buscan automáticamente por nombre.")]
    public Renderer[] ceilingRenderers;

    [Tooltip("Texto que debe contener el nombre del GameObject para considerarse techo (se usa si Ceiling Renderers está vacío).")]
    public string ceilingNameFilter = "ceiling";

    private void Awake()
    {
        if (starMaterial == null)
        {
            Debug.LogWarning("[CeilingSkybox] No hay material asignado.", this);
            return;
        }

        Renderer[] targets = ceilingRenderers != null && ceilingRenderers.Length > 0
            ? ceilingRenderers
            : FindCeilingRenderers();

        foreach (Renderer r in targets)
        {
            if (r == null) continue;
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = starMaterial;
            r.sharedMaterials = mats;
        }

        Debug.Log($"<color=cyan>[CeilingSkybox] ✨ Material de estrellas aplicado a {targets.Length} renderer(s).</color>");
    }

    private Renderer[] FindCeilingRenderers()
    {
        Renderer[] all = GetComponentsInChildren<Renderer>(true);
        var result = new System.Collections.Generic.List<Renderer>();
        foreach (Renderer r in all)
        {
            if (r.gameObject.name.ToLower().Contains(ceilingNameFilter.ToLower()))
                result.Add(r);
        }
        return result.ToArray();
    }
}
