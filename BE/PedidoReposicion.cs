using System;
using System.Collections.Generic;

namespace BE
{
    // Cabecera del Pedido de Reposición (N03). Un pedido es de una sola bodega.
    public class PedidoReposicion
    {
        public int Id { get; set; }
        public int BodegaId { get; set; }
        public string BodegaNombre { get; set; }
        public EstadoPedidoReposicion Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CreadoPor { get; set; }
        public string CreadoPorLogin { get; set; }

        // Quien autorizó o rechazó (RN-33: nunca el creador).
        public int? ResolutorId { get; set; }
        public string ResolutorLogin { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string MotivoRechazo { get; set; }

        public int? RecibidoPor { get; set; }
        public string RecibidoPorLogin { get; set; }
        public DateTime? FechaRecepcion { get; set; }

        public string Observacion { get; set; }

        // Resumen que devuelve el listado.
        public int CantidadLineas { get; set; }
        public int UnidadesPedidas { get; set; }

        public List<PedidoReposicionLinea> Lineas { get; set; } = new List<PedidoReposicionLinea>();
    }
}
