using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmHistorialDespachos : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.CajaMensualBLL _bll = new BLL.CajaMensualBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmHistorialDespachos()
        {
            InitializeComponent();
        }

        private void frmHistorialDespachos_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_HistorialDespachos"] = lblTitulo;
            _defaults["lblTitulo_HistorialDespachos"] = lblTitulo.Text;

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarCajasDespachadas();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmHistorialDespachos_FormClosed(object sender, FormClosedEventArgs e)
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

            if (dgvCajasDespachadas.Columns.Count > 0)
            {
                if (dgvCajasDespachadas.Columns["SocioNombre"] != null)
                    dgvCajasDespachadas.Columns["SocioNombre"].HeaderText = mgr.Traducir("colhdr_Socio") ?? "Socio";
                if (dgvCajasDespachadas.Columns["Periodo"] != null)
                    dgvCajasDespachadas.Columns["Periodo"].HeaderText = mgr.Traducir("colhdr_Periodo") ?? "Período";
                if (dgvCajasDespachadas.Columns["FechaDespacho"] != null)
                    dgvCajasDespachadas.Columns["FechaDespacho"].HeaderText = mgr.Traducir("colhdr_FechaDespacho") ?? "Fecha de despacho";
                if (dgvCajasDespachadas.Columns["DespachadoPorLogin"] != null)
                    dgvCajasDespachadas.Columns["DespachadoPorLogin"].HeaderText = mgr.Traducir("colhdr_DespachadoPor") ?? "Despachada por";
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

        private void CargarCajasDespachadas()
        {
            dgvCajasDespachadas.DataSource = _bll.ListarDespachadas();
            if (dgvCajasDespachadas.Columns.Count > 0)
            {
                if (dgvCajasDespachadas.Columns["Id"] != null) dgvCajasDespachadas.Columns["Id"].Visible = false;
                if (dgvCajasDespachadas.Columns["SocioId"] != null) dgvCajasDespachadas.Columns["SocioId"].Visible = false;
                if (dgvCajasDespachadas.Columns["Estado"] != null) dgvCajasDespachadas.Columns["Estado"].Visible = false;
                if (dgvCajasDespachadas.Columns["PresupuestoSnapshot"] != null) dgvCajasDespachadas.Columns["PresupuestoSnapshot"].Visible = false;
                if (dgvCajasDespachadas.Columns["FechaArmado"] != null) dgvCajasDespachadas.Columns["FechaArmado"].Visible = false;
                if (dgvCajasDespachadas.Columns["ArmadoPor"] != null) dgvCajasDespachadas.Columns["ArmadoPor"].Visible = false;
                if (dgvCajasDespachadas.Columns["ArmadoPorLogin"] != null) dgvCajasDespachadas.Columns["ArmadoPorLogin"].Visible = false;
                if (dgvCajasDespachadas.Columns["DespachadoPor"] != null) dgvCajasDespachadas.Columns["DespachadoPor"].Visible = false;
                if (dgvCajasDespachadas.Columns["FechaCancelacion"] != null) dgvCajasDespachadas.Columns["FechaCancelacion"].Visible = false;
                if (dgvCajasDespachadas.Columns["CanceladaPor"] != null) dgvCajasDespachadas.Columns["CanceladaPor"].Visible = false;
                if (dgvCajasDespachadas.Columns["MotivoCancelacion"] != null) dgvCajasDespachadas.Columns["MotivoCancelacion"].Visible = false;
                if (dgvCajasDespachadas.Columns["Lineas"] != null) dgvCajasDespachadas.Columns["Lineas"].Visible = false;
                ActualizarEncabezados();
            }
        }

        private void dgvCajasDespachadas_SelectionChanged(object sender, EventArgs e)
        {
            btnRegenerar.Enabled = dgvCajasDespachadas.CurrentRow?.DataBoundItem is BE.CajaMensual;
        }

        // RN-12: regenerar sobrescribe Remito_Caja{id}.pdf en el mismo path, sin
        // sufijo de timestamp — RemitoDespachoPdf.Generar ya trunca el archivo
        // existente (PdfDocument.Save). Misma semántica best-effort que CU-31
        // (RN-11): una falla acá no afecta el estado de la caja, que ya está
        // Despachada y es inmutable desde este form.
        private void btnRegenerar_Click(object sender, EventArgs e)
        {
            if (!(dgvCajasDespachadas.CurrentRow?.DataBoundItem is BE.CajaMensual cajaFila)) return;

            try
            {
                BE.CajaMensual caja = _bll.ObtenerConComposicionEfectiva(cajaFila.Id);
                string rutaRemito = RemitoDespachoPdf.Generar(caja);
                RemitoDespachoPdf.Abrir(rutaRemito);
            }
            catch (Exception)
            {
                MsgBox.Show("No se pudo generar el remito de despacho; la caja fue despachada igualmente.",
                    "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
