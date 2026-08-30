using System;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmRestaurarIntegridad : MaterialForm
    {
        private readonly BLL.ResultadoIntegridad _resultado;
        private readonly BLL.UsuarioBLL _usuarioBll = new BLL.UsuarioBLL();
        private readonly BLL.UsuarioHistorialBLL _historialBll = new BLL.UsuarioHistorialBLL();

        public frmRestaurarIntegridad(BLL.ResultadoIntegridad resultado)
        {
            _resultado = resultado;
            InitializeComponent();
        }

        private void frmRestaurarIntegridad_Load(object sender, EventArgs e)
        {
            txtErrores.Text = string.Join(Environment.NewLine, _resultado.Errores);
            Program.GuardarLogIntegridad(_resultado.Errores);
            btnRestaurar.Enabled = _resultado.IdsUsuariosAfectados.Count > 0;
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void btnRecalcular_Click(object sender, EventArgs e)
        {
            new BLL.IntegridadBLL().RecalcularIntegridadUsuarios();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            bool alMenosUnoConHistorial = false;

            foreach (int id in _resultado.IdsUsuariosAfectados)
            {
                BE.USUARIO usuario = _usuarioBll.ObtenerPorId(id);
                if (usuario == null) continue;

                var historial = _historialBll.ObtenerHistorial(id);
                if (historial.Count == 0)
                {
                    MsgBox.Show(
                        $"El usuario '{usuario.Usuario}' no tiene historial de cambios registrado.\n\n" +
                        "No es posible restaurarlo desde historial. Use 'Recalcular y continuar'.",
                        "Sin historial",
                        MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                    continue;
                }

                alMenosUnoConHistorial = true;
                using (var frm = new frmHistorialUsuario(usuario))
                    frm.ShowDialog(this);
            }

            if (!alMenosUnoConHistorial) return;

            new BLL.IntegridadBLL().RecalcularIntegridadUsuarios();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
