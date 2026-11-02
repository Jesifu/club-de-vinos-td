using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // CU-39 — Registrar Recepción de Pedido de Reposición. Lista los pedidos Aprobados y permite
    // informar la cantidad recibida de cada línea (0 a la cantidad pedida, por defecto la pedida).
    // Al confirmar se generan las Entradas de stock y el pedido queda Recibido (terminal, RN-35).
    public partial class frmRecibirPedidoReposicion : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.PedidoReposicionBLL _bll = new BLL.PedidoReposicionBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        // Líneas del pedido elegido; CantidadRecibida es lo que edita el usuario.
        private List<BE.PedidoReposicionLinea> _lineas;

        public frmRecibirPedidoReposicion()
        {
            InitializeComponent();
        }

        private void frmRecibirPedidoReposicion_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_RecibirPedido"] = lblTitulo;
            _defaults["lblTitulo_RecibirPedido"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            dgvPedidos.DataBindingComplete += (s, ev) => ActualizarSeleccion();
            this.Shown += (s, ev) => ActualizarSeleccion();
            CargarAprobados();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmRecibirPedidoReposicion_FormClosed(object sender, FormClosedEventArgs e)
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

        private void CargarAprobados()
        {
            try
            {
                dgvPedidos.DataSource = _bll.Listar(BE.EstadoPedidoReposicion.Aprobado);
                PedidoReposicionUI.ConfigurarGrillaPedidos(dgvPedidos, false);
            }
            catch (Exception)
            {
                dgvPedidos.DataSource = null;
                MostrarErrorGenerico();
            }
            ActualizarSeleccion();
        }

        private static void MostrarErrorGenerico()
        {
            MsgBox.Show("No se pudo completar la operación. Intente nuevamente.", "Error",
                MsgBox.Botones.OK, MsgBox.Icono.Error);
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
            BE.PedidoReposicion p = PedidoSeleccionado();
            if (p == null)
            {
                txtDetalle.Clear();
                _lineas = null;
                dgvLineas.DataSource = null;
            }
            else
            {
                try
                {
                    txtDetalle.Text = PedidoReposicionUI.ArmarCabecera(p);
                    _lineas = _bll.ObtenerLineas(p.Id);
                    // Por defecto se asume recibido todo lo pedido.
                    foreach (BE.PedidoReposicionLinea l in _lineas)
                        l.CantidadRecibida = l.CantidadPedida;
                    dgvLineas.DataSource = _lineas;
                    PedidoReposicionUI.ConfigurarGrillaLineas(dgvLineas, true);
                    foreach (DataGridViewColumn col in dgvLineas.Columns)
                        col.ReadOnly = col.Name != "CantidadRecibida";
                    if (dgvLineas.Columns["CantidadSugerida"] != null)
                        dgvLineas.Columns["CantidadSugerida"].Visible = false;
                    dgvLineas.EditMode = DataGridViewEditMode.EditOnEnter;
                }
                catch (Exception)
                {
                    txtDetalle.Clear();
                    _lineas = null;
                    dgvLineas.DataSource = null;
                    MostrarErrorGenerico();
                }
            }
            btnRegistrarRecepcion.Enabled = p != null && _lineas != null;
        }

        // Un valor no numérico en la cantidad recibida vuelve al anterior (la celda se revierte).
        private void dgvLineas_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
            MsgBox.Show("La cantidad recibida debe ser un número entero.", "Atención",
                MsgBox.Botones.OK, MsgBox.Icono.Atencion);
        }

        // RN-35: 0 <= recibida <= pedida. Fuera de rango vuelve a la cantidad pedida.
        private void dgvLineas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvLineas.Columns[e.ColumnIndex].Name != "CantidadRecibida") return;
            BE.PedidoReposicionLinea l = dgvLineas.Rows[e.RowIndex].DataBoundItem as BE.PedidoReposicionLinea;
            if (l == null) return;
            // Además del rango pedido, se respeta el tope por línea (BLL.PedidoReposicionBLL.CantidadMaximaPorLinea).
            int tope = Math.Min(l.CantidadPedida, BLL.PedidoReposicionBLL.CantidadMaximaPorLinea);
            if (!l.CantidadRecibida.HasValue || l.CantidadRecibida.Value < 0 || l.CantidadRecibida.Value > tope)
            {
                MsgBox.Show("La cantidad recibida de \"" + l.VinoNombre + "\" debe estar entre 0 y " + tope + ".",
                    "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                l.CantidadRecibida = l.CantidadPedida;
                dgvLineas.Refresh();
            }
        }

        private void btnRegistrarRecepcion_Click(object sender, EventArgs e)
        {
            dgvLineas.EndEdit();
            BE.PedidoReposicion p = PedidoSeleccionado();
            if (p == null || _lineas == null)
            {
                MsgBox.Show("Seleccione un pedido aprobado.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            var cantidades = new Dictionary<int, int>();
            foreach (BE.PedidoReposicionLinea l in _lineas)
                cantidades[l.Id] = l.CantidadRecibida ?? 0;

            if (cantidades.Values.All(c => c == 0))
            {
                MsgBox.Show("Al menos una línea debe tener una cantidad recibida mayor a cero.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            int lineasConEntrada = cantidades.Values.Count(c => c > 0);
            long unidades = cantidades.Values.Sum(c => (long)c);
            if (MsgBox.Show("¿Registrar la recepción del pedido Nº " + p.Id + "? Se generan entradas de stock por " +
                            unidades + " unidad(es) y el pedido queda Recibido; lo no recibido se da por no entregado.",
                    "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();
            try
            {
                _bll.Recibir(p.Id, cantidades, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                CargarAprobados();
                return;
            }
            catch (Exception)
            {
                MostrarErrorGenerico();
                CargarAprobados();
                return;
            }

            MsgBox.Show("Recepción del pedido Nº " + p.Id + " registrada. Entradas de stock generadas: " +
                        lineasConEntrada + " línea(s), " + unidades + " unidad(es).",
                "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarAprobados();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
