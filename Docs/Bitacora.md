# Bitácora de Avances: La Llorona del Callejón

**Materia:** Desarrollo de Videojuego 2D/3D (UTNG)  
**Integrantes:** Carol Ross y Princes  
**Profesor/Evaluador:** Rúbrica de Avances Graduales  

---

## Entrada 1 — Avance 1: Prototipo Base (Viernes 16 de Octubre)
* **Objetivo:** Prototipo base jugable: personaje Carlos, controles de movimiento, animaciones en 4 direcciones, mecánica de escondite y enemigo básico.
* **Lo realizado:**
  * Configuración inicial del proyecto en Unity 6 Universal 2D (URP).
  * Creación de `PlayerMovement.cs` con Input System nuevo y `Rigidbody2D.linearVelocity`.
  * Animaciones de Carlos (Idle y Walk en 4 direcciones con Blend Trees 2D).
  * Mecánica de escondite (`HideSystem.cs` y `HideSpot.cs`) con ocultamiento físico y reordenamiento de capas (`sortingOrder`).
  * IA de La Llorona (`LloronaAI.cs` y `LloronaAnimation.cs`) con cono de visión, distancia de oído y estados de patrulla y persecución.
  * Importación de la hoja de 48 tiles del pueblo (`tiles_0` a `tiles_47`).
* **Problemas encontrados:**
  * Al atrapar al jugador, la escena se recargaba directamente sin descontar vidas ni desplegar menú de derrota.
  * Los tiles y sprites requerían ser asignados manualmente en la escena.

---

## Entrada 2 — Avance 2: Nivel 1 Completo y Menús (Fase A)
* **Objetivo:** Nivel 1 ("El Mercado Vacío") finalizado con sus 4 estados funcionales (Jugar, Pausa, Fin de Nivel, Game Over), HUD contextual, vidas persistentes, menú principal y guardado.
* **Lo realizado:**
  * **Arquitectura de Gestión:**
    * `GameManager.cs`: Máquina de estados completa (`Playing`, `Paused`, `LevelCompleted`, `GameOver`), cronómetro y cálculo de puntaje.
    * `GameSession.cs`: Servicio persistente con `DontDestroyOnLoad` que mantiene las 3 vidas entre reinicios y cambios de escena.
    * `SceneLoader.cs`: Carga asíncrona de escenas con transición de fundido a negro (Fade In / Fade Out) independiente de `Time.timeScale`.
    * `SaveSystem.cs`: Almacenamiento seguro en `PlayerPrefs` para niveles desbloqueados (1 al 4), récords de puntaje, volúmenes de audio y calidad gráfica.
  * **Interfaz de Usuario (uGUI):**
    * `HUD.cs`: Visualización de 3 corazones de vida, cronómetro, puntaje, objetivo y banner interactivo de tutorial contextual.
    * `PauseMenu.cs`: Menú modal con tecla `Escape` (`Time.timeScale = 0`) con Reanudar, Reiniciar Nivel, Opciones y Menú Principal.
    * `LevelCompletedUI.cs`: Pantalla de victoria al cruzar la meta en X ≈ 39.2 con estadísticas y botón de Siguiente Nivel.
    * `GameOverUI.cs`: Pantalla de derrota al perder las 3 vidas con Reintentar o Volver al Menú.
    * `MainMenuUI.cs`: Menú principal con Jugar, Continuar, Selección de Nivel, Opciones (sliders de audio y dropdown de calidad), Créditos y Salir (oculto en WebGL).
    * `LevelSelectUI.cs`: Selector de los 4 niveles con validación de niveles bloqueados/desbloqueados según progreso.
    * `SplashScreen.cs`: Pantalla de carga inicial con logo y avance automático a los 3 segundos.
  * **Herramientas de Editor (Automatización):**
    * `Level1Builder.cs`: Reescrito completamente. Genera el Nivel 1 de 40×16 unidades cargando automáticamente tiles y sprites desde `AssetDatabase`, con colisiones compuestas (`TilemapCollider2D` + `CompositeCollider2D`), iluminación 2D (`Global Light 2D` y faroles cálidos), 3 escondites con `HideSpot` + `HidePoint`, waypoints de patrulla asignados a La Llorona y triggers de tutorial.
    * `UIBuilder.cs`: Genera y registra en `Build Settings` las escenas `Splash`, `MainMenu`, `LevelSelect`, `Credits` y el Canvas completo de `Level1_Mercado`.
  * **Utilidades:**
    * `CameraFollow2D.cs`: Seguimiento ortográfico suave (Size 5, Z=-10) con límites confinados a los márgenes del mapa.
    * `TutorialTrigger.cs`: Disparadores de mensajes contextuales en zonas clave.
* **Problemas superados:**
  * Se corrigieron advertencias de deprecación en Unity 6.6 (`FindAnyObjectByType`).
  * Se eliminaron excepciones de nulos en `PlayerMovement.cs` cuando no hay teclado conectado.
* **Lo pendiente para Avance 3 (Fase B):**
  * Construcción automatizada de Niveles 2 ("El Callejón del Beso") y 3 ("El Atrio de la Parroquia").
  * Mecánica de ofrendas (`Candle` y `Offering`) con distracción de 3 segundos en La Llorona.
  * `AudioManager` y `AudioMixer` con grupos Music y SFX, llanto 3D y música ambiental.
  * Efecto `LightFlicker` para faroles del callejón.
