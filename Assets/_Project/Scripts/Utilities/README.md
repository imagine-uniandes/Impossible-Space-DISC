# Utilities

Helpers genéricos reutilizables sin lógica de gameplay propia. Se pueden conectar a cualquier sistema vía UnityEvents o trigger de colisión.

## Scripts

### ObjectToggler
Activa, desactiva o alterna GameObjects. Puede funcionar llamado desde UnityEvents (botones, puzzles) o automáticamente al detectar al jugador en un collider trigger. Soporta destruir en lugar de solo desactivar.

### UnitMarkerTrigger
Trigger invisible que reproduce un sonido 3D cuando el jugador lo cruza. Sirve para marcar posiciones específicas en el espacio VR. Incluye un helper de editor para generar grillas de marcadores en los ejes X/Z.

### UnitStepSoundTracker
Reproduce un sonido cada vez que el jugador avanza 1 unidad Unity (configurable). Diseñado para calibrar escala y distancias en VR. Puede usar pitches distintos para el eje X y el eje Z.
