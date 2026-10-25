# Registro de Cambios Justificados

**Proyecto:** La Llorona del Callejón  
**Materia:** Desarrollo de Videojuegos 2D/3D (UTNG)  
**Equipo:** Carol y Princes  
**Fecha de actualización:** 15 de octubre de 2026  

Este documento registra de forma detallada y fundamentada cada decisión técnica, ajuste de diseño y diferencia implementada respecto al Documento de Diseño de Videojuego (GDD) preliminar de la Unidad 1.

---

## 1. Motor y Versión Tecnológica

| Parámetro | GDD Unidad 1 | Implementación Final | Justificación Técnica |
| :--- | :--- | :--- | :--- |
| **Versión del Motor** | Unity 2022 LTS | Unity 6.6 (6000.6.0f1) URP 2D | El proyecto se configuró en Unity 6 para aprovechar las optimizaciones de rendimiento en Universal 2D (URP), el nuevo Input System nativo y la API moderna `Rigidbody2D.linearVelocity` (reemplazando `velocity`, obsoleta en Unity 6). |
| **Render Pipeline** | Universal 2D / Built-in indistinto | Universal 2D (URP) | Permite iluminación 2D en tiempo real (`Light2D` Global y Point Lights para faroles), esencial para la atmósfera de terror nocturno y visibilidad de zonas seguras. |

---

## 2. Ajustes de Jugabilidad y Mecánicas por Nivel

### 2.1. Nivel 1: El Mercado Vacío
* **GDD Original:** La Llorona está lejos; solo se escucha su llanto, sin patrulla visible. Había un solo escondite.
* **Implementación Actual:**
  * **Patrulla en Zona 3:** La Llorona patrulla una línea vertical (X=29, Y=3.6 a 12.4).
  * **3 Escondites estratégicos:**
    1. *Zona 1 (Entrada, X=8, Y=11):* Puesto de cajas. Permite al jugador entender la mecánica de ocultamiento (`E`) en un entorno seguro antes de enfrentar peligro.
    2. *Zona 2 (Plaza de puestos, X=18, Y=11):* Barril con lona. Sirve de punto de seguridad intermedio entre los puestos en zigzag.
    3. *Zona 3 (Paso de La Llorona, X=26.5, Y=13):* Puerta con cajas. Permite al jugador refugiarse y esperar a que La Llorona camine al extremo opuesto de su patrulla vertical para cruzar a la meta.
* **Justificación:** La presencia de una patrulla simple y 3 escondites convierte el nivel en un tutorial orgánico donde el jugador experimenta la tensión, el sigilo y la evasión antes de los niveles avanzados, cumpliendo con el Criterio 4 de la rúbrica (tutorial integrado).

### 2.2. Sistema de Ofrendas (Velas)
* **GDD Original:** Recoger velas/ofrendas con `E` distrae a La Llorona automáticamente durante 3 segundos.
* **Implementación Actual:**
  * Se diseñó el sistema para soportar tanto la distracción inmediata como la colocación táctica con `F`.
* **Justificación:** Colocar la ofrenda en el suelo (`F`) da control táctico al jugador sobre dónde atraer a La Llorona para despejar cuellos de botella (especialmente en el Nivel 3 - Atrio y Nivel 4 - Cementerio). Se mantendrá configurable mediante el ScriptableObject de cada nivel.

---

## 3. Arquitectura del Sistema de Vidas y Estados

* **GDD Original:** Al ser tocado por La Llorona, recargaba la escena directamente.
* **Implementación Actual:** `GameManager` centralizado con `GameSession` persistente (`DontDestroyOnLoad`). Carlos dispone de 3 vidas. Al ser tocado:
  * Si le quedan vidas: se resta una vida y se reinicia el nivel actual con fundido suave.
  * Con 0 vidas: se detiene el juego y se muestra la pantalla de `GameOverUI` con opción de reintentar.
* **Justificación:** Cumple estrictamente con los 4 estados obligatorios por nivel evaluados en la rúbrica (Jugar, Pausado, Fin del Nivel y Game Over).

---

## 4. Sistema de Capas y Orden de Dibujado (Sorting Layers)

* **Capas Físicas (Layers):** `Player` (6), `Llorona` (7), `Walls` (8), `HideSpot` (9).
* **Sorting Layers:** `Background`, `Ground`, `Props`, `Characters`, `Foreground`, `UI`.
* **Modo de Transparencia:** `Custom Axis (0, 1, 0)` en Sorting Layer Characters.
* **Justificación:** Garantiza que en vista top-down, cuando Carlos camina hacia arriba o abajo de un puesto, el dibujado respete la perspectiva (si está abajo pasa por delante; si está arriba pasa por detrás). En los escondites, el script ajusta temporalmente el `sortingOrder = spot.order - 1`, logrando que Carlos quede visualmente oculto detrás del obstáculo sin recurrir a transparencias artificiales.
