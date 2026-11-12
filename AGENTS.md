# Reglas del Proyecto Unity - La Llorona del Callejón

- **Aislamiento por Nivel:** NUNCA modifiques múltiples escenas (`.unity`) simultáneamente a menos que se solicite de forma explícita. Trabaja exclusivamente en el nivel y archivos solicitados por el usuario.
- **Integridad de Scripts y Prefabs Compartidos:** No cambies valores base o globales en scripts compartidos (`GameManager`, `HUD`, `LloronaAI`, `Candle`, etc.) ni en Prefabs base si eso puede desconfigurar o romper otros niveles. Los valores específicos deben asignarse por nivel en su respectiva escena o ScriptableObject.
- **Alcance Preciso:** Antes de aplicar cambios, delimita exactamente los archivos a modificar y mantén intactas las mecánicas y escenas que ya están funcionando.
