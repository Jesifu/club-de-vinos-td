using System;

namespace BE
{
    public class Vino
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int BodegaId { get; set; }
        public string BodegaNombre { get; set; }
        public string Varietal { get; set; }
        public int Aniada { get; set; }
        public decimal Precio { get; set; }
        public int StockMinimo { get; set; }
        public EstadoVino Estado { get; set; }
        public string Maridaje { get; set; }
        public int? Puntaje { get; set; }
        public int CreadoPor { get; set; }
        public string CreadoPorLogin { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? AutorizadoPor { get; set; }
        public string AutorizadoPorLogin { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
        // Descontinuación (CU-24/25): solicitud pendiente mientras BajaSolicitadaPor != null y Estado == Activo.
        public int? BajaSolicitadaPor { get; set; }
        public string BajaSolicitadaPorLogin { get; set; }
        public DateTime? FechaSolicitudBaja { get; set; }
        public int? DescontinuadoPor { get; set; }
        public DateTime? FechaDescontinuacion { get; set; }
        // Última solicitud de autorización del vino (R7). Solo la completa VINO_LISTAR_CATALOGO;
        // en el resto de las consultas quedan en null.
        public TipoSolicitud? UltimaSolicitudTipo { get; set; }
        public EstadoSolicitud? UltimaSolicitudEstado { get; set; }
        public string UltimoMotivoRechazo { get; set; }
    }
}
