using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Win32;
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
            conexion.ConnectionString = CadenaConexion;
            conexion.Open();
        }

        // Servidor SQL: variable de entorno CLUBVINOS_SQL (soporte), luego el registro que
        // escribe el instalador (HKLM\SOFTWARE\Club de Vinos\SqlInstance) y por último "." (predeterminada).
        // Se resuelve una sola vez y nunca lanza: si el registro no se puede leer, se usa ".".
        private static readonly Lazy<string> _cadenaConexion = new Lazy<string>(ConstruirCadenaConexion);

        private static string CadenaConexion
        {
            get { return _cadenaConexion.Value; }
        }

        private static readonly Lazy<string> _servidor = new Lazy<string>(ResolverServidor);

        // Servidor/instancia efectivo (para la prueba de conexión y los mensajes al usuario).
        internal static string Servidor
        {
            get { return _servidor.Value; }
        }

        internal static string CadenaConexionPublica
        {
            get { return CadenaConexion; }
        }

        private static string ResolverServidor()
        {
            return LeerEntorno() ?? LeerRegistro(RegistryView.Registry64)
                   ?? LeerRegistro(RegistryView.Registry32) ?? ".";
        }

        private static string ConstruirCadenaConexion()
        {
            string servidor = Servidor;
            try
            {
                return new SqlConnectionStringBuilder
                {
                    DataSource = servidor,
                    InitialCatalog = "BDCAPAS",
                    IntegratedSecurity = true
                }.ConnectionString;
            }
            catch (Exception)
            {
                return "initial catalog=BDCAPAS; Data Source=.; Integrated Security=SSPI";
            }
        }

        private static string LeerEntorno()
        {
            try
            {
                string v = Environment.GetEnvironmentVariable("CLUBVINOS_SQL");
                return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string LeerRegistro(RegistryView vista)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, vista))
                using (RegistryKey clave = baseKey.OpenSubKey(@"SOFTWARE\Club de Vinos"))
                {
                    string v = clave == null ? null : clave.GetValue("SqlInstance") as string;
                    return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
                }
            }
            catch (Exception)
            {
                return null;
            }
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
