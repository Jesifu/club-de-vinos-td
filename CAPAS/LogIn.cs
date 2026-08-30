using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class LogIn : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public LogIn()
        {
            InitializeComponent();
        }

        private void LogIn_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void LogIn_FormClosed(object sender, FormClosedEventArgs e)
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

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                MsgBox.Show("Completá usuario y contraseña.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BLL.LoginBLL bll = new BLL.LoginBLL();
            BE.LoginResultado resultado = bll.AutenticarUsuario(
                txtUsuario.Text.Trim(), txtContrasena.Text.Trim());

            switch (resultado)
            {
                case BE.LoginResultado.Exito:
                    BE.USUARIO usuarioActual = SeguridadYServicios.SessionManager.getInstance().getUsuario();

                    if (Program.ResultadoIntegridad != null && !Program.ResultadoIntegridad.EsValido)
                    {
                        bool esAdmin = SeguridadYServicios.SessionManager.getInstance().TienePermiso("Administrar usuarios");
                        bool suDataEstaIntegra = !Program.ResultadoIntegridad.IdsUsuariosAfectados.Contains(usuarioActual.Id);

                        if (esAdmin && suDataEstaIntegra)
                        {
                            var frm = new frmRestaurarIntegridad(Program.ResultadoIntegridad);
                            if (frm.ShowDialog() != DialogResult.OK)
                            {
                                SeguridadYServicios.SessionManager.getInstance().cerrarSesion();
                                return;
                            }
                        }
                        else
                        {
                            MsgBox.Show(
                                "El sistema no puede iniciarse debido a un problema interno." +
                                Environment.NewLine + Environment.NewLine +
                                "Comuníquese con el administrador del sistema.",
                                "Error del sistema",
                                MsgBox.Botones.OK, MsgBox.Icono.Error);
                            SeguridadYServicios.SessionManager.getInstance().cerrarSesion();
                            return;
                        }
                    }
                    else
                    {
                        new BLL.IntegridadBLL().RecalcularIntegridadUsuarios();
                    }

                    MsgBox.Show("Bienvenido, " + usuarioActual.Usuario + "!",
                        "Login exitoso", MsgBox.Botones.OK, MsgBox.Icono.Exito);
                    new frmMenu().Show();
                    this.Hide();
                    break;

                case BE.LoginResultado.UsuarioBloqueado:
                    if (Program.ResultadoIntegridad == null || Program.ResultadoIntegridad.EsValido)
                        new BLL.IntegridadBLL().RecalcularIntegridadUsuarios();
                    MsgBox.Show(
                        "Usuario bloqueado por intentos fallidos. Contactate con un administrador.",
                        "Acceso denegado", MsgBox.Botones.OK, MsgBox.Icono.Error);
                    break;

                default:
                    if (Program.ResultadoIntegridad == null || Program.ResultadoIntegridad.EsValido)
                        new BLL.IntegridadBLL().RecalcularIntegridadUsuarios();
                    MsgBox.Show("Usuario o contraseña incorrectos.", "Error",
                        MsgBox.Botones.OK, MsgBox.Icono.Error);
                    break;
            }
        }
    }
}
