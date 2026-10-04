using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // CU-26: consulta pasiva (el usuario la abre; no hay aviso push al iniciar sesión).
    public partial class frmAlertaStockMinimo : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.VinoBLL _bll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmAlertaStockMinimo()
        {
            InitializeComponent();
        }

        private void frmAlertaStockMinimo_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_AlertaStock"] = lblTitulo;
            _defaults["lblTitulo_AlertaStock"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarAlertas();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmAlertaStockMinimo_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvAlertas.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvAlertas.Columns["Codigo"] != null)
                dgvAlertas.Columns["Codigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvAlertas.Columns["Nombre"] != null)
                dgvAlertas.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvAlertas.Columns["BodegaNombre"] != null)
                dgvAlertas.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvAlertas.Columns["StockMinimo"] != null)
                dgvAlertas.Columns["StockMinimo"].HeaderText = mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo";
            if (dgvAlertas.Columns["StockActual"] != null)
                dgvAlertas.Columns["StockActual"].HeaderText = mgr.Traducir("colhdr_StockActual") ?? "Stock actual";
            if (dgvAlertas.Columns["Faltante"] != null)
                dgvAlertas.Columns["Faltante"].HeaderText = mgr.Traducir("colhdr_Faltante") ?? "Faltante";
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

        private void CargarAlertas()
        {
            List<BE.AlertaStockMinimo> alertas = _bll.ListarAlertaStockMinimo();
            dgvAlertas.DataSource = alertas;

            if (dgvAlertas.Columns.Count > 0)
            {
                if (dgvAlertas.Columns["VinoId"] != null) dgvAlertas.Columns["VinoId"].Visible = false;
                ActualizarEncabezados();
            }

            lblSinAlertas.Visible = alertas.Count == 0;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
