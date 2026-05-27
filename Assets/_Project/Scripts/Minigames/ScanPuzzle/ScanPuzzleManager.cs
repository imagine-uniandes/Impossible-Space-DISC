using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manager de la fase de escaneo en oscuridad.
///
/// Lleva la cuenta de los objetivos correctos (ScanTarget con isCorrectTarget=true).
/// Cuando todos han sido encontrados con el HandheldScanner, dispara
/// onAllTargetsFound — pensado para cablear a:
///   • RoomLightingController.TurnOn()  (encender luces)
///   • RoomAdvanceNotifier.OnDoorOpened()  (protocolo de teletransporte)
/// </summary>
public class ScanPuzzleManager : MonoBehaviour
{
    [Header("Objetivos")]
    [Tooltip("ScanTargets de la sala. Si está vacío y autoFind está activo, los busca en hijos al iniciar.")]
    public ScanTarget[] targets;

    [Tooltip("Si está vacío, buscar todos los ScanTarget en hijos al iniciar")]
    public bool autoFindTargetsIfEmpty = true;

    [Header("Eventos")]
    [Tooltip("Se dispara cada vez que se encuentra un objetivo correcto (encontrados, total)")]
    public ProgressEvent onProgress;

    [Tooltip("Se dispara cuando se encuentran TODOS los objetivos correctos")]
    public UnityEvent onAllTargetsFound;

    [Header("Debug")]
    public bool showLogs = true;

    private int totalCorrect;
    private int foundCorrect;
    private bool completed;

    [System.Serializable]
    public class ProgressEvent : UnityEvent<int, int> { }

    public int TotalCorrect => totalCorrect;
    public int FoundCorrect => foundCorrect;
    public bool IsCompleted => completed;

    private void Start()
    {
        if ((targets == null || targets.Length == 0) && autoFindTargetsIfEmpty)
        {
            targets = GetComponentsInChildren<ScanTarget>(true);
        }

        totalCorrect = 0;
        foundCorrect = 0;
        completed = false;

        if (targets != null)
        {
            foreach (ScanTarget t in targets)
            {
                if (t == null) continue;
                t.manager = this;
                if (t.IsCorrectTarget)
                    totalCorrect++;
            }
        }

        if (showLogs)
            Debug.Log($"<color=cyan>[ScanPuzzleManager] Inicializado. Objetivos correctos: {totalCorrect}</color>");

        if (totalCorrect == 0)
            Debug.LogWarning("<color=yellow>[ScanPuzzleManager] ⚠️ No hay objetivos correctos. ¿Olvidaste marcar isCorrectTarget?</color>");
    }

    /// <summary>
    /// Llamado por cada ScanTarget cuando es encontrado por primera vez.
    /// </summary>
    public void NotifyTargetFound(ScanTarget target)
    {
        if (completed) return;

        foundCorrect++;
        onProgress?.Invoke(foundCorrect, totalCorrect);

        if (showLogs)
            Debug.Log($"<color=lime>[ScanPuzzleManager] Progreso: {foundCorrect}/{totalCorrect}</color>");

        if (foundCorrect >= totalCorrect && totalCorrect > 0)
        {
            completed = true;

            if (showLogs)
                Debug.Log("<color=lime>[ScanPuzzleManager] 🎉 Todos los objetivos encontrados.</color>");

            onAllTargetsFound?.Invoke();
        }
    }

    [ContextMenu("Force Complete (Test)")]
    private void ForceComplete()
    {
        if (completed) return;
        completed = true;
        onAllTargetsFound?.Invoke();
    }
}
