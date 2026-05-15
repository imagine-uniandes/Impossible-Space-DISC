using System.Collections;
using UnityEngine;

/// <summary>
/// Extiende RoomSpawnManager con una fase intermedia (antecámara) entre habitaciones.
///
/// El jugador NO se mueve — solo cambia el entorno a su alrededor.
///
/// Flujo:
///   puerta abre → [delay] → antecámara aparece (habitación actual desaparece)
///   jugador camina al centro → [delay] → siguiente habitación aparece (antecámara desaparece)
///
/// Setup por habitación:
///   • Eliminar SpawnTransitionTrigger del collider de la puerta.
///   • Cablear: ObjectToggler.onActionComplete → RoomAdvanceNotifier.OnDoorOpened()
///
/// Setup por prefab de antecámara:
///   • Añadir SpawnTransitionTrigger en el objeto del centro (collider trigger en el suelo).
///   • autoFindSpawnManager: false (se asigna automáticamente al instanciar).
/// </summary>
public class TeleportRoomManager : RoomSpawnManager
{
    [Header("Antechambers")]
    [Tooltip("Prefabs de antecámara — uno por transición, índice paralelo a roomPrefabs[]")]
    public GameObject[] antechamberPrefabs;

    [Header("Transition")]
    [Tooltip("Segundos de espera antes de swapear el entorno (al entrar a antecámara y al entrar a habitación)")]
    [Range(0f, 10f)]
    public float transitionDelay = 0f;

    [Header("Debug")]
    public bool showTeleportLogs = true;

    private enum TransitionState { InRoom, InAntechamber }
    private TransitionState state = TransitionState.InRoom;
    private GameObject currentAntechamber;

    public override void AdvanceToNextRoom()
    {
        if (state == TransitionState.InRoom)
            StartCoroutine(GoToAntechamber());
        else
            StartCoroutine(GoToNextRoom());
    }

    private IEnumerator GoToAntechamber()
    {
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        int index = CurrentRoomIndex;

        if (antechamberPrefabs == null || index >= antechamberPrefabs.Length || antechamberPrefabs[index] == null)
        {
            if (showTeleportLogs)
                Debug.LogWarning($"[TeleportRoomManager] Sin antecámara en índice {index}. Avanzando directo.");
            base.AdvanceToNextRoom();
            yield break;
        }

        GameObject prefab = antechamberPrefabs[index];
        Vector3 pos = spawnPoint != null ? spawnPoint.position
                      : usePrefabOriginalTransform ? prefab.transform.position
                      : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation
                         : usePrefabOriginalTransform ? prefab.transform.rotation
                         : transform.rotation;

        currentAntechamber = Instantiate(prefab, pos, rot, activeRoomsParent);
        currentAntechamber.name = prefab.name + "_Antechamber";

        AssignManagerToSpawnedTriggers(currentAntechamber);
        ReleaseCurrentRoom();

        state = TransitionState.InAntechamber;

        if (showTeleportLogs)
            Debug.Log($"<color=cyan>[TeleportRoomManager] Antecámara {index} activa.</color>");
    }

    private IEnumerator GoToNextRoom()
    {
        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        base.AdvanceToNextRoom();

        if (currentAntechamber != null)
        {
            Destroy(currentAntechamber);
            currentAntechamber = null;
        }

        state = TransitionState.InRoom;

        if (showTeleportLogs)
            Debug.Log($"<color=lime>[TeleportRoomManager] Habitación {CurrentRoomIndex} activa.</color>");
    }
}
