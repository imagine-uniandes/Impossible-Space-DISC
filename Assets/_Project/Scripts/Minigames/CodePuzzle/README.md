# Minigames / CodePuzzle

Puzzle de completar una línea de código: el jugador agarra palabras flotantes en VR y las coloca en huecos. Los slots fijos tienen respuesta única; el slot de color es libre y determina qué llave física abre la puerta.

## Scripts

### CodePuzzleManager
Evalúa los 5 slots (objeto, operador, color, función, booleano) cuando todos están llenos. Los slots fijos deben tener la palabra exacta; el slot de color acepta cualquier valor del array `validColors`. Al resolverse guarda el color elegido para que `CodeKeyDoorZone` lo use.

### CodePuzzleWord
Palabra 3D agarrable (XR Interactable). Al soltarla busca el `CodeWordSlot` más cercano dentro de su radio y se snappea. Llama `OnGrabbed()` desde Select Entered y `OnReleasedTryPlace()` desde Select Exited.

### CodeWordSlot
Hueco en el código. Si `expectedValue` tiene texto, solo acepta esa palabra (slot fijo). Si está vacío, acepta cualquier valor (slot dinámico). Se auto-asigna al `CodePuzzleManager` más cercano en la jerarquía.

### CodeKey
Componente identificador de una llave física en escena. Solo guarda el string `keyColor`. Se coloca junto al XR Interactable de cada objeto llave.

### CodeKeyDoorZone
Trigger en la zona de la puerta. Verifica que el código esté resuelto y que el color de la `CodeKey` que entra coincida con el elegido en el puzzle. Si coincide, llama a `ObjectToggler.Disable()` para abrir la puerta.

### FloatingCodeWordMotion
Mueve suavemente una palabra flotante en VR: deriva aleatoria en XZ dentro de un radio horizontal y oscilación sinusoidal en Y. Sin física, solo transform. Configurado por el spawner mediante `Setup()`.

### FloatingCodeWordSpawner
Instancia N palabras desde un pool de prefabs distribuidas en un área de spawn. Asigna a cada una un `FloatingCodeWordMotion` con parámetros aleatorios dentro de rangos configurables.

### CodePuzzleAudioManager
Maneja todos los sonidos del puzzle. Se auto-suscribe en `Start()` a los eventos de `CodePuzzleManager`, todos los `CodeWordSlot` y todas las `CodePuzzleWord` de la escena. Solo requiere asignar los AudioClips en el Inspector; los sonidos faltantes se omiten sin error. Gestiona música de fondo en loop y efectos de interacción (agarrar, colocar, quitar, victoria, error).
