using System;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Infraestructura.Reportes
{
    /// <summary>
    /// Punto único para abrir reportes .rpt heredados de VB6 (que traen DSN ODBC embebido)
    /// aplicando la conexión central de la aplicación en runtime.
    /// Requiere referencias: CrystalDecisions.CrystalReports.Engine, CrystalDecisions.Shared.
    /// </summary>
    public static class CrystalReportHelper
    {
        // TODO: enlazar estas propiedades con la configuración real de la app
        // (appsettings.json / web.config / app.config). No dejar valores fijos.
        public static string ServerName { get; set; } = string.Empty;
        public static string DatabaseName { get; set; } = string.Empty;
        public static string UserId { get; set; } = string.Empty;
        public static string Password { get; set; } = string.Empty;
        public static bool UseIntegratedSecurity { get; set; }

        /// <summary>Carga un reporte y le aplica la conexión central (incluye subreportes).</summary>
        public static ReportDocument AbrirReporte(string rutaRpt)
        {
            var report = new ReportDocument();
            // OpenReportByTempCopy evita bloquear el .rpt en disco
            report.Load(rutaRpt, OpenReportMethod.OpenReportByTempCopy);
            AplicarConexion(report);
            return report;
        }

        /// <summary>
        /// Aplica el ConnectionInfo central a todas las tablas del reporte
        /// y de forma recursiva a sus subreportes. Sustituye al DSN ODBC embebido.
        /// </summary>
        public static void AplicarConexion(ReportDocument report)
        {
            var ci = CrearConnectionInfo();

            foreach (Table tabla in report.Database.Tables)
            {
                TableLogOnInfo logon = tabla.LogOnInfo;
                logon.ConnectionInfo = ci;
                tabla.ApplyLogOnInfo(logon);
            }

            foreach (ReportDocument subreporte in report.Subreports)
            {
                AplicarConexion(subreporte);
            }
        }

        private static ConnectionInfo CrearConnectionInfo()
        {
            var ci = new ConnectionInfo
            {
                ServerName = ServerName,
                DatabaseName = DatabaseName,
                IntegratedSecurity = UseIntegratedSecurity
            };
            if (!UseIntegratedSecurity)
            {
                ci.UserID = UserId;
                ci.Password = Password;
            }
            return ci;
        }

        /// <summary>
        /// Equivalente al "Verify Database" del Designer: sincroniza el .rpt con la
        /// estructura actual de la fuente de datos (necesario tras agregar campos al SP/consulta).
        /// </summary>
        public static void VerificarBaseDeDatos(ReportDocument report)
        {
            report.VerifyDatabase();
        }
    }
}
