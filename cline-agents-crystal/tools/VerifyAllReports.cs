// Herramienta de consola (Windows + runtime CR for Visual Studio v13):
// verifica que todos los .rpt de una carpeta carguen y pasen VerifyDatabase().
//
// Uso:  VerifyAllReports.exe <carpetaRpt> [filtroNombre]
// Salida: imprime OK/DATOS/CONEXION/RUNTIME por reporte y termina con código 1 si hay fallos.
//
// Sugerencia: compilar como proyecto de consola .NET Framework 4.x (x86 o x64 según el runtime CR instalado)
// referenciando CrystalDecisions.CrystalReports.Engine y CrystalDecisions.Shared,
// y reutilizando CrystalReportHelper del proyecto principal.

using System;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Herramientas.Reportes
{
    internal static class VerifyAllReports
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uso: VerifyAllReports.exe <carpetaRpt> [filtroNombre]");
                return 2;
            }

            string carpeta = args[0];
            string filtro = args.Length > 1 ? args[1] : null;
            int fallos = 0;

            foreach (string rpt in Directory.GetFiles(carpeta, "*.rpt", SearchOption.AllDirectories))
            {
                string nombre = Path.GetFileNameWithoutExtension(rpt);
                if (filtro != null && !nombre.Equals(filtro, StringComparison.OrdinalIgnoreCase))
                    continue;

                string resultado = VerificarUno(rpt);
                Console.WriteLine($"{resultado}\t{nombre}");
                if (resultado != "OK") fallos++;
            }

            return fallos == 0 ? 0 : 1;
        }

        private static string VerificarUno(string rutaRpt)
        {
            ReportDocument report = null;
            try
            {
                report = new ReportDocument();
                report.Load(rutaRpt, OpenReportMethod.OpenReportByTempCopy);

                // Aplicar la conexión central (sustituye el DSN ODBC embebido)
                Infraestructura.Reportes.CrystalReportHelper.AplicarConexion(report);

                report.VerifyDatabase();
                return "OK";
            }
            catch (DataSourceException)
            {
                return "DATOS"; // estructura de la fuente distinta al diseño del reporte
            }
            catch (LogOnException)
            {
                return "CONEXION"; // DSN inexistente, credenciales o servidor inalcanzable
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  -> {rutaRpt}: {ex.GetType().Name}: {ex.Message}");
                return "RUNTIME";
            }
            finally
            {
                if (report != null)
                {
                    report.Close();
                    report.Dispose();
                }
            }
        }
    }
}
