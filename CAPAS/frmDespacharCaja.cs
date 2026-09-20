using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmDespacharCaja : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.CajaMensualBLL _bll = new BLL.CajaMensualBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();
        private readonly BE.USUARIO _sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

        public frmDespacharCaja()
        {
            InitializeComponent();
        }

        private void frmDespacharCaja_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_DespacharCaja"] = lblTitulo;
            _defaults["lblTitulo_DespacharCaja"] = lblTitulo.Text;

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarCajasArmadas();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmDespacharCaja_FormClosed(object sender, FormClosedEventArgs e)
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
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();

            if (dgvCajasArmadas.Columns.Count > 0)
            {
                if (dgvCajasArmadas.Columns["SocioNombre"] != null)
                    dgvCajasArmadas.Columns["SocioNombre"].HeaderText = mgr.Traducir("colhdr_Socio") ?? "Socio";
                if (dgvCajasArmadas.Columns["Periodo"] != null)
                    dgvCajasArmadas.Columns["Periodo"].HeaderText = mgr.Traducir("colhdr_Periodo") ?? "Período";
                if (dgvCajasArmadas.Columns["ArmadoPorLogin"] != null)
                    dgvCajasArmadas.Columns["ArmadoPorLogin"].HeaderText = mgr.Traducir("colhdr_ArmadoPor") ?? "Armada por";
            }

            if (dgvComposicionEfectiva.Columns.Count > 0)
            {
                if (dgvComposicionEfectiva.Columns["NombreEfectivo"] != null)
                    dgvComposicionEfectiva.Columns["NombreEfectivo"].HeaderText = mgr.Traducir("colhdr_VinoEfectivo") ?? "Vino";
                if (dgvComposicionEfectiva.Columns["Cantidad"] != null)
                    dgvComposicionEfectiva.Columns["Cantidad"].HeaderText = mgr.Traducir("colhdr_Cantidad") ?? "Cantidad";
                if (dgvComposicionEfectiva.Columns["StockDisponible"] != null)
                    dgvComposicionEfectiva.Columns["StockDisponible"].HeaderText = mgr.Traducir("colhdr_StockDisponible") ?? "Stock disponible";
                if (dgvComposicionEfectiva.Columns["Sustituido"] != null)
                    dgvComposicionEfectiva.Columns["Sustituido"].HeaderText = mgr.Traducir("colhdr_Sustituido") ?? "Sustituido";
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

        private void CargarCajasArmadas()
        {
            dgvCajasArmadas.DataSource = _bll.ListarArmadas();
            if (dgvCajasArmadas.Columns.Count > 0)
            {
                if (dgvCajasArmadas.Columns["Id"] != null) dgvCajasArmadas.Columns["Id"].Visible = false;
                if (dgvCajasArmadas.Columns["SocioId"] != null) dgvCajasArmadas.Columns["SocioId"].Visible = false;
                if (dgvCajasArmadas.Columns["Estado"] != null) dgvCajasArmadas.Columns["Estado"].Visible = false;
                if (dgvCajasArmadas.Columns["PresupuestoSnapshot"] != null) dgvCajasArmadas.Columns["PresupuestoSnapshot"].Visible = false;
                if (dgvCajasArmadas.Columns["FechaArmado"] != null) dgvCajasArmadas.Columns["FechaArmado"].Visible = false;
                if (dgvCajasArmadas.Columns["ArmadoPor"] != null) dgvCajasArmadas.Columns["ArmadoPor"].Visible = false;
                if (dgvCajasArmadas.Columns["FechaDespacho"] != null) dgvCajasArmadas.Columns["FechaDespacho"].Visible = false;
                if (dgvCajasArmadas.Columns["DespachadoPor"] != null) dgvCajasArmadas.Columns["DespachadoPor"].Visible = false;
                if (dgvCajasArmadas.Columns["DespachadoPorLogin"] != null) dgvCajasArmadas.Columns["DespachadoPorLogin"].Visible = false;
                if (dgvCajasArmadas.Columns["FechaCancelacion"] != null) dgvCajasArmadas.Columns["FechaCancelacion"].Visible = false;
                if (dgvCajasArmadas.Columns["CanceladaPor"] != null) dgvCajasArmadas.Columns["CanceladaPor"].Visible = false;
                if (dgvCajasArmadas.Columns["MotivoCancelacion"] != null) dgvCajasArmadas.Columns["MotivoCancelacion"].Visible = false;
                if (dgvCajasArmadas.Columns["Lineas"] != null) dgvCajasArmadas.Columns["Lineas"].Visible = false;
                ActualizarEncabezados();
            }
        }

        // btnDespachar deshabilitado si el usuario logueado armó la caja seleccionada —
        // capa UI de RN-10 (BLL y la propia SP CAJA_DESPACHAR repiten el guard).
        private void dgvCajasArmadas_SelectionChanged(object sender, EventArgs e)
        {
            if (!(dgvCajasArmadas.CurrentRow?.DataBoundItem is BE.CajaMensual cajaFila))
            {
                dgvComposicionEfectiva.DataSource = null;
                btnDespachar.Enabled = false;
                return;
            }

            BE.CajaMensual caja = _bll.ObtenerConComposicionEfectiva(cajaFila.Id);

            dgvComposicionEfectiva.DataSource = caja.Lineas;
            if (dgvComposicionEfectiva.Columns.Count > 0)
            {
                if (dgvComposicionEfectiva.Columns["Id"] != null) dgvComposicionEfectiva.Columns["Id"].Visible = false;
                if (dgvComposicionEfectiva.Columns["CajaId"] != null) dgvComposicionEfectiva.Columns["CajaId"].Visible = false;
                if (dgvComposicionEfectiva.Columns["VinoId"] != null) dgvComposicionEfectiva.Columns["VinoId"].Visible = false;
                if (dgvComposicionEfectiva.Columns["NombreSnapshot"] != null) dgvComposicionEfectiva.Columns["NombreSnapshot"].Visible = false;
                if (dgvComposicionEfectiva.Columns["PrecioSnapshot"] != null) dgvComposicionEfectiva.Columns["PrecioSnapshot"].Visible = false;
                if (dgvComposicionEfectiva.Columns["VinoEfectivoId"] != null) dgvComposicionEfectiva.Columns["VinoEfectivoId"].Visible = false;
                if (dgvComposicionEfectiva.Columns["PrecioEfectivo"] != null) dgvComposicionEfectiva.Columns["PrecioEfectivo"].Visible = false;
                ActualizarEncabezados();
            }

            btnDespachar.Enabled = caja.ArmadoPor != _sesion.Id;
        }

        // RN-03: resalta las líneas sin stock suficiente — el despacho las bloquearía
        // todas de forma atómica (rollback completo en CAJA_DESPACHAR, no parcial).
        private void dgvComposicionEfectiva_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(dgvComposicionEfectiva.Rows[e.RowIndex].DataBoundItem is BE.CajaVino linea)) return;
            if (linea.StockDisponible < linea.Cantidad)
            {
                e.CellStyle.BackColor = Color.MistyRose;
                e.CellStyle.ForeColor = Color.DarkRed;
            }
        }

        private void btnDespachar_Click(object sender, EventArgs e)
        {
            if (!(dgvCajasArmadas.CurrentRow?.DataBoundItem is BE.CajaMensual caja)) return;

            DialogResult confirmacion = MsgBox.Show(
                "¿Confirma el despacho de esta caja? La acción no se puede deshacer.",
                "Confirmar despacho", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta);
            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _bll.Despachar(caja.Id, _sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Caja despachada correctamente.", "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarCajasArmadas();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
