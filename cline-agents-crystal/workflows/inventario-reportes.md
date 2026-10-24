# Workflow: Inventario y auditoría de reportes

<!-- Copia a .cline/workflows/inventario-reportes.md — se invoca con /inventario-reportes.md -->
<!-- Rol: "Agente Auditor de Reportes" -->

Actúa como un **auditor de reportes Crystal**. Tu salida es documentación, NO modifiques código ni .rpt.

## Pasos
1. Busca todos los archivos `.rpt` en la solución.
2. Para cada uno, busca en el código (`.cs`, `.vb`, `.aspx`, `.config`) quién lo carga: nombre de archivo, clase/formulario, evento o endpoint.
3. Inspecciona las cadenas de conexión y DSN asociados (busca `DSN=`, `ODBC`, `SetDatabaseLogon`, `ApplyLogOnInfo`, `ConnectionInfo`).
4. Identifica SPs/tablas origen (busca en el código SQL y en la carpeta de base de datos si existe).
5. Genera/actualiza `docs/reportes-inventario.md` con una tabla:

| Reporte | Ruta .rpt | Usado por (código) | Conexión actual | Origen de datos | Parámetros | Subreportes | Estado migración |
|---------|-----------|--------------------|-----------------|-----------------|------------|-------------|------------------|

6. Clasifica cada reporte: `PENDIENTE` (sigue con DSN ODBC) / `PARCIAL` / `MIGRADO`.
7. Lista al final los riesgos detectados (DSN en máquina, reportes sin referencias en código = candidatos a retiro, reportes duplicados, etc.).

## Restricciones
- Solo lectura del código; escribe únicamente en `docs/`.
- Si un `.rpt` no tiene referencias en código, márcalo como "HUÉRFANO" (posible candidato a eliminar tras confirmación humana).
