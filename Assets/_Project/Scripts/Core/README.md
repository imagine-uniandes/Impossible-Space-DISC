# Core

Scripts de inicialización del entorno VR. Se ejecutan una sola vez al arrancar la escena y no tienen dependencias de gameplay.

## Scripts

### VRCameraInitialRotation
Fuerza la rotación Y del Camera Rig al iniciar el juego. Útil para que el jugador mire en la dirección correcta desde el primer frame. Se auto-detecta el rig por nombre si no se asigna manualmente.

### VRPlayerPositionReset
Reposiciona el Camera Rig al iniciar y corrige automáticamente la altura del headset. Incluye monitoreo continuo: si el headset supera una altura máxima (por problemas de Guardian boundary), lo recentra sin afectar la posición horizontal.

### GameTimer / GameTimerDisplay / GlobalTimerAnchor
Sistema de contador global para la experiencia de una sola escena (teleport + pool de salas).

- **GameTimer**: singleton con la *lógica* del tiempo. Vive en un objeto persistente de la escena, separado de las salas, por eso **nunca se reinicia ni se pausa** al cambiar de sala. `StartTimer()` / `StopTimer()` / `ResetTimer()`.
- **GameTimerDisplay**: el *visual* (TextMeshPro 3D). Objeto único en la escena; expone `Instance` estático.
- **GlobalTimerAnchor**: va en cada **prefab de sala/subsala**. Al activarse la sala (`OnEnable`, que dispara el pool en cada recicle) mueve el display único al `anchor` indicado. Con `parentToAnchor` el contador se hace hijo de la sala y la sigue aunque la teletransporten.

### FinalScoreDisplay
Puntuación final basada en el tiempo total, modelo de **usuario único** (sin registro): guarda un único **mejor tiempo global** en `PlayerPrefs`. Llama `ShowFinalScore()` desde la última puerta/trigger/botón: para el `GameTimer`, compara contra el récord, lo guarda si lo bate, y muestra "Tu tiempo" + "Mejor tiempo" en un TextMeshPro. Expone `GetBestTime()`, `HasBestTime()`, `ClearBestTime()` estáticos y eventos `onScoreCalculated` / `onNewRecord`.
