using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmAdminUsuarios : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private const int TAMANIO_PAGINA = 15;

        private readonly BLL.UsuarioBLL _bll = new BLL.UsuarioBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        private int _paginaActual = 1;
        private int _totalPaginas = 1;
        private string _busquedaActual = null;

        public frmAdminUsuarios()
        {
            InitializeComponent();
            this.Resize += (s, e) => ReposicionarLayout();
            this.Shown += (s, e) => ReposicionarLayout();
            panelInferior.Layout += (s, e) => ReposicionarBotonesPanel();
        }

        private void frmAdminUsuarios_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles.Remove("lblPagina");
            _defaults.Remove("lblPagina");
            _controles["lblTitulo_AdminUsuarios"] = lblTitulo;
            _defaults["lblTitulo_AdminUsuarios"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarUsuarios();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ReposicionarLayout();
        }

        private void ReposicionarLayout()
        {
            if (panelInferior == null || dgvUsuarios == null) return;

            Rectangle area = this.ClientRectangle;
            foreach (Control c in this.Controls)
            {
                if (c.Dock == DockStyle.Bottom && c.Visible)
                    area.Height -= c.Height;
            }

            int altoGrilla = area.Height - dgvUsuarios.Top - 10;
            if (altoGrilla < 80) altoGrilla = 80;
            dgvUsuarios.Height = altoGrilla;
            dgvUsuarios.Width = this.ClientSize.Width - dgvUsuarios.Left - 40;

            // Buscador: bloque compacto alineado a la derecha
            int margenDer = 40;
            btnBuscar.Left = this.ClientSize.Width - btnBuscar.Width - margenDer;
            int anchoBuscar = 230;
            txtBuscar.Width = anchoBuscar;
            txtBuscar.Left = btnBuscar.Left - anchoBuscar - 8;

            ReposicionarBotonesPanel();
        }

        private void ReposicionarBotonesPanel()
        {
            if (panelInferior == null) return;
            int w = panelInferior.ClientSize.Width;
            if (w < 50) return;
            btnPaginaSiguiente.Left = w - btnPaginaSiguiente.Width - 40;
            btnPaginaAnterior.Left = btnPaginaSiguiente.Left - btnPaginaAnterior.Width - 12;
            btnCerrar.Left = w - btnCerrar.Width - 40;
        }

        private void frmAdminUsuarios_FormClosed(object sender, FormClosedEventArgs e)
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

        private void ActualizarEncabezados()
        {
            if (dgvUsuarios.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvUsuarios.Columns["Id"] != null)
                dgvUsuarios.Columns["Id"].HeaderText = mgr.Traducir("colhdr_Id") ?? "ID";
            if (dgvUsuarios.Columns["Usuario"] != null)
                dgvUsuarios.Columns["Usuario"].HeaderText = mgr.Traducir("colhdr_Usuario") ?? "Usuario";
            if (dgvUsuarios.Columns["Nombre"] != null)
                dgvUsuarios.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombrePersona") ?? "Nombre";
            if (dgvUsuarios.Columns["Apellido"] != null)
                dgvUsuarios.Columns["Apellido"].HeaderText = mgr.Traducir("colhdr_Apellido") ?? "Apellido";
            if (dgvUsuarios.Columns["RolNombre"] != null)
                dgvUsuarios.Columns["RolNombre"].HeaderText = mgr.Traducir("colhdr_Rol") ?? "Rol";
            if (dgvUsuarios.Columns["Bloqueado"] != null)
                dgvUsuarios.Columns["Bloqueado"].HeaderText = mgr.Traducir("colhdr_Bloqueado") ?? "Bloqueado";
            if (dgvUsuarios.Columns["Telefono"] != null)
                dgvUsuarios.Columns["Telefono"].HeaderText = mgr.Traducir("colhdr_Telefono") ?? "Teléfono";
            if (dgvUsuarios.Columns["Email"] != null)
                dgvUsuarios.Columns["Email"].HeaderText = mgr.Traducir("colhdr_Email") ?? "Email";
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

        private void CargarUsuarios()
        {
            BE.PaginaResultado<BE.USUARIO> resultado = _bll.ListarPaginado(_busquedaActual, _paginaActual, TAMANIO_PAGINA);

            _totalPaginas = resultado.TotalPaginas == 0 ? 1 : resultado.TotalPaginas;
            if (_paginaActual > _totalPaginas) _paginaActual = _totalPaginas;

            dgvUsuarios.DataSource = resultado.Items;

            if (dgvUsuarios.Columns.Count > 0)
            {
                if (dgvUsuarios.Columns["Contrasena"] != null) dgvUsuarios.Columns["Contrasena"].Visible = false;
                if (dgvUsuarios.Columns["IntentosFallidos"] != null) dgvUsuarios.Columns["IntentosFallidos"].Visible = false;
                if (dgvUsuarios.Columns["IdiomaId"] != null) dgvUsuarios.Columns["IdiomaId"].Visible = false;
                if (dgvUsuarios.Columns["Rol"] != null) dgvUsuarios.Columns["Rol"].Visible = false;
                if (dgvUsuarios.Columns["RolId"] != null) dgvUsuarios.Columns["RolId"].Visible = false;
                ActualizarEncabezados();
            }

            ActualizarControlesPaginacion();
        }

        private void ActualizarControlesPaginacion()
        {
            lblPagina.Text = string.Format("Página {0} de {1}", _paginaActual, _totalPaginas);
            btnPaginaAnterior.Enabled = _paginaActual > 1;
            btnPaginaSiguiente.Enabled = _paginaActual < _totalPaginas;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            _busquedaActual = string.IsNullOrWhiteSpace(txtBuscar.Text) ? null : txtBuscar.Text.Trim();
            _paginaActual = 1;
            CargarUsuarios();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnBuscar_Click(sender, e);
            }
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (_paginaActual <= 1) return;
            _paginaActual--;
            CargarUsuarios();
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (_paginaActual >= _totalPaginas) return;
            _paginaActual++;
            CargarUsuarios();
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (frmNuevoUsuario frm = new frmNuevoUsuario())
            {
                if (frm.ShowDialog() != DialogResult.OK) return;

                bool creado = _bll.Crear(frm.NombreUsuario, frm.Contrasena, frm.RolId,
                    frm.Nombre, frm.Apellido, frm.Telefono, frm.Email);

                if (creado)
                {
                    MsgBox.Show("Usuario creado correctamente.", "Éxito",
                        MsgBox.Botones.OK, MsgBox.Icono.Exito);
                    CargarUsuarios();
                }
                else
                {
                    MsgBox.Show("El nombre de usuario ya existe.", "Atención",
                        MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                }
            }
        }

        private void btnModificarPerfiles_Click(object sender, EventArgs e)
        {
            BE.USUARIO seleccionado = dgvUsuarios.CurrentRow?.DataBoundItem as BE.USUARIO;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            using (frmAsignarPerfiles frm = new frmAsignarPerfiles(seleccionado))
                frm.ShowDialog();
        }

        private void btnDesbloquear_Click(object sender, EventArgs e)
        {
            BE.USUARIO seleccionado = dgvUsuarios.CurrentRow?.DataBoundItem as BE.USUARIO;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (!seleccionado.Bloqueado)
            {
                MsgBox.Show("El usuario no está bloqueado.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show("¿Desbloquear al usuario '" + seleccionado.Usuario + "'?",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Pregunta) != DialogResult.Yes)
                return;

            _bll.Desbloquear(seleccionado.Usuario);
            MsgBox.Show("Usuario desbloqueado.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
            CargarUsuarios();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            BE.USUARIO seleccionado = dgvUsuarios.CurrentRow?.DataBoundItem as BE.USUARIO;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            string usuarioActual = SeguridadYServicios.SessionManager.getInstance().getUsuario().Usuario;
            if (seleccionado.Usuario == usuarioActual)
            {
                MsgBox.Show("No podés eliminar tu propio usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            if (MsgBox.Show($"¿Eliminar al usuario '{seleccionado.Usuario}'? Esta acción no se puede deshacer.",
                "Confirmar", MsgBox.Botones.SiNo, MsgBox.Icono.Atencion) != DialogResult.Yes)
                return;

            _bll.Eliminar(seleccionado.Id);
            CargarUsuarios();
        }

        private void btnEditarDatos_Click(object sender, EventArgs e)
        {
            BE.USUARIO seleccionado = dgvUsuarios.CurrentRow?.DataBoundItem as BE.USUARIO;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            using (frmEditarUsuario frm = new frmEditarUsuario(seleccionado))
            {
                if (frm.ShowDialog() != DialogResult.OK) return;

                _bll.ActualizarDatos(seleccionado.Id, frm.Nombre, frm.Apellido, frm.Telefono, frm.Email);
                CargarUsuarios();
            }
        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            BE.USUARIO seleccionado = dgvUsuarios.CurrentRow?.DataBoundItem as BE.USUARIO;
            if (seleccionado == null)
            {
                MsgBox.Show("Seleccioná un usuario.", "Atención",
                    MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            using (frmHistorialUsuario frm = new frmHistorialUsuario(seleccionado))
                frm.ShowDialog();

            CargarUsuarios();
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
