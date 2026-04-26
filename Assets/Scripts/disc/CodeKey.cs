using UnityEngine;

/// <summary>
/// Identifica el color de una llave física en la escena.
/// Poner este componente en cada objeto llave junto al XR Interactable.
/// </summary>
public class CodeKey : MonoBehaviour
{
    [Header("Key Data")]
    [Tooltip("Color de esta llave. Debe coincidir exactamente con uno de los validColors del CodePuzzleManager.")]
    public string keyColor;

    public string KeyColor => keyColor.ToLower().Trim();
}
