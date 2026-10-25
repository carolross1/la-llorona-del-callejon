# Diagnóstico Visual y Técnico del Proyecto
**Proyecto:** La Llorona del Callejón (Unity 6.6)  
**Documento base:** `PROMPT_VISUAL.md`  
**Fecha:** 24 de octubre de 2026  

---

## 1. Sprites Provisionales: Uso y Ubicaciones Detectadas

En revisiones anteriores, el proyecto generaba proceduralmente sprites simples de pocos píxeles que reemplazaban el arte real.

| Sprite Provisional | Resolución | Dónde se generaba / usaba | Estado actual |
|---|---|---|---|
| `Environment/perro_callejero.png` | 96×96 px | Generado por `SpriteAssetGenerator.GenerateStrayDogSprite()`. Usado como fallback en `StrayDogAI` y constructores de nivel (`Level1Builder` a `Level4Builder`). | Eliminado del disco por el usuario; constructores tenían referencias en código como fallback. |
| `Environment/charco_lodo.png` | 128×64 px | Generado por `SpriteAssetGenerator.GeneratePuddleSprite()`. Usado en `CreateMudPuddle()` en todos los niveles. | Eliminado del disco por el usuario; constructores tenían referencias en código como fallback. |
| `Environment/basura_ruido.png` | 96×64 px | Generado por `SpriteAssetGenerator.GenerateTrashNoiseSprite()`. Usado en `CreateNoiseObstacle()`. | Eliminado del disco por el usuario; constructores tenían referencias en código como fallback. |
| `Environment/portal_meta.png` | 128×128 px | Generado por `SpriteAssetGenerator.GenerateExitPortalSprite()`. Usado en `CreateExitPortal()`. | Eliminado del disco por el usuario; constructores tenían referencias en código como fallback. |
| `UI/flecha_meta.png` | 64×64 px | Generado por `SpriteAssetGenerator.GenerateGoalArrow()`. Usado en `GoalCompassUI` en el HUD. | Eliminado del disco por el usuario; constructores tenían referencias en código como fallback. |
| `UI/panel_hud_card.png` | 128×128 px | Generado por `SpriteAssetGenerator.GenerateHudCard()`. Usado como tarjeta de fondo en `UIBuilder.cs`. | Existe en disco (`Assets/Sprites/UI/panel_hud_card.png`). Debe moverse a `_Deprecated/` y dejar de usarse. |

> **Regla de la Sección 3:** Los sprites provisionales deben ser trasladados a `Assets/_Deprecated/` conservando sus archivos `.meta` para evitar referencias rotas, y eliminar toda referencia a ellos en código y escenas.

---

## 2. Prefabs y Escenas que los Referencian

* **Prefabs:** No existen prefabs en `Assets/Prefabs/` (solo `.gitkeep`); todos los objetos del juego se instancian y configuran proceduralmente mediante los scripts de Editor (`Level1Builder`, `Level2Builder`, `Level3Builder`, `Level4Builder`, `UIBuilder`).
* **Escenas (`Assets/Scenes/`):**
  * `Level1_Mercado.unity`, `Level2_Callejon.unity`, `Level3_Atrio.unity`, `Level4_Cementerio.unity`:
    * Contienen referencias en sus objetos serializados a `panel_hud_card.png` (en componentes `Image` de modales y tarjetas) y a `portal_meta.png` / `charco_lodo.png` generados en ejecuciones anteriores.
  * `MainMenu.unity`, `LevelSelect.unity`, `Splash.unity`, `Credits.unity`:
    * Referencian `panel_hud_card.png` en los paneles de menú y tarjetas de selección.

---

## 3. Diagnóstico del Anclaje del HUD Hoy

1. **Jerarquía y Contenedor:**
   * El HUD se crea dentro de `UICanvas` (`CanvasScaler` a 1920×1080, Match 0.5).
   * Anteriormente, el GameObject intermedio `HUD` tenía un `RectTransform` por defecto de 100×100 px centrado en `(0.5, 0.5)`. Esto causaba que todos los elementos hijos (anclados en teoría a las esquinas) se dibujaran en el tercio central de la pantalla, encimándose sobre el juego.
   * Se aplicó `MakeFullStretch` a `HUD` (`anchorMin = (0,0)`, `anchorMax = (1,1)`), lo que permite anclar a las esquinas reales de la pantalla.
2. **Discrepancias con `PROMPT_VISUAL.md` (Sección 6):**
   * **Franja superior unificada (120 px de alto):**
     * Actualmente existen 3 tarjetas separadas (`Card_Jugador`, `Card_Tiempo`, `Card_Objetivo`) con `panel_hud_card.png` oscuro en lugar de la estructura oficial con placas de `interfaz.png`.
   * **Bloque Izquierdo (Top Left):**
     * Los corazones miden 44 px en lugar de los **56 px** especificados con separación de 8 px.
     * Falta la **Barra de estamina** con `Barra_marco` de 300 px de ancho e `Image Filled Horizontal` que descienda al correr y se recargue al caminar.
   * **Bloque Central (Top Center):**
     * Falta montar el tiempo y los puntos sobre una `Placa_normal` de 520×96 px con dos líneas (`TIEMPO 00:00` y `PUNTOS 0`).
   * **Bloque Derecho (Top Right):**
     * Falta montar el objetivo y distancia sobre una `Placa_normal` de 400×96 px.
     * El `Icono_ojo` (64 px) debe colocarse a la izquierda de la placa y pulsar en rojo cuando La Llorona detecte a Carlos.
   * **Tipografía:**
     * Se está usando la fuente estándar de Unity UI (`Text`). Se requiere `TextMeshPro` con una fuente pixel de licencia abierta (Pixelify Sans / Silkscreen), **sin emojis en texto** (usando los iconos de sprite).
   * **Avisos de Evento:**
     * Deben aparecer a 140 px del borde superior, centrados horizontalmente, con 2.5 s de fundido, nunca en el centro de la pantalla.

---

## 4. Conflictos y Discrepancias Detectadas

1. **Nomenclatura y Corte de Spritesheets (Sección 4 vs Estado Actual):**
   * El slicer actual utilizaba nombres genéricos (`interfaz_0..7`, `Puesto_0..1`, `Sacos_0..5`, `Botella_0..7`).
   * Debe actualizarse a los nombres exactos del documento:
     * `interfaz.png`: `Corazon_lleno`, `Corazon_vacio`, `Icono_vela`, `Icono_ojo`, `Barra_marco`, `Placa_normal`, `Placa_resaltada`, `Placa_presionada`.
     * `Puestos_Nivl1.png`: `Puesto_frutas`, `Puesto_verduras`.
     * `Contenedores_Sacos_Nvl1.png`: `Huacales_apilados`, `Huacal_roto_paja`, `Canasta_vacia`, `Canasta_chiles`, `Costales_apilados`, `Costal_maiz_abierto`.
     * `Botellas_charcolodo_Nvl1.png`: `Charco_0` a `Charco_5`, `Botella_verde`, `Botella_ambar`, `Botella_transparente_acostada`, `Botella_ambar_etiqueta_acostada`, `Botella_rota`, `Vidrios_rotos`, `Botellas_caidas`, `Botella_azul`.
     * `Perro/perro.png`: `Perro_frente`, `Perro_espalda`, `Perro_lado_izq`, `Perro_lado_der`.
2. **Perro Callejero (Sección 8):**
   * `perro.png` es una hoja de 4 vistas de pie. No tiene animaciones de echado ni ladrando.
   * El código actual intentaba buscar sprites de perro echado/ladrando inexistentes. Debe implementar la solución oficial descrita en la Sección 8:
     * *Dormido:* Vista lateral con escala `(1, 0.7)`, oscurecido, respiración leve oscilante en Y y texto flotante `Z`.
     * *Ladrando:* Vista lateral con sacudida horizontal, icono `!` flotante y sonido.
     * *Orientación:* Mirar hacia Carlos según su posición.
3. **PPU y Escalas Objetivo (Sección 5.4):**
   * Carlos mide ≈ 1.2 unidades de alto.
   * Los PPU actuales no estaban normalizados para que un puesto mida exactamente 5 u de ancho, los huacales 1.8 a 2 u, los costales 1.6 u, los charcos 1.4 a 3.8 u, las botellas 0.55 u y las salidas 7 a 8 u.
4. **Salidas como Escenas de Cierre (Sección 9):**
   * Cada nivel debe tener su escena de cierre completa de `Salidas.png` (7 a 8 u de ancho) con pilares sólidos y hueco central trigger, con Point Light cálida interior y sin ninguna flecha.
5. **Limpieza de Halos Verdes (Chroma despill - Sección 5.3):**
   * Varias hojas conservan restos de fondo verde (#00FF00). Se requiere la herramienta de limpieza para neutralizar el borde verde y crear respaldos en `Assets/Sprites/_originals/`.

---

## 5. Propuesta de Plan Detallado para la Fase 1

La **Fase 1** se enfoca exclusivamente en la **preparación y normalización de sprites** sin tocar aún la reconstrucción de niveles ni la interfaz:

### Paso 1.1: Carpeta de Deprecados y Resguardo
* Crear `Assets/_Deprecated/`.
* Mover los sprites provisionales (`panel_hud_card.png`, etc.) a `Assets/_Deprecated/` preservando sus archivos `.meta`.
* Crear `Assets/Sprites/_originals/` y guardar copia intacta de todas las hojas de arte originales antes de cualquier limpieza.

### Paso 1.2: Limpieza de Verde (Chroma Despill)
* Crear `Tools/clean_green.py` para procesar los PNGs de `Assets/Sprites/Environment/` (`Puestos_Nivl1.png`, etc.), eliminando píxeles cercanos a `#00FF00` y neutralizando el halo verde en los bordes.
* Proporcionar tanto el script en Python como la alternativa automatizada en C# / .NET para que se ejecute directamente sin requerir configuraciones de entorno externas si no se tiene Pillow.

### Paso 1.3: Slicer Oficial por Componentes Conectados (`SpriteSheetSlicer.cs`)
* Implementar corte por componentes conectados del canal alfa (con dilatación de 2-3 px para unir partes contiguas y descarte de componentes < 400 px²).
* Nombrar con absoluta precisión cada sprite según las tablas de la Sección 4:
  * `interfaz.png`: 8 piezas oficiales (`Corazon_lleno`, `Corazon_vacio`, `Icono_vela`, `Icono_ojo`, `Barra_marco`, `Placa_normal`, `Placa_resaltada`, `Placa_presionada`).
  * `Puestos_Nivl1.png`: 2 puestos (`Puesto_frutas`, `Puesto_verduras`).
  * `Contenedores_Sacos_Nvl1.png`: 6 piezas (`Huacales_apilados`, `Huacal_roto_paja`, `Canasta_vacia`, `Canasta_chiles`, `Costales_apilados`, `Costal_maiz_abierto`).
  * `Botellas_charcolodo_Nvl1.png`: 6 charcos (`Charco_0` a `Charco_5`) y 8 botellas (`Botella_verde`, `Botella_ambar`, `Botella_transparente_acostada`, `Botella_ambar_etiqueta_acostada`, `Botella_rota`, `Vidrios_rotos`, `Botellas_caidas`, `Botella_azul`).
  * `Perro/perro.png`: 4 vistas (`Perro_frente`, `Perro_espalda`, `Perro_lado_izq`, `Perro_lado_der`).
  * `Salidas.png`: 4 salidas (`Salida_0`, `Salida_1`, `Salida_2`, `Salida_3`).
  * `tiles_nvl2.png`: 7 tiles utilizables únicamente.
  * `Puertas_nvl2.png` y `accesorios_nvl2.png`.
* Configurar en cada textura: `FilterMode.Point`, `TextureImporterCompression.Uncompressed` y pivotes adecuados (`Bottom Center` para elementos verticales, `Center` para charcos/botellas/tiles).

### Paso 1.4: Cálculo de PPU para Escala Objetivo
* Calcular el Pixels Per Unit (PPU) exacto de cada hoja para que coincida con las dimensiones objetivo relativas a Carlos (1.2 u de alto):
  * Puestos: 5 u de ancho.
  * Huacales/Costales: 1.6 a 2.0 u de ancho.
  * Salidas: 7 a 8 u de ancho.
  * Perro: 1.1 u de largo.
* Crear documento `Docs/Assets_pendientes.md` para registrar formalmente los assets faltantes (sprites de perro durmiendo/ladrando dedicados, candado de level select, etc.) con sus prompts recomendados.

---

> **Estado:** Diagnóstico completado. Detenido a la espera de tu autorización para comenzar con la Fase 1.
