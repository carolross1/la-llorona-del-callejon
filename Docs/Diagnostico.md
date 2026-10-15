# Diagnóstico Técnico del Proyecto: La Llorona del Callejón

**Fecha de evaluación:** 15 de octubre de 2026  
**Motor:** Unity 6.6 (6000.6.0f1) Universal 2D (URP)  
**Desarrollado para:** UTNG — Proyecto escolar en parejas (Carol y Princes)  
**Evaluador:** Desarrollador Senior Unity 2D (AI Pair Programmer)

---

## 1. Resumen Ejecutivo

El proyecto cuenta con una base funcional correspondiente al prototipo inicial (Avance 1): movimiento del personaje, animaciones básicas con Blend Trees 2D, lógica de sigilo mediante triggers de escondite y una inteligencia artificial para La Llorona con estados de patrulla y persecución.

Sin embargo, para cumplir con la rúbrica y la entrega de la **Fase A (Avance 2 - 30 de octubre)**, se requiere una reestructuración importante de la arquitectura: implementar el ciclo completo de estados de juego (`GameManager`, `GameSession`, `SaveSystem`, `SceneLoader`), las pantallas de navegación (`Splash`, `MainMenu`, `LevelSelect`, `PauseMenu`, `GameOver`, `LevelComplete`), el `HUD` con vidas y tutorial contextual, y un generador robusto del Nivel 1 (`Level1Builder`) que ensamble automáticamente el mapa con iluminación 2D, colisiones compuestas y configuración precisa de capas.

---

## 2. Estado Actual: Lo que Existe en el Proyecto

### 2.1. Scripts de Runtime
* `Assets/Scripts/Player/PlayerMovement.cs`: Control de movimiento top-down en 4 direcciones (WASD / flechas) y carrera con `Shift`. Usa la API moderna `Rigidbody2D.linearVelocity` de Unity 6. Expone `IsRunning` e `IsHidden`.
* `Assets/Scripts/Player/PlayerAnimation.cs`: Controla los parámetros del Animator (`MoveX`, `MoveY`, `Speed`) leyendo la velocidad lineal del Rigidbody2D.
* `Assets/Scripts/Player/HideSystem.cs`: Permite a Carlos entrar y salir de un escondite pulsando la tecla `E`. Congela el Rigidbody (`FreezeAll`), mueve al jugador a la posición del escondite y ajusta el `sortingOrder` para dibujarlo detrás del obstáculo.
* `Assets/Scripts/Systems/HideSpot.cs`: Componente para objetos de escondite; expone la posición de ocultamiento (`HidePosition`) y referencia a su `SpriteRenderer`.
* `Assets/Scripts/Llorona/lloronaAI.cs`: Máquina de estados (`Patrol`, `Wait`, `Chase`) con detección visual mediante cono de ángulo (`viewAngle`), distancia (`viewDistance`), oído por proximidad (`hearDistance`) y raycast contra obstáculos (`obstacleMask`). Respeta el estado oculto del jugador.
* `Assets/Scripts/Llorona/LloronaAnimation.cs`: Controla el Animator de La Llorona sincronizando dirección (`Facing`), velocidad y el booleano `IsChasing`.
* `Assets/Scripts/Llorona/LevelGoal.cs`: Trigger básico de meta que detecta al jugador y recarga la escena actual temporalmente.
* `Assets/Editor/Level1Builder.cs`: Script de Editor tipo `EditorWindow` para construir el Nivel 1 mediante campos públicos en el Inspector.

### 2.2. Animaciones y Controladores
* `Assets/Animations/Player/`: Contiene `Player.controller` con Blend Trees 2D de Idle y Walk en 4 direcciones (`Idle_Down`, `Idle_Up`, `Idle_Left`, `Idle_Right`, `Walk_Down`, `Walk_Up`, `Walk_Left`, `Walk_Right`).
* `Assets/Animations/Llorona/`: Contiene `Llorona.controller` con clips de caminata en 4 direcciones, reposo y el estado `Chase.anim`.

### 2.3. Recursos Gráficos (Sprites y Tiles)
* `Assets/Sprites/Environment/tiles.png`: Textura con 48 tiles divididos en 6 columnas × 8 filas (PPU 192, 192×192 px), ya cortados en el Sprite Editor (`tiles_0` a `tiles_47`).
* `Assets/Tiles/`: Contiene los 48 assets `tiles_0.asset` a `tiles_47.asset` ya generados.
* `Assets/Sprites/Environment/decoracion.png` y `escondites.png`: Hojas de sprites cortadas que contienen los puestos, faroles, sacos, barriles, puertas y cajas.
* `Assets/Sprites/Player/` y `Assets/Sprites/Llorona/`: Sprites de Carlos y La Llorona para reposo y movimiento.
* `Assets/Sprites/UI/`: Contiene `logo.png` e `interfaz.png`.

### 2.4. Escenas
* `Assets/Scenes/Level1_Mercado.unity`: Escena activa con configuración básica de Tilemap, límites, objetos decorativos, Carlos y La Llorona.

### 2.5. Configuración de Capas y Tags
* En `ProjectSettings/TagManager.asset` existen las capas (`Layers`): `Player` (6), `Llorona` (7), `Walls` (8), `HideSpot` (9), `Interactable` (10), `Background` (11), `Ground` (12), `Props` (13), `Characters` (14), `Foreground` (15), `UI` (16).
* `SortingLayers`: Solo existen `Default`, `Characters` y `Props`.

---

## 3. Lo que Falta en el Proyecto

### 3.1. Arquitectura y Managers (Fase A)
* `GameManager`: Máquina de estados global (`Playing`, `Paused`, `LevelCompleted`, `GameOver`), conteo de vidas (3 iniciales), temporizador de nivel y cálculo de puntaje.
* `GameSession`: Objeto persistente (`DontDestroyOnLoad`) para mantener vidas y datos entre recargas y cambios de escena.
* `SceneLoader`: Manejo centralizado de transiciones asíncronas con efecto de cortinilla o fundido a negro (Fade In / Fade Out).
* `SaveSystem`: Almacenamiento seguro en `PlayerPrefs` para: niveles desbloqueados, puntajes récord, volumen de audio y configuración gráfica.

### 3.2. Interfaces de Usuario (UI / HUD)
* `HUD`: Barra/iconos de corazones (3 vidas), temporizador en pantalla, puntaje acumulado, texto de objetivo actual y sistema de tutorial contextual dinámico en pantalla (ej. "WASD para moverte", "Presiona E para esconderte").
* `PauseMenu`: Panel modal activable con `Escape` que pause el tiempo (`Time.timeScale = 0`), con botones funcionales: Reanudar, Reiniciar nivel, Opciones y Salir al menú.
* `LevelCompletedScreen`: Pantalla de victoria al cruzar la meta con tiempo transcurrido, bonificación de puntaje, botón Siguiente Nivel y Menú Principal.
* `GameOverScreen`: Pantalla de derrota al agotar las 3 vidas, con opciones de Reintentar Nivel y Volver al Menú.
* `Canvas Scaler`: Configuración uniforme en modo `Scale With Screen Size` (1920×1080, Match 0.5) y anclajes adaptables a pantallas anchas y móviles.

### 3.3. Escenas del Flujo Principal
* `Assets/Scenes/Splash.unity`: Pantalla de presentación con logotipo del equipo, nombre del juego y carga automática tras 3 segundos.
* `Assets/Scenes/MainMenu.unity`: Menú principal con fondo temático, logo y botones: Jugar, Continuar, Selección de Nivel, Opciones, Créditos y Salir (ocultando Salir en compilaciones WebGL).
* `Assets/Scenes/LevelSelect.unity`: Selección con botones de niveles 1 a 4 (nivel 1 desbloqueado por defecto, 2-4 bloqueados según progreso guardado).
* `Assets/Scenes/Credits.unity`: Pantalla con créditos del equipo, herramientas y mención obligatoria de arte generado por IA.

### 3.4. Audio
* La carpeta `Assets/Audio/` está vacía (sin archivos `.wav`, `.mp3` ni `.ogg`). Faltan clips provisionales o definitivos para: pasos, llanto lejano en bucle, sonido de escondite, campana, clic de botones y música de fondo ambiental.
* No existe `AudioMixer` con grupos `Master`, `Music` y `SFX`.

### 3.5. Carpeta de Documentación
* La carpeta `Docs/` en la raíz del repositorio no existía. Se requiere crear y mantener los documentos solicitados por la rúbrica escolar.

---

## 4. Problemas y Advertencias Detectados

1. **Inconsistencias de rutas y nombres de archivo:**
   * `Assets/Scripts/Llorona/lloronaAI.cs`: El archivo inicia con minúscula (`lloronaAI.cs`) mientras que la clase interna es `LloronaAI`. En C# y en sistemas de archivos sensibles a mayúsculas (como entornos Linux/WebGL/Git) esto genera conflictos de referencia.
   * `HideSystem.cs` está en `Assets/Scripts/Player/` en lugar de `Assets/Scripts/Systems/`.
   * `LevelGoal.cs` está en `Assets/Scripts/Llorona/` en lugar de `Assets/Scripts/Systems/`.
   *(Nota: para corregir estas rutas se debe usar `git mv` o herramientas de Unity para no romper los archivos `.meta` ni sus GUIDs).*

2. **Riesgo de `NullReferenceException` con Input System en `PlayerMovement.cs`:**
   * En `PlayerMovement.Update()`, se accede directamente a `Keyboard.current` sin comprobar si es nulo (`Keyboard.current.dKey...`). Si se ejecuta en dispositivos táctiles (Android) o plataformas donde no haya teclado conectado, arrojará excepciones continuas. Se debe agregar protección de nulidad (`if (Keyboard.current == null) return;`).

3. **Limitaciones del `Level1Builder.cs` actual:**
   * El script actual hereda de `EditorWindow` y requiere que el usuario arrastre manualmente cada tile y cada sprite al Inspector antes de pulsar el botón. No aprovecha la búsqueda programática por GUID / `AssetDatabase.LoadAssetAtPath`.
   * **Error de Tag:** El builder intenta asignar `go.tag = "HideSpot"`, pero el tag `"HideSpot"` no está registrado en `TagManager.asset`, lo cual lanza `UnityException: Tag: HideSpot is not defined.`.
   * **Falta el componente `HideSpot`:** El builder crea el objeto con trigger, pero **no** le añade el componente `HideSpot` ni crea el GameObject hijo `HidePoint`. En consecuencia, `HideSystem.cs` no puede detectar el escondite al entrar en el trigger.
   * **Falta iluminación 2D:** No configura `Global Light 2D` (intensidad 0.2, color azul-morado) ni las `Point Light 2D` cálidas en los faroles especificadas en el diseño.
   * **Falta `CompositeCollider2D` en paredes:** El builder actual añade únicamente `TilemapCollider2D` en lugar de combinarlo con `CompositeCollider2D` y `Rigidbody2D Static` (para optimización de colisiones continuas y sin atascos de esquinas).

4. **Desconexión con el flujo de vidas y victoria:**
   * `lloronaAI.cs` y `LevelGoal.cs` invocan directamente `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)` en lugar de comunicarse con el `GameManager`.

5. **Dependencia de Cinemachine:**
   * La especificación menciona el uso de Cinemachine 2D (`CinemachineCamera` en Unity 6). Sin embargo, el paquete `com.unity.cinemachine` no está incluido actualmente en `Packages/manifest.json`. Si no se instala dicho paquete, se debe utilizar un controlador suave de cámara 2D (`CameraFollow2D`) con límites ortográficos para garantizar el seguimiento sin errores de compilación.

6. **Instancias concurrentes de Unity Editor detectadas:**
   * Se identificaron procesos de `Unity.exe` activos en el sistema correspondientes al proyecto abierto por el usuario. Cuando Unity Editor tiene el proyecto abierto, ejecutar scripts por línea de comandos con `-batchmode` sobre el mismo directorio fallará debido al bloqueo exclusivo de `UnityLockfile`.
   * **Solución de ingeniería:** Implementar todos los scripts de Editor con atributos `[MenuItem("La Llorona/...")]`. De este modo, la generación puede ejecutarse directamente desde el menú superior de Unity mientras el editor esté abierto, y también mediante batchmode si el editor se cierra.

---

## 5. Conflictos y Diferencias Respecto al GDD Original

| Elemento | GDD Original (Unidad 1) | Implementación Actual / Rúbrica | Justificación Técnica |
| :--- | :--- | :--- | :--- |
| **Versión de Unity** | Unity 2022 LTS | Unity 6.6 (6000.6.0f1) URP 2D | El proyecto fue inicializado en Unity 6 aprovechando nuevas APIs como `linearVelocity` y mejoras nativas del Universal 2D. |
| **Patrulla en Nivel 1** | La Llorona no patrulla (solo llanto lejano). | Patrulla vertical en Zona 3 (X=29, Y=3.6 a 12.4). | Enseña al jugador la mecánica de evadir y esperar el momento oportuno antes de los niveles avanzados. |
| **Cantidad de escondites** | 1 escondite en el mapa. | 3 escondites estratégicos (zonas 1, 2 y 3). | Permite un ritmo progresivo de sigilo (zona 1: aprendizaje; zona 2: seguridad; zona 3: evasión directa de la patrulla). |
| **Sorting Layers** | No especificado a fondo. | Configuración estricta: `Background`, `Ground`, `Props`, `Characters`, `Foreground`, `UI`. | Evita solapamientos erróneos de sprites en perspectiva top-down y asegura la correcta oclusión en escondites. |

---

## 6. Conclusión y Recomendación

El código base es sólido y funcional en sus mecánicas elementales de movimiento e IA. La prioridad inmediata para la **Fase A (Avance 2)** es la infraestructura de gestión (`GameManager`, `GameSession`, `SceneLoader`, `SaveSystem`), el sistema de interfaz completo (HUD contextual, menús modales de pausa, victoria y game over), la corrección y automatización total del `Level1Builder` con iluminación 2D, y la creación de las escenas de flujo (`Splash`, `MainMenu`, `LevelSelect`).
