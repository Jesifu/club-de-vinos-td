using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmArmarCaja : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.CajaMensualBLL _bll = new BLL.CajaMensualBLL();
        private readonly BLL.SocioBLL _socioBll = new BLL.SocioBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        // Working list de líneas antes de confirmar el armado — no es la composición
        // efectiva (RN-07, eso solo existe después de INSERT vía CAJA_VINO_LISTAR_EFECTIVO).
        private readonly List<BE.CajaVino> _lineas = new List<BE.CajaVino>();
        private decimal _presupuestoSocio = 0;

        public frmArmarCaja()
        {
            InitializeComponent();
        }

        private void frmArmarCaja_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_ArmarCaja"] = lblTitulo;
            _defaults["lblTitulo_ArmarCaja"] = lblTitulo.Text;

            // Labels de valor calculado en tiempo real: si quedaran en _controles, un
            // cambio de idioma pisaría el importe mostrado (mismo criterio que
            // numPresupuesto en frmSocios / lblStockActualValor en frmRegistrarEntradaStock).
            _controles.Remove("lblPresupuestoValor");
            _defaults.Remove("lblPresupuestoValor");
            _controles.Remove("lblTotalValor");
            _defaults.Remove("lblTotalValor");

            // NumericUpDown: refleja lo cargado por el usuario, mismo criterio.
            _controles.Remove("numCantidad");
            _defaults.Remove("numCantidad");

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarSocios();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ActualizarTotales();
        }

        private void frmArmarCaja_FormClosed(object sender, FormClosedEventArgs e)
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

            if (dgvCandidatos.Columns.Count > 0)
            {
                if (dgvCandidatos.Columns["Codigo"] != null)
                    dgvCandidatos.Columns["Codigo"].HeaderText = mgr.Traducir("colhdr_Codigo") ?? "SKU";
                if (dgvCandidatos.Columns["Nombre"] != null)
                    dgvCandidatos.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
                if (dgvCandidatos.Columns["BodegaNombre"] != null)
                    dgvCandidatos.Columns["BodegaNombre"].HeaderText = mgr.Traducir("colhdr_Bodega") ?? "Bodega";
                if (dgvCandidatos.Columns["Varietal"] != null)
                    dgvCandidatos.Columns["Varietal"].HeaderText = mgr.Traducir("colhdr_Varietal") ?? "Varietal";
                if (dgvCandidatos.Columns["Aniada"] != null)
                    dgvCandidatos.Columns["Aniada"].HeaderText = mgr.Traducir("colhdr_Aniada") ?? "Añada";
                if (dgvCandidatos.Columns["Precio"] != null)
                    dgvCandidatos.Columns["Precio"].HeaderText = mgr.Traducir("colhdr_Precio") ?? "Precio";
                if (dgvCandidatos.Columns["Stock"] != null)
                    dgvCandidatos.Columns["Stock"].HeaderText = mgr.Traducir("colhdr_Stock") ?? "Stock";
                if (dgvCandidatos.Columns["Preferido"] != null)
                    dgvCandidatos.Columns["Preferido"].HeaderText = mgr.Traducir("colhdr_Preferido") ?? "Preferido";
            }

            if (dgvLineas.Columns.Count > 0)
            {
                if (dgvLineas.Columns["NombreSnapshot"] != null)
                    dgvLineas.Columns["NombreSnapshot"].HeaderText = mgr.Traducir("colhdr_NombreVino") ?? "Nombre";
                if (dgvLineas.Columns["Cantidad"] != null)
                    dgvLineas.Columns["Cantidad"].HeaderText = mgr.Traducir("colhdr_Cantidad") ?? "Cantidad";
                if (dgvLineas.Columns["PrecioSnapshot"] != null)
                    dgvLineas.Columns["PrecioSnapshot"].HeaderText = mgr.Traducir("colhdr_Precio") ?? "Precio";
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

        private void CargarSocios()
        {
            cboSocio.DataSource = _socioBll.ListarActivos();
        }

        private void cboSocio_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lineas.Clear();

            if (cboSocio.SelectedItem is BE.Socio socio)
            {
                _presupuestoSocio = socio.PresupuestoMensual;
                CargarCandidatos(socio.Id);
            }
            else
            {
                _presupuestoSocio = 0;
                dgvCandidatos.DataSource = null;
            }

            RefreshLineas();
            ActualizarTotales();
        }

        // RN-06 filtra en la SP (solo Activo + autorizado + stock > 0), RN-05.2 ordena en
        // la SP (preferidos primero) — la UI preserva el orden devuelto, no reordena.
        private void CargarCandidatos(int socioId)
        {
            dgvCandidatos.DataSource = _bll.ListarCandidatos(socioId);
            if (dgvCandidatos.Columns.Count > 0)
            {
                if (dgvCandidatos.Columns["Id"] != null) dgvCandidatos.Columns["Id"].Visible = false;
                ActualizarEncabezados();
            }
        }

        // RN-05.2: resalta la columna Preferido, nunca filtra el resto de la grilla.
        private void dgvCandidatos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCandidatos.Columns[e.ColumnIndex].Name != "Preferido") return;
            if (e.Value is bool preferido && preferido)
            {
                e.CellStyle.Font = new Font(dgvCandidatos.Font, FontStyle.Bold);
                e.CellStyle.BackColor = AppTheme.Acento;
            }
        }

        private void btnAgregarLinea_Click(object sender, EventArgs e)
        {
            if (!(cboSocio.SelectedItem is BE.Socio))
            {
                MsgBox.Show("Seleccioná un socio.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            if (!(dgvCandidatos.CurrentRow?.DataBoundItem is BE.VinoCandidato candidato))
            {
                MsgBox.Show("Seleccioná un vino de la lista.", "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            int cantidad = (int)numCantidad.Value;

            // UQ_CAJA_VINO (CAJA_ID, VINO_ID): un mismo vino no puede repetirse como línea
            // separada — si ya está agregado, se suma la cantidad en la línea existente.
            BE.CajaVino linea = _lineas.FirstOrDefault(l => l.VinoId == candidato.Id);
            if (linea != null)
            {
                linea.Cantidad += cantidad;
            }
            else
            {
                _lineas.Add(new BE.CajaVino
                {
                    VinoId = candidato.Id,
                    NombreSnapshot = candidato.Nombre, // RN-04, snapshot provisorio — la BLL revalida al confirmar
                    PrecioSnapshot = candidato.Precio,
                    Cantidad = cantidad
                });
            }

            RefreshLineas();
            ActualizarTotales();
        }

        // Quitar una línea agregada por error, sin cerrar el diálogo (doble clic sobre la fila).
        private void dgvLineas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(dgvLineas.Rows[e.RowIndex].DataBoundItem is BE.CajaVino linea)) return;

            _lineas.Remove(linea);
            RefreshLineas();
            ActualizarTotales();
        }

        private void RefreshLineas()
        {
            dgvLineas.DataSource = null;
            dgvLineas.DataSource = _lineas.ToList();

            if (dgvLineas.Columns.Count > 0)
            {
                if (dgvLineas.Columns["Id"] != null) dgvLineas.Columns["Id"].Visible = false;
                if (dgvLineas.Columns["CajaId"] != null) dgvLineas.Columns["CajaId"].Visible = false;
                if (dgvLineas.Columns["VinoId"] != null) dgvLineas.Columns["VinoId"].Visible = false;
                if (dgvLineas.Columns["VinoEfectivoId"] != null) dgvLineas.Columns["VinoEfectivoId"].Visible = false;
                if (dgvLineas.Columns["NombreEfectivo"] != null) dgvLineas.Columns["NombreEfectivo"].Visible = false;
                if (dgvLineas.Columns["PrecioEfectivo"] != null) dgvLineas.Columns["PrecioEfectivo"].Visible = false;
                if (dgvLineas.Columns["Sustituido"] != null) dgvLineas.Columns["Sustituido"].Visible = false;
                if (dgvLineas.Columns["StockDisponible"] != null) dgvLineas.Columns["StockDisponible"].Visible = false;
                ActualizarEncabezados();
            }
        }

        private void txtPeriodo_TextChanged(object sender, EventArgs e)
        {
            ActualizarTotales();
        }

        // RN-05.1 en la capa UI: el botón queda deshabilitado mientras el total supere
        // el presupuesto mensual del socio. La BLL revalida lo mismo antes del DAL.
        private void ActualizarTotales()
        {
            decimal total = _bll.CalcularTotal(_lineas);
            lblPresupuestoValor.Text = _presupuestoSocio.ToString("C2");
            lblTotalValor.Text = total.ToString("C2");
            lblTotalValor.ForeColor = total > _presupuestoSocio ? Color.IndianRed : AppTheme.TextoPrincipal;

            btnConfirmarArmado.Enabled = cboSocio.SelectedItem is BE.Socio
                && _lineas.Count > 0
                && total <= _presupuestoSocio
                && EsPeriodoValido(txtPeriodo.Text);
        }

        private static bool EsPeriodoValido(string periodo)
        {
            return Regex.IsMatch(periodo ?? string.Empty, @"^\d{4}-(0[1-9]|1[0-2])$");
        }

        private void btnConfirmarArmado_Click(object sender, EventArgs e)
        {
            if (!(cboSocio.SelectedItem is BE.Socio socio)) return;

            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            BE.CajaMensual caja = new BE.CajaMensual
            {
                SocioId = socio.Id,
                Periodo = txtPeriodo.Text.Trim(),
                Lineas = _lineas.ToList()
            };

            try
            {
                _bll.Armar(caja, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Caja armada correctamente.", "Éxito", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _lineas.Clear();
            txtPeriodo.Clear();
            numCantidad.Value = 1;
            RefreshLineas();
            ActualizarTotales();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
