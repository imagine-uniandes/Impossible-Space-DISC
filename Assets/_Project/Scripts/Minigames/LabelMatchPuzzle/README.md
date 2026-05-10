# Minigames / LabelMatchPuzzle

Puzzle de arrastrar etiquetas a sus objetos correspondientes. El jugador agarra etiquetas en VR y las coloca en slots. Al llenar todos los slots se evalúa si cada etiqueta está en el lugar correcto y se muestra feedback visual contextual.

## Scripts

### LabelMatchPuzzleManager
Manager del puzzle. Escucha cambios de los slots y evalúa cuando todos están llenos. En caso de error puede mostrar feedback contextual: instancia el objeto del slot incorrecto y muestra su etiqueta con "?" en un TMP. Dispara `onPuzzleSolved` / `onPuzzleFailed` para conectar la apertura de puertas u otros eventos.

### LabelObjectSlot
Slot de destino para una etiqueta. Gestiona el snap al `snapPoint`, permite o bloquea reemplazos, y notifica al manager en cada cambio. Compara el `LabelId` de la etiqueta recibida con su `expectedLabelId` para determinar si es correcto.

### LabelTagItem
Etiqueta agarrable en VR. Al soltarla busca el `LabelObjectSlot` más cercano dentro de su radio y se snappea. Llama `OnGrabbed()` desde Select Entered y `OnReleasedTryPlace()` desde Select Exited. Al estar colocada, congela el Rigidbody para evitar que caiga.

### MiniPuzzleVisualSwitch
Feedback visual de resolución: cambia el color de un array de Renderers al `solvedColor` y oculta un texto TMP. Se llama con `ExecutePuzzle()` desde un UnityEvent, típicamente conectado al `onPuzzleSolved` del manager.
