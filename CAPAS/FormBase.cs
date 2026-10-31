using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;

namespace CAPAS
{
    // Base común de todos los formularios. MaterialForm.OnPaint limpia todo el formulario con el
    // fondo del tema Material (gris) y no llama a base.OnPaint, por lo que ni BackColor ni el evento
    // Paint sirven para cambiarlo. Se deja pintar a Material (barra de título, botones) y luego se
    // repinta el área de contenido (UserArea) con el fondo de la paleta.
    //
    // Ayuda en línea (D02): F1 abre frmAyuda en el tema del formulario actual. Se resuelve acá, en la
    // base, con ProcessCmdKey (no depende de KeyPreview ni del control con foco), así que ningún
    // formulario necesita código propio. El tema sale de AyudaContexto (tabla tipo de formulario ->
    // id de tema); un formulario puede sobrescribir AyudaTema para elegir uno dinámico.
    public class FormBase : MaterialForm
    {
        // Id del tema de ayuda para este formulario; la raíz si no hay uno específico.
        protected virtual string AyudaTema
        {
            get { return AyudaContexto.TemaPara(GetType()); }
        }

        // frmAyuda lo deshabilita para no abrir una ayuda encima de la ayuda.
        protected virtual bool AyudaHabilitada
        {
            get { return true; }
        }

        protected void AbrirAyuda()
        {
            using (var ayuda = new frmAyuda(AyudaTema))
                ayuda.ShowDialog(this);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1 && AyudaHabilitada)
            {
                AbrirAyuda();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var b = new SolidBrush(AppTheme.FondoForm))
                e.Graphics.FillRectangle(b, UserArea);
        }
    }
}
