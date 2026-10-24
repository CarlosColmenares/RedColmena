# Workflow: Migrar conexiones ODBC de reportes a conexión central .NET

<!-- Copia a .cline/workflows/migrar-odbc-a-sql.md — se invoca con /migrar-odbc-a-sql.md -->
<!-- Rol: "Agente Migrador de Conexiones" -->

Actúa como un **ingeniero de migración VB6→.NET**. Objetivo: eliminar la dependencia de DSN ODBC en runtime.

## Contexto
Los `.rpt` heredados de VB6 llevan conexión ODBC embebida. En .NET se corrige aplicando
`ConnectionInfo`/`TableLogOnInfo` en runtime sobre cada tabla (y subreporte) del reporte.

## Pasos
1. Ejecuta primero el workflow `/inventario-reportes.md` si `docs/reportes-inventario.md` no existe o está desactualizado.
2. Verifica que exista `CrystalReportHelper` con un método equivalente a:

```csharp
public static void AplicarConexion(ReportDocument report)
{
    var ci = new ConnectionInfo
    {
        ServerName = Config.ReportServer,      // p.ej. servidor o DSN único de transición
        DatabaseName = Config.ReportDatabase,
        UserID = Config.ReportUser,
        Password = Config.ReportPassword,
        IntegratedSecurity = Config.UseIntegratedSecurity
    };
    foreach (Table t in report.Database.Tables)
    {
        var logon = t.LogOnInfo;
        logon.ConnectionInfo = ci;
        t.ApplyLogOnInfo(logon);
    }
    foreach (ReportDocument sub in report.Subreports)
        AplicarConexion(sub); // recursivo
}
```

   Si no existe, créalo en la capa de infraestructura del proyecto siguiendo las convenciones existentes.
   La configuración debe venir de `appsettings.json`/`web.config`, nunca hardcodeada.

3. Busca todos los usos de `SetDatabaseLogon`, `ConnectionInfo`, `DSN=` y `ApplyLogOnInfo` dispersos en el código y reemplázalos por llamadas a `CrystalReportHelper.AplicarConexion`.
4. Asegura que cada `ReportDocument` se disponga correctamente (`using` o `try/finally` con `Close()` y `Dispose()`).
5. Compila la solución tras cada lote de cambios (no más de 5 archivos por lote).
6. Actualiza la columna "Estado migración" de `docs/reportes-inventario.md`.

## Restricciones
- No cambies la lógica de negocio ni los parámetros de los reportes: solo la conexión.
- Si un formulario muestra el reporte en un `CrystalReportViewer`, conserva el viewer y su binding.
- Si el proyecto destino usa autenticación integrada, propón ambos caminos y pide confirmación antes de elegir.
