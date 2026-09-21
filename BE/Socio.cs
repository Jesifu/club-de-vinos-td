using System;
using System.Collections.Generic;

namespace BE
{
    public class Socio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Domicilio { get; set; }
        public decimal PresupuestoMensual { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public int CreadoPor { get; set; }
        public string CreadoPorLogin { get; set; }

        public List<string> VarietalesPreferidos { get; set; } = new List<string>();

        public override string ToString() => Apellido + ", " + Nombre;
    }
}
