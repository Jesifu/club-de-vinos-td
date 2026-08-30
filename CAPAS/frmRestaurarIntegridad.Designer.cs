namespace CAPAS
{
    partial class frmRestaurarIntegridad
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
            this.txtErrores = new System.Windows.Forms.TextBox();
            this.btnRecalcular = new System.Windows.Forms.Button();
            this.btnRestaurar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblTitulo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTitulo.Location = new System.Drawing.Point(12, 76);
            this.lblTitulo.Size = new System.Drawing.Size(460, 40);
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Text = "Se detectaron problemas de integridad en la base de datos:";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtErrores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtErrores.Location = new System.Drawing.Point(12, 122);
            this.txtErrores.Size = new System.Drawing.Size(460, 150);
            this.txtErrores.Multiline = true;
            this.txtErrores.ReadOnly = true;
            this.txtErrores.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtErrores.BackColor = System.Drawing.SystemColors.Window;
            this.txtErrores.Font = new System.Drawing.Font("Consolas", 9F);

            this.btnRecalcular.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRecalcular.Location = new System.Drawing.Point(12, 292);
            this.btnRecalcular.Size = new System.Drawing.Size(175, 35);
            this.btnRecalcular.Text = "Recalcular y continuar";
            this.btnRecalcular.Click += new System.EventHandler(this.btnRecalcular_Click);

            this.btnRestaurar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRestaurar.Location = new System.Drawing.Point(197, 292);
            this.btnRestaurar.Size = new System.Drawing.Size(185, 35);
            this.btnRestaurar.Text = "Restaurar desde historial";
            this.btnRestaurar.Click += new System.EventHandler(this.btnRestaurar_Click);

            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location = new System.Drawing.Point(392, 292);
            this.btnCancelar.Size = new System.Drawing.Size(80, 35);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 342);
            this.MinimumSize = new System.Drawing.Size(420, 300);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.txtErrores);
            this.Controls.Add(this.btnRecalcular);
            this.Controls.Add(this.btnRestaurar);
            this.Controls.Add(this.btnCancelar);
            this.Name = "frmRestaurarIntegridad";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error de integridad";
            this.Load += new System.EventHandler(this.frmRestaurarIntegridad_Load);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.TextBox txtErrores;
        private System.Windows.Forms.Button btnRecalcular;
        private System.Windows.Forms.Button btnRestaurar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
