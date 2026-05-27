using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Apaga/enciende la "luz" de una habitación manipulando la iluminación ambiente
/// (y opcionalmente el skybox y la niebla) de RenderSettings.
///
/// Como en este proyecto solo hay una habitación activa a la vez (las demás se
/// despawnean), el efecto queda acotado a esta sala: al apagar se guardan los
/// valores actuales y al encender (o al desactivarse el objeto) se restauran,
/// de modo que ninguna otra habitación hereda la oscuridad.
///
/// Métodos pensados para llamarse desde UnityEvents:
///   • TurnOff()  → desde LabelMatchPuzzleManager.onPuzzleSolved
///   • TurnOn()   → desde ScanPuzzleManager.onAllTargetsFound
/// </summary>
public class RoomLightingController : MonoBehaviour
{
    [Header("Ambiente oscuro")]
    [Tooltip("Color de luz ambiente al apagar (negro = oscuridad total para materiales Lit)")]
    public Color darkAmbientColor = Color.black;

    [Tooltip("Intensidad de ambiente al apagar (modo Trilight/Flat)")]
    [Range(0f, 1f)]
    public float darkAmbientIntensity = 0f;

    [Tooltip("Intensidad de los reflejos de entorno al apagar (0 = sin reflejos del skybox; clave para que de verdad se vea oscuro)")]
    [Range(0f, 1f)]
    public float darkReflectionIntensity = 0f;

    [Header("Skybox (opcional)")]
    [Tooltip("Si está activo, reemplaza el skybox al apagar y lo restaura al encender")]
    public bool controlSkybox = false;

    [Tooltip("Skybox a usar mientras está oscuro (puede ser null para 'sin skybox')")]
    public Material darkSkybox;

    [Header("Niebla (opcional)")]
    [Tooltip("Si está activo, habilita niebla oscura al apagar para reforzar la sensación de oscuridad")]
    public bool controlFog = false;

    [Tooltip("Color de la niebla mientras está oscuro")]
    public Color darkFogColor = Color.black;

    [Tooltip("Densidad de la niebla exponencial mientras está oscuro")]
    [Range(0f, 1f)]
    public float darkFogDensity = 0.15f;

    [Header("Eventos")]
    public UnityEvent onLightsOff;
    public UnityEvent onLightsOn;

    [Header("Debug")]
    public bool showLogs = true;

    // Estado guardado
    private bool isOff;
    private bool hasSavedState;

    private UnityEngine.Rendering.AmbientMode savedAmbientMode;
    private Color savedAmbientLight;
    private Color savedAmbientSky;
    private Color savedAmbientEquator;
    private Color savedAmbientGround;
    private float savedAmbientIntensity;
    private float savedReflectionIntensity;

    private Material savedSkybox;

    private bool savedFogEnabled;
    private Color savedFogColor;
    private FogMode savedFogMode;
    private float savedFogDensity;

    /// <summary>
    /// Apaga las luces: guarda el estado actual de iluminación y aplica el oscuro.
    /// </summary>
    [ContextMenu("Turn Off")]
    public void TurnOff()
    {
        if (isOff) return;

        SaveState();

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = darkAmbientColor;
        RenderSettings.ambientIntensity = darkAmbientIntensity;
        RenderSettings.reflectionIntensity = darkReflectionIntensity;

        if (controlSkybox)
        {
            RenderSettings.skybox = darkSkybox;
        }

        if (controlFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = darkFogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = darkFogDensity;
        }

        DynamicGI.UpdateEnvironment();

        isOff = true;

        if (showLogs)
            Debug.Log("<color=cyan>[RoomLightingController] 🌑 Luces apagadas (ambiente oscuro).</color>");

        onLightsOff?.Invoke();
    }

    /// <summary>
    /// Enciende las luces: restaura el estado de iluminación guardado.
    /// </summary>
    [ContextMenu("Turn On")]
    public void TurnOn()
    {
        if (!isOff)
        {
            // Aunque no estuviera "apagado", restaura si hay estado guardado.
            if (!hasSavedState) return;
        }

        RestoreState();
        isOff = false;

        if (showLogs)
            Debug.Log("<color=lime>[RoomLightingController] 💡 Luces encendidas (ambiente restaurado).</color>");

        onLightsOn?.Invoke();
    }

    private void SaveState()
    {
        savedAmbientMode = RenderSettings.ambientMode;
        savedAmbientLight = RenderSettings.ambientLight;
        savedAmbientSky = RenderSettings.ambientSkyColor;
        savedAmbientEquator = RenderSettings.ambientEquatorColor;
        savedAmbientGround = RenderSettings.ambientGroundColor;
        savedAmbientIntensity = RenderSettings.ambientIntensity;
        savedReflectionIntensity = RenderSettings.reflectionIntensity;

        savedSkybox = RenderSettings.skybox;

        savedFogEnabled = RenderSettings.fog;
        savedFogColor = RenderSettings.fogColor;
        savedFogMode = RenderSettings.fogMode;
        savedFogDensity = RenderSettings.fogDensity;

        hasSavedState = true;
    }

    private void RestoreState()
    {
        if (!hasSavedState) return;

        RenderSettings.ambientMode = savedAmbientMode;
        RenderSettings.ambientLight = savedAmbientLight;
        RenderSettings.ambientSkyColor = savedAmbientSky;
        RenderSettings.ambientEquatorColor = savedAmbientEquator;
        RenderSettings.ambientGroundColor = savedAmbientGround;
        RenderSettings.ambientIntensity = savedAmbientIntensity;
        RenderSettings.reflectionIntensity = savedReflectionIntensity;

        if (controlSkybox)
            RenderSettings.skybox = savedSkybox;

        if (controlFog)
        {
            RenderSettings.fog = savedFogEnabled;
            RenderSettings.fogColor = savedFogColor;
            RenderSettings.fogMode = savedFogMode;
            RenderSettings.fogDensity = savedFogDensity;
        }

        DynamicGI.UpdateEnvironment();
    }

    // Seguridad: si la sala se desactiva/despawnea estando a oscuras, restaura
    // para que ninguna otra habitación quede oscura.
    private void OnDisable()
    {
        if (isOff)
        {
            RestoreState();
            isOff = false;

            if (showLogs)
                Debug.Log("<color=yellow>[RoomLightingController] ⚠️ Restaurado por OnDisable (sala desactivada estando oscura).</color>");
        }
    }
}
