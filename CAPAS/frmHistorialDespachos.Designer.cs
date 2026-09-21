namespace CAPAS
{
    partial class frmHistorialDespachos
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
            this.dgvCajasDespachadas = new System.Windows.Forms.DataGridView();
            this.btnRegenerar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasDespachadas)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(13, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(240, 26);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Historial de Despachos";
            //
            // dgvCajasDespachadas
            //
            this.dgvCajasDespachadas.AllowUserToAddRows = false;
            this.dgvCajasDespachadas.AllowUserToDeleteRows = false;
            this.dgvCajasDespachadas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCajasDespachadas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCajasDespachadas.Location = new System.Drawing.Point(13, 130);
            this.dgvCajasDespachadas.MultiSelect = false;
            this.dgvCajasDespachadas.Name = "dgvCajasDespachadas";
            this.dgvCajasDespachadas.ReadOnly = true;
            this.dgvCajasDespachadas.RowHeadersWidth = 55;
            this.dgvCajasDespachadas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCajasDespachadas.Size = new System.Drawing.Size(860, 340);
            this.dgvCajasDespachadas.TabIndex = 1;
            this.dgvCajasDespachadas.SelectionChanged += new System.EventHandler(this.dgvCajasDespachadas_SelectionChanged);
            //
            // btnRegenerar
            //
            this.btnRegenerar.Enabled = false;
            this.btnRegenerar.Location = new System.Drawing.Point(13, 500);
            this.btnRegenerar.Name = "btnRegenerar";
            this.btnRegenerar.Size = new System.Drawing.Size(220, 55);
            this.btnRegenerar.TabIndex = 2;
            this.btnRegenerar.Text = "Regenerar remito";
            this.btnRegenerar.UseVisualStyleBackColor = true;
            this.btnRegenerar.Click += new System.EventHandler(this.btnRegenerar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(773, 500);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 55);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmHistorialDespachos
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.dgvCajasDespachadas);
            this.Controls.Add(this.btnRegenerar);
            this.Controls.Add(this.btnCerrar);
            this.MinimumSize = new System.Drawing.Size(850, 560);
            this.Name = "frmHistorialDespachos";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Historial de Despachos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmHistorialDespachos_FormClosed);
            this.Load += new System.EventHandler(this.frmHistorialDespachos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasDespachadas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvCajasDespachadas;
        private System.Windows.Forms.Button btnRegenerar;
        private System.Windows.Forms.Button btnCerrar;
    }
}
