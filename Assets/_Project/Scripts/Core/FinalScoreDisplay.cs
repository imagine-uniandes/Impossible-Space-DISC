using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Muestra la puntuación final de la experiencia (basada en el tiempo total).
///
/// Modelo de "usuario único": no hay registro de usuarios; se guarda un único
/// MEJOR TIEMPO GLOBAL en PlayerPrefs. Al terminar se muestra el tiempo de este
/// intento y el mejor tiempo histórico, indicando si se batió el récord.
///
/// Wiring:
///   - Pon este componente en un GameObject con un TextMeshPro 3D (el panel de
///     resultados del final), o asigna 'resultLabel' manualmente.
///   - Llama ShowFinalScore() desde la última puerta / trigger / botón
///     (UnityEvent). Ese método ya se encarga de parar el GameTimer.
/// </summary>
public class FinalScoreDisplay : MonoBehaviour
{
    private const string BestTimeKey = "ImpossibleSpaces_BestTime";

    [Header("Referencias")]
    [Tooltip("Texto donde se muestra el resultado. Si se deja vacío se busca un TextMeshPro en este mismo objeto.")]
    public TextMeshPro resultLabel;

    [Header("Textos")]
    [Tooltip("Encabezado del panel de resultados.")]
    public string header = "¡COMPLETADO!";
    [Tooltip("Etiqueta del tiempo de este intento.")]
    public string yourTimeLabel = "Tu tiempo:";
    [Tooltip("Etiqueta del mejor tiempo histórico.")]
    public string bestTimeLabel = "Mejor tiempo:";
    [Tooltip("Mensaje cuando se bate el récord.")]
    public string newRecordText = "¡NUEVO RÉCORD!";

    [Header("Eventos")]
    [Tooltip("Se dispara cuando se calcula el resultado. (tiempoIntento, mejorTiempo)")]
    public UnityEvent<float, float> onScoreCalculated;
    [Tooltip("Se dispara solo si este intento batió el récord.")]
    public UnityEvent onNewRecord;

    private void Awake()
    {
        if (resultLabel == null)
            resultLabel = GetComponent<TextMeshPro>();
    }

    /// <summary>Mejor tiempo global guardado (segundos). float.MaxValue si aún no hay ninguno.</summary>
    public static float GetBestTime()
    {
        return PlayerPrefs.GetFloat(BestTimeKey, float.MaxValue);
    }

    /// <summary>¿Hay algún mejor tiempo registrado?</summary>
    public static bool HasBestTime()
    {
        return PlayerPrefs.HasKey(BestTimeKey);
    }

    /// <summary>Borra el mejor tiempo global (útil para reset/pruebas).</summary>
    public static void ClearBestTime()
    {
        PlayerPrefs.DeleteKey(BestTimeKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Para el timer (si sigue corriendo), guarda el mejor tiempo y muestra el resultado.
    /// Llama esto desde la última puerta/trigger/botón.
    /// </summary>
    public void ShowFinalScore()
    {
        float attempt;

        if (GameTimer.Instance != null)
        {
            if (GameTimer.Instance.IsRunning)
                GameTimer.Instance.StopTimer();
            attempt = GameTimer.Instance.Elapsed;
        }
        else
        {
            Debug.LogWarning("[FinalScoreDisplay] No hay GameTimer en la escena; usando 0 como tiempo.");
            attempt = 0f;
        }

        float previousBest = GetBestTime();
        bool isRecord = attempt < previousBest;

        if (isRecord)
        {
            PlayerPrefs.SetFloat(BestTimeKey, attempt);
            PlayerPrefs.Save();
        }

        float best = isRecord ? attempt : previousBest;
        Render(attempt, best, isRecord);

        onScoreCalculated?.Invoke(attempt, best);
        if (isRecord)
            onNewRecord?.Invoke();

        Debug.Log($"<color=lime>[FinalScoreDisplay] Intento: {GameTimer.FormatTime(attempt)} | Mejor: {GameTimer.FormatTime(best)} | Récord: {isRecord}</color>");
    }

    private void Render(float attempt, float best, bool isRecord)
    {
        if (resultLabel == null)
        {
            Debug.LogWarning("[FinalScoreDisplay] Sin TextMeshPro asignado; no se puede mostrar el resultado.");
            return;
        }

        string bestStr = best == float.MaxValue ? "--:--" : GameTimer.FormatTime(best);
        string text = $"{header}\n\n{yourTimeLabel} {GameTimer.FormatTime(attempt)}\n{bestTimeLabel} {bestStr}";
        if (isRecord)
            text += $"\n\n{newRecordText}";

        resultLabel.text = text;
    }
}
