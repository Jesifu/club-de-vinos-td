namespace BE
{
    public class USUARIO
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

        private string contrasena;
        public string Contrasena
        {
            get { return contrasena; }
            set { contrasena = value; }
        }

        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public string Rol { get; set; }
        public int? RolId { get; set; }
        public string RolNombre { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int DVH { get; set; }
        public int? IdiomaId { get; set; }
    }
}