using BE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmMenu : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly Dictionary<string, ToolStripItem> _menuItems = new Dictionary<string, ToolStripItem>();
        private readonly Dictionary<string, string> _menuDefaults = new Dictionary<string, string>();
        private bool _cargandoIdiomas;

        public frmMenu()
        {
            InitializeComponent();
        }

        private void frmMenu_Load(object sender, EventArgs e)
        {
            BE.USUARIO u = SeguridadYServicios.SessionManager.getInstance().getUsuario();
            this.Text = "Menu — " + u.Usuario;

            ActualizarVisibilidadMenu();

            GuardarMenuDefaults(menuStrip1.Items);
            CargarIdiomas();
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        public void ActualizarVisibilidadMenu()
        {
            var sm = SeguridadYServicios.SessionManager.getInstance();
            bool puedeAdminUsuarios = sm.TienePermiso("Administrar usuarios");
            bool puedeGestionRoles = sm.TienePermiso("Gestión de roles");
            bool puedeGestionIdiomas = sm.TienePermiso("Gestión de idiomas");
            bool puedeCatalogo = sm.TienePermiso("Gestionar catálogo de vinos");
            bool puedeAutorizarCatalogo = sm.TienePermiso("Autorizar alta de vinos");
            bool puedeSocios = sm.TienePermiso("Gestionar socios");
            bool puedeArmar = sm.TienePermiso("Armar cajas mensuales");
            bool puedePicking = sm.TienePermiso("Registrar picking y sustituciones");
            bool puedeDespachar = sm.TienePermiso("Despachar cajas");

            usuariosBloqueadosToolStripMenuItem.Visible = puedeAdminUsuarios;
            perfilesToolStripMenuItem.Visible = puedeGestionRoles;
            idiomasToolStripMenuItem.Visible = puedeGestionIdiomas;
            administracionToolStripMenuItem.Visible = puedeAdminUsuarios
                                                   || puedeGestionRoles
                                                   || puedeGestionIdiomas;
            bitacoraToolStripMenuItem.Visible = sm.TienePermiso("Ver bitácora");

            catalogoVinosToolStripMenuItem.Visible = puedeCatalogo;
            entradaStockToolStripMenuItem.Visible = puedeCatalogo;
            autorizarVinoToolStripMenuItem.Visible = puedeAutorizarCatalogo;
            catalogoToolStripMenuItem.Visible = puedeCatalogo || puedeAutorizarCatalogo;

            // CU-28/29/31: los ítems ya existen y quedan visibles-pero-inertes hasta
            // que su propio slice los conecte (Reconciliation note 2 del apply de tasks).
            sociosToolStripMenuItem.Visible = puedeSocios;
            armarCajaToolStripMenuItem.Visible = puedeArmar;
            pickingCajaToolStripMenuItem.Visible = puedePicking;
            despacharCajaToolStripMenuItem.Visible = puedeDespachar;
            clubSociosToolStripMenuItem.Visible = puedeSocios || puedeArmar || puedePicking || puedeDespachar;
        }

        private void frmMenu_FormClosed(object sender, FormClosedEventArgs e)
        {
            SeguridadYServicios.IdiomaManager.getInstance().Desregistrar(this);
        }

        public void ActualizarIdioma()
        {
            foreach (var kvp in _menuItems)
            {
                string texto = SeguridadYServicios.IdiomaManager.getInstance().Traducir(kvp.Key)
                               ?? _menuDefaults[kvp.Key];
                kvp.Value.Text = texto;
            }
        }

        private void GuardarMenuDefaults(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Text))
                {
                    _menuItems[item.Name] = item;
                    _menuDefaults[item.Name] = item.Text;
                }
                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                    GuardarMenuDefaults(mi.DropDownItems);
            }
        }

        private void CargarIdiomas()
        {
            _cargandoIdiomas = true;
            cboIdiomaStatus.Items.Clear();
            foreach (IDIOMA idioma in new BLL.IdiomaBLL().ListarHabilitados())
                cboIdiomaStatus.Items.Add(idioma);
            cboIdiomaStatus.SelectedIndex = -1;
            _cargandoIdiomas = false;
        }

        private void cboIdiomaStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargandoIdiomas) return;
            if (!(cboIdiomaStatus.SelectedItem is IDIOMA idioma)) return;

            var bll = new BLL.IdiomaBLL();

            foreach (var kvp in _menuDefaults)
                bll.RegistrarControl(kvp.Key, "[" + kvp.Value + "]");

            var traducciones = bll.CargarTraducciones(idioma.Id);
            SeguridadYServicios.IdiomaManager.getInstance().CambiarIdioma(idioma, traducciones);

            BE.USUARIO usuario = SeguridadYServicios.SessionManager.getInstance().getUsuario();
            if (usuario != null)
                new BLL.UsuarioBLL().ActualizarIdioma(usuario.Id, idioma.Id);
        }

        private void usuariosBloqueadosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAdminUsuarios().ShowDialog();
            ActualizarVisibilidadMenu();
        }

        private void cambiarContraseñaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmContraseña().ShowDialog();
        }

        private void cerrarSesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string usuario = SeguridadYServicios.SessionManager.getInstance().getUsuario().Usuario;
            new BLL.BitacoraBLL().RegistrarLogout(usuario);
            SeguridadYServicios.SessionManager.getInstance().cerrarSesion();

            new LogIn().Show();
            this.Close();
        }

        private void bitacoraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmBitacora().ShowDialog();
        }

        private void perfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmPerfiles().ShowDialog();
        }

        private void idiomasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmIdiomas().ShowDialog();
            CargarIdiomas();
        }

        private void catalogoVinosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmCatalogoVinos().ShowDialog();
        }

        private void autorizarVinoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAutorizarAltaVino().ShowDialog();
        }

        private void entradaStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmRegistrarEntradaStock().ShowDialog();
        }

        private void sociosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmSocios().ShowDialog();
        }

        private void armarCajaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmArmarCaja().ShowDialog();
        }
    }
}