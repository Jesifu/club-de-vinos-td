using System;

namespace BE
{
    public class BITACORA
    {
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string usuario;
        public string Usuario
        {
            get { return usuario; }
            set { usuario = value; }
        }

        private string accion;
        public string Accion
        {
            get { return accion; }
            set { accion = value; }
        }

        private DateTime fecha;
        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }

        public int DVH { get; set; }
    }
}