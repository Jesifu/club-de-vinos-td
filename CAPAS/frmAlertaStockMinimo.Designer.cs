namespace CAPAS
{
    partial class frmAlertaStockMinimo
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
            this.lblAyudaAlertaStock = new System.Windows.Forms.Label();
            this.dgvAlertas = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblSinAlertas = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlertas)).BeginInit();
            this.panelInferior.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(5, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(220, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Alerta de stock mínimo";
            //
            // lblAyudaAlertaStock
            //
            this.lblAyudaAlertaStock.AutoSize = true;
            this.lblAyudaAlertaStock.Location = new System.Drawing.Point(5, 108);
            this.lblAyudaAlertaStock.Name = "lblAyudaAlertaStock";
            this.lblAyudaAlertaStock.Size = new System.Drawing.Size(500, 20);
            this.lblAyudaAlertaStock.TabIndex = 1;
            this.lblAyudaAlertaStock.Text = "Vinos activos cuyo stock actual está por debajo del stock mínimo.";
            //
            // dgvAlertas
            //
            this.dgvAlertas.AllowUserToAddRows = false;
            this.dgvAlertas.AllowUserToDeleteRows = false;
            this.dgvAlertas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAlertas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAlertas.Location = new System.Drawing.Point(5, 140);
            this.dgvAlertas.MultiSelect = false;
            this.dgvAlertas.Name = "dgvAlertas";
            this.dgvAlertas.ReadOnly = true;
            this.dgvAlertas.RowHeadersWidth = 55;
            this.dgvAlertas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAlertas.Size = new System.Drawing.Size(700, 320);
            this.dgvAlertas.TabIndex = 2;
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblSinAlertas);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 470);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 70);
            this.panelInferior.TabIndex = 3;
            //
            // lblSinAlertas
            //
            this.lblSinAlertas.AutoSize = true;
            this.lblSinAlertas.Location = new System.Drawing.Point(13, 25);
            this.lblSinAlertas.Name = "lblSinAlertas";
            this.lblSinAlertas.Size = new System.Drawing.Size(330, 20);
            this.lblSinAlertas.TabIndex = 0;
            this.lblSinAlertas.Text = "No hay vinos por debajo del stock mínimo.";
            this.lblSinAlertas.Visible = false;
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(597, 20);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAlertaStockMinimo
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(710, 540);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvAlertas);
            this.Controls.Add(this.lblAyudaAlertaStock);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 420);
            this.Name = "frmAlertaStockMinimo";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Alerta de stock mínimo";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAlertaStockMinimo_FormClosed);
            this.Load += new System.EventHandler(this.frmAlertaStockMinimo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlertas)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblAyudaAlertaStock;
        private System.Windows.Forms.DataGridView dgvAlertas;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblSinAlertas;
        private System.Windows.Forms.Button btnCerrar;
    }
}
