using UnityEngine;

/// <summary>
/// Controla el flujo de habitaciones por prefabs: instancia la actual,
/// precarga la siguiente y avanza bajo demanda.
/// </summary>
public class RoomSpawnManager : MonoBehaviour
{
    [Header("Room Prefabs")]
    [Tooltip("Orden de habitaciones a recorrer")]
    public GameObject[] roomPrefabs;

    [Tooltip("Índice de la habitación inicial")]
    [Min(0)]
    public int startRoomIndex = 0;

    [Header("Spawn Settings")]
    [Tooltip("Punto de aparición de habitaciones (si null, usa este transform)")]
    public Transform spawnPoint;

    [Tooltip("Si no hay Spawn Point, usar la posición/rotación original guardada en cada prefab")]
    public bool usePrefabOriginalTransform = true;

    [Tooltip("Padre de las habitaciones activas")]
    public Transform activeRoomsParent;

    [Tooltip("Preinstanciar la siguiente habitación en estado inactivo")]
    public bool preloadNextRoom = true;

    [Tooltip("Asignar automáticamente este manager a todos los SpawnTransitionTrigger de la habitación instanciada")]
    public bool autoAssignManagerToSpawnedTriggers = true;

    [Header("Lifetime")]
    [Tooltip("Usar pool para reciclar habitaciones")]
    public bool usePool = true;

    [Tooltip("Referencia al pool (opcional si usePool=false)")]
    public RoomPrefabPool roomPool;

    [Tooltip("Si no usa pool, destruir la habitación anterior al avanzar")]
    public bool destroyPreviousWhenAdvancing = true;

    [Tooltip("Delay para destruir la habitación anterior (si no usa pool)")]
    [Range(0f, 5f)]
    public float destroyDelay = 0f;

    [Header("Debug")]
    public bool showLogs = true;

    private int currentRoomIndex = -1;
    private GameObject currentRoomInstance;
    private GameObject preloadedNextInstance;

    public int CurrentRoomIndex => currentRoomIndex;
    public GameObject CurrentRoomInstance => currentRoomInstance;

    private void Start()
    {
        Initialize();
    }

    [ContextMenu("Initialize")]
    public void Initialize()
    {
        CleanupRuntimeState();

        if (!IsIndexValid(startRoomIndex))
        {
            Debug.LogError("<color=red>[RoomSpawnManager] ❌ startRoomIndex fuera de rango.</color>");
            return;
        }

        currentRoomIndex = startRoomIndex;
        currentRoomInstance = SpawnRoom(currentRoomIndex, true);

        if (preloadNextRoom)
        {
            PreloadNext();
        }

        if (showLogs)
        {
            Debug.Log($"<color=lime>[RoomSpawnManager] ✅ Inicializada habitación {currentRoomIndex}: {roomPrefabs[currentRoomIndex].name}</color>");
        }
    }

    public void AdvanceToNextRoom()
    {
        int nextIndex = currentRoomIndex + 1;

        if (!IsIndexValid(nextIndex))
        {
            if (showLogs)
            {
                Debug.LogWarning("<color=yellow>[RoomSpawnManager] ⚠️ No hay más habitaciones en la lista.</color>");
            }
            return;
        }

        GameObject previousRoom = currentRoomInstance;

        if (preloadedNextInstance != null)
        {
            currentRoomInstance = preloadedNextInstance;
            preloadedNextInstance = null;

            currentRoomInstance.transform.SetParent(activeRoomsParent);
            ApplySpawnPose(currentRoomInstance, roomPrefabs[nextIndex]);
            currentRoomInstance.SetActive(true);
        }
        else
        {
            currentRoomInstance = SpawnRoom(nextIndex, true);
        }

        currentRoomIndex = nextIndex;

        ReleasePrevious(previousRoom);

        if (preloadNextRoom)
        {
            PreloadNext();
        }

        if (showLogs)
        {
            Debug.Log($"<color=cyan>[RoomSpawnManager] 🚪 Avanzó a habitación {currentRoomIndex}: {roomPrefabs[currentRoomIndex].name}</color>");
        }
    }

    [ContextMenu("Advance To Next Room")]
    public void TestAdvance()
    {
        AdvanceToNextRoom();
    }

    private void PreloadNext()
    {
        int nextIndex = currentRoomIndex + 1;
        if (!IsIndexValid(nextIndex))
        {
            preloadedNextInstance = null;
            return;
        }

        preloadedNextInstance = SpawnRoom(nextIndex, false);

        if (showLogs)
        {
            Debug.Log($"<color=yellow>[RoomSpawnManager] 📦 Preload habitación {nextIndex}: {roomPrefabs[nextIndex].name}</color>");
        }
    }

    private GameObject SpawnRoom(int index, bool active)
    {
        GameObject prefab = roomPrefabs[index];
        if (prefab == null)
        {
            Debug.LogError($"<color=red>[RoomSpawnManager] ❌ Prefab null en índice {index}.</color>");
            return null;
        }

        GameObject instance;

        Vector3 spawnPosition = GetSpawnPosition(prefab);
        Quaternion spawnRotation = GetSpawnRotation(prefab);

        if (usePool && roomPool != null)
        {
            instance = roomPool.Get(prefab, spawnPosition, spawnRotation, activeRoomsParent);
        }
        else
        {
            instance = Instantiate(prefab, spawnPosition, spawnRotation, activeRoomsParent);
        }

        if (instance != null)
        {
            if (autoAssignManagerToSpawnedTriggers)
            {
                AssignManagerToSpawnedTriggers(instance);
            }

            instance.SetActive(active);
        }

        return instance;
    }

    private void ApplySpawnPose(GameObject instance, GameObject prefab)
    {
        if (instance == null) return;

        instance.transform.SetPositionAndRotation(GetSpawnPosition(prefab), GetSpawnRotation(prefab));
    }

    private void AssignManagerToSpawnedTriggers(GameObject roomInstance)
    {
        if (roomInstance == null) return;

        SpawnTransitionTrigger[] triggers = roomInstance.GetComponentsInChildren<SpawnTransitionTrigger>(true);
        if (triggers == null || triggers.Length == 0) return;

        for (int i = 0; i < triggers.Length; i++)
        {
            if (triggers[i] == null) continue;
            triggers[i].SetSpawnManager(this);
        }

        if (showLogs)
        {
            Debug.Log($"<color=cyan>[RoomSpawnManager] 🔗 Triggers vinculados en '{roomInstance.name}': {triggers.Length}</color>");
        }
    }

    private void ReleasePrevious(GameObject previousRoom)
    {
        if (previousRoom == null) return;

        if (usePool && roomPool != null)
        {
            roomPool.Release(previousRoom);
            return;
        }

        if (destroyPreviousWhenAdvancing)
        {
            Destroy(previousRoom, destroyDelay);
        }
        else
        {
            previousRoom.SetActive(false);
            previousRoom.transform.SetParent(null);
        }
    }

    private Vector3 GetSpawnPosition(GameObject prefab)
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }

        if (usePrefabOriginalTransform && prefab != null)
        {
            return prefab.transform.position;
        }

        return transform.position;
    }

    private Quaternion GetSpawnRotation(GameObject prefab)
    {
        if (spawnPoint != null)
        {
            return spawnPoint.rotation;
        }

        if (usePrefabOriginalTransform && prefab != null)
        {
            return prefab.transform.rotation;
        }

        return transform.rotation;
    }

    private bool IsIndexValid(int index)
    {
        return roomPrefabs != null && index >= 0 && index < roomPrefabs.Length;
    }

    private void CleanupRuntimeState()
    {
        if (currentRoomInstance != null)
        {
            if (usePool && roomPool != null)
            {
                roomPool.Release(currentRoomInstance);
            }
            else
            {
                Destroy(currentRoomInstance);
            }
        }

        if (preloadedNextInstance != null)
        {
            if (usePool && roomPool != null)
            {
                roomPool.Release(preloadedNextInstance);
            }
            else
            {
                Destroy(preloadedNextInstance);
            }
        }

        currentRoomInstance = null;
        preloadedNextInstance = null;
        currentRoomIndex = -1;
    }
}
