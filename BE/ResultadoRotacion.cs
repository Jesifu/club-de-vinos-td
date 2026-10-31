namespace BE
{
    // Textos de la clasificación por rotación (N03).
    public static class ClasificacionRotacion
    {
        public const string Alta = "Alta rotación";
        public const string Media = "Rotación media";
        public const string Baja = "Baja rotación";
        public const string SinMovimiento = "Sin movimiento";
    }

    // Textos de la recomendación de reposición (N03).
    public static class RecomendacionReposicion
    {
        public const string Urgente = "Reponer urgente";
        public const string Reponer = "Reponer";
        public const string Mantener = "Mantener";
        public const string Sobrestock = "Sobrestock / no reponer";
    }

    // Fila del análisis de rotación de un vino activo. La arma ROTACION_ANALIZAR (campos de
    // datos) y RotacionBLL.Evaluar completa los derivados (consumo, cobertura, clasificación
    // y recomendación). No se persiste.
    public class ResultadoRotacion
    {
        public int VinoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string BodegaNombre { get; set; }
        public int StockMinimo { get; set; }
        public int StockActual { get; set; }

        // Salidas por cajas despachadas dentro del período.
        public int UnidadesVendidas { get; set; }

        // Salidas por ajustes de inventario (roturas, mermas) dentro del período.
        public int UnidadesPerdidas { get; set; }

        // Socios distintos que recibieron el vino (efectivo, con sustituciones) en el período.
        public int SociosAtendidos { get; set; }

        // Derivados (BLL).
        public decimal ConsumoMensual { get; set; }

        // null cuando no hay consumo: la cobertura no se puede calcular.
        public decimal? MesesCobertura { get; set; }

        public string Clasificacion { get; set; }
        public string Recomendacion { get; set; }

        // Unidades sugeridas a reponer (0 salvo recomendación Urgente o Reponer). La calcula
        // RotacionBLL.Evaluar.
        public int CantidadSugerida { get; set; }

        // 0 = más urgente. Sirve para ordenar el informe.
        public int Prioridad { get; set; }
    }
}
