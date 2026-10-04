using BE;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class MovimientoStockBLL
    {
        private readonly DAL.MovimientoStockDAL _dal = new DAL.MovimientoStockDAL();
        private readonly DAL.VinoDAL _vinoDal = new DAL.VinoDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        public void RegistrarEntrada(MovimientoStock m, USUARIO sesion)
        {
            if (m.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(m.Motivo))
                throw new InvalidOperationException("El motivo es obligatorio.");

            Vino v = _vinoDal.ObtenerPorId(m.VinoId);
            if (v == null || !v.AutorizadoPor.HasValue || v.Estado != EstadoVino.Activo)
                throw new InvalidOperationException("El vino debe estar autorizado para registrar movimientos de stock.");

            m.Tipo = TipoMovimiento.Entrada;
            m.Fecha = DateTime.Now;
            m.Responsable = sesion.Usuario;

            _dal.Insertar(m);

            // Un ajuste positivo (CU-27) reutiliza este flujo; solo cambia la traza de bitácora.
            if (m.ReferenciaTipo == ReferenciaAjuste)
                _bitacora.RegistrarAccion(sesion.Usuario, "AJUSTE_STOCK:" + v.Codigo + ":+" + m.Cantidad);
            else
                _bitacora.RegistrarAccion(sesion.Usuario, "ENTRADA_STOCK:" + v.Codigo + ":" + m.Cantidad);
        }

        // CU-27, sentido negativo. Se guarda como TIPO 'Salida' con REFERENCIA_TIPO 'AJUSTE', de modo que
        // la expresión de stock derivado (Entrada suma, el resto resta) sigue siendo válida en todos los SP.
        public void RegistrarSalida(MovimientoStock m, USUARIO sesion)
        {
            if (m.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
            if (string.IsNullOrWhiteSpace(m.Motivo))
                throw new InvalidOperationException("El motivo es obligatorio.");

            Vino v = _vinoDal.ObtenerPorId(m.VinoId);
            if (v == null || !v.AutorizadoPor.HasValue || v.Estado != EstadoVino.Activo)
                throw new InvalidOperationException("El vino debe estar autorizado para registrar movimientos de stock.");

            // RN-03, capa BLL: mensaje claro antes de tocar la escritura.
            int stock = _dal.ObtenerStockActual(m.VinoId);
            if (stock < m.Cantidad)
                throw new InvalidOperationException("Stock insuficiente: el ajuste dejaría el stock por debajo de cero (stock actual: " + stock + ").");

            m.Tipo = TipoMovimiento.Salida;
            m.Fecha = DateTime.Now;
            m.Responsable = sesion.Usuario;
            m.ReferenciaTipo = ReferenciaAjuste;
            m.ReferenciaId = null;

            // RN-03, backstop SP: inserta solo si el stock alcanza (atómico). 0 filas = rechazado.
            int filas = _dal.InsertarSalida(m);
            if (filas == 0)
                throw new InvalidOperationException("Stock insuficiente: el ajuste dejaría el stock por debajo de cero.");

            _bitacora.RegistrarAccion(sesion.Usuario, "AJUSTE_STOCK:" + v.Codigo + ":-" + m.Cantidad);
        }

        // Valor de REFERENCIA_TIPO que identifica los movimientos de ajuste de inventario.
        public const string ReferenciaAjuste = "AJUSTE";

        public int ObtenerStockActual(int vinoId)
        {
            return _dal.ObtenerStockActual(vinoId);
        }

        public List<MovimientoStock> ObtenerMovimientos(int vinoId)
        {
            return _dal.ListarPorVino(vinoId);
        }
    }
}
