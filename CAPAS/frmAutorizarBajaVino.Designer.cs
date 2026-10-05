namespace CAPAS
{
    partial class frmAutorizarBajaVino
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
            this.dgvPendientesBaja = new System.Windows.Forms.DataGridView();
            this.pnlDetalle = new System.Windows.Forms.Panel();
            this.lblDetalleSolicitud = new System.Windows.Forms.Label();
            this.txtDetalle = new System.Windows.Forms.TextBox();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblAyudaBaja = new System.Windows.Forms.Label();
            this.lblMotivoRechazo = new System.Windows.Forms.Label();
            this.txtMotivoRechazo = new System.Windows.Forms.TextBox();
            this.btnAutorizarBaja = new System.Windows.Forms.Button();
            this.btnRechazarBaja = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientesBaja)).BeginInit();
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
            this.lblTitulo.Size = new System.Drawing.Size(320, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Autorizar descontinuación de vinos";
            //
            // dgvPendientesBaja
            //
            this.dgvPendientesBaja.AllowUserToAddRows = false;
            this.dgvPendientesBaja.AllowUserToDeleteRows = false;
            this.dgvPendientesBaja.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvPendientesBaja.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendientesBaja.Location = new System.Drawing.Point(5, 127);
            this.dgvPendientesBaja.MultiSelect = false;
            this.dgvPendientesBaja.Name = "dgvPendientesBaja";
            this.dgvPendientesBaja.ReadOnly = true;
            this.dgvPendientesBaja.RowHeadersWidth = 55;
            this.dgvPendientesBaja.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendientesBaja.Size = new System.Drawing.Size(640, 330);
            this.dgvPendientesBaja.TabIndex = 1;
            this.dgvPendientesBaja.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvPendientesBaja_CellFormatting);
            this.dgvPendientesBaja.SelectionChanged += new System.EventHandler(this.dgvPendientesBaja_SelectionChanged);
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
            this.panelInferior.Controls.Add(this.lblAyudaBaja);
            this.panelInferior.Controls.Add(this.lblMotivoRechazo);
            this.panelInferior.Controls.Add(this.txtMotivoRechazo);
            this.panelInferior.Controls.Add(this.btnAutorizarBaja);
            this.panelInferior.Controls.Add(this.btnRechazarBaja);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 460);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(1060, 130);
            this.panelInferior.TabIndex = 2;
            //
            // lblAyudaBaja
            //
            this.lblAyudaBaja.AutoSize = true;
            this.lblAyudaBaja.Location = new System.Drawing.Point(13, 10);
            this.lblAyudaBaja.Name = "lblAyudaBaja";
            this.lblAyudaBaja.Size = new System.Drawing.Size(520, 20);
            this.lblAyudaBaja.TabIndex = 0;
            this.lblAyudaBaja.Text = "No puede autorizar ni rechazar una solicitud que usted mismo emitió.";
            //
            // lblMotivoRechazo
            //
            this.lblMotivoRechazo.AutoSize = true;
            this.lblMotivoRechazo.Location = new System.Drawing.Point(13, 43);
            this.lblMotivoRechazo.Name = "lblMotivoRechazo";
            this.lblMotivoRechazo.Size = new System.Drawing.Size(160, 20);
            this.lblMotivoRechazo.TabIndex = 4;
            this.lblMotivoRechazo.Text = "Motivo de rechazo:";
            //
            // txtMotivoRechazo
            //
            this.txtMotivoRechazo.Location = new System.Drawing.Point(180, 40);
            this.txtMotivoRechazo.MaxLength = 200;
            this.txtMotivoRechazo.Name = "txtMotivoRechazo";
            this.txtMotivoRechazo.Size = new System.Drawing.Size(600, 26);
            this.txtMotivoRechazo.TabIndex = 5;
            //
            // btnAutorizarBaja
            //
            this.btnAutorizarBaja.Enabled = false;
            this.btnAutorizarBaja.Location = new System.Drawing.Point(13, 80);
            this.btnAutorizarBaja.Name = "btnAutorizarBaja";
            this.btnAutorizarBaja.Size = new System.Drawing.Size(220, 38);
            this.btnAutorizarBaja.TabIndex = 1;
            this.btnAutorizarBaja.Text = "Autorizar baja";
            this.btnAutorizarBaja.UseVisualStyleBackColor = true;
            this.btnAutorizarBaja.Click += new System.EventHandler(this.btnAutorizarBaja_Click);
            //
            // btnRechazarBaja
            //
            this.btnRechazarBaja.Enabled = false;
            this.btnRechazarBaja.Location = new System.Drawing.Point(245, 80);
            this.btnRechazarBaja.Name = "btnRechazarBaja";
            this.btnRechazarBaja.Size = new System.Drawing.Size(220, 38);
            this.btnRechazarBaja.TabIndex = 2;
            this.btnRechazarBaja.Text = "Rechazar baja";
            this.btnRechazarBaja.UseVisualStyleBackColor = true;
            this.btnRechazarBaja.Click += new System.EventHandler(this.btnRechazarBaja_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(947, 80);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAutorizarBajaVino
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1060, 590);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.pnlDetalle);
            this.Controls.Add(this.dgvPendientesBaja);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(1000, 560);
            this.Name = "frmAutorizarBajaVino";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autorizar descontinuación de vinos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAutorizarBajaVino_FormClosed);
            this.Load += new System.EventHandler(this.frmAutorizarBajaVino_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientesBaja)).EndInit();
            this.pnlDetalle.ResumeLayout(false);
            this.pnlDetalle.PerformLayout();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvPendientesBaja;
        private System.Windows.Forms.Panel pnlDetalle;
        private System.Windows.Forms.Label lblDetalleSolicitud;
        private System.Windows.Forms.TextBox txtDetalle;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblAyudaBaja;
        private System.Windows.Forms.Label lblMotivoRechazo;
        private System.Windows.Forms.TextBox txtMotivoRechazo;
        private System.Windows.Forms.Button btnAutorizarBaja;
        private System.Windows.Forms.Button btnRechazarBaja;
        private System.Windows.Forms.Button btnCerrar;
    }
}
