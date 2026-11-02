namespace CAPAS
{
    partial class frmAyuda
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
            this.rtbContenido = new System.Windows.Forms.RichTextBox();
            this.panelIzquierdo = new System.Windows.Forms.Panel();
            this.treeTemas = new System.Windows.Forms.TreeView();
            this.panelBuscar = new System.Windows.Forms.Panel();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblBuscarAyuda = new System.Windows.Forms.Label();
            this.panelInferior = new System.Windows.Forms.Panel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.panelIzquierdo.SuspendLayout();
            this.panelBuscar.SuspendLayout();
            this.panelInferior.SuspendLayout();
            this.SuspendLayout();
            //
            // rtbContenido
            //
            this.rtbContenido.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbContenido.Location = new System.Drawing.Point(303, 64);
            this.rtbContenido.Name = "rtbContenido";
            this.rtbContenido.ReadOnly = true;
            this.rtbContenido.Size = new System.Drawing.Size(605, 418);
            this.rtbContenido.TabIndex = 1;
            this.rtbContenido.TabStop = false;
            this.rtbContenido.Text = "";
            //
            // panelIzquierdo
            //
            this.panelIzquierdo.Controls.Add(this.treeTemas);
            this.panelIzquierdo.Controls.Add(this.panelBuscar);
            this.panelIzquierdo.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelIzquierdo.Location = new System.Drawing.Point(3, 64);
            this.panelIzquierdo.Name = "panelIzquierdo";
            this.panelIzquierdo.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.panelIzquierdo.Size = new System.Drawing.Size(300, 418);
            this.panelIzquierdo.TabIndex = 0;
            //
            // treeTemas
            //
            this.treeTemas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeTemas.HideSelection = false;
            this.treeTemas.Location = new System.Drawing.Point(0, 70);
            this.treeTemas.Name = "treeTemas";
            this.treeTemas.ShowNodeToolTips = true;
            this.treeTemas.Size = new System.Drawing.Size(292, 348);
            this.treeTemas.TabIndex = 1;
            this.treeTemas.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeTemas_AfterSelect);
            //
            // panelBuscar
            //
            this.panelBuscar.Controls.Add(this.txtBuscar);
            this.panelBuscar.Controls.Add(this.lblBuscarAyuda);
            this.panelBuscar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBuscar.Location = new System.Drawing.Point(0, 0);
            this.panelBuscar.Name = "panelBuscar";
            this.panelBuscar.Size = new System.Drawing.Size(292, 70);
            this.panelBuscar.TabIndex = 0;
            //
            // txtBuscar
            //
            this.txtBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBuscar.Location = new System.Drawing.Point(0, 32);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(292, 26);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            //
            // lblBuscarAyuda
            //
            this.lblBuscarAyuda.AutoSize = true;
            this.lblBuscarAyuda.Location = new System.Drawing.Point(0, 8);
            this.lblBuscarAyuda.Name = "lblBuscarAyuda";
            this.lblBuscarAyuda.Size = new System.Drawing.Size(60, 20);
            this.lblBuscarAyuda.TabIndex = 0;
            this.lblBuscarAyuda.Text = "Buscar:";
            //
            // panelInferior
            //
            this.panelInferior.Controls.Add(this.btnCerrar);
            this.panelInferior.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelInferior.Location = new System.Drawing.Point(3, 482);
            this.panelInferior.Name = "panelInferior";
            this.panelInferior.Size = new System.Drawing.Size(905, 66);
            this.panelInferior.TabIndex = 2;
            //
            // btnCerrar
            //
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location = new System.Drawing.Point(797, 14);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(100, 38);
            this.btnCerrar.TabIndex = 0;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmAyuda
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCerrar;
            this.ClientSize = new System.Drawing.Size(910, 550);
            this.Controls.Add(this.rtbContenido);
            this.Controls.Add(this.panelIzquierdo);
            this.Controls.Add(this.panelInferior);
            this.MinimumSize = new System.Drawing.Size(760, 480);
            this.Name = "frmAyuda";
            this.Padding = new System.Windows.Forms.Padding(3, 64, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ayuda en línea";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAyuda_FormClosed);
            this.Load += new System.EventHandler(this.frmAyuda_Load);
            this.panelIzquierdo.ResumeLayout(false);
            this.panelBuscar.ResumeLayout(false);
            this.panelBuscar.PerformLayout();
            this.panelInferior.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.RichTextBox rtbContenido;
        private System.Windows.Forms.Panel panelIzquierdo;
        private System.Windows.Forms.TreeView treeTemas;
        private System.Windows.Forms.Panel panelBuscar;
        private System.Windows.Forms.TextBox txtBuscar;
        private System.Windows.Forms.Label lblBuscarAyuda;
        private System.Windows.Forms.Panel panelInferior;
        private System.Windows.Forms.Button btnCerrar;
    }
}
