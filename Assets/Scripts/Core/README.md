# Core

Scripts de inicialización del entorno VR. Se ejecutan una sola vez al arrancar la escena y no tienen dependencias de gameplay.

## Scripts

### VRCameraInitialRotation
Fuerza la rotación Y del Camera Rig al iniciar el juego. Útil para que el jugador mire en la dirección correcta desde el primer frame. Se auto-detecta el rig por nombre si no se asigna manualmente.

### VRPlayerPositionReset
Reposiciona el Camera Rig al iniciar y corrige automáticamente la altura del headset. Incluye monitoreo continuo: si el headset supera una altura máxima (por problemas de Guardian boundary), lo recentra sin afectar la posición horizontal.
