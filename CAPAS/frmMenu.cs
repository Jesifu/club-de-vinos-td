using BE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmMenu : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly Dictionary<string, ToolStripItem> _menuItems = new Dictionary<string, ToolStripItem>();
        private readonly Dictionary<string, string> _menuDefaults = new Dictionary<string, string>();
        private bool _cargandoIdiomas;

        // Menú Ayuda (D02): se arma por código para no tocar frmMenu.Designer.cs. Se crea antes de
        // Load, así GuardarMenuDefaults lo incluye en la traducción. Visible para todo usuario con sesión.
        private ToolStripMenuItem ayudaToolStripMenuItem;
        private ToolStripMenuItem ayudaEnLineaToolStripMenuItem;
        private ToolStripMenuItem acercaDeToolStripMenuItem;

        // Ítems del Pedido de Reposición (CU-38, CU-39, CU-40): se arman por código, como el menú Ayuda,
        // y se insertan en Catálogo a continuación de "Analizar rotación de stock".
        private ToolStripMenuItem autorizarPedidoToolStripMenuItem;
        private ToolStripMenuItem recibirPedidoToolStripMenuItem;
        private ToolStripMenuItem consultarPedidosToolStripMenuItem;

        public frmMenu()
        {
            InitializeComponent();
            CrearMenuAyuda();
            CrearMenuPedidos();
        }

        private void CrearMenuPedidos()
        {
            autorizarPedidoToolStripMenuItem = new ToolStripMenuItem("Autorizar pedidos de reposición")
            {
                Name = "autorizarPedidoToolStripMenuItem"
            };
            autorizarPedidoToolStripMenuItem.Click += (s, e) => new frmAutorizarPedidoReposicion().ShowDialog();

            recibirPedidoToolStripMenuItem = new ToolStripMenuItem("Registrar recepción de pedido")
            {
                Name = "recibirPedidoToolStripMenuItem"
            };
            recibirPedidoToolStripMenuItem.Click += (s, e) => new frmRecibirPedidoReposicion().ShowDialog();

            consultarPedidosToolStripMenuItem = new ToolStripMenuItem("Consultar pedidos de reposición")
            {
                Name = "consultarPedidosToolStripMenuItem"
            };
            consultarPedidosToolStripMenuItem.Click += (s, e) => new frmConsultarPedidosReposicion().ShowDialog();

            int indice = catalogoToolStripMenuItem.DropDownItems.IndexOf(rotacionStockToolStripMenuItem) + 1;
            catalogoToolStripMenuItem.DropDownItems.Insert(indice, consultarPedidosToolStripMenuItem);
            catalogoToolStripMenuItem.DropDownItems.Insert(indice, recibirPedidoToolStripMenuItem);
            catalogoToolStripMenuItem.DropDownItems.Insert(indice, autorizarPedidoToolStripMenuItem);
        }

        private void CrearMenuAyuda()
        {
            ayudaEnLineaToolStripMenuItem = new ToolStripMenuItem("Ayuda en línea (F1)")
            {
                Name = "ayudaEnLineaToolStripMenuItem"
            };
            ayudaEnLineaToolStripMenuItem.Click += ayudaEnLineaToolStripMenuItem_Click;

            acercaDeToolStripMenuItem = new ToolStripMenuItem("Acerca de")
            {
                Name = "acercaDeToolStripMenuItem"
            };
            acercaDeToolStripMenuItem.Click += acercaDeToolStripMenuItem_Click;

            ayudaToolStripMenuItem = new ToolStripMenuItem("Ayuda") { Name = "ayudaToolStripMenuItem" };
            ayudaToolStripMenuItem.DropDownItems.Add(ayudaEnLineaToolStripMenuItem);
            ayudaToolStripMenuItem.DropDownItems.Add(acercaDeToolStripMenuItem);
            menuStrip1.Items.Add(ayudaToolStripMenuItem);
        }

        private void ayudaEnLineaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirAyuda();
        }

        private void acercaDeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            string titulo = mgr.Traducir("acercaDeToolStripMenuItem") ?? "Acerca de";
            string etiquetaVersion = mgr.Traducir("ayuda_acercaDe_version") ?? "Versión";
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MsgBox.Show("Club de Vinos" + Environment.NewLine + etiquetaVersion + " " + version,
                titulo, MsgBox.Botones.OK, MsgBox.Icono.Info);
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
        }

        public void ActualizarVisibilidadMenu()
        {
            var sm = SeguridadYServicios.SessionManager.getInstance();
            bool puedeAdminUsuarios = sm.TienePermiso("Administrar usuarios");
            bool puedeGestionRoles = sm.TienePermiso("Gestión de roles");
            bool puedeGestionIdiomas = sm.TienePermiso("Gestión de idiomas");
            bool puedeCatalogo = sm.TienePermiso("Gestionar catálogo de vinos");
            bool puedeStock = sm.TienePermiso("Gestionar stock de vinos");
            bool puedeAutorizarCatalogo = sm.TienePermiso("Autorizar alta de vinos");
            bool puedeSocios = sm.TienePermiso("Gestionar socios");
            bool puedeArmar = sm.TienePermiso("Armar cajas mensuales");
            bool puedePicking = sm.TienePermiso("Registrar picking y sustituciones");
            bool puedeDespachar = sm.TienePermiso("Despachar cajas");
            bool puedeRotacion = sm.TienePermiso("Analizar rotación de stock");
            bool puedeGestionarPedidos = sm.TienePermiso("Gestionar pedidos de reposición");
            bool puedeAutorizarPedidos = sm.TienePermiso("Autorizar pedidos de reposición");
            bool puedeRecibirPedidos = sm.TienePermiso("Recibir pedidos de reposición");

            usuariosBloqueadosToolStripMenuItem.Visible = puedeAdminUsuarios;
            perfilesToolStripMenuItem.Visible = puedeGestionRoles;
            idiomasToolStripMenuItem.Visible = puedeGestionIdiomas;
            administracionToolStripMenuItem.Visible = puedeAdminUsuarios
                                                   || puedeGestionRoles
                                                   || puedeGestionIdiomas;
            bitacoraToolStripMenuItem.Visible = sm.TienePermiso("Ver bitácora");

            catalogoVinosToolStripMenuItem.Visible = puedeCatalogo;
            // Entrada, ajuste y alerta de stock tienen su propio permiso: el Encargado de Depósito
            // opera el stock sin poder proponer altas ni bajas del catálogo.
            entradaStockToolStripMenuItem.Visible = puedeStock;
            ajusteInventarioToolStripMenuItem.Visible = puedeStock;
            alertaStockToolStripMenuItem.Visible = puedeStock;
            autorizarVinoToolStripMenuItem.Visible = puedeAutorizarCatalogo;
            autorizarBajaVinoToolStripMenuItem.Visible = puedeAutorizarCatalogo;
            rotacionStockToolStripMenuItem.Visible = puedeRotacion;
            // Cada ítem del Pedido de Reposición se oculta por su propio permiso; la consulta (solo
            // lectura) la ve quien tenga cualquiera de los tres permisos del ciclo.
            autorizarPedidoToolStripMenuItem.Visible = puedeAutorizarPedidos;
            recibirPedidoToolStripMenuItem.Visible = puedeRecibirPedidos;
            consultarPedidosToolStripMenuItem.Visible = puedeGestionarPedidos || puedeAutorizarPedidos || puedeRecibirPedidos;
            // El padre se oculta solo si ningún hijo es visible. Se usan los permisos y no la propiedad
            // Visible de los hijos: mientras el desplegable no se muestra, esta devuelve false.
            catalogoToolStripMenuItem.Visible = puedeCatalogo || puedeStock || puedeAutorizarCatalogo || puedeRotacion
                                             || puedeGestionarPedidos || puedeAutorizarPedidos || puedeRecibirPedidos;

            // CU-28/29/31: los ítems ya existen y quedan visibles-pero-inertes hasta
            // que su propio slice los conecte (Reconciliation note 2 del apply de tasks).
            sociosToolStripMenuItem.Visible = puedeSocios;
            armarCajaToolStripMenuItem.Visible = puedeArmar;
            pickingCajaToolStripMenuItem.Visible = puedePicking;
            despacharCajaToolStripMenuItem.Visible = puedeDespachar;
            historialDespachosToolStripMenuItem.Visible = puedeDespachar;
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

        private void ajusteInventarioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmRegistrarAjusteInventario().ShowDialog();
        }

        private void alertaStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAlertaStockMinimo().ShowDialog();
        }

        private void autorizarBajaVinoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAutorizarBajaVino().ShowDialog();
        }

        private void rotacionStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAnalisisRotacion().ShowDialog();
        }

        private void sociosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmSocios().ShowDialog();
        }

        private void armarCajaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmArmarCaja().ShowDialog();
        }

        private void pickingCajaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmPickingCaja().ShowDialog();
        }

        private void despacharCajaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmDespacharCaja().ShowDialog();
        }

        private void historialDespachosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmHistorialDespachos().ShowDialog();
        }
    }
}