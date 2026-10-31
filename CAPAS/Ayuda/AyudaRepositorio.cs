using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CAPAS
{
    // Un tema de la ayuda en línea. Un tema sin pasos funciona como "área" que agrupa subtemas.
    internal sealed class AyudaTema
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Permiso { get; set; }
        public List<string> Pasos { get; } = new List<string>();
        public List<string> Reglas { get; } = new List<string>();
        public List<string> Mensajes { get; } = new List<string>();
        public List<AyudaTema> Hijos { get; } = new List<AyudaTema>();
        public AyudaTema Padre { get; set; }

        public bool EsArea
        {
            get { return Hijos.Count > 0 && Pasos.Count == 0; }
        }

        // Texto plano de todo el tema, para el filtro de búsqueda.
        public string TextoCompleto()
        {
            var sb = new StringBuilder();
            sb.Append(Titulo).Append(' ').Append(Resumen).Append(' ').Append(Permiso);
            foreach (string s in Pasos.Concat(Reglas).Concat(Mensajes))
                sb.Append(' ').Append(s);
            return sb.ToString();
        }
    }

    // Carga el contenido de la ayuda desde un XML incrustado (CAPAS.Ayuda.ayuda_{codigo}.xml).
    // Solo existe contenido en español; si no hay recurso para el idioma activo se usa el español.
    internal static class AyudaRepositorio
    {
        private const string Prefijo = "CAPAS.Ayuda.ayuda_";
        private const string CodigoBase = "es";

        private static readonly Dictionary<string, List<AyudaTema>> _cache =
            new Dictionary<string, List<AyudaTema>>();

        // Código de idioma de contenido para el idioma activo de la interfaz ("es", "en", "pt", ...).
        public static string CodigoIdiomaActivo()
        {
            BE.IDIOMA activo = SeguridadYServicios.IdiomaManager.getInstance().IdiomaActivo;
            if (activo == null || string.IsNullOrEmpty(activo.Nombre)) return CodigoBase;

            string n = Normalizar(activo.Nombre);
            if (n.StartsWith("ing") || n.StartsWith("eng")) return "en";
            if (n.StartsWith("port")) return "pt";
            return CodigoBase;
        }

        // Árbol de temas raíz para el código pedido, con respaldo en español.
        public static List<AyudaTema> Cargar(string codigo)
        {
            string cod = ExisteRecurso(codigo) ? codigo : CodigoBase;
            lock (_cache)
            {
                List<AyudaTema> raiz;
                if (!_cache.TryGetValue(cod, out raiz))
                {
                    raiz = Parsear(cod);
                    _cache[cod] = raiz;
                }
                return raiz;
            }
        }

        // True si el contenido cargado para ese código es el español de respaldo.
        public static bool EsRespaldo(string codigo)
        {
            return !ExisteRecurso(codigo) && codigo != CodigoBase;
        }

        public static AyudaTema Buscar(IEnumerable<AyudaTema> temas, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (AyudaTema t in temas)
            {
                if (string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) return t;
                AyudaTema h = Buscar(t.Hijos, id);
                if (h != null) return h;
            }
            return null;
        }

        // Coincidencia sin distinguir mayúsculas, tildes ni anchos de caracter.
        public static bool Coincide(AyudaTema tema, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return true;
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                tema.TextoCompleto(), texto.Trim(),
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreWidth) >= 0;
        }

        private static bool ExisteRecurso(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) return false;
            string nombre = Prefijo + codigo + ".xml";
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains(nombre);
        }

        private static List<AyudaTema> Parsear(string codigo)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream(Prefijo + codigo + ".xml"))
            {
                if (s == null) return new List<AyudaTema>();
                XDocument doc = XDocument.Load(s);
                var raiz = new List<AyudaTema>();
                foreach (XElement e in doc.Root.Elements("Tema"))
                    raiz.Add(LeerTema(e, null));
                return raiz;
            }
        }

        private static AyudaTema LeerTema(XElement e, AyudaTema padre)
        {
            var t = new AyudaTema
            {
                Id = (string)e.Attribute("id") ?? string.Empty,
                Titulo = (string)e.Attribute("titulo") ?? string.Empty,
                Resumen = Limpiar((string)e.Element("Resumen")),
                Permiso = Limpiar((string)e.Element("Permiso")),
                Padre = padre
            };
            LeerLista(e.Element("Pasos"), "Paso", t.Pasos);
            LeerLista(e.Element("Reglas"), "Regla", t.Reglas);
            LeerLista(e.Element("Mensajes"), "Mensaje", t.Mensajes);
            foreach (XElement h in e.Elements("Tema"))
                t.Hijos.Add(LeerTema(h, t));
            return t;
        }

        private static void LeerLista(XElement contenedor, string nombre, List<string> destino)
        {
            if (contenedor == null) return;
            foreach (XElement x in contenedor.Elements(nombre))
            {
                string texto = Limpiar(x.Value);
                if (texto.Length > 0) destino.Add(texto);
            }
        }

        private static string Limpiar(string s)
        {
            return (s ?? string.Empty).Trim();
        }

        private static string Normalizar(string s)
        {
            string d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(c));
            return sb.ToString();
        }
    }
}
