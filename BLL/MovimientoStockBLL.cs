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

            _bitacora.RegistrarAccion(sesion.Usuario, "ENTRADA_STOCK:" + v.Codigo + ":" + m.Cantidad);
        }

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
