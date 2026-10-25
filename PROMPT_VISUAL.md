# Prompt de corrección visual y jugabilidad para Antigravity
**La Llorona del Callejón · Unity 6.6 · Carol y Princes · UTNG**

## Cómo usarlo
1. Guarda este documento como `PROMPT_VISUAL.md` en la raíz del repositorio (junto a `PROMPT.md` si ya lo tienes) o pégalo completo en Antigravity.
2. Primer mensaje: «Lee PROMPT_VISUAL.md completo, haz el diagnóstico de la sección 12 y espera mi confirmación antes de modificar nada».
3. Autoriza una fase a la vez y prueba en Unity (Game view a 1920×1080) antes de continuar.

---

## 1. Objetivo
El juego ya funciona (mecánicas en la sección 2). Lo que falta es que **se vea bien y tenga coherencia**: usar el arte real que diseñé en lugar de los sprites provisionales, reconstruir los escenarios como lugares creíbles, mover toda la interfaz a una franja superior con el centro de la pantalla libre, y darle al perro y a las salidas el aspecto que les corresponde. **No se debe romper nada de lo que ya funciona.**

---

## 2. Lo que ya funciona (no romper)
- **Carlos:** movimiento con WASD o flechas, caminar a 3.5 y correr con Shift a 6.0 consumiendo estamina, rotación Z congelada, `flipX` para mirar a los lados, 3 vidas con 1.5 s de invulnerabilidad parpadeante, escondite con `[E]` (invisible y silencioso) y colocación de velas con `[F]` que detienen a la Llorona 3 s.
- **La Llorona:** máquina de estados Patrulla, Búsqueda (corre de Carlos, botellas rotas, ladridos del perro), Persecución (cono de luz roja) y Distraída (reza 3 s ante una vela). Latidos de corazón que suben de volumen según la cercanía. Rotación bloqueada.
- **Perro territorial:** duerme en una esquina; si Carlos corre cerca, despierta, ladra y atrae a la Llorona.
- **Charcos:** reducen la velocidad de Carlos al 50 % durante 2.5 s. **Botellas:** hacen ruido de vidrios y alertan a la Llorona.
- **Escenas:** `MainMenu`, `LevelSelect` (4 niveles, candados y progreso en `PlayerPrefs`), `Level1_Mercado`, `Level2_Callejon`, `Level3_Atrio`, `Level4_Cementerio`, y modales de Pausa (`Esc`), Nivel Completado y Game Over.
- **Reglas del Nivel 1 que se conservan:** 7 puestos grandes, 5 escondites funcionales, laberinto de cajas y sacos, 6 charcos, botellas, perro durmiendo y **cero bancas**.

---

## 3. Problemas detectados y qué hacer
Revisé la carpeta `Assets/Sprites`. El juego está usando sprites provisionales dibujados por código (cuadros de pocos píxeles) en lugar del arte real que sí existe en las hojas grandes.

| # | Problema | Causa | Solución |
|---|---|---|---|
| 1 | El perro no es el que quiero | Se usa `Environment/perro_callejero.png` (96×96, provisional) | Usar `Sprites/Perro/perro.png` (sección 8) |
| 2 | Los charcos se ven como una mancha plana | Se usa `Environment/charco_lodo.png` (128×64, provisional) | Usar los 6 charcos de `Botellas_charcolodo_Nvl1.png` |
| 3 | Las botellas son 4 rectángulos | Se usa `Environment/basura_ruido.png` (96×64, provisional) | Usar las 8 botellas de `Botellas_charcolodo_Nvl1.png` |
| 4 | La salida es solo una puerta verde | Se usa `Environment/portal_meta.png` y `UI/flecha_meta.png` (provisionales) | Usar `Salidas.png` como escenas de cierre (sección 9) |
| 5 | El HUD es un rectángulo oscuro con borde dorado | Se usa `UI/panel_hud_card.png` (provisional) | Usar `UI/interfaz.png` (sección 6) |
| 6 | Vidas, tiempo, puntos y mensajes están en el centro | Anclajes mal puestos | Franja superior, centro libre (sección 6) |
| 7 | Los escenarios no tienen coherencia | Piezas colocadas sin composición | Reconstruir según la sección 7 |
| 8 | Halos y restos de verde en varios sprites | Fondo verde chroma mal recortado | Limpieza de verde (sección 5) |
| 9 | `tiles_nvl2.png` está dañada | La IA generó celdas rotas | Solo 7 tiles son utilizables (sección 4) |

> **Regla:** mueve los sprites provisionales a `Assets/_Deprecated/` conservando sus `.meta`, y elimina toda referencia a ellos en prefabs, escenas y scripts. No los borres.

---

## 4. Inventario de sprites (qué es cada archivo y dónde se usa)
Todas las hojas tienen fondo transparente. Las piezas están separadas, así que se cortan por componentes conectados (alfa), no con cuadrícula fija. Nombra cada pieza como indica la tabla.

### Personajes
- `Player/Carlos_Caminando.png`: 4 columnas × 4 filas (abajo, arriba, izquierda, derecha). Animaciones de Carlos.
- `Player/carlos_frente_espalda.png`, `Carlos.png`: Poses de reposo / Idle.
- `Llorona/llorona_movimiento.png`: 4 columnas × 5 filas (frente, lado, lado, espalda, persecución). Animaciones de la Llorona.
- `Perro/perro.png` (2400×1340): Hoja de referencia con 4 vistas de un perro callejero café oscuro, flaco, ojos ámbar: `Perro_frente`, `Perro_espalda`, `Perro_lado_izq` (arriba a la derecha), `Perro_lado_der` (abajo a la derecha).

### Nivel 1: Mercado
- `Environment/tiles.png` (1152×1536): Hoja uniforme 6×8 de tiles de 192 px: filas 0-1 adoquín (la 1 con charcos), 2-3 tierra, 4-5 muro de concreto, 6-7 fachadas con ventanas y faroles. Suelo y bordes del Nivel 1.
- `Environment/Puestos_Nivl1.png`: `Puesto_frutas`, `Puesto_verduras` (lona azul rota, mercancía). Los 7 puestos, alternando los dos.
- `Environment/Contenedores_Sacos_Nvl1.png`:
  - Fila 1: `Huacales_apilados`, `Huacal_roto_paja`, `Canasta_vacia`.
  - Fila 2: `Canasta_chiles`, `Costales_apilados`, `Costal_maiz_abierto`.
  - Laberinto de cajas y sacos, y escondites.
- `Environment/Botellas_charcolodo_Nvl1.png`:
  - Filas 1-2: `Charco_0` a `Charco_5` (3 pequeños-medianos-grandes por fila).
  - Fila 3: `Botella_verde`, `Botella_ambar`, `Botella_transparente_acostada`, `Botella_ambar_etiqueta_acostada`.
  - Fila 4: `Botella_rota`, `Vidrios_rotos`, `Botellas_caidas`, `Botella_azul`.
- `Environment/escondites.png`: `Escondite_cajas` (cajas con lona azul), `Escondite_barril`, `Escondite_puerta`.
- `Environment/decoracion.png`: `Farol`, `Bote`, `Costales`, `Banca`, `Cartel_feria`, `Papel_picado`. Decoración (no usar `Banca` en el Nivel 1).
- `Environment/puesto_mercado.png`, `vela.png`: Puesto con lona azul y huacales; vela encendida.

### Nivel 2: Callejón
- `Environment/tiles_nvl2.png`: Solo 7 utilizables: fila 1: `Fachada_balcon_roja_0`, `ocre_1`, `azul_2`, `roja_3`, `ocre_4`, `morada_5`; fila 2, columna 1: `Fachada_ventana_roja_6`. El resto es basura semitransparente con líneas verdes: ignorarla.
- `Environment/tiles_adoquin_nvl2.png`: 6×2: fila 1 adoquín seco con musgo, fila 2 adoquín con luz de farol y charcos. Suelo del callejón.
- `Environment/Puertas_nvl2.png`: `Puerta_0`, `Puerta_1`, `Puerta_2` (puertas coloniales entreabiertas con marco de cantera). Escondites del Nivel 2.
- `Environment/accesorios_nvl2.png`: `Maceta_colgante`, `Maceta_suelo_seca`, `Balcon_hierro`, `Farol_pared`.

### Salidas (una por nivel)
`Environment/Salidas.png` contiene cuatro escenas de cierre:
- `Salida_0` (arriba a la izquierda): Arco de cantera entre dos muros con teja, farol y un callejón empedrado iluminado al fondo, un puesto con toldo azul a un lado -> **Nivel 1: del mercado al callejón**.
- `Salida_1` (arriba a la derecha): Pasaje angosto entre casas que desemboca en una plaza con fuente iluminada -> **Nivel 2: del callejón al atrio**.
- `Salida_2` (abajo a la izquierda): Portón de hierro forjado entre pilares de cantera rosa con cruces y camino de tierra hacia un cementerio con niebla -> **Nivel 3: del atrio al panteón**.
- `Salida_3` (abajo a la derecha): Casa humilde de adobe con teja, ventana enrejada, puerta de madera entreabierta con luz cálida, maceta con flores y banca -> **Nivel 4: meta final, la casa de Carlos**.

### Interfaz
- `UI/interfaz.png` (2400×1368): `Corazon_lleno`, `Corazon_vacio`, `Icono_vela`, `Icono_ojo` (ojo rojo), `Barra_marco` (barra larga con marco), `Placa_normal`, `Placa_resaltada` (borde naranja), `Placa_presionada`.
- `UI/logo.png` (1672×941, sin transparencia): Logotipo del juego.

---

## 5. Preparación de los sprites (Fase 1)
1. **No generes arte por código.** Ningún PNG se crea con un script. Todo sale de estas hojas. Si falta una pieza, anótala en `Docs/Assets_pendientes.md` y coloca en su lugar un objeto vacío llamado `PENDIENTE_nombre` con un Gizmo visible; no la inventes.
2. **Script de Editor `SpriteSheetSlicer` (`Assets/Editor/`)** con menú `La Llorona/Cortar hojas de sprites`: corta cada hoja por componentes conectados del canal alfa (con una pequeña dilatación para unir piezas fragmentadas), ordena las piezas de arriba abajo y de izquierda a derecha, las nombra según la sección 4, y fija Point (no filter), Compression None y el pivote que corresponda (Bottom Center para objetos de pie, Center para charcos, botellas y tiles). Usa `ISpriteEditorDataProvider`. Ignora componentes menores a 400 px² para descartar basura.
3. **Limpieza de verde (halos):** varias hojas (por ejemplo `Puestos_Nivl1.png`) conservan un borde verde y verde visible entre el toldo y la mercancía. Crea `Tools/clean_green.py` (Python con Pillow) que, para los PNG de `Sprites/Environment`, vuelva transparentes los píxeles cercanos a `#00FF00` y neutralice el derrame de verde en los bordes (despill). Guarda antes una copia de los originales en `Sprites/_originals/`.
4. **Escala objetivo** (Carlos mide ≈ 1.2 unidades de alto; calcula un PPU por sprite = ancho en píxeles ÷ ancho objetivo):
   - `Puesto_*`: 5 u de ancho
   - `Huacales_apilados`, `Huacal_roto_paja`: 1.8 a 2 u de ancho
   - `Canasta_*`: 1.2 u de ancho
   - `Costales_apilados`, `Costal_maiz_abierto`: 1.6 u de ancho
   - `Escondite_cajas`, `Escondite_barril`, `Escondite_puerta`: 2.6, 2.4 y 2.8 u de ancho
   - `Charco pequeño, mediano, grande`: 1.4, 2.6 y 3.8 u de ancho
   - `Botella_* de pie`: 0.55 u de alto
   - `Botella_* acostada`, `Botellas_caidas`: 0.6 a 0.8 u de largo
   - `Vidrios_rotos`: 0.7 u de ancho
   - `Salida_0`, `Salida_2`: 8 u de ancho
   - `Salida_1`: 7 u de ancho
   - `Salida_3`: 7 u de ancho
   - `Perro_lado_*`: 1.1 u de largo
   - `Puerta_* (N2)`: 2.8 u de ancho (≈ 2 veces la altura de Carlos)
   - `Maceta_*`, `Farol_pared`: ≈ 0.9 a 1 u de alto
   - `Balcon_hierro`: 2 u de ancho
   - `Fachada_* (N2)`: 3 u de ancho

---

## 6. Interfaz (HUD y menús)
Usa únicamente las piezas de `interfaz.png`. Elimina `panel_hud_card.png` y `flecha_meta.png` del juego.

### Reglas generales
- Canvas Screen Space - Overlay, Canvas Scaler Scale With Screen Size 1920×1080, Match 0.5, y respeto de `Screen.safeArea` para móvil.
- **El centro de la pantalla debe quedar 100 % libre.** Nada de HUD, avisos ni mensajes en el tercio central vertical de la pantalla.
- Márgenes de 24 px. Todos los elementos anclados a su esquina, nunca al centro.
- Fuente TextMeshPro de estilo pixel con licencia abierta (por ejemplo Pixelify Sans o Silkscreen, licencia OFL). Cítala en `Docs/Creditos.md`. No uses emojis en el texto: usa iconos de sprite.

### HUD: franja superior (alto ≈ 120 px)
- **Izquierda (Top Left):** Tres corazones de 56 px (`Corazon_lleno` o `Corazon_vacio`) con separación de 8 px. Debajo: `Icono_vela` de 40 px con el número de velas (`x2`). Debajo: barra de estamina con `Barra_marco` de 300 px de ancho y relleno (Image Filled Horizontal) que baja al correr y se recarga al caminar.
- **Centro (Top Center):** Una `Placa_normal` de 520×96 px con dos líneas: `TIEMPO 00:00` y `PUNTOS 0`.
- **Derecha (Top Right):** Una `Placa_normal` de 400×96 px con `OBJETIVO: Llega a la salida` y `META: 42 m`. `Icono_ojo` de 64 px a la izquierda: se enciende y pulsa en rojo cuando la Llorona detecta a Carlos.

### Mensajes
- **Tutorial:** una placa flotante inferior (Bottom Center, a 60 px del borde) con fundido de entrada y salida, visible unos 6 s y solo la primera vez.
- **Avisos de evento** («Pisaste lodo», «¡Cristales! La Llorona te oyó», «El perro despertó»): una línea, debajo de la franja superior (a 140 px del borde de arriba), alineada al centro horizontalmente, 2.5 s con fundido. Nunca en el centro de la pantalla.

### Animación del HUD
- El corazón perdido tiembla y pasa a `Corazon_vacio`.
- Los puntos hacen un pequeño «pop» al subir.
- La barra de estamina parpadea cuando llega a cero.

### Menú principal (`MainMenu`)
- Fondo: una escena decorativa estática armada con los mismos sprites (callejón oscuro con adoquín, fachadas, faroles encendidos y la silueta de la Llorona al fondo), con viñeta y partículas suaves de niebla. Sin colisiones ni lógica.
- Título: `logo.png` (no texto plano), arriba y centrado.
- Botones: JUGAR, SELECCIONAR NIVEL, OPCIONES, CRÉDITOS y SALIR (oculta SALIR en WebGL), con `Placa_normal`, `Placa_resaltada` (hover) y `Placa_presionada` (clic), texto centrado, escala 1.05 al pasar el cursor y sonido de clic. Proporciones correctas, sin estirar los sprites (usa 9-slice o Preserve Aspect).

### Selección de nivel (`LevelSelect`)
- Cuatro tarjetas, cada una con una `Placa_normal`, el nombre del nivel y una miniatura tomada de `Salida_0` a `Salida_3`. Los niveles bloqueados llevan la miniatura en gris con la etiqueta BLOQUEADO (si no existe un sprite de candado, anótalo en `Assets_pendientes.md`).

### Modales
Pausa, Nivel Completado y Game Over: centrados, con una capa negra al 70 % detrás y las placas de `interfaz.png`. Son los únicos elementos que pueden ocupar el centro. Sin flechas debajo de los botones.

---

## 7. Escenarios
### Principios para todos los niveles
1. Arma cada nivel como un lugar real, con un camino legible de la entrada a la salida, zonas con identidad y bordes cerrados con fachadas y muros. Nada flotando, nada duplicado encima de otra pieza.
2. Capas y orden de dibujo: `Ground` (tilemap de suelo), `Decals` (charcos, botellas y manchas, justo encima del suelo, sin colisión), `Walls` (fachadas y muros con colisión), `Characters` (personajes y todos los objetos altos, con Sprite Sort Point = Pivot y Transparency Sort Mode = Custom Axis (0,1,0) para ordenar por altura), `Foreground` (papel picado y copas). UI aparte.
3. Colisiones: puestos solo en la base; contenedores en su base; charcos y botellas como triggers; escondites con trigger que sobresalga 0.6 u por el frente y un HidePoint detrás.
4. Un escondite es un clúster, no una pieza suelta: agrupa 3 o 4 piezas (huacales, costales, canasta) hasta lograr al menos 2.4 u de ancho y 1.8 de alto, para que Carlos quepa detrás.
5. Pasillos libres de al menos 2.5 u (contando los colliders). Verifica que exista una ruta de la entrada a la salida y que ningún escondite quede a más de 10 u del siguiente.
6. Iluminación: Global Light 2D por nivel (N1 0.55, N2 0.46, N3 0.50, N4 0.42) y una Point Light 2D cálida en cada farol y en la salida. El cono rojo de la Llorona se mantiene.
7. Cámara: Orthographic Size 5 con Cinemachine y Confiner 2D del tamaño del mapa.
8. Cada nivel se construye con un script de Editor (`LevelNBuilder`) con menú `La Llorona/Construir Nivel N`, que primero borra el nivel generado anterior para poder reconstruirlo.

### Nivel 1: El Mercado Vacío (56 × 20 unidades)
- Zona jugable de y = 2 a y = 18; fachadas (`tiles` filas 6-7) en y = 0-1 y y = 18-19, y muros de concreto en x = -1 y x = 56.
- **Zona 1 (Entrada, x = 0 a 8):** Suelo de tierra. Inicio de Carlos en (3, 10). `Cartel_feria` (4, 14), `Papel_picado` en Foreground (6, 17), faroles en (2, 15) y (2, 5), un charco pequeño (5, 6).
- **Zona 2 (Calle de puestos, x = 8 a 40):** Adoquín. Hilera superior de 3 puestos en x = 13, 22 y 31 (base y ≈ 12); hilera inferior de 4 puestos en x = 11, 19, 27 y 35 (base y ≈ 4). Alterna `Puesto_frutas` y `Puesto_verduras`. Entre ellos, barricadas de huacales y costales forman un laberinto suave con dos rutas. Faroles en (16, 17) y (30, 17). Papel picado entre puestos.
- **Zona 3 (Rincón del perro, x = 40 a 48):** Tierra. El perro durmiendo en la esquina inferior (45, 3.8). Dos charcos grandes y montones de botellas junto a él: es la zona de riesgo de ruido. Una ruta alterna por arriba, más larga y sin perro.
- **Zona 4 (Salida, x = 48 a 56):** `Salida_0` como escena de cierre, centrada verticalmente en el pasillo. El camino de adoquín llega hasta ella sin cortes.
- **Puestos:** 7 en total, 3 y 4 por hilera.
- **Escondites (5):** `Escondite_cajas` cerca de la entrada (9.5, 14.5), un clúster de huacales y costales (17.5, 8.5), `Escondite_barril` (26.5, 12.8), un clúster de costales (35.5, 9), y `Escondite_cajas` junto a la zona del perro (42, 6).
- **Charcos (6):** `Charco_0` a `Charco_5`, repartidos: uno en la entrada, dos en pasillos, uno junto a un puesto, dos en el rincón del perro.
- **Botellas:** 5 o 6 montones usando las 8 piezas, cerca de las rutas (por ejemplo en (19, 10), (28, 9.5), (33, 15), (38, 6), (43, 5) y (47, 4.5)).
- **Decoración:** 6 faroles con luz, 2 botes de basura, costales sueltos, papel picado. **Sin bancas.**
- **Suelo:** adoquín (`tiles` 0-11) en la calle; tierra (12-23) en la entrada y el rincón del perro; los charcos y botellas van como decals encima.
- **La Llorona:** patrulla en vertical en x ≈ 33 entre y = 4 y y = 16, con paradas.
- **Efectos:** al pisar un charco, 50 % de velocidad durante 2.5 s con salpicadura; al pisar botellas, sonido de vidrio, cambio a `Vidrios_rotos` y alerta a la Llorona.

### Nivel 2: El Callejón del Beso
- Callejón de 5 a 6 u de ancho y unas 60 u de largo con dos giros a 90°: tramo horizontal de 20 u, giro hacia arriba de 14 u, giro a la derecha de 20 u y la salida.
- Fachadas continuas a ambos lados con `Fachada_*` (alternando colores y la de ventana), `Balcon_hierro` sobre ellas, `Maceta_colgante` y `Maceta_suelo_seca` junto a los muros, y `Farol_pared` que parpadea (`LightFlicker`).
- Suelo con `tiles_adoquin_nvl2` (fila 1 en tramos oscuros, fila 2 bajo los faroles).
- 3 o 4 escondites: `Puerta_0`, `Puerta_1`, `Puerta_2` y un barril.
- Salida: `Salida_1` (pasaje a la plaza con fuente).

### Nivel 3: El Atrio de la Parroquia y Nivel 4: El Panteón de las Ánimas
- Marcar los huecos de los sprites que faltan con objetos `PENDIENTE_*` y registrarlos en `Docs/Assets_pendientes.md`.
- Nivel 3: explanada amplia, la Llorona rodea la fuente en círculo, velas coleccionables. Salida: `Salida_2` (portón al panteón).
- Nivel 4: niebla densa con partículas, la Llorona más rápida, grito final. Salida: `Salida_3` (la casa de Carlos).

---

## 8. Perro callejero
- `Sprites/Perro/perro.png` es una hoja de referencia con 4 vistas de pie. No contiene al perro echado ni ladrando.
- Corta las 4 vistas con `SpriteSheetSlicer` y úsalas como base. Pivote Bottom Center, 1.1 u de largo en la vista lateral.
- Mientras no existan los sprites definitivos (no los inventes por código):
  - **Dormido:** vista lateral con escala (1, 0.7), tinte más oscuro, respiración leve (escala Y oscilando) y un texto flotante `Z` pequeño que sube.
  - **Ladrando:** vista lateral con sacudida horizontal rápida, icono `!` sobre la cabeza y sonido.
  - **Orientación según Carlos:** vista izquierda o derecha, y frente o espalda cuando se mueva en vertical.
  - Pendientes: anótalos en `Assets_pendientes.md`.
- **Comportamiento:**
  - *Dormido:* Estado inicial, en una esquina del mercado.
  - *Alerta:* Carlos corre a menos de 4 u, o pasa a menos de 2.5 u a velocidad mayor que la de caminar.
  - *Ladrando:* Dura 4 s y emite un evento de ruido que envía a la Llorona a ese punto exacto.
  - *Dormido otra vez:* Tras 8 s sin ruido cerca.
  - El perro no persigue a Carlos: su amenaza es delatarlo. Esconderse con `[E]` lo despista.

---

## 9. Salidas como escenas de cierre
- Cada nivel termina con su pieza de `Salidas.png` colocada como escena grande (7 a 8 u de ancho) en el extremo del mapa.
- Pivote en Bottom Center, centrada verticalmente en el pasillo para que la abertura quede alineada con el camino. El suelo del nivel conecta con el suelo del sprite sin cortes visibles.
- Colisiones: los muros y pilares laterales son sólidos; el hueco central es un trigger que dispara `LevelGoal`.
- Luz: Point Light 2D cálida en el interior y pulso suave. Sin flechas.
- Transición: al cruzar, fundido a negro y pantalla de Nivel Completado. En Nivel 4, `Salida_3` activa la cinemática de cierre.

---

## 10. Reglas técnicas
1. No edites a mano `.unity`, `.prefab`, `.controller`, `.anim`, `.asset` ni `.meta`. Usa scripts de Editor.
2. APIs de Unity 6: `Rigidbody2D.linearVelocity`, `FindFirstObjectByType`, `Collider2D.compositeOperation`, `Light2D` en `UnityEngine.Rendering.Universal`. Sin APIs obsoletas ni warnings.
3. Código en C# con PascalCase, `[SerializeField]` en campos privados, comentarios en español.
4. Estructura de carpetas en Assets: `Sprites` (Player, Llorona, Perro, Environment, UI), `Animations`, `Prefabs`, `Scripts`, `Audio`, `Tiles`, `Scenes`, `Editor`, `_Deprecated`.
5. Git: una rama por funcionalidad (`feature/...`) y commits pequeños en español. No hagas push ni merge.
6. Cita recursos externos en `Docs/Creditos.md`.
7. Si algo es ambiguo, haz como máximo 3 preguntas y sigue con la mejor suposición anotada en `Docs/Cambios_justificados.md`.

---

## 11. Fases (una a la vez, esperando confirmación)
1. **Fase 1: sprites.** Mover provisionales a `_Deprecated`, `SpriteSheetSlicer`, limpieza de verde, escalas.
2. **Fase 2: interfaz.** HUD en franja superior, mensajes, menú principal, selección de nivel y modales.
3. **Fase 3: Nivel 1 reconstruido** con salida, charcos, botellas y perro.
4. **Fase 4: Nivel 2.**
5. **Fase 5: Niveles 3 y 4** con los huecos marcados.
6. **Fase 6: pulido de efectos** (partículas, luces, vignette al ser detectado) y revisión final.

---

## 12. Tu primera tarea ahora
1. Lee el proyecto completo, los scripts existentes y este documento.
2. Genera `Docs/Diagnostico_visual.md` con: qué sprites provisionales se usan y dónde, qué prefabs y escenas los referencian, cómo está anclado hoy el HUD, y cualquier conflicto con lo descrito aquí.
3. Propón el plan detallado de la Fase 1.
4. **Detente y espera mi confirmación. No modifiques nada más hasta que la dé.**
