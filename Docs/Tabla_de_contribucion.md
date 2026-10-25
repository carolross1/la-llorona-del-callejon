# Tabla de Contribución Individual

**Proyecto:** La Llorona del Callejón  
**Modalidad:** Parejas (4 niveles jugables)  
**Institución:** UTNG — Universidad Tecnológica del Norte de Guanajuato  
**Integrantes:**
1. **Carol Ross** (`carolross1`)
2. **Princes**

---

## 1. Distribución General de Roles

| Área | Responsable Principal | Responsable de Apoyo |
| :--- | :--- | :--- |
| **Arquitectura y Programación de Sistemas** | Carol Ross | Princes |
| **Inteligencia Artificial y Enemigos** | Carol Ross | Princes |
| **Diseño de Niveles y Tilemaps** | Princes | Carol Ross |
| **Interfaz de Usuario (UI / HUD)** | Carol Ross | Princes |
| **Audio, Efectos y Sonorización** | Princes | Carol Ross |
| **Narrativa, Diálogos y Guion** | Princes | Carol Ross |
| **Pruebas, Optimización y Despliegue** | Carol Ross | Princes |

---

## 2. Registro de Tareas y Commits por Avance

### Avance 1: Prototipo Base
| Fecha | Integrante | Tarea Realizada | Rama / Commit Asociado |
| :--- | :--- | :--- | :--- |
| 14/10/2026 | Carol | Programación de `PlayerMovement.cs` y Blend Trees en Unity 6. | `feature/carlos-movimiento` |
| 14/10/2026 | Carol | Implementación del sistema de escondite (`HideSystem` y `HideSpot`). | `feature/carlos-escondite` |
| 15/10/2026 | Carol | Desarrollo de la IA de La Llorona (`LloronaAI`) con patrulla y visión. | `feature/llorona-ia` |
| 15/10/2026 | Princes | Armado de paleta de tiles y corte de sprites de entorno. | `feature/diseño-nivel1` |
| 15/10/2026 | Princes | Configuración de animaciones de caminata y persecución. | `feature/animaciones-llorona` |

---

### Avance 2: Nivel 1 Completo y Menús
| Fecha | Integrante | Tarea Realizada | Rama / Commit Asociado |
| :--- | :--- | :--- | :--- |
| 15/10/2026 | Carol | Implementación de `GameManager` (4 estados), `GameSession` y `SaveSystem`. | `feature/fase-a-nivel1-menus` |
| 15/10/2026 | Carol | Creación de `SceneLoader` con transiciones suaves de fundido a negro. | `feature/fase-a-nivel1-menus` |
| 15/10/2026 | Carol | Desarrollo de scripts de UI: `HUD`, `PauseMenu`, `LevelCompletedUI`, `GameOverUI`. | `feature/fase-a-nivel1-menus` |
| 15/10/2026 | Princes | Diseño de pantallas: `SplashScreen`, `MainMenuUI` y `LevelSelectUI`. | `feature/fase-a-nivel1-menus` |
| 15/10/2026 | Carol & Princes | Automatización del constructor del mapa y UI (`Level1Builder` y `UIBuilder`). | `feature/fase-a-nivel1-menus` |

---

### Avance 3: Niveles 2 y 3, Audio y Ofrendas (Próximo)
| Fecha | Integrante | Tarea Planificada |
| :--- | :--- | :--- |
| — | Carol | Sistema de ofrendas (`Candle`, `Offering`) y distracción de La Llorona. |
| — | Carol | Lógica de dificultad progresiva (`LloronaSettings`) y constructor de Nivel 2 y 3. |
| — | Princes | Integración de `AudioManager`, efectos sonoros (pasos, llanto, campana) y música ambiental. |
| — | Princes | Efecto de faroles parpadeantes (`LightFlicker`) y diseño del Callejón y Atrio. |

---

### Avance 4: Nivel 4, Narrativa y Opciones
| Fecha | Integrante | Tarea Planificada |
| :--- | :--- | :--- |
| — | Carol | Constructor de Nivel 4 (Cementerio, escape final) y panel de opciones avanzado. |
| — | Princes | Sistema de diálogos (`DialogueSystem`, `DialogueData`) y cinemática de cierre. |

---

### Avance 5 & Final: Despliegue Multiplataforma y Cierre
| Fecha | Integrante | Tarea Planificada |
| :--- | :--- | :--- |
| — | Carol & Princes | Builds para PC (.exe), Web (WebGL en itch.io) y Móvil (APK Android). |
| — | Carol & Princes | Pruebas de usuario externo, manual de usuario y video demostrativo. |
