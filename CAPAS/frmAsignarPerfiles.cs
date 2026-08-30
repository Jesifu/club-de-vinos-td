using BE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    public partial class frmAsignarPerfiles : MaterialForm, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.PerfilBLL _perfilBll = new BLL.PerfilBLL();
        private readonly BLL.UsuarioPerfilBLL _asignacionBll = new BLL.UsuarioPerfilBLL();

        private readonly USUARIO _usuario;
        private List<NodoPermiso> _arbol;
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        public frmAsignarPerfiles(USUARIO usuario)
        {
            InitializeComponent();
            _usuario = usuario;
        }

        private void frmAsignarPerfiles_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = "Usuario: " + _usuario.Usuario;
            GuardarDefaults(this.Controls);
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();
            IdiomaUIHelper.AgregarSelector(this);

            _arbol = _perfilBll.ObtenerArbol();
            CargarArbolRoles();
            MarcarRolesAsignados();
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
        }

        private void frmAsignarPerfiles_FormClosed(object sender, FormClosedEventArgs e)
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

        private void CargarArbolRoles()
        {
            treeRoles.Nodes.Clear();
            foreach (NodoPermiso raiz in _arbol)
            {
                if (!raiz.EsHoja())
                {
                    TreeNode nodo = new TreeNode(raiz.Nombre) { Tag = raiz };
                    AgregarRolesRecursivo(nodo, raiz);
                    treeRoles.Nodes.Add(nodo);
                }
            }
            treeRoles.ExpandAll();
        }

        private void AgregarRolesRecursivo(TreeNode nodoTree, NodoPermiso nodo)
        {
            foreach (NodoPermiso hijo in nodo.ObtenerHijos())
            {
                if (!hijo.EsHoja())
                {
                    TreeNode nodoHijo = new TreeNode(hijo.Nombre) { Tag = hijo };
                    AgregarRolesRecursivo(nodoHijo, hijo);
                    nodoTree.Nodes.Add(nodoHijo);
                }
            }
        }

        private void MarcarRolesAsignados()
        {
            List<int> asignados = _asignacionBll.ObtenerPerfilesAsignados(_usuario.Id);
            MarcarNodos(treeRoles.Nodes, asignados);
        }

        private void MarcarNodos(TreeNodeCollection nodos, List<int> asignados)
        {
            foreach (TreeNode tn in nodos)
            {
                if (tn.Tag is NodoPermiso nodo)
                    tn.Checked = asignados.Contains(nodo.Id);
                MarcarNodos(tn.Nodes, asignados);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            List<int> seleccionados = new List<int>();
            ObtenerIdsCheckeados(treeRoles.Nodes, seleccionados);
            _asignacionBll.GuardarAsignaciones(_usuario.Id, seleccionados);
            MsgBox.Show("Asignaciones guardadas.", "Éxito",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
        }

        private void ObtenerIdsCheckeados(TreeNodeCollection nodos, List<int> ids)
        {
            foreach (TreeNode tn in nodos)
            {
                if (tn.Checked && tn.Tag is NodoPermiso nodo)
                    ids.Add(nodo.Id);
                ObtenerIdsCheckeados(tn.Nodes, ids);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
