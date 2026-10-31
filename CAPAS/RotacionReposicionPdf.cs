using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace CAPAS
{
    // Renderer estático del Informe de Rotación y Reposición de Stock (N03 / A02). Documento
    // comercial: texto y formato siempre en español/es-AR, independiente del idioma de sesión,
    // igual que el remito de despacho — no implementa IObservadorIdioma.
    internal static class RotacionReposicionPdf
    {
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        // Guarda el informe en Documentos\ClubDeVinos\Informes\Rotacion_{yyyyMMdd_HHmmss_fff}.pdf y
        // devuelve la ruta. Propaga IOException/UnauthorizedAccessException si la carpeta no es
        // escribible: el caller decide cómo informarlo.
        internal static string Generar(List<BE.ResultadoRotacion> filas, DateTime desde, DateTime hasta,
                                       BE.ConfiguracionRotacion config)
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ClubDeVinos", "Informes");
            Directory.CreateDirectory(carpeta);
            DateTime generado = DateTime.Now;
            string ruta = Path.Combine(carpeta, $"Rotacion_{generado:yyyyMMdd_HHmmss_fff}.pdf");

            Document documento = new Document();
            Section seccion = documento.AddSection();
            seccion.PageSetup.PageFormat = PageFormat.A4;
            // Apaisado: la tabla tiene 11 columnas.
            seccion.PageSetup.Orientation = Orientation.Landscape;
            seccion.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            seccion.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            seccion.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            seccion.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            Paragraph titulo = seccion.AddParagraph("INFORME DE ROTACIÓN Y REPOSICIÓN DE STOCK");
            titulo.Format.Font.Size = 16;
            titulo.Format.Font.Bold = true;

            Paragraph subtitulo = seccion.AddParagraph("Club de Vinos");
            subtitulo.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            AgregarEncabezado(seccion, filas, desde, hasta, generado, config);
            AgregarResumen(seccion, filas);
            AgregarTabla(seccion, filas);

            Paragraph nota = seccion.AddParagraph(
                "Consumo mensual = unidades vendidas por cajas / (días del período / 30). " +
                "Cobertura = stock actual / consumo mensual. Las pérdidas son salidas por ajustes de inventario. " +
                "A reponer (solo vinos a reponer o a reponer con urgencia) = máx(consumo mensual × cobertura objetivo; stock mínimo) − stock actual, " +
                "redondeado hacia arriba.");
            nota.Format.Font.Size = 8;
            nota.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true) { Document = documento };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(ruta);

            return ruta;
        }

        private static void AgregarEncabezado(Section seccion, List<BE.ResultadoRotacion> filas, DateTime desde,
                                              DateTime hasta, DateTime generado, BE.ConfiguracionRotacion config)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Visible = false;
            tabla.AddColumn(Unit.FromCentimeter(12.5));
            tabla.AddColumn(Unit.FromCentimeter(12.5));

            int dias = BLL.RotacionBLL.CantidadDias(desde, hasta);
            Row fila1 = tabla.AddRow();
            fila1.Cells[0].AddParagraph("Período analizado: " + desde.ToString("dd/MM/yyyy", _cultura)
                + " al " + hasta.ToString("dd/MM/yyyy", _cultura) + " (" + dias + " días)");
            fila1.Cells[1].AddParagraph("Fecha de generación: " + generado.ToString("dd/MM/yyyy HH:mm", _cultura));

            Row fila2 = tabla.AddRow();
            fila2.Cells[0].AddParagraph("Umbrales: alta rotación ≥ "
                + config.UmbralAltaRotacion.ToString("0.##", _cultura) + " u/mes; baja rotación < "
                + config.UmbralBajaRotacion.ToString("0.##", _cultura) + " u/mes");
            fila2.Cells[1].AddParagraph("Cobertura (meses): urgente < "
                + config.MesesCoberturaUrgente.ToString("0.##", _cultura) + "; reponer < "
                + config.MesesCoberturaReposicion.ToString("0.##", _cultura) + "; sobrestock > "
                + config.MesesCoberturaSobrestock.ToString("0.##", _cultura) + "; objetivo = "
                + config.MesesCoberturaObjetivo.ToString("0.##", _cultura));

            // Stock actual vs. rango pasado: la cobertura puede no ser representativa.
            if (hasta.Date < generado.Date)
            {
                Paragraph aviso = seccion.AddParagraph(
                    "Nota: el stock es el del día de generación del informe; la cobertura puede no ser " +
                    "representativa para períodos que terminan antes de esa fecha.");
                aviso.Format.Font.Italic = true;
                aviso.Format.Font.Size = 9;
                aviso.Format.SpaceBefore = Unit.FromCentimeter(0.15);
            }

            seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.2);
        }

        // Recuadro con la cantidad de vinos por recomendación.
        private static void AgregarResumen(Section seccion, List<BE.ResultadoRotacion> filas)
        {
            Paragraph rotulo = seccion.AddParagraph("Resumen (" + filas.Count + " vinos activos analizados)");
            rotulo.Format.Font.Bold = true;
            rotulo.Format.SpaceAfter = Unit.FromCentimeter(0.15);

            List<KeyValuePair<string, int>> resumen = BLL.RotacionBLL.ResumirPorRecomendacion(filas);

            Table tabla = seccion.AddTable();
            tabla.Borders.Width = 0.75;
            tabla.Shading.Color = Color.FromRgb(245, 245, 245);
            // Una columna por recomendación más una para el total de unidades sugeridas
            // (5 x 5.2 cm = 26 cm de 26.7 útiles).
            for (int i = 0; i <= resumen.Count; i++)
                tabla.AddColumn(Unit.FromCentimeter(5.2));

            Row encabezado = tabla.AddRow();
            encabezado.Format.Font.Bold = true;
            Row valores = tabla.AddRow();
            for (int i = 0; i < resumen.Count; i++)
            {
                encabezado.Cells[i].AddParagraph(resumen[i].Key);
                Paragraph p = valores.Cells[i].AddParagraph(resumen[i].Value.ToString(_cultura));
                p.Format.Font.Size = 14;
                p.Format.Font.Bold = true;
                encabezado.Cells[i].Shading.Color = ColorDeRecomendacion(resumen[i].Key);
            }
            encabezado.Cells[resumen.Count].AddParagraph("Unidades sugeridas a reponer");
            Paragraph total = valores.Cells[resumen.Count].AddParagraph(
                BLL.RotacionBLL.TotalUnidadesSugeridas(filas).ToString(_cultura));
            total.Format.Font.Size = 14;
            total.Format.Font.Bold = true;

            seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        // Una sola tabla, ordenada de la recomendación más urgente a la menos urgente.
        private static void AgregarTabla(Section seccion, List<BE.ResultadoRotacion> filas)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Width = 0.5;
            tabla.Format.Font.Size = 8.5;
            tabla.AddColumn(Unit.FromCentimeter(2.0));   // SKU
            tabla.AddColumn(Unit.FromCentimeter(4.6));   // Vino
            tabla.AddColumn(Unit.FromCentimeter(1.6));   // Stock
            tabla.AddColumn(Unit.FromCentimeter(1.8));   // Vendidas
            tabla.AddColumn(Unit.FromCentimeter(1.8));   // Pérdidas
            tabla.AddColumn(Unit.FromCentimeter(2.0));   // Consumo mensual
            tabla.AddColumn(Unit.FromCentimeter(2.2));   // Cobertura
            tabla.AddColumn(Unit.FromCentimeter(1.4));   // Socios
            tabla.AddColumn(Unit.FromCentimeter(3.0));   // Clasificación
            tabla.AddColumn(Unit.FromCentimeter(3.6));   // Recomendación
            tabla.AddColumn(Unit.FromCentimeter(1.9));   // A reponer (total 25.9 cm de 26.7 útiles)

            Row encabezado = tabla.AddRow();
            encabezado.HeadingFormat = true;
            encabezado.Shading.Color = Colors.LightGray;
            encabezado.Format.Font.Bold = true;
            string[] titulos = { "SKU", "Vino", "Stock", "Vendidas", "Pérdidas", "Cons./mes", "Cobertura (meses)",
                                 "Socios", "Clasificación", "Recomendación", "A reponer" };
            for (int i = 0; i < titulos.Length; i++)
                encabezado.Cells[i].AddParagraph(titulos[i]);

            foreach (BE.ResultadoRotacion f in filas.OrderBy(x => x.Prioridad).ThenBy(x => x.Nombre))
            {
                Row fila = tabla.AddRow();
                fila.Cells[0].AddParagraph(f.Codigo);
                fila.Cells[1].AddParagraph(f.Nombre);
                fila.Cells[2].AddParagraph(f.StockActual.ToString(_cultura));
                fila.Cells[3].AddParagraph(f.UnidadesVendidas.ToString(_cultura));
                fila.Cells[4].AddParagraph(f.UnidadesPerdidas.ToString(_cultura));
                fila.Cells[5].AddParagraph(f.ConsumoMensual.ToString("0.0", _cultura));
                fila.Cells[6].AddParagraph(f.MesesCobertura.HasValue ? f.MesesCobertura.Value.ToString("0.0", _cultura) : "—");
                fila.Cells[7].AddParagraph(f.SociosAtendidos.ToString(_cultura));
                fila.Cells[8].AddParagraph(f.Clasificacion);
                fila.Cells[9].AddParagraph(f.Recomendacion);
                fila.Cells[9].Shading.Color = ColorDeRecomendacion(f.Recomendacion);
                fila.Cells[9].Format.Font.Bold = true;
                fila.Cells[10].AddParagraph(f.CantidadSugerida.ToString(_cultura));
                fila.Cells[10].Format.Font.Bold = f.CantidadSugerida > 0;
            }
        }

        // Tonos claros para que el texto negro siga legible al imprimir.
        private static Color ColorDeRecomendacion(string recomendacion)
        {
            switch (recomendacion)
            {
                case BE.RecomendacionReposicion.Urgente: return Color.FromRgb(244, 199, 195);
                case BE.RecomendacionReposicion.Reponer: return Color.FromRgb(252, 232, 178);
                case BE.RecomendacionReposicion.Mantener: return Color.FromRgb(200, 230, 201);
                default: return Color.FromRgb(207, 226, 243);
            }
        }

        // Abre el PDF con la aplicación asociada del SO. Traga su propia excepción
        // (lector de PDF ausente, etc.): el archivo ya quedó en disco.
        internal static void Abrir(string rutaPdf)
        {
            try
            {
                // Process.Start puede devolver null (ShellExecute); using lo tolera y libera el handle.
                using (System.Diagnostics.Process.Start(rutaPdf)) { }
            }
            catch
            {
                // best-effort: el archivo ya existe en la carpeta de informes, solo falló abrirlo.
            }
        }
    }
}
