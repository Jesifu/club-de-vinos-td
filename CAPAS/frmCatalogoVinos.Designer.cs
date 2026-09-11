namespace CAPAS
{
    partial class frmCatalogoVinos
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
            this.dgvVinos = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblBodega = new System.Windows.Forms.Label();
            this.cboBodega = new System.Windows.Forms.ComboBox();
            this.lblVarietal = new System.Windows.Forms.Label();
            this.txtVarietal = new System.Windows.Forms.TextBox();
            this.lblMaridaje = new System.Windows.Forms.Label();
            this.txtMaridaje = new System.Windows.Forms.TextBox();
            this.lblAniada = new System.Windows.Forms.Label();
            this.numAniada = new System.Windows.Forms.NumericUpDown();
            this.lblPrecio = new System.Windows.Forms.Label();
            this.numPrecio = new System.Windows.Forms.NumericUpDown();
            this.lblStockMinimo = new System.Windows.Forms.Label();
            this.numStockMinimo = new System.Windows.Forms.NumericUpDown();
            this.chkPuntaje = new System.Windows.Forms.CheckBox();
            this.lblPuntaje = new System.Windows.Forms.Label();
            this.numPuntaje = new System.Windows.Forms.NumericUpDown();
            this.btnGuardarVino = new System.Windows.Forms.Button();
            this.btnLimpiarVino = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVinos)).BeginInit();
            this.panelInferior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAniada)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPrecio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStockMinimo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPuntaje)).BeginInit();
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
            this.lblTitulo.Text = "Catálogo de vinos";
            //
            // dgvVinos
            //
            this.dgvVinos.AllowUserToAddRows = false;
            this.dgvVinos.AllowUserToDeleteRows = false;
            this.dgvVinos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVinos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVinos.Location = new System.Drawing.Point(5, 127);
            this.dgvVinos.MultiSelect = false;
            this.dgvVinos.Name = "dgvVinos";
            this.dgvVinos.ReadOnly = true;
            this.dgvVinos.RowHeadersWidth = 55;
            this.dgvVinos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVinos.Size = new System.Drawing.Size(700, 190);
            this.dgvVinos.TabIndex = 1;
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblCodigo);
            this.panelInferior.Controls.Add(this.txtCodigo);
            this.panelInferior.Controls.Add(this.lblNombre);
            this.panelInferior.Controls.Add(this.txtNombre);
            this.panelInferior.Controls.Add(this.lblBodega);
            this.panelInferior.Controls.Add(this.cboBodega);
            this.panelInferior.Controls.Add(this.lblVarietal);
            this.panelInferior.Controls.Add(this.txtVarietal);
            this.panelInferior.Controls.Add(this.lblMaridaje);
            this.panelInferior.Controls.Add(this.txtMaridaje);
            this.panelInferior.Controls.Add(this.lblAniada);
            this.panelInferior.Controls.Add(this.numAniada);
            this.panelInferior.Controls.Add(this.lblPrecio);
            this.panelInferior.Controls.Add(this.numPrecio);
            this.panelInferior.Controls.Add(this.lblStockMinimo);
            this.panelInferior.Controls.Add(this.numStockMinimo);
            this.panelInferior.Controls.Add(this.chkPuntaje);
            this.panelInferior.Controls.Add(this.lblPuntaje);
            this.panelInferior.Controls.Add(this.numPuntaje);
            this.panelInferior.Controls.Add(this.btnGuardarVino);
            this.panelInferior.Controls.Add(this.btnLimpiarVino);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 330);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(710, 270);
            this.panelInferior.TabIndex = 2;
            //
            // lblCodigo
            //
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Location = new System.Drawing.Point(13, 13);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(60, 20);
            this.lblCodigo.TabIndex = 0;
            this.lblCodigo.Text = "SKU:";
            //
            // txtCodigo
            //
            this.txtCodigo.Location = new System.Drawing.Point(120, 10);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(190, 26);
            this.txtCodigo.TabIndex = 1;
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
            // lblBodega
            //
            this.lblBodega.AutoSize = true;
            this.lblBodega.Location = new System.Drawing.Point(13, 93);
            this.lblBodega.Name = "lblBodega";
            this.lblBodega.Size = new System.Drawing.Size(70, 20);
            this.lblBodega.TabIndex = 4;
            this.lblBodega.Text = "Bodega:";
            //
            // cboBodega
            //
            this.cboBodega.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBodega.Location = new System.Drawing.Point(120, 90);
            this.cboBodega.Name = "cboBodega";
            this.cboBodega.Size = new System.Drawing.Size(190, 28);
            this.cboBodega.TabIndex = 5;
            //
            // lblVarietal
            //
            this.lblVarietal.AutoSize = true;
            this.lblVarietal.Location = new System.Drawing.Point(13, 133);
            this.lblVarietal.Name = "lblVarietal";
            this.lblVarietal.Size = new System.Drawing.Size(70, 20);
            this.lblVarietal.TabIndex = 6;
            this.lblVarietal.Text = "Varietal:";
            //
            // txtVarietal
            //
            this.txtVarietal.Location = new System.Drawing.Point(120, 130);
            this.txtVarietal.Name = "txtVarietal";
            this.txtVarietal.Size = new System.Drawing.Size(190, 26);
            this.txtVarietal.TabIndex = 7;
            //
            // lblMaridaje
            //
            this.lblMaridaje.AutoSize = true;
            this.lblMaridaje.Location = new System.Drawing.Point(13, 173);
            this.lblMaridaje.Name = "lblMaridaje";
            this.lblMaridaje.Size = new System.Drawing.Size(85, 20);
            this.lblMaridaje.TabIndex = 8;
            this.lblMaridaje.Text = "Maridaje:";
            //
            // txtMaridaje
            //
            this.txtMaridaje.Location = new System.Drawing.Point(120, 170);
            this.txtMaridaje.Name = "txtMaridaje";
            this.txtMaridaje.Size = new System.Drawing.Size(310, 26);
            this.txtMaridaje.TabIndex = 9;
            //
            // lblAniada
            //
            this.lblAniada.AutoSize = true;
            this.lblAniada.Location = new System.Drawing.Point(360, 13);
            this.lblAniada.Name = "lblAniada";
            this.lblAniada.Size = new System.Drawing.Size(70, 20);
            this.lblAniada.TabIndex = 10;
            this.lblAniada.Text = "Añada:";
            //
            // numAniada
            //
            this.numAniada.Location = new System.Drawing.Point(460, 11);
            this.numAniada.Maximum = new decimal(new int[] { 2100, 0, 0, 0 });
            this.numAniada.Minimum = new decimal(new int[] { 1900, 0, 0, 0 });
            this.numAniada.Name = "numAniada";
            this.numAniada.Size = new System.Drawing.Size(110, 26);
            this.numAniada.TabIndex = 11;
            this.numAniada.Value = new decimal(new int[] { 2020, 0, 0, 0 });
            //
            // lblPrecio
            //
            this.lblPrecio.AutoSize = true;
            this.lblPrecio.Location = new System.Drawing.Point(360, 53);
            this.lblPrecio.Name = "lblPrecio";
            this.lblPrecio.Size = new System.Drawing.Size(65, 20);
            this.lblPrecio.TabIndex = 12;
            this.lblPrecio.Text = "Precio:";
            //
            // numPrecio
            //
            this.numPrecio.DecimalPlaces = 2;
            this.numPrecio.Increment = new decimal(new int[] { 50, 0, 0, 65536 });
            this.numPrecio.Location = new System.Drawing.Point(460, 51);
            this.numPrecio.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            this.numPrecio.Name = "numPrecio";
            this.numPrecio.Size = new System.Drawing.Size(110, 26);
            this.numPrecio.TabIndex = 13;
            //
            // lblStockMinimo
            //
            this.lblStockMinimo.AutoSize = true;
            this.lblStockMinimo.Location = new System.Drawing.Point(360, 93);
            this.lblStockMinimo.Name = "lblStockMinimo";
            this.lblStockMinimo.Size = new System.Drawing.Size(115, 20);
            this.lblStockMinimo.TabIndex = 14;
            this.lblStockMinimo.Text = "Stock mínimo:";
            //
            // numStockMinimo
            //
            this.numStockMinimo.Location = new System.Drawing.Point(460, 91);
            this.numStockMinimo.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numStockMinimo.Name = "numStockMinimo";
            this.numStockMinimo.Size = new System.Drawing.Size(110, 26);
            this.numStockMinimo.TabIndex = 15;
            //
            // chkPuntaje
            //
            this.chkPuntaje.AutoSize = true;
            this.chkPuntaje.Location = new System.Drawing.Point(360, 134);
            this.chkPuntaje.Name = "chkPuntaje";
            this.chkPuntaje.Size = new System.Drawing.Size(140, 24);
            this.chkPuntaje.TabIndex = 16;
            this.chkPuntaje.Text = "Incluir puntaje";
            this.chkPuntaje.UseVisualStyleBackColor = true;
            this.chkPuntaje.CheckedChanged += new System.EventHandler(this.chkPuntaje_CheckedChanged);
            //
            // lblPuntaje
            //
            this.lblPuntaje.AutoSize = true;
            this.lblPuntaje.Location = new System.Drawing.Point(360, 173);
            this.lblPuntaje.Name = "lblPuntaje";
            this.lblPuntaje.Size = new System.Drawing.Size(70, 20);
            this.lblPuntaje.TabIndex = 17;
            this.lblPuntaje.Text = "Puntaje:";
            //
            // numPuntaje
            //
            this.numPuntaje.Enabled = false;
            this.numPuntaje.Location = new System.Drawing.Point(460, 171);
            this.numPuntaje.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPuntaje.Name = "numPuntaje";
            this.numPuntaje.Size = new System.Drawing.Size(110, 26);
            this.numPuntaje.TabIndex = 18;
            //
            // btnGuardarVino
            //
            this.btnGuardarVino.Location = new System.Drawing.Point(13, 218);
            this.btnGuardarVino.Name = "btnGuardarVino";
            this.btnGuardarVino.Size = new System.Drawing.Size(140, 38);
            this.btnGuardarVino.TabIndex = 19;
            this.btnGuardarVino.Text = "Proponer alta";
            this.btnGuardarVino.UseVisualStyleBackColor = true;
            this.btnGuardarVino.Click += new System.EventHandler(this.btnGuardarVino_Click);
            //
            // btnLimpiarVino
            //
            this.btnLimpiarVino.Location = new System.Drawing.Point(160, 218);
            this.btnLimpiarVino.Name = "btnLimpiarVino";
            this.btnLimpiarVino.Size = new System.Drawing.Size(120, 38);
            this.btnLimpiarVino.TabIndex = 20;
            this.btnLimpiarVino.Text = "Limpiar";
            this.btnLimpiarVino.UseVisualStyleBackColor = true;
            this.btnLimpiarVino.Click += new System.EventHandler(this.btnLimpiarVino_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(597, 218);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 21;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmCatalogoVinos
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(710, 600);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvVinos);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(650, 500);
            this.Name = "frmCatalogoVinos";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Catálogo de vinos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmCatalogoVinos_FormClosed);
            this.Load += new System.EventHandler(this.frmCatalogoVinos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVinos)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.panelInferior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAniada)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPrecio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStockMinimo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPuntaje)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvVinos;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Label lblCodigo;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblBodega;
        private System.Windows.Forms.ComboBox cboBodega;
        private System.Windows.Forms.Label lblVarietal;
        private System.Windows.Forms.TextBox txtVarietal;
        private System.Windows.Forms.Label lblMaridaje;
        private System.Windows.Forms.TextBox txtMaridaje;
        private System.Windows.Forms.Label lblAniada;
        private System.Windows.Forms.NumericUpDown numAniada;
        private System.Windows.Forms.Label lblPrecio;
        private System.Windows.Forms.NumericUpDown numPrecio;
        private System.Windows.Forms.Label lblStockMinimo;
        private System.Windows.Forms.NumericUpDown numStockMinimo;
        private System.Windows.Forms.CheckBox chkPuntaje;
        private System.Windows.Forms.Label lblPuntaje;
        private System.Windows.Forms.NumericUpDown numPuntaje;
        private System.Windows.Forms.Button btnGuardarVino;
        private System.Windows.Forms.Button btnLimpiarVino;
        private System.Windows.Forms.Button btnCerrar;
    }
}
