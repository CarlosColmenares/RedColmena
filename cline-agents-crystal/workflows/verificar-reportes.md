# Workflow: Verificación de salud de todos los reportes

<!-- Copia a .cline/workflows/verificar-reportes.md — se invoca con /verificar-reportes.md -->
<!-- Rol: "Agente de Calidad de Reportes" -->

Actúa como un **QA de reportes Crystal**. Objetivo: detectar reportes rotos ANTES de que lleguen a producción.

## Pasos
1. Ejecuta en Windows (con runtime CR) la herramienta `tools/VerifyAllReports.cs` sobre la carpeta de `.rpt`.
   Debe intentar por cada reporte:
   - `Load()` con `OpenReportMethod.OpenReportByTempCopy`.
   - Aplicar `CrystalReportHelper.AplicarConexion` (o ConnectionInfo de prueba).
   - `VerifyDatabase()`.
   - Recorrer `ParameterFields` y listar los requeridos.
   - Cerrar y disponer correctamente.
2. Clasifica resultados:
   - **OK**: carga y verifica sin errores.
   - **DATOS**: falla `VerifyDatabase` (estructura de BD distinta al diseño del reporte).
   - **CONEXIÓN**: no logra conectar (DSN inexistente, credenciales, servidor).
   - **RUNTIME**: excepción del motor (archivo corrupto, versión incompatible).
3. Escribe el resultado en `docs/reportes-salud.md` con fecha, y compara contra la ejecución anterior si existe (mejoras/empeoramientos).
4. Para cada reporte con falla, abre una sección con la excepción completa y la hipótesis de causa.

## Restricciones
- No modifiques `.rpt` en este workflow; es solo diagnóstico.
- Ejecuta la verificación con un grado de paralelismo bajo (2-4) para no agotar las licencias del motor CR.
