using BE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class SocioBLL
    {
        private readonly DAL.SocioDAL _dal = new DAL.SocioDAL();
        private readonly BitacoraBLL _bitacora = new BitacoraBLL();

        public List<Socio> ListarActivos()
        {
            return _dal.ListarActivos();
        }

        public Socio ObtenerPorId(int id)
        {
            return _dal.ObtenerPorId(id);
        }

        public List<string> ListarVarietalesDeCatalogo()
        {
            return _dal.ListarVarietalesDeCatalogo();
        }

        public void Registrar(Socio s, USUARIO sesion)
        {
            Validar(s);

            s.CreadoPor = sesion.Id;
            s.Activo = true;
            s.VarietalesPreferidos = s.VarietalesPreferidos.Distinct().ToList();

            _dal.Insertar(s); // La transacción vive en el DAL (C2)

            _bitacora.RegistrarAccion(sesion.Usuario, "ALTA_SOCIO:" + s.Apellido);
        }

        public void Actualizar(Socio s, USUARIO sesion)
        {
            Validar(s);

            s.VarietalesPreferidos = s.VarietalesPreferidos.Distinct().ToList();

            _dal.Actualizar(s); // La transacción vive en el DAL (C2)

            _bitacora.RegistrarAccion(sesion.Usuario, "EDICION_SOCIO:" + s.Id);
        }

        private void Validar(Socio s)
        {
            if (string.IsNullOrWhiteSpace(s.Nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(s.Apellido))
                throw new InvalidOperationException("El apellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(s.Domicilio))
                throw new InvalidOperationException("El domicilio es obligatorio.");
            if (s.PresupuestoMensual <= 0)
                throw new InvalidOperationException("El presupuesto mensual debe ser mayor a cero.");
        }
    }
}
