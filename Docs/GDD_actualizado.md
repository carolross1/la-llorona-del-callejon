# GAME DESIGN DOCUMENT (GDD) ACTUALIZADO
## La Llorona del Callejón

**Género:** Terror / Sigilo 2D Top-Down  
**Ambientación:** Dolores Hidalgo, C.I.N., Guanajuato, México  
**Plataformas:** PC (Windows .exe), Web (WebGL en itch.io) y Móvil (Android APK)  
**Equipo de Desarrollo:** Carol Ross y Princes  
**Institución:** UTNG — Universidad Tecnológica del Norte de Guanajuato  
**Versión del Documento:** 2.0 (Actualizado a la Unidad 2 / Avance 2)  

---

## 1. Concepción de la Idea

### 1.1. Resumen
*La Llorona del Callejón* es un videojuego de sigilo y terror nocturno en perspectiva 2D cenital (top-down). Carlos, un estudiante de Dolores Hidalgo, regresa tarde a casa cruzando el callejón del pueblo tras la fiesta patronal. Se escucha un llanto desgarrador en la distancia: La Llorona acecha las calles desiertas. El objetivo del jugador es guiar a Carlos hasta la seguridad de su hogar a lo largo de 4 niveles, aprovechando puestos de feria, barriles, puertas y sombras para ocultarse sin ser atrapado.

### 1.2. Justificación Técnica y Perspectiva
Se selecciona la perspectiva **2D top-down (vista desde arriba)** porque:
* Elimina la necesidad de salto y gravedad compleja, permitiendo centrar la experiencia en la tensión, la visión y el sigilo en 4 direcciones (WASD / flechas).
* Se adapta de forma óptima a pantallas panorámicas (PC/Web) y formatos móviles (Android).
* Permite el uso de un sistema de oclusión en profundidad mediante `Transparency Sort Mode = Custom Axis (0, 1, 0)`, donde los personajes se dibujan por delante o por detrás de los obstáculos según su coordenada vertical Y.

---

## 2. Mecánicas Principales de Juego

1. **Movimiento y Carrera:**
   * Desplazamiento en 4 direcciones a velocidad base (3 u/s) mediante `Rigidbody2D.linearVelocity`.
   * Carrera (`Shift`): duplica la velocidad (6 u/s), pero genera ruido que amplía el cono de detección de La Llorona ×1.5 veces.
2. **Sistema de Escondite:**
   * Al estar cerca de un obstáculo interactivo (puesto, barril, puerta) y presionar la tecla `E`, Carlos entra al escondite.
   * Físicamente se desactiva su detección para La Llorona y su `sortingOrder` pasa a ser menor que el del obstáculo, ocultándolo visualmente detrás de este.
   * Presionar `E` nuevamente devuelve a Carlos a su posición exterior.
3. **Sistema de Vidas y Derrota:**
   * Carlos cuenta con **3 vidas** iniciales que persisten entre reinicios mediante `GameSession` (`DontDestroyOnLoad`).
   * Si La Llorona toca a Carlos mientras no está escondido, se pierde una vida y se reinicia el nivel. Al llegar a 0 vidas, se despliega la pantalla de **Game Over**.
4. **Condición de Victoria:**
   * En los niveles 1, 2 y 3: alcanzar la zona de meta en el extremo opuesto del mapa.
   * En el nivel 4: alcanzar la puerta de la casa de Carlos sin ser atrapado.
5. **Ofrendas (Velas - Niveles 3 y 4):**
   * Velas que pueden ser recogidas y colocadas para llamar la atención de La Llorona, distrayéndola durante 3 segundos para permitir el paso por cuellos de botella.

---

## 3. Estructura de los 4 Niveles

| Nivel | Nombre de Escena | Entorno y Atmósfera | Mecánica Principal | Comportamiento de La Llorona |
| :--- | :--- | :--- | :--- | :--- |
| **1** | `Level1_Mercado` | Plaza del mercado vacío, puestos cerrados, suelo de adoquín y tierra. | Esconderse (`E`), evasión básica. Tutorial contextual. | Patrulla vertical en Zona 3 (X=29, Y=3.6 a 12.4). Llanto lejano. |
| **2** | `Level2_Callejon` | Callejón estrecho y largo con faroles titilantes y giros cerrados. | Carrera medida, faroles parpadeantes (`LightFlicker`). | Patrulla continua A → B, persecución activa si detecta ruido o visión. |
| **3** | `Level3_Atrio` | Explanada colonial frente a la parroquia con fuente central y bancas. | Ofrendas de velas para distracción táctica (3 s). | Patrulla circular en bucle alrededor de la fuente central. |
| **4** | `Level4_Cementerio` | Panteón con tumbas, criptas y niebla espesa; casa de Carlos al fondo. | Evasión de alta velocidad y escape final. | Mayor velocidad, búsqueda agresiva. Cinemática y grito final. |

### Dificultad Progresiva de La Llorona
| Nivel | patrolSpeed | chaseSpeed | viewDistance | viewAngle | Escondites |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | 1.2 | 2.0 | 4.0 | 90° | 3 |
| **2** | 1.5 | 2.5 | 5.0 | 100° | 3 |
| **3** | 1.6 | 2.8 | 5.5 | 110° | 3 |
| **4** | 2.0 | 3.4 | 6.5 | 120° | 4 |

---

## 4. Los 4 Estados de Cada Nivel

En cumplimiento estricto de la rúbrica de evaluación, cada nivel implementa:
1. **Jugar:** Nivel en curso con movimiento, colisiones, HUD activo (3 vidas en corazones, cronómetro, puntaje, objetivo y tutorial contextual) e IA reactiva.
2. **Pausado:** Detención total del juego con `Time.timeScale = 0` activada mediante tecla `Escape`. Ofrece: *Reanudar*, *Reiniciar Nivel*, *Opciones* y *Salir al Menú Principal*.
3. **Fin del Nivel:** Pantalla de victoria con tiempo total, puntaje acumulado, bonificación de sigilo, botón *Siguiente Nivel* y *Menú Principal*.
4. **Game Over:** Pantalla de derrota al agotarse las 3 vidas, con *Reintentar Nivel* y *Volver al Menú*.

---

## 5. Arquitectura del Proyecto

```
Assets/
├── Scenes/            Splash, MainMenu, LevelSelect, Level1_Mercado, Level2_Callejon,
│                      Level3_Atrio, Level4_Cementerio, Credits
├── Scripts/
│   ├── Player/        PlayerMovement, PlayerAnimation
│   ├── Llorona/       LloronaAI, LloronaAnimation, LloronaSettings
│   ├── Managers/      GameManager, GameSession, SceneLoader, AudioManager, SaveSystem
│   ├── Systems/       HideSystem, HideSpot, LevelGoal, TutorialTrigger, Candle, Offering
│   ├── UI/            HUD, PauseMenu, MainMenuUI, LevelSelectUI, LevelCompletedUI, GameOverUI, SplashScreen
│   └── Utils/         CameraFollow2D
├── Editor/            Level1Builder, UIBuilder, BuildScript
├── Sprites/
│   ├── Player/        Carlos, caminatas
│   ├── Llorona/       Llorona, movimiento, persecución
│   ├── Environment/   tiles, decoracion, escondites, puesto_mercado, vela
│   └── UI/            logo, interfaz
├── Tiles/             tiles_0 a tiles_47
└── Audio/             Music, SFX
```
