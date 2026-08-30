using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class UsuarioHistorialDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public void Insertar(BE.UsuarioHistorial h)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id",        h.UsuarioId),
                _acceso.CrearParametro("@usuario_login",     h.UsuarioLogin),
                _acceso.CrearParametro("@rol",               h.Rol),
                _acceso.CrearParametro("@bloqueado",         h.Bloqueado ? 1 : 0),
                _acceso.CrearParametro("@intentos_fallidos", h.IntentosFallidos),
                _acceso.CrearParametro("@perfiles",          h.Perfiles ?? ""),
                _acceso.CrearParametro("@nombre",            h.Nombre ?? ""),
                _acceso.CrearParametro("@apellido",          h.Apellido ?? ""),
                _acceso.CrearParametro("@telefono",          h.Telefono ?? ""),
                _acceso.CrearParametro("@email",             h.Email ?? ""),
                _acceso.CrearParametro("@realizado_por",     h.RealizadoPor),
                _acceso.CrearParametro("@tipo_cambio",       h.TipoCambio)
            };

            SqlParameter pOrigen = new SqlParameter("@version_origen",
                (object)h.VersionOrigen ?? DBNull.Value);
            pOrigen.SqlDbType = SqlDbType.Int;
            parametros.Add(pOrigen);

            SqlParameter pRolId = new SqlParameter("@rol_id",
                (object)h.RolId ?? DBNull.Value);
            pRolId.SqlDbType = SqlDbType.Int;
            parametros.Add(pRolId);

            try
            {
                _acceso.Abrir();
                _acceso.Escribir("USUARIO_HISTORIAL_INSERTAR", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        public List<BE.UsuarioHistorial> ListarPorUsuario(int usuarioId)
        {
            List<BE.UsuarioHistorial> lista = new List<BE.UsuarioHistorial>();
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("USUARIO_HISTORIAL_LISTAR", parametros);
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public BE.PaginaResultado<BE.UsuarioHistorial> ListarPorUsuarioPaginado(int usuarioId, int pagina, int tamanio)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@usuario_id", usuarioId),
                _acceso.CrearParametro("@pagina", pagina),
                _acceso.CrearParametro("@tamanio", tamanio)
            };

            BE.PaginaResultado<BE.UsuarioHistorial> resultado = new BE.PaginaResultado<BE.UsuarioHistorial>
            {
                Pagina = pagina,
                Tamanio = tamanio
            };

            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("USUARIO_HISTORIAL_LISTAR_PAGINADO", parametros);
                foreach (DataRow fila in tabla.Rows)
                    resultado.Items.Add(MapearFila(fila));
                resultado.TotalFilas = tabla.Rows.Count > 0
                    ? Convert.ToInt32(tabla.Rows[0]["TOTAL_FILAS"])
                    : 0;
            }
            finally { _acceso.Cerrar(); }
            return resultado;
        }

        public BE.UsuarioHistorial ObtenerPorId(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id", id)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("USUARIO_HISTORIAL_OBTENER", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        private BE.UsuarioHistorial MapearFila(DataRow fila)
        {
            return new BE.UsuarioHistorial
            {
                Id = Convert.ToInt32(fila["ID"]),
                UsuarioId = Convert.ToInt32(fila["USUARIO_ID"]),
                UsuarioLogin = fila["USUARIO_LOGIN"].ToString(),
                Rol = fila["ROL"].ToString(),
                RolId = fila["ROL_ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["ROL_ID"]),
                Bloqueado = Convert.ToBoolean(fila["BLOQUEADO"]),
                IntentosFallidos = Convert.ToInt32(fila["INTENTOS_FALLIDOS"]),
                Perfiles = fila["PERFILES"].ToString(),
                Nombre = fila["NOMBRE"].ToString(),
                Apellido = fila["APELLIDO"].ToString(),
                Telefono = fila["TELEFONO"] == DBNull.Value ? null : fila["TELEFONO"].ToString(),
                Email = fila["EMAIL"] == DBNull.Value ? null : fila["EMAIL"].ToString(),
                FechaCambio = Convert.ToDateTime(fila["FECHA_CAMBIO"]),
                RealizadoPor = fila["REALIZADO_POR"].ToString(),
                TipoCambio = fila["TIPO_CAMBIO"].ToString(),
                VersionOrigen = fila["VERSION_ORIGEN"] == DBNull.Value
                                   ? (int?)null
                                   : Convert.ToInt32(fila["VERSION_ORIGEN"])
            };
        }
    }
}
