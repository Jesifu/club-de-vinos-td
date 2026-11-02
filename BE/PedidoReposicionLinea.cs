namespace BE
{
    // Línea de un Pedido de Reposición. CantidadSugerida es el snapshot de la sugerencia del
    // análisis al generar el pedido; CantidadRecibida es null hasta la recepción.
    public class PedidoReposicionLinea
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int VinoId { get; set; }
        public string VinoCodigo { get; set; }
        public string VinoNombre { get; set; }
        public int CantidadSugerida { get; set; }
        public int CantidadPedida { get; set; }
        public int? CantidadRecibida { get; set; }
    }
}
