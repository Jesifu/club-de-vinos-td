namespace BE
{
    public class CajaVino
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public int VinoId { get; set; }
        public int Cantidad { get; set; }
        public string NombreSnapshot { get; set; }
        public decimal PrecioSnapshot { get; set; }

        // Carriers de composición efectiva (RN-07) — solo poblados por CAJA_VINO_LISTAR_EFECTIVO.
        public int VinoEfectivoId { get; set; }
        public string NombreEfectivo { get; set; }
        public decimal PrecioEfectivo { get; set; }
        public bool Sustituido { get; set; }
        public int StockDisponible { get; set; }
    }
}
