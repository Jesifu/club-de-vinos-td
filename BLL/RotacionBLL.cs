using System;
using System.Collections.Generic;
using System.Linq;
using BE;

namespace BLL
{
    // N03 — Análisis de rotación y reposición de stock. La clasificación y la recomendación
    // son funciones puras sobre las filas de la DAL y la configuración serializada.
    public class RotacionBLL
    {
        private const decimal DiasPorMes = 30m;

        private readonly DAL.RotacionDAL _dal = new DAL.RotacionDAL();

        public List<ResultadoRotacion> Analizar(DateTime desde, DateTime hasta, ConfiguracionRotacion config)
        {
            Validar(config);
            if (desde.Date > hasta.Date)
                throw new InvalidOperationException("La fecha desde no puede ser posterior a la fecha hasta.");

            List<ResultadoRotacion> filas = _dal.Analizar(desde, hasta);
            int dias = CantidadDias(desde, hasta);
            foreach (ResultadoRotacion fila in filas)
                Evaluar(fila, dias, config);

            return filas
                .OrderBy(f => f.Prioridad)
                .ThenBy(f => f.MesesCobertura ?? decimal.MaxValue)
                .ThenBy(f => f.Nombre)
                .ToList();
        }

        // Valida coherencia de umbrales; lanza InvalidOperationException con el motivo.
        public static void Validar(ConfiguracionRotacion c)
        {
            if (c == null)
                throw new InvalidOperationException("La configuración de rotación es obligatoria.");
            if (c.DiasAnalisis < 1 || c.DiasAnalisis > 3650)
                throw new InvalidOperationException("Los días de análisis deben estar entre 1 y 3650.");
            if (c.UmbralBajaRotacion <= 0)
                throw new InvalidOperationException("El umbral de baja rotación debe ser mayor a cero.");
            if (c.UmbralAltaRotacion <= c.UmbralBajaRotacion)
                throw new InvalidOperationException("El umbral de alta rotación debe ser mayor al de baja rotación.");
            if (c.MesesCoberturaUrgente <= 0)
                throw new InvalidOperationException("La cobertura de reposición urgente debe ser mayor a cero.");
            if (c.MesesCoberturaReposicion <= c.MesesCoberturaUrgente)
                throw new InvalidOperationException("La cobertura de reposición debe ser mayor a la de reposición urgente.");
            if (c.MesesCoberturaSobrestock <= c.MesesCoberturaReposicion)
                throw new InvalidOperationException("La cobertura de sobrestock debe ser mayor a la de reposición.");
        }

        // Días del período, ambos extremos inclusive (mínimo 1).
        public static int CantidadDias(DateTime desde, DateTime hasta)
        {
            return Math.Max(1, (hasta.Date - desde.Date).Days + 1);
        }

        // Completa consumo mensual, cobertura, clasificación, recomendación y prioridad.
        public static void Evaluar(ResultadoRotacion r, int dias, ConfiguracionRotacion c)
        {
            decimal meses = Math.Max(1, dias) / DiasPorMes;
            r.ConsumoMensual = r.UnidadesVendidas / meses;
            r.MesesCobertura = r.ConsumoMensual > 0 ? r.StockActual / r.ConsumoMensual : (decimal?)null;

            if (r.UnidadesVendidas <= 0)
                r.Clasificacion = ClasificacionRotacion.SinMovimiento;
            else if (r.ConsumoMensual >= c.UmbralAltaRotacion)
                r.Clasificacion = ClasificacionRotacion.Alta;
            else if (r.ConsumoMensual < c.UmbralBajaRotacion)
                r.Clasificacion = ClasificacionRotacion.Baja;
            else
                r.Clasificacion = ClasificacionRotacion.Media;

            if (r.StockActual <= r.StockMinimo
                || (r.MesesCobertura.HasValue && r.MesesCobertura.Value < c.MesesCoberturaUrgente))
            {
                r.Recomendacion = RecomendacionReposicion.Urgente;
                r.Prioridad = 0;
            }
            else if (!r.MesesCobertura.HasValue || r.MesesCobertura.Value > c.MesesCoberturaSobrestock)
            {
                // Sin consumo pero con stock sobre el mínimo, o cobertura excesiva.
                r.Recomendacion = RecomendacionReposicion.Sobrestock;
                r.Prioridad = 3;
            }
            else if (r.MesesCobertura.Value < c.MesesCoberturaReposicion)
            {
                r.Recomendacion = RecomendacionReposicion.Reponer;
                r.Prioridad = 1;
            }
            else
            {
                r.Recomendacion = RecomendacionReposicion.Mantener;
                r.Prioridad = 2;
            }
        }

        // Cantidad de vinos por recomendación, en orden de urgencia (incluye las que tienen 0).
        public static List<KeyValuePair<string, int>> ResumirPorRecomendacion(IEnumerable<ResultadoRotacion> filas)
        {
            List<ResultadoRotacion> lista = filas.ToList();
            string[] orden =
            {
                RecomendacionReposicion.Urgente,
                RecomendacionReposicion.Reponer,
                RecomendacionReposicion.Mantener,
                RecomendacionReposicion.Sobrestock
            };
            return orden
                .Select(t => new KeyValuePair<string, int>(t, lista.Count(f => f.Recomendacion == t)))
                .ToList();
        }
    }
}
