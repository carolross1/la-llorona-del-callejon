# Registro Oficial de Assets Pendientes
**Proyecto:** La Llorona del Callejón (Unity 6.6)  
**Documento base:** `PROMPT_VISUAL.md` (Secciones 5.1, 7, 8)  
**Fecha:** 24 de octubre de 2026  

Este documento registra los recursos visuales pendientes que no existen en los spritesheets actuales. En el juego, cada elemento faltante está señalizado con un GameObject vacío con nombre `PENDIENTE_[Nombre]` y un Gizmo visible para no bloquear la jugabilidad.

---

## 1. Animaciones Dedicadas del Perro Callejero (Sección 8)

Actualmente, `Sprites/Perro/perro.png` provee 4 vistas de referencia de pie (`Perro_frente`, `Perro_espalda`, `Perro_lado_izq`, `Perro_lado_der`). Mientras se generan las hojas dedicadas, el juego utiliza las 4 vistas con transformaciones dinámicas en tiempo de ejecución (compresión y respiración oscilante para dormir; sacudida horizontal e icono `!` para ladrar).

### Pendiente 1.1: Hoja del Perro Echado Durmiendo
* **Descripción:** Sprite sheet con 4 cuadros de animación de ciclo de respiración de un perro callejero mestizo café oscuro, flaco, echado en el suelo sobre sus patas, durmiendo en perspectiva top-down 2D.
* **Resolución recomendada:** 1024 × 256 px (4 cuadros de 256 × 256 px).
* **Fondo:** Transparente (sin fondo verde ni blanco).
* **Prompt sugerido:**
  > `2D pixel art sprite sheet of a skinny dark brown stray dog sleeping lying down on the ground, breathing idle loop animation, 4 frames side view slightly top-down perspective, dark colonial Mexico atmosphere, clean alpha transparency, no background.`

### Pendiente 1.2: Hoja del Perro Ladrando
* **Descripción:** Sprite sheet con 4 cuadros de animación de ladrido agresivo, hocico abierto, cuerpo tenso levantado, mirando en vista lateral.
* **Resolución recomendada:** 1024 × 256 px (4 cuadros de 256 × 256 px).
* **Fondo:** Transparente.
* **Prompt sugerido:**
  > `2D pixel art sprite sheet of a skinny dark brown stray dog barking aggressively with open mouth, alert territorial posture, 4 frames side view, horror colonial Mexico game style, clean transparent background, no background.`

### Pendiente 1.3: Hoja del Perro Corriendo / Persecución
* **Descripción:** 4 a 6 cuadros de carrera lateral rápida.
* **Prompt sugerido:**
  > `2D pixel art sprite sheet of a stray dog running side view loop, 4 frames, horror game, transparent background.`

---

## 2. Elementos del Nivel 3: El Atrio de la Parroquia (Sección 7)

Actualmente, el Nivel 3 se estructura con el portón monumental `Salida_2` y la arquitectura colonial disponible. Los huecos arquitectónicos específicos están marcados con `PENDIENTE_*`:

| Objeto en Escena | Descripción Visual Requerida | Prompt Sugerido |
|---|---|---|
| `PENDIENTE_Fuente_Octagonal` | Fuente colonial de cantera rosa tallada con agua estancada y reflejos, de forma octagonal, vista top-down / isométrica (~4 u de ancho). | `2D top-down game asset, antique colonial Mexican octagonal stone fountain carved from pink quarry stone, dark stagnant water, isolated on transparent background, pixel art aesthetic.` |
| `PENDIENTE_Fachada_Parroquia` | Fachada monumental barroca de la Parroquia de Nuestra Señora de los Dolores (Guanajuato), con dos campanarios y nichos coloniales (~12 u de ancho). | `2D architectural facade of a Mexican colonial baroque parish church, Guanajuato pink cantera stone, bell towers, night lighting, transparent background.` |
| `PENDIENTE_Arboles_Atrio` | Árboles frondosos de plaza colonial (laureles de la India o fresnos centenarios) que arrojan sombras sobre el atrio. | `2D top-down game asset, large colonial plaza tree canopy, dark moody lighting, isolated transparent background.` |
| `PENDIENTE_Cruces_Hornacinas` | Cruces de cantera atrial y hornacinas con nichos de santos con veladoras votivas encendidas. | `2D stone atrial cross and stone wall niches with lit candles, Mexican colonial horror, transparent background.` |

---

## 3. Elementos del Nivel 4: El Panteón de las Ánimas (Sección 7)

Actualmente, el Nivel 4 concluye con la casa humilde de Carlos `Salida_3`. Los huecos de elementos fúnebres están marcados con `PENDIENTE_*`:

| Objeto en Escena | Descripción Visual Requerida | Prompt Sugerido |
|---|---|---|
| `PENDIENTE_Lapidas_Antiguas` | Variedad de 4 a 6 lápidas y tumbas de piedra desgastadas por el tiempo con inscripciones borrosas y cruces de hierro forjado. | `2D cemetery tombstones and gravestones set, weathered stone with moss and cracked crosses, Mexican gothic cemetery, transparent background.` |
| `PENDIENTE_Cripta_Monumental` | Mausoleo familiar colonial de cantera gris con rejas de hierro oxidado y puerta entreabierta (~4 u de ancho). | `2D gothic stone crypt mausoleum, rusty iron gate, horror atmosphere, transparent background.` |
| `PENDIENTE_Cipreses_Panteon` | Cipreses altos y lúgubres que enmarcan los caminos del camposanto. | `2D tall cemetery cypress trees, misty gloomy night, game asset, transparent background.` |
| `PENDIENTE_Senderos_Cempasuchil` | Caminos de pétalos de flor de cempasúchil naranja brillante que guían hacia las ofrendas entre las tumbas. | `2D top-down marigold flower petal path trail (cempasúchil), glowing orange petals on dirt ground, transparent background.` |
| `PENDIENTE_Reja_Cementerio` | Tramos modulares de reja de hierro forjado gótico con puntas de lanza para delimitar las secciones del panteón. | `2D modular wrought iron cemetery fence with spear tops, colonial horror, transparent background.` |

---

## 4. Elementos de Interfaz (Sección 6)

| Objeto | Descripción | Estado Actual |
|---|---|---|
| `Sprite_Candado_Bloqueado` | Candado rústico de hierro forjado para las tarjetas de niveles bloqueados en `LevelSelect`. | Sustituido temporalmente por filtro en escala de grises y etiqueta de texto `BLOQUEADO`. |
