namespace CAPAS
{
    partial class frmAnalisisRotacion
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
            this.lblDesde = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.lblHasta = new System.Windows.Forms.Label();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.btnAnalizar = new System.Windows.Forms.Button();
            this.lblAdvertenciaFecha = new System.Windows.Forms.Label();
            this.grpConfiguracion = new System.Windows.Forms.GroupBox();
            this.tlpConfiguracion = new System.Windows.Forms.TableLayoutPanel();
            this.lblDiasAnalisis = new System.Windows.Forms.Label();
            this.nudDiasAnalisis = new System.Windows.Forms.NumericUpDown();
            this.lblUmbralAlta = new System.Windows.Forms.Label();
            this.nudUmbralAlta = new System.Windows.Forms.NumericUpDown();
            this.lblUmbralBaja = new System.Windows.Forms.Label();
            this.nudUmbralBaja = new System.Windows.Forms.NumericUpDown();
            this.lblCoberturaUrgente = new System.Windows.Forms.Label();
            this.nudCoberturaUrgente = new System.Windows.Forms.NumericUpDown();
            this.lblCoberturaReposicion = new System.Windows.Forms.Label();
            this.nudCoberturaReposicion = new System.Windows.Forms.NumericUpDown();
            this.lblCoberturaSobrestock = new System.Windows.Forms.Label();
            this.nudCoberturaSobrestock = new System.Windows.Forms.NumericUpDown();
            this.lblCoberturaObjetivo = new System.Windows.Forms.Label();
            this.nudCoberturaObjetivo = new System.Windows.Forms.NumericUpDown();
            this.btnGuardarConfiguracion = new System.Windows.Forms.Button();
            this.lblResumen = new System.Windows.Forms.Label();
            this.dgvRotacion = new System.Windows.Forms.DataGridView();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.btnExportarPdf = new System.Windows.Forms.Button();
            this.btnGenerarPedido = new System.Windows.Forms.Button();
            this.lblSeleccionPedido = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.grpConfiguracion.SuspendLayout();
            this.tlpConfiguracion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiasAnalisis)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudUmbralAlta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudUmbralBaja)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaUrgente)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaReposicion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaSobrestock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaObjetivo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRotacion)).BeginInit();
            this.panelInferior.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(5, 84);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(420, 18);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Análisis de rotación y reposición de stock";
            //
            // lblDesde
            //
            this.lblDesde.AutoSize = true;
            this.lblDesde.Location = new System.Drawing.Point(5, 127);
            this.lblDesde.Name = "lblDesde";
            this.lblDesde.Size = new System.Drawing.Size(63, 20);
            this.lblDesde.TabIndex = 1;
            this.lblDesde.Text = "Desde:";
            //
            // dtpDesde
            //
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(80, 122);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(150, 26);
            this.dtpDesde.TabIndex = 2;
            //
            // lblHasta
            //
            this.lblHasta.AutoSize = true;
            this.lblHasta.Location = new System.Drawing.Point(260, 127);
            this.lblHasta.Name = "lblHasta";
            this.lblHasta.Size = new System.Drawing.Size(60, 20);
            this.lblHasta.TabIndex = 3;
            this.lblHasta.Text = "Hasta:";
            //
            // dtpHasta
            //
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(335, 122);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(150, 26);
            this.dtpHasta.TabIndex = 4;
            //
            // btnAnalizar
            //
            this.btnAnalizar.Location = new System.Drawing.Point(520, 115);
            this.btnAnalizar.Name = "btnAnalizar";
            this.btnAnalizar.Size = new System.Drawing.Size(130, 38);
            this.btnAnalizar.TabIndex = 5;
            this.btnAnalizar.Text = "Analizar";
            this.btnAnalizar.UseVisualStyleBackColor = true;
            this.btnAnalizar.Click += new System.EventHandler(this.btnAnalizar_Click);
            //
            // lblAdvertenciaFecha
            //
            this.lblAdvertenciaFecha.AutoSize = true;
            this.lblAdvertenciaFecha.Location = new System.Drawing.Point(5, 160);
            this.lblAdvertenciaFecha.MaximumSize = new System.Drawing.Size(1110, 0);
            this.lblAdvertenciaFecha.Name = "lblAdvertenciaFecha";
            this.lblAdvertenciaFecha.Size = new System.Drawing.Size(0, 20);
            this.lblAdvertenciaFecha.TabIndex = 13;
            this.lblAdvertenciaFecha.Visible = false;
            //
            // grpConfiguracion
            //
            this.grpConfiguracion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfiguracion.Controls.Add(this.tlpConfiguracion);
            this.grpConfiguracion.Location = new System.Drawing.Point(5, 200);
            this.grpConfiguracion.Name = "grpConfiguracion";
            this.grpConfiguracion.Size = new System.Drawing.Size(1115, 190);
            this.grpConfiguracion.TabIndex = 6;
            this.grpConfiguracion.TabStop = false;
            this.grpConfiguracion.Text = "Configuración del análisis";
            //
            // tlpConfiguracion
            //
            this.tlpConfiguracion.AutoSize = true;
            this.tlpConfiguracion.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpConfiguracion.ColumnCount = 4;
            this.tlpConfiguracion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpConfiguracion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpConfiguracion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpConfiguracion.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpConfiguracion.Controls.Add(this.lblDiasAnalisis, 0, 0);
            this.tlpConfiguracion.Controls.Add(this.nudDiasAnalisis, 1, 0);
            this.tlpConfiguracion.Controls.Add(this.lblUmbralAlta, 2, 0);
            this.tlpConfiguracion.Controls.Add(this.nudUmbralAlta, 3, 0);
            this.tlpConfiguracion.Controls.Add(this.lblUmbralBaja, 0, 1);
            this.tlpConfiguracion.Controls.Add(this.nudUmbralBaja, 1, 1);
            this.tlpConfiguracion.Controls.Add(this.lblCoberturaUrgente, 2, 1);
            this.tlpConfiguracion.Controls.Add(this.nudCoberturaUrgente, 3, 1);
            this.tlpConfiguracion.Controls.Add(this.lblCoberturaReposicion, 0, 2);
            this.tlpConfiguracion.Controls.Add(this.nudCoberturaReposicion, 1, 2);
            this.tlpConfiguracion.Controls.Add(this.lblCoberturaSobrestock, 2, 2);
            this.tlpConfiguracion.Controls.Add(this.nudCoberturaSobrestock, 3, 2);
            this.tlpConfiguracion.Controls.Add(this.lblCoberturaObjetivo, 0, 3);
            this.tlpConfiguracion.Controls.Add(this.nudCoberturaObjetivo, 1, 3);
            this.tlpConfiguracion.Controls.Add(this.btnGuardarConfiguracion, 2, 3);
            this.tlpConfiguracion.Location = new System.Drawing.Point(10, 30);
            this.tlpConfiguracion.Name = "tlpConfiguracion";
            this.tlpConfiguracion.RowCount = 4;
            this.tlpConfiguracion.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpConfiguracion.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpConfiguracion.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpConfiguracion.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpConfiguracion.Size = new System.Drawing.Size(600, 154);
            this.tlpConfiguracion.TabIndex = 0;
            //
            // lblDiasAnalisis
            //
            this.lblDiasAnalisis.AutoSize = true;
            this.lblDiasAnalisis.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDiasAnalisis.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblDiasAnalisis.Name = "lblDiasAnalisis";
            this.lblDiasAnalisis.Size = new System.Drawing.Size(150, 20);
            this.lblDiasAnalisis.TabIndex = 0;
            this.lblDiasAnalisis.Text = "Días de análisis:";
            //
            // nudDiasAnalisis
            //
            this.nudDiasAnalisis.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudDiasAnalisis.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
            this.nudDiasAnalisis.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudDiasAnalisis.Name = "nudDiasAnalisis";
            this.nudDiasAnalisis.Size = new System.Drawing.Size(80, 26);
            this.nudDiasAnalisis.TabIndex = 1;
            this.nudDiasAnalisis.Value = new decimal(new int[] { 90, 0, 0, 0 });
            //
            // lblUmbralAlta
            //
            this.lblUmbralAlta.AutoSize = true;
            this.lblUmbralAlta.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUmbralAlta.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblUmbralAlta.Name = "lblUmbralAlta";
            this.lblUmbralAlta.Size = new System.Drawing.Size(200, 20);
            this.lblUmbralAlta.TabIndex = 2;
            this.lblUmbralAlta.Text = "Alta rotación desde (u/mes):";
            //
            // nudUmbralAlta
            //
            this.nudUmbralAlta.DecimalPlaces = 1;
            this.nudUmbralAlta.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudUmbralAlta.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudUmbralAlta.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudUmbralAlta.Name = "nudUmbralAlta";
            this.nudUmbralAlta.Size = new System.Drawing.Size(80, 26);
            this.nudUmbralAlta.TabIndex = 3;
            this.nudUmbralAlta.Value = new decimal(new int[] { 40, 0, 0, 65536 });
            //
            // lblUmbralBaja
            //
            this.lblUmbralBaja.AutoSize = true;
            this.lblUmbralBaja.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUmbralBaja.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblUmbralBaja.Name = "lblUmbralBaja";
            this.lblUmbralBaja.Size = new System.Drawing.Size(200, 20);
            this.lblUmbralBaja.TabIndex = 4;
            this.lblUmbralBaja.Text = "Baja rotación bajo (u/mes):";
            //
            // nudUmbralBaja
            //
            this.nudUmbralBaja.DecimalPlaces = 1;
            this.nudUmbralBaja.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudUmbralBaja.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudUmbralBaja.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudUmbralBaja.Name = "nudUmbralBaja";
            this.nudUmbralBaja.Size = new System.Drawing.Size(80, 26);
            this.nudUmbralBaja.TabIndex = 5;
            this.nudUmbralBaja.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            //
            // lblCoberturaUrgente
            //
            this.lblCoberturaUrgente.AutoSize = true;
            this.lblCoberturaUrgente.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCoberturaUrgente.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblCoberturaUrgente.Name = "lblCoberturaUrgente";
            this.lblCoberturaUrgente.Size = new System.Drawing.Size(250, 20);
            this.lblCoberturaUrgente.TabIndex = 6;
            this.lblCoberturaUrgente.Text = "Reposición urgente bajo (meses):";
            //
            // nudCoberturaUrgente
            //
            this.nudCoberturaUrgente.DecimalPlaces = 1;
            this.nudCoberturaUrgente.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudCoberturaUrgente.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCoberturaUrgente.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.nudCoberturaUrgente.Name = "nudCoberturaUrgente";
            this.nudCoberturaUrgente.Size = new System.Drawing.Size(80, 26);
            this.nudCoberturaUrgente.TabIndex = 7;
            this.nudCoberturaUrgente.Value = new decimal(new int[] { 10, 0, 0, 65536 });
            //
            // lblCoberturaReposicion
            //
            this.lblCoberturaReposicion.AutoSize = true;
            this.lblCoberturaReposicion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCoberturaReposicion.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblCoberturaReposicion.Name = "lblCoberturaReposicion";
            this.lblCoberturaReposicion.Size = new System.Drawing.Size(250, 20);
            this.lblCoberturaReposicion.TabIndex = 8;
            this.lblCoberturaReposicion.Text = "Reponer bajo (meses):";
            //
            // nudCoberturaReposicion
            //
            this.nudCoberturaReposicion.DecimalPlaces = 1;
            this.nudCoberturaReposicion.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudCoberturaReposicion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCoberturaReposicion.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.nudCoberturaReposicion.Name = "nudCoberturaReposicion";
            this.nudCoberturaReposicion.Size = new System.Drawing.Size(80, 26);
            this.nudCoberturaReposicion.TabIndex = 9;
            this.nudCoberturaReposicion.Value = new decimal(new int[] { 20, 0, 0, 65536 });
            //
            // lblCoberturaSobrestock
            //
            this.lblCoberturaSobrestock.AutoSize = true;
            this.lblCoberturaSobrestock.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCoberturaSobrestock.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblCoberturaSobrestock.Name = "lblCoberturaSobrestock";
            this.lblCoberturaSobrestock.Size = new System.Drawing.Size(250, 20);
            this.lblCoberturaSobrestock.TabIndex = 10;
            this.lblCoberturaSobrestock.Text = "Sobrestock sobre (meses):";
            //
            // nudCoberturaSobrestock
            //
            this.nudCoberturaSobrestock.DecimalPlaces = 1;
            this.nudCoberturaSobrestock.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudCoberturaSobrestock.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCoberturaSobrestock.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.nudCoberturaSobrestock.Name = "nudCoberturaSobrestock";
            this.nudCoberturaSobrestock.Size = new System.Drawing.Size(80, 26);
            this.nudCoberturaSobrestock.TabIndex = 11;
            this.nudCoberturaSobrestock.Value = new decimal(new int[] { 60, 0, 0, 65536 });
            //
            // lblCoberturaObjetivo
            //
            this.lblCoberturaObjetivo.AutoSize = true;
            this.lblCoberturaObjetivo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblCoberturaObjetivo.Margin = new System.Windows.Forms.Padding(3, 3, 24, 3);
            this.lblCoberturaObjetivo.Name = "lblCoberturaObjetivo";
            this.lblCoberturaObjetivo.Size = new System.Drawing.Size(250, 20);
            this.lblCoberturaObjetivo.TabIndex = 12;
            this.lblCoberturaObjetivo.Text = "Cobertura objetivo (meses):";
            //
            // nudCoberturaObjetivo
            //
            this.nudCoberturaObjetivo.DecimalPlaces = 1;
            this.nudCoberturaObjetivo.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudCoberturaObjetivo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudCoberturaObjetivo.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.nudCoberturaObjetivo.Name = "nudCoberturaObjetivo";
            this.nudCoberturaObjetivo.Size = new System.Drawing.Size(80, 26);
            this.nudCoberturaObjetivo.TabIndex = 13;
            this.nudCoberturaObjetivo.Value = new decimal(new int[] { 30, 0, 0, 65536 });
            //
            // btnGuardarConfiguracion
            //
            this.btnGuardarConfiguracion.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tlpConfiguracion.SetColumnSpan(this.btnGuardarConfiguracion, 2);
            this.btnGuardarConfiguracion.Name = "btnGuardarConfiguracion";
            this.btnGuardarConfiguracion.Size = new System.Drawing.Size(240, 38);
            this.btnGuardarConfiguracion.TabIndex = 14;
            this.btnGuardarConfiguracion.Text = "Guardar configuración";
            this.btnGuardarConfiguracion.UseVisualStyleBackColor = true;
            this.btnGuardarConfiguracion.Click += new System.EventHandler(this.btnGuardarConfiguracion_Click);
            //
            // lblResumen
            //
            this.lblResumen.AutoSize = true;
            this.lblResumen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblResumen.Location = new System.Drawing.Point(5, 396);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Size = new System.Drawing.Size(10, 20);
            this.lblResumen.TabIndex = 7;
            //
            // dgvRotacion
            //
            this.dgvRotacion.AllowUserToAddRows = false;
            this.dgvRotacion.AllowUserToDeleteRows = false;
            this.dgvRotacion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRotacion.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRotacion.Location = new System.Drawing.Point(5, 428);
            this.dgvRotacion.MultiSelect = false;
            this.dgvRotacion.Name = "dgvRotacion";
            this.dgvRotacion.ReadOnly = true;
            this.dgvRotacion.RowHeadersWidth = 55;
            this.dgvRotacion.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRotacion.Size = new System.Drawing.Size(1115, 222);
            this.dgvRotacion.TabIndex = 8;
            this.dgvRotacion.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvRotacion_CellFormatting);
            this.dgvRotacion.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvRotacion_CellPainting);
            this.dgvRotacion.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRotacion_CellEndEdit);
            this.dgvRotacion.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvRotacion_DataError);
            this.dgvRotacion.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvRotacion_CurrentCellDirtyStateChanged);
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.lblSeleccionPedido);
            this.panelInferior.Controls.Add(this.btnGenerarPedido);
            this.panelInferior.Controls.Add(this.btnExportarPdf);
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(0, 660);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(1130, 70);
            this.panelInferior.TabIndex = 9;
            //
            // lblSeleccionPedido (texto dinámico: se arma en código con ped_Seleccionados)
            //
            this.lblSeleccionPedido.AutoSize = true;
            this.lblSeleccionPedido.Location = new System.Drawing.Point(5, 24);
            this.lblSeleccionPedido.Name = "lblSeleccionPedido";
            this.lblSeleccionPedido.Size = new System.Drawing.Size(0, 20);
            this.lblSeleccionPedido.TabIndex = 2;
            //
            // btnGenerarPedido
            //
            this.btnGenerarPedido.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerarPedido.Enabled = false;
            this.btnGenerarPedido.Location = new System.Drawing.Point(640, 16);
            this.btnGenerarPedido.Name = "btnGenerarPedido";
            this.btnGenerarPedido.Size = new System.Drawing.Size(190, 38);
            this.btnGenerarPedido.TabIndex = 3;
            this.btnGenerarPedido.Text = "Generar pedido";
            this.btnGenerarPedido.UseVisualStyleBackColor = true;
            this.btnGenerarPedido.Click += new System.EventHandler(this.btnGenerarPedido_Click);
            //
            // btnExportarPdf
            //
            this.btnExportarPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportarPdf.Enabled = false;
            this.btnExportarPdf.Location = new System.Drawing.Point(840, 16);
            this.btnExportarPdf.Name = "btnExportarPdf";
            this.btnExportarPdf.Size = new System.Drawing.Size(170, 38);
            this.btnExportarPdf.TabIndex = 0;
            this.btnExportarPdf.Text = "Exportar a PDF";
            this.btnExportarPdf.UseVisualStyleBackColor = true;
            this.btnExportarPdf.Click += new System.EventHandler(this.btnExportarPdf_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(1020, 16);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAnalisisRotacion
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1130, 730);
            this.Controls.Add(this.panelInferior);
            this.Controls.Add(this.dgvRotacion);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.grpConfiguracion);
            this.Controls.Add(this.lblAdvertenciaFecha);
            this.Controls.Add(this.btnAnalizar);
            this.Controls.Add(this.dtpHasta);
            this.Controls.Add(this.lblHasta);
            this.Controls.Add(this.dtpDesde);
            this.Controls.Add(this.lblDesde);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(1000, 640);
            this.Name = "frmAnalisisRotacion";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Análisis de rotación y reposición de stock";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAnalisisRotacion_FormClosed);
            this.Load += new System.EventHandler(this.frmAnalisisRotacion_Load);
            this.grpConfiguracion.ResumeLayout(false);
            this.grpConfiguracion.PerformLayout();
            this.tlpConfiguracion.ResumeLayout(false);
            this.tlpConfiguracion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDiasAnalisis)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudUmbralAlta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudUmbralBaja)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaUrgente)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaReposicion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaSobrestock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCoberturaObjetivo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRotacion)).EndInit();
            this.panelInferior.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDesde;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblHasta;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Button btnAnalizar;
        private System.Windows.Forms.Label lblAdvertenciaFecha;
        private System.Windows.Forms.GroupBox grpConfiguracion;
        private System.Windows.Forms.TableLayoutPanel tlpConfiguracion;
        private System.Windows.Forms.Label lblDiasAnalisis;
        private System.Windows.Forms.NumericUpDown nudDiasAnalisis;
        private System.Windows.Forms.Label lblUmbralAlta;
        private System.Windows.Forms.NumericUpDown nudUmbralAlta;
        private System.Windows.Forms.Label lblUmbralBaja;
        private System.Windows.Forms.NumericUpDown nudUmbralBaja;
        private System.Windows.Forms.Label lblCoberturaUrgente;
        private System.Windows.Forms.NumericUpDown nudCoberturaUrgente;
        private System.Windows.Forms.Label lblCoberturaReposicion;
        private System.Windows.Forms.NumericUpDown nudCoberturaReposicion;
        private System.Windows.Forms.Label lblCoberturaSobrestock;
        private System.Windows.Forms.NumericUpDown nudCoberturaSobrestock;
        private System.Windows.Forms.Label lblCoberturaObjetivo;
        private System.Windows.Forms.NumericUpDown nudCoberturaObjetivo;
        private System.Windows.Forms.Button btnGuardarConfiguracion;
        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.DataGridView dgvRotacion;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Button btnExportarPdf;
        private System.Windows.Forms.Button btnGenerarPedido;
        private System.Windows.Forms.Label lblSeleccionPedido;
        private System.Windows.Forms.Button btnCerrar;
    }
}
