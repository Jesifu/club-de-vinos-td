namespace CAPAS
{
    partial class frmArmarCaja
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
            this.lblSocio = new System.Windows.Forms.Label();
            this.cboSocio = new System.Windows.Forms.ComboBox();
            this.lblPeriodo = new System.Windows.Forms.Label();
            this.txtPeriodo = new System.Windows.Forms.TextBox();
            this.lblCandidatos = new System.Windows.Forms.Label();
            this.dgvCandidatos = new System.Windows.Forms.DataGridView();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.numCantidad = new System.Windows.Forms.NumericUpDown();
            this.btnAgregarLinea = new System.Windows.Forms.Button();
            this.lblLineas = new System.Windows.Forms.Label();
            this.dgvLineas = new System.Windows.Forms.DataGridView();
            this.lblPresupuesto = new System.Windows.Forms.Label();
            this.lblPresupuestoValor = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblTotalValor = new System.Windows.Forms.Label();
            this.btnConfirmarArmado = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCandidatos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(13, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(229, 26);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Armar Caja Mensual";
            // 
            // lblSocio
            // 
            this.lblSocio.AutoSize = true;
            this.lblSocio.Location = new System.Drawing.Point(13, 122);
            this.lblSocio.Name = "lblSocio";
            this.lblSocio.Size = new System.Drawing.Size(53, 20);
            this.lblSocio.TabIndex = 1;
            this.lblSocio.Text = "Socio:";
            // 
            // cboSocio
            // 
            this.cboSocio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSocio.FormattingEnabled = true;
            this.cboSocio.Location = new System.Drawing.Point(90, 118);
            this.cboSocio.Name = "cboSocio";
            this.cboSocio.Size = new System.Drawing.Size(280, 28);
            this.cboSocio.TabIndex = 2;
            this.cboSocio.SelectedIndexChanged += new System.EventHandler(this.cboSocio_SelectedIndexChanged);
            // 
            // lblPeriodo
            // 
            this.lblPeriodo.AutoSize = true;
            this.lblPeriodo.Location = new System.Drawing.Point(400, 122);
            this.lblPeriodo.Name = "lblPeriodo";
            this.lblPeriodo.Size = new System.Drawing.Size(156, 20);
            this.lblPeriodo.TabIndex = 3;
            this.lblPeriodo.Text = "Período (AAAA-MM):";
            // 
            // txtPeriodo
            // 
            this.txtPeriodo.Location = new System.Drawing.Point(600, 118);
            this.txtPeriodo.MaxLength = 7;
            this.txtPeriodo.Name = "txtPeriodo";
            this.txtPeriodo.Size = new System.Drawing.Size(100, 26);
            this.txtPeriodo.TabIndex = 4;
            this.txtPeriodo.TextChanged += new System.EventHandler(this.txtPeriodo_TextChanged);
            // 
            // lblCandidatos
            // 
            this.lblCandidatos.AutoSize = true;
            this.lblCandidatos.Location = new System.Drawing.Point(13, 158);
            this.lblCandidatos.Name = "lblCandidatos";
            this.lblCandidatos.Size = new System.Drawing.Size(229, 20);
            this.lblCandidatos.TabIndex = 5;
            this.lblCandidatos.Text = "Vinos disponibles para el socio:";
            // 
            // dgvCandidatos
            // 
            this.dgvCandidatos.AllowUserToAddRows = false;
            this.dgvCandidatos.AllowUserToDeleteRows = false;
            this.dgvCandidatos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCandidatos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCandidatos.Location = new System.Drawing.Point(13, 183);
            this.dgvCandidatos.MultiSelect = false;
            this.dgvCandidatos.Name = "dgvCandidatos";
            this.dgvCandidatos.ReadOnly = true;
            this.dgvCandidatos.RowHeadersWidth = 55;
            this.dgvCandidatos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCandidatos.Size = new System.Drawing.Size(860, 180);
            this.dgvCandidatos.TabIndex = 6;
            this.dgvCandidatos.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvCandidatos_CellFormatting);
            // 
            // lblCantidad
            // 
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Location = new System.Drawing.Point(13, 378);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(77, 20);
            this.lblCantidad.TabIndex = 7;
            this.lblCantidad.Text = "Cantidad:";
            // 
            // numCantidad
            // 
            this.numCantidad.Location = new System.Drawing.Point(127, 372);
            this.numCantidad.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numCantidad.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCantidad.Name = "numCantidad";
            this.numCantidad.Size = new System.Drawing.Size(93, 26);
            this.numCantidad.TabIndex = 8;
            this.numCantidad.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnAgregarLinea
            // 
            this.btnAgregarLinea.Location = new System.Drawing.Point(236, 364);
            this.btnAgregarLinea.Name = "btnAgregarLinea";
            this.btnAgregarLinea.Size = new System.Drawing.Size(180, 37);
            this.btnAgregarLinea.TabIndex = 9;
            this.btnAgregarLinea.Text = "Agregar a la caja";
            this.btnAgregarLinea.UseVisualStyleBackColor = true;
            this.btnAgregarLinea.Click += new System.EventHandler(this.btnAgregarLinea_Click);
            // 
            // lblLineas
            // 
            this.lblLineas.AutoSize = true;
            this.lblLineas.Location = new System.Drawing.Point(13, 418);
            this.lblLineas.Name = "lblLineas";
            this.lblLineas.Size = new System.Drawing.Size(124, 20);
            this.lblLineas.TabIndex = 10;
            this.lblLineas.Text = "Vinos en la caja:";
            // 
            // dgvLineas
            // 
            this.dgvLineas.AllowUserToAddRows = false;
            this.dgvLineas.AllowUserToDeleteRows = false;
            this.dgvLineas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLineas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLineas.Location = new System.Drawing.Point(13, 443);
            this.dgvLineas.MultiSelect = false;
            this.dgvLineas.Name = "dgvLineas";
            this.dgvLineas.ReadOnly = true;
            this.dgvLineas.RowHeadersWidth = 55;
            this.dgvLineas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLineas.Size = new System.Drawing.Size(860, 145);
            this.dgvLineas.TabIndex = 11;
            this.dgvLineas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLineas_CellDoubleClick);
            // 
            // lblPresupuesto
            // 
            this.lblPresupuesto.AutoSize = true;
            this.lblPresupuesto.Location = new System.Drawing.Point(13, 605);
            this.lblPresupuesto.Name = "lblPresupuesto";
            this.lblPresupuesto.Size = new System.Drawing.Size(167, 20);
            this.lblPresupuesto.TabIndex = 12;
            this.lblPresupuesto.Text = "Presupuesto mensual:";
            // 
            // lblPresupuestoValor
            // 
            this.lblPresupuestoValor.AutoSize = true;
            this.lblPresupuestoValor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblPresupuestoValor.Location = new System.Drawing.Point(200, 605);
            this.lblPresupuestoValor.Name = "lblPresupuestoValor";
            this.lblPresupuestoValor.Size = new System.Drawing.Size(20, 25);
            this.lblPresupuestoValor.TabIndex = 13;
            this.lblPresupuestoValor.Text = "-";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(400, 605);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(48, 20);
            this.lblTotal.TabIndex = 14;
            this.lblTotal.Text = "Total:";
            // 
            // lblTotalValor
            // 
            this.lblTotalValor.AutoSize = true;
            this.lblTotalValor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalValor.Location = new System.Drawing.Point(460, 605);
            this.lblTotalValor.Name = "lblTotalValor";
            this.lblTotalValor.Size = new System.Drawing.Size(20, 25);
            this.lblTotalValor.TabIndex = 15;
            this.lblTotalValor.Text = "-";
            // 
            // btnConfirmarArmado
            // 
            this.btnConfirmarArmado.Enabled = false;
            this.btnConfirmarArmado.Location = new System.Drawing.Point(13, 638);
            this.btnConfirmarArmado.Name = "btnConfirmarArmado";
            this.btnConfirmarArmado.Size = new System.Drawing.Size(220, 38);
            this.btnConfirmarArmado.TabIndex = 16;
            this.btnConfirmarArmado.Text = "Confirmar armado";
            this.btnConfirmarArmado.UseVisualStyleBackColor = true;
            this.btnConfirmarArmado.Click += new System.EventHandler(this.btnConfirmarArmado_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(773, 638);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 17;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // frmArmarCaja
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(900, 700);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblSocio);
            this.Controls.Add(this.cboSocio);
            this.Controls.Add(this.lblPeriodo);
            this.Controls.Add(this.txtPeriodo);
            this.Controls.Add(this.lblCandidatos);
            this.Controls.Add(this.dgvCandidatos);
            this.Controls.Add(this.lblCantidad);
            this.Controls.Add(this.numCantidad);
            this.Controls.Add(this.btnAgregarLinea);
            this.Controls.Add(this.lblLineas);
            this.Controls.Add(this.dgvLineas);
            this.Controls.Add(this.lblPresupuesto);
            this.Controls.Add(this.lblPresupuestoValor);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblTotalValor);
            this.Controls.Add(this.btnConfirmarArmado);
            this.Controls.Add(this.btnCerrar);
            this.MinimumSize = new System.Drawing.Size(850, 650);
            this.Name = "frmArmarCaja";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Armar Caja Mensual";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmArmarCaja_FormClosed);
            this.Load += new System.EventHandler(this.frmArmarCaja_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCandidatos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSocio;
        private System.Windows.Forms.ComboBox cboSocio;
        private System.Windows.Forms.Label lblPeriodo;
        private System.Windows.Forms.TextBox txtPeriodo;
        private System.Windows.Forms.Label lblCandidatos;
        private System.Windows.Forms.DataGridView dgvCandidatos;
        private System.Windows.Forms.Label lblCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;
        private System.Windows.Forms.Button btnAgregarLinea;
        private System.Windows.Forms.Label lblLineas;
        private System.Windows.Forms.DataGridView dgvLineas;
        private System.Windows.Forms.Label lblPresupuesto;
        private System.Windows.Forms.Label lblPresupuestoValor;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalValor;
        private System.Windows.Forms.Button btnConfirmarArmado;
        private System.Windows.Forms.Button btnCerrar;
    }
}
