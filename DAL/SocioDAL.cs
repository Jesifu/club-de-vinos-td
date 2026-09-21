using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class SocioDAL
    {
        private readonly Acceso _acceso = new Acceso();

        // Transaccional: SOCIO_INSERTAR (Leer -> SCOPE_IDENTITY, C1) + N x SOCIO_VARIETAL_INSERTAR.
        // _acceso.Abrir() fuera del try: si Abrir() falla, DeshacerTX()/Cerrar() no deben
        // NRE-ar sobre transaccion/conexion nulos (C8, desvío deliberado de PerfilDAL).
        public int Insertar(Socio s)
        {
            _acceso.Abrir();
            try
            {
                _acceso.IniciarTx();

                DataTable tabla = _acceso.Leer("SOCIO_INSERTAR", ParametrosCabecera(s));
                int socioId = Convert.ToInt32(tabla.Rows[0]["ID"]);

                foreach (string varietal in s.VarietalesPreferidos)
                {
                    _acceso.Escribir("SOCIO_VARIETAL_INSERTAR", new List<SqlParameter>
                    {
                        _acceso.CrearParametro("@socio_id", socioId),
                        _acceso.CrearParametro("@varietal", varietal)
                    });
                }

                _acceso.ConfirmarTX();
                return socioId;
            }
            catch
            {
                _acceso.DeshacerTX();
                throw;
            }
            finally { _acceso.Cerrar(); }
        }

        // Transaccional: SOCIO_ACTUALIZAR + SOCIO_VARIETAL_LIMPIAR + N x SOCIO_VARIETAL_INSERTAR (C2).
        public void Actualizar(Socio s)
        {
            _acceso.Abrir();
            try
            {
                _acceso.IniciarTx();

                List<SqlParameter> parametros = ParametrosCabecera(s);
                parametros.Add(new SqlParameter("@id", SqlDbType.Int) { Value = s.Id });
                parametros.Add(new SqlParameter("@activo", SqlDbType.Bit) { Value = s.Activo ? 1 : 0 });
                _acceso.Escribir("SOCIO_ACTUALIZAR", parametros);

                _acceso.Escribir("SOCIO_VARIETAL_LIMPIAR",
                    new List<SqlParameter> { _acceso.CrearParametro("@socio_id", s.Id) });

                foreach (string varietal in s.VarietalesPreferidos)
                {
                    _acceso.Escribir("SOCIO_VARIETAL_INSERTAR", new List<SqlParameter>
                    {
                        _acceso.CrearParametro("@socio_id", s.Id),
                        _acceso.CrearParametro("@varietal", varietal)
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

        // SOCIO_OBTENER_POR_ID + SOCIO_VARIETAL_LISTAR: dos llamadas separadas porque
        // Acceso.Leer() -> SqlDataAdapter.Fill() solo lee el primer result set (C6).
        public Socio ObtenerPorId(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter> { _acceso.CrearParametro("@id", id) };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SOCIO_OBTENER_POR_ID", parametros);
                if (tabla.Rows.Count == 0) return null;

                Socio s = MapearFila(tabla.Rows[0]);

                DataTable varietales = _acceso.Leer("SOCIO_VARIETAL_LISTAR",
                    new List<SqlParameter> { _acceso.CrearParametro("@socio_id", id) });
                foreach (DataRow fila in varietales.Rows)
                    s.VarietalesPreferidos.Add(fila["VARIETAL"].ToString());

                return s;
            }
            finally { _acceso.Cerrar(); }
        }

        public List<Socio> ListarActivos()
        {
            List<Socio> lista = new List<Socio>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("SOCIO_LISTAR_ACTIVOS");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public List<string> ListarVarietalesDeCatalogo()
        {
            List<string> lista = new List<string>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("VARIETAL_LISTAR_CATALOGO");
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(fila["VARIETAL"].ToString());
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        private List<SqlParameter> ParametrosCabecera(Socio s)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@nombre",     s.Nombre),
                _acceso.CrearParametro("@apellido",   s.Apellido),
                _acceso.CrearParametro("@creado_por", s.CreadoPor)
            };

            // Acceso.CrearParametro no tiene overload para decimal.
            parametros.Add(new SqlParameter("@presupuesto_mensual", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = s.PresupuestoMensual
            });

            // Nullable varchars: construcción manual, per convención documentada en CLAUDE.md.
            parametros.Add(new SqlParameter("@email", SqlDbType.VarChar, 100)
            {
                Value = (object)s.Email ?? DBNull.Value
            });
            parametros.Add(new SqlParameter("@telefono", SqlDbType.VarChar, 20)
            {
                Value = (object)s.Telefono ?? DBNull.Value
            });
            parametros.Add(new SqlParameter("@domicilio", SqlDbType.VarChar, 150)
            {
                Value = (object)s.Domicilio ?? DBNull.Value
            });

            return parametros;
        }

        private Socio MapearFila(DataRow fila)
        {
            return new Socio
            {
                Id = Convert.ToInt32(fila["ID"]),
                Nombre = fila["NOMBRE"].ToString(),
                Apellido = fila["APELLIDO"].ToString(),
                Email = fila["EMAIL"] == DBNull.Value ? null : fila["EMAIL"].ToString(),
                Telefono = fila["TELEFONO"] == DBNull.Value ? null : fila["TELEFONO"].ToString(),
                Domicilio = fila["DOMICILIO"] == DBNull.Value ? null : fila["DOMICILIO"].ToString(),
                PresupuestoMensual = Convert.ToDecimal(fila["PRESUPUESTO_MENSUAL"]),
                Activo = Convert.ToBoolean(fila["ACTIVO"]),
                FechaAlta = Convert.ToDateTime(fila["FECHA_ALTA"]),
                CreadoPor = Convert.ToInt32(fila["CREADO_POR"]),
                CreadoPorLogin = fila["CREADO_POR_LOGIN"].ToString()
            };
        }
    }
}
