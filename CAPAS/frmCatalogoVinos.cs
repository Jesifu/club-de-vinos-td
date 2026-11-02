using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmCatalogoVinos : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.VinoBLL _bll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();
        // Modo re-propuesta (R2): Id del vino rechazado que se está corrigiendo; null = alta nueva.
        private int? _vinoReproponerId;

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
            if (dgvVinos.Columns["Estado"] != null)
                dgvVinos.Columns["Estado"].HeaderText = mgr.Traducir("colhdr_Estado") ?? "Estado";
            if (dgvVinos.Columns["UltimaSolicitudTipo"] != null)
                dgvVinos.Columns["UltimaSolicitudTipo"].HeaderText = mgr.Traducir("colhdr_UltimaSolicitud") ?? "Última solicitud";
            if (dgvVinos.Columns["UltimaSolicitudEstado"] != null)
                dgvVinos.Columns["UltimaSolicitudEstado"].HeaderText = mgr.Traducir("colhdr_EstadoSolicitud") ?? "Estado de la solicitud";
            if (dgvVinos.Columns["UltimoMotivoRechazo"] != null)
                dgvVinos.Columns["UltimoMotivoRechazo"].HeaderText = mgr.Traducir("colhdr_MotivoRechazo") ?? "Motivo de rechazo";
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

            // Activos más los propios pendientes o rechazados, con la última solicitud de cada vino (R7).
            dgvVinos.DataSource = _bll.ListarCatalogo(sesion);

            if (dgvVinos.Columns.Count > 0)
            {
                if (dgvVinos.Columns["Id"] != null) dgvVinos.Columns["Id"].Visible = false;
                if (dgvVinos.Columns["BodegaId"] != null) dgvVinos.Columns["BodegaId"].Visible = false;
                if (dgvVinos.Columns["CreadoPor"] != null) dgvVinos.Columns["CreadoPor"].Visible = false;
                ActualizarEncabezados();
            }

            ActualizarEstadoBajaBoton();
        }

        private void dgvVinos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBajaBoton();
        }

        // CU-24: la baja solo se solicita para un vino Activo sin otra baja pendiente. La baja pendiente
        // surge de la última solicitud del vino (tipo BAJA en estado Pendiente).
        // La re-propuesta (R2) solo aplica a un vino Rechazado.
        private void ActualizarEstadoBajaBoton()
        {
            BE.Vino seleccionado = dgvVinos.CurrentRow?.DataBoundItem as BE.Vino;
            bool bajaPendiente = seleccionado != null
                                 && seleccionado.UltimaSolicitudTipo == BE.TipoSolicitud.Baja
                                 && seleccionado.UltimaSolicitudEstado == BE.EstadoSolicitud.Pendiente;
            btnSolicitarBaja.Enabled = seleccionado != null
                                       && seleccionado.Estado == BE.EstadoVino.Activo
                                       && !bajaPendiente;
            btnReproponer.Enabled = seleccionado != null
                                    && seleccionado.Estado == BE.EstadoVino.Rechazado;
        }

        // Tipo y estado de la última solicitud son enums (TipoSolicitud/EstadoSolicitud): se muestran traducidos.
        private void dgvVinos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            string columna = dgvVinos.Columns[e.ColumnIndex].Name;
            if (columna != "UltimaSolicitudTipo" && columna != "UltimaSolicitudEstado") return;

            // El nombre del enum coincide con la clave: valsol_Alta, valsol_Pendiente, etc.
            string valor = e.Value.ToString();
            e.Value = SeguridadYServicios.IdiomaManager.getInstance().Traducir("valsol_" + valor) ?? valor;
            e.FormattingApplied = true;
        }

        // R2: carga el vino rechazado en el formulario para corregirlo. El SKU no se puede cambiar.
        private void btnReproponer_Click(object sender, EventArgs e)
        {
            BE.Vino seleccionado = dgvVinos.CurrentRow?.DataBoundItem as BE.Vino;
            if (seleccionado == null || seleccionado.Estado != BE.EstadoVino.Rechazado)
            {
                MsgBox.Show("Seleccione un vino rechazado.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            LimpiarFormulario();
            _vinoReproponerId = seleccionado.Id;
            txtCodigo.Text = seleccionado.Codigo;
            txtCodigo.ReadOnly = true;
            txtNombre.Text = seleccionado.Nombre;
            cboBodega.SelectedValue = seleccionado.BodegaId;
            txtVarietal.Text = seleccionado.Varietal;
            numAniada.Value = Math.Min(Math.Max(seleccionado.Aniada, numAniada.Minimum), numAniada.Maximum);
            numPrecio.Value = Math.Min(seleccionado.Precio, numPrecio.Maximum);
            numStockMinimo.Value = Math.Min(seleccionado.StockMinimo, numStockMinimo.Maximum);
            txtMaridaje.Text = seleccionado.Maridaje ?? string.Empty;
            // chkPuntaje habilita numPuntaje y lo pone en 0 al desmarcarse: primero el check, después el valor.
            chkPuntaje.Checked = seleccionado.Puntaje.HasValue;
            if (seleccionado.Puntaje.HasValue) numPuntaje.Value = seleccionado.Puntaje.Value;
            lblAyudaReproponer.Visible = true;
            txtNombre.Focus();
        }

        private void btnSolicitarBaja_Click(object sender, EventArgs e)
        {
            BE.Vino seleccionado = dgvVinos.CurrentRow?.DataBoundItem as BE.Vino;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un vino.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show("¿Solicitar la descontinuación del vino '" + seleccionado.Nombre + "'?",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.SolicitarBaja(seleccionado.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarVinos();
                return;
            }

            MsgBox.Show("Descontinuación solicitada, pendiente de autorización.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarVinos();
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

            bool esReproposicion = _vinoReproponerId.HasValue;
            try
            {
                if (esReproposicion)
                {
                    v.Id = _vinoReproponerId.Value;
                    _bll.Reproponer(v, sesion);
                }
                else
                {
                    _bll.ProponerAlta(v, sesion);
                }
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                // Una re-propuesta fallida puede deberse a que el vino ya cambió de estado: se refresca la grilla.
                if (esReproposicion) CargarVinos();
                return;
            }

            MsgBox.Show(esReproposicion
                    ? "Vino repropuesto correctamente. Se emitió una nueva solicitud de alta, pendiente de autorización."
                    : "Vino propuesto correctamente. Queda pendiente de autorización.",
                "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            LimpiarFormulario();
            CargarVinos();
        }

        private void btnLimpiarVino_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            // Sale del modo re-propuesta: el SKU vuelve a ser editable.
            _vinoReproponerId = null;
            txtCodigo.ReadOnly = false;
            lblAyudaReproponer.Visible = false;
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
