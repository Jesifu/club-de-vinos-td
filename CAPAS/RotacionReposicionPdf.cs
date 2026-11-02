using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Windows.Forms.DataVisualization.Charting;

namespace CAPAS
{
    // Renderer estático del Informe de Rotación y Reposición de Stock (N03 / A02). Los textos salen
    // en el idioma que recibe por la función de traducción (la misma del formulario); sin función
    // (null) o sin traducción de una clave, queda el español por defecto. Fechas y números van
    // siempre en es-AR: BE.IDIOMA no tiene cultura asociada. No implementa IObservadorIdioma.
    internal static class RotacionReposicionPdf
    {
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        // Guarda el informe en Documentos\ClubDeVinos\Informes\Rotacion_{yyyyMMdd_HHmmss_fff}.pdf y
        // devuelve la ruta. Propaga IOException/UnauthorizedAccessException si la carpeta no es
        // escribible: el caller decide cómo informarlo.
        internal static string Generar(List<BE.ResultadoRotacion> filas, DateTime desde, DateTime hasta,
                                       BE.ConfiguracionRotacion config,
                                       Func<string, string> traducir = null)
        {
            RotacionGraficos.Opciones txt = new RotacionGraficos.Opciones { Traducir = traducir, Cultura = _cultura };
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

            Paragraph titulo = seccion.AddParagraph(RotacionGraficos.T(txt, "rot_pdf_Titulo", "INFORME DE ROTACIÓN Y REPOSICIÓN DE STOCK"));
            titulo.Format.Font.Size = 16;
            titulo.Format.Font.Bold = true;

            Paragraph subtitulo = seccion.AddParagraph("Club de Vinos");
            subtitulo.Format.SpaceAfter = Unit.FromCentimeter(0.3);

            AgregarEncabezado(seccion, filas, desde, hasta, generado, config, txt);
            AgregarResumen(seccion, filas, txt);
            AgregarGraficos(seccion, filas, txt);
            AgregarTabla(seccion, filas, txt);

            seccion.AddParagraph(
                // Dos claves: el texto de TRADUCCION admite 200 caracteres como máximo.
                RotacionGraficos.T(txt, "rot_pdf_NotaFormula1",
                    "Consumo mensual = unidades vendidas por cajas / (días del período / 30). " +
                    "Cobertura = stock actual / consumo mensual. Las pérdidas son salidas por ajustes de inventario.") + " " +
                RotacionGraficos.T(txt, "rot_pdf_NotaFormula2",
                    "A reponer (solo vinos a reponer o a reponer con urgencia) = máx(consumo mensual × cobertura objetivo; stock mínimo) − stock actual, " +
                    "redondeado hacia arriba."));
            Paragraph nota = (Paragraph)seccion.LastParagraph;
            nota.Format.Font.Size = 8;
            nota.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true) { Document = documento };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(ruta);

            return ruta;
        }

        private static void AgregarEncabezado(Section seccion, List<BE.ResultadoRotacion> filas, DateTime desde,
                                              DateTime hasta, DateTime generado, BE.ConfiguracionRotacion config,
                                              RotacionGraficos.Opciones txt)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Visible = false;
            tabla.AddColumn(Unit.FromCentimeter(12.5));
            tabla.AddColumn(Unit.FromCentimeter(12.5));

            int dias = BLL.RotacionBLL.CantidadDias(desde, hasta);
            Row fila1 = tabla.AddRow();
            fila1.Cells[0].AddParagraph(RotacionGraficos.Formatear(txt, "rot_pdf_Periodo",
                "Período analizado: {0} al {1} ({2} días)",
                desde.ToString("dd/MM/yyyy", _cultura), hasta.ToString("dd/MM/yyyy", _cultura), dias));
            fila1.Cells[1].AddParagraph(RotacionGraficos.Formatear(txt, "rot_pdf_Generado",
                "Fecha de generación: {0}", generado.ToString("dd/MM/yyyy HH:mm", _cultura)));

            Row fila2 = tabla.AddRow();
            fila2.Cells[0].AddParagraph(RotacionGraficos.Formatear(txt, "rot_pdf_Umbrales",
                "Umbrales: alta rotación ≥ {0} u/mes; baja rotación < {1} u/mes",
                config.UmbralAltaRotacion.ToString("0.##", _cultura),
                config.UmbralBajaRotacion.ToString("0.##", _cultura)));
            fila2.Cells[1].AddParagraph(RotacionGraficos.Formatear(txt, "rot_pdf_Cobertura",
                "Cobertura (meses): urgente < {0}; reponer < {1}; sobrestock > {2}; objetivo = {3}",
                config.MesesCoberturaUrgente.ToString("0.##", _cultura),
                config.MesesCoberturaReposicion.ToString("0.##", _cultura),
                config.MesesCoberturaSobrestock.ToString("0.##", _cultura),
                config.MesesCoberturaObjetivo.ToString("0.##", _cultura)));

            // Stock actual vs. rango pasado: la cobertura puede no ser representativa.
            if (hasta.Date < generado.Date)
            {
                Paragraph aviso = seccion.AddParagraph(RotacionGraficos.T(txt, "rot_pdf_NotaStock",
                    "Nota: el stock es el del día de generación del informe; la cobertura puede no ser " +
                    "representativa para períodos que terminan antes de esa fecha."));
                aviso.Format.Font.Italic = true;
                aviso.Format.Font.Size = 9;
                aviso.Format.SpaceBefore = Unit.FromCentimeter(0.15);
            }

            seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.2);
        }

        // Recuadro con la cantidad de vinos por recomendación.
        private static void AgregarResumen(Section seccion, List<BE.ResultadoRotacion> filas,
                                            RotacionGraficos.Opciones txt)
        {
            Paragraph rotulo = seccion.AddParagraph(RotacionGraficos.Formatear(txt, "rot_pdf_Resumen",
                "Resumen ({0} vinos activos analizados)", filas.Count));
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
                encabezado.Cells[i].AddParagraph(NombreValor(txt, resumen[i].Key));
                Paragraph p = valores.Cells[i].AddParagraph(resumen[i].Value.ToString(_cultura));
                p.Format.Font.Size = 14;
                p.Format.Font.Bold = true;
                encabezado.Cells[i].Shading.Color = ColorDeRecomendacion(resumen[i].Key);
            }
            encabezado.Cells[resumen.Count].AddParagraph(
                RotacionGraficos.T(txt, "rot_UnidadesSugeridas", "Unidades sugeridas a reponer"));
            Paragraph total = valores.Cells[resumen.Count].AddParagraph(
                BLL.RotacionBLL.TotalUnidadesSugeridas(filas).ToString(_cultura));
            total.Format.Font.Size = 14;
            total.Format.Font.Bold = true;

            seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.3);
        }

        // Resolución con que se renderizan los gráficos para el papel.
        private const double DpiGraficos = 200.0;

        // Resumen visual: la primera página termina con dos gráficos lado a lado (consumo vs stock
        // y distribución por recomendación); la segunda abre con las unidades sugeridas, si las hay,
        // y sigue con la tabla. Las imágenes van en memoria (base64), sin archivos temporales.
        private static void AgregarGraficos(Section seccion, List<BE.ResultadoRotacion> filas,
                                             RotacionGraficos.Opciones txt)
        {
            // Anchos y alto en cm: dos columnas de 15.6 y 11.0 cm entran en los 26.7 cm útiles.
            const double anchoConsumo = 15.3, anchoDistribucion = 10.3, altoFila = 9.6;
            const double anchoSugeridas = 26.4, altoSugeridas = 6.8;

            byte[] pngConsumo, pngDistribucion, pngSugeridas = null;
            using (Chart c = RotacionGraficos.ConsumoVsStock(filas, OpcionesPdf(txt, anchoConsumo, altoFila)))
                pngConsumo = RotacionGraficos.APng(c);
            using (Chart c = RotacionGraficos.Distribucion(filas, OpcionesPdf(txt, anchoDistribucion, altoFila)))
                pngDistribucion = RotacionGraficos.APng(c);
            if (filas.Any(f => f.CantidadSugerida > 0))
            {
                using (Chart c = RotacionGraficos.UnidadesSugeridas(filas, OpcionesPdf(txt, anchoSugeridas, altoSugeridas)))
                    pngSugeridas = RotacionGraficos.APng(c);
            }

            Table tabla = seccion.AddTable();
            tabla.Borders.Visible = false;
            tabla.AddColumn(Unit.FromCentimeter(15.6));
            tabla.AddColumn(Unit.FromCentimeter(11.0));
            Row fila = tabla.AddRow();
            AgregarImagen(fila.Cells[0].AddImage(ComoBase64(pngConsumo)), anchoConsumo);
            AgregarImagen(fila.Cells[1].AddImage(ComoBase64(pngDistribucion)), anchoDistribucion);

            // La tabla de detalle arranca en la página siguiente.
            seccion.AddPageBreak();
            if (pngSugeridas != null)
            {
                AgregarImagen(seccion.AddImage(ComoBase64(pngSugeridas)), anchoSugeridas);
                seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.2);
            }
        }

        private static RotacionGraficos.Opciones OpcionesPdf(RotacionGraficos.Opciones txt, double anchoCm, double altoCm)
        {
            double pxPorCm = DpiGraficos / 2.54;
            return new RotacionGraficos.Opciones
            {
                Traducir = txt.Traducir,   // mismo traductor que el formulario (null = español)
                Cultura = _cultura,
                Escala = (float)(DpiGraficos / 96.0),
                Ancho = (int)Math.Round(anchoCm * pxPorCm),
                Alto = (int)Math.Round(altoCm * pxPorCm)
            };
        }

        private static string ComoBase64(byte[] png)
        {
            return "base64:" + Convert.ToBase64String(png);
        }

        private static void AgregarImagen(MigraDoc.DocumentObjectModel.Shapes.Image imagen, double anchoCm)
        {
            imagen.LockAspectRatio = true;
            imagen.Width = Unit.FromCentimeter(anchoCm);
        }

        // Una sola tabla, ordenada de la recomendación más urgente a la menos urgente.
        private static void AgregarTabla(Section seccion, List<BE.ResultadoRotacion> filas,
                                         RotacionGraficos.Opciones txt)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Width = 0.5;
            tabla.Format.Font.Size = 8.5;
            tabla.AddColumn(Unit.FromCentimeter(2.0));   // SKU
            tabla.AddColumn(Unit.FromCentimeter(4.4));   // Vino
            tabla.AddColumn(Unit.FromCentimeter(1.6));   // Stock
            tabla.AddColumn(Unit.FromCentimeter(1.8));   // Vendidas
            tabla.AddColumn(Unit.FromCentimeter(1.8));   // Pérdidas
            tabla.AddColumn(Unit.FromCentimeter(2.3));   // Consumo mensual
            tabla.AddColumn(Unit.FromCentimeter(2.2));   // Cobertura
            tabla.AddColumn(Unit.FromCentimeter(1.6));   // Socios
            tabla.AddColumn(Unit.FromCentimeter(3.0));   // Clasificación
            tabla.AddColumn(Unit.FromCentimeter(3.6));   // Recomendación
            tabla.AddColumn(Unit.FromCentimeter(2.3));   // A reponer (total 26.6 cm de 26.7 útiles)

            Row encabezado = tabla.AddRow();
            encabezado.HeadingFormat = true;
            encabezado.Shading.Color = Colors.LightGray;
            encabezado.Format.Font.Bold = true;
            string[] titulos = {
                RotacionGraficos.T(txt, "colhdr_Codigo", "SKU"),
                RotacionGraficos.T(txt, "rot_gra_EjeVino", "Vino"),
                RotacionGraficos.T(txt, "rot_pdf_ColStock", "Stock"),
                RotacionGraficos.T(txt, "colhdr_Vendidas", "Vendidas"),
                RotacionGraficos.T(txt, "colhdr_Perdidas", "Pérdidas"),
                RotacionGraficos.T(txt, "rot_pdf_ColConsMes", "Cons./mes"),
                RotacionGraficos.T(txt, "colhdr_Cobertura", "Cobertura (meses)"),
                RotacionGraficos.T(txt, "rot_pdf_ColSocios", "Socios"),
                RotacionGraficos.T(txt, "colhdr_Clasificacion", "Clasificación"),
                RotacionGraficos.T(txt, "colhdr_Recomendacion", "Recomendación"),
                RotacionGraficos.T(txt, "rot_pdf_ColAReponer", "A reponer") };
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
                fila.Cells[8].AddParagraph(NombreValor(txt, f.Clasificacion));
                fila.Cells[9].AddParagraph(NombreValor(txt, f.Recomendacion));
                fila.Cells[9].Shading.Color = ColorDeRecomendacion(f.Recomendacion);
                fila.Cells[9].Format.Font.Bold = true;
                fila.Cells[10].AddParagraph(f.CantidadSugerida.ToString(_cultura));
                fila.Cells[10].Format.Font.Bold = f.CantidadSugerida > 0;
            }
        }

        // Nombre traducido de una clasificación o recomendación (los valores guardados siguen siendo
        // las constantes en español de BE); sin traducción queda el valor original.
        private static string NombreValor(RotacionGraficos.Opciones txt, string valor)
        {
            string clave;
            switch (valor)
            {
                case BE.ClasificacionRotacion.Alta: clave = "rot_clas_Alta"; break;
                case BE.ClasificacionRotacion.Media: clave = "rot_clas_Media"; break;
                case BE.ClasificacionRotacion.Baja: clave = "rot_clas_Baja"; break;
                case BE.ClasificacionRotacion.SinMovimiento: clave = "rot_clas_SinMovimiento"; break;
                case BE.RecomendacionReposicion.Urgente: clave = "rot_rec_Urgente"; break;
                case BE.RecomendacionReposicion.Reponer: clave = "rot_rec_Reponer"; break;
                case BE.RecomendacionReposicion.Mantener: clave = "rot_rec_Mantener"; break;
                case BE.RecomendacionReposicion.Sobrestock: clave = "rot_rec_Sobrestock"; break;
                default: return valor;
            }
            return RotacionGraficos.T(txt, clave, valor);
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
