using UnityEngine;

/// <summary>
/// Movimiento ligero para palabras flotantes en VR (sin física pesada).
/// Mantiene límite inferior de altura y deriva suave en XZ.
/// </summary>
public class FloatingCodeWordMotion : MonoBehaviour
{
    [Header("Bounds")]
    [Tooltip("Centro de movimiento en mundo")]
    public Vector3 movementCenter;

    [Tooltip("Radio horizontal máximo alrededor del centro")]
    [Min(0.1f)]
    public float horizontalRadius = 2f;

    [Tooltip("Altura mínima permitida")]
    public float minHeight = 1.5f;

    [Tooltip("Altura máxima permitida")]
    public float maxHeight = 3f;

    [Header("Floating")]
    [Tooltip("Velocidad de deriva horizontal")]
    [Min(0f)]
    public float driftSpeed = 0.2f;

    [Tooltip("Amplitud vertical del flotado")]
    [Min(0f)]
    public float bobAmplitude = 0.08f;

    [Tooltip("Frecuencia vertical del flotado")]
    [Min(0f)]
    public float bobFrequency = 1f;

    [Header("Optional Rotation")]
    [Tooltip("Rotación suave alrededor de Y")]
    public bool rotateAroundY = false;

    [Tooltip("Grados por segundo")]
    public float rotateYSpeed = 20f;

    private Vector3 driftDirection;
    private float baseY;
    private float phase;

    public void Setup(
        Vector3 center,
        float radius,
        float minY,
        float maxY,
        float speed,
        float amplitude,
        float frequency,
        bool useRotation,
        float rotationSpeed)
    {
        movementCenter = center;
        horizontalRadius = Mathf.Max(0.1f, radius);
        minHeight = minY;
        maxHeight = Mathf.Max(minY + 0.01f, maxY);
        driftSpeed = Mathf.Max(0f, speed);
        bobAmplitude = Mathf.Max(0f, amplitude);
        bobFrequency = Mathf.Max(0f, frequency);
        rotateAroundY = useRotation;
        rotateYSpeed = rotationSpeed;

        InitializeInternalState();
    }

    private void Awake()
    {
        InitializeInternalState();
    }

    private void InitializeInternalState()
    {
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        if (dir2D.sqrMagnitude < 0.0001f)
        {
            dir2D = Vector2.right;
        }

        driftDirection = new Vector3(dir2D.x, 0f, dir2D.y);
        baseY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        Vector3 position = transform.position;

        Vector3 horizontal = new Vector3(position.x, 0f, position.z);
        Vector3 centerHorizontal = new Vector3(movementCenter.x, 0f, movementCenter.z);

        horizontal += driftDirection * driftSpeed * Time.deltaTime;

        Vector3 fromCenter = horizontal - centerHorizontal;
        float distance = fromCenter.magnitude;
        if (distance > horizontalRadius)
        {
            Vector3 toCenter = (centerHorizontal - horizontal).normalized;
            driftDirection = Vector3.Lerp(driftDirection, toCenter, 0.8f).normalized;
            horizontal = centerHorizontal + fromCenter.normalized * horizontalRadius;
        }

        float y = baseY;
        if (bobAmplitude > 0f && bobFrequency > 0f)
        {
            y += Mathf.Sin((Time.time * bobFrequency) + phase) * bobAmplitude;
        }

        y = Mathf.Clamp(y, minHeight, maxHeight);

        transform.position = new Vector3(horizontal.x, y, horizontal.z);

        if (rotateAroundY)
        {
            transform.Rotate(0f, rotateYSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }
}
