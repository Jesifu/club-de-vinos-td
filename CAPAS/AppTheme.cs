using System;
using System.Drawing;
using System.Windows.Forms;

namespace CAPAS
{
    internal static class AppTheme
    {
        // Paleta: Rosé Pine Main — oscuro con pasteles rose/iris/foam
        internal static readonly Color FondoForm      = Color.FromArgb( 25,  23,  36); // base    #191724
        internal static readonly Color FondoHeader    = Color.FromArgb( 38,  35,  58); // overlay #26233a
        internal static readonly Color FondoControl   = Color.FromArgb( 31,  29,  46); // surface #1f1d2e
        internal static readonly Color FondoGrilla    = Color.FromArgb( 31,  29,  46); // surface #1f1d2e
        internal static readonly Color FondoGrillaAlt = Color.FromArgb( 38,  35,  58); // overlay #26233a
        internal static readonly Color Acento         = Color.FromArgb(196, 167, 231); // iris    #c4a7e7  ← lavanda pastel
        internal static readonly Color AcentoHover    = Color.FromArgb(235, 188, 186); // rose    #ebbcba  ← rosa pastel
        internal static readonly Color TextoPrincipal  = Color.FromArgb(224, 222, 244); // text    #e0def4  ← lavanda claro
        internal static readonly Color TextoEncabezado = Color.FromArgb(196, 167, 231); // iris    #c4a7e7  ← lavanda pastel
        internal static readonly Color TextoSecundario = Color.FromArgb(144, 140, 170); // subtle  #908caa  ← gris lavanda
        internal static readonly Color Borde          = Color.FromArgb(110, 106, 134); // muted   #6e6a86
        internal static readonly Color Seleccion      = Color.FromArgb( 49, 116, 143); // pine    #31748f  ← teal muted
        internal static readonly Color SeleccionTexto = Color.FromArgb(224, 222, 244); // text    #e0def4

        internal static readonly Font FontTitulo = new Font("Segoe UI", 11f, FontStyle.Bold);
        internal static readonly Font FontBold   = new Font("Segoe UI", 10f, FontStyle.Bold);
        internal static readonly Font FontNormal = new Font("Segoe UI", 10f, FontStyle.Regular);
        internal static readonly Font FontMono   = new Font("Consolas",  9f, FontStyle.Regular);

        internal static void AplicarTema(Form form)
        {
            form.BackColor = FondoForm;
            form.ForeColor = TextoPrincipal;
            EstilizarControles(form.Controls, FondoForm);
        }

        private static void EstilizarControles(Control.ControlCollection controles, Color fondoPadre)
        {
            foreach (Control c in controles)
            {
                if (c is MenuStrip ms)    { EstilizarMenuStrip(ms);   continue; }
                if (c is StatusStrip ss)  { EstilizarStatusStrip(ss); continue; }
                if (c is DataGridView dg) { EstilizarDGV(dg);         continue; }
                if (c is TreeView tv)     { EstilizarTreeView(tv);    continue; }

                if (c is CheckedListBox clb)
                {
                    clb.BackColor   = FondoControl;
                    clb.ForeColor   = TextoPrincipal;
                    clb.Font        = FontNormal;
                    clb.BorderStyle = BorderStyle.FixedSingle;
                    continue;
                }
                if (c is ListBox lb)
                {
                    lb.BackColor   = FondoControl;
                    lb.ForeColor   = TextoPrincipal;
                    lb.Font        = FontNormal;
                    lb.BorderStyle = BorderStyle.FixedSingle;
                    continue;
                }
                if (c is ComboBox cbo)
                {
                    cbo.BackColor = FondoControl;
                    cbo.ForeColor = TextoPrincipal;
                    cbo.Font      = FontNormal;
                    cbo.FlatStyle = FlatStyle.Flat;
                    continue;
                }
                if (c is TextBox txt)
                {
                    bool mono = txt.Font != null && txt.Font.FontFamily.Name == "Consolas";
                    txt.BackColor   = FondoControl;
                    txt.ForeColor   = TextoPrincipal;
                    txt.Font        = mono ? FontMono : FontNormal;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    continue;
                }
                if (c is Button btn)
                {
                    EstilizarBoton(btn);
                    continue;
                }
                if (c is Label lbl)
                {
                    // Preservar etiquetas blancas del login (texto sobre imagen oscura)
                    if (lbl.ForeColor == Color.White)
                    {
                        lbl.Font = lbl.Font != null && lbl.Font.Bold ? FontBold : FontNormal;
                        continue;
                    }
                    lbl.ForeColor = TextoPrincipal;
                    lbl.BackColor = fondoPadre; // mismo color que el contenedor — sin Transparent
                    bool bold   = lbl.Font != null && lbl.Font.Bold;
                    bool grande = lbl.Font != null && lbl.Font.Size >= 11f;
                    lbl.Font = (grande && bold) ? FontTitulo : (bold ? FontBold : FontNormal);
                    continue;
                }
                if (c is DateTimePicker dtp)
                {
                    // BackColor no es compatible en DTP nativo; solo configurar el calendario
                    dtp.CalendarMonthBackground = FondoControl;
                    dtp.CalendarForeColor       = TextoPrincipal;
                    dtp.CalendarTitleBackColor  = FondoHeader;
                    dtp.CalendarTitleForeColor  = TextoEncabezado;
                    dtp.Font = FontNormal;
                    continue;
                }
                if (c is Panel pnl)
                {
                    pnl.BackColor = FondoForm;
                    pnl.ForeColor = TextoPrincipal;
                    EstilizarControles(pnl.Controls, FondoForm);
                    continue;
                }
                if (c is GroupBox gb)
                {
                    gb.BackColor = FondoForm;
                    gb.ForeColor = Acento;
                    gb.Font      = FontBold;
                    EstilizarControles(gb.Controls, FondoForm);
                    continue;
                }

                // Default
                try { c.BackColor = FondoForm; c.ForeColor = TextoPrincipal; } catch { }
                if (c.HasChildren) EstilizarControles(c.Controls, FondoForm);
            }
        }

        private static void EstilizarBoton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Acento;
            btn.FlatAppearance.BorderSize  = 1;
            btn.BackColor = FondoControl;
            btn.ForeColor = Acento;
            btn.Cursor    = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            // No se cambia btn.Font — preserva el tamaño original para que el texto entre

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = FondoHeader;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = FondoHeader;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = FondoControl;
                btn.ForeColor = Acento;
                btn.FlatAppearance.BorderColor = Acento;
            };
        }

        private static void EstilizarDGV(DataGridView dgv)
        {
            dgv.BackgroundColor       = FondoGrilla;
            dgv.GridColor             = Borde;
            dgv.BorderStyle           = BorderStyle.FixedSingle;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor          = FondoHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor          = TextoEncabezado;
            dgv.ColumnHeadersDefaultCellStyle.Font               = FontBold;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = FondoHeader;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextoEncabezado;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgv.DefaultCellStyle.BackColor          = FondoGrilla;
            dgv.DefaultCellStyle.ForeColor          = TextoPrincipal;
            dgv.DefaultCellStyle.SelectionBackColor = Seleccion;
            dgv.DefaultCellStyle.SelectionForeColor = SeleccionTexto;
            dgv.DefaultCellStyle.Font               = FontNormal;

            dgv.AlternatingRowsDefaultCellStyle.BackColor          = FondoGrillaAlt;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor          = TextoPrincipal;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Seleccion;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = SeleccionTexto;

            dgv.RowHeadersDefaultCellStyle.BackColor          = FondoGrillaAlt;
            dgv.RowHeadersDefaultCellStyle.ForeColor          = TextoSecundario;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Seleccion;
        }

        private static void EstilizarTreeView(TreeView tv)
        {
            tv.BackColor   = FondoControl;
            tv.ForeColor   = TextoPrincipal;
            tv.Font        = FontNormal;
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.LineColor   = Acento;
        }

        internal static void EstilizarMenuStrip(MenuStrip ms)
        {
            ms.BackColor  = FondoHeader;
            ms.ForeColor  = TextoEncabezado;
            ms.Font       = FontNormal;
            ms.RenderMode = ToolStripRenderMode.Professional;
            ms.Renderer   = new RenderizadorCorporativo();

            foreach (ToolStripItem item in ms.Items)
            {
                item.BackColor = FondoHeader;
                item.ForeColor = TextoEncabezado;
                item.Font      = FontNormal;
                if (item is ToolStripMenuItem tsi)
                    EstilizarMenuItems(tsi);
            }
        }

        private static void EstilizarMenuItems(ToolStripMenuItem item)
        {
            item.BackColor = FondoHeader;
            item.ForeColor = TextoEncabezado;
            item.Font      = FontNormal;
            foreach (ToolStripItem sub in item.DropDownItems)
            {
                sub.BackColor = FondoControl;
                sub.ForeColor = TextoPrincipal;
                sub.Font      = FontNormal;
                if (sub is ToolStripMenuItem subMenu)
                    EstilizarMenuItems(subMenu);
            }
        }

        internal static void EstilizarStatusStrip(StatusStrip ss)
        {
            Color fondoStatus = Color.FromArgb(144, 140, 170); // subtle #908caa  ← gris claro abajo
            ss.BackColor  = fondoStatus;
            ss.ForeColor  = TextoSecundario;
            ss.Font       = FontNormal;
            ss.RenderMode = ToolStripRenderMode.Professional;
            ss.Renderer   = new RenderizadorCorporativo();

            foreach (ToolStripItem item in ss.Items)
            {
                item.BackColor = fondoStatus;
                item.ForeColor = TextoSecundario;
                item.Font      = FontNormal;
                if (item is ToolStripComboBox tscbo)
                {
                    tscbo.BackColor          = FondoControl;
                    tscbo.ForeColor          = TextoPrincipal;
                    tscbo.ComboBox.BackColor = FondoControl;
                    tscbo.ComboBox.ForeColor = TextoPrincipal;
                    tscbo.ComboBox.FlatStyle = FlatStyle.Flat;
                    tscbo.ComboBox.Font      = FontNormal;
                }
            }
        }
    }

    internal class RenderizadorCorporativo : ToolStripProfessionalRenderer
    {
        public RenderizadorCorporativo() : base(new TablaColaresCorpo()) { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color fondo = e.Item.Selected ? AppTheme.AcentoHover : e.Item.BackColor;
            using (var b = new SolidBrush(fondo))
                e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            bool sobreAzul = e.Item.BackColor == AppTheme.FondoHeader
                          || e.Item.BackColor == AppTheme.AcentoHover
                          || e.Item.Selected;
            e.TextColor = sobreAzul ? Color.White : AppTheme.TextoPrincipal;
            base.OnRenderItemText(e);
        }
    }

    internal class TablaColaresCorpo : ProfessionalColorTable
    {
        public override Color MenuItemSelected              => AppTheme.AcentoHover;
        public override Color MenuItemBorder               => AppTheme.Acento;
        public override Color MenuBorder                   => AppTheme.Borde;
        public override Color ToolStripDropDownBackground  => AppTheme.FondoControl;
        public override Color ImageMarginGradientBegin     => AppTheme.FondoGrillaAlt;
        public override Color ImageMarginGradientMiddle    => AppTheme.FondoGrillaAlt;
        public override Color ImageMarginGradientEnd       => AppTheme.FondoGrillaAlt;
        public override Color MenuItemSelectedGradientBegin => AppTheme.AcentoHover;
        public override Color MenuItemSelectedGradientEnd   => AppTheme.AcentoHover;
        public override Color MenuItemPressedGradientBegin  => AppTheme.Acento;
        public override Color MenuItemPressedGradientEnd    => AppTheme.Acento;
        public override Color SeparatorLight               => AppTheme.Borde;
        public override Color SeparatorDark                => AppTheme.Borde;
        public override Color StatusStripGradientBegin     => Color.FromArgb(144, 140, 170);
        public override Color StatusStripGradientEnd       => Color.FromArgb(144, 140, 170);
        public override Color MenuStripGradientBegin       => AppTheme.FondoHeader;
        public override Color MenuStripGradientEnd         => AppTheme.FondoHeader;
    }
}
