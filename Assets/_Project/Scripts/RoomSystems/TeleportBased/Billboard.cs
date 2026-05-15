using UnityEngine;

/// <summary>
/// Hace que el objeto siempre mire hacia la cámara principal (headset en VR).
/// Ponlo en el mismo GameObject que el TextMeshPro del contador.
/// </summary>
public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }
}
