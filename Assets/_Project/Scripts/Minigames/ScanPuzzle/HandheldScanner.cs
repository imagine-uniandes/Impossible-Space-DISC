using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Linterna/escáner agarrable. Mientras el jugador la sostiene, lanza un raycast
/// hacia adelante, dibuja un haz (LineRenderer) y enciende un spotlight.
/// Al apuntar a un ScanTarget correcto suena un sonido de acierto y el objeto
/// se ilumina en verde; los distractores reproducen (opcionalmente) un error.
///
/// Funciona con cualquier sistema de grab (XR o custom) cableando:
///   • Select Entered → OnGrabbed()
///   • Select Exited  → OnReleasedTryPlace()
/// (mismos nombres que LabelTagItem para mantener el convenio del proyecto).
///
/// Si scanOnlyWhileHeld = false, escanea siempre (útil si la linterna queda
/// fija en el pilar en vez de agarrarse).
/// </summary>
public class HandheldScanner : MonoBehaviour
{
    [Header("Origen del haz")]
    [Tooltip("Punta de la linterna desde donde sale el raycast (usa forward como dirección). Si null, usa este transform.")]
    public Transform beamOrigin;

    [Tooltip("Distancia máxima del escaneo")]
    [Min(0.1f)]
    public float maxDistance = 10f;

    [Tooltip("Capas escaneables (donde están los ScanTarget)")]
    public LayerMask scannableLayers = ~0;

    [Tooltip("Segundos que hay que mantener el haz sobre un objeto para confirmarlo (0 = inmediato)")]
    [Range(0f, 3f)]
    public float requiredDwell = 1f;

    [Header("Tolerancia de apuntado")]
    [Tooltip("Usa un SphereCast (haz con grosor) en vez de un rayo fino. Mucho más fácil acertar a objetos como Séneca sin tener que agrandar tanto el collider.")]
    public bool useSphereCast = true;

    [Tooltip("Radio del haz al usar SphereCast (metros). Súbelo si cuesta apuntar; bájalo si engancha objetos sin querer.")]
    [Range(0.01f, 1f)]
    public float sphereRadius = 0.15f;

    [Header("Iluminación del objetivo")]
    [Tooltip("Mientras el haz apunta a un objeto con ScannerIlluminable, se enciende su emisión (la linterna 'alumbra' el modelo).")]
    public bool illuminateOnAim = true;

    [Header("Visual del haz")]
    [Tooltip("LineRenderer que dibuja el haz (opcional)")]
    public LineRenderer beam;

    [Tooltip("Spotlight real de la linterna (opcional). Se enciende mientras escanea.")]
    public Light spotlight;

    [Header("Audio")]
    [Tooltip("AudioSource para los sonidos. Si null, se crea uno al iniciar.")]
    public AudioSource audioSource;

    [Tooltip("Sonido al encontrar un objetivo correcto")]
    public AudioClip correctSound;

    [Tooltip("Sonido al escanear un distractor (opcional)")]
    public AudioClip incorrectSound;

    [Header("Comportamiento")]
    [Tooltip("Solo escanear mientras está agarrada. Si false, escanea siempre (linterna fija o para PROBAR sin VR).")]
    public bool scanOnlyWhileHeld = true;

    [Header("Eventos")]
    public UnityEvent onGrabbed;
    public UnityEvent onReleased;

    [Header("Debug")]
    public bool showLogs = false;

    private bool isHeld;
    private ScanTarget currentDwellTarget;
    private float dwellTimer;
    private ScannerIlluminable currentIlluminated;

    private void Awake()
    {
        if (beamOrigin == null)
            beamOrigin = transform;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // El haz se dibuja en coordenadas de mundo; sin esto la línea no sigue a la linterna.
        if (beam != null)
        {
            beam.useWorldSpace = true;
            if (beam.positionCount < 2) beam.positionCount = 2;
        }

        SetScanActive(!scanOnlyWhileHeld);
    }

    /// <summary>Llamar cuando el jugador toma la linterna (XR Select Entered).</summary>
    public void OnGrabbed()
    {
        isHeld = true;
        SetScanActive(true);
        onGrabbed?.Invoke();

        if (showLogs)
            Debug.Log("<color=cyan>[HandheldScanner] Agarrada.</color>");
    }

    /// <summary>Llamar cuando el jugador suelta la linterna (XR Select Exited).</summary>
    public void OnReleasedTryPlace()
    {
        isHeld = false;
        if (scanOnlyWhileHeld)
            SetScanActive(false);
        onReleased?.Invoke();

        if (showLogs)
            Debug.Log("<color=cyan>[HandheldScanner] Soltada.</color>");
    }

    private void SetScanActive(bool active)
    {
        if (beam != null) beam.enabled = active;
        if (spotlight != null) spotlight.enabled = active;
        if (!active)
        {
            currentDwellTarget = null;
            dwellTimer = 0f;
        }
    }

    private void Update()
    {
        bool scanning = scanOnlyWhileHeld ? isHeld : true;
        if (!scanning)
        {
            UpdateIllumination(null);
            return;
        }

        Vector3 origin = beamOrigin.position;
        Vector3 dir = beamOrigin.forward;
        Vector3 endPoint = origin + dir * maxDistance;

        ScanTarget hitTarget = null;
        ScannerIlluminable hitIlluminable = null;

        RaycastHit hit;
        bool didHit = useSphereCast
            ? Physics.SphereCast(origin, sphereRadius, dir, out hit, maxDistance, scannableLayers, QueryTriggerInteraction.Collide)
            : Physics.Raycast(origin, dir, out hit, maxDistance, scannableLayers, QueryTriggerInteraction.Collide);

        if (didHit)
        {
            // SphereCast devuelve point = Vector3.zero si el origen ya solapa el collider; en ese caso dejamos el haz a tope.
            endPoint = hit.point != Vector3.zero ? hit.point : endPoint;
            hitTarget = hit.collider.GetComponentInParent<ScanTarget>();
            hitIlluminable = hit.collider.GetComponentInParent<ScannerIlluminable>();

            if (showLogs)
                Debug.Log($"[HandheldScanner] Haz golpeó '{hit.collider.name}' | ScanTarget: {(hitTarget != null ? hitTarget.name : "ninguno")}");
        }

        UpdateBeam(origin, endPoint);
        ProcessDwell(hitTarget);
        UpdateIllumination(illuminateOnAim ? hitIlluminable : null);
    }

    /// <summary>Enciende la emisión del objeto apuntado y apaga el anterior cuando cambia.</summary>
    private void UpdateIllumination(ScannerIlluminable target)
    {
        if (target == currentIlluminated) return;

        if (currentIlluminated != null)
            currentIlluminated.SetIlluminated(false);

        currentIlluminated = target;

        if (currentIlluminated != null)
            currentIlluminated.SetIlluminated(true);
    }

    private void ProcessDwell(ScanTarget target)
    {
        // Objetivos ya encontrados o distractores no requieren dwell repetido.
        if (target == null || target.IsFound)
        {
            currentDwellTarget = null;
            dwellTimer = 0f;
            return;
        }

        if (target != currentDwellTarget)
        {
            currentDwellTarget = target;
            dwellTimer = 0f;
        }

        dwellTimer += Time.deltaTime;

        if (dwellTimer >= requiredDwell)
        {
            bool wasCorrect = target.OnScanned(this);
            PlayFeedback(wasCorrect, target.IsCorrectTarget);

            currentDwellTarget = null;
            dwellTimer = 0f;
        }
    }

    private void PlayFeedback(bool wasNewCorrect, bool isCorrectTarget)
    {
        if (wasNewCorrect)
        {
            if (correctSound != null)
                audioSource.PlayOneShot(correctSound);
        }
        else if (!isCorrectTarget)
        {
            if (incorrectSound != null)
                audioSource.PlayOneShot(incorrectSound);
        }
    }

    private void UpdateBeam(Vector3 start, Vector3 end)
    {
        if (beam == null || !beam.enabled) return;
        beam.positionCount = 2;
        beam.SetPosition(0, start);
        beam.SetPosition(1, end);
    }

    private void OnDrawGizmosSelected()
    {
        Transform o = beamOrigin != null ? beamOrigin : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(o.position, o.position + o.forward * maxDistance);
    }
}
