using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BLL
{
    // N03 — Pedido de Reposición. Reglas RN-31..RN-37 (ver bloque N03 de DAL/script.sql). Los permisos
    // ('Gestionar/Autorizar/Recibir pedidos de reposición') se controlan en la UI, como en el resto del repo.
    public class PedidoReposicionBLL
    {
        // Tope de unidades por línea (pedida y recibida); lo respalda el CHECK CK_PEDREPLIN_TOPE.
        public const int CantidadMaximaPorLinea = 100000;

        private const int MotivoMaxLongitud = 200;
        private const int ObservacionMaxLongitud = 200;

        private readonly DAL.PedidoReposicionDAL _dal = new DAL.PedidoReposicionDAL();
        private readonly DAL.VinoDAL _vinoDal = new DAL.VinoDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        // Genera un pedido por bodega a partir de las filas elegidas del análisis; la cantidad pedida de
        // cada vino es su cantidad sugerida neta (RN-37). Devuelve los pedidos creados (Pendientes).
        public List<PedidoReposicion> Generar(IEnumerable<ResultadoRotacion> filas, USUARIO sesion)
        {
            return Generar(filas, null, sesion);
        }

        // cantidadesPorVino (opcional): VinoId -> cantidad a pedir, para corregir la sugerencia.
        public List<PedidoReposicion> Generar(IEnumerable<ResultadoRotacion> filas,
                                              IDictionary<int, int> cantidadesPorVino, USUARIO sesion)
        {
            if (sesion == null)
                throw new InvalidOperationException("Debe haber un usuario en sesión para generar el pedido.");
            List<ResultadoRotacion> elegidas = filas == null ? new List<ResultadoRotacion>() : filas.ToList();
            if (elegidas.Count == 0)
                throw new InvalidOperationException("RN-31: el pedido debe tener al menos una línea.");
            if (elegidas.GroupBy(f => f.VinoId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("RN-31: un vino no puede repetirse dentro del pedido.");

            // Se valida todo antes de crear el primer pedido, para no dejar pedidos a medias por un dato malo.
            // La bodega se toma del vino en la base (no de la fila de la UI).
            var porBodega = new Dictionary<int, List<PedidoReposicionLinea>>();
            var nombresBodega = new Dictionary<int, string>();
            foreach (ResultadoRotacion f in elegidas)
            {
                int cantidad = cantidadesPorVino != null && cantidadesPorVino.ContainsKey(f.VinoId)
                    ? cantidadesPorVino[f.VinoId] : f.CantidadSugerida;
                Vino v = _vinoDal.ObtenerPorId(f.VinoId);
                ValidarLinea(v, f.Nombre, cantidad);
                nombresBodega[v.BodegaId] = string.IsNullOrEmpty(f.BodegaNombre) ? v.BodegaNombre : f.BodegaNombre;

                List<PedidoReposicionLinea> grupo;
                if (!porBodega.TryGetValue(v.BodegaId, out grupo))
                    porBodega[v.BodegaId] = grupo = new List<PedidoReposicionLinea>();
                grupo.Add(new PedidoReposicionLinea
                {
                    VinoId = v.Id,
                    VinoCodigo = v.Codigo,
                    VinoNombre = v.Nombre,
                    CantidadSugerida = Math.Max(0, f.CantidadSugerida),
                    CantidadPedida = cantidad
                });
            }
            ValidarSinPedidoAbierto(porBodega.Values.SelectMany(g => g).ToList());

            List<PedidoReposicion> creados = new List<PedidoReposicion>();
            foreach (KeyValuePair<int, List<PedidoReposicionLinea>> kv in porBodega)
            {
                try
                {
                    creados.Add(GenerarPedido(kv.Key, kv.Value, null, sesion));
                }
                catch (Exception ex) when (creados.Count > 0)
                {
                    // No se ocultan los pedidos ya creados: la UI los informa junto con el que falló.
                    string bodega;
                    if (!nombresBodega.TryGetValue(kv.Key, out bodega) || string.IsNullOrEmpty(bodega))
                        bodega = "Nº " + kv.Key;
                    string motivo = ex is InvalidOperationException
                        ? ex.Message : "error inesperado al guardar el pedido.";
                    throw new PedidoParcialException(creados, bodega, motivo, ex);
                }
            }
            return creados;
        }

        // Un pedido de UNA bodega. Lanza si alguna línea es de otra bodega (RN-31).
        public PedidoReposicion GenerarPedido(int bodegaId, IList<PedidoReposicionLinea> lineas,
                                              string observacion, USUARIO sesion)
        {
            if (sesion == null)
                throw new InvalidOperationException("Debe haber un usuario en sesión para generar el pedido.");
            if (lineas == null || lineas.Count == 0)
                throw new InvalidOperationException("RN-31: el pedido debe tener al menos una línea.");
            if (lineas.GroupBy(l => l.VinoId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("RN-31: un vino no puede repetirse dentro del pedido.");

            string obs = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (obs != null && obs.Length > ObservacionMaxLongitud)
                throw new InvalidOperationException("La observación no puede superar los " + ObservacionMaxLongitud + " caracteres.");

            foreach (PedidoReposicionLinea l in lineas)
            {
                Vino v = _vinoDal.ObtenerPorId(l.VinoId);
                ValidarLinea(v, l.VinoNombre, l.CantidadPedida);
                if (v.BodegaId != bodegaId)
                    throw new InvalidOperationException(
                        "RN-31: todos los vinos del pedido deben ser de la misma bodega (\"" + v.Nombre + "\" es de otra bodega).");
            }
            ValidarSinPedidoAbierto(lineas);

            int id;
            try
            {
                id = _dal.Insertar(bodegaId, sesion.Id, obs, lineas);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // El índice único filtrado UX_PEDREPLIN_VINO_ABIERTO es la autoridad ante una carrera.
                throw new InvalidOperationException("RN-32: un vino del pedido ya está incluido en otro pedido abierto.");
            }
            if (id == 0)
                throw new InvalidOperationException(
                    "RN-31/RN-32: no se pudo generar el pedido (un vino dejó de estar activo, cambió de bodega o ya está en otro pedido abierto). Actualice el análisis e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "PEDIDO_GENERADO:" + id);
            return _dal.ObtenerPorId(id);
        }

        // estado null = todos los estados.
        public List<PedidoReposicion> Listar(EstadoPedidoReposicion? estado)
        {
            return _dal.Listar(estado);
        }

        public PedidoReposicion Obtener(int pedidoId)
        {
            return _dal.ObtenerPorId(pedidoId);
        }

        public List<PedidoReposicionLinea> ObtenerLineas(int pedidoId)
        {
            return _dal.ObtenerLineas(pedidoId);
        }

        // RN-33: quien autoriza no puede ser quien generó el pedido.
        public void Autorizar(int pedidoId, USUARIO sesion)
        {
            PedidoReposicion p = ObtenerPendiente(pedidoId);
            ValidarSeparacionDeFunciones(p, sesion, "autorizar");

            int filas = _dal.Autorizar(p.Id, sesion.Id);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo autorizar el pedido: cambió de estado o usted lo generó. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "PEDIDO_AUTORIZADO:" + p.Id);
        }

        // RN-33 + RN-34.
        public void Rechazar(int pedidoId, string motivo, USUARIO sesion)
        {
            string motivoLimpio = ValidarMotivo(motivo);
            PedidoReposicion p = ObtenerPendiente(pedidoId);
            ValidarSeparacionDeFunciones(p, sesion, "rechazar");

            int filas = _dal.Rechazar(p.Id, sesion.Id, motivoLimpio);
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo rechazar el pedido: cambió de estado o usted lo generó. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "PEDIDO_RECHAZADO:" + p.Id);
        }

        // RN-35/RN-36: cantidadesPorLinea = id de línea -> cantidad recibida de CADA línea del pedido.
        // Cualquier usuario con el permiso de recepción puede recibir (sin restricción de identidad).
        public void Recibir(int pedidoId, IDictionary<int, int> cantidadesPorLinea, USUARIO sesion)
        {
            if (sesion == null)
                throw new InvalidOperationException("Debe haber un usuario en sesión para recibir el pedido.");
            PedidoReposicion p = _dal.ObtenerPorId(pedidoId);
            if (p == null)
                throw new InvalidOperationException("El pedido no existe.");
            if (p.Estado != EstadoPedidoReposicion.Aprobado)
                throw new InvalidOperationException("RN-35: solo se puede recibir un pedido Aprobado.");
            if (cantidadesPorLinea == null)
                throw new InvalidOperationException("Debe informar la cantidad recibida de cada línea.");

            List<PedidoReposicionLinea> lineas = _dal.ObtenerLineas(p.Id);
            if (lineas.Count != cantidadesPorLinea.Count || lineas.Any(l => !cantidadesPorLinea.ContainsKey(l.Id)))
                throw new InvalidOperationException("RN-35: debe informar la cantidad recibida de cada línea del pedido.");

            foreach (PedidoReposicionLinea l in lineas)
            {
                int recibida = cantidadesPorLinea[l.Id];
                if (recibida < 0 || recibida > l.CantidadPedida)
                    throw new InvalidOperationException(
                        "RN-35: la cantidad recibida de \"" + l.VinoNombre + "\" debe estar entre 0 y " + l.CantidadPedida + ".");
                if (recibida > CantidadMaximaPorLinea)
                    throw new InvalidOperationException(
                        "RN-35: la cantidad recibida de \"" + l.VinoNombre + "\" no puede superar las " +
                        CantidadMaximaPorLinea + " unidades por línea.");
                if (recibida > 0)
                {
                    Vino v = _vinoDal.ObtenerPorId(l.VinoId);
                    if (v == null || v.Estado != EstadoVino.Activo)
                        throw new InvalidOperationException(
                            "El vino \"" + l.VinoNombre + "\" no está activo: no admite entradas de stock.");
                }
            }
            if (cantidadesPorLinea.Values.All(c => c == 0))
                throw new InvalidOperationException("RN-35: al menos una línea debe tener cantidad recibida mayor a cero.");

            int filas = _dal.Recibir(p.Id, sesion.Id, sesion.Usuario, cantidadesPorLinea); // transacción en el SP
            if (filas == 0)
                throw new InvalidOperationException(
                    "No se pudo registrar la recepción: el pedido cambió de estado o los datos no son válidos. Actualice e intente nuevamente.");

            _bitacora.RegistrarAccion(sesion.Usuario, "PEDIDO_RECIBIDO:" + p.Id);
        }

        // ---- validaciones ------------------------------------------------------------------------------

        // RN-31 por línea: vino existente y Activo, cantidad entera > 0.
        private static void ValidarLinea(Vino v, string nombre, int cantidad)
        {
            if (v == null)
                throw new InvalidOperationException("RN-31: el vino \"" + nombre + "\" no existe.");
            if (v.Estado != EstadoVino.Activo)
                throw new InvalidOperationException("RN-31: solo se pueden pedir vinos Activos (\"" + v.Nombre + "\" no lo está).");
            if (cantidad <= 0)
                throw new InvalidOperationException("RN-31: la cantidad pedida de \"" + v.Nombre + "\" debe ser mayor a cero.");
            if (cantidad > CantidadMaximaPorLinea)
                throw new InvalidOperationException(
                    "RN-31: la cantidad pedida de \"" + v.Nombre + "\" no puede superar las " + CantidadMaximaPorLinea + " unidades por línea.");
        }

        // RN-32: ningún vino puede estar en otro pedido abierto (Pendiente o Aprobado).
        private void ValidarSinPedidoAbierto(IEnumerable<PedidoReposicionLinea> lineas)
        {
            HashSet<int> vinos = new HashSet<int>(lineas.Select(l => l.VinoId));
            IEnumerable<PedidoReposicion> abiertos = _dal.Listar(EstadoPedidoReposicion.Pendiente)
                .Concat(_dal.Listar(EstadoPedidoReposicion.Aprobado));
            foreach (PedidoReposicion abierto in abiertos)
            {
                PedidoReposicionLinea repetida = _dal.ObtenerLineas(abierto.Id).FirstOrDefault(l => vinos.Contains(l.VinoId));
                if (repetida != null)
                    throw new InvalidOperationException(
                        "RN-32: \"" + repetida.VinoNombre + "\" ya está en el pedido abierto Nº " + abierto.Id + " (" + abierto.Estado + ").");
            }
        }

        private PedidoReposicion ObtenerPendiente(int pedidoId)
        {
            PedidoReposicion p = _dal.ObtenerPorId(pedidoId);
            if (p == null)
                throw new InvalidOperationException("El pedido no existe.");
            if (p.Estado != EstadoPedidoReposicion.Pendiente)
                throw new InvalidOperationException("El pedido ya fue resuelto (estado: " + p.Estado + ").");
            return p;
        }

        // RN-33
        private static void ValidarSeparacionDeFunciones(PedidoReposicion p, USUARIO sesion, string accion)
        {
            if (sesion == null)
                throw new InvalidOperationException("Debe haber un usuario en sesión.");
            if (p.CreadoPor == sesion.Id)
                throw new InvalidOperationException("RN-33: no puede " + accion + " un pedido que usted mismo generó.");
        }

        // RN-34: obligatorio, recortado, 1 a 200 caracteres.
        private static string ValidarMotivo(string motivo)
        {
            string limpio = motivo == null ? string.Empty : motivo.Trim();
            if (limpio.Length == 0)
                throw new InvalidOperationException("RN-34: debe indicar el motivo del rechazo.");
            if (limpio.Length > MotivoMaxLongitud)
                throw new InvalidOperationException("RN-34: el motivo del rechazo no puede superar los " + MotivoMaxLongitud + " caracteres.");
            return limpio;
        }
    }
}
