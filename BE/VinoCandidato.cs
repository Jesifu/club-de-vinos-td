namespace BE
{
    public class VinoCandidato
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string BodegaNombre { get; set; }
        public string Varietal { get; set; }
        public int Aniada { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Preferido { get; set; }

        public override string ToString() => Nombre + " (" + Varietal + ")";
    }
}
