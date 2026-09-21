namespace CAPAS
{
    partial class frmSocios
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
            this.dgvSocios = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblApellido = new System.Windows.Forms.Label();
            this.txtApellido = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblTelefono = new System.Windows.Forms.Label();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.lblDomicilio = new System.Windows.Forms.Label();
            this.txtDomicilio = new System.Windows.Forms.TextBox();
            this.lblPresupuesto = new System.Windows.Forms.Label();
            this.numPresupuesto = new System.Windows.Forms.NumericUpDown();
            this.chkActivo = new System.Windows.Forms.CheckBox();
            this.lblVarietales = new System.Windows.Forms.Label();
            this.chkVarietales = new System.Windows.Forms.CheckedListBox();
            this.btnGuardarSocio = new System.Windows.Forms.Button();
            this.btnNuevoSocio = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSocios)).BeginInit();
            this.panelInferior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPresupuesto)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(5, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(210, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Gestión de Socios";
            //
            // dgvSocios
            //
            this.dgvSocios.AllowUserToAddRows = false;
            this.dgvSocios.AllowUserToDeleteRows = false;
            this.dgvSocios.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSocios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSocios.Location = new System.Drawing.Point(5, 127);
            this.dgvSocios.MultiSelect = false;
            this.dgvSocios.Name = "dgvSocios";
            this.dgvSocios.ReadOnly = true;
            this.dgvSocios.RowHeadersWidth = 55;
            this.dgvSocios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSocios.Size = new System.Drawing.Size(700, 190);
            this.dgvSocios.TabIndex = 1;
            this.dgvSocios.SelectionChanged += new System.EventHandler(this.dgvSocios_SelectionChanged);
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblApellido);
            this.panelInferior.Controls.Add(this.txtApellido);
            this.panelInferior.Controls.Add(this.lblNombre);
            this.panelInferior.Controls.Add(this.txtNombre);
            this.panelInferior.Controls.Add(this.lblEmail);
            this.panelInferior.Controls.Add(this.txtEmail);
            this.panelInferior.Controls.Add(this.lblTelefono);
            this.panelInferior.Controls.Add(this.txtTelefono);
            this.panelInferior.Controls.Add(this.lblDomicilio);
            this.panelInferior.Controls.Add(this.txtDomicilio);
            this.panelInferior.Controls.Add(this.lblPresupuesto);
            this.panelInferior.Controls.Add(this.numPresupuesto);
            this.panelInferior.Controls.Add(this.chkActivo);
            this.panelInferior.Controls.Add(this.lblVarietales);
            this.panelInferior.Controls.Add(this.chkVarietales);
            this.panelInferior.Controls.Add(this.btnGuardarSocio);
            this.panelInferior.Controls.Add(this.btnNuevoSocio);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 330);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 340);
            this.panelInferior.TabIndex = 2;
            //
            // lblApellido
            //
            this.lblApellido.AutoSize = true;
            this.lblApellido.Location = new System.Drawing.Point(13, 13);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(68, 20);
            this.lblApellido.TabIndex = 0;
            this.lblApellido.Text = "Apellido:";
            //
            // txtApellido
            //
            this.txtApellido.Location = new System.Drawing.Point(120, 10);
            this.txtApellido.Name = "txtApellido";
            this.txtApellido.Size = new System.Drawing.Size(190, 26);
            this.txtApellido.TabIndex = 1;
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(13, 53);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(75, 20);
            this.lblNombre.TabIndex = 2;
            this.lblNombre.Text = "Nombre:";
            //
            // txtNombre
            //
            this.txtNombre.Location = new System.Drawing.Point(120, 50);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(190, 26);
            this.txtNombre.TabIndex = 3;
            //
            // lblEmail
            //
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(13, 93);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(68, 20);
            this.lblEmail.TabIndex = 4;
            this.lblEmail.Text = "Email:";
            //
            // txtEmail
            //
            this.txtEmail.Location = new System.Drawing.Point(120, 90);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(190, 26);
            this.txtEmail.TabIndex = 5;
            //
            // lblTelefono
            //
            this.lblTelefono.AutoSize = true;
            this.lblTelefono.Location = new System.Drawing.Point(13, 133);
            this.lblTelefono.Name = "lblTelefono";
            this.lblTelefono.Size = new System.Drawing.Size(68, 20);
            this.lblTelefono.TabIndex = 6;
            this.lblTelefono.Text = "Teléfono:";
            //
            // txtTelefono
            //
            this.txtTelefono.Location = new System.Drawing.Point(120, 130);
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(190, 26);
            this.txtTelefono.TabIndex = 7;
            //
            // lblDomicilio
            //
            this.lblDomicilio.AutoSize = true;
            this.lblDomicilio.Location = new System.Drawing.Point(13, 173);
            this.lblDomicilio.Name = "lblDomicilio";
            this.lblDomicilio.Size = new System.Drawing.Size(85, 20);
            this.lblDomicilio.TabIndex = 8;
            this.lblDomicilio.Text = "Domicilio:";
            //
            // txtDomicilio
            //
            this.txtDomicilio.Location = new System.Drawing.Point(120, 170);
            this.txtDomicilio.Name = "txtDomicilio";
            this.txtDomicilio.Size = new System.Drawing.Size(190, 26);
            this.txtDomicilio.TabIndex = 9;
            //
            // lblPresupuesto
            //
            this.lblPresupuesto.AutoSize = true;
            this.lblPresupuesto.Location = new System.Drawing.Point(360, 13);
            this.lblPresupuesto.Name = "lblPresupuesto";
            this.lblPresupuesto.Size = new System.Drawing.Size(140, 20);
            this.lblPresupuesto.TabIndex = 10;
            this.lblPresupuesto.Text = "Presupuesto mensual:";
            //
            // numPresupuesto
            //
            this.numPresupuesto.DecimalPlaces = 2;
            this.numPresupuesto.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPresupuesto.Location = new System.Drawing.Point(360, 36);
            this.numPresupuesto.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            this.numPresupuesto.Name = "numPresupuesto";
            this.numPresupuesto.Size = new System.Drawing.Size(150, 26);
            this.numPresupuesto.TabIndex = 11;
            //
            // chkActivo
            //
            this.chkActivo.AutoSize = true;
            this.chkActivo.Checked = true;
            this.chkActivo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkActivo.Location = new System.Drawing.Point(360, 72);
            this.chkActivo.Name = "chkActivo";
            this.chkActivo.Size = new System.Drawing.Size(80, 24);
            this.chkActivo.TabIndex = 12;
            this.chkActivo.Text = "Activo";
            this.chkActivo.UseVisualStyleBackColor = true;
            //
            // lblVarietales
            //
            this.lblVarietales.AutoSize = true;
            this.lblVarietales.Location = new System.Drawing.Point(360, 102);
            this.lblVarietales.Name = "lblVarietales";
            this.lblVarietales.Size = new System.Drawing.Size(150, 20);
            this.lblVarietales.TabIndex = 13;
            this.lblVarietales.Text = "Varietales preferidos:";
            //
            // chkVarietales
            //
            this.chkVarietales.CheckOnClick = true;
            this.chkVarietales.FormattingEnabled = true;
            this.chkVarietales.Location = new System.Drawing.Point(360, 126);
            this.chkVarietales.Name = "chkVarietales";
            this.chkVarietales.Size = new System.Drawing.Size(250, 148);
            this.chkVarietales.TabIndex = 14;
            //
            // btnGuardarSocio
            //
            this.btnGuardarSocio.Location = new System.Drawing.Point(13, 280);
            this.btnGuardarSocio.Name = "btnGuardarSocio";
            this.btnGuardarSocio.Size = new System.Drawing.Size(140, 38);
            this.btnGuardarSocio.TabIndex = 15;
            this.btnGuardarSocio.Text = "Guardar";
            this.btnGuardarSocio.UseVisualStyleBackColor = true;
            this.btnGuardarSocio.Click += new System.EventHandler(this.btnGuardarSocio_Click);
            //
            // btnNuevoSocio
            //
            this.btnNuevoSocio.Location = new System.Drawing.Point(160, 280);
            this.btnNuevoSocio.Name = "btnNuevoSocio";
            this.btnNuevoSocio.Size = new System.Drawing.Size(120, 38);
            this.btnNuevoSocio.TabIndex = 16;
            this.btnNuevoSocio.Text = "Nuevo";
            this.btnNuevoSocio.UseVisualStyleBackColor = true;
            this.btnNuevoSocio.Click += new System.EventHandler(this.btnNuevoSocio_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(597, 280);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 17;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmSocios
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(710, 670);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvSocios);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 560);
            this.Name = "frmSocios";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestión de Socios";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSocios_FormClosed);
            this.Load += new System.EventHandler(this.frmSocios_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSocios)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPresupuesto)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvSocios;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label lblDomicilio;
        private System.Windows.Forms.TextBox txtDomicilio;
        private System.Windows.Forms.Label lblPresupuesto;
        private System.Windows.Forms.NumericUpDown numPresupuesto;
        private System.Windows.Forms.CheckBox chkActivo;
        private System.Windows.Forms.Label lblVarietales;
        private System.Windows.Forms.CheckedListBox chkVarietales;
        private System.Windows.Forms.Button btnGuardarSocio;
        private System.Windows.Forms.Button btnNuevoSocio;
        private System.Windows.Forms.Button btnCerrar;
    }
}
