using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // CU-40 — Consultar Pedidos de Reposición. Solo lectura: lista con filtro por estado y panel de
    // detalle (cabecera, responsables, fechas, motivo de rechazo y líneas). Accede quien tenga
    // cualquiera de los tres permisos del ciclo (gestionar, autorizar o recibir): frmMenu lo controla.
    public partial class frmConsultarPedidosReposicion : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.PedidoReposicionBLL _bll = new BLL.PedidoReposicionBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        // Evita recargar la lista mientras se rearma el combo (al cambiar de idioma).
        private bool _cargandoFiltro;

        // Opción del filtro: Estado null = Todos.
        private class OpcionEstado
        {
            internal BE.EstadoPedidoReposicion? Estado;
            internal string Texto;
            public override string ToString() { return Texto; }
        }

        public frmConsultarPedidosReposicion()
        {
            InitializeComponent();
        }

        private void frmConsultarPedidosReposicion_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_ConsultarPedidos"] = lblTitulo;
            _defaults["lblTitulo_ConsultarPedidos"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            dgvPedidos.DataBindingComplete += (s, ev) => MostrarDetalle();
            this.Shown += (s, ev) => MostrarDetalle();
            CargarPedidos();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
        }

        private void frmConsultarPedidosReposicion_FormClosed(object sender, FormClosedEventArgs e)
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
            ArmarFiltro();
            PedidoReposicionUI.ActualizarEncabezadosPedidos(dgvPedidos);
            PedidoReposicionUI.ActualizarEncabezadosLineas(dgvLineas);
            if (dgvPedidos.CurrentRow?.DataBoundItem is BE.PedidoReposicion p)
                txtDetalle.Text = PedidoReposicionUI.ArmarCabecera(p);
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

        // Rearma las opciones del filtro con los textos del idioma activo, conservando la elegida.
        private void ArmarFiltro()
        {
            BE.EstadoPedidoReposicion? actual = (cboEstado.SelectedItem as OpcionEstado)?.Estado;
            bool habiaSeleccion = cboEstado.SelectedItem != null;

            _cargandoFiltro = true;
            cboEstado.Items.Clear();
            cboEstado.Items.Add(new OpcionEstado { Estado = null, Texto = PedidoReposicionUI.T("valped_Todos", "Todos") });
            foreach (BE.EstadoPedidoReposicion est in Enum.GetValues(typeof(BE.EstadoPedidoReposicion)))
                cboEstado.Items.Add(new OpcionEstado { Estado = est, Texto = PedidoReposicionUI.Estado(est) });

            int indice = 0;
            if (habiaSeleccion)
                for (int i = 0; i < cboEstado.Items.Count; i++)
                    if (((OpcionEstado)cboEstado.Items[i]).Estado == actual) indice = i;
            cboEstado.SelectedIndex = indice;
            _cargandoFiltro = false;
        }

        private void cboEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargandoFiltro) return;
            CargarPedidos();
        }

        private void CargarPedidos()
        {
            BE.EstadoPedidoReposicion? filtro = (cboEstado.SelectedItem as OpcionEstado)?.Estado;
            try
            {
                dgvPedidos.DataSource = _bll.Listar(filtro);
                PedidoReposicionUI.ConfigurarGrillaPedidos(dgvPedidos, true);
            }
            catch (Exception)
            {
                dgvPedidos.DataSource = null;
                MostrarErrorGenerico();
            }
            MostrarDetalle();
        }

        private static void MostrarErrorGenerico()
        {
            MsgBox.Show("No se pudo completar la operación. Intente nuevamente.", "Error",
                MsgBox.Botones.OK, MsgBox.Icono.Error);
        }

        private void dgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            MostrarDetalle();
        }

        private void dgvPedidos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            PedidoReposicionUI.FormatearEstado(dgvPedidos, e);
        }

        private void MostrarDetalle()
        {
            BE.PedidoReposicion p = dgvPedidos.CurrentRow?.DataBoundItem as BE.PedidoReposicion;
            if (p == null)
            {
                txtDetalle.Clear();
                dgvLineas.DataSource = null;
                return;
            }
            try
            {
                txtDetalle.Text = PedidoReposicionUI.ArmarCabecera(p);
                dgvLineas.DataSource = _bll.ObtenerLineas(p.Id);
                PedidoReposicionUI.ConfigurarGrillaLineas(dgvLineas, p.Estado == BE.EstadoPedidoReposicion.Recibido);
            }
            catch (Exception)
            {
                txtDetalle.Clear();
                dgvLineas.DataSource = null;
                MostrarErrorGenerico();
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
