using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class BodegaDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public List<BE.Bodega> ListarHabilitadas()
        {
            List<BE.Bodega> lista = new List<BE.Bodega>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("BODEGA_LISTAR_HABILITADAS");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        private BE.Bodega MapearFila(DataRow fila)
        {
            return new BE.Bodega
            {
                Id = Convert.ToInt32(fila["ID"]),
                Nombre = fila["NOMBRE"].ToString(),
                Pais = fila["PAIS"].ToString(),
                Region = fila["REGION"] == DBNull.Value ? null : fila["REGION"].ToString(),
                Habilitado = Convert.ToBoolean(fila["HABILITADO"])
            };
        }
    }
}
