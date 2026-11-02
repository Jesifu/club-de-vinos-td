def grid(names, cols):
    out = []
    rows = [names[i:i + cols] for i in range(0, len(names), cols)]
    for r, row in enumerate(rows):
        for c, n in enumerate(row):
            if c + 1 < len(row):
                out.append(f'{n} -[hidden]right-> {row[c+1]}')
            if r + 1 < len(rows) and c < len(rows[r + 1]):
                out.append(f'{n} -[hidden]down-> {rows[r+1][c]}')
    return out, rows

SEG = ['Hasher', 'ValidadorContrasena', 'CalculadorDVH', 'SessionManager', 'IdiomaManager', 'IObservadorIdioma']
CAPAS = ['IdiomaUIHelper', 'FormBase', 'MsgBox', 'FrmMensaje', 'Program', 'RemitoDespachoPdf',
         'LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña',
         'frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas', 'frmRestaurarIntegridad',
         'frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo',
         'frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos',
         'frmAnalisisRotacion', 'frmAutorizarPedidoReposicion', 'frmRecibirPedidoReposicion', 'frmConsultarPedidosReposicion', 'FrmMotivoRechazoPedido',
         'frmAyuda', 'PedidoReposicionUI', 'ConfiguracionRotacionStore', 'RotacionGraficos', 'RotacionReposicionPdf', 'ErrorInicio', 'AyudaContexto', 'AyudaRepositorio', 'AyudaTema']
BLL = ['ResultadoIntegridad', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'VinoBLL', 'MovimientoStockBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL',
       'UsuarioHistorialBLL', 'SocioBLL', 'CajaMensualBLL', 'IntegridadBLL',
       'RotacionBLL', 'PedidoReposicionBLL', 'PedidoParcialException', 'ConexionBLL']
DAL = ['UsuarioDAL', 'PerfilDAL', 'UsuarioPerfilDAL', 'BitacoraDAL', 'IntegridadDAL', 'IdiomaDAL', 'UsuarioHistorialDAL', 'VinoDAL', 'SolicitudAutorizacionDAL',
       'BodegaDAL', 'MovimientoStockDAL', 'SocioDAL', 'CajaMensualDAL', 'SustitucionDAL', 'RotacionDAL', 'PedidoReposicionDAL', 'ConexionDAL', 'Acceso']
BE = ['LoginResultado', 'USUARIO', 'BITACORA', 'IDIOMA', 'CONTROL_IDIOMA', 'UsuarioHistorial', 'PaginaResultado', 'SolicitudAutorizacion', 'MovimientoStock',
      'Socio', 'TipoSolicitud', 'EstadoSolicitud', 'TipoMovimiento', 'Vino', 'CajaMensual', 'EstadoVino', 'Bodega', 'EstadoCaja', 'CajaVino', 'VinoCandidato',
      'AlertaStockMinimo', 'Sustitucion', 'EstadoPedidoReposicion', 'ConfiguracionRotacion', 'ClasificacionRotacion', 'RecomendacionReposicion', 'ResultadoRotacion', 'PedidoReposicion', 'PedidoReposicionLinea', 'ResultadoConexion', 'NodoPermiso', 'Rol', 'Permiso']

def grid_rows(rows):
    out = []
    for r, row in enumerate(rows):
        for c, n in enumerate(row):
            if c + 1 < len(row):
                out.append(f'{n} -[hidden]right-> {row[c+1]}')
            if r + 1 < len(rows) and c < len(rows[r + 1]):
                out.append(f'{n} -[hidden]down-> {rows[r+1][c]}')
    return out, rows

def capas_rows(cols=7):
    return [CAPAS[i:i + cols] for i in range(0, len(CAPAS), cols)]

def build(cs=6, cc=7, cb=6, cd=6, ce=8):
    L = ["' ═══ DISPOSICIÓN VERTICAL (solo directivas de layout) ═══",
         "' Las capas se apilan de arriba hacia abajo (CAPAS, SeguridadYServicios, BLL, DAL, BE)",
         "' con enlaces ocultos entre paquetes y entre una clase de cada capa; las clases de",
         "' cada paquete se acomodan en una grilla con enlaces ocultos (no modifican el modelo).",
         'skinparam nodesep 18', 'skinparam ranksep 26']
    gs, rs = grid(SEG, cs); gc, rc = grid_rows(capas_rows(cc)); gb, rb = grid(BLL, cb); gd, rd = grid(DAL, cd); ge, re_ = grid(BE, ce)
    order = (('CAPAS', rc), ('SeguridadYServicios', rs), ('BLL', rb), ('DAL', rd), ('BE', re_))
    for (na, ra), (nb, rb_) in zip(order, order[1:]):
        L.append(f'{na} -[hidden]down-> {nb}')
        for _ in range(3):
            L.append(f'{ra[-1][0]} -[hidden]down-> {rb_[0][0]}')
    for title, g in (('CAPAS', gc), ('SeguridadYServicios', gs), ('BLL', gb), ('DAL', gd), ('BE', ge)):
        L.append("' Grilla de " + title)
        L += g
    return chr(10).join(L) + chr(10)
