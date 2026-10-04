using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmAutorizarBajaVino : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.VinoBLL _bll = new BLL.VinoBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmAutorizarBajaVino()
        {
            InitializeComponent();
        }

        private void frmAutorizarBajaVino_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_AutorizarBajaVino"] = lblTitulo;
            _defaults["lblTitulo_AutorizarBajaVino"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            // En Load la grilla todavía no tiene fila actual: si hay una sola fila, SelectionChanged
            // no vuelve a dispararse y el botón quedaría deshabilitado. Se reevalúa al terminar el
            // enlace de datos y al mostrarse el formulario.
            dgvPendientesBaja.DataBindingComplete += (s, ev) => ActualizarEstadoBotones();
            this.Shown += (s, ev) => ActualizarEstadoBotones();
            CargarPendientes();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmAutorizarBajaVino_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvPendientesBaja.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvPendientesBaja.Columns["Codigo"] != null)
                dgvPendientesBaja.Columns["Codigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvPendientesBaja.Columns["Nombre"] != null)
                dgvPendientesBaja.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvPendientesBaja.Columns["BodegaNombre"] != null)
                dgvPendientesBaja.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvPendientesBaja.Columns["Varietal"] != null)
                dgvPendientesBaja.Columns["Varietal"].HeaderText = mgr.Traducir("colhdr_Varietal") ?? "Varietal";
            if (dgvPendientesBaja.Columns["Aniada"] != null)
                dgvPendientesBaja.Columns["Aniada"].HeaderText = mgr.Traducir("colhdr_Aniada") ?? "Añada";
            if (dgvPendientesBaja.Columns["Precio"] != null)
                dgvPendientesBaja.Columns["Precio"].HeaderText = mgr.Traducir("colhdr_Precio") ?? "Precio";
            if (dgvPendientesBaja.Columns["StockMinimo"] != null)
                dgvPendientesBaja.Columns["StockMinimo"].HeaderText = mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo";
            if (dgvPendientesBaja.Columns["BajaSolicitadaPorLogin"] != null)
                dgvPendientesBaja.Columns["BajaSolicitadaPorLogin"].HeaderText = mgr.Traducir("colhdr_BajaSolicitadaPor") ?? "Baja solicitada por";
            if (dgvPendientesBaja.Columns["FechaSolicitudBaja"] != null)
                dgvPendientesBaja.Columns["FechaSolicitudBaja"].HeaderText = mgr.Traducir("colhdr_FechaSolicitudBaja") ?? "Fecha de solicitud de baja";
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
            dgvPendientesBaja.DataSource = _bll.ListarPendientesDeBaja();

            if (dgvPendientesBaja.Columns.Count > 0)
            {
                string[] ocultas =
                {
                    "Id", "BodegaId", "Estado", "Maridaje", "Puntaje", "CreadoPor", "CreadoPorLogin",
                    "FechaAlta", "AutorizadoPor", "AutorizadoPorLogin", "FechaAutorizacion",
                    "BajaSolicitadaPor", "DescontinuadoPor", "FechaDescontinuacion"
                };
                foreach (string nombre in ocultas)
                    if (dgvPendientesBaja.Columns[nombre] != null)
                        dgvPendientesBaja.Columns[nombre].Visible = false;
                ActualizarEncabezados();
            }

            ActualizarEstadoBotones();
        }

        private void dgvPendientesBaja_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void ActualizarEstadoBotones()
        {
            BE.Vino seleccionado = dgvPendientesBaja.CurrentRow?.DataBoundItem as BE.Vino;
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            // Capa UI del triple guard de RN-01: quien solicitó la baja no puede autorizarla,
            // aunque BLL y SP también la rechacen.
            btnAutorizarBaja.Enabled = seleccionado != null && seleccionado.BajaSolicitadaPor != sesion.Id;
            btnRechazarBaja.Enabled = seleccionado != null;
        }

        private void btnAutorizarBaja_Click(object sender, EventArgs e)
        {
            BE.Vino seleccionado = dgvPendientesBaja.CurrentRow?.DataBoundItem as BE.Vino;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un vino con baja solicitada.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show("¿Autorizar la descontinuación del vino '" + seleccionado.Nombre + "'? El vino dejará de estar disponible.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Atencion) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.AutorizarBaja(seleccionado.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Vino descontinuado correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarPendientes();
        }

        private void btnRechazarBaja_Click(object sender, EventArgs e)
        {
            BE.Vino seleccionado = dgvPendientesBaja.CurrentRow?.DataBoundItem as BE.Vino;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un vino con baja solicitada.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show("¿Rechazar la descontinuación del vino '" + seleccionado.Nombre + "'? El vino seguirá activo.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.RechazarBaja(seleccionado.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Solicitud de descontinuación rechazada. El vino sigue activo.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarPendientes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
