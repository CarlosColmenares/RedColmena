# Reglas para trabajar con reportes Crystal (.rpt)

<!-- Copia este archivo a .clinerules/02-manejo-reportes.md en tu proyecto -->

## Inventario obligatorio antes de tocar un reporte
Antes de modificar cualquier `.rpt` o el código que lo usa, el agente debe:
1. Localizar el `.rpt` y listar todos los archivos de código que lo referencian (buscar por nombre de archivo sin extensión).
2. Identificar: DSN/ODBC que usa, tablas o SPs de origen, parámetros (`ParameterFields`), fórmulas (`FormulaFields`) y subreportes.
3. Documentar lo anterior en `docs/reportes/<NombreReporte>.md` (crear si no existe).

## Patrón de conexión en runtime (sustituye al DSN)
Todo reporte debe abrirse así:

```csharp
var report = new ReportDocument();
report.Load(rutaRpt, OpenReportMethod.OpenReportByTempCopy);
CrystalReportHelper.AplicarConexion(report); // aplica ConnectionInfo a TODAS las tablas y subreportes
report.SetParameterValue("NombreParam", valor);
```

`CrystalReportHelper.AplicarConexion` debe recorrer `report.Database.Tables` y los subreportes
(`report.Subreports`) aplicando `TableLogOnInfo` con el proveedor destino
(p. ej. OLE DB `SQLOLEDB`/`MSOLEDBSQL` o el que use la app migrada).

## Agregar campos a un reporte — procedimiento canónico
1. Modificar la fuente de datos (SP o consulta) para incluir el/los campos nuevos.
2. Ejecutar `VerifyDatabase()` vía la herramienta `tools/VerifyAllReports.cs` para que el `.rpt` registre el campo.
3. Insertar el campo en el lienzo:
   - **Opción A (recomendada al inicio):** el agente prepara todo y deja instrucciones exactas para insertarlo a mano en el Designer (sección, posición, formato).
   - **Opción B (avanzada):** usar `tools/AddFieldToReport.cs` (RAS in-proc) para insertar el `FieldObject` y su encabezado `TextObject` automáticamente.
4. Generar/actualizar la doc del reporte en `docs/reportes/`.
5. Compilar la solución y ejecutar la verificación de carga de todos los `.rpt`.

## Qué NO hacer
- No cambiar la ubicación de los `.rpt` sin actualizar todas las referencias y el .csproj (Copy to Output Directory).
- No usar `report.SetDataSource()` con un DataTable si el reporte original usa conexión directa a BD salvo migración explícita aprobada (cambia el modelo de datos del reporte).
- No tocar fórmulas en sintaxis "Basic" sin revisión humana (heredadas de VB6, sensibles).
