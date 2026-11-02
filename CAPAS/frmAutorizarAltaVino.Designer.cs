namespace CAPAS
{
    partial class frmAutorizarAltaVino
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
            this.dgvPendientes = new System.Windows.Forms.DataGridView();
            this.pnlDetalle = new System.Windows.Forms.Panel();
            this.lblDetalleSolicitud = new System.Windows.Forms.Label();
            this.txtDetalle = new System.Windows.Forms.TextBox();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblAyuda = new System.Windows.Forms.Label();
            this.lblMotivoRechazo = new System.Windows.Forms.Label();
            this.txtMotivoRechazo = new System.Windows.Forms.TextBox();
            this.btnAutorizar = new System.Windows.Forms.Button();
            this.btnRechazarAlta = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).BeginInit();
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
            this.lblTitulo.Size = new System.Drawing.Size(260, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Autorizar alta de vinos";
            //
            // dgvPendientes
            //
            this.dgvPendientes.AllowUserToAddRows = false;
            this.dgvPendientes.AllowUserToDeleteRows = false;
            this.dgvPendientes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvPendientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendientes.Location = new System.Drawing.Point(5, 127);
            this.dgvPendientes.MultiSelect = false;
            this.dgvPendientes.Name = "dgvPendientes";
            this.dgvPendientes.ReadOnly = true;
            this.dgvPendientes.RowHeadersWidth = 55;
            this.dgvPendientes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendientes.Size = new System.Drawing.Size(640, 330);
            this.dgvPendientes.TabIndex = 1;
            this.dgvPendientes.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvPendientes_CellFormatting);
            this.dgvPendientes.SelectionChanged += new System.EventHandler(this.dgvPendientes_SelectionChanged);
            //
            // pnlDetalle
            //
            this.pnlDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDetalle.Controls.Add(this.lblDetalleSolicitud);
            this.pnlDetalle.Controls.Add(this.txtDetalle);
            this.pnlDetalle.Location = new System.Drawing.Point(655, 127);
            this.pnlDetalle.Name = "pnlDetalle";
            this.pnlDetalle.Size = new System.Drawing.Size(400, 330);
            this.pnlDetalle.TabIndex = 3;
            //
            // lblDetalleSolicitud
            //
            this.lblDetalleSolicitud.AutoSize = true;
            this.lblDetalleSolicitud.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblDetalleSolicitud.Location = new System.Drawing.Point(10, 8);
            this.lblDetalleSolicitud.Name = "lblDetalleSolicitud";
            this.lblDetalleSolicitud.Size = new System.Drawing.Size(180, 20);
            this.lblDetalleSolicitud.TabIndex = 0;
            this.lblDetalleSolicitud.Text = "Detalle de la solicitud";
            //
            // txtDetalle
            //
            this.txtDetalle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetalle.Location = new System.Drawing.Point(10, 35);
            this.txtDetalle.Multiline = true;
            this.txtDetalle.Name = "txtDetalle";
            this.txtDetalle.ReadOnly = true;
            this.txtDetalle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetalle.Size = new System.Drawing.Size(380, 285);
            this.txtDetalle.TabIndex = 1;
            this.txtDetalle.TabStop = false;
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblAyuda);
            this.panelInferior.Controls.Add(this.lblMotivoRechazo);
            this.panelInferior.Controls.Add(this.txtMotivoRechazo);
            this.panelInferior.Controls.Add(this.btnAutorizar);
            this.panelInferior.Controls.Add(this.btnRechazarAlta);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 460);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(1060, 130);
            this.panelInferior.TabIndex = 2;
            //
            // lblAyuda
            //
            this.lblAyuda.AutoSize = true;
            this.lblAyuda.Location = new System.Drawing.Point(13, 10);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Size = new System.Drawing.Size(430, 20);
            this.lblAyuda.TabIndex = 0;
            this.lblAyuda.Text = "No puede autorizar ni rechazar una solicitud que usted mismo emitió.";
            //
            // lblMotivoRechazo
            //
            this.lblMotivoRechazo.AutoSize = true;
            this.lblMotivoRechazo.Location = new System.Drawing.Point(13, 43);
            this.lblMotivoRechazo.Name = "lblMotivoRechazo";
            this.lblMotivoRechazo.Size = new System.Drawing.Size(160, 20);
            this.lblMotivoRechazo.TabIndex = 1;
            this.lblMotivoRechazo.Text = "Motivo de rechazo:";
            //
            // txtMotivoRechazo
            //
            this.txtMotivoRechazo.Location = new System.Drawing.Point(180, 40);
            this.txtMotivoRechazo.MaxLength = 200;
            this.txtMotivoRechazo.Name = "txtMotivoRechazo";
            this.txtMotivoRechazo.Size = new System.Drawing.Size(600, 26);
            this.txtMotivoRechazo.TabIndex = 2;
            //
            // btnAutorizar
            //
            this.btnAutorizar.Enabled = false;
            this.btnAutorizar.Location = new System.Drawing.Point(13, 80);
            this.btnAutorizar.Name = "btnAutorizar";
            this.btnAutorizar.Size = new System.Drawing.Size(160, 38);
            this.btnAutorizar.TabIndex = 3;
            this.btnAutorizar.Text = "Autorizar";
            this.btnAutorizar.UseVisualStyleBackColor = true;
            this.btnAutorizar.Click += new System.EventHandler(this.btnAutorizar_Click);
            //
            // btnRechazarAlta
            //
            this.btnRechazarAlta.Enabled = false;
            this.btnRechazarAlta.Location = new System.Drawing.Point(185, 80);
            this.btnRechazarAlta.Name = "btnRechazarAlta";
            this.btnRechazarAlta.Size = new System.Drawing.Size(160, 38);
            this.btnRechazarAlta.TabIndex = 4;
            this.btnRechazarAlta.Text = "Rechazar alta";
            this.btnRechazarAlta.UseVisualStyleBackColor = true;
            this.btnRechazarAlta.Click += new System.EventHandler(this.btnRechazarAlta_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(947, 80);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 5;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAutorizarAltaVino
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1060, 590);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.pnlDetalle);
            this.Controls.Add(this.dgvPendientes);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(1000, 560);
            this.Name = "frmAutorizarAltaVino";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autorizar alta de vinos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAutorizarAltaVino_FormClosed);
            this.Load += new System.EventHandler(this.frmAutorizarAltaVino_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).EndInit();
            this.pnlDetalle.ResumeLayout(false);
            this.pnlDetalle.PerformLayout();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvPendientes;
        private System.Windows.Forms.Panel pnlDetalle;
        private System.Windows.Forms.Label lblDetalleSolicitud;
        private System.Windows.Forms.TextBox txtDetalle;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Label lblMotivoRechazo;
        private System.Windows.Forms.TextBox txtMotivoRechazo;
        private System.Windows.Forms.Button btnAutorizar;
        private System.Windows.Forms.Button btnRechazarAlta;
        private System.Windows.Forms.Button btnCerrar;
    }
}
