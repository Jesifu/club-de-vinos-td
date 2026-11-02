using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // D02 — Ayuda en línea. Árbol de temas a la izquierda, contenido de solo lectura a la derecha
    // (RichTextBox) y un cuadro de búsqueda que filtra los temas. El contenido sale de
    // AyudaRepositorio (XML incrustado, solo en español: sin contenido para otro idioma se usa el
    // español); los rótulos de la propia ventana se traducen con las tablas de idioma.
    public partial class frmAyuda : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();
        private readonly string _temaInicial;

        private List<AyudaTema> _raiz;
        private AyudaTema _actual;
        private string _codigoIdioma;
        private bool _reconstruyendo;

        // Constructor sin parámetros: lo necesita el diseñador de Visual Studio.
        public frmAyuda() : this(null)
        {
        }

        public frmAyuda(string temaInicial)
        {
            InitializeComponent();
            _temaInicial = temaInicial;
        }

        // F1 dentro de la propia ayuda no debe abrir otra ayuda encima.
        protected override bool AyudaHabilitada
        {
            get { return false; }
        }

        private void frmAyuda_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);

            CargarContenido();
            ActualizarIdioma();
            ConstruirArbol(string.Empty, _temaInicial);

            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            EstilizarContenido();
            if (_actual != null) MostrarTema(_actual);
        }

        private void frmAyuda_FormClosed(object sender, FormClosedEventArgs e)
        {
            SeguridadYServicios.IdiomaManager.getInstance().Desregistrar(this);
        }

        public void ActualizarIdioma()
        {
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            foreach (var kvp in _controles)
                kvp.Value.Text = mgr.Traducir(kvp.Key) ?? _defaults[kvp.Key];

            if (_raiz == null) return; // todavía no terminó Load

            // Si cambió el idioma de contenido (hoy siempre cae al español) se recarga el árbol.
            if (AyudaRepositorio.CodigoIdiomaActivo() != _codigoIdioma)
            {
                string id = _actual != null ? _actual.Id : null;
                CargarContenido();
                ConstruirArbol(txtBuscar.Text, id);
            }
            else if (_actual != null)
            {
                MostrarTema(_actual);
            }
        }

        private void GuardarDefaults(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                if (!string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Text))
                {
                    _controles[c.Name] = c;
                    _defaults[c.Name] = c.Text;
                }
                if (c.HasChildren) GuardarDefaults(c.Controls);
            }
        }

        private void CargarContenido()
        {
            _codigoIdioma = AyudaRepositorio.CodigoIdiomaActivo();
            _raiz = AyudaRepositorio.Cargar(_codigoIdioma);
        }

        // Un RichTextBox de solo lectura toma el color de formulario: se fija el de ventana del sistema.
        private void EstilizarContenido()
        {
            rtbContenido.BackColor = SystemColors.Window;
            rtbContenido.ForeColor = SystemColors.WindowText;
            rtbContenido.DetectUrls = false;
        }

        // ---- Árbol y búsqueda ---------------------------------------------------------------

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            ConstruirArbol(txtBuscar.Text, _actual != null ? _actual.Id : null);
        }

        private void ConstruirArbol(string filtro, string idASeleccionar)
        {
            _reconstruyendo = true;
            treeTemas.BeginUpdate();
            treeTemas.Nodes.Clear();
            foreach (AyudaTema tema in _raiz)
            {
                TreeNode nodo = CrearNodo(tema, filtro);
                if (nodo != null) treeTemas.Nodes.Add(nodo);
            }
            treeTemas.ExpandAll();
            treeTemas.EndUpdate();
            _reconstruyendo = false;

            TreeNode seleccionado = BuscarNodo(treeTemas.Nodes, idASeleccionar);
            if (seleccionado == null && treeTemas.Nodes.Count > 0)
                seleccionado = treeTemas.Nodes[0];

            if (seleccionado != null)
            {
                treeTemas.SelectedNode = seleccionado;
                seleccionado.EnsureVisible();
                MostrarTema((AyudaTema)seleccionado.Tag);
            }
            else
            {
                _actual = null;
                MostrarSinResultados();
            }
        }

        // Un nodo se incluye si el tema coincide o si alguno de sus descendientes coincide.
        private TreeNode CrearNodo(AyudaTema tema, string filtro)
        {
            var hijos = new List<TreeNode>();
            foreach (AyudaTema h in tema.Hijos)
            {
                TreeNode nh = CrearNodo(h, filtro);
                if (nh != null) hijos.Add(nh);
            }

            if (hijos.Count == 0 && !AyudaRepositorio.Coincide(tema, filtro))
                return null;

            var nodo = new TreeNode(tema.Titulo) { Tag = tema };
            foreach (TreeNode nh in hijos) nodo.Nodes.Add(nh);
            return nodo;
        }

        private static TreeNode BuscarNodo(TreeNodeCollection nodos, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (TreeNode n in nodos)
            {
                var tema = n.Tag as AyudaTema;
                if (tema != null && string.Equals(tema.Id, id, StringComparison.OrdinalIgnoreCase))
                    return n;
                TreeNode h = BuscarNodo(n.Nodes, id);
                if (h != null) return h;
            }
            return null;
        }

        private void treeTemas_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_reconstruyendo) return;
            var tema = e.Node != null ? e.Node.Tag as AyudaTema : null;
            if (tema != null) MostrarTema(tema);
        }

        // ---- Contenido ----------------------------------------------------------------------

        private void MostrarSinResultados()
        {
            rtbContenido.Clear();
            using (var f = new Font("Segoe UI", 10.5f, FontStyle.Italic))
                Escribir(Traducir("ayuda_sinResultados", "No se encontraron temas para la búsqueda."),
                    f, SystemColors.GrayText);
        }

        private void MostrarTema(AyudaTema tema)
        {
            _actual = tema;
            rtbContenido.Clear();

            using (var fTitulo = new Font("Segoe UI", 16f, FontStyle.Bold))
            using (var fSeccion = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var fTexto = new Font("Segoe UI", 10.5f, FontStyle.Regular))
            using (var fNota = new Font("Segoe UI", 9.5f, FontStyle.Italic))
            {
                Escribir(tema.Titulo + "\n", fTitulo, SystemColors.WindowText);

                if (AyudaRepositorio.EsRespaldo(_codigoIdioma))
                    Escribir(Traducir("ayuda_contenidoSoloEs",
                        "El contenido de la ayuda está disponible en español.") + "\n", fNota, SystemColors.GrayText);

                Seccion("ayuda_sec_Resumen", "Qué es", tema.Resumen, fSeccion, fTexto);
                Seccion("ayuda_sec_Permiso", "Quién puede usarlo", tema.Permiso, fSeccion, fTexto);
                Lista("ayuda_sec_Pasos", "Paso a paso", tema.Pasos, true, fSeccion, fTexto);
                Lista("ayuda_sec_Reglas", "Reglas aplicables", tema.Reglas, false, fSeccion, fTexto);
                Lista("ayuda_sec_Mensajes", "Mensajes frecuentes", tema.Mensajes, false, fSeccion, fTexto);

                if (tema.Hijos.Count > 0)
                {
                    var titulos = new List<string>();
                    foreach (AyudaTema h in tema.Hijos) titulos.Add(h.Titulo);
                    Lista("ayuda_sec_Temas", "Temas de esta área", titulos, false, fSeccion, fTexto);
                }
            }

            rtbContenido.SelectionStart = 0;
            rtbContenido.SelectionLength = 0;
            rtbContenido.ScrollToCaret();
        }

        private void Seccion(string clave, string porDefecto, string texto, Font fSeccion, Font fTexto)
        {
            if (string.IsNullOrEmpty(texto)) return;
            Encabezado(clave, porDefecto, fSeccion);
            Sangria(0, 0);
            Escribir(texto + "\n", fTexto, SystemColors.WindowText);
        }

        private void Lista(string clave, string porDefecto, List<string> items, bool numerada,
            Font fSeccion, Font fTexto)
        {
            if (items == null || items.Count == 0) return;
            Encabezado(clave, porDefecto, fSeccion);
            for (int i = 0; i < items.Count; i++)
            {
                Sangria(0, 22);
                string marca = numerada ? (i + 1) + ". " : "• ";
                Escribir(marca + items[i] + "\n", fTexto, SystemColors.WindowText);
            }
            Sangria(0, 0);
        }

        private void Encabezado(string clave, string porDefecto, Font fSeccion)
        {
            Sangria(0, 0);
            Escribir("\n" + Traducir(clave, porDefecto) + "\n", fSeccion, SystemColors.WindowText);
        }

        private void Sangria(int izquierda, int colgante)
        {
            rtbContenido.SelectionStart = rtbContenido.TextLength;
            rtbContenido.SelectionLength = 0;
            rtbContenido.SelectionIndent = izquierda;
            rtbContenido.SelectionHangingIndent = colgante;
        }

        private void Escribir(string texto, Font fuente, Color color)
        {
            rtbContenido.SelectionStart = rtbContenido.TextLength;
            rtbContenido.SelectionLength = 0;
            rtbContenido.SelectionFont = fuente;
            rtbContenido.SelectionColor = color;
            rtbContenido.SelectedText = texto;
        }

        private static string Traducir(string clave, string porDefecto)
        {
            return SeguridadYServicios.IdiomaManager.getInstance().Traducir(clave) ?? porDefecto;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
