using System;
using System.Collections.Generic;

namespace BE
{
    public class CajaMensual
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public string SocioNombre { get; set; }
        public string Periodo { get; set; }
        public EstadoCaja Estado { get; set; }
        public decimal PresupuestoSnapshot { get; set; }
        public DateTime FechaArmado { get; set; }
        public int ArmadoPor { get; set; }
        public string ArmadoPorLogin { get; set; }
        public DateTime? FechaDespacho { get; set; }
        public int? DespachadoPor { get; set; }
        public string DespachadoPorLogin { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public int? CanceladaPor { get; set; }
        public string MotivoCancelacion { get; set; }

        public List<CajaVino> Lineas { get; set; } = new List<CajaVino>();
    }
}
