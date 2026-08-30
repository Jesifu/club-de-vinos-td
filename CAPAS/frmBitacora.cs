using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmBitacora : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private const string OPCION_TODOS = "(Todos)";
        private const int TAMANIO_PAGINA = 30;

        private readonly BLL.BitacoraBLL _bll = new BLL.BitacoraBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        private int _paginaActual = 1;
        private int _totalPaginas = 1;

        public frmBitacora()
        {
            InitializeComponent();
            this.Resize += (s, e) => ReposicionarLayout();
            this.Shown += (s, e) => ReposicionarLayout();
            panelPaginado.Layout += (s, e) => ReposicionarBotonesPaginado();
        }

        private void frmBitacora_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles.Remove("lblPagina");
            _defaults.Remove("lblPagina");
            _controles["lblTitulo_Bitacora"] = lblTitulo;
            _defaults["lblTitulo_Bitacora"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            PoblarFiltros();
            Refrescar();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ReposicionarLayout();
        }

        private void ReposicionarLayout()
        {
            if (panelPaginado == null || dataGridView1 == null) return;

            Rectangle area = this.ClientRectangle;
            foreach (Control c in this.Controls)
            {
                if (c.Dock == DockStyle.Bottom && c.Visible)
                    area.Height -= c.Height;
            }

            int altoGrilla = area.Height - dataGridView1.Top - 10;
            if (altoGrilla < 100) altoGrilla = 100;
            dataGridView1.Height = altoGrilla;
            dataGridView1.Width = this.ClientSize.Width - dataGridView1.Left - 40;

            ReposicionarBotonesPaginado();
        }

        private void ReposicionarBotonesPaginado()
        {
            if (panelPaginado == null) return;
            int w = panelPaginado.ClientSize.Width;
            if (w < 50) return;
            btnPaginaSiguiente.Left = w - btnPaginaSiguiente.Width - 40;
            btnPaginaAnterior.Left = btnPaginaSiguiente.Left - btnPaginaAnterior.Width - 12;
        }

        private void frmBitacora_FormClosed(object sender, FormClosedEventArgs e)
        {
            SeguridadYServicios.IdiomaManager.getInstance().Desregistrar(this);
        }

        public void ActualizarIdioma()
        {
            foreach (var kvp in _controles)
            {
                string t = SeguridadYServicios.IdiomaManager.getInstance().Traducir(kvp.Key)
                           ?? _defaults[kvp.Key];
                kvp.Value.Text = t;
            }
            ActualizarEncabezados();
        }

        private void ActualizarEncabezados()
        {
            if (dataGridView1.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dataGridView1.Columns["Id"] != null)
                dataGridView1.Columns["Id"].HeaderText = mgr.Traducir("colhdr_Id") ?? "ID";
            if (dataGridView1.Columns["Usuario"] != null)
                dataGridView1.Columns["Usuario"].HeaderText = mgr.Traducir("colhdr_Usuario") ?? "Usuario";
            if (dataGridView1.Columns["Accion"] != null)
                dataGridView1.Columns["Accion"].HeaderText = mgr.Traducir("colhdr_Accion") ?? "Acción";
            if (dataGridView1.Columns["Fecha"] != null)
                dataGridView1.Columns["Fecha"].HeaderText = mgr.Traducir("colhdr_Fecha") ?? "Fecha y Hora";
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

        private void PoblarFiltros()
        {
            cmbUsuario.Items.Clear();
            cmbUsuario.Items.Add(OPCION_TODOS);
            foreach (string u in _bll.ListarUsuariosDistinct())
                cmbUsuario.Items.Add(u);
            cmbUsuario.SelectedIndex = 0;

            cmbAccion.Items.Clear();
            cmbAccion.Items.Add(OPCION_TODOS);
            foreach (string a in _bll.ListarAccionesDistinct())
                cmbAccion.Items.Add(a);
            cmbAccion.SelectedIndex = 0;

            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
        }

        private void Refrescar()
        {
            string usuario = (cmbUsuario.SelectedItem != null && (string)cmbUsuario.SelectedItem != OPCION_TODOS)
                ? (string)cmbUsuario.SelectedItem : null;
            string accion = (cmbAccion.SelectedItem != null && (string)cmbAccion.SelectedItem != OPCION_TODOS)
                ? (string)cmbAccion.SelectedItem : null;
            DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : (DateTime?)null;
            DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date.AddDays(1) : (DateTime?)null;

            BE.PaginaResultado<BE.BITACORA> resultado =
                _bll.ListarPaginado(usuario, accion, desde, hasta, _paginaActual, TAMANIO_PAGINA);

            _totalPaginas = resultado.TotalPaginas == 0 ? 1 : resultado.TotalPaginas;
            if (_paginaActual > _totalPaginas) _paginaActual = _totalPaginas;

            dataGridView1.DataSource = resultado.Items;

            if (dataGridView1.Columns.Count > 0)
            {
                if (dataGridView1.Columns["Id"] != null) dataGridView1.Columns["Id"].Width = 50;
                if (dataGridView1.Columns["Usuario"] != null) dataGridView1.Columns["Usuario"].Width = 150;
                if (dataGridView1.Columns["Accion"] != null) dataGridView1.Columns["Accion"].Width = 200;
                if (dataGridView1.Columns["Fecha"] != null) dataGridView1.Columns["Fecha"].Width = 200;
                ActualizarEncabezados();
            }

            ActualizarControlesPaginacion();
        }

        private void ActualizarControlesPaginacion()
        {
            lblPagina.Text = string.Format("Página {0} de {1}", _paginaActual, _totalPaginas);
            btnPaginaAnterior.Enabled = _paginaActual > 1;
            btnPaginaSiguiente.Enabled = _paginaActual < _totalPaginas;
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (_paginaActual <= 1) return;
            _paginaActual--;
            Refrescar();
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (_paginaActual >= _totalPaginas) return;
            _paginaActual++;
            Refrescar();
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            _paginaActual = 1;
            Refrescar();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cmbUsuario.SelectedIndex = 0;
            cmbAccion.SelectedIndex = 0;
            dtpDesde.Checked = false;
            dtpHasta.Checked = false;
            _paginaActual = 1;
            Refrescar();
        }
    }
}
