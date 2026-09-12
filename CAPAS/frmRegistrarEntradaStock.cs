using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmRegistrarEntradaStock : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.MovimientoStockBLL _bll = new BLL.MovimientoStockBLL();
        private readonly BLL.VinoBLL _vinoBll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmRegistrarEntradaStock()
        {
            InitializeComponent();
        }

        private void frmRegistrarEntradaStock_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_EntradaStock"] = lblTitulo;
            _defaults["lblTitulo_EntradaStock"] = lblTitulo.Text;

            // El NumericUpDown refleja el valor cargado por el usuario: si quedara en
            // _controles, un cambio de idioma lo pisaría (mismo criterio que frmCatalogoVinos).
            _controles.Remove("numCantidad");
            _defaults.Remove("numCantidad");

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarVinos();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmRegistrarEntradaStock_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvMovimientos.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvMovimientos.Columns["Fecha"] != null)
                dgvMovimientos.Columns["Fecha"].HeaderText = mgr.Traducir("colhdr_Fecha") ?? "Fecha y Hora";
            if (dgvMovimientos.Columns["Tipo"] != null)
                dgvMovimientos.Columns["Tipo"].HeaderText = mgr.Traducir("colhdr_TipoMovimiento") ?? "Tipo de movimiento";
            if (dgvMovimientos.Columns["Cantidad"] != null)
                dgvMovimientos.Columns["Cantidad"].HeaderText = mgr.Traducir("colhdr_Cantidad") ?? "Cantidad";
            if (dgvMovimientos.Columns["Motivo"] != null)
                dgvMovimientos.Columns["Motivo"].HeaderText = mgr.Traducir("colhdr_Motivo") ?? "Motivo";
            if (dgvMovimientos.Columns["Responsable"] != null)
                dgvMovimientos.Columns["Responsable"].HeaderText = mgr.Traducir("colhdr_Responsable") ?? "Responsable";
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

        private void CargarVinos()
        {
            cboVino.DataSource = _vinoBll.ListarAutorizados();
            cboVino.DisplayMember = "Nombre";
            cboVino.ValueMember = "Id";
            if (cboVino.Items.Count > 0) cboVino.SelectedIndex = 0;
            ActualizarVinoSeleccionado();
        }

        private void cboVino_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarVinoSeleccionado();
        }

        private void ActualizarVinoSeleccionado()
        {
            if (!(cboVino.SelectedItem is BE.Vino vino))
            {
                lblStockActualValor.Text = "-";
                dgvMovimientos.DataSource = null;
                return;
            }

            lblStockActualValor.Text = _bll.ObtenerStockActual(vino.Id).ToString();
            CargarMovimientos(vino.Id);
        }

        private void CargarMovimientos(int vinoId)
        {
            dgvMovimientos.DataSource = _bll.ObtenerMovimientos(vinoId)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            if (dgvMovimientos.Columns.Count > 0)
            {
                if (dgvMovimientos.Columns["Id"] != null) dgvMovimientos.Columns["Id"].Visible = false;
                if (dgvMovimientos.Columns["VinoId"] != null) dgvMovimientos.Columns["VinoId"].Visible = false;
                if (dgvMovimientos.Columns["VinoNombre"] != null) dgvMovimientos.Columns["VinoNombre"].Visible = false;
                if (dgvMovimientos.Columns["ReferenciaTipo"] != null) dgvMovimientos.Columns["ReferenciaTipo"].Visible = false;
                if (dgvMovimientos.Columns["ReferenciaId"] != null) dgvMovimientos.Columns["ReferenciaId"].Visible = false;
                ActualizarEncabezados();
            }
        }

        private void btnRegistrarEntrada_Click(object sender, EventArgs e)
        {
            if (!(cboVino.SelectedItem is BE.Vino vino))
            {
                MsgBox.Show("Seleccioná un vino.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            BE.MovimientoStock m = new BE.MovimientoStock
            {
                VinoId = vino.Id,
                Cantidad = (int)numCantidad.Value,
                Motivo = string.IsNullOrWhiteSpace(txtMotivo.Text) ? null : txtMotivo.Text.Trim()
            };

            try
            {
                _bll.RegistrarEntrada(m, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Entrada de stock registrada correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            numCantidad.Value = 0;
            txtMotivo.Clear();
            ActualizarVinoSeleccionado();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
