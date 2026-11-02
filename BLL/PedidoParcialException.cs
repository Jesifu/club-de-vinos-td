using BE;
using System;
using System.Collections.Generic;

namespace BLL
{
    // N03 — Se lanza cuando Generar creó uno o más pedidos (uno por bodega) y falló la creación de uno
    // posterior. Conserva los pedidos ya creados para que la UI no los oculte.
    public class PedidoParcialException : InvalidOperationException
    {
        public List<PedidoReposicion> Creados { get; private set; }
        public string BodegaFallida { get; private set; }
        public string Motivo { get; private set; }

        public PedidoParcialException(List<PedidoReposicion> creados, string bodegaFallida, string motivo, Exception causa)
            : base(ArmarMensaje(creados, bodegaFallida, motivo), causa)
        {
            Creados = creados;
            BodegaFallida = bodegaFallida;
            Motivo = motivo;
        }

        private static string ArmarMensaje(List<PedidoReposicion> creados, string bodega, string motivo)
        {
            string numeros = string.Join(", ", creados.ConvertAll(p => p.Id.ToString()));
            return (creados.Count == 1 ? "Se creó el pedido Nº " : "Se crearon los pedidos Nº ") + numeros +
                   " pero falló el de la bodega " + bodega + ": " + motivo;
        }
    }
}
