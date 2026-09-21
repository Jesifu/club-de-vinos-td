using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class CajaMensualDAL
    {
        private readonly Acceso _acceso = new Acceso();

        // Transaccional: CAJA_INSERTAR (Leer -> SCOPE_IDENTITY, C1) + N x CAJA_VINO_INSERTAR (RN-04 snapshots).
        // _acceso.Abrir() fuera del try (C8).
        public int Armar(CajaMensual caja)
        {
            _acceso.Abrir();
            try
            {
                _acceso.IniciarTx();

                List<SqlParameter> parametrosCabecera = new List<SqlParameter>
                {
                    _acceso.CrearParametro("@socio_id",   caja.SocioId),
                    _acceso.CrearParametro("@periodo",    caja.Periodo),
                    _acceso.CrearParametro("@armado_por", caja.ArmadoPor)
                };
                parametrosCabecera.Add(new SqlParameter("@presupuesto_snapshot", SqlDbType.Decimal)
                {
                    Precision = 10,
                    Scale = 2,
                    Value = caja.PresupuestoSnapshot
                });

                DataTable tabla = _acceso.Leer("CAJA_INSERTAR", parametrosCabecera);
                int cajaId = Convert.ToInt32(tabla.Rows[0]["ID"]);

                foreach (CajaVino linea in caja.Lineas)
                {
                    List<SqlParameter> parametrosLinea = new List<SqlParameter>
                    {
                        _acceso.CrearParametro("@caja_id",  cajaId),
                        _acceso.CrearParametro("@vino_id",  linea.VinoId),
                        _acceso.CrearParametro("@cantidad", linea.Cantidad),
                        _acceso.CrearParametro("@nombre_snapshot", linea.NombreSnapshot)
                    };
                    parametrosLinea.Add(new SqlParameter("@precio_snapshot", SqlDbType.Decimal)
                    {
                        Precision = 10,
                        Scale = 2,
                        Value = linea.PrecioSnapshot
                    });
                    _acceso.Escribir("CAJA_VINO_INSERTAR", parametrosLinea);
                }

                _acceso.ConfirmarTX();
                return cajaId;
            }
            catch
            {
                _acceso.DeshacerTX();
                throw;
            }
            finally { _acceso.Cerrar(); }
        }

        public CajaMensual ObtenerPorId(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter> { _acceso.CrearParametro("@id", id) };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("CAJA_OBTENER_POR_ID", parametros);
                return tabla.Rows.Count > 0 ? MapearFila(tabla.Rows[0]) : null;
            }
            finally { _acceso.Cerrar(); }
        }

        // RN-07: composición DERIVADA vía CTE sobre SUSTITUCION, jamás un UPDATE de la línea original.
        public List<CajaVino> ListarComposicionEfectiva(int cajaId)
        {
            List<CajaVino> lista = new List<CajaVino>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("CAJA_VINO_LISTAR_EFECTIVO",
                    new List<SqlParameter> { _acceso.CrearParametro("@caja_id", cajaId) });
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFilaEfectiva(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        public List<CajaMensual> ListarPorEstado(string estado)
        {
            List<CajaMensual> lista = new List<CajaMensual>();
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("CAJA_LISTAR_POR_ESTADO",
                    new List<SqlParameter> { _acceso.CrearParametro("@estado", estado) });
                foreach (DataRow fila in tabla.Rows)
                    lista.Add(MapearFila(fila));
            }
            finally { _acceso.Cerrar(); }
            return lista;
        }

        // CAJA_DESPACHAR: primera SP transaccional del repo (C4/C5/C6). Devuelve un único
        // result set (FILAS) porque Acceso.Leer() -> Fill() solo lee el primero.
        public int Despachar(int cajaId, int despachadoPor, string responsableLogin)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@caja_id",           cajaId),
                _acceso.CrearParametro("@despachado_por",    despachadoPor),
                _acceso.CrearParametro("@responsable_login", responsableLogin)
            };
            try
            {
                _acceso.Abrir();
                DataTable tabla = _acceso.Leer("CAJA_DESPACHAR", parametros);
                return Convert.ToInt32(tabla.Rows[0]["FILAS"]);
            }
            finally { _acceso.Cerrar(); }
        }

        public int Cancelar(int cajaId, string motivo, int canceladaPor)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _acceso.CrearParametro("@caja_id",       cajaId),
                _acceso.CrearParametro("@motivo",        motivo),
                _acceso.CrearParametro("@cancelada_por", canceladaPor)
            };
            try
            {
                _acceso.Abrir();
                return _acceso.Escribir("CAJA_CANCELAR", parametros);
            }
            finally { _acceso.Cerrar(); }
        }

        private CajaMensual MapearFila(DataRow fila)
        {
            return new CajaMensual
            {
                Id = Convert.ToInt32(fila["ID"]),
                SocioId = Convert.ToInt32(fila["SOCIO_ID"]),
                SocioNombre = fila["SOCIO_NOMBRE"].ToString(),
                SocioDomicilio = fila["SOCIO_DOMICILIO"] == DBNull.Value ? null : fila["SOCIO_DOMICILIO"].ToString(),
                SocioTelefono = fila["SOCIO_TELEFONO"] == DBNull.Value ? null : fila["SOCIO_TELEFONO"].ToString(),
                Periodo = fila["PERIODO"].ToString(),
                Estado = (EstadoCaja)Enum.Parse(typeof(EstadoCaja), fila["ESTADO"].ToString()),
                PresupuestoSnapshot = Convert.ToDecimal(fila["PRESUPUESTO_SNAPSHOT"]),
                FechaArmado = Convert.ToDateTime(fila["FECHA_ARMADO"]),
                ArmadoPor = Convert.ToInt32(fila["ARMADO_POR"]),
                ArmadoPorLogin = fila["ARMADO_POR_LOGIN"].ToString(),
                FechaDespacho = fila["FECHA_DESPACHO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHA_DESPACHO"]),
                DespachadoPor = fila["DESPACHADO_POR"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["DESPACHADO_POR"]),
                DespachadoPorLogin = fila["DESPACHADO_POR_LOGIN"] == DBNull.Value ? null : fila["DESPACHADO_POR_LOGIN"].ToString(),
                FechaCancelacion = fila["FECHA_CANCELACION"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHA_CANCELACION"]),
                CanceladaPor = fila["CANCELADA_POR"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["CANCELADA_POR"]),
                MotivoCancelacion = fila["MOTIVO_CANCELACION"] == DBNull.Value ? null : fila["MOTIVO_CANCELACION"].ToString()
            };
        }

        private CajaVino MapearFilaEfectiva(DataRow fila)
        {
            return new CajaVino
            {
                Id = Convert.ToInt32(fila["ID"]),
                CajaId = Convert.ToInt32(fila["CAJA_ID"]),
                VinoId = Convert.ToInt32(fila["VINO_ID"]),
                Cantidad = Convert.ToInt32(fila["CANTIDAD"]),
                NombreSnapshot = fila["NOMBRE_SNAPSHOT"].ToString(),
                PrecioSnapshot = Convert.ToDecimal(fila["PRECIO_SNAPSHOT"]),
                VinoEfectivoId = Convert.ToInt32(fila["VINO_EFECTIVO_ID"]),
                NombreEfectivo = fila["NOMBRE_EFECTIVO"].ToString(),
                PrecioEfectivo = Convert.ToDecimal(fila["PRECIO_EFECTIVO"]),
                Sustituido = Convert.ToBoolean(fila["SUSTITUIDO"]),
                StockDisponible = Convert.ToInt32(fila["STOCK_DISPONIBLE"])
            };
        }
    }
}
