using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool simple para reutilizar instancias de habitaciones por prefab.
/// Evita picos de Instantiate/Destroy en runtime.
/// </summary>
public class RoomPrefabPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Prefabs permitidos para el pool (opcional, pero recomendado)")]
    public GameObject[] registeredPrefabs;

    [Tooltip("Precalentar el pool al iniciar")]
    public bool prewarmOnStart = false;

    [Tooltip("Cantidad de instancias por prefab al precalentar")]
    [Min(0)]
    public int prewarmCountPerPrefab = 1;

    [Tooltip("Padre donde se guardan las instancias inactivas")]
    public Transform pooledParent;

    [Header("Debug")]
    public bool showLogs = true;

    private readonly Dictionary<GameObject, Queue<GameObject>> poolByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (pooledParent == null)
        {
            pooledParent = transform;
        }

        if (registeredPrefabs != null)
        {
            foreach (GameObject prefab in registeredPrefabs)
            {
                RegisterPrefab(prefab);
            }
        }
    }

    private void Start()
    {
        if (prewarmOnStart)
        {
            Prewarm();
        }
    }

    public void RegisterPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        if (!poolByPrefab.ContainsKey(prefab))
        {
            poolByPrefab[prefab] = new Queue<GameObject>();
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parentOverride = null)
    {
        if (prefab == null) return null;

        RegisterPrefab(prefab);

        Queue<GameObject> queue = poolByPrefab[prefab];
        GameObject instance = null;

        while (queue.Count > 0 && instance == null)
        {
            instance = queue.Dequeue();
        }

        if (instance == null)
        {
            instance = Instantiate(prefab);
            instanceToPrefab[instance] = prefab;

            if (showLogs)
            {
                Debug.Log($"<color=cyan>[RoomPrefabPool] 🆕 Instanciado: {prefab.name}</color>");
            }
        }

        Transform targetParent = parentOverride != null ? parentOverride : null;
        instance.transform.SetParent(targetParent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;

        if (!instanceToPrefab.TryGetValue(instance, out GameObject prefab) || prefab == null)
        {
            if (showLogs)
            {
                Debug.LogWarning("<color=orange>[RoomPrefabPool] ⚠️ Instancia no registrada en pool. Se desactiva sin encolar.</color>");
            }

            instance.SetActive(false);
            instance.transform.SetParent(pooledParent);
            return;
        }

        RegisterPrefab(prefab);

        instance.SetActive(false);
        instance.transform.SetParent(pooledParent);
        poolByPrefab[prefab].Enqueue(instance);

        if (showLogs)
        {
            Debug.Log($"<color=lime>[RoomPrefabPool] ♻️ Devuelto al pool: {instance.name}</color>");
        }
    }

    [ContextMenu("Prewarm")]
    public void Prewarm()
    {
        if (registeredPrefabs == null || registeredPrefabs.Length == 0 || prewarmCountPerPrefab <= 0)
        {
            return;
        }

        foreach (GameObject prefab in registeredPrefabs)
        {
            if (prefab == null) continue;

            RegisterPrefab(prefab);

            for (int i = 0; i < prewarmCountPerPrefab; i++)
            {
                GameObject instance = Instantiate(prefab, pooledParent);
                instance.name = prefab.name + "_Pooled";
                instance.SetActive(false);
                instanceToPrefab[instance] = prefab;
                poolByPrefab[prefab].Enqueue(instance);
            }

            if (showLogs)
            {
                Debug.Log($"<color=cyan>[RoomPrefabPool] 🔥 Prewarm {prefab.name}: {prewarmCountPerPrefab}</color>");
            }
        }
    }
}
