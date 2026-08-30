using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class PerfilDAL
    {
        private readonly Acceso _acceso = new Acceso();

        public List<NodoPermiso> ListarRoles()
        {
            List<NodoPermiso> lista = new List<NodoPermiso>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROL_LISTAR_TODOS");
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new Rol
                    {
                        Id = Convert.ToInt32(fila["ID"]),
                        Nombre = fila["NOMBRE"].ToString(),
                        PadreId = fila["PADRE_ID"] == DBNull.Value
                                      ? (int?)null
                                      : Convert.ToInt32(fila["PADRE_ID"]),
                        Protegido = Convert.ToBoolean(fila["PROTEGIDO"])
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public List<Permiso> ListarPermisosDisponibles()
        {
            List<Permiso> lista = new List<Permiso>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("PERMISO_LISTAR_TODOS");
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new Permiso
                    {
                        Id = Convert.ToInt32(fila["ID"]),
                        Nombre = fila["NOMBRE"].ToString()
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public List<Permiso> ListarRolPermisos()
        {
            List<Permiso> lista = new List<Permiso>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROL_PERMISO_LISTAR_TODOS");
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new Permiso
                    {
                        Id = Convert.ToInt32(fila["PERMISO_ID"]),
                        Nombre = fila["NOMBRE"].ToString(),
                        PadreId = Convert.ToInt32(fila["ROL_ID"])
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public void GuardarPermisosDeRol(int rolId, List<int> permisoIds)
        {
            try
            {
                _acceso.Abrir();
                _acceso.IniciarTx();

                _acceso.Escribir("ROL_PERMISO_LIMPIAR",
                    new List<SqlParameter> { _acceso.CrearParametro("@rol_id", rolId) });

                foreach (int pid in permisoIds)
                {
                    _acceso.Escribir("ROL_PERMISO_INSERTAR", new List<SqlParameter>
                    {
                        _acceso.CrearParametro("@rol_id",     rolId),
                        _acceso.CrearParametro("@permiso_id", pid)
                    });
                }

                _acceso.ConfirmarTX();
            }
            catch
            {
                _acceso.DeshacerTX();
                throw;
            }
            finally { _acceso.Cerrar(); }
        }

        public int Insertar(string nombre, int? padreId)
        {
            SqlParameter paramPadre = new SqlParameter("@padre_id", DbType.Int32)
            {
                Value = padreId.HasValue ? (object)padreId.Value : DBNull.Value
            };
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@nombre", nombre),
                paramPadre
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROL_INSERTAR", parametros);
                return Convert.ToInt32(tabla.Rows[0]["ID"]);
            }
            finally { _acceso.Cerrar(); }
        }

        public bool TieneUsuariosAsignados(int rolId)
        {
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROL_TIENE_USUARIOS",
                    new List<SqlParameter> { _acceso.CrearParametro("@rol_id", rolId) });
                return Convert.ToInt32(tabla.Rows[0]["TOTAL"]) > 0;
            }
            finally { _acceso.Cerrar(); }
        }

        public void CambiarPadre(int rolId, int? nuevoPadreId)
        {
            SqlParameter paramPadre = new SqlParameter("@padre_id", DbType.Int32)
            {
                Value = nuevoPadreId.HasValue ? (object)nuevoPadreId.Value : DBNull.Value
            };
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@id", rolId),
                paramPadre
            };
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("ROL_CAMBIAR_PADRE", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        public void Eliminar(int id)
        {
            try
            {
                _acceso.Abrir();
                _acceso.Escribir("ROL_ELIMINAR",
                    new List<SqlParameter> { _acceso.CrearParametro("@id", id) });
            }
            finally { _acceso.Cerrar(); }
        }

        public List<Rol> ListarParaCombo()
        {
            List<Rol> lista = new List<Rol>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("ROL_LISTAR_PARA_COMBO");
                foreach (DataRow fila in tabla.Rows)
                {
                    lista.Add(new Rol
                    {
                        Id = Convert.ToInt32(fila["ID"]),
                        Nombre = fila["NOMBRE"].ToString()
                    });
                }
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }
    }
}
