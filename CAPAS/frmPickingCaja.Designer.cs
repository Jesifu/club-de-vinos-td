namespace CAPAS
{
    partial class frmPickingCaja
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
            this.lblReemplazo = new System.Windows.Forms.Label();
            this.cboVinoReemplazo = new System.Windows.Forms.ComboBox();
            this.lblMotivo = new System.Windows.Forms.Label();
            this.txtMotivo = new System.Windows.Forms.TextBox();
            this.btnRegistrarSustitucion = new System.Windows.Forms.Button();
            this.lblTraza = new System.Windows.Forms.Label();
            this.dgvTrazaSustituciones = new System.Windows.Forms.DataGridView();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasArmadas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComposicionEfectiva)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrazaSustituciones)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(13, 113);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(259, 26);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Picking y Sustituciones";
            // 
            // lblCajas
            // 
            this.lblCajas.AutoSize = true;
            this.lblCajas.Location = new System.Drawing.Point(13, 151);
            this.lblCajas.Name = "lblCajas";
            this.lblCajas.Size = new System.Drawing.Size(119, 20);
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
            this.dgvCajasArmadas.Location = new System.Drawing.Point(13, 176);
            this.dgvCajasArmadas.MultiSelect = false;
            this.dgvCajasArmadas.Name = "dgvCajasArmadas";
            this.dgvCajasArmadas.ReadOnly = true;
            this.dgvCajasArmadas.RowHeadersWidth = 55;
            this.dgvCajasArmadas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCajasArmadas.Size = new System.Drawing.Size(960, 130);
            this.dgvCajasArmadas.TabIndex = 2;
            this.dgvCajasArmadas.SelectionChanged += new System.EventHandler(this.dgvCajasArmadas_SelectionChanged);
            // 
            // lblComposicion
            // 
            this.lblComposicion.AutoSize = true;
            this.lblComposicion.Location = new System.Drawing.Point(13, 321);
            this.lblComposicion.Name = "lblComposicion";
            this.lblComposicion.Size = new System.Drawing.Size(163, 20);
            this.lblComposicion.TabIndex = 3;
            this.lblComposicion.Text = "Composición efectiva:";
            // 
            // dgvComposicionEfectiva
            // 
            this.dgvComposicionEfectiva.AllowUserToAddRows = false;
            this.dgvComposicionEfectiva.AllowUserToDeleteRows = false;
            this.dgvComposicionEfectiva.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvComposicionEfectiva.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvComposicionEfectiva.Location = new System.Drawing.Point(13, 346);
            this.dgvComposicionEfectiva.MultiSelect = false;
            this.dgvComposicionEfectiva.Name = "dgvComposicionEfectiva";
            this.dgvComposicionEfectiva.ReadOnly = true;
            this.dgvComposicionEfectiva.RowHeadersWidth = 55;
            this.dgvComposicionEfectiva.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvComposicionEfectiva.Size = new System.Drawing.Size(960, 130);
            this.dgvComposicionEfectiva.TabIndex = 4;
            this.dgvComposicionEfectiva.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvComposicionEfectiva_CellFormatting);
            // 
            // lblReemplazo
            // 
            this.lblReemplazo.AutoSize = true;
            this.lblReemplazo.Location = new System.Drawing.Point(13, 495);
            this.lblReemplazo.Name = "lblReemplazo";
            this.lblReemplazo.Size = new System.Drawing.Size(126, 20);
            this.lblReemplazo.TabIndex = 5;
            this.lblReemplazo.Text = "Reemplazar por:";
            // 
            // cboVinoReemplazo
            // 
            this.cboVinoReemplazo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVinoReemplazo.FormattingEnabled = true;
            this.cboVinoReemplazo.Location = new System.Drawing.Point(170, 492);
            this.cboVinoReemplazo.Name = "cboVinoReemplazo";
            this.cboVinoReemplazo.Size = new System.Drawing.Size(380, 28);
            this.cboVinoReemplazo.TabIndex = 6;
            // 
            // lblMotivo
            // 
            this.lblMotivo.AutoSize = true;
            this.lblMotivo.Location = new System.Drawing.Point(570, 495);
            this.lblMotivo.Name = "lblMotivo";
            this.lblMotivo.Size = new System.Drawing.Size(59, 20);
            this.lblMotivo.TabIndex = 7;
            this.lblMotivo.Text = "Motivo:";
            // 
            // txtMotivo
            // 
            this.txtMotivo.Location = new System.Drawing.Point(650, 492);
            this.txtMotivo.MaxLength = 200;
            this.txtMotivo.Name = "txtMotivo";
            this.txtMotivo.Size = new System.Drawing.Size(323, 26);
            this.txtMotivo.TabIndex = 8;
            // 
            // btnRegistrarSustitucion
            // 
            this.btnRegistrarSustitucion.Location = new System.Drawing.Point(13, 535);
            this.btnRegistrarSustitucion.Name = "btnRegistrarSustitucion";
            this.btnRegistrarSustitucion.Size = new System.Drawing.Size(260, 36);
            this.btnRegistrarSustitucion.TabIndex = 9;
            this.btnRegistrarSustitucion.Text = "Registrar sustitución";
            this.btnRegistrarSustitucion.UseVisualStyleBackColor = true;
            this.btnRegistrarSustitucion.Click += new System.EventHandler(this.btnRegistrarSustitucion_Click);
            // 
            // lblTraza
            // 
            this.lblTraza.AutoSize = true;
            this.lblTraza.Location = new System.Drawing.Point(13, 590);
            this.lblTraza.Name = "lblTraza";
            this.lblTraza.Size = new System.Drawing.Size(189, 20);
            this.lblTraza.TabIndex = 10;
            this.lblTraza.Text = "Historial de sustituciones:";
            // 
            // dgvTrazaSustituciones
            // 
            this.dgvTrazaSustituciones.AllowUserToAddRows = false;
            this.dgvTrazaSustituciones.AllowUserToDeleteRows = false;
            this.dgvTrazaSustituciones.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTrazaSustituciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTrazaSustituciones.Location = new System.Drawing.Point(13, 615);
            this.dgvTrazaSustituciones.MultiSelect = false;
            this.dgvTrazaSustituciones.Name = "dgvTrazaSustituciones";
            this.dgvTrazaSustituciones.ReadOnly = true;
            this.dgvTrazaSustituciones.RowHeadersWidth = 55;
            this.dgvTrazaSustituciones.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTrazaSustituciones.Size = new System.Drawing.Size(960, 103);
            this.dgvTrazaSustituciones.TabIndex = 11;
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(873, 741);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 12;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // frmPickingCaja
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1000, 804);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblCajas);
            this.Controls.Add(this.dgvCajasArmadas);
            this.Controls.Add(this.lblComposicion);
            this.Controls.Add(this.dgvComposicionEfectiva);
            this.Controls.Add(this.lblReemplazo);
            this.Controls.Add(this.cboVinoReemplazo);
            this.Controls.Add(this.lblMotivo);
            this.Controls.Add(this.txtMotivo);
            this.Controls.Add(this.btnRegistrarSustitucion);
            this.Controls.Add(this.lblTraza);
            this.Controls.Add(this.dgvTrazaSustituciones);
            this.Controls.Add(this.btnCerrar);
            this.MinimumSize = new System.Drawing.Size(950, 700);
            this.Name = "frmPickingCaja";
            this.Padding = new System.Windows.Forms.Padding(3, 93, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Picking y Sustituciones";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmPickingCaja_FormClosed);
            this.Load += new System.EventHandler(this.frmPickingCaja_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCajasArmadas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvComposicionEfectiva)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrazaSustituciones)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCajas;
        private System.Windows.Forms.DataGridView dgvCajasArmadas;
        private System.Windows.Forms.Label lblComposicion;
        private System.Windows.Forms.DataGridView dgvComposicionEfectiva;
        private System.Windows.Forms.Label lblReemplazo;
        private System.Windows.Forms.ComboBox cboVinoReemplazo;
        private System.Windows.Forms.Label lblMotivo;
        private System.Windows.Forms.TextBox txtMotivo;
        private System.Windows.Forms.Button btnRegistrarSustitucion;
        private System.Windows.Forms.Label lblTraza;
        private System.Windows.Forms.DataGridView dgvTrazaSustituciones;
        private System.Windows.Forms.Button btnCerrar;
    }
}
