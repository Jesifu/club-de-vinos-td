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
CAPAS = ['IdiomaUIHelper', 'FormBase', 'AppTheme', 'MsgBox', 'FrmMensaje', 'Program', 'RemitoDespachoPdf',
         'LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña',
         'frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas', 'frmRestaurarIntegridad',
         'frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo',
         'frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos']
BLL = ['ResultadoIntegridad', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'VinoBLL', 'MovimientoStockBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL',
       'UsuarioHistorialBLL', 'SocioBLL', 'CajaMensualBLL', 'IntegridadBLL']
DAL = ['UsuarioDAL', 'PerfilDAL', 'UsuarioPerfilDAL', 'BitacoraDAL', 'IntegridadDAL', 'IdiomaDAL', 'UsuarioHistorialDAL', 'VinoDAL', 'SolicitudAutorizacionDAL',
       'BodegaDAL', 'MovimientoStockDAL', 'SocioDAL', 'CajaMensualDAL', 'SustitucionDAL', 'Acceso']
BE = ['LoginResultado', 'USUARIO', 'BITACORA', 'IDIOMA', 'CONTROL_IDIOMA', 'UsuarioHistorial', 'PaginaResultado', 'SolicitudAutorizacion', 'MovimientoStock',
      'Socio', 'TipoSolicitud', 'EstadoSolicitud', 'TipoMovimiento', 'Vino', 'CajaMensual', 'EstadoVino', 'Bodega', 'EstadoCaja', 'CajaVino', 'VinoCandidato',
      'AlertaStockMinimo', 'Sustitucion', 'NodoPermiso', 'Rol', 'Permiso']

def grid_rows(rows):
    out = []
    for r, row in enumerate(rows):
        for c, n in enumerate(row):
            if c + 1 < len(row):
                out.append(f'{n} -[hidden]right-> {row[c+1]}')
            if r + 1 < len(rows) and c < len(rows[r + 1]):
                out.append(f'{n} -[hidden]down-> {rows[r+1][c]}')
    return out, rows

def capas_rows():
    helpers = CAPAS[:7]; forms = CAPAS[7:]
    rows = []
    n = max(len(helpers), (len(forms) + 3) // 4)
    for i in range(n):
        row = ([helpers[i]] if i < len(helpers) else []) + forms[4 * i:4 * i + 4]
        rows.append(row)
    return rows

def build(cs=3, cc=4, cb=3, cd=3, ce=4):
    L = ["' ═══ DISPOSICIÓN VERTICAL (solo directivas de layout) ═══",
         "' Disposición vertical: las capas se apilan de arriba hacia abajo con enlaces",
         "' ocultos entre paquetes; las clases de cada paquete se acomodan en una grilla",
         "' de varias filas con enlaces ocultos entre clases (no modifican el modelo).",
         'skinparam nodesep 20', 'skinparam ranksep 30',
         'SeguridadYServicios -[hidden]down-> CAPAS', 'CAPAS -[hidden]down-> BLL', 'BLL -[hidden]down-> DAL', 'DAL -[hidden]down-> BE']
    gs, rs = grid(SEG, cs); gc, rc = grid_rows(capas_rows()); gb, rb = grid(BLL, cb); gd, rd = grid(DAL, cd); ge, re_ = grid(BE, ce)
    # anclas entre paquetes (esquina inferior izquierda de arriba -> esquina superior izquierda de abajo), con peso
    for up, dn in ((rs, rc), (rc, rb), (rb, rd), (rd, re_)):
        for col in range(min(len(up[-1]), len(dn[0]))):
            pass
        for _ in range(4):
            L.append(f'{up[-1][0]} -[hidden]down-> {dn[0][0]}')
    for title, g in (('SeguridadYServicios', gs), ('CAPAS', gc), ('BLL', gb), ('DAL', gd), ('BE', ge)):
        L.append("' Grilla de " + title)
        L += g
    return '\n'.join(L) + '\n'
