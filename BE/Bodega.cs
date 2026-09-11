namespace BE
{
    public class Bodega
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public string Region { get; set; }
        public bool Habilitado { get; set; }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
