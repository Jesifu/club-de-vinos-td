using System;
using System.Xml.Serialization;

namespace BE
{
    // Parámetros del análisis de rotación y reposición (N03). Se serializa a XML con
    // XmlSerializer y el sistema lo usa de verdad: el análisis lee todos sus umbrales de acá.
    // Debe ser pública, con constructor sin parámetros y propiedades públicas de lectura/escritura.
    [Serializable]
    [XmlRoot("ConfiguracionRotacion")]
    public class ConfiguracionRotacion
    {
        // Cantidad de días del período por defecto (hasta hoy, inclusive).
        public int DiasAnalisis { get; set; } = 90;

        // Consumo mensual (unidades/mes) desde el cual un vino es de alta rotación (>=).
        public decimal UmbralAltaRotacion { get; set; } = 4m;

        // Consumo mensual por debajo del cual un vino con movimiento es de baja rotación (<).
        public decimal UmbralBajaRotacion { get; set; } = 1.5m;

        // Cobertura (meses) por debajo de la cual la reposición es urgente (<).
        public decimal MesesCoberturaUrgente { get; set; } = 1m;

        // Cobertura (meses) por debajo de la cual corresponde reponer (<).
        public decimal MesesCoberturaReposicion { get; set; } = 2m;

        // Cobertura (meses) por encima de la cual hay sobrestock (>).
        public decimal MesesCoberturaSobrestock { get; set; } = 6m;

        // Meses de consumo que se quiere tener en stock después de reponer (base de la cantidad
        // sugerida). Debe ser mayor a MesesCoberturaReposicion y no superar MesesCoberturaSobrestock.
        // Un XML anterior que no trae este elemento conserva este valor inicial al deserializarse.
        public decimal MesesCoberturaObjetivo { get; set; } = 3m;

        public static ConfiguracionRotacion PorDefecto()
        {
            return new ConfiguracionRotacion();
        }
    }
}
