using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    internal class Acceso
    {
        private SqlConnection conexion;
        private SqlTransaction transaccion;

        public void Abrir()
        {
            conexion = new SqlConnection();
            conexion.ConnectionString = "initial catalog=BDCAPAS; Data Source=.; Integrated Security=SSPI";
            //conexion.ConnectionString = "Server=localhost\\SQLEXPRESS01; Database=BDCAPAS; Trusted_Connection=True";
            conexion.Open();
        }

        public void Cerrar()
        {
            conexion.Close();
            conexion = null;
            GC.Collect();
        }

        public void IniciarTx()
        {
            transaccion = conexion.BeginTransaction();
        }

        public void ConfirmarTX()
        {
            transaccion.Commit();
            transaccion = null;
        }

        public void DeshacerTX()
        {
            transaccion.Rollback();
            transaccion = null;
        }


        private SqlCommand CrearComando(string sql, List<SqlParameter> parametros = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sql;
            cmd.Connection = conexion;
            if (transaccion != null)
            {
                cmd.Transaction = transaccion;
            }
            if (parametros != null)
            {
                cmd.Parameters.AddRange(parametros.ToArray());
            }
            return cmd;
        }

        public int Escribir(string sql, List<SqlParameter> parametros = null)
        {
            SqlCommand cmd = CrearComando(sql, parametros);
            int filas = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return filas;
        }


        public DataTable Leer(string sql, List<SqlParameter> parametros = null)
        {
            SqlDataAdapter adaptador = new SqlDataAdapter();
            adaptador.SelectCommand = CrearComando(sql, parametros);
            DataTable tabla = new DataTable();

            adaptador.Fill(tabla);

            return tabla;

        }



        public SqlParameter CrearParametro(string nombre, string valor)
        {
            SqlParameter p = new SqlParameter();
            p.Value = valor;
            p.ParameterName = nombre;
            p.DbType = DbType.String;
            return p;
        }
        public SqlParameter CrearParametro(string nombre, int valor)
        {
            SqlParameter p = new SqlParameter();
            p.Value = valor;
            p.ParameterName = nombre;
            p.DbType = DbType.Int32;
            return p;
        }

    }
}
