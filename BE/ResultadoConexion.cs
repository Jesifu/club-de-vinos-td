namespace BE
{
    /// <summary>Resultado de la prueba de conexión a la base de datos al iniciar la aplicación.</summary>
    public class ResultadoConexion
    {
        public bool Exitosa { get; set; }

        /// <summary>Servidor o instancia de SQL Server que se intentó usar.</summary>
        public string Servidor { get; set; }

        /// <summary>Motivo breve del fallo, en español (vacío si la conexión fue exitosa).</summary>
        public string Motivo { get; set; }
    }
}
