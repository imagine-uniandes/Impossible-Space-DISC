# Audio

Sistema de audio ambiental para la experiencia de una sola escena (teleport + pool de salas).

## Scripts

### AmbientMusicPlayer
Música de fondo **global y persistente** (objeto único en la escena, igual que `GameTimer`). Suena en bucle y **no se reinicia al cambiar de sala**. Reutiliza la música que ya usas en la sala de código. `PlayTrack(clip)` cambia de pista con crossfade; `SetVolume(v)`; `Stop()`.

### RoomAmbience
Sonido ambiente **propio de cada sala** (va en el prefab). Colchón en bucle (zumbido/drone) + sonidos puntuales aleatorios opcionales. Arranca/para con `OnEnable`/`OnDisable`, así el pool lo gestiona solo.

## Reparto recomendado

- **Música de fondo continua** → `AmbientMusicPlayer` (una sola, toda la experiencia).
- **Ambiente por sala** (servidores, IA, intro) → `RoomAmbience` en cada prefab.
- **SFX de puzzles** → ya lo llevan los managers de cada minijuego (ej. `CodePuzzleAudioManager`).
