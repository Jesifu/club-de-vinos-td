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
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblAyuda = new System.Windows.Forms.Label();
            this.btnAutorizar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).BeginInit();
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
            this.dgvPendientes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPendientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendientes.Location = new System.Drawing.Point(5, 127);
            this.dgvPendientes.MultiSelect = false;
            this.dgvPendientes.Name = "dgvPendientes";
            this.dgvPendientes.ReadOnly = true;
            this.dgvPendientes.RowHeadersWidth = 55;
            this.dgvPendientes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPendientes.Size = new System.Drawing.Size(700, 330);
            this.dgvPendientes.TabIndex = 1;
            this.dgvPendientes.SelectionChanged += new System.EventHandler(this.dgvPendientes_SelectionChanged);
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblAyuda);
            this.panelInferior.Controls.Add(this.btnAutorizar);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 460);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 80);
            this.panelInferior.TabIndex = 2;
            //
            // lblAyuda
            //
            this.lblAyuda.AutoSize = true;
            this.lblAyuda.Location = new System.Drawing.Point(13, 15);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Size = new System.Drawing.Size(430, 20);
            this.lblAyuda.TabIndex = 0;
            this.lblAyuda.Text = "No puede autorizar un vino que usted mismo dio de alta.";
            //
            // btnAutorizar
            //
            this.btnAutorizar.Enabled = false;
            this.btnAutorizar.Location = new System.Drawing.Point(13, 40);
            this.btnAutorizar.Name = "btnAutorizar";
            this.btnAutorizar.Size = new System.Drawing.Size(160, 38);
            this.btnAutorizar.TabIndex = 1;
            this.btnAutorizar.Text = "Autorizar";
            this.btnAutorizar.UseVisualStyleBackColor = true;
            this.btnAutorizar.Click += new System.EventHandler(this.btnAutorizar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(597, 40);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAutorizarAltaVino
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(710, 540);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvPendientes);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 420);
            this.Name = "frmAutorizarAltaVino";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Autorizar alta de vinos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAutorizarAltaVino_FormClosed);
            this.Load += new System.EventHandler(this.frmAutorizarAltaVino_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendientes)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvPendientes;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Button btnAutorizar;
        private System.Windows.Forms.Button btnCerrar;
    }
}
