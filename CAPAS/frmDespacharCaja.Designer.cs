namespace CAPAS
{
    partial class frmDespacharCaja
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
            this.lblCajas = new System.Windows.Forms.Label();
            this.dgvCajasArmadas = new System.Windows.Forms.DataGridView();
            this.lblComposicion = new System.Windows.Forms.Label();
            this.dgvComposicionEfectiva = new System.Windows.Forms.DataGridView();
            this.btnDespachar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasArmadas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComposicionEfectiva)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(13, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(240, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Despachar Cajas";
            //
            // lblCajas
            //
            this.lblCajas.AutoSize = true;
            this.lblCajas.Location = new System.Drawing.Point(13, 122);
            this.lblCajas.Name = "lblCajas";
            this.lblCajas.Size = new System.Drawing.Size(150, 20);
            this.lblCajas.TabIndex = 1;
            this.lblCajas.Text = "Cajas armadas:";
            //
            // dgvCajasArmadas
            //
            this.dgvCajasArmadas.AllowUserToAddRows = false;
            this.dgvCajasArmadas.AllowUserToDeleteRows = false;
            this.dgvCajasArmadas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCajasArmadas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCajasArmadas.Location = new System.Drawing.Point(13, 147);
            this.dgvCajasArmadas.MultiSelect = false;
            this.dgvCajasArmadas.Name = "dgvCajasArmadas";
            this.dgvCajasArmadas.ReadOnly = true;
            this.dgvCajasArmadas.RowHeadersWidth = 55;
            this.dgvCajasArmadas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCajasArmadas.Size = new System.Drawing.Size(860, 190);
            this.dgvCajasArmadas.TabIndex = 2;
            this.dgvCajasArmadas.SelectionChanged += new System.EventHandler(this.dgvCajasArmadas_SelectionChanged);
            //
            // lblComposicion
            //
            this.lblComposicion.AutoSize = true;
            this.lblComposicion.Location = new System.Drawing.Point(13, 352);
            this.lblComposicion.Name = "lblComposicion";
            this.lblComposicion.Size = new System.Drawing.Size(220, 20);
            this.lblComposicion.TabIndex = 3;
            this.lblComposicion.Text = "Composición efectiva:";
            //
            // dgvComposicionEfectiva
            //
            this.dgvComposicionEfectiva.AllowUserToAddRows = false;
            this.dgvComposicionEfectiva.AllowUserToDeleteRows = false;
            this.dgvComposicionEfectiva.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvComposicionEfectiva.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvComposicionEfectiva.Location = new System.Drawing.Point(13, 377);
            this.dgvComposicionEfectiva.MultiSelect = false;
            this.dgvComposicionEfectiva.Name = "dgvComposicionEfectiva";
            this.dgvComposicionEfectiva.ReadOnly = true;
            this.dgvComposicionEfectiva.RowHeadersWidth = 55;
            this.dgvComposicionEfectiva.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvComposicionEfectiva.Size = new System.Drawing.Size(860, 210);
            this.dgvComposicionEfectiva.TabIndex = 4;
            this.dgvComposicionEfectiva.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvComposicionEfectiva_CellFormatting);
            //
            // btnDespachar
            //
            this.btnDespachar.Enabled = false;
            this.btnDespachar.Location = new System.Drawing.Point(13, 605);
            this.btnDespachar.Name = "btnDespachar";
            this.btnDespachar.Size = new System.Drawing.Size(220, 38);
            this.btnDespachar.TabIndex = 5;
            this.btnDespachar.Text = "Despachar caja";
            this.btnDespachar.UseVisualStyleBackColor = true;
            this.btnDespachar.Click += new System.EventHandler(this.btnDespachar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(773, 605);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 6;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmDespacharCaja
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(900, 665);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblCajas);
            this.Controls.Add(this.dgvCajasArmadas);
            this.Controls.Add(this.lblComposicion);
            this.Controls.Add(this.dgvComposicionEfectiva);
            this.Controls.Add(this.btnDespachar);
            this.Controls.Add(this.btnCerrar);
            this.MinimumSize = new System.Drawing.Size(850, 600);
            this.Name = "frmDespacharCaja";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Despachar Cajas";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDespacharCaja_FormClosed);
            this.Load += new System.EventHandler(this.frmDespacharCaja_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasArmadas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComposicionEfectiva)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCajas;
        private System.Windows.Forms.DataGridView dgvCajasArmadas;
        private System.Windows.Forms.Label lblComposicion;
        private System.Windows.Forms.DataGridView dgvComposicionEfectiva;
        private System.Windows.Forms.Button btnDespachar;
        private System.Windows.Forms.Button btnCerrar;
    }
}
