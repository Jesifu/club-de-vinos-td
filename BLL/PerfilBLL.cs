using BE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class PerfilBLL
    {
        private readonly DAL.PerfilDAL _dal = new DAL.PerfilDAL();

        public List<NodoPermiso> ObtenerArbol()
        {
            List<NodoPermiso> roles = _dal.ListarRoles();
            Dictionary<int, NodoPermiso> mapa = new Dictionary<int, NodoPermiso>();
            foreach (NodoPermiso r in roles) mapa[r.Id] = r;

            List<NodoPermiso> raices = new List<NodoPermiso>();
            foreach (NodoPermiso r in roles)
            {
                if (r.PadreId == null)
                    raices.Add(r);
                else if (mapa.ContainsKey(r.PadreId.Value))
                    mapa[r.PadreId.Value].Agregar(r);
            }

            foreach (Permiso p in _dal.ListarRolPermisos())
            {
                if (p.PadreId.HasValue && mapa.ContainsKey(p.PadreId.Value))
                    mapa[p.PadreId.Value].Agregar(p);
            }

            return raices;
        }

        public List<Permiso> ObtenerPermisosDisponibles()
        {
            return _dal.ListarPermisosDisponibles();
        }

        public List<Rol> ListarRolesParaCombo()
        {
            return _dal.ListarParaCombo();
        }

        public void ActualizarPermisosDeRol(int rolId, List<int> permisoIds)
        {
            _dal.GuardarPermisosDeRol(rolId, permisoIds);
        }

        public NodoPermiso AgregarRol(string nombre, int? padreId)
        {
            int id = _dal.Insertar(nombre, padreId);
            return new Rol { Id = id, Nombre = nombre, PadreId = padreId };
        }

        public void CambiarPadre(int rolId, int? nuevoPadreId)
        {
            if (nuevoPadreId.HasValue)
            {
                List<NodoPermiso> todos = _dal.ListarRoles();
                if (GenerariaCiclo(rolId, nuevoPadreId.Value, todos))
                    throw new InvalidOperationException(
                        "No se puede asignar ese padre: generaría una referencia circular.");

                int? padreActual = todos.FirstOrDefault(n => n.Id == rolId)?.PadreId;
                if (EsAncestroDelPadreActual(padreActual, nuevoPadreId.Value, todos))
                    throw new InvalidOperationException(
                        "No se puede asignar como padre a un ancestro del padre actual del rol.");
            }
            _dal.CambiarPadre(rolId, nuevoPadreId);
        }

        public void Eliminar(int id)
        {
            NodoPermiso nodo = _dal.ListarRoles().FirstOrDefault(n => n.Id == id);
            if (nodo is Rol r && r.Protegido)
                throw new InvalidOperationException(
                    "Este rol es del sistema y no puede eliminarse.");
            if (_dal.TieneUsuariosAsignados(id))
                throw new InvalidOperationException(
                    "No se puede eliminar un rol que tiene usuarios asignados. Reasignálos primero.");
            _dal.Eliminar(id);
        }

        private bool GenerariaCiclo(int rolId, int candidatoPadreId, List<NodoPermiso> todos)
        {
            int? actual = candidatoPadreId;
            while (actual.HasValue)
            {
                if (actual.Value == rolId) return true;
                actual = todos.FirstOrDefault(n => n.Id == actual.Value)?.PadreId;
            }
            return false;
        }

        private bool EsAncestroDelPadreActual(int? padreActualId, int candidatoPadreId, List<NodoPermiso> todos)
        {
            int? actual = padreActualId.HasValue
                ? todos.FirstOrDefault(n => n.Id == padreActualId.Value)?.PadreId
                : null;
            while (actual.HasValue)
            {
                if (actual.Value == candidatoPadreId) return true;
                actual = todos.FirstOrDefault(n => n.Id == actual.Value)?.PadreId;
            }
            return false;
        }
    }
}
