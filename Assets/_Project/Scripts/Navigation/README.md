# Navigation

Scripts que detectan al jugador en el espacio y disparan transiciones, cambios de habitación o movimiento de objetos. Todos funcionan por colisión (trigger).

## Scripts

### TransitionTrigger
Trigger plano (BoxCollider) que activa una `Room` y desactiva otra cuando el jugador lo cruza. Gestiona además el quad de WallPenetrationTunneling para evitar artefactos VR durante la transición.

### SpawnTransitionTrigger
Trigger que llama a `RoomSpawnManager.AdvanceToNextRoom()` al cruzarlo. Soporta delay configurable, eventos Before/After, y se auto-asigna al manager si está presente en escena. Pensado para colocarse dentro de los prefabs de habitación.

### ProximityShifter
Mueve y rota un objeto destino hacia posiciones predefinidas cada vez que el jugador (o sus manos) entra al trigger. Cada posición tiene delay, velocidad de animación y sonido propio. Útil para botones que "huyen" cuando te acercas.
