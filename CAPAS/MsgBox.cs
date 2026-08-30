using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    /// <summary>
    /// Reemplazo estilizado de MessageBox.Show() que usa MaterialForm
    /// y los colores de AppTheme para mantener consistencia visual.
    /// Uso: MsgBox.Show("mensaje", "título", MsgBox.Botones.OK, MsgBox.Icono.Info);
    /// </summary>
    internal static class MsgBox
    {
        internal enum Botones { OK, SiNo }
        internal enum Icono { Info, Exito, Atencion, Error, Pregunta }

        internal static DialogResult Show(string mensaje, string titulo,
            Botones botones = Botones.OK, Icono icono = Icono.Info)
        {
            using (var frm = new FrmMensaje(mensaje, titulo, botones, icono))
            {
                return frm.ShowDialog();
            }
        }
    }

    internal class FrmMensaje : MaterialForm
    {
        private readonly MsgBox.Botones _botones;
        private readonly MsgBox.Icono _icono;
        private readonly string _mensaje;

        private Panel pnlIcono;
        private Label lblMensaje;
        private Button btnAceptar;
        private Button btnCancelar;

        internal FrmMensaje(string mensaje, string titulo,
            MsgBox.Botones botones, MsgBox.Icono icono)
        {
            _mensaje = mensaje;
            _botones = botones;
            _icono = icono;

            this.Text = titulo;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = AppTheme.FondoForm;
            this.ForeColor = AppTheme.TextoPrincipal;

            CrearControles();
            AjustarAlto();

            MaterialSkinManager.Instance.AddFormToManage(this);
        }

        private void CrearControles()
        {
            int topInicio = 75;   // debajo del título de MaterialForm
            int margen = 20;

            // ── Panel del ícono ──
            pnlIcono = new Panel
            {
                Location = new Point(margen, topInicio),
                Size = new Size(56, 56),
                BackColor = Color.Transparent
            };
            pnlIcono.Paint += PnlIcono_Paint;
            this.Controls.Add(pnlIcono);

            // ── Label del mensaje ──
            lblMensaje = new Label
            {
                Location = new Point(margen + 70, topInicio + 4),
                MaximumSize = new Size(320, 0),
                AutoSize = true,
                Font = AppTheme.FontNormal,
                ForeColor = AppTheme.TextoPrincipal,
                Text = _mensaje
            };
            this.Controls.Add(lblMensaje);

            // ── Botones ──
            int anchoForm = Math.Max(lblMensaje.Right + margen + 20, 420);
            this.ClientSize = new Size(anchoForm, 220);

            btnAceptar = CrearBoton(_botones == MsgBox.Botones.SiNo ? "Sí" : "Aceptar");
            btnAceptar.DialogResult = DialogResult.OK;
            if (_botones == MsgBox.Botones.SiNo)
                btnAceptar.DialogResult = DialogResult.Yes;
            btnAceptar.Click += (s, e) => this.Close();

            if (_botones == MsgBox.Botones.SiNo)
            {
                btnCancelar = CrearBoton("No");
                btnCancelar.DialogResult = DialogResult.No;
                btnCancelar.Click += (s, e) => this.Close();

                btnCancelar.Location = new Point(anchoForm - btnCancelar.Width - margen - 10,
                    this.ClientSize.Height - 55);
                btnAceptar.Location = new Point(btnCancelar.Left - btnAceptar.Width - 12,
                    btnCancelar.Top);

                this.Controls.Add(btnCancelar);
            }
            else
            {
                btnAceptar.Location = new Point(anchoForm - btnAceptar.Width - margen - 10,
                    this.ClientSize.Height - 55);
            }

            this.Controls.Add(btnAceptar);
            this.AcceptButton = btnAceptar;
            if (btnCancelar != null) this.CancelButton = btnCancelar;
        }

        private void AjustarAlto()
        {
            int altoMensaje = Math.Max(lblMensaje.Height, 56);
            int altoTotal = 75 + altoMensaje + 30 + 50 + 20;
            if (altoTotal < 200) altoTotal = 200;
            this.ClientSize = new Size(this.ClientSize.Width, altoTotal);

            int yBotones = altoTotal - 55;
            if (_botones == MsgBox.Botones.SiNo && btnCancelar != null)
            {
                btnCancelar.Top = yBotones;
                btnAceptar.Top = yBotones;
            }
            else
            {
                btnAceptar.Top = yBotones;
            }
        }

        private Button CrearBoton(string texto)
        {
            var btn = new Button
            {
                Text = texto,
                Size = new Size(100, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.FondoControl,
                ForeColor = AppTheme.Acento,
                Cursor = Cursors.Hand,
                Font = AppTheme.FontNormal,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderColor = AppTheme.Acento;
            btn.FlatAppearance.BorderSize = 1;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = AppTheme.FondoHeader;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = AppTheme.FondoHeader;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = AppTheme.FondoControl;
                btn.ForeColor = AppTheme.Acento;
                btn.FlatAppearance.BorderColor = AppTheme.Acento;
            };

            return btn;
        }

        private void PnlIcono_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color colorFondo;
            string simbolo;

            switch (_icono)
            {
                case MsgBox.Icono.Exito:
                    colorFondo = Color.FromArgb(46, 125, 50);   // verde
                    simbolo = "✓";
                    break;
                case MsgBox.Icono.Atencion:
                    colorFondo = Color.FromArgb(245, 166, 35);  // naranja
                    simbolo = "!";
                    break;
                case MsgBox.Icono.Error:
                    colorFondo = Color.FromArgb(198, 40, 40);   // rojo
                    simbolo = "✕";
                    break;
                case MsgBox.Icono.Pregunta:
                    colorFondo = AppTheme.Acento;               // azul
                    simbolo = "?";
                    break;
                default: // Info
                    colorFondo = AppTheme.Acento;               // azul
                    simbolo = "i";
                    break;
            }

            using (var brush = new SolidBrush(colorFondo))
                g.FillEllipse(brush, 2, 2, 50, 50);

            using (var font = new Font("Segoe UI", 22f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(simbolo, font, Brushes.White, new RectangleF(2, 2, 50, 50), sf);
        }
    }
}
