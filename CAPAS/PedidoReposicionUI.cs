using System;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // Utilidades compartidas por los formularios del Pedido de Reposición (CU-38, CU-39 y CU-40):
    // textos traducidos de los estados, detalle de la cabecera, grilla de líneas y fechas.
    // Los valores guardados siguen siendo los del enum de BE; solo cambia lo que se ve.
    internal static class PedidoReposicionUI
    {
        internal static string T(string clave, string porDefecto)
        {
            return SeguridadYServicios.IdiomaManager.getInstance().Traducir(clave) ?? porDefecto;
        }

        // Estado del pedido traducido (claves valped_*).
        internal static string Estado(string estado)
        {
            return T("valped_" + estado, estado);
        }

        internal static string Estado(BE.EstadoPedidoReposicion estado)
        {
            return Estado(estado.ToString());
        }

        internal static string Fecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        }

        // Texto de la cabecera del pedido: lo que lee quien autoriza, recibe o consulta. Las líneas
        // de resolución y recepción aparecen solo cuando el pedido ya pasó por ese paso.
        internal static string ArmarCabecera(BE.PedidoReposicion p)
        {
            if (p == null) return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(T("colhdr_PedNro", "Nº de pedido") + ": " + p.Id);
            sb.AppendLine(T("colhdr_Estado", "Estado") + ": " + Estado(p.Estado));
            sb.AppendLine(T("colhdr_Bodega", "Bodega") + ": " + p.BodegaNombre);
            sb.AppendLine(T("colhdr_PedGeneradoPor", "Generado por") + ": " + p.CreadoPorLogin
                          + "  (" + Fecha(p.FechaCreacion) + ")");
            if (p.ResolutorLogin != null)
                sb.AppendLine(T("colhdr_PedResueltoPor", "Resuelto por") + ": " + p.ResolutorLogin
                              + "  (" + Fecha(p.FechaResolucion) + ")");
            if (!string.IsNullOrEmpty(p.MotivoRechazo))
                sb.AppendLine(T("colhdr_MotivoRechazo", "Motivo de rechazo") + ": " + p.MotivoRechazo);
            if (p.RecibidoPorLogin != null)
                sb.AppendLine(T("colhdr_PedRecibidoPor", "Recibido por") + ": " + p.RecibidoPorLogin
                              + "  (" + Fecha(p.FechaRecepcion) + ")");
            if (!string.IsNullOrEmpty(p.Observacion))
                sb.AppendLine(T("colhdr_PedObservacion", "Observación") + ": " + p.Observacion);
            return sb.ToString();
        }

        // Oculta lo que no se muestra y deja en las grillas de pedidos las columnas de resumen.
        internal static void ConfigurarGrillaPedidos(DataGridView dgv, bool conEstado)
        {
            if (dgv.Columns.Count == 0) return;
            foreach (string col in new[] { "BodegaId", "CreadoPor", "ResolutorId", "ResolutorLogin", "FechaResolucion",
                                           "MotivoRechazo", "RecibidoPor", "RecibidoPorLogin", "FechaRecepcion",
                                           "Observacion", "Lineas", "CantidadLineas" })
                if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;
            if (!conEstado && dgv.Columns["Estado"] != null) dgv.Columns["Estado"].Visible = false;

            string[] orden = { "Id", "Estado", "BodegaNombre", "CreadoPorLogin", "FechaCreacion", "CantidadLineas", "UnidadesPedidas" };
            int i = 0;
            foreach (string col in orden)
                if (dgv.Columns[col] != null && dgv.Columns[col].Visible) dgv.Columns[col].DisplayIndex = i++;

            if (dgv.Columns["FechaCreacion"] != null)
                dgv.Columns["FechaCreacion"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ActualizarEncabezadosPedidos(dgv);
        }

        internal static void ActualizarEncabezadosPedidos(DataGridView dgv)
        {
            Encabezado(dgv, "Id", "colhdr_PedNro", "Nº pedido");
            Encabezado(dgv, "Estado", "colhdr_Estado", "Estado");
            Encabezado(dgv, "BodegaNombre", "colhdr_Bodega", "Bodega");
            Encabezado(dgv, "CreadoPorLogin", "colhdr_PedGeneradoPor", "Generado por");
            Encabezado(dgv, "FechaCreacion", "colhdr_PedFechaGeneracion", "Fecha de generación");
            Encabezado(dgv, "CantidadLineas", "colhdr_PedLineas", "Líneas");
            Encabezado(dgv, "UnidadesPedidas", "colhdr_PedUnidades", "Unidades pedidas");
        }

        // El estado de la grilla de pedidos se muestra traducido (el valor enlazado es el enum).
        internal static void FormatearEstado(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            if (dgv.Columns[e.ColumnIndex].Name == "Estado")
            {
                e.Value = Estado(e.Value.ToString());
                e.FormattingApplied = true;
            }
        }

        // Grilla de líneas del pedido. conRecibida agrega la cantidad recibida (CU-39 y CU-40).
        internal static void ConfigurarGrillaLineas(DataGridView dgv, bool conRecibida)
        {
            if (dgv.Columns.Count == 0) return;
            foreach (string col in new[] { "Id", "PedidoId", "VinoId" })
                if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;
            // Se fija en ambos sentidos: al reenlazar, la grilla conserva la visibilidad anterior.
            if (dgv.Columns["CantidadRecibida"] != null)
                dgv.Columns["CantidadRecibida"].Visible = conRecibida;

            string[] orden = { "VinoCodigo", "VinoNombre", "CantidadSugerida", "CantidadPedida", "CantidadRecibida" };
            int i = 0;
            foreach (string col in orden)
                if (dgv.Columns[col] != null && dgv.Columns[col].Visible) dgv.Columns[col].DisplayIndex = i++;

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            if (dgv.Columns["VinoNombre"] != null)
            {
                dgv.Columns["VinoNombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgv.Columns["VinoNombre"].MinimumWidth = 160;
            }
            ActualizarEncabezadosLineas(dgv);
        }

        internal static void ActualizarEncabezadosLineas(DataGridView dgv)
        {
            Encabezado(dgv, "VinoCodigo", "colhdr_Codigo", "SKU");
            Encabezado(dgv, "VinoNombre", "colhdr_NombreVino", "Nombre");
            Encabezado(dgv, "CantidadSugerida", "colhdr_PedCantidadSugerida", "Sugerida al generar");
            Encabezado(dgv, "CantidadPedida", "colhdr_PedCantidadPedida", "Cantidad pedida");
            Encabezado(dgv, "CantidadRecibida", "colhdr_PedCantidadRecibida", "Cantidad recibida");
        }

        private static void Encabezado(DataGridView dgv, string columna, string clave, string porDefecto)
        {
            if (dgv.Columns[columna] != null)
                dgv.Columns[columna].HeaderText = T(clave, porDefecto);
        }
    }

    // Diálogo para pedir el motivo del rechazo de un pedido (RN-34: obligatorio, 1 a 200
    // caracteres). Se arma en código, igual que FrmMensaje, con el tema de la aplicación.
    internal class FrmMotivoRechazoPedido : FormBase
    {
        private TextBox txtMotivo;
        private Label lblEtiqueta;
        private Button btnAceptar;
        private Button btnCancelar;

        // Motivo ingresado (ya recortado); solo es válido si el diálogo se cerró con OK.
        internal string Motivo { get; private set; }

        protected override bool AyudaHabilitada { get { return false; } }

        internal FrmMotivoRechazoPedido(int pedidoId)
        {
            Text = PedidoReposicionUI.T("ped_MotivoTitulo", "Rechazar pedido") + " Nº " + pedidoId;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 230);

            lblEtiqueta = new Label
            {
                Location = new Point(20, 80),
                Size = new Size(480, 44),
                Font = SystemFonts.MessageBoxFont,
                Text = PedidoReposicionUI.T("ped_MotivoEtiqueta",
                    "Motivo del rechazo (obligatorio, hasta 200 caracteres):")
            };
            txtMotivo = new TextBox
            {
                Location = new Point(20, 128),
                Size = new Size(480, 26),
                MaxLength = 200,
                Font = SystemFonts.MessageBoxFont
            };
            btnAceptar = CrearBoton(PedidoReposicionUI.T("ped_MotivoConfirmar", "Rechazar pedido"), 20, 175, 170);
            btnCancelar = CrearBoton(PedidoReposicionUI.T("ped_MotivoCancelar", "Cancelar"), 400, 175, 100);
            btnAceptar.Click += btnAceptar_Click;
            btnCancelar.DialogResult = DialogResult.Cancel;

            Controls.Add(lblEtiqueta);
            Controls.Add(txtMotivo);
            Controls.Add(btnAceptar);
            Controls.Add(btnCancelar);
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;

            MaterialSkinManager.Instance.AddFormToManage(this);
        }

        private static Button CrearBoton(string texto, int x, int y, int ancho)
        {
            var b = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(ancho, 38),
                UseVisualStyleBackColor = true
            };
            return b;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            string motivo = txtMotivo.Text.Trim();
            if (motivo.Length == 0)
            {
                MsgBox.Show("Debe indicar el motivo del rechazo.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                txtMotivo.Focus();
                return;
            }
            Motivo = motivo;
            DialogResult = DialogResult.OK;
        }
    }
}
