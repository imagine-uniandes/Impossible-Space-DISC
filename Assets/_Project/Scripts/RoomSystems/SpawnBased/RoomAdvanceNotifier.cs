using UnityEngine;

/// <summary>
/// Ponlo en el mismo GameObject que ObjectToggler (en el prefab de habitación).
/// Cablea: ObjectToggler.onActionComplete → RoomAdvanceNotifier.OnDoorOpened()
/// Busca el RoomSpawnManager en escena y llama AdvanceToNextRoom().
/// Funciona con RoomSpawnManager y TeleportRoomManager (subclase).
/// </summary>
public class RoomAdvanceNotifier : MonoBehaviour
{
    private RoomSpawnManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<RoomSpawnManager>();
    }

    public void OnDoorOpened()
    {
        if (manager == null)
            manager = FindObjectOfType<RoomSpawnManager>();

        if (manager != null)
            manager.AdvanceToNextRoom();
        else
            Debug.LogError("[RoomAdvanceNotifier] No se encontró RoomSpawnManager en la escena.");
    }
}
