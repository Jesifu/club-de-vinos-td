using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace CAPAS
{
    // Renderer estático del Remito de Despacho (A02). Documento comercial: texto y
    // formato siempre en español/es-AR, independiente del idioma de sesión — no
    // implementa IObservadorIdioma (Decision 3, design). Ambos call sites (CU-31
    // frmDespacharCaja y CU-32 frmHistorialDespachos) comparten esta única implementación.
    internal static class RemitoDespachoPdf
    {
        private static readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("es-AR");

        // Construye el documento y lo guarda en Remitos\Remito_Caja{id}.pdf, devolviendo
        // la ruta completa. Propaga IOException/UnauthorizedAccessException si Remitos\
        // no es escribible — el caller decide cómo informarlo (best-effort, RN-11).
        internal static string Generar(BE.CajaMensual caja)
        {
            string carpeta = Path.Combine(Application.StartupPath, "Remitos");
            Directory.CreateDirectory(carpeta);
            string ruta = Path.Combine(carpeta, $"Remito_Caja{caja.Id}.pdf");

            Document documento = new Document();
            Section seccion = documento.AddSection();
            seccion.PageSetup.PageFormat = PageFormat.A4;
            seccion.PageSetup.TopMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            seccion.PageSetup.RightMargin = Unit.FromCentimeter(2);

            Paragraph titulo = seccion.AddParagraph("REMITO DE DESPACHO");
            titulo.Format.Font.Size = 16;
            titulo.Format.Font.Bold = true;

            Paragraph subtitulo = seccion.AddParagraph("Club de Vinos");
            subtitulo.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            AgregarInfoBlock(seccion, caja);
            AgregarTablaLineas(seccion, caja);

            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true) { Document = documento };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(ruta);

            return ruta;
        }

        private static void AgregarInfoBlock(Section seccion, BE.CajaMensual caja)
        {
            DateTime fechaDespacho = caja.FechaDespacho ?? DateTime.Now;

            Table tabla = seccion.AddTable();
            tabla.Borders.Visible = false;
            tabla.AddColumn(Unit.FromCentimeter(8));
            tabla.AddColumn(Unit.FromCentimeter(8));

            Row fila1 = tabla.AddRow();
            fila1.Cells[0].AddParagraph("N° de caja: " + caja.Id);
            fila1.Cells[1].AddParagraph("Socio: " + caja.SocioNombre);

            Row fila2 = tabla.AddRow();
            fila2.Cells[0].AddParagraph("Período: " + caja.Periodo);
            fila2.Cells[1].AddParagraph("Fecha de despacho: " + fechaDespacho.ToString("dd/MM/yyyy", _cultura));

            Row fila3 = tabla.AddRow();
            fila3.Cells[0].AddParagraph("Responsable: " + caja.DespachadoPorLogin);

            seccion.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.5);
        }

        private static void AgregarTablaLineas(Section seccion, BE.CajaMensual caja)
        {
            Table tabla = seccion.AddTable();
            tabla.Borders.Width = 0.5;
            tabla.AddColumn(Unit.FromCentimeter(2.5));
            tabla.AddColumn(Unit.FromCentimeter(7));
            tabla.AddColumn(Unit.FromCentimeter(3));
            tabla.AddColumn(Unit.FromCentimeter(3.5));

            Row encabezado = tabla.AddRow();
            encabezado.Shading.Color = Colors.LightGray;
            encabezado.Format.Font.Bold = true;
            encabezado.Cells[0].AddParagraph("Cantidad");
            encabezado.Cells[1].AddParagraph("Vino");
            encabezado.Cells[2].AddParagraph("Precio unit.");
            encabezado.Cells[3].AddParagraph("Subtotal");

            // Total = Σ Cantidad × PrecioEfectivo (RN-07). No se reutiliza
            // CajaMensualBLL.CalcularTotal — esa suma usa PrecioSnapshot e ignora
            // sustituciones (Corrección del design frente a la proposal).
            decimal total = 0m;
            foreach (BE.CajaVino linea in caja.Lineas)
            {
                decimal subtotal = linea.Cantidad * linea.PrecioEfectivo;
                total += subtotal;

                Row fila = tabla.AddRow();
                fila.Cells[0].AddParagraph(linea.Cantidad.ToString(_cultura));
                fila.Cells[1].AddParagraph(linea.NombreEfectivo);
                fila.Cells[2].AddParagraph(linea.PrecioEfectivo.ToString("C", _cultura));
                fila.Cells[3].AddParagraph(subtotal.ToString("C", _cultura));
            }

            Row filaTotal = tabla.AddRow();
            filaTotal.Format.Font.Bold = true;
            filaTotal.Cells[0].MergeRight = 2;
            filaTotal.Cells[0].AddParagraph("Total");
            filaTotal.Cells[3].AddParagraph(total.ToString("C", _cultura));

            Paragraph firma = seccion.AddParagraph();
            firma.Format.SpaceBefore = Unit.FromCentimeter(2);
            firma.AddText("Firma de recepción: ______________________");
        }

        // Abre el PDF con la aplicación asociada del SO. Traga su propia excepción
        // (lector de PDF ausente, etc.) — el archivo ya quedó en disco, generado
        // correctamente por Generar().
        internal static void Abrir(string rutaPdf)
        {
            try
            {
                System.Diagnostics.Process.Start(rutaPdf);
            }
            catch
            {
                // best-effort: el archivo ya existe en Remitos\, solo falló abrirlo.
            }
        }
    }
}
