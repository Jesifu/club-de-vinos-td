using BE;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BitacoraBLL
    {
        private readonly DAL.BitacoraDAL _dal = new DAL.BitacoraDAL();

        public void RegistrarLogin(string usuario)
        {
            _dal.Registrar(usuario, "LOGIN");
        }

        public void RegistrarLogout(string usuario)
        {
            _dal.Registrar(usuario, "LOGOUT");
        }

        public void RegistrarAccion(string usuario, string accion)
        {
            _dal.Registrar(usuario, accion);
        }

        public BE.PaginaResultado<BE.BITACORA> ListarPaginado(string usuario, string accion,
            DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int tamanio)
        {
            return _dal.ListarPaginado(usuario, accion, fechaDesde, fechaHasta, pagina, tamanio);
        }

        public List<string> ListarUsuariosDistinct()
        {
            return _dal.ListarUsuariosDistinct();
        }

        public List<string> ListarAccionesDistinct()
        {
            return _dal.ListarAccionesDistinct();
        }
    }
}