# Workflow: Agregar un campo a un reporte Crystal

<!-- Copia a .cline/workflows/agregar-campo-reporte.md — se invoca con /agregar-campo-reporte.md -->
<!-- Rol: "Agente Modificador de Reportes" -->

Actúa como un **desarrollador experto en Crystal Reports**. El usuario indicará: reporte, campo(s) nuevos y sección destino (detalle, encabezado de grupo, etc.).

## Pasos

### 1. Fuente de datos
- Localiza el SP/consulta origen del reporte (ver `docs/reportes/<Reporte>.md`; si no existe, créalo siguiendo `rules/02-manejo-reportes.md`).
- Agrega el/los campo(s) al SELECT del SP o consulta. Respeta alias y tipos.
- Si el SP vive en la BD, genera el script `ALTER PROCEDURE` en la carpeta de scripts SQL del proyecto (no lo ejecutes contra producción).

### 2. Sincronizar el .rpt con la nueva fuente
- Crea respaldo del `.rpt`: `<Nombre>.rpt.bak-YYYYMMDD`.
- Ejecuta la verificación de base de datos sobre ese reporte (herramienta `tools/VerifyAllReports.cs` con filtro por nombre, en una máquina Windows con el runtime CR instalado). Esto equivale al "Verify Database" del Designer y hace visible el campo nuevo en el Field Explorer.

### 3. Insertar el campo en el lienzo
Pregunta al usuario cuál opción prefiere:

- **Opción A — Manual asistida (segura):** genera instrucciones exactas para el Designer:
  sección destino, campo a arrastrar desde Field Explorer, texto de encabezado de columna,
  alineación, formato (moneda/fecha/decimales) y ajustes de ancho de columnas vecinas.
- **Opción B — Automática (avanzada):** ejecuta `tools/AddFieldToReport.cs` pasando:
  ruta del `.rpt`, nombre de tabla, nombre de campo, sección, posición X/Y, ancho, texto de encabezado.
  Requiere runtime CR for Visual Studio v13 en Windows. Tras ejecutar, el agente abre una
  verificación de carga para confirmar que el `.rpt` sigue siendo válido.

### 4. Código y validación
- Si el campo requiere pasar datos desde código (parámetro o fórmula), actualiza el código de carga.
- Compila la solución.
- Ejecuta la verificación de carga de TODOS los `.rpt` para detectar regresiones.
- Actualiza `docs/reportes/<Reporte>.md` con el cambio (campo, fecha, motivo).

## Restricciones
- Nunca edites el `.rpt` con herramientas de texto.
- No modifiques fórmulas en sintaxis Basic sin confirmación humana.
- Si el campo nuevo no aparece tras `VerifyDatabase`, la causa casi siempre es que el SP/consulta no se actualizó en la misma BD a la que apunta la conexión de verificación: verifícalo antes de intentar otra cosa.
