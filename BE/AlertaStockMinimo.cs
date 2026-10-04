namespace BE
{
    // Fila de la alerta pasiva de stock mínimo (CU-26): vino activo cuyo stock derivado
    // del kardex está por debajo de su umbral. No se persiste; la arma VINO_LISTAR_STOCK_BAJO.
    public class AlertaStockMinimo
    {
        public int VinoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string BodegaNombre { get; set; }
        public int StockMinimo { get; set; }
        public int StockActual { get; set; }
        public int Faltante { get; set; }
    }
}
