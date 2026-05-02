using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner de palabras de código flotantes desde un pool de prefabs.
/// Diseñado para VR: sin físicas dinámicas, con movimiento por script.
/// </summary>
public class FloatingCodeWordSpawner : MonoBehaviour
{
    [Header("Pool")]
    [Tooltip("Prefabs de palabras 3D (TMP 3D o mallas)")]
    public GameObject[] wordPrefabs;

    [Tooltip("Cantidad total a instanciar")]
    [Min(1)]
    public int totalWords = 12;

    [Header("Spawn Area")]
    [Tooltip("Centro de spawn (si está vacío usa este transform)")]
    public Transform spawnCenter;

    [Tooltip("Tamaño de la caja de spawn")]
    public Vector3 spawnAreaSize = new Vector3(4f, 2f, 4f);

    [Tooltip("Altura mínima global (no bajar de aquí)")]
    public float minHeight = 1.5f;

    [Tooltip("Altura máxima global")]
    public float maxHeight = 3f;

    [Tooltip("Padre opcional para mantener ordenada la jerarquía")]
    public Transform spawnedParent;

    [Header("Motion Settings")]
    [Tooltip("Radio horizontal de movimiento alrededor del centro")]
    [Min(0.1f)]
    public float movementRadius = 2.5f;

    [Tooltip("Rango de velocidad horizontal")]
    public Vector2 driftSpeedRange = new Vector2(0.08f, 0.22f);

    [Tooltip("Rango de amplitud vertical")]
    public Vector2 bobAmplitudeRange = new Vector2(0.03f, 0.12f);

    [Tooltip("Rango de frecuencia vertical")]
    public Vector2 bobFrequencyRange = new Vector2(0.6f, 1.4f);

    [Header("Optional Rotation")]
    public bool enableRotation = false;
    public Vector2 rotateYSpeedRange = new Vector2(-25f, 25f);

    [Header("Behavior")]
    [Tooltip("Spawnear automáticamente en Start")]
    public bool spawnOnStart = true;

    [Tooltip("Destruir anteriores antes de volver a spawnear")]
    public bool clearBeforeSpawn = true;

    [Header("Debug")]
    public bool showLogs = true;
    public bool drawSpawnGizmo = true;

    private readonly List<GameObject> spawnedWords = new List<GameObject>();

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnWords();
        }
    }

    [ContextMenu("Spawn Words")]
    public void SpawnWords()
    {
        if (wordPrefabs == null || wordPrefabs.Length == 0)
        {
            if (showLogs)
            {
                Debug.LogWarning("[FloatingCodeWordSpawner] No hay prefabs asignados en wordPrefabs.");
            }
            return;
        }

        if (clearBeforeSpawn)
        {
            ClearSpawned();
        }

        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;

        int spawnedCount = 0;
        for (int i = 0; i < totalWords; i++)
        {
            GameObject prefab = GetRandomPrefab();
            if (prefab == null)
            {
                continue;
            }

            Vector3 position = GetRandomSpawnPosition(center);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Transform parent = spawnedParent != null ? spawnedParent : transform;

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            spawnedWords.Add(instance);

            FloatingCodeWordMotion motion = instance.GetComponent<FloatingCodeWordMotion>();
            if (motion == null)
            {
                motion = instance.AddComponent<FloatingCodeWordMotion>();
            }

            float speed = RandomRangeSafe(driftSpeedRange, 0f);
            float amplitude = RandomRangeSafe(bobAmplitudeRange, 0f);
            float frequency = RandomRangeSafe(bobFrequencyRange, 0.5f);
            float rotateSpeed = RandomRangeSafe(rotateYSpeedRange, 0f);

            motion.Setup(
                center,
                movementRadius,
                minHeight,
                maxHeight,
                speed,
                amplitude,
                frequency,
                enableRotation,
                rotateSpeed);

            spawnedCount++;
        }

        if (showLogs)
        {
            Debug.Log($"<color=cyan>[FloatingCodeWordSpawner] Spawn completado: {spawnedCount} palabras.</color>");
        }
    }

    [ContextMenu("Clear Spawned")]
    public void ClearSpawned()
    {
        for (int i = spawnedWords.Count - 1; i >= 0; i--)
        {
            if (spawnedWords[i] != null)
            {
                Destroy(spawnedWords[i]);
            }
        }

        spawnedWords.Clear();
    }

    private GameObject GetRandomPrefab()
    {
        if (wordPrefabs == null || wordPrefabs.Length == 0)
        {
            return null;
        }

        int validCount = 0;
        for (int i = 0; i < wordPrefabs.Length; i++)
        {
            if (wordPrefabs[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        int selectedIndex = Random.Range(0, validCount);
        int currentValid = 0;

        for (int i = 0; i < wordPrefabs.Length; i++)
        {
            if (wordPrefabs[i] == null)
            {
                continue;
            }

            if (currentValid == selectedIndex)
            {
                return wordPrefabs[i];
            }

            currentValid++;
        }

        return null;
    }

    private Vector3 GetRandomSpawnPosition(Vector3 center)
    {
        float halfX = Mathf.Abs(spawnAreaSize.x) * 0.5f;
        float halfY = Mathf.Abs(spawnAreaSize.y) * 0.5f;
        float halfZ = Mathf.Abs(spawnAreaSize.z) * 0.5f;

        float x = center.x + Random.Range(-halfX, halfX);
        float z = center.z + Random.Range(-halfZ, halfZ);

        float minYFromArea = center.y - halfY;
        float maxYFromArea = center.y + halfY;

        float finalMinY = Mathf.Max(minHeight, minYFromArea);
        float finalMaxY = Mathf.Max(finalMinY + 0.01f, Mathf.Min(maxHeight, maxYFromArea));

        float y = Random.Range(finalMinY, finalMaxY);

        return new Vector3(x, y, z);
    }

    private float RandomRangeSafe(Vector2 range, float fallback)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);

        if (Mathf.Approximately(min, max))
        {
            return min;
        }

        if (max < min)
        {
            return fallback;
        }

        return Random.Range(min, max);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawSpawnGizmo)
        {
            return;
        }

        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(center, spawnAreaSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, movementRadius);

        Gizmos.color = Color.green;
        Vector3 p1 = new Vector3(center.x - spawnAreaSize.x * 0.5f, minHeight, center.z - spawnAreaSize.z * 0.5f);
        Vector3 p2 = new Vector3(center.x + spawnAreaSize.x * 0.5f, minHeight, center.z - spawnAreaSize.z * 0.5f);
        Vector3 p3 = new Vector3(center.x + spawnAreaSize.x * 0.5f, minHeight, center.z + spawnAreaSize.z * 0.5f);
        Vector3 p4 = new Vector3(center.x - spawnAreaSize.x * 0.5f, minHeight, center.z + spawnAreaSize.z * 0.5f);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}
