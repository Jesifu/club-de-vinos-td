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
            dgvPendientesBaja.DataBindingComplete += (s, ev) => ActualizarSeleccion();
            this.Shown += (s, ev) => ActualizarSeleccion();
            CargarPendientes();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
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
            // El panel de detalle se arma con texto traducido: se vuelve a generar.
            MostrarDetalle();
            dgvPendientesBaja.Invalidate();
        }

        private void ActualizarEncabezados()
        {
            if (dgvPendientesBaja.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvPendientesBaja.Columns["Tipo"] != null)
                dgvPendientesBaja.Columns["Tipo"].HeaderText = mgr.Traducir("colhdr_TipoSolicitud") ?? "Tipo";
            if (dgvPendientesBaja.Columns["VinoCodigo"] != null)
                dgvPendientesBaja.Columns["VinoCodigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
            if (dgvPendientesBaja.Columns["VinoNombre"] != null)
                dgvPendientesBaja.Columns["VinoNombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
            if (dgvPendientesBaja.Columns["BodegaNombre"] != null)
                dgvPendientesBaja.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
            if (dgvPendientesBaja.Columns["EmisorLogin"] != null)
                dgvPendientesBaja.Columns["EmisorLogin"].HeaderText = mgr.Traducir("colhdr_Emisor") ?? "Emitida por";
            if (dgvPendientesBaja.Columns["FechaEmision"] != null)
                dgvPendientesBaja.Columns["FechaEmision"].HeaderText = mgr.Traducir("colhdr_FechaEmision") ?? "Fecha de emisión";
            if (dgvPendientesBaja.Columns["StockActual"] != null)
                dgvPendientesBaja.Columns["StockActual"].HeaderText = mgr.Traducir("colhdr_StockActual") ?? "Stock actual";
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
            dgvPendientesBaja.DataSource = _bll.ListarSolicitudesPendientes(BE.TipoSolicitud.Baja);

            if (dgvPendientesBaja.Columns.Count > 0)
            {
                // Los datos completos del vino se muestran en el panel de detalle, no en la grilla.
                foreach (string col in new[] { "Id", "VinoId", "Estado", "EmisorId", "ResolutorId", "ResolutorLogin",
                                               "FechaResolucion", "MotivoRechazo", "Vino" })
                    if (dgvPendientesBaja.Columns[col] != null) dgvPendientesBaja.Columns[col].Visible = false;
                if (dgvPendientesBaja.Columns["FechaEmision"] != null)
                    dgvPendientesBaja.Columns["FechaEmision"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                ActualizarEncabezados();
            }

            ActualizarSeleccion();
        }

        private void dgvPendientesBaja_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarSeleccion();
        }

        private BE.SolicitudAutorizacion SolicitudSeleccionada()
        {
            return dgvPendientesBaja.CurrentRow?.DataBoundItem as BE.SolicitudAutorizacion;
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

            // Capa UI del triple guard de RN-01: quien emitió la solicitud de baja no puede resolverla
            // (ni autorizarla ni rechazarla), aunque BLL y SP también lo rechacen.
            bool puedeResolver = seleccionada != null && seleccionada.EmisorId != sesion.Id;
            btnAutorizarBaja.Enabled = puedeResolver;
            btnRechazarBaja.Enabled = puedeResolver;
        }

        // Valor de tipo o estado de solicitud traducido (claves valsol_*).
        private static string TextoValor(string clave, string porDefecto)
        {
            return SeguridadYServicios.IdiomaManager.getInstance().Traducir(clave) ?? porDefecto;
        }

        // Panel de detalle (R8): el documento que lee el autorizador, con el stock actual del vino
        // (RN-15). Se reconstruye en cada cambio de selección y al cambiar de idioma.
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
            sb.AppendLine((mgr.Traducir("colhdr_StockActual") ?? "Stock actual") + ": " + s.StockActual);
            sb.AppendLine((mgr.Traducir("colhdr_Maridaje") ?? "Maridaje") + ": " + (string.IsNullOrEmpty(v.Maridaje) ? "-" : v.Maridaje));
            sb.AppendLine((mgr.Traducir("colhdr_Puntaje") ?? "Puntaje") + ": " + (v.Puntaje.HasValue ? v.Puntaje.Value.ToString() : "-"));
            txtDetalle.Text = sb.ToString();
        }

        // El tipo es un enum (Alta/Baja): en la grilla se muestra traducido.
        private void dgvPendientesBaja_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            if (dgvPendientesBaja.Columns[e.ColumnIndex].Name == "Tipo")
            {
                e.Value = TextoValor("valsol_" + e.Value, e.Value.ToString());
                e.FormattingApplied = true;
            }
        }

        private void btnAutorizarBaja_Click(object sender, EventArgs e)
        {
            BE.SolicitudAutorizacion seleccionada = SolicitudSeleccionada();
            if (seleccionada == null)
            {
                MsgBox.Show("Seleccioná un vino con baja solicitada.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show("¿Autorizar la descontinuación del vino '" + seleccionada.VinoNombre + "'? El vino dejará de estar disponible.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Atencion) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.AutorizarBaja(seleccionada.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Vino descontinuado correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            txtMotivoRechazo.Clear();
            CargarPendientes();
        }

        private void btnRechazarBaja_Click(object sender, EventArgs e)
        {
            BE.SolicitudAutorizacion seleccionada = SolicitudSeleccionada();
            if (seleccionada == null)
            {
                MsgBox.Show("Seleccioná un vino con baja solicitada.", "Atención",
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

            if (MsgBox.Show("¿Rechazar la descontinuación del vino '" + seleccionada.VinoNombre + "'? El vino seguirá activo.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            try
            {
                _bll.RechazarBaja(seleccionada.Id, motivo, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Solicitud de descontinuación rechazada. El vino sigue activo.", "Éxito",
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
