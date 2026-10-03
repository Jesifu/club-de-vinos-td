using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;

namespace CAPAS
{
    // Base común de todos los formularios. MaterialForm.OnPaint limpia todo el formulario con el
    // fondo del tema Material (gris) y no llama a base.OnPaint, por lo que ni BackColor ni el evento
    // Paint sirven para cambiarlo. Se deja pintar a Material (barra de título, botones) y luego se
    // repinta el área de contenido (UserArea) con el fondo de la paleta.
    public class FormBase : MaterialForm
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var b = new SolidBrush(AppTheme.FondoForm))
                e.Graphics.FillRectangle(b, UserArea);
        }
    }
}
