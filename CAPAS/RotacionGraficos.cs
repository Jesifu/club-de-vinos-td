using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace CAPAS
{
    // Gráficos del análisis de rotación (N03), compartidos por la pantalla y por el informe PDF.
    // Usa el control Chart de System.Windows.Forms.DataVisualization, que viene con .NET
    // Framework 4.x (no hay nada extra que instalar). Todos los gráficos comparten paleta y un
    // fondo claro tipo "tarjeta", así se leen igual sobre el tema oscuro de la pantalla y sobre
    // papel. Ninguno es 3D.
    internal static class RotacionGraficos
    {
        // Cantidad máxima de vinos en los gráficos con un barrido por vino.
        internal const int MaxVinos = 10;
        private const int MaxLetrasNombre = 16;

        // Parámetros de render. Traducir devuelve la traducción de una clave o null; con null
        // se usa el texto en español (el PDF es siempre en español).
        internal sealed class Opciones
        {
            public Func<string, string> Traducir;
            public CultureInfo Cultura = CultureInfo.CurrentCulture;

            // Multiplica los tamaños de fuente: sirve para renderizar a mayor resolución (PDF).
            public float Escala = 1f;
            public int Ancho = 640;
            public int Alto = 360;

            // Fuentes creadas para el gráfico en construcción: las libera el propio gráfico al
            // desecharse (ver NuevoGrafico), así no se pierden handles GDI al reconstruirlos.
            internal List<Font> Fuentes = new List<Font>();

            // Copia con lista de fuentes propia: cada gráfico es dueño de las suyas.
            internal Opciones ParaGrafico()
            {
                return new Opciones
                {
                    Traducir = Traducir, Cultura = Cultura, Escala = Escala, Ancho = Ancho, Alto = Alto
                };
            }
        }

        // Paleta: una por recomendación, más tres neutros para las series de consumo y stock.
        private static readonly Color ColorFondo = Color.FromArgb(252, 250, 250);
        private static readonly Color ColorTexto = Color.FromArgb(45, 45, 52);
        private static readonly Color ColorTextoSuave = Color.FromArgb(110, 110, 118);
        private static readonly Color ColorGrilla = Color.FromArgb(226, 226, 230);
        private static readonly Color ColorConsumo = Color.FromArgb(74, 64, 120);
        private static readonly Color ColorStock = Color.FromArgb(166, 174, 184);

        internal static Color ColorDeRecomendacion(string recomendacion)
        {
            switch (recomendacion)
            {
                case BE.RecomendacionReposicion.Urgente: return Color.FromArgb(192, 57, 43);
                case BE.RecomendacionReposicion.Reponer: return Color.FromArgb(230, 126, 34);
                case BE.RecomendacionReposicion.Mantener: return Color.FromArgb(46, 139, 87);
                default: return Color.FromArgb(84, 110, 140);
            }
        }

        // 1. Consumo mensual vs stock actual de los vinos más urgentes, con el stock mínimo como marcador.
        internal static Chart ConsumoVsStock(List<BE.ResultadoRotacion> filas, Opciones o)
        {
            o = o.ParaGrafico();
            List<BE.ResultadoRotacion> top = (filas ?? new List<BE.ResultadoRotacion>())
                .OrderBy(f => f.Prioridad)
                .ThenBy(f => f.MesesCobertura ?? decimal.MaxValue)
                .ThenBy(f => f.Nombre)
                .Take(MaxVinos)
                .ToList();
            int total = filas == null ? 0 : filas.Count;

            Chart chart = NuevoGrafico(o, Formatear(o, "rot_gra_Titulo1",
                "Consumo mensual vs stock actual ({0} de {1} vinos)", top.Count, total));
            if (top.Count == 0) return SinDatos(chart, o);

            ChartArea area = chart.ChartAreas[0];
            EstilizarEjes(area, o, T(o, "rot_gra_EjeVino", "Vino"), T(o, "rot_gra_EjeUnidades", "Unidades"));
            area.AxisX.Interval = 1;
            area.AxisX.IsMarginVisible = true;
            area.AxisX.LabelStyle.Angle = -35;
            area.AxisX.Minimum = 0.4;
            area.AxisX.Maximum = top.Count + 0.6;

            Series consumo = NuevaSerie(chart, T(o, "rot_gra_SerieConsumo", "Consumo mensual (u/mes)"), SeriesChartType.Column, ColorConsumo);
            Series stock = NuevaSerie(chart, T(o, "rot_gra_SerieStock", "Stock actual"), SeriesChartType.Column, ColorStock);
            Series minimo = NuevaSerie(chart, T(o, "rot_gra_SerieMinimo", "Stock mínimo"), SeriesChartType.Point, Color.Black);
            consumo["PointWidth"] = "0.7";
            stock["PointWidth"] = "0.7";

            minimo.MarkerStyle = MarkerStyle.Diamond;
            minimo.MarkerSize = (int)Math.Round(9 * o.Escala);
            minimo.MarkerColor = Color.FromArgb(25, 25, 30);
            minimo.MarkerBorderColor = Color.White;
            minimo.MarkerBorderWidth = 1;

            foreach (Series s in new[] { consumo, stock })
            {
                s.IsValueShownAsLabel = true;
                s.LabelForeColor = ColorTexto;
                s.Font = Fuente(o, 7.5f, FontStyle.Regular);
            }
            // Un consumo de 0 no necesita rótulo (la barra no se ve): "0.#;;" deja el cero vacío.
            consumo.LabelFormat = "0.#;;";
            stock.LabelFormat = "0";

            for (int i = 0; i < top.Count; i++)
            {
                BE.ResultadoRotacion f = top[i];
                // Con X numérica las etiquetas de categoría salen de las etiquetas personalizadas del eje.
                area.AxisX.CustomLabels.Add(i + 0.5, i + 1.5, Truncar(f.Nombre));
                consumo.Points.AddXY(i + 1, (double)f.ConsumoMensual);
                stock.Points.AddXY(i + 1, f.StockActual);
                minimo.Points.AddXY(i + 1, f.StockMinimo);
            }

            AgregarLeyenda(chart, o, Docking.Top);
            return chart;
        }

        // 2. Cantidad de vinos por recomendación (dona), con los mismos colores que la grilla.
        internal static Chart Distribucion(List<BE.ResultadoRotacion> filas, Opciones o)
        {
            o = o.ParaGrafico();
            Chart chart = NuevoGrafico(o, T(o, "rot_gra_Titulo2", "Distribución por recomendación"));
            List<KeyValuePair<string, int>> resumen = BLL.RotacionBLL
                .ResumirPorRecomendacion(filas ?? new List<BE.ResultadoRotacion>())
                .Where(p => p.Value > 0)
                .ToList();
            int total = resumen.Sum(p => p.Value);
            if (total == 0) return SinDatos(chart, o);

            Series serie = new Series("distribucion")
            {
                ChartType = SeriesChartType.Doughnut,
                ChartArea = chart.ChartAreas[0].Name,
                Legend = chart.Legends[0].Name,
                IsValueShownAsLabel = true,
                LabelFormat = "0",
                LabelForeColor = Color.White,
                Font = Fuente(o, 11f, FontStyle.Bold),
                BorderColor = ColorFondo,
                BorderWidth = 2
            };
            serie["DoughnutRadius"] = "55";
            serie["PieStartAngle"] = "270";
            chart.Series.Add(serie);

            foreach (KeyValuePair<string, int> par in resumen)
            {
                DataPoint p = serie.Points[serie.Points.AddY(par.Value)];
                p.Color = ColorDeRecomendacion(par.Key);
                double pct = par.Value * 100.0 / total;
                p.LegendText = NombreRecomendacion(o, par.Key) + ": " + par.Value.ToString(o.Cultura)
                    + " (" + pct.ToString("0.#", o.Cultura) + "%)";
            }

            AgregarLeyenda(chart, o, Docking.Right);
            return chart;
        }

        // 3. Unidades sugeridas a reponer por vino (solo los que tienen cantidad sugerida), coloreadas por recomendación.
        internal static Chart UnidadesSugeridas(List<BE.ResultadoRotacion> filas, Opciones o)
        {
            o = o.ParaGrafico();
            List<BE.ResultadoRotacion> con = (filas ?? new List<BE.ResultadoRotacion>())
                .Where(f => f.CantidadSugerida > 0)
                .ToList();
            List<BE.ResultadoRotacion> top = con
                .OrderByDescending(f => f.CantidadSugerida)
                .ThenBy(f => f.Prioridad)
                .ThenBy(f => f.Nombre)
                .Take(MaxVinos)
                .ToList();

            Chart chart = NuevoGrafico(o, Formatear(o, "rot_gra_Titulo3",
                "Unidades sugeridas a reponer ({0} de {1} vinos)", top.Count, con.Count));
            if (top.Count == 0) return SinDatos(chart, o);

            ChartArea area = chart.ChartAreas[0];
            EstilizarEjes(area, o, T(o, "rot_gra_EjeVino", "Vino"), T(o, "rot_gra_EjeUnidades", "Unidades"));
            area.AxisX.Interval = 1;
            area.AxisX.IsMarginVisible = true;

            Series serie = NuevaSerie(chart, "sugeridas", SeriesChartType.Bar, ColorConsumo);
            serie.IsVisibleInLegend = false;
            serie.IsValueShownAsLabel = true;
            serie.LabelForeColor = ColorTexto;
            serie.Font = Fuente(o, 8.5f, FontStyle.Bold);
            serie["PointWidth"] = "0.65";

            // En un gráfico de barras horizontales la primera categoría queda abajo: se carga de
            // menor a mayor para que la mayor cantidad quede arriba (sin invertir el eje, que
            // movería los valores a la parte superior).
            for (int i = top.Count - 1; i >= 0; i--)
            {
                DataPoint p = serie.Points[serie.Points.AddXY(top.Count - i, top[i].CantidadSugerida)];
                p.AxisLabel = Truncar(top[i].Nombre);
                p.Color = ColorDeRecomendacion(top[i].Recomendacion);
            }

            // Leyenda manual: una entrada por recomendación presente.
            Legend leyenda = chart.Legends[0];
            foreach (string rec in new[] { BE.RecomendacionReposicion.Urgente, BE.RecomendacionReposicion.Reponer })
            {
                if (top.Any(f => f.Recomendacion == rec))
                    leyenda.CustomItems.Add(new LegendItem(NombreRecomendacion(o, rec), ColorDeRecomendacion(rec), ""));
            }
            AgregarLeyenda(chart, o, Docking.Top);
            return chart;
        }

        // Imagen PNG en memoria (el PDF la incrusta sin pasar por archivos temporales).
        internal static byte[] APng(Chart chart)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Png);
                return ms.ToArray();
            }
        }

        // ---------------------------------------------------------------- helpers

        internal static string T(Opciones o, string clave, string porDefecto)
        {
            string t = o.Traducir == null ? null : o.Traducir(clave);
            return string.IsNullOrEmpty(t) ? porDefecto : t;
        }

        // Aplica string.Format a una traducción editable. Si el texto guardado tiene llaves mal
        // formadas (se edita en frmIdiomas), cae al formato en español en lugar de romper la pantalla.
        internal static string Formatear(Opciones o, string clave, string formatoPorDefecto, params object[] args)
        {
            try
            {
                return string.Format(o.Cultura, T(o, clave, formatoPorDefecto), args);
            }
            catch (FormatException)
            {
                return string.Format(o.Cultura, formatoPorDefecto, args);
            }
        }

        private static string NombreRecomendacion(Opciones o, string recomendacion)
        {
            string clave;
            switch (recomendacion)
            {
                case BE.RecomendacionReposicion.Urgente: clave = "rot_rec_Urgente"; break;
                case BE.RecomendacionReposicion.Reponer: clave = "rot_rec_Reponer"; break;
                case BE.RecomendacionReposicion.Mantener: clave = "rot_rec_Mantener"; break;
                case BE.RecomendacionReposicion.Sobrestock: clave = "rot_rec_Sobrestock"; break;
                default: return recomendacion;
            }
            return T(o, clave, recomendacion);
        }

        private static string Truncar(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return "";
            return nombre.Length <= MaxLetrasNombre ? nombre : nombre.Substring(0, MaxLetrasNombre - 1).TrimEnd() + "…";
        }

        private static Font Fuente(Opciones o, float tamano, FontStyle estilo)
        {
            Font f = new Font("Segoe UI", tamano * o.Escala, estilo, GraphicsUnit.Point);
            o.Fuentes.Add(f);
            return f;
        }

        private static Chart NuevoGrafico(Opciones o, string titulo)
        {
            Chart chart = new Chart
            {
                Width = o.Ancho,
                Height = o.Alto,
                BackColor = ColorFondo,
                BorderlineColor = Color.FromArgb(205, 205, 210),
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            ChartArea area = new ChartArea("area") { BackColor = ColorFondo };
            area.Area3DStyle.Enable3D = false;
            chart.ChartAreas.Add(area);

            Title t = new Title(titulo, Docking.Top, Fuente(o, 11f, FontStyle.Bold), ColorTexto);
            chart.Titles.Add(t);

            // El gráfico libera sus fuentes al desecharse (Chart.Dispose no lo hace por sí solo).
            List<Font> fuentes = o.Fuentes;
            chart.Disposed += (s, e) =>
            {
                foreach (Font f in fuentes) f.Dispose();
                fuentes.Clear();
            };

            chart.Legends.Add(new Legend("leyenda")
            {
                BackColor = Color.Transparent,
                ForeColor = ColorTexto,
                Font = Fuente(o, 8.5f, FontStyle.Regular)
            });
            return chart;
        }

        private static void AgregarLeyenda(Chart chart, Opciones o, Docking posicion)
        {
            Legend l = chart.Legends[0];
            l.MaximumAutoSize = 65;   // porcentaje del ancho: evita que los textos largos se corten con "…"
            l.Docking = posicion;
            l.Alignment = StringAlignment.Center;
            l.Enabled = true;
        }

        private static Series NuevaSerie(Chart chart, string nombre, SeriesChartType tipo, Color color)
        {
            Series s = new Series(nombre)
            {
                ChartType = tipo,
                ChartArea = chart.ChartAreas[0].Name,
                Legend = chart.Legends[0].Name,
                Color = color
            };
            chart.Series.Add(s);
            return s;
        }

        private static void EstilizarEjes(ChartArea area, Opciones o, string tituloX, string tituloY)
        {
            foreach (Axis eje in new[] { area.AxisX, area.AxisY })
            {
                eje.LabelStyle.Font = Fuente(o, 8f, FontStyle.Regular);
                eje.LabelStyle.ForeColor = ColorTexto;
                eje.LineColor = ColorTextoSuave;
                eje.MajorTickMark.LineColor = ColorTextoSuave;
                eje.TitleFont = Fuente(o, 8.5f, FontStyle.Regular);
                eje.TitleForeColor = ColorTextoSuave;
            }
            area.AxisX.Title = tituloX;
            area.AxisY.Title = tituloY;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = ColorGrilla;
            area.AxisY.LabelStyle.Format = "0";
        }

        // Sin filas para graficar: un texto neutro en lugar de un gráfico roto o vacío.
        private static Chart SinDatos(Chart chart, Opciones o)
        {
            chart.ChartAreas[0].Visible = false;
            chart.Legends[0].Enabled = false;
            chart.Annotations.Add(new TextAnnotation
            {
                Text = T(o, "rot_gra_SinDatos", "Sin datos para graficar"),
                X = 0,
                Y = 0,
                Width = 100,
                Height = 100,
                Alignment = ContentAlignment.MiddleCenter,
                ForeColor = ColorTextoSuave,
                Font = Fuente(o, 10f, FontStyle.Italic)
            });
            return chart;
        }
    }
}
