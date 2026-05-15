using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton que lleva el tiempo global de la sesión de juego.
/// Llama StartTimer() al inicio de la experiencia y StopTimer() al final.
/// </summary>
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("¿Empieza automáticamente al arrancar la escena?")]
    public bool autoStart = true;

    [Header("Events")]
    public UnityEvent<float> onTimerStopped;   // devuelve el tiempo final en segundos

    private float elapsed;
    private bool running;

    public float Elapsed => elapsed;
    public bool IsRunning => running;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoStart)
            StartTimer();
    }

    private void Update()
    {
        if (running)
            elapsed += Time.deltaTime;
    }

    public void StartTimer()
    {
        running = true;
        Debug.Log("<color=cyan>[GameTimer] ▶ Timer iniciado.</color>");
    }

    public void StopTimer()
    {
        if (!running) return;
        running = false;
        onTimerStopped?.Invoke(elapsed);
        Debug.Log($"<color=lime>[GameTimer] ⏹ Timer parado. Tiempo final: {FormatTime(elapsed)}</color>");
    }

    public void ResetTimer()
    {
        running = false;
        elapsed = 0f;
        Debug.Log("<color=yellow>[GameTimer] 🔄 Timer reseteado.</color>");
    }

    /// <summary>Devuelve el tiempo en formato MM:SS.</summary>
    public static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
