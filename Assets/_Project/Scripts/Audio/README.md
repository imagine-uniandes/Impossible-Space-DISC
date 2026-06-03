# Audio

Sistema de audio ambiental para la experiencia de una sola escena (teleport + pool de salas).

## Scripts

### AmbientMusicPlayer
Reproductor de música de fondo **global y persistente** (objeto único en la escena, igual que `GameTimer`). Es **una sola fuente de música** con dos AudioSources que hacen **crossfade**. `PlayTrack(clip)` cambia de pista con fundido (e ignora la petición si ya suena esa misma pista, así no hay cortes); `SetVolume(v)`; `Stop()`. No reproduce nada por sí solo salvo el `backgroundMusic` por defecto: la música por sala la deciden los `RoomMusicTrack`.

### RoomMusicTrack
Selector de música **por sala** (va en cada prefab de sala, incluida la intermedia/antecámara y la inicial). En `OnEnable` le pide al `AmbientMusicPlayer` que ponga **su** pista con crossfade. Como el player ignora la pista repetida, dos salas seguidas con la misma música no cortan. Así la música **alterna entre salas** sin solapamientos. `overrideVolume` por pista y `instantOnFirstPlay` (sin fundido la primera vez, útil en la sala inicial).

### RoomAmbience
Sonido ambiente **propio de cada sala** (va en el prefab). Colchón en bucle (zumbido/drone) + sonidos puntuales aleatorios opcionales. Arranca/para con `OnEnable`/`OnDisable`, así el pool lo gestiona solo. Es independiente de la música: puede sonar a la vez que el `RoomMusicTrack`.

## Reparto recomendado

- **Música de fondo que cambia por sala** → `RoomMusicTrack` en cada prefab + `AmbientMusicPlayer` (el player es el único que suena música).
- **Ambiente por sala** (drone servidores, zumbido IA…) → `RoomAmbience` en cada prefab.
- **SFX de puzzles** → ya lo llevan los managers de cada minijuego (ej. `CodePuzzleAudioManager`).

> ⚠️ La sala de código ya tenía su música en `CodePuzzleAudioManager.backgroundMusic`. Al migrar al sistema por sala, **vacía ese campo** y pon la música de código en un `RoomMusicTrack` del prefab de código, para no tener dos músicas a la vez.
