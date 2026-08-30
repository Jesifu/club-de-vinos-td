using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmHistorialUsuario : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private const int TAMANIO_PAGINA = 20;

        private readonly BE.USUARIO _usuario;
        private readonly BLL.UsuarioHistorialBLL _bll = new BLL.UsuarioHistorialBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        private int _paginaActual = 1;
        private int _totalPaginas = 1;

        public frmHistorialUsuario(BE.USUARIO usuario)
        {
            _usuario = usuario;
            InitializeComponent();
            this.Resize += (s, e) => ReposicionarLayout();
            this.Shown += (s, e) => ReposicionarLayout();
            panelInferior.Layout += (s, e) => ReposicionarBotonesPanel();
        }

        private void frmHistorialUsuario_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles.Remove("lblPagina");
            _defaults.Remove("lblPagina");
            _controles["lblTitulo_HistorialUsuarios"] = lblTitulo;
            _defaults["lblTitulo_HistorialUsuarios"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            CargarHistorial();
            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            ReposicionarLayout();
        }

        private void ReposicionarLayout()
        {
            if (panelInferior == null || dgvHistorial == null) return;

            Rectangle area = this.ClientRectangle;
            foreach (Control c in this.Controls)
            {
                if (c.Dock == DockStyle.Bottom && c.Visible)
                    area.Height -= c.Height;
            }

            int altoGrilla = area.Height - dgvHistorial.Top - 10;
            if (altoGrilla < 80) altoGrilla = 80;
            dgvHistorial.Height = altoGrilla;
            dgvHistorial.Width = this.ClientSize.Width - dgvHistorial.Left - 40;

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

        private void frmHistorialUsuario_FormClosed(object sender, FormClosedEventArgs e)
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
            if (dgvHistorial.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            if (dgvHistorial.Columns["FechaCambio"] != null)
                dgvHistorial.Columns["FechaCambio"].HeaderText = mgr.Traducir("colhdr_FechaCambio") ?? "Fecha";
            if (dgvHistorial.Columns["TipoCambio"] != null)
                dgvHistorial.Columns["TipoCambio"].HeaderText = mgr.Traducir("colhdr_TipoCambio") ?? "Tipo";
            if (dgvHistorial.Columns["Rol"] != null)
                dgvHistorial.Columns["Rol"].HeaderText = mgr.Traducir("colhdr_Rol") ?? "Rol";
            if (dgvHistorial.Columns["Bloqueado"] != null)
                dgvHistorial.Columns["Bloqueado"].HeaderText = mgr.Traducir("colhdr_Bloqueado") ?? "Bloqueado";
            if (dgvHistorial.Columns["IntentosFallidos"] != null)
                dgvHistorial.Columns["IntentosFallidos"].HeaderText = mgr.Traducir("colhdr_IntentosFallidos") ?? "Intentos";
            if (dgvHistorial.Columns["Perfiles"] != null)
                dgvHistorial.Columns["Perfiles"].HeaderText = mgr.Traducir("colhdr_Perfiles") ?? "Perfiles";
            if (dgvHistorial.Columns["Nombre"] != null)
                dgvHistorial.Columns["Nombre"].HeaderText = mgr.Traducir("colhdr_NombrePersona") ?? "Nombre";
            if (dgvHistorial.Columns["Apellido"] != null)
                dgvHistorial.Columns["Apellido"].HeaderText = mgr.Traducir("colhdr_Apellido") ?? "Apellido";
            if (dgvHistorial.Columns["RealizadoPor"] != null)
                dgvHistorial.Columns["RealizadoPor"].HeaderText = mgr.Traducir("colhdr_RealizadoPor") ?? "Realizado por";
            if (dgvHistorial.Columns["VersionOrigen"] != null)
                dgvHistorial.Columns["VersionOrigen"].HeaderText = mgr.Traducir("colhdr_VersionOrigen") ?? "Versión origen";
            if (dgvHistorial.Columns["Telefono"] != null)
                dgvHistorial.Columns["Telefono"].HeaderText = mgr.Traducir("colhdr_Telefono") ?? "Teléfono";
            if (dgvHistorial.Columns["Email"] != null)
                dgvHistorial.Columns["Email"].HeaderText = mgr.Traducir("colhdr_Email") ?? "Email";
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

        private void CargarHistorial()
        {
            BE.PaginaResultado<BE.UsuarioHistorial> resultado =
                _bll.ObtenerHistorialPaginado(_usuario.Id, _paginaActual, TAMANIO_PAGINA);

            _totalPaginas = resultado.TotalPaginas == 0 ? 1 : resultado.TotalPaginas;
            if (_paginaActual > _totalPaginas) _paginaActual = _totalPaginas;

            dgvHistorial.DataSource = resultado.Items;

            if (dgvHistorial.Columns.Count > 0)
            {
                if (dgvHistorial.Columns["Id"] != null) dgvHistorial.Columns["Id"].Visible = false;
                if (dgvHistorial.Columns["UsuarioId"] != null) dgvHistorial.Columns["UsuarioId"].Visible = false;
                if (dgvHistorial.Columns["UsuarioLogin"] != null) dgvHistorial.Columns["UsuarioLogin"].Visible = false;
                if (dgvHistorial.Columns["RolId"] != null) dgvHistorial.Columns["RolId"].Visible = false;
                if (dgvHistorial.Columns["FechaCambio"] != null) dgvHistorial.Columns["FechaCambio"].Width = 145;
                if (dgvHistorial.Columns["TipoCambio"] != null) dgvHistorial.Columns["TipoCambio"].Width = 130;
                if (dgvHistorial.Columns["Rol"] != null) dgvHistorial.Columns["Rol"].Width = 80;
                if (dgvHistorial.Columns["Bloqueado"] != null) dgvHistorial.Columns["Bloqueado"].Width = 80;
                if (dgvHistorial.Columns["IntentosFallidos"] != null) dgvHistorial.Columns["IntentosFallidos"].Width = 90;
                if (dgvHistorial.Columns["Perfiles"] != null) dgvHistorial.Columns["Perfiles"].Width = 90;
                if (dgvHistorial.Columns["Nombre"] != null) dgvHistorial.Columns["Nombre"].Width = 100;
                if (dgvHistorial.Columns["Apellido"] != null) dgvHistorial.Columns["Apellido"].Width = 100;
                if (dgvHistorial.Columns["RealizadoPor"] != null) dgvHistorial.Columns["RealizadoPor"].Width = 130;
                if (dgvHistorial.Columns["VersionOrigen"] != null) dgvHistorial.Columns["VersionOrigen"].Width = 100;
                ActualizarEncabezados();
            }

            btnRollback.Enabled = false;
            ActualizarControlesPaginacion();
        }

        private void ActualizarControlesPaginacion()
        {
            lblPagina.Text = string.Format("Página {0} de {1}", _paginaActual, _totalPaginas);
            btnPaginaAnterior.Enabled = _paginaActual > 1;
            btnPaginaSiguiente.Enabled = _paginaActual < _totalPaginas;
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (_paginaActual <= 1) return;
            _paginaActual--;
            CargarHistorial();
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (_paginaActual >= _totalPaginas) return;
            _paginaActual++;
            CargarHistorial();
        }

        private void dgvHistorial_SelectionChanged(object sender, EventArgs e)
        {
            BE.UsuarioHistorial sel = dgvHistorial.CurrentRow?.DataBoundItem as BE.UsuarioHistorial;
            btnRollback.Enabled = sel != null && sel.TipoCambio != "BAJA";
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            BE.UsuarioHistorial sel = dgvHistorial.CurrentRow?.DataBoundItem as BE.UsuarioHistorial;
            if (sel == null) return;

            // FIX: no permitir rollback de un rollback
            if (sel.TipoCambio == "ROLLBACK")
            {
                MsgBox.Show("No se puede restaurar una versión que ya es un rollback.",
                    "Operación no permitida", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            string msg = string.Format(
                "¿Restaurar el estado del usuario '{0}' a la versión del {1:dd/MM/yyyy HH:mm}?\n\n" +
                "Tipo de cambio registrado: {2}\n\n" +
                "Nota: la contraseña NO se restaurará (se mantendrá la actual).",
                _usuario.Usuario, sel.FechaCambio, sel.TipoCambio);

            if (MsgBox.Show(msg, "Confirmar rollback",
                    MsgBox.Botones.SiNo, MsgBox.Icono.Atencion) != DialogResult.Yes)
                return;

            try
            {
                string admin = SeguridadYServicios.SessionManager.getInstance().getUsuario().Usuario;
                _bll.Rollback(sel.Id, admin);
                MsgBox.Show("Estado restaurado correctamente.", "Éxito",
                    MsgBox.Botones.OK, MsgBox.Icono.Exito);
                CargarHistorial();
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Error",
                    MsgBox.Botones.OK, MsgBox.Icono.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
