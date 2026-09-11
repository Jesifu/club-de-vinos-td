using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmAutorizarAltaVino : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.VinoBLL _bll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmAutorizarAltaVino()
        {
            InitializeComponent();
        }

        private void frmAutorizarAltaVino_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_AutorizarAltaVino"] = lblTitulo;
            _defaults["lblTitulo_AutorizarAltaVino"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarPendientes();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmAutorizarAltaVino_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvPendientes.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvPendientes.Columns["Codigo"] != null)
                dgvPendientes.Columns["Codigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvPendientes.Columns["Nombre"] != null)
                dgvPendientes.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvPendientes.Columns["BodegaNombre"] != null)
                dgvPendientes.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvPendientes.Columns["Varietal"] != null)
                dgvPendientes.Columns["Varietal"].HeaderText = mgr.Traducir("colhdr_Varietal") ?? "Varietal";
            if (dgvPendientes.Columns["Aniada"] != null)
                dgvPendientes.Columns["Aniada"].HeaderText = mgr.Traducir("colhdr_Aniada") ?? "Añada";
            if (dgvPendientes.Columns["Precio"] != null)
                dgvPendientes.Columns["Precio"].HeaderText = mgr.Traducir("colhdr_Precio") ?? "Precio";
            if (dgvPendientes.Columns["StockMinimo"] != null)
                dgvPendientes.Columns["StockMinimo"].HeaderText = mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo";
            if (dgvPendientes.Columns["Maridaje"] != null)
                dgvPendientes.Columns["Maridaje"].HeaderText = mgr.Traducir("colhdr_Maridaje") ?? "Maridaje";
            if (dgvPendientes.Columns["Puntaje"] != null)
                dgvPendientes.Columns["Puntaje"].HeaderText = mgr.Traducir("colhdr_Puntaje") ?? "Puntaje";
            if (dgvPendientes.Columns["CreadoPorLogin"] != null)
                dgvPendientes.Columns["CreadoPorLogin"].HeaderText = mgr.Traducir("colhdr_CreadoPor") ?? "Creado por";
            if (dgvPendientes.Columns["FechaAlta"] != null)
                dgvPendientes.Columns["FechaAlta"].HeaderText = mgr.Traducir("colhdr_FechaAlta") ?? "Fecha de alta";
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

        private void CargarPendientes()
        {
            dgvPendientes.DataSource = _bll.ListarPendientes();

            if (dgvPendientes.Columns.Count > 0)
            {
                if (dgvPendientes.Columns["Id"] != null) dgvPendientes.Columns["Id"].Visible = false;
                if (dgvPendientes.Columns["BodegaId"] != null) dgvPendientes.Columns["BodegaId"].Visible = false;
                if (dgvPendientes.Columns["CreadoPor"] != null) dgvPendientes.Columns["CreadoPor"].Visible = false;
                if (dgvPendientes.Columns["AutorizadoPor"] != null) dgvPendientes.Columns["AutorizadoPor"].Visible = false;
                if (dgvPendientes.Columns["AutorizadoPorLogin"] != null) dgvPendientes.Columns["AutorizadoPorLogin"].Visible = false;
                if (dgvPendientes.Columns["FechaAutorizacion"] != null) dgvPendientes.Columns["FechaAutorizacion"].Visible = false;
                if (dgvPendientes.Columns["Estado"] != null) dgvPendientes.Columns["Estado"].Visible = false;
                ActualizarEncabezados();
            }

            ActualizarEstadoBoton();
        }

        private void dgvPendientes_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBoton();
        }

        private void ActualizarEstadoBoton()
        {
            BE.Vino seleccionado = dgvPendientes.CurrentRow?.DataBoundItem as BE.Vino;
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            // Capa UI del triple guard de RN-01: la acción queda deshabilitada
            // para el solicitante, aunque BLL y SP también la rechacen.
            btnAutorizar.Enabled = seleccionado != null && seleccionado.CreadoPor != sesion.Id;
        }

        private void btnAutorizar_Click(object sender, EventArgs e)
        {
            BE.Vino seleccionado = dgvPendientes.CurrentRow?.DataBoundItem as BE.Vino;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un vino pendiente.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.Autorizar(seleccionado.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Vino autorizado correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarPendientes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
