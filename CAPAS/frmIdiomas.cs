using BE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmIdiomas : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.IdiomaBLL _bll = new BLL.IdiomaBLL();
        private IDIOMA _idiomaSeleccionado;
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmIdiomas()
        {
            InitializeComponent();
        }

        private void frmIdiomas_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            CargarIdiomas();
            ActualizarIdioma();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmIdiomas_FormClosed(object sender, FormClosedEventArgs e)
        {
            SeguridadYServicios.IdiomaManager.getInstance().Desregistrar(this);
        }

        public void ActualizarIdioma()
        {
            foreach (var kvp in _controles)
            {
                string t = SeguridadYServicios.IdiomaManager.getInstance().Traducir(kvp.Key)
                           ?? _defaults[kvp.Key];
                kvp.Value.Text = t;
            }
            ActualizarEncabezados();
        }

        private void GuardarDefaults(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                if (!string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Text))
                {
                    _controles[c.Name] = c;
                    _defaults[c.Name] = c.Text;
                }
                if (c.HasChildren) GuardarDefaults(c.Controls);
            }
        }

        private void ActualizarEncabezados()
        {
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvIdiomas.Columns.Count > 0)
            {
                if (dgvIdiomas.Columns["Nombre"] != null)
                    dgvIdiomas.Columns["Nombre"].HeaderText =
                        mgr.Traducir("colhdr_Nombre") ?? "Idioma";
                if (dgvIdiomas.Columns["Habilitado"] != null)
                    dgvIdiomas.Columns["Habilitado"].HeaderText =
                        mgr.Traducir("colhdr_Habilitado") ?? "Habilitado";
            }
            if (dgvTraducciones.Columns.Count > 0)
            {
                if (dgvTraducciones.Columns["Clave"] != null)
                    dgvTraducciones.Columns["Clave"].HeaderText =
                        mgr.Traducir("colhdr_Clave") ?? "Clave";
                if (dgvTraducciones.Columns["TextoDefault"] != null)
                    dgvTraducciones.Columns["TextoDefault"].HeaderText =
                        mgr.Traducir("colhdr_TextoDefault") ?? "Texto por defecto";
                if (dgvTraducciones.Columns["TextoTraduccion"] != null)
                    dgvTraducciones.Columns["TextoTraduccion"].HeaderText =
                        mgr.Traducir("colhdr_TextoTraduccion") ?? "Traducción";
            }
        }

        private void CargarIdiomas()
        {
            var idiomas = _bll.ListarTodos();
            dgvIdiomas.DataSource = null;
            dgvIdiomas.DataSource = idiomas;

            if (dgvIdiomas.Columns.Count > 0)
            {
                dgvIdiomas.Columns["Id"].Visible = false;
                dgvIdiomas.Columns["Nombre"].HeaderText = "Idioma";
                dgvIdiomas.Columns["Habilitado"].HeaderText = "Habilitado";
            }
        }

        private void dgvIdiomas_SelectionChanged(object sender, System.EventArgs e)
        {
            _idiomaSeleccionado = dgvIdiomas.CurrentRow?.DataBoundItem as IDIOMA;
            if (_idiomaSeleccionado != null)
                CargarTraducciones();
        }

        private void CargarTraducciones()
        {
            List<CONTROL_IDIOMA> controles = _bll.ListarControlesConTraduccion(_idiomaSeleccionado.Id);
            dgvTraducciones.DataSource = null;
            dgvTraducciones.DataSource = controles;

            if (dgvTraducciones.Columns.Count > 0)
            {
                dgvTraducciones.Columns["Id"].Visible = false;
                dgvTraducciones.Columns["Clave"].HeaderText = "Clave";
                dgvTraducciones.Columns["Clave"].ReadOnly = true;
                dgvTraducciones.Columns["TextoDefault"].HeaderText = "Texto por defecto";
                dgvTraducciones.Columns["TextoDefault"].ReadOnly = true;
                dgvTraducciones.Columns["TextoTraduccion"].HeaderText = "Traducción";
                dgvTraducciones.Columns["TextoTraduccion"].ReadOnly = false;
            }
        }

        private void btnAgregarIdioma_Click(object sender, System.EventArgs e)
        {
            string nombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Nombre del nuevo idioma:", "Agregar idioma");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            _bll.Crear(nombre, false);
            CargarIdiomas();
        }

        private void btnEliminarIdioma_Click(object sender, System.EventArgs e)
        {
            if (_idiomaSeleccionado == null)
            {
                MsgBox.Show("Seleccioná un idioma.", "Aviso",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (_idiomaSeleccionado.Predeterminado)
            {
                MsgBox.Show("No se puede eliminar el idioma predeterminado del sistema.", "Aviso",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (_bll.EstaEnUso(_idiomaSeleccionado.Id))
            {
                MsgBox.Show("No se puede eliminar un idioma que está en uso por algún usuario.", "Aviso",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show(
                    $"¿Eliminar el idioma '{_idiomaSeleccionado.Nombre}'? Se borrarán todas sus traducciones.",
                    "Confirmar eliminación",
                    MsgBox.Botones.SiNo, MsgBox.Icono.Atencion) != DialogResult.Yes)
                return;

            _bll.Eliminar(_idiomaSeleccionado.Id);
            _idiomaSeleccionado = null;
            dgvTraducciones.DataSource = null;
            CargarIdiomas();
        }

        private void btnRenombrar_Click(object sender, System.EventArgs e)
        {
            if (_idiomaSeleccionado == null)
            {
                MsgBox.Show("Seleccioná un idioma.", "Aviso",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Nuevo nombre:", "Renombrar idioma", _idiomaSeleccionado.Nombre);

            if (string.IsNullOrWhiteSpace(nuevoNombre)) return;
            if (nuevoNombre == _idiomaSeleccionado.Nombre) return;

            _bll.Renombrar(_idiomaSeleccionado.Id, nuevoNombre);
            CargarIdiomas();
        }

        private void btnToggleHabilitado_Click(object sender, System.EventArgs e)
        {
            if (_idiomaSeleccionado == null)
            {
                MsgBox.Show("Seleccioná un idioma.", "Aviso",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }
            _bll.ActualizarEstado(_idiomaSeleccionado.Id, !_idiomaSeleccionado.Habilitado);
            CargarIdiomas();
        }

        private void btnGuardarTraducciones_Click(object sender, System.EventArgs e)
        {
            if (_idiomaSeleccionado == null) return;

            dgvTraducciones.CommitEdit(DataGridViewDataErrorContexts.Commit);

            foreach (DataGridViewRow fila in dgvTraducciones.Rows)
            {
                if (!(fila.DataBoundItem is CONTROL_IDIOMA control)) continue;
                if (string.IsNullOrWhiteSpace(control.TextoTraduccion)) continue;
                _bll.GuardarTraduccion(_idiomaSeleccionado.Id, control.Id, control.TextoTraduccion);
            }

            MsgBox.Show("Traducciones guardadas.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);

            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (mgr.IdiomaActivo != null && mgr.IdiomaActivo.Id == _idiomaSeleccionado.Id)
            {
                var traducciones = _bll.CargarTraducciones(_idiomaSeleccionado.Id);
                mgr.CambiarIdioma(_idiomaSeleccionado, traducciones);
            }
        }

        private void btnCerrar_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
