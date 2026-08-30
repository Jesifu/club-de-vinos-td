using BE;
using System.Collections.Generic;

namespace BLL
{
    public class IdiomaBLL
    {
        private readonly DAL.IdiomaDAL _dal = new DAL.IdiomaDAL();

        public List<IDIOMA> ListarTodos() => _dal.ListarTodos();

        public List<IDIOMA> ListarHabilitados() => _dal.ListarHabilitados();

        public IDIOMA ObtenerPorId(int id) => _dal.ObtenerPorId(id);

        public bool EstaEnUso(int id) => _dal.EstaEnUso(id);

        public IDIOMA Crear(string nombre, bool habilitado)
        {
            int id = _dal.Insertar(nombre, habilitado);
            return new IDIOMA { Id = id, Nombre = nombre, Habilitado = habilitado };
        }

        public void Renombrar(int id, string nombre) =>
            _dal.Renombrar(id, nombre);

        public void ActualizarEstado(int id, bool habilitado) =>
            _dal.ActualizarEstado(id, habilitado);

        public void Eliminar(int id) => _dal.Eliminar(id);

        public void RegistrarControl(string clave, string textoDefault) =>
            _dal.RegistrarControl(clave, textoDefault);

        public Dictionary<string, string> CargarTraducciones(int idiomaId) =>
            _dal.CargarTraducciones(idiomaId);

        public List<CONTROL_IDIOMA> ListarControlesConTraduccion(int idiomaId) =>
            _dal.ListarControlesConTraduccion(idiomaId);

        public void GuardarTraduccion(int idiomaId, int controlId, string texto) =>
            _dal.GuardarTraduccion(idiomaId, controlId, texto);
    }
}
