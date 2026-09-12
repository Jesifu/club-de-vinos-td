namespace CAPAS
{
    partial class frmRegistrarEntradaStock
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
            this.dgvMovimientos = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblVino = new System.Windows.Forms.Label();
            this.cboVino = new System.Windows.Forms.ComboBox();
            this.lblStockActual = new System.Windows.Forms.Label();
            this.lblStockActualValor = new System.Windows.Forms.Label();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.numCantidad = new System.Windows.Forms.NumericUpDown();
            this.lblMotivo = new System.Windows.Forms.Label();
            this.txtMotivo = new System.Windows.Forms.TextBox();
            this.btnRegistrarEntrada = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMovimientos)).BeginInit();
            this.panelInferior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).BeginInit();
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
            this.lblTitulo.Text = "Registrar entrada de stock";
            //
            // dgvMovimientos
            //
            this.dgvMovimientos.AllowUserToAddRows = false;
            this.dgvMovimientos.AllowUserToDeleteRows = false;
            this.dgvMovimientos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMovimientos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMovimientos.Location = new System.Drawing.Point(5, 127);
            this.dgvMovimientos.MultiSelect = false;
            this.dgvMovimientos.Name = "dgvMovimientos";
            this.dgvMovimientos.ReadOnly = true;
            this.dgvMovimientos.RowHeadersWidth = 55;
            this.dgvMovimientos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMovimientos.Size = new System.Drawing.Size(700, 190);
            this.dgvMovimientos.TabIndex = 1;
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblVino);
            this.panelInferior.Controls.Add(this.cboVino);
            this.panelInferior.Controls.Add(this.lblStockActual);
            this.panelInferior.Controls.Add(this.lblStockActualValor);
            this.panelInferior.Controls.Add(this.lblCantidad);
            this.panelInferior.Controls.Add(this.numCantidad);
            this.panelInferior.Controls.Add(this.lblMotivo);
            this.panelInferior.Controls.Add(this.txtMotivo);
            this.panelInferior.Controls.Add(this.btnRegistrarEntrada);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 330);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 270);
            this.panelInferior.TabIndex = 2;
            //
            // lblVino
            //
            this.lblVino.AutoSize = true;
            this.lblVino.Location = new System.Drawing.Point(13, 13);
            this.lblVino.Name = "lblVino";
            this.lblVino.Size = new System.Drawing.Size(60, 20);
            this.lblVino.TabIndex = 0;
            this.lblVino.Text = "Vino:";
            //
            // cboVino
            //
            this.cboVino.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVino.Location = new System.Drawing.Point(120, 10);
            this.cboVino.Name = "cboVino";
            this.cboVino.Size = new System.Drawing.Size(260, 28);
            this.cboVino.TabIndex = 1;
            this.cboVino.SelectedIndexChanged += new System.EventHandler(this.cboVino_SelectedIndexChanged);
            //
            // lblStockActual
            //
            this.lblStockActual.AutoSize = true;
            this.lblStockActual.Location = new System.Drawing.Point(400, 13);
            this.lblStockActual.Name = "lblStockActual";
            this.lblStockActual.Size = new System.Drawing.Size(110, 20);
            this.lblStockActual.TabIndex = 2;
            this.lblStockActual.Text = "Stock actual:";
            //
            // lblStockActualValor
            //
            this.lblStockActualValor.AutoSize = true;
            this.lblStockActualValor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblStockActualValor.Location = new System.Drawing.Point(520, 13);
            this.lblStockActualValor.Name = "lblStockActualValor";
            this.lblStockActualValor.Size = new System.Drawing.Size(30, 20);
            this.lblStockActualValor.TabIndex = 3;
            this.lblStockActualValor.Text = "-";
            //
            // lblCantidad
            //
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Location = new System.Drawing.Point(13, 53);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(90, 20);
            this.lblCantidad.TabIndex = 4;
            this.lblCantidad.Text = "Cantidad:";
            //
            // numCantidad
            //
            this.numCantidad.Location = new System.Drawing.Point(120, 51);
            this.numCantidad.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numCantidad.Name = "numCantidad";
            this.numCantidad.Size = new System.Drawing.Size(120, 26);
            this.numCantidad.TabIndex = 5;
            //
            // lblMotivo
            //
            this.lblMotivo.AutoSize = true;
            this.lblMotivo.Location = new System.Drawing.Point(13, 93);
            this.lblMotivo.Name = "lblMotivo";
            this.lblMotivo.Size = new System.Drawing.Size(80, 20);
            this.lblMotivo.TabIndex = 6;
            this.lblMotivo.Text = "Motivo:";
            //
            // txtMotivo
            //
            this.txtMotivo.Location = new System.Drawing.Point(120, 90);
            this.txtMotivo.Name = "txtMotivo";
            this.txtMotivo.Size = new System.Drawing.Size(430, 26);
            this.txtMotivo.TabIndex = 7;
            //
            // btnRegistrarEntrada
            //
            this.btnRegistrarEntrada.Location = new System.Drawing.Point(13, 140);
            this.btnRegistrarEntrada.Name = "btnRegistrarEntrada";
            this.btnRegistrarEntrada.Size = new System.Drawing.Size(200, 38);
            this.btnRegistrarEntrada.TabIndex = 8;
            this.btnRegistrarEntrada.Text = "Registrar entrada";
            this.btnRegistrarEntrada.UseVisualStyleBackColor = true;
            this.btnRegistrarEntrada.Click += new System.EventHandler(this.btnRegistrarEntrada_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(597, 140);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 9;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmRegistrarEntradaStock
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(710, 600);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvMovimientos);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 500);
            this.Name = "frmRegistrarEntradaStock";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Registrar entrada de stock";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmRegistrarEntradaStock_FormClosed);
            this.Load += new System.EventHandler(this.frmRegistrarEntradaStock_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMovimientos)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvMovimientos;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblVino;
        private System.Windows.Forms.ComboBox cboVino;
        private System.Windows.Forms.Label lblStockActual;
        private System.Windows.Forms.Label lblStockActualValor;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;
        private System.Windows.Forms.Label lblMotivo;
        private System.Windows.Forms.TextBox txtMotivo;
        private System.Windows.Forms.Button btnRegistrarEntrada;
        private System.Windows.Forms.Button btnCerrar;
    }
}
