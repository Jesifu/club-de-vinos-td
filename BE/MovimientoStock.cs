using System;

namespace BE
{
    public class MovimientoStock
    {
        public int Id { get; set; }
        public int VinoId { get; set; }
        public string VinoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public TipoMovimiento Tipo { get; set; }
        public int Cantidad { get; set; }
        public string Motivo { get; set; }
        public string Responsable { get; set; }
        public string ReferenciaTipo { get; set; }
        public int? ReferenciaId { get; set; }
    }
}
