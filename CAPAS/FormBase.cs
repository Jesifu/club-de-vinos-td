using System;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;

namespace CAPAS
{
    // Base común de todos los formularios. MaterialForm.OnPaint limpia todo el formulario con el
    // fondo del tema Material (gris) y no llama a base.OnPaint, por lo que ni BackColor ni el evento
    // Paint sirven para cambiarlo. Se deja pintar a Material (barra de título, botones) y luego se
    // repinta el área de contenido (UserArea) con el color de formulario del sistema (SystemColors.Control),
    // de modo que los controles se vean con la apariencia estándar de Windows.
    //
    // Ayuda en línea (D02): F1 abre frmAyuda en el tema del formulario actual. Se resuelve acá, en la
    // base, con ProcessCmdKey (no depende de KeyPreview ni del control con foco), así que ningún
    // formulario necesita código propio. El tema sale de AyudaContexto (tabla tipo de formulario ->
    // id de tema); un formulario puede sobrescribir AyudaTema para elegir uno dinámico.
    public class FormBase : MaterialForm
    {
        // Ícono embebido en el .exe (ApplicationIcon); se extrae una sola vez y se comparte entre formularios.
        // Si la extracción falla se deja el ícono por defecto: un error acá no debe impedir abrir los formularios.
        private static readonly Lazy<Icon> _iconoApp = new Lazy<Icon>(() =>
        {
            try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { return null; }
        });

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_iconoApp.Value != null)
                Icon = _iconoApp.Value;
            // MaterialSkinManager (AddFormToManage, que cada formulario llama en su Load) pinta de gris las
            // cajas de entrada; se restablecen los colores por defecto del sistema (blanco de ventana).
            RestablecerColoresDeEntrada(Controls);
        }

        private static void RestablecerColoresDeEntrada(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                if (c is TextBox || c is ComboBox || c is NumericUpDown || c is ListBox
                    || c is TreeView || c is RichTextBox)
                {
                    c.ResetForeColor();
                    // Los de solo lectura toman el gris de formulario por defecto: se los deja en blanco de ventana.
                    var tb = c as TextBoxBase;
                    if (tb != null && tb.ReadOnly) c.BackColor = SystemColors.Window;
                    else c.ResetBackColor();
                }
                if (c.HasChildren)
                    RestablecerColoresDeEntrada(c.Controls);
            }
        }

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
            using (var b = new SolidBrush(SystemColors.Control))
                e.Graphics.FillRectangle(b, UserArea);
        }
    }
}
