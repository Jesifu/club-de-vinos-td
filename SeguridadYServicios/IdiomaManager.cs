using BE;
using System.Collections.Generic;

namespace SeguridadYServicios
{
    public sealed class IdiomaManager
    {
        private static IdiomaManager _instance;
        private static readonly object _cerrojo = new object();

        private readonly List<IObservadorIdioma> _observers = new List<IObservadorIdioma>();
        private Dictionary<string, string> _traducciones = new Dictionary<string, string>();
        private IDIOMA _idiomaActivo;

        private IdiomaManager() { }

        public static IdiomaManager getInstance()
        {
            if (_instance == null)
            {
                lock (_cerrojo)
                {
                    if (_instance == null)
                        _instance = new IdiomaManager();
                }
            }
            return _instance;
        }

        public IDIOMA IdiomaActivo => _idiomaActivo;

        public void Registrar(IObservadorIdioma obs)
        {
            if (!_observers.Contains(obs))
                _observers.Add(obs);
        }

        public void Desregistrar(IObservadorIdioma obs)
        {
            _observers.Remove(obs);
        }

        public void CambiarIdioma(IDIOMA idioma, Dictionary<string, string> traducciones)
        {
            _idiomaActivo = idioma;
            _traducciones = traducciones ?? new Dictionary<string, string>();
            Notificar();
        }

        private void Notificar()
        {
            foreach (var obs in new List<IObservadorIdioma>(_observers))
                obs.ActualizarIdioma();
        }

        public string Traducir(string clave)
        {
            return _traducciones.TryGetValue(clave, out string texto) ? texto : null;
        }
    }
}
