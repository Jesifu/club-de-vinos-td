using System.Collections.Generic;

namespace BE
{
    public class PaginaResultado<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalFilas { get; set; }
        public int Pagina { get; set; }
        public int Tamanio { get; set; }

        public int TotalPaginas => Tamanio > 0
            ? (int)System.Math.Ceiling(TotalFilas / (double)Tamanio)
            : 0;
    }
}
