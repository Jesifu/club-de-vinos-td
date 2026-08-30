namespace CAPAS
{
    partial class frmPerfiles
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
            this.treePermisos = new System.Windows.Forms.TreeView();
            this.panelDerecho = new System.Windows.Forms.Panel();
            this.lblSeleccionado = new System.Windows.Forms.Label();
            this.btnAgregarRol = new System.Windows.Forms.Button();
            this.panelPadre = new System.Windows.Forms.Panel();
            this.lblPadre = new System.Windows.Forms.Label();
            this.cboPadre = new System.Windows.Forms.ComboBox();
            this.btnAsignarPadre = new System.Windows.Forms.Button();
            this.panelPermisos = new System.Windows.Forms.Panel();
            this.lblPermisos = new System.Windows.Forms.Label();
            this.chkPermisos = new System.Windows.Forms.CheckedListBox();
            this.btnGuardarPermisos = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.panelDerecho.SuspendLayout();
            this.panelPadre.SuspendLayout();
            this.panelPermisos.SuspendLayout();
            this.SuspendLayout();
            // 
            // treePermisos
            // 
            this.treePermisos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treePermisos.Location = new System.Drawing.Point(6, 105);
            this.treePermisos.Name = "treePermisos";
            this.treePermisos.Size = new System.Drawing.Size(360, 462);
            this.treePermisos.TabIndex = 0;
            this.treePermisos.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treePermisos_AfterSelect);
            // 
            // panelDerecho
            // 
            this.panelDerecho.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDerecho.Controls.Add(this.lblSeleccionado);
            this.panelDerecho.Controls.Add(this.btnAgregarRol);
            this.panelDerecho.Controls.Add(this.panelPadre);
            this.panelDerecho.Controls.Add(this.panelPermisos);
            this.panelDerecho.Controls.Add(this.btnEliminar);
            this.panelDerecho.Controls.Add(this.btnCerrar);
            this.panelDerecho.Location = new System.Drawing.Point(390, 105);
            this.panelDerecho.Name = "panelDerecho";
            this.panelDerecho.Size = new System.Drawing.Size(257, 635);
            this.panelDerecho.TabIndex = 1;
            // 
            // lblSeleccionado
            // 
            this.lblSeleccionado.Location = new System.Drawing.Point(4, 5);
            this.lblSeleccionado.Name = "lblSeleccionado";
            this.lblSeleccionado.Size = new System.Drawing.Size(220, 40);
            this.lblSeleccionado.TabIndex = 0;
            this.lblSeleccionado.Text = "Seleccioná un nodo:";
            this.lblSeleccionado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnAgregarRol
            // 
            this.btnAgregarRol.Location = new System.Drawing.Point(46, 44);
            this.btnAgregarRol.Name = "btnAgregarRol";
            this.btnAgregarRol.Size = new System.Drawing.Size(170, 47);
            this.btnAgregarRol.TabIndex = 1;
            this.btnAgregarRol.Text = "Agregar Rol";
            this.btnAgregarRol.Click += new System.EventHandler(this.btnAgregarRol_Click);
            // 
            // panelPadre
            // 
            this.panelPadre.Controls.Add(this.lblPadre);
            this.panelPadre.Controls.Add(this.cboPadre);
            this.panelPadre.Controls.Add(this.btnAsignarPadre);
            this.panelPadre.Location = new System.Drawing.Point(3, 97);
            this.panelPadre.Name = "panelPadre";
            this.panelPadre.Size = new System.Drawing.Size(251, 154);
            this.panelPadre.TabIndex = 2;
            this.panelPadre.Visible = false;
            //
            // lblPadre
            // 
            this.lblPadre.Location = new System.Drawing.Point(3, 16);
            this.lblPadre.Name = "lblPadre";
            this.lblPadre.Size = new System.Drawing.Size(210, 29);
            this.lblPadre.TabIndex = 0;
            this.lblPadre.Text = "Rol padre:";
            // 
            // cboPadre
            // 
            this.cboPadre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPadre.Location = new System.Drawing.Point(6, 56);
            this.cboPadre.Name = "cboPadre";
            this.cboPadre.Size = new System.Drawing.Size(210, 28);
            this.cboPadre.TabIndex = 1;
            // 
            // btnAsignarPadre
            // 
            this.btnAsignarPadre.Location = new System.Drawing.Point(42, 97);
            this.btnAsignarPadre.Name = "btnAsignarPadre";
            this.btnAsignarPadre.Size = new System.Drawing.Size(170, 40);
            this.btnAsignarPadre.TabIndex = 2;
            this.btnAsignarPadre.Text = "Asignar padre";
            this.btnAsignarPadre.Click += new System.EventHandler(this.btnAsignarPadre_Click);
            // 
            // panelPermisos
            // 
            this.panelPermisos.Controls.Add(this.lblPermisos);
            this.panelPermisos.Controls.Add(this.chkPermisos);
            this.panelPermisos.Controls.Add(this.btnGuardarPermisos);
            this.panelPermisos.Location = new System.Drawing.Point(0, 257);
            this.panelPermisos.Name = "panelPermisos";
            this.panelPermisos.Size = new System.Drawing.Size(254, 260);
            this.panelPermisos.TabIndex = 3;
            this.panelPermisos.Visible = false;
            // 
            // lblPermisos
            // 
            this.lblPermisos.Location = new System.Drawing.Point(2, 14);
            this.lblPermisos.Name = "lblPermisos";
            this.lblPermisos.Size = new System.Drawing.Size(236, 26);
            this.lblPermisos.TabIndex = 0;
            this.lblPermisos.Text = "Permisos del rol:";
            // 
            // chkPermisos
            // 
            this.chkPermisos.CheckOnClick = true;
            this.chkPermisos.FormattingEnabled = true;
            this.chkPermisos.Location = new System.Drawing.Point(6, 43);
            this.chkPermisos.Name = "chkPermisos";
            this.chkPermisos.Size = new System.Drawing.Size(245, 142);
            this.chkPermisos.TabIndex = 1;
            // 
            // btnGuardarPermisos
            // 
            this.btnGuardarPermisos.Location = new System.Drawing.Point(28, 201);
            this.btnGuardarPermisos.Name = "btnGuardarPermisos";
            this.btnGuardarPermisos.Size = new System.Drawing.Size(170, 51);
            this.btnGuardarPermisos.TabIndex = 2;
            this.btnGuardarPermisos.Text = "Guardar permisos";
            this.btnGuardarPermisos.Click += new System.EventHandler(this.btnGuardarPermisos_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.Location = new System.Drawing.Point(29, 534);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(170, 42);
            this.btnEliminar.TabIndex = 4;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Location = new System.Drawing.Point(29, 582);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(170, 44);
            this.btnCerrar.TabIndex = 5;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // frmPerfiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(697, 823);
            this.MinimumSize = new System.Drawing.Size(560, 500);
            this.Controls.Add(this.treePermisos);
            this.Controls.Add(this.panelDerecho);
            this.Name = "frmPerfiles";
            this.Text = "Gestión de Roles y Permisos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmPerfiles_FormClosed);
            this.Load += new System.EventHandler(this.frmPerfiles_Load);
            this.panelDerecho.ResumeLayout(false);
            this.panelPadre.ResumeLayout(false);
            this.panelPermisos.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TreeView treePermisos;
        private System.Windows.Forms.Panel panelDerecho;
        private System.Windows.Forms.Label lblSeleccionado;
        private System.Windows.Forms.Button btnAgregarRol;
        private System.Windows.Forms.Panel panelPadre;
        private System.Windows.Forms.Label lblPadre;
        private System.Windows.Forms.ComboBox cboPadre;
        private System.Windows.Forms.Button btnAsignarPadre;
        private System.Windows.Forms.Panel panelPermisos;
        private System.Windows.Forms.Label lblPermisos;
        private System.Windows.Forms.CheckedListBox chkPermisos;
        private System.Windows.Forms.Button btnGuardarPermisos;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnCerrar;
    }
}
