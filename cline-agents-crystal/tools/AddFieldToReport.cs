// Herramienta AVANZADA (opción B del workflow agregar-campo-reporte):
// inserta un campo de base de datos en una sección del .rpt usando el SDK RAS in-proc
// incluido en Crystal Reports for Visual Studio (v13). Solo Windows.
//
// IMPORTANTE:
// - Antes de insertar el campo, la fuente de datos debe exponerlo y el reporte debe
//   haber pasado VerifyDatabase(); si no, el campo no existe para el reporte.
// - Esta herramienta SIEMPRE crea respaldo <archivo>.bak-YYYYMMDD antes de guardar.
// - Referencias necesarias:
//     CrystalDecisions.CrystalReports.Engine
//     CrystalDecisions.ReportAppServer.ClientDoc
//     CrystalDecisions.ReportAppServer.Controllers
//     CrystalDecisions.ReportAppServer.DataDefModel
//     CrystalDecisions.ReportAppServer.ReportDefModel
//
// Uso:
//   AddFieldToReport.exe <ruta.rpt> <tabla> <campo> <seccion> <x> <y> <ancho> <alto> [encabezado]
//   seccion: reportHeader | pageHeader | groupHeader1..n | detail | groupFooter1..n | reportFooter | pageFooter
//   x, y, ancho, alto en twips (1440 twips = 1 pulgada).

using System;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.Controllers;
using CrystalDecisions.ReportAppServer.DataDefModel;
using CrystalDecisions.ReportAppServer.ReportDefModel;

using RasFieldObject = CrystalDecisions.ReportAppServer.ReportDefModel.FieldObject;
using RasTextObject = CrystalDecisions.ReportAppServer.ReportDefModel.TextObject;
using RasSection = CrystalDecisions.ReportAppServer.ReportDefModel.Section;

namespace Herramientas.Reportes
{
    internal static class AddFieldToReport
    {
        private static int Main(string[] args)
        {
            if (args.Length < 8)
            {
                Console.WriteLine("Uso: AddFieldToReport.exe <ruta.rpt> <tabla> <campo> <seccion> <x> <y> <ancho> <alto> [encabezado]");
                return 2;
            }

            string rutaRpt = args[0];
            string tabla = args[1];
            string campo = args[2];
            string seccionNombre = args[3];
            int x = int.Parse(args[4]);
            int y = int.Parse(args[5]);
            int ancho = int.Parse(args[6]);
            int alto = int.Parse(args[7]);
            string encabezado = args.Length > 8 ? args[8] : null;

            // 1) Respaldo obligatorio
            string respaldo = $"{rutaRpt}.bak-{DateTime.Now:yyyyMMdd}";
            if (!File.Exists(respaldo))
                File.Copy(rutaRpt, respaldo);


            ReportDocument report = null;
            try
            {
                report = new ReportDocument();
                report.Load(rutaRpt, OpenReportMethod.OpenReportByCopy);

                ISCDReportClientDocument clientDoc = report.ReportClientDocument;
                ReportDefController defController = clientDoc.ReportDefController;
                ReportObjectController objController = defController.ReportObjectController;

                // 2) Resolver la sección destino por nombre (detail, pageHeader, etc.)
                RasSection seccion = BuscarSeccion(report, seccionNombre);
                if (seccion == null)
                {
                    Console.Error.WriteLine($"Sección no encontrada: {seccionNombre}");
                    return 1;
                }

                // 3) Crear el FieldObject apuntando al campo de BD
                var field = new RasFieldObject
                {
                    FieldValueType = FieldValueType.DatabaseField,
                    DataSourceName = $"{{{tabla}.{campo}}}", // sintaxis de nombre de campo CR
                    Left = x,
                    Top = y,
                    Width = ancho,
                    Height = alto
                };

                objController.Add(field, seccion);

                // 4) Encabezado de columna opcional (TextObject en el encabezado de página)
                if (!string.IsNullOrEmpty(encabezado))
                {
                    RasSection pageHeader = BuscarSeccion(report, "pageHeader");
                    if (pageHeader != null)
                    {
                        var titulo = new RasTextObject
                        {
                            Text = encabezado,
                            Left = x,
                            Top = 0,
                            Width = ancho,
                            Height = alto
                        };
                        objController.Add(titulo, pageHeader);
                    }
                }

                // 5) Guardar como .rpt
                clientDoc.SaveAs(rutaRpt, (int)CdReportClientDocumentSaveAsOptionsEnum.cdReportClientDocumentSaveAsOverwriteExisting);
                Console.WriteLine($"Campo '{tabla}.{campo}' agregado a '{seccionNombre}' en {rutaRpt}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine("El respaldo .bak-YYYYMMDD conserva la versión previa del reporte.");
                return 1;
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

        /// <summary>
        /// Busca una sección por nombre recorriendo las áreas del reporte.
        /// Nombres RAS: ReportHeader, PageHeader, GroupHeaderSection1..n, Details,
        /// GroupFooterSection1..n, ReportFooter, PageFooter.
        /// </summary>
        private static RasSection BuscarSeccion(ReportDocument report, string nombre)
        {
            string canonico = NormalizarNombreSeccion(nombre);
            var def = report.ReportClientDocument.ReportDefController.ReportDefinition;

            var areas = new[]
            {
                def.ReportHeaderArea,
                def.PageHeaderArea,
                def.DetailArea,
                def.ReportFooterArea,
                def.PageFooterArea
            };

            foreach (var area in areas)
            {
                if (area == null) continue;
                foreach (RasSection s in area.Sections)
                {
                    if (s.Name.Equals(canonico, StringComparison.OrdinalIgnoreCase))
                        return s;
                }
            }
            return null;
        }

        private static string NormalizarNombreSeccion(string nombre)
        {
            switch (nombre.ToLowerInvariant())
            {
                case "reportheader": return "ReportHeader";
                case "pageheader": return "PageHeader";
                case "detail":
                case "details": return "Details";
                case "reportfooter": return "ReportFooter";
                case "pagefooter": return "PageFooter";
                default:
                    // groupHeader1 -> GroupHeaderSection1, etc.
                    if (nombre.StartsWith("groupheader", StringComparison.OrdinalIgnoreCase))
                        return "GroupHeaderSection" + nombre.Substring("groupheader".Length);
                    if (nombre.StartsWith("groupfooter", StringComparison.OrdinalIgnoreCase))
                        return "GroupFooterSection" + nombre.Substring("groupfooter".Length);
                    return nombre;
            }
        }
    }
}

