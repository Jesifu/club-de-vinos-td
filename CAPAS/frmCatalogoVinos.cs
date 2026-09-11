using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmCatalogoVinos : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.VinoBLL _bll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmCatalogoVinos()
        {
            InitializeComponent();
            this.Resize += (s, e) => ReposicionarLayout();
            this.Shown += (s, e) => ReposicionarLayout();
        }

        private void frmCatalogoVinos_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_CatalogoVinos"] = lblTitulo;
            _defaults["lblTitulo_CatalogoVinos"] = lblTitulo.Text;

            // Colisión con la clave global 'lblNombre' (frmNuevoUsuario = "Usuario:").
            _controles.Remove("lblNombre");
            _defaults.Remove("lblNombre");
            _controles["lblNombre_CatalogoVinos"] = lblNombre;
            _defaults["lblNombre_CatalogoVinos"] = lblNombre.Text;

            // Los NumericUpDown reflejan el valor actual en .Text: si quedaran en
            // _controles, un cambio de idioma pisaría lo que el usuario cargó.
            _controles.Remove("numAniada");
            _defaults.Remove("numAniada");
            _controles.Remove("numPrecio");
            _defaults.Remove("numPrecio");
            _controles.Remove("numStockMinimo");
            _defaults.Remove("numStockMinimo");
            _controles.Remove("numPuntaje");
            _defaults.Remove("numPuntaje");

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarBodegas();
            CargarVinos();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ReposicionarLayout();
        }

        private void ReposicionarLayout()
        {
            if (panelInferior == null || dgvVinos == null) return;

            int altoGrilla = this.ClientSize.Height - panelInferior.Height - dgvVinos.Top - 10;
            if (altoGrilla < 80) altoGrilla = 80;
            dgvVinos.Height = altoGrilla;
            dgvVinos.Width = this.ClientSize.Width - dgvVinos.Left - 15;
        }

        private void frmCatalogoVinos_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvVinos.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvVinos.Columns["Codigo"] != null)
                dgvVinos.Columns["Codigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvVinos.Columns["Nombre"] != null)
                dgvVinos.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvVinos.Columns["BodegaNombre"] != null)
                dgvVinos.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvVinos.Columns["Varietal"] != null)
                dgvVinos.Columns["Varietal"].HeaderText = mgr.Traducir("colhdr_Varietal") ?? "Varietal";
            if (dgvVinos.Columns["Aniada"] != null)
                dgvVinos.Columns["Aniada"].HeaderText = mgr.Traducir("colhdr_Aniada") ?? "Añada";
            if (dgvVinos.Columns["Precio"] != null)
                dgvVinos.Columns["Precio"].HeaderText = mgr.Traducir("colhdr_Precio") ?? "Precio";
            if (dgvVinos.Columns["StockMinimo"] != null)
                dgvVinos.Columns["StockMinimo"].HeaderText = mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo";
            if (dgvVinos.Columns["Maridaje"] != null)
                dgvVinos.Columns["Maridaje"].HeaderText = mgr.Traducir("colhdr_Maridaje") ?? "Maridaje";
            if (dgvVinos.Columns["Puntaje"] != null)
                dgvVinos.Columns["Puntaje"].HeaderText = mgr.Traducir("colhdr_Puntaje") ?? "Puntaje";
            if (dgvVinos.Columns["CreadoPorLogin"] != null)
                dgvVinos.Columns["CreadoPorLogin"].HeaderText = mgr.Traducir("colhdr_CreadoPor") ?? "Creado por";
            if (dgvVinos.Columns["FechaAlta"] != null)
                dgvVinos.Columns["FechaAlta"].HeaderText = mgr.Traducir("colhdr_FechaAlta") ?? "Fecha de alta";
            if (dgvVinos.Columns["AutorizadoPorLogin"] != null)
                dgvVinos.Columns["AutorizadoPorLogin"].HeaderText = mgr.Traducir("colhdr_AutorizadoPor") ?? "Autorizado por";
            if (dgvVinos.Columns["FechaAutorizacion"] != null)
                dgvVinos.Columns["FechaAutorizacion"].HeaderText = mgr.Traducir("colhdr_FechaAutorizacion") ?? "Fecha de autorización";
            if (dgvVinos.Columns["Estado"] != null)
                dgvVinos.Columns["Estado"].HeaderText = mgr.Traducir("colhdr_Estado") ?? "Estado";
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

        private void CargarBodegas()
        {
            cboBodega.DataSource = _bll.ListarBodegas();
            cboBodega.DisplayMember = "Nombre";
            cboBodega.ValueMember = "Id";
            if (cboBodega.Items.Count > 0) cboBodega.SelectedIndex = 0;
        }

        private void CargarVinos()
        {
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            List<BE.Vino> lista = _bll.ListarAutorizados();
            lista.AddRange(_bll.ListarPendientes().Where(v => v.CreadoPor == sesion.Id));

            dgvVinos.DataSource = lista.OrderByDescending(v => v.FechaAlta).ToList();

            if (dgvVinos.Columns.Count > 0)
            {
                if (dgvVinos.Columns["Id"] != null) dgvVinos.Columns["Id"].Visible = false;
                if (dgvVinos.Columns["BodegaId"] != null) dgvVinos.Columns["BodegaId"].Visible = false;
                if (dgvVinos.Columns["CreadoPor"] != null) dgvVinos.Columns["CreadoPor"].Visible = false;
                if (dgvVinos.Columns["AutorizadoPor"] != null) dgvVinos.Columns["AutorizadoPor"].Visible = false;
                ActualizarEncabezados();
            }
        }

        private void chkPuntaje_CheckedChanged(object sender, EventArgs e)
        {
            numPuntaje.Enabled = chkPuntaje.Checked;
            if (!chkPuntaje.Checked) numPuntaje.Value = 0;
        }

        private void btnGuardarVino_Click(object sender, EventArgs e)
        {
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            BE.Vino v = new BE.Vino
            {
                Codigo = txtCodigo.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                BodegaId = cboBodega.SelectedValue is int bodegaId ? bodegaId : 0,
                Varietal = txtVarietal.Text.Trim(),
                Aniada = (int)numAniada.Value,
                Precio = numPrecio.Value,
                StockMinimo = (int)numStockMinimo.Value,
                Maridaje = string.IsNullOrWhiteSpace(txtMaridaje.Text) ? null : txtMaridaje.Text.Trim(),
                Puntaje = chkPuntaje.Checked ? (int?)numPuntaje.Value : null
            };

            try
            {
                _bll.ProponerAlta(v, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Vino propuesto correctamente. Queda pendiente de autorización.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            LimpiarFormulario();
            CargarVinos();
        }

        private void btnLimpiarVino_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            txtVarietal.Clear();
            txtMaridaje.Clear();
            if (cboBodega.Items.Count > 0) cboBodega.SelectedIndex = 0;
            numAniada.Value = 2020;
            numPrecio.Value = 0;
            numStockMinimo.Value = 0;
            chkPuntaje.Checked = false;
            numPuntaje.Value = 0;
            txtCodigo.Focus();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
