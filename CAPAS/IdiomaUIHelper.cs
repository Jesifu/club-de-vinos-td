using BE;
using System.Windows.Forms;

namespace CAPAS
{
    internal static class IdiomaUIHelper
    {
        internal static void AgregarSelector(Form form)
        {
            var strip = new StatusStrip { Dock = DockStyle.Bottom };
            strip.Items.Add(new ToolStripLabel("Idioma: "));

            var cbo = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 160
            };
            strip.Items.Add(cbo);
            form.Controls.Add(strip);
            form.Height += strip.Height;

            IDIOMA activo = SeguridadYServicios.IdiomaManager.getInstance().IdiomaActivo;
            foreach (IDIOMA idioma in new BLL.IdiomaBLL().ListarHabilitados())
            {
                cbo.Items.Add(idioma);
                if (activo != null && idioma.Id == activo.Id)
                    cbo.SelectedItem = idioma;
            }

            cbo.SelectedIndexChanged += (s, e) =>
            {
                if (!(cbo.SelectedItem is IDIOMA idioma)) return;
                var bll = new BLL.IdiomaBLL();
                var traducciones = bll.CargarTraducciones(idioma.Id);
                SeguridadYServicios.IdiomaManager.getInstance().CambiarIdioma(idioma, traducciones);

                BE.USUARIO usuario = SeguridadYServicios.SessionManager.getInstance().getUsuario();
                if (usuario != null)
                    new BLL.UsuarioBLL().ActualizarIdioma(usuario.Id, idioma.Id);
            };
        }
    }
}
