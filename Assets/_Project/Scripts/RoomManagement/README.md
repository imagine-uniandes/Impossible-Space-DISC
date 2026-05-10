# RoomManagement

Scripts que controlan el ciclo de vida de las habitaciones: instanciación, activación/desactivación y reciclado de prefabs. Son el núcleo del sistema de Impossible Spaces.

## Scripts

### Room
Representa una habitación individual. Expone métodos `Activate()` / `Deactivate()` que activan o desactivan todos los Renderers, luces y colliders hijos de golpe. Puede destruirse en lugar de solo ocultarse para liberar memoria.

### RoomPrefabPool
Pool de objetos genérico para reutilizar instancias de habitaciones. Evita picos de `Instantiate` / `Destroy` en runtime. Se le pasan los prefabs registrados, y devuelve instancias recicladas con `Get()` / `Release()`.

### RoomSpawnManager
Orquesta el flujo de habitaciones: instancia la habitación inicial, precarga la siguiente en estado inactivo y avanza al llamar `AdvanceToNextRoom()`. Usa `RoomPrefabPool` si está configurado, o destruye la habitación anterior directamente.
