using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmPickingCaja : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.CajaMensualBLL _bll = new BLL.CajaMensualBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();
        private readonly BE.USUARIO _sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

        public frmPickingCaja()
        {
            InitializeComponent();
        }

        private void frmPickingCaja_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_PickingCaja"] = lblTitulo;
            _defaults["lblTitulo_PickingCaja"] = lblTitulo.Text;

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarCajasArmadas();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmPickingCaja_FormClosed(object sender, FormClosedEventArgs e)
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

            if (dgvTrazaSustituciones.Columns.Count > 0)
            {
                if (dgvTrazaSustituciones.Columns["NombreSnapshot"] != null)
                    dgvTrazaSustituciones.Columns["NombreSnapshot"].HeaderText = mgr.Traducir("colhdr_VinoReemplazo") ?? "Reemplazo";
                if (dgvTrazaSustituciones.Columns["Motivo"] != null)
                    dgvTrazaSustituciones.Columns["Motivo"].HeaderText = mgr.Traducir("colhdr_MotivoSustitucion") ?? "Motivo";
                if (dgvTrazaSustituciones.Columns["ResponsableLogin"] != null)
                    dgvTrazaSustituciones.Columns["ResponsableLogin"].HeaderText = mgr.Traducir("colhdr_Responsable_Sust") ?? "Responsable";
                if (dgvTrazaSustituciones.Columns["Fecha"] != null)
                    dgvTrazaSustituciones.Columns["Fecha"].HeaderText = mgr.Traducir("colhdr_Fecha") ?? "Fecha y Hora";
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
                // Datos de contacto del socio: se usan para el remito (RN-14), no para la grilla.
                if (dgvCajasArmadas.Columns["SocioDomicilio"] != null) dgvCajasArmadas.Columns["SocioDomicilio"].Visible = false;
                if (dgvCajasArmadas.Columns["SocioTelefono"] != null) dgvCajasArmadas.Columns["SocioTelefono"].Visible = false;
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

        private void dgvCajasArmadas_SelectionChanged(object sender, EventArgs e)
        {
            CargarComposicion();
        }

        // btnRegistrarSustitucion queda deshabilitado si el socio logueado armó la caja
        // seleccionada — capa UI de RN-10 (BLL y SP repiten el guard, triple defensa).
        private void CargarComposicion()
        {
            if (!(dgvCajasArmadas.CurrentRow?.DataBoundItem is BE.CajaMensual cajaFila))
            {
                dgvComposicionEfectiva.DataSource = null;
                dgvTrazaSustituciones.DataSource = null;
                cboVinoReemplazo.DataSource = null;
                btnRegistrarSustitucion.Enabled = false;
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

            cboVinoReemplazo.DataSource = _bll.ListarCandidatos(caja.SocioId);

            dgvTrazaSustituciones.DataSource = _bll.ListarTrazaSustituciones(caja.Id);
            if (dgvTrazaSustituciones.Columns.Count > 0)
            {
                if (dgvTrazaSustituciones.Columns["Id"] != null) dgvTrazaSustituciones.Columns["Id"].Visible = false;
                if (dgvTrazaSustituciones.Columns["CajaVinoId"] != null) dgvTrazaSustituciones.Columns["CajaVinoId"].Visible = false;
                if (dgvTrazaSustituciones.Columns["VinoOriginalId"] != null) dgvTrazaSustituciones.Columns["VinoOriginalId"].Visible = false;
                if (dgvTrazaSustituciones.Columns["VinoReemplazoId"] != null) dgvTrazaSustituciones.Columns["VinoReemplazoId"].Visible = false;
                if (dgvTrazaSustituciones.Columns["PrecioSnapshot"] != null) dgvTrazaSustituciones.Columns["PrecioSnapshot"].Visible = false;
                if (dgvTrazaSustituciones.Columns["ResponsableId"] != null) dgvTrazaSustituciones.Columns["ResponsableId"].Visible = false;
                ActualizarEncabezados();
            }

            btnRegistrarSustitucion.Enabled = caja.ArmadoPor != _sesion.Id;
        }

        // RN-03 no bloquea la sustitución (solo el despacho); esto es una señal visual
        // para que el encargado de depósito priorice qué líneas necesitan reemplazo.
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

        private void btnRegistrarSustitucion_Click(object sender, EventArgs e)
        {
            if (!(dgvCajasArmadas.CurrentRow?.DataBoundItem is BE.CajaMensual caja)) return;

            if (!(dgvComposicionEfectiva.CurrentRow?.DataBoundItem is BE.CajaVino linea))
            {
                MsgBox.Show("Seleccioná una línea de la composición.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            if (!(cboVinoReemplazo.SelectedItem is BE.VinoCandidato reemplazo))
            {
                MsgBox.Show("Seleccioná el vino de reemplazo.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                MsgBox.Show("Ingresá un motivo para la sustitución.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            try
            {
                _bll.RegistrarSustitucion(caja.Id, linea.Id, reemplazo.Id, txtMotivo.Text.Trim(), _sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Sustitución registrada correctamente.", "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            txtMotivo.Clear();
            CargarComposicion();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
