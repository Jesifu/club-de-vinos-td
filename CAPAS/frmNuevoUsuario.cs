using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmNuevoUsuario : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.PerfilBLL _perfilBll = new BLL.PerfilBLL();

        public string NombreUsuario => txtNombre.Text.Trim();
        public string Contrasena => txtContrasena.Text;
        public int RolId => ((BE.Rol)cboRol.SelectedItem).Id;
        public string Nombre => txtNombrePersona.Text.Trim();
        public string Apellido => txtApellido.Text.Trim();
        public string Telefono => txtTelefono.Text.Trim();
        public string Email => txtEmail.Text.Trim();

        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmNuevoUsuario()
        {
            InitializeComponent();
        }

        private void frmNuevoUsuario_Load(object sender, EventArgs e)
        {
            cboRol.DataSource = _perfilBll.ListarRolesParaCombo();
            cboRol.DisplayMember = "Nombre";
            cboRol.ValueMember = "Id";
            if (cboRol.Items.Count > 0) cboRol.SelectedIndex = 0;

            GuardarDefaults(this.Controls);
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmNuevoUsuario_FormClosed(object sender, FormClosedEventArgs e)
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

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MsgBox.Show("Ingresá un nombre de usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            if (string.IsNullOrWhiteSpace(Contrasena))
            {
                MsgBox.Show("Ingresá una contraseña.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            if (cboRol.SelectedItem == null)
            {
                MsgBox.Show("Seleccioná un rol.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
