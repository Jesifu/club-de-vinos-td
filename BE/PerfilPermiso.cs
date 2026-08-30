using System.Collections.Generic;

namespace BE
{
    public class Rol : NodoPermiso
    {
        public bool Protegido { get; set; }

        private readonly List<NodoPermiso> _hijos = new List<NodoPermiso>();

        public override void Agregar(NodoPermiso nodo)
        {
            _hijos.Add(nodo);
        }

        public override void Quitar(NodoPermiso nodo)
        {
            _hijos.Remove(nodo);
        }

        public override IList<NodoPermiso> ObtenerHijos()
        {
            return _hijos;
        }

        public override bool EsHoja()
        {
            return false;
        }
    }
}
