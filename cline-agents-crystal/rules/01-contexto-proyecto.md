# Contexto del proyecto (migración VB6 → .NET con Crystal Reports)

<!-- Copia este archivo a .clinerules/01-contexto-proyecto.md en tu proyecto -->

## Historia y estado
- El sistema fue migrado desde Visual Basic 6.0 a .NET.
- Los reportes son archivos Crystal Reports (`.rpt`) creados en la época de VB6.
- Los `.rpt` apuntan a conexiones **ODBC (DSN)** heredadas. Esto es un problema porque:
  - El DSN debe existir en cada máquina cliente/servidor.
  - Las credenciales y el servidor quedan "quemados" en el binario del reporte.
- Objetivo: centralizar la conexión en configuración .NET (`appsettings.json` / `web.config` / `app.config`)
  y aplicarla en runtime con `ConnectionInfo`, sin depender de DSN de máquina.

## Restricciones técnicas que TODO agente debe respetar
1. Los archivos `.rpt` son **binarios**: NUNCA intentes editarlos como texto ni con search/replace.
2. Toda modificación de estructura de un `.rpt` se hace:
   - En el Crystal Reports Designer (manual), o
   - Programáticamente vía SDK RAS in-proc (`ReportDocument.ReportClientDocument`) con la herramienta de `tools/`.
3. Después de cambiar la consulta/SP que alimenta un reporte, SIEMPRE ejecutar `VerifyDatabase()` para que el `.rpt` reconozca campos nuevos.
4. Antes de modificar un `.rpt` programáticamente, crear copia de respaldo con sufijo `.bak-YYYYMMDD`.
5. El runtime de Crystal Reports (CR for Visual Studio, v13.x) solo funciona en **Windows**. Los scripts de `tools/` deben ejecutarse allí.
6. Framework Crystal usado: `CrystalDecisions.CrystalReports.Engine` y `CrystalDecisions.Shared`.

## Convenciones del código migrado
- Toda apertura de reporte debe pasar por un helper central `CrystalReportHelper` (si no existe, créalo).
- Prohibido dejar cadenas de conexión ODBC (`DSN=...`) en código nuevo; usar la conexión central de la app.
- Los parámetros del reporte se asignan con `SetParameterValue` validando nombre y tipo.
- Cada `ReportDocument` debe disponerse con `Close()` + `Dispose()` (patrón `using` cuando sea posible) para no agotar el límite de licencias del runtime (75 procesos de reporte por defecto).
