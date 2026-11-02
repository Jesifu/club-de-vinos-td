using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // CU-38 — Autorizar o Rechazar Pedido de Reposición. Lista los pedidos Pendientes, muestra su
    // detalle (cabecera + líneas) y permite autorizarlos o rechazarlos con motivo. RN-33: quien
    // generó el pedido no puede resolverlo (UI, BLL, SP y CHECK).
    public partial class frmAutorizarPedidoReposicion : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.PedidoReposicionBLL _bll = new BLL.PedidoReposicionBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmAutorizarPedidoReposicion()
        {
            InitializeComponent();
        }

        private void frmAutorizarPedidoReposicion_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_AutorizarPedido"] = lblTitulo;
            _defaults["lblTitulo_AutorizarPedido"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            // Si hay una sola fila, SelectionChanged no se vuelve a disparar: se reevalúa al enlazar
            // los datos y al mostrarse el formulario (mismo patrón que frmAutorizarAltaVino).
            dgvPedidos.DataBindingComplete += (s, ev) => ActualizarSeleccion();
            this.Shown += (s, ev) => ActualizarSeleccion();
            CargarPendientes();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmAutorizarPedidoReposicion_FormClosed(object sender, FormClosedEventArgs e)
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
            PedidoReposicionUI.ActualizarEncabezadosPedidos(dgvPedidos);
            PedidoReposicionUI.ActualizarEncabezadosLineas(dgvLineas);
            MostrarDetalle();
            dgvPedidos.Invalidate();
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
            dgvPedidos.DataSource = _bll.Listar(BE.EstadoPedidoReposicion.Pendiente);
            PedidoReposicionUI.ConfigurarGrillaPedidos(dgvPedidos, false);
            ActualizarSeleccion();
        }

        private BE.PedidoReposicion PedidoSeleccionado()
        {
            return dgvPedidos.CurrentRow?.DataBoundItem as BE.PedidoReposicion;
        }

        private void dgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarSeleccion();
        }

        private void dgvPedidos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            PedidoReposicionUI.FormatearEstado(dgvPedidos, e);
        }

        private void ActualizarSeleccion()
        {
            MostrarDetalle();
            ActualizarEstadoBotones();
        }

        // Panel de detalle: cabecera y líneas del pedido elegido.
        private void MostrarDetalle()
        {
            BE.PedidoReposicion p = PedidoSeleccionado();
            if (p == null)
            {
                txtDetalle.Clear();
                dgvLineas.DataSource = null;
                return;
            }
            txtDetalle.Text = PedidoReposicionUI.ArmarCabecera(p);
            dgvLineas.DataSource = _bll.ObtenerLineas(p.Id);
            PedidoReposicionUI.ConfigurarGrillaLineas(dgvLineas, false);
        }

        private void ActualizarEstadoBotones()
        {
            BE.PedidoReposicion p = PedidoSeleccionado();
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            // RN-33 (capa UI): quien generó el pedido no puede autorizarlo ni rechazarlo.
            bool esPropio = p != null && p.CreadoPor == sesion.Id;
            bool puedeResolver = p != null && !esPropio;
            btnAutorizarPedido.Enabled = puedeResolver;
            btnRechazarPedido.Enabled = puedeResolver;
            lblAyudaPedido.ForeColor = esPropio ? AppTheme.Acento : AppTheme.TextoPrincipal;
        }

        private void btnAutorizarPedido_Click(object sender, EventArgs e)
        {
            BE.PedidoReposicion p = PedidoSeleccionado();
            if (p == null)
            {
                MsgBox.Show("Seleccione un pedido pendiente.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();
            try
            {
                _bll.Autorizar(p.Id, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Pedido Nº " + p.Id + " autorizado correctamente. Queda Aprobado, a la espera de su recepción.",
                "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarPendientes();
        }

        private void btnRechazarPedido_Click(object sender, EventArgs e)
        {
            BE.PedidoReposicion p = PedidoSeleccionado();
            if (p == null)
            {
                MsgBox.Show("Seleccione un pedido pendiente.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            // RN-34: el motivo es obligatorio. La BLL, el SP y un CHECK lo vuelven a validar.
            string motivo;
            using (var dlg = new FrmMotivoRechazoPedido(p.Id))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                motivo = dlg.Motivo;
            }

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();
            try
            {
                _bll.Rechazar(p.Id, motivo, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarPendientes();
                return;
            }

            MsgBox.Show("Pedido Nº " + p.Id + " rechazado. Sus vinos quedan libres para un nuevo pedido.",
                "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarPendientes();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
