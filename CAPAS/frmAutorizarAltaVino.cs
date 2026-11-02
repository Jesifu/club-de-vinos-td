using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmAutorizarAltaVino : FormBase, SeguridadYServicios.IObservadorIdioma
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
            // En Load la grilla todavía no tiene fila actual: si hay una sola fila, SelectionChanged
            // no vuelve a dispararse y los botones quedarían deshabilitados. Se reevalúa al terminar el
            // enlace de datos y al mostrarse el formulario.
            dgvPendientes.DataBindingComplete += (s, ev) => ActualizarSeleccion();
            this.Shown += (s, ev) => ActualizarSeleccion();
            CargarPendientes();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
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
            // El panel de detalle se arma con texto traducido: se vuelve a generar.
            MostrarDetalle();
            dgvPendientes.Invalidate();
        }

        private void ActualizarEncabezados()
        {
            if (dgvPendientes.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvPendientes.Columns["Tipo"] != null)
                dgvPendientes.Columns["Tipo"].HeaderText = mgr.Traducir("colhdr_TipoSolicitud") ?? "Tipo";
            if (dgvPendientes.Columns["VinoCodigo"] != null)
                dgvPendientes.Columns["VinoCodigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvPendientes.Columns["VinoNombre"] != null)
                dgvPendientes.Columns["VinoNombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvPendientes.Columns["BodegaNombre"] != null)
                dgvPendientes.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvPendientes.Columns["EmisorLogin"] != null)
                dgvPendientes.Columns["EmisorLogin"].HeaderText = mgr.Traducir("colhdr_Emisor") ?? "Emitida por";
            if (dgvPendientes.Columns["FechaEmision"] != null)
                dgvPendientes.Columns["FechaEmision"].HeaderText = mgr.Traducir("colhdr_FechaEmision") ?? "Fecha de emisión";
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
            dgvPendientes.DataSource = _bll.ListarSolicitudesPendientes(BE.TipoSolicitud.Alta);

            if (dgvPendientes.Columns.Count > 0)
            {
                // Los datos completos del vino se muestran en el panel de detalle, no en la grilla.
                foreach (string col in new[] { "Id", "VinoId", "Estado", "EmisorId", "ResolutorId", "ResolutorLogin",
                                               "FechaResolucion", "MotivoRechazo", "StockActual", "Vino" })
                    if (dgvPendientes.Columns[col] != null) dgvPendientes.Columns[col].Visible = false;
                if (dgvPendientes.Columns["FechaEmision"] != null)
                    dgvPendientes.Columns["FechaEmision"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                ActualizarEncabezados();
            }

            ActualizarSeleccion();
        }

        private void dgvPendientes_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarSeleccion();
        }

        private BE.SolicitudAutorizacion SolicitudSeleccionada()
        {
            return dgvPendientes.CurrentRow?.DataBoundItem as BE.SolicitudAutorizacion;
        }

        private void ActualizarSeleccion()
        {
            MostrarDetalle();
            ActualizarEstadoBotones();
        }

        private void ActualizarEstadoBotones()
        {
            BE.SolicitudAutorizacion seleccionada = SolicitudSeleccionada();
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            // Capa UI del triple guard de RN-01: quien emitió la solicitud no puede resolverla
            // (ni autorizarla ni rechazarla), aunque BLL y SP también lo rechacen.
            bool puedeResolver = seleccionada != null && seleccionada.EmisorId != sesion.Id;
            btnAutorizar.Enabled = puedeResolver;
            btnRechazarAlta.Enabled = puedeResolver;
        }

        // Valor de tipo o estado de solicitud traducido (claves valsol_*).
        private static string TextoValor(string clave, string porDefecto)
        {
            return SeguridadYServicios.IdiomaManager.getInstance().Traducir(clave) ?? porDefecto;
        }

        // Panel de detalle (R8): el documento que lee el autorizador. Se reconstruye en cada cambio
        // de selección y al cambiar de idioma.
        private void MostrarDetalle()
        {
            BE.SolicitudAutorizacion s = SolicitudSeleccionada();
            if (s == null || s.Vino == null)
            {
                txtDetalle.Clear();
                return;
            }

            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            BE.Vino v = s.Vino;
            string tipo = TextoValor("valsol_" + s.Tipo, s.Tipo.ToString());

            var sb = new System.Text.StringBuilder();
            sb.AppendLine((mgr.Traducir("colhdr_TipoSolicitud") ?? "Tipo") + ": " + tipo);
            sb.AppendLine((mgr.Traducir("colhdr_Emisor") ?? "Emitida por") + ": " + s.EmisorLogin);
            sb.AppendLine((mgr.Traducir("colhdr_FechaEmision") ?? "Fecha de emisión") + ": " + s.FechaEmision.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine();
            sb.AppendLine((mgr.Traducir("colhdr_Codigo") ?? "SKU") + ": " + v.Codigo);
            sb.AppendLine((mgr.Traducir("colhdr_NombreVino") ?? "Nombre") + ": " + v.Nombre);
            sb.AppendLine((mgr.Traducir("colhdr_Bodega") ?? "Bodega") + ": " + v.BodegaNombre);
            sb.AppendLine((mgr.Traducir("colhdr_Varietal") ?? "Varietal") + ": " + v.Varietal);
            sb.AppendLine((mgr.Traducir("colhdr_Aniada") ?? "Añada") + ": " + v.Aniada);
            sb.AppendLine((mgr.Traducir("colhdr_Precio") ?? "Precio") + ": " + v.Precio.ToString("N2"));
            sb.AppendLine((mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo") + ": " + v.StockMinimo);
            sb.AppendLine((mgr.Traducir("colhdr_Maridaje") ?? "Maridaje") + ": " + (string.IsNullOrEmpty(v.Maridaje) ? "-" : v.Maridaje));
            sb.AppendLine((mgr.Traducir("colhdr_Puntaje") ?? "Puntaje") + ": " + (v.Puntaje.HasValue ? v.Puntaje.Value.ToString() : "-"));
            txtDetalle.Text = sb.ToString();
        }

        // El tipo es un enum (Alta/Baja): en la grilla se muestra traducido.
        private void dgvPendientes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            if (dgvPendientes.Columns[e.ColumnIndex].Name == "Tipo")
            {
                e.Value = TextoValor("valsol_" + e.Value, e.Value.ToString());
                e.FormattingApplied = true;
            }
        }

        private void btnAutorizar_Click(object sender, EventArgs e)
        {
            BE.SolicitudAutorizacion seleccionada = SolicitudSeleccionada();
            if (seleccionada == null)
            {
                MsgBox.Show("Seleccione una solicitud de alta pendiente.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.AutorizarAlta(seleccionada.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Vino autorizado correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            txtMotivoRechazo.Clear();
            CargarPendientes();
        }

        private void btnRechazarAlta_Click(object sender, EventArgs e)
        {
            BE.SolicitudAutorizacion seleccionada = SolicitudSeleccionada();
            if (seleccionada == null)
            {
                MsgBox.Show("Seleccione una solicitud de alta pendiente.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            // R6: el motivo es obligatorio. La BLL y la SP lo vuelven a validar.
            string motivo = txtMotivoRechazo.Text.Trim();
            if (motivo.Length == 0)
            {
                MsgBox.Show("Debe indicar el motivo del rechazo.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                txtMotivoRechazo.Focus();
                return;
            }

            if (MsgBox.Show("¿Rechazar el alta del vino '" + seleccionada.VinoNombre + "'? Quedará rechazado y Compras podrá corregirlo y reproponerlo.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.RechazarAlta(seleccionada.Id, motivo, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Alta rechazada. El vino quedó rechazado.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            txtMotivoRechazo.Clear();
            CargarPendientes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
