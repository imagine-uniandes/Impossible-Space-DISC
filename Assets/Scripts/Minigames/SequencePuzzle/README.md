# Minigames / SequencePuzzle

Puzzle de secuencia oculta: el jugador debe presionar un conjunto de botones en el orden correcto. Un error resetea el progreso. Al completarse puede activar/desactivar objetos y disparar un UnityEvent.

## Scripts

### SequencePuzzle
Manager central del puzzle. Define la lista de botones y el array `correctSequence` con los índices en orden. Valida cada pulsación, maneja el reset por error con delay y dispara las acciones de completado (activar objetos, sonido, evento).

### SequenceButton
Botón individual del puzzle. Cambia de color según su estado (normal / correcto / incorrecto) y ejecuta una animación de escala al presionarse. Se registra automáticamente en el `SequencePuzzle` al que pertenece. Llamar `PressButton()` desde los eventos XR o Unity.

### CountdownButton
Botón con contador regresivo: muestra un número en TextMeshPro que baja con cada pulsación. Al llegar a cero activa, desactiva o alterna objetos target. Independiente del `SequencePuzzle`, se puede usar solo para cualquier mecánica de "presionar N veces".
