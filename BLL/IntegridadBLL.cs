using System;
using System.Collections.Generic;
using System.Data;
using SeguridadYServicios;

namespace BLL
{
    public class ResultadoIntegridad
    {
        public bool EsValido { get; set; } = true;
        public List<string> Errores { get; set; } = new List<string>();
        public List<int> IdsUsuariosAfectados { get; set; } = new List<int>();
    }

    public class IntegridadBLL
    {
        private static readonly string[] ColumnasUsuario =
            { "ID", "USUARIO", "PASS", "INTENTOS_FALLIDOS", "BLOQUEADO", "ROL", "PERFILES" };

        private readonly DAL.IntegridadDAL    _dal       = new DAL.IntegridadDAL();
        private readonly DAL.UsuarioPerfilDAL _perfilDal = new DAL.UsuarioPerfilDAL();


        private string[] ExtraerAtributosUsuario(DataRow fila, string perfilesStr)
        {
            return new string[]
            {
                fila["ID"].ToString(),
                fila["USUARIO"].ToString(),
                fila["PASS"].ToString(),
                fila["INTENTOS_FALLIDOS"].ToString(),
                Convert.ToBoolean(fila["BLOQUEADO"]) ? "1" : "0",
                fila["ROL"].ToString(),
                perfilesStr
            };
        }

        private string ObtenerPerfilesStr(int usuarioId)
        {
            List<int> ids = _perfilDal.ListarPerfilesPorUsuario(usuarioId);
            ids.Sort();
            return string.Join(",", ids);
        }


        private void VerificarTabla(
            string          nombreTabla,
            string[]        columnas,
            DataTable       datos,
            Func<DataRow, string[]> extraerAtributos,
            ResultadoIntegridad resultado)
        {
            List<string[]> todasFilas = new List<string[]>();

            foreach (DataRow fila in datos.Rows)
            {
                int id            = Convert.ToInt32(fila["ID"]);
                int dvhAlmacenado = Convert.ToInt32(fila["DVH"]);
                string[] atribs   = extraerAtributos(fila);
                int dvhCalculado  = CalculadorDVH.Calcular(atribs);

                if (dvhAlmacenado != dvhCalculado)
                {
                    resultado.EsValido = false;
                    resultado.Errores.Add($"{nombreTabla} ID={id}: DVH inválido.");
                    if (nombreTabla == "USUARIO")
                        resultado.IdsUsuariosAfectados.Add(id);
                }

                todasFilas.Add(atribs);
            }

            Dictionary<string, int> dvvsAlmacenados = _dal.ObtenerDVV(nombreTabla);

            if (dvvsAlmacenados.Count == 0 && datos.Rows.Count == 0)
                return;

            if (dvvsAlmacenados.Count == 0)
            {
                resultado.EsValido = false;
                resultado.Errores.Add(
                    $"{nombreTabla}: dígitos verificadores verticales ausentes.");
                return;
            }

            for (int c = 0; c < columnas.Length; c++)
            {
                string col       = columnas[c];
                int dvvCalculado = CalculadorDVH.CalcularVertical(todasFilas, c);

                if (!dvvsAlmacenados.ContainsKey(col) || dvvsAlmacenados[col] != dvvCalculado)
                {
                    resultado.EsValido = false;
                    resultado.Errores.Add(
                        $"{nombreTabla} columna {col}: DVV inválido. " +
                        "Posible inserción, eliminación o reordenamiento de filas.");
                }
            }
        }


        private void RecalcularTabla(
            string          nombreTabla,
            string[]        columnas,
            DataTable       datos,
            Func<DataRow, string[]> extraerAtributos,
            Action<int, int> actualizarDVH)
        {
            List<string[]> todasFilas = new List<string[]>();

            foreach (DataRow fila in datos.Rows)
            {
                int id          = Convert.ToInt32(fila["ID"]);
                string[] atribs = extraerAtributos(fila);
                int dvh         = CalculadorDVH.Calcular(atribs);
                actualizarDVH(id, dvh);
                todasFilas.Add(atribs);
            }

            for (int c = 0; c < columnas.Length; c++)
            {
                int dvv = CalculadorDVH.CalcularVertical(todasFilas, c);
                _dal.ActualizarDVV(nombreTabla, columnas[c], dvv);
            }
        }


        public ResultadoIntegridad VerificarIntegridad()
        {
            ResultadoIntegridad resultado = new ResultadoIntegridad();

            DataTable usuarios = _dal.ListarUsuariosParaIntegridad();
            VerificarTabla(
                "USUARIO", ColumnasUsuario, usuarios,
                fila => ExtraerAtributosUsuario(fila, ObtenerPerfilesStr(Convert.ToInt32(fila["ID"]))),
                resultado);

            return resultado;
        }


        public void RecalcularIntegridadUsuarios()
        {
            RecalcularTabla(
                "USUARIO", ColumnasUsuario,
                _dal.ListarUsuariosParaIntegridad(),
                fila => ExtraerAtributosUsuario(fila, ObtenerPerfilesStr(Convert.ToInt32(fila["ID"]))),
                _dal.ActualizarDVHUsuario);
        }

        public void RecalcularIntegridad()
        {
            RecalcularIntegridadUsuarios();
        }


        public bool EstaInicializado()
        {
            return _dal.ObtenerDVV("USUARIO").Count > 0;
        }
    }
}
