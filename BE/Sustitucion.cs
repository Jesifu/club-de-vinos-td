using System;

namespace BE
{
    public class Sustitucion
    {
        public int Id { get; set; }
        public int CajaVinoId { get; set; }
        public int VinoOriginalId { get; set; }
        public int VinoReemplazoId { get; set; }
        public string NombreSnapshot { get; set; }
        public decimal PrecioSnapshot { get; set; }
        public string Motivo { get; set; }
        public int ResponsableId { get; set; }
        public string ResponsableLogin { get; set; }
        public DateTime Fecha { get; set; }
    }
}
