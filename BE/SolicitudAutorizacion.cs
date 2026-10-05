using System;

namespace BE
{
    // Solicitud de autorización de alta o baja de un vino. Propiedades planas para la grilla
    // y el panel de detalle, más el vino asociado (Vino) para quien necesite sus datos completos.
    public class SolicitudAutorizacion
    {
        public int Id { get; set; }
        public TipoSolicitud Tipo { get; set; }
        public int VinoId { get; set; }
        public string VinoCodigo { get; set; }
        public string VinoNombre { get; set; }
        public string BodegaNombre { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public int EmisorId { get; set; }
        public string EmisorLogin { get; set; }
        public DateTime FechaEmision { get; set; }
        public int? ResolutorId { get; set; }
        public string ResolutorLogin { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string MotivoRechazo { get; set; }
        public int StockActual { get; set; }
        public Vino Vino { get; set; }
    }
}
