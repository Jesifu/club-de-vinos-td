using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmSocios : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.SocioBLL _bll = new BLL.SocioBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        private int _socioIdActual = 0;

        public frmSocios()
        {
            InitializeComponent();
            this.Resize += (s, e) => ReposicionarLayout();
            this.Shown += (s, e) => ReposicionarLayout();
        }

        private void frmSocios_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);

            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_Socios"] = lblTitulo;
            _defaults["lblTitulo_Socios"] = lblTitulo.Text;

            // Colisión con la clave global 'lblNombre' (frmNuevoUsuario = "Usuario:").
            _controles.Remove("lblNombre");
            _defaults.Remove("lblNombre");
            _controles["lblNombre_Socios"] = lblNombre;
            _defaults["lblNombre_Socios"] = lblNombre.Text;

            // El NumericUpDown refleja el valor actual en .Text: si quedara en
            // _controles, un cambio de idioma pisaría lo que el usuario cargó.
            _controles.Remove("numPresupuesto");
            _defaults.Remove("numPresupuesto");

            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarVarietalesDisponibles();
            CargarSocios();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ReposicionarLayout();
        }

        private void ReposicionarLayout()
        {
            if (panelInferior == null || dgvSocios == null) return;

            int altoGrilla = this.ClientSize.Height - panelInferior.Height - dgvSocios.Top - 10;
            if (altoGrilla < 80) altoGrilla = 80;
            dgvSocios.Height = altoGrilla;
            dgvSocios.Width = this.ClientSize.Width - dgvSocios.Left - 15;
        }

        private void frmSocios_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvSocios.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvSocios.Columns["Nombre"] != null)
                dgvSocios.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombrePersona") ?? "Nombre";
            if (dgvSocios.Columns["Apellido"] != null)
                dgvSocios.Columns["Apellido"].HeaderText = mgr.Traducir("colhdr_Apellido") ?? "Apellido";
            if (dgvSocios.Columns["Email"] != null)
                dgvSocios.Columns["Email"].HeaderText = mgr.Traducir("colhdr_Email") ?? "Email";
            if (dgvSocios.Columns["Telefono"] != null)
                dgvSocios.Columns["Telefono"].HeaderText = mgr.Traducir("colhdr_Telefono") ?? "Teléfono";
            if (dgvSocios.Columns["Domicilio"] != null)
                dgvSocios.Columns["Domicilio"].HeaderText = mgr.Traducir("colhdr_Domicilio") ?? "Domicilio";
            if (dgvSocios.Columns["PresupuestoMensual"] != null)
                dgvSocios.Columns["PresupuestoMensual"].HeaderText = mgr.Traducir("colhdr_PresupuestoMensual") ?? "Presupuesto mensual";
            if (dgvSocios.Columns["FechaAlta"] != null)
                dgvSocios.Columns["FechaAlta"].HeaderText = mgr.Traducir("colhdr_FechaAlta") ?? "Fecha de alta";
            if (dgvSocios.Columns["CreadoPorLogin"] != null)
                dgvSocios.Columns["CreadoPorLogin"].HeaderText = mgr.Traducir("colhdr_CreadoPor") ?? "Creado por";
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

        private void CargarVarietalesDisponibles()
        {
            chkVarietales.Items.Clear();
            foreach (string varietal in _bll.ListarVarietalesDeCatalogo())
                chkVarietales.Items.Add(varietal);
        }

        private void CargarSocios()
        {
            dgvSocios.DataSource = _bll.ListarActivos();

            if (dgvSocios.Columns.Count > 0)
            {
                if (dgvSocios.Columns["Id"] != null) dgvSocios.Columns["Id"].Visible = false;
                if (dgvSocios.Columns["Activo"] != null) dgvSocios.Columns["Activo"].Visible = false;
                if (dgvSocios.Columns["CreadoPor"] != null) dgvSocios.Columns["CreadoPor"].Visible = false;
                if (dgvSocios.Columns["VarietalesPreferidos"] != null) dgvSocios.Columns["VarietalesPreferidos"].Visible = false;
                ActualizarEncabezados();
            }
        }

        private void dgvSocios_SelectionChanged(object sender, EventArgs e)
        {
            if (!(dgvSocios.CurrentRow?.DataBoundItem is BE.Socio seleccionado)) return;
            CargarParaEdicion(seleccionado.Id);
        }

        private void CargarParaEdicion(int id)
        {
            BE.Socio s = _bll.ObtenerPorId(id);
            if (s == null) return;

            _socioIdActual = s.Id;
            txtNombre.Text = s.Nombre;
            txtApellido.Text = s.Apellido;
            txtEmail.Text = s.Email;
            txtTelefono.Text = s.Telefono;
            txtDomicilio.Text = s.Domicilio;
            numPresupuesto.Value = s.PresupuestoMensual;
            chkActivo.Checked = s.Activo;

            for (int i = 0; i < chkVarietales.Items.Count; i++)
                chkVarietales.SetItemChecked(i, s.VarietalesPreferidos.Contains(chkVarietales.Items[i].ToString()));
        }

        private void btnGuardarSocio_Click(object sender, EventArgs e)
        {
            BE.USUARIO sesion = SeguridadYServicios.SessionManager.getInstance().getUsuario();

            BE.Socio s = new BE.Socio
            {
                Id = _socioIdActual,
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                Domicilio = string.IsNullOrWhiteSpace(txtDomicilio.Text) ? null : txtDomicilio.Text.Trim(),
                PresupuestoMensual = numPresupuesto.Value,
                Activo = chkActivo.Checked
            };

            foreach (object item in chkVarietales.CheckedItems)
                s.VarietalesPreferidos.Add(item.ToString());

            try
            {
                if (_socioIdActual == 0)
                    _bll.Registrar(s, sesion);
                else
                    _bll.Actualizar(s, sesion);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            MsgBox.Show("Socio guardado correctamente.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            LimpiarFormulario();
            CargarSocios();
        }

        private void btnNuevoSocio_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _socioIdActual = 0;
            txtNombre.Clear();
            txtApellido.Clear();
            txtEmail.Clear();
            txtTelefono.Clear();
            txtDomicilio.Clear();
            numPresupuesto.Value = 0;
            chkActivo.Checked = true;
            for (int i = 0; i < chkVarietales.Items.Count; i++)
                chkVarietales.SetItemChecked(i, false);
            dgvSocios.ClearSelection();
            txtApellido.Focus();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
