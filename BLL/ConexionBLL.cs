using System;

namespace BLL
{
    public class ConexionBLL
    {
        private readonly DAL.ConexionDAL _dal = new DAL.ConexionDAL();

        public BE.ResultadoConexion ProbarConexion()
        {
            return _dal.Probar();
        }

        public static bool EsErrorDeConexion(Exception ex)
        {
            return DAL.ConexionDAL.EsErrorDeConexion(ex);
        }
    }
}
