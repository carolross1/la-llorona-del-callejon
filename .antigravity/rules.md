# Reglas del Proyecto Unity - La Llorona del Callejón

1. **Aislamiento Estricto por Nivel / Escena:**
   - NUNCA modifiques múltiples escenas (`.unity`) al mismo tiempo a menos que el usuario lo solicite de forma explícita.
   - Cada escena (`Level1_Mercado`, `Level2_Callejon`, `Level3_Atrio`, `Level4_Cementerio`, `MainMenu`, etc.) debe mantener su propia integridad.
   - Si se solicita un cambio para un nivel específico, limita los cambios estrictamente a ese nivel.

2. **Cuidado con Scripts Compartidos, Prefabs y ScriptableObjects:**
   - NO alteres valores por defecto globales ni constructores en scripts de C# (`GameManager`, `HUD`, `Candle`, `LloronaAI`, etc.) o Prefabs que se compartan entre niveles si eso altera el funcionamiento de las demás escenas.
   - Cualquier parámetro específico de dificultad, velocidad, radios o mecánicas de un nivel debe configurarse en los componentes serializados de esa escena en particular o mediante variables expuestas/configurables por nivel (ej. `DifficultySettings`).

3. **Verificación Previa y Confirmación de Archivos:**
   - Antes de modificar archivos compartidos o recrear escenas, define con precisión qué archivos serán tocados y respeta las mecánicas ya aprobadas en las escenas funcionales.
   - Nunca borres ni alteres elementos funcionales de otros niveles sin confirmación previa.
