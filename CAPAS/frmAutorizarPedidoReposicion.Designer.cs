namespace CAPAS
{
    partial class frmAutorizarPedidoReposicion
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.dgvPedidos = new System.Windows.Forms.DataGridView();
            this.pnlDetalle = new System.Windows.Forms.Panel();
            this.lblDetallePedido = new System.Windows.Forms.Label();
            this.txtDetalle = new System.Windows.Forms.TextBox();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblAyudaPedido = new System.Windows.Forms.Label();
            this.btnAutorizarPedido = new System.Windows.Forms.Button();
            this.btnRechazarPedido = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.pnlDetalle.SuspendLayout();
            this.panelInferior.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(5, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(360, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Autorizar pedidos de reposición";
            //
            // dgvPedidos
            //
            this.dgvPedidos.AllowUserToAddRows = false;
            this.dgvPedidos.AllowUserToDeleteRows = false;
            this.dgvPedidos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvPedidos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPedidos.Location = new System.Drawing.Point(5, 127);
            this.dgvPedidos.MultiSelect = false;
            this.dgvPedidos.Name = "dgvPedidos";
            this.dgvPedidos.ReadOnly = true;
            this.dgvPedidos.RowHeadersWidth = 30;
            this.dgvPedidos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPedidos.Size = new System.Drawing.Size(600, 347);
            this.dgvPedidos.TabIndex = 1;
            this.dgvPedidos.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvPedidos_CellFormatting);
            this.dgvPedidos.SelectionChanged += new System.EventHandler(this.dgvPedidos_SelectionChanged);
            //
            // pnlDetalle
            //
            this.pnlDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDetalle.Controls.Add(this.lblDetallePedido);
            this.pnlDetalle.Controls.Add(this.txtDetalle);
            this.pnlDetalle.Controls.Add(this.dgvLineas);
            this.pnlDetalle.Location = new System.Drawing.Point(615, 127);
            this.pnlDetalle.Name = "pnlDetalle";
            this.pnlDetalle.Size = new System.Drawing.Size(555, 347);
            this.pnlDetalle.TabIndex = 3;
            //
            // lblDetallePedido
            //
            this.lblDetallePedido.AutoSize = true;
            this.lblDetallePedido.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblDetallePedido.Location = new System.Drawing.Point(10, 8);
            this.lblDetallePedido.Name = "lblDetallePedido";
            this.lblDetallePedido.Size = new System.Drawing.Size(150, 20);
            this.lblDetallePedido.TabIndex = 0;
            this.lblDetallePedido.Text = "Detalle del pedido";
            //
            // txtDetalle
            //
            this.txtDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetalle.Location = new System.Drawing.Point(10, 35);
            this.txtDetalle.Multiline = true;
            this.txtDetalle.Name = "txtDetalle";
            this.txtDetalle.ReadOnly = true;
            this.txtDetalle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetalle.Size = new System.Drawing.Size(535, 150);
            this.txtDetalle.TabIndex = 1;
            this.txtDetalle.TabStop = false;
            //
            // dgvLineas
            //
            this.dgvLineas.AllowUserToAddRows = false;
            this.dgvLineas.AllowUserToDeleteRows = false;
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLineas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLineas.Location = new System.Drawing.Point(10, 195);
            this.dgvLineas.MultiSelect = false;
            this.dgvLineas.Name = "dgvLineas";
            this.dgvLineas.ReadOnly = true;
            this.dgvLineas.RowHeadersVisible = false;
            this.dgvLineas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLineas.Size = new System.Drawing.Size(535, 142);
            this.dgvLineas.TabIndex = 2;
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblAyudaPedido);
            this.panelInferior.Controls.Add(this.btnAutorizarPedido);
            this.panelInferior.Controls.Add(this.btnRechazarPedido);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 510);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(1180, 100);
            this.panelInferior.TabIndex = 2;
            //
            // lblAyudaPedido
            //
            this.lblAyudaPedido.AutoSize = true;
            this.lblAyudaPedido.Location = new System.Drawing.Point(13, 10);
            this.lblAyudaPedido.Name = "lblAyudaPedido";
            this.lblAyudaPedido.Size = new System.Drawing.Size(430, 20);
            this.lblAyudaPedido.TabIndex = 0;
            this.lblAyudaPedido.Text = "No puede autorizar ni rechazar un pedido que usted mismo generó.";
            //
            // btnAutorizarPedido
            //
            this.btnAutorizarPedido.Enabled = false;
            this.btnAutorizarPedido.Location = new System.Drawing.Point(13, 48);
            this.btnAutorizarPedido.Name = "btnAutorizarPedido";
            this.btnAutorizarPedido.Size = new System.Drawing.Size(160, 38);
            this.btnAutorizarPedido.TabIndex = 1;
            this.btnAutorizarPedido.Text = "Autorizar";
            this.btnAutorizarPedido.UseVisualStyleBackColor = true;
            this.btnAutorizarPedido.Click += new System.EventHandler(this.btnAutorizarPedido_Click);
            //
            // btnRechazarPedido
            //
            this.btnRechazarPedido.Enabled = false;
            this.btnRechazarPedido.Location = new System.Drawing.Point(185, 48);
            this.btnRechazarPedido.Name = "btnRechazarPedido";
            this.btnRechazarPedido.Size = new System.Drawing.Size(160, 38);
            this.btnRechazarPedido.TabIndex = 2;
            this.btnRechazarPedido.Text = "Rechazar";
            this.btnRechazarPedido.UseVisualStyleBackColor = true;
            this.btnRechazarPedido.Click += new System.EventHandler(this.btnRechazarPedido_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(1067, 48);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAutorizarPedidoReposicion
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 610);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.pnlDetalle);
            this.Controls.Add(this.dgvPedidos);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "frmAutorizarPedidoReposicion";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autorizar pedidos de reposición";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAutorizarPedidoReposicion_FormClosed);
            this.Load += new System.EventHandler(this.frmAutorizarPedidoReposicion_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedidos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.pnlDetalle.ResumeLayout(false);
            this.pnlDetalle.PerformLayout();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvPedidos;
        private System.Windows.Forms.Panel pnlDetalle;
        private System.Windows.Forms.Label lblDetallePedido;
        private System.Windows.Forms.TextBox txtDetalle;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblAyudaPedido;
        private System.Windows.Forms.Button btnAutorizarPedido;
        private System.Windows.Forms.Button btnRechazarPedido;
        private System.Windows.Forms.Button btnCerrar;
    }
}
