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
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblAyudaBaja = new System.Windows.Forms.Label();
            this.btnAutorizarBaja = new System.Windows.Forms.Button();
            this.btnRechazarBaja = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientesBaja)).BeginInit();
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
            this.dgvPendientesBaja.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPendientesBaja.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendientesBaja.Location = new System.Drawing.Point(5, 127);
            this.dgvPendientesBaja.MultiSelect = false;
            this.dgvPendientesBaja.Name = "dgvPendientesBaja";
            this.dgvPendientesBaja.ReadOnly = true;
            this.dgvPendientesBaja.RowHeadersWidth = 55;
            this.dgvPendientesBaja.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendientesBaja.Size = new System.Drawing.Size(700, 330);
            this.dgvPendientesBaja.TabIndex = 1;
            this.dgvPendientesBaja.SelectionChanged += new System.EventHandler(this.dgvPendientesBaja_SelectionChanged);
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblAyudaBaja);
            this.panelInferior.Controls.Add(this.btnAutorizarBaja);
            this.panelInferior.Controls.Add(this.btnRechazarBaja);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 460);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 80);
            this.panelInferior.TabIndex = 2;
            //
            // lblAyudaBaja
            //
            this.lblAyudaBaja.AutoSize = true;
            this.lblAyudaBaja.Location = new System.Drawing.Point(13, 15);
            this.lblAyudaBaja.Name = "lblAyudaBaja";
            this.lblAyudaBaja.Size = new System.Drawing.Size(520, 20);
            this.lblAyudaBaja.TabIndex = 0;
            this.lblAyudaBaja.Text = "No puede autorizar una descontinuación que usted mismo solicitó.";
            //
            // btnAutorizarBaja
            //
            this.btnAutorizarBaja.Enabled = false;
            this.btnAutorizarBaja.Location = new System.Drawing.Point(13, 40);
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
            this.btnRechazarBaja.Location = new System.Drawing.Point(245, 40);
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
            this.btnCerrar.Location = new System.Drawing.Point(597, 40);
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
            this.ClientSize = new System.Drawing.Size(710, 540);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvPendientesBaja);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 420);
            this.Name = "frmAutorizarBajaVino";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autorizar descontinuación de vinos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAutorizarBajaVino_FormClosed);
            this.Load += new System.EventHandler(this.frmAutorizarBajaVino_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientesBaja)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvPendientesBaja;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblAyudaBaja;
        private System.Windows.Forms.Button btnAutorizarBaja;
        private System.Windows.Forms.Button btnRechazarBaja;
        private System.Windows.Forms.Button btnCerrar;
    }
}
