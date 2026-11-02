"""Genera DIAGRAMAS/DiagramaClases.puml (maestro) a partir del codigo real (indice csparse).
Los miembros salen del codigo; las relaciones se derivan de las referencias reales entre clases.
Uso: python -I gen_master.py <salida.puml>
"""
import sys, os, re
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from csparse import load_all, ROOT
from pumlparse import parse_puml

# Diagrama vigente: de ahi se leen los miembros privados que ya estaban dibujados (keep_private).
OLD = os.path.join(ROOT, 'DIAGRAMAS', 'DiagramaClases.puml')
allt, codes = load_all()
by = {}
for t in allt:
    by.setdefault(t['name'], []).append(t)
old_classes, _ = parse_puml(OLD)

def layer(n):
    return os.path.relpath(by[n][0]['file'], ROOT).split(os.sep)[0]

RAW = {}
def raw_init(m):
    f = m['file']
    if f not in RAW:
        RAW[f] = open(f, encoding='utf-8-sig').read().splitlines()
    l = RAW[f][m['line'] - 1]
    mm = re.search(r'=\s*(.*?);\s*(//.*)?$', l)
    return mm.group(1) if mm else None

def ty(t):
    t = re.sub(r'\b(BE|DAL|BLL|SeguridadYServicios)\.', '', t or '')
    t = t.replace('System.', '')
    return t

VIS = {'public': '+', 'private': '-', 'protected': '#', 'internal': '~'}
DEP_FIELD_RE = re.compile(r'(DAL|BLL)$|^Acceso$')
ALWAYS_PRIV_FIELDS = {'_controles', '_defaults', '_menuItems', '_menuDefaults'}
ALWAYS_PRIV_METHODS = {'ActualizarEncabezados'}

def members_of(name):
    res = []
    for t in by[name]:
        for m in t['members']:
            if t['designer']:
                continue
            m = dict(m); m['file'] = t['file']
            if t['kind'] == 'interface' and m.get('vis') == 'private':
                m['vis'] = 'public'
            res.append(m)
    return res

def keep_private(name, m):
    old = old_classes.get(name)
    oldnames = {x['name'] for x in old['members'] if x['kind'] in ('attr', 'method') and x['vis'] == '-'} if old else set()
    n = m['name']
    if m['kind'] == 'field_or_prop':
        if n in ALWAYS_PRIV_FIELDS or n in oldnames:
            return True
        base = re.sub(r'<.*', '', ty(m['type']))
        return bool(DEP_FIELD_RE.search(base)) and m['vis'] == 'private' and not m['static']
    if m['kind'] == 'method':
        return n in ALWAYS_PRIV_METHODS or n in oldnames
    return False

def fmt_params(ps):
    out = []
    for p in ps:
        s = '%s : %s' % (p['name'], ty(p['type']))
        if p['default']:
            s += ' = ' + p['default']
        out.append(s)
    return ', '.join(out)

def member_lines(name, kind):
    lines = []
    ms = members_of(name)
    if kind == 'enum':
        return ['    ' + m['name'] for m in ms if m['kind'] == 'enumval']
    for m in ms:
        k = m['kind']
        if k not in ('field_or_prop', 'property', 'method', 'ctor'):
            continue
        vis = m['vis']
        if vis not in ('public', 'internal', 'protected') and not keep_private(name, m):
            continue
        if vis == 'internal' and k in ('field_or_prop',) and layer(name) == 'CAPAS' and name == 'AppTheme':
            continue  # paleta de colores/fuentes (constantes internas): no se dibuja
        v = VIS[vis]
        st = '{static} ' if m['static'] else ''
        ab = '{abstract} ' if m.get('abstract') else ''
        if k in ('field_or_prop', 'property'):
            s = '    %s %s%s%s : %s' % (v, st, ab, m['name'], ty(m['type']))
            if m['static'] and vis in ('public', 'internal') and m.get('init'):
                s += ' = ' + raw_init(m)
            lines.append(s)
        elif k == 'ctor':
            lines.append('    %s %s(%s)' % (v, m['name'], fmt_params(m['params'])))
        else:
            lines.append('    %s %s%s%s(%s) : %s' % (v, st, ab, m['name'], fmt_params(m['params']), ty(m['ret'])))
    return lines

def cls(name, indent='  ', kind=None, stereo='', label=None, extra_note=None):
    t0 = by[name][0]
    kd = kind or ('abstract class' if 'abstract' in t0['mods'] and t0['kind'] == 'class' else t0['kind'])
    head = '%s%s ' % (indent, kd)
    head += ('"%s" as %s' % (label, name)) if label else name
    if stereo:
        head += ' ' + stereo
    out = [head + ' {']
    for l in member_lines(name, t0['kind']):
        out.append(indent + '  ' + l.strip())
    out.append(indent + '}')
    return '\n'.join(out)

STATIC = '<<static>>'
SPEC_STEREO = {
    'Hasher': STATIC, 'ValidadorContrasena': STATIC, 'CalculadorDVH': STATIC, 'IdiomaUIHelper': STATIC,
    'MsgBox': STATIC, 'RemitoDespachoPdf': STATIC, 'Program': STATIC,
    'ClasificacionRotacion': STATIC, 'RecomendacionReposicion': STATIC, 'ErrorInicio': STATIC, 'ConfiguracionRotacionStore': STATIC,
    'RotacionGraficos': STATIC, 'RotacionReposicionPdf': STATIC, 'PedidoReposicionUI': STATIC, 'AyudaContexto': STATIC, 'AyudaRepositorio': STATIC,
    'SessionManager': '<<singleton>>', 'IdiomaManager': '<<singleton>>', 'Acceso': '<<internal>>',
}

def C(name, indent='  ', **kw):
    kw.setdefault('stereo', SPEC_STEREO.get(name, ''))
    return cls(name, indent, **kw)

# ---------------------------------------------------------------- relaciones
def deps_text(name):
    txt = ''
    for t in by[name]:
        s = codes[t['file']]
        multi = sum(1 for x in allt if x['file'] == t['file'] and not x['designer']) > 1
        txt += s[t['span'][0]:t['span'][1]] if (multi and t.get('span')) else s
    return txt
def uses(a, b):
    return len(re.findall(r'\b' + re.escape(b) + r'\b', deps_text(a))) > 0

def forms():
    return [n for n in by if layer(n) == 'CAPAS' and n.startswith('frm')] + ['LogIn']

def has_field(a, b):
    # True si a guarda b en un campo/propiedad (asociacion); si solo lo usa en locales,
    # parametros o retornos es una dependencia.
    for m in members_of(a):
        if m['kind'] in ('field_or_prop', 'property') and re.search(r'\b' + re.escape(b) + r'\b', ty(m['type']) or ''):
            return True
    return False

def arrow(a, b):
    return '-->' if has_field(a, b) else '..>'

REL = []   # (a, arrow, b, label)
def rel(a, arrow, b, label=None):
    assert uses(a, b) or arrow.startswith('<|'), (a, b)
    REL.append((a, arrow, b, label))

def auto_rels(a, targets):
    out = []
    for b in targets:
        if b != a and uses(a, b):
            out.append(f'{a} {arrow(a, b)} {b}')
    return out

P3_BE = ['ConfiguracionRotacion', 'ResultadoRotacion', 'ClasificacionRotacion', 'RecomendacionReposicion',
         'PedidoReposicion', 'PedidoReposicionLinea', 'EstadoPedidoReposicion', 'ResultadoConexion']
P3_DAL = ['RotacionDAL', 'PedidoReposicionDAL', 'ConexionDAL']
P3_BLL = ['RotacionBLL', 'PedidoReposicionBLL', 'PedidoParcialException', 'ConexionBLL']
P3_FORMS = ['frmAnalisisRotacion', 'frmAutorizarPedidoReposicion', 'frmRecibirPedidoReposicion', 'frmConsultarPedidosReposicion', 'FrmMotivoRechazoPedido']
P3_HELPERS = ['PedidoReposicionUI', 'ConfiguracionRotacionStore', 'RotacionGraficos', 'RotacionReposicionPdf', 'ErrorInicio', 'AyudaContexto', 'AyudaRepositorio', 'AyudaTema']

DALS = [n for n in by if layer(n) == 'DAL' and n != 'Acceso']
BLLS = [n for n in by if layer(n) == 'BLL' and n != 'ResultadoIntegridad']
FORMS = sorted(set(forms()))

def rels_dal():
    out = []
    for n in sorted(DALS):
        out.append(f'{n} --> Acceso')
    for n in ('UsuarioDAL', 'BitacoraDAL', 'UsuarioHistorialDAL'):
        out.append(f'{n} --> PaginaResultado')
    return out

FRMMENU_NOTE = '''note bottom of frmMenu
  Abre con ShowDialog() los formularios de sus ítems de menú
  (contraseña, usuarios, perfiles, idiomas, bitácora, catálogo,
  stock, socios y cajas). Los demás los abren otros formularios
  (frmAdminUsuarios, LogIn, frmRestaurarIntegridad) o MsgBox.
  Al cerrar sesión muestra LogIn con Show().
end note
'''

# ---------------------------------------------------------------- generacion
def main(outpath):
    P = []
    w = P.append
    w('@startuml DiagramaClases - BDCAPAS\n')
    w('skinparam classAttributeIconSize 0\nskinparam packageStyle rectangle\nskinparam linetype ortho\nhide empty members\n')
    SEP = '\' ─────────────────────────────────────────'
    # ----- BE
    w(SEP + '\n\' CAPA BE\n' + SEP)
    w('package "BE" #FEFECE {\n')
    w(C('LoginResultado'))
    for n in ('USUARIO', 'BITACORA', 'IDIOMA', 'CONTROL_IDIOMA', 'UsuarioHistorial'):
        w('\n' + C(n))
    w('\n' + C('PaginaResultado', label='PaginaResultado<T>'))
    w('\n  \' ── Dominio: catálogo y stock de vinos (gestion-catalogo-stock-vinos) ──')
    for n in ('EstadoVino', 'TipoSolicitud', 'EstadoSolicitud', 'TipoMovimiento'):
        w(C(n) + '\n')
    for n in ('Vino', 'SolicitudAutorizacion', 'Bodega', 'MovimientoStock', 'AlertaStockMinimo'):
        w(C(n) + '\n')
    w('''  Vino --> EstadoVino
  Vino --> TipoSolicitud
  Vino --> EstadoSolicitud
  Vino --> Bodega : BodegaId >
  SolicitudAutorizacion "0..*" --> "1" Vino : Vino >
  SolicitudAutorizacion --> TipoSolicitud
  SolicitudAutorizacion --> EstadoSolicitud
  MovimientoStock --> TipoMovimiento
  MovimientoStock --> Vino : VinoId >
''')
    w('  \' ── Dominio: curación y armado de cajas mensuales (curacion-cajas-mensuales) ──')
    w(C('EstadoCaja') + '\n')
    for n in ('Socio', 'CajaMensual', 'CajaVino', 'Sustitucion', 'VinoCandidato'):
        w(C(n) + '\n')
    w('''  CajaMensual  "0..*" --> "1" Socio : SocioId >
  CajaMensual  "1" o-- "0..*" CajaVino : Lineas
  Sustitucion  "0..*" --> "1" CajaVino : CajaVinoId >
  CajaMensual  --> EstadoCaja
''')
    w('  \' ── Dominio: rotación y pedido de reposición ──')
    w(C('EstadoPedidoReposicion') + '\n')
    for n in ('ConfiguracionRotacion', 'ClasificacionRotacion', 'RecomendacionReposicion', 'ResultadoRotacion', 'PedidoReposicion', 'PedidoReposicionLinea', 'ResultadoConexion'):
        w(C(n) + '\n')
    for a in ('PedidoReposicion', 'PedidoReposicionLinea', 'ResultadoRotacion'):
        for l in auto_rels(a, P3_BE):
            w('  ' + l)
    w('  PedidoReposicion "1" o-- "1..*" PedidoReposicionLinea : Lineas\n')
    w(C('NodoPermiso') + '\n')
    w(C('Rol') + '\n')
    w(C('Permiso') + '\n')
    w('''  NodoPermiso <|-- Rol
  NodoPermiso <|-- Permiso
  Rol "1" o-- "0..*" NodoPermiso : _hijos
}
''')
    # ----- SeguridadYServicios
    w(SEP + '\n\' CAPA SeguridadYServicios\n' + SEP)
    w('package "SeguridadYServicios" #D5E8D4 {\n')
    for n in ('IObservadorIdioma', 'Hasher', 'ValidadorContrasena', 'CalculadorDVH', 'SessionManager', 'IdiomaManager'):
        w(C(n) + '\n')
    w('''  IdiomaManager "1" o-- "0..*" IObservadorIdioma : _observers
  IdiomaManager --> IDIOMA : IdiomaActivo >
  SessionManager --> USUARIO : usa >
}
''')
    # ----- DAL
    w(SEP + '\n\' CAPA DAL\n' + SEP)
    w('package "DAL" #DAE8FC {\n')
    for n in ('Acceso', 'UsuarioDAL', 'PerfilDAL', 'UsuarioPerfilDAL', 'BitacoraDAL', 'IntegridadDAL', 'IdiomaDAL', 'UsuarioHistorialDAL'):
        w(C(n) + '\n')
    w('  \' ── Dominio: catálogo y stock de vinos ──')
    for n in ('VinoDAL', 'SolicitudAutorizacionDAL', 'BodegaDAL', 'MovimientoStockDAL'):
        w(C(n) + '\n')
    base_d = ['UsuarioDAL', 'PerfilDAL', 'UsuarioPerfilDAL', 'BitacoraDAL', 'IntegridadDAL', 'IdiomaDAL', 'UsuarioHistorialDAL']
    for n in base_d:
        assert uses(n, 'Acceso')
        w(f'  {n} --> Acceso')
    for n in ('UsuarioDAL', 'BitacoraDAL', 'UsuarioHistorialDAL'):
        assert uses(n, 'PaginaResultado')
        w(f'  {n} {arrow(n, "PaginaResultado")} PaginaResultado')
    for n in ('VinoDAL', 'SolicitudAutorizacionDAL', 'BodegaDAL', 'MovimientoStockDAL'):
        assert uses(n, 'Acceso')
        w(f'  {n} --> Acceso')
    assert uses('VinoDAL', 'SolicitudAutorizacionDAL')
    w('  VinoDAL ..> SolicitudAutorizacionDAL : TipoDesdeDb >')
    w('\n  \' ── Dominio: curación y armado de cajas mensuales ──')
    for n in ('SocioDAL', 'CajaMensualDAL', 'SustitucionDAL'):
        w(C(n) + '\n')
    for n in ('SocioDAL', 'CajaMensualDAL', 'SustitucionDAL'):
        assert uses(n, 'Acceso')
        w(f'  {n} --> Acceso')
    w('\n  \' ── Dominio: rotación y pedido de reposición ──')
    for n in P3_DAL:
        w(C(n) + '\n')
    for n in P3_DAL:
        if uses(n, 'Acceso'):
            w(f'  {n} --> Acceso')
        for l in auto_rels(n, P3_BE + ['Vino', 'Bodega', 'MovimientoStock']):
            w('  ' + l)
    w('}\n')
    # ----- BLL
    w(SEP + '\n\' CAPA BLL\n' + SEP)
    w('package "BLL" #FFE6CC {\n')
    for n in ('ResultadoIntegridad', 'IntegridadBLL', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL', 'UsuarioHistorialBLL'):
        w(C(n) + '\n')
    w('  \' ── Dominio: catálogo y stock de vinos ──')
    for n in ('VinoBLL', 'MovimientoStockBLL'):
        w(C(n) + '\n')
    bll_pairs = []
    names_bll = ['RotacionBLL', 'PedidoReposicionBLL', 'ConexionBLL', 'IntegridadBLL', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL', 'UsuarioHistorialBLL', 'VinoBLL', 'MovimientoStockBLL', 'SocioBLL', 'CajaMensualBLL']
    targets = [n for n in DALS] + [n for n in names_bll]
    # Dependencias reales BLL -> DAL/BLL (campos), emitidas en orden de la clase origen
    def bll_rels(a):
        lines = []
        for b in sorted(targets, key=lambda x: (x not in DALS, x)):
            if b != a and uses(a, b):
                lines.append(f'  {a} {arrow(a, b)} {b}')
        return lines
    order1 = ['IntegridadBLL', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL', 'UsuarioHistorialBLL', 'VinoBLL', 'MovimientoStockBLL']
    for a in order1:
        for l in bll_rels(a):
            w(l)
    # Servicios transversales y tipos BE usados
    svc = []
    for a in order1 + ['SocioBLL', 'CajaMensualBLL']:
        for b in ('IdiomaManager',):
            if uses(a, b):
                svc.append(f'  {a} {arrow(a, b)} {b}')
        for b in ('SessionManager', 'Hasher', 'CalculadorDVH', 'ValidadorContrasena'):
            if uses(a, b):
                svc.append(f'  {a} {arrow(a, b)} {b}')
    for l in svc:
        w(l)
    for a, b in (('IntegridadBLL', 'ResultadoIntegridad'), ('LoginBLL', 'LoginResultado'), ('UsuarioBLL', 'PaginaResultado'), ('BitacoraBLL', 'PaginaResultado'), ('UsuarioHistorialBLL', 'PaginaResultado')):
        assert uses(a, b)
        w(f'  {a} {arrow(a, b)} {b}')
    w('\n  \' ── Dominio: curación y armado de cajas mensuales ──')
    for n in ('SocioBLL', 'CajaMensualBLL'):
        w(C(n) + '\n')
    for a in ('SocioBLL', 'CajaMensualBLL'):
        for l in bll_rels(a):
            w(l)
    w('\n  \' ── Dominio: rotación y pedido de reposición ──')
    for n in P3_BLL:
        w(C(n) + '\n')
    for a in ('RotacionBLL', 'PedidoReposicionBLL', 'ConexionBLL'):
        for l in bll_rels(a):
            w(l)
        for b in ('SessionManager', 'IdiomaManager'):
            if uses(a, b):
                w(f'  {a} {arrow(a, b)} {b}')
        for l in auto_rels(a, P3_BE + ['Vino', 'Bodega', 'PedidoParcialException']):
            w('  ' + l)
    w('}\n')
    # ----- CAPAS
    w(SEP + '\n\' CAPA CAPAS (presentación)\n' + SEP)
    w('package "CAPAS" #F8CECC {\n')
    w(C('IdiomaUIHelper'))
    for n in ('FormBase',):
        w('\n' + C(n))
    w('\n  note right of FormBase : Hereda de ReaLTaiizor.Forms.MaterialForm\n')
    for n in ('MsgBox', 'FrmMensaje', 'Program', 'RemitoDespachoPdf'):
        w(C(n) + '\n')
    def tog(names):
        w('  together {')
        for n in names:
            w(C(n, indent='    '))
        w('  }\n')
    tog(['LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña'])
    tog(['frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas'])
    w(C('frmRestaurarIntegridad') + '\n')
    w(C('frmAyuda') + '\n')
    w('  \' ── Utilidades de ayuda y arranque ──')
    for n in ('AyudaContexto', 'AyudaRepositorio', 'AyudaTema', 'ErrorInicio'):
        w(C(n) + '\n')
    w('  \' ── Dominio: catálogo y stock de vinos ──')
    tog(['frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo'])
    w('  \' ── Dominio: curación y armado de cajas mensuales ──')
    tog(['frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos'])
    w('  \' ── Dominio: rotación y pedido de reposición ──')
    tog(P3_FORMS)
    for n in ('PedidoReposicionUI', 'ConfiguracionRotacionStore', 'RotacionGraficos', 'RotacionReposicionPdf'):
        w(C(n) + '\n')
    # herencia
    allforms = ['LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña', 'frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas', 'frmRestaurarIntegridad',
                'frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo',
                'frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos', 'frmAyuda'] + P3_FORMS
    for n in allforms + ['FrmMensaje']:
        assert 'FormBase' in str(by[n][0]['bases']) or any('FormBase' in str(t['bases']) for t in by[n]), n
        w(f'  FormBase <|-- {n}')
    w('')
    for n in allforms:
        b = ' '.join(str(t['bases']) for t in by[n])
        if 'IObservadorIdioma' in b:
            w(f'  IObservadorIdioma <|.. {n}')
    w('}\n')
    # ----- dependencias entre capas
    w(SEP + '\n\' Dependencias entre capas\n' + SEP)
    BLLSET = set(names_bll)
    grp = [
        ('Núcleo / seguridad', ['LogIn', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmContraseña', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas', 'frmHistorialUsuario', 'frmRestaurarIntegridad', 'frmMenu', 'IdiomaUIHelper', 'Program']),
        ('Dominio: catálogo y stock de vinos', ['frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo']),
        ('Dominio: curación y armado de cajas mensuales', ['frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos']),
        ('Dominio: rotación y pedido de reposición', P3_FORMS),
    ]
    for title, lst in grp:
        w('\' ' + title)
        for a in lst:
            for b in sorted(BLLSET):
                if uses(a, b):
                    w(f'{a} {arrow(a, b)} {b}')
            if a in ('LogIn', 'frmMenu') and uses(a, 'SessionManager'):
                w(f'{a} {arrow(a, "SessionManager")} SessionManager')
            if uses(a, 'IdiomaManager') and re.search(r'CambiarIdioma\(', deps_text(a)):
                w(f'{a} {arrow(a, "IdiomaManager")} IdiomaManager')
            if a not in ('Program', 'IdiomaUIHelper') and uses(a, 'ValidadorContrasena'):
                w(f'{a} {arrow(a, "ValidadorContrasena")} ValidadorContrasena')
            if uses(a, 'RemitoDespachoPdf'):
                w(f'{a} ..> RemitoDespachoPdf')
            for l in auto_rels(a, P3_HELPERS + P3_BE):
                w(l)
        w('')
    # navegacion y utilidades de UI
    nav = [('Program', 'LogIn'), ('LogIn', 'frmMenu'), ('frmMenu', 'LogIn'), ('frmRestaurarIntegridad', 'ResultadoIntegridad'), ('LogIn', 'frmRestaurarIntegridad'), ('frmRestaurarIntegridad', 'frmHistorialUsuario'),
           ('frmAdminUsuarios', 'frmNuevoUsuario'), ('frmAdminUsuarios', 'frmEditarUsuario'), ('frmAdminUsuarios', 'frmHistorialUsuario'), ('frmAdminUsuarios', 'frmAsignarPerfiles'),
           ('Program', 'ResultadoIntegridad'), ('Program', 'ErrorInicio'), ('frmMenu', 'frmAnalisisRotacion'), ('frmMenu', 'frmAutorizarPedidoReposicion'), ('frmMenu', 'frmRecibirPedidoReposicion'), ('frmMenu', 'frmConsultarPedidosReposicion'), ('frmAutorizarPedidoReposicion', 'FrmMotivoRechazoPedido'), ('FormBase', 'frmAyuda'), ('MsgBox', 'FrmMensaje')]
    w('\' Navegación entre formularios y utilidades de presentación')
    for a, b in nav:
        assert uses(a, b), (a, b)
        w(f'{a} {arrow(a, b)} {b}')
    w(FRMMENU_NOTE)

    # ----- layout
    w(LAYOUT)
    w('@enduml\n')
    text = layered('\n'.join(P))
    open(outpath, 'w', encoding='utf-8', newline='\n').write(text)

REL_RE = re.compile(r'^\s*(\w+)\s+(?:"[^"]*"\s+)?(<\|--|<\|\.\.|-->|\.\.>|o--|--\|>)\s+(?:"[^"]*"\s+)?(\w+)')
NORANK = {'-->': '-[norank]->', '..>': '.[norank].>', 'o--': 'o-[norank]-', '<|--': '<|-[norank]-', '<|..': '<|.[norank].', '--|>': '-[norank]-|>'}
PKG_ORDER = ['CAPAS', 'SeguridadYServicios', 'BLL', 'DAL', 'BE']

def layered(text):
    """Las relaciones entre clases de capas distintas se resumen como flechas entre paquetes
    (asi las capas quedan apiladas); las relaciones dentro de una misma capa se conservan."""
    out, pairs = [], {}
    for l in text.split('\n'):
        m = REL_RE.match(l)
        if m and m.group(1) in by and m.group(3) in by:
            la, lb = layer(m.group(1)), layer(m.group(3))
            if la != lb and m.group(2) == '<|..':
                # realizacion de IObservadorIdioma (Observer): se conserva entre capas contiguas
                out.append(l.replace(' <|.. ', ' <|.[norank]. ', 1))
                continue
            if la != lb:
                # herencia/realizacion (<|--): el dependiente es el hijo (segundo operando)
                src, dst = (lb, la) if m.group(2).startswith('<|') else (la, lb)
                pairs[(src, dst)] = pairs.get((src, dst), 0) + 1
                continue
            # relacion dentro de la capa: no influye en el rango (la grilla de la capa manda)
            tok = m.group(2)
            l = l.replace(' ' + tok + ' ', ' ' + NORANK[tok] + ' ', 1)
        out.append(l)
    arrows = ["' ═══ Dependencias entre capas (resumen de las relaciones entre clases de capas distintas) ═══"]
    for (a, b) in sorted(pairs, key=lambda k: (PKG_ORDER.index(k[0]), PKG_ORDER.index(k[1]))):
        d = 'down' if PKG_ORDER.index(a) < PKG_ORDER.index(b) else 'up'
        arrows.append('%s -%s-> %s : usa' % (a, d, b))
    text = '\n'.join(out)
    marker = "' ═══ DISPOSICIÓN VERTICAL"
    assert marker in text
    return text.replace(marker, '\n'.join(arrows) + '\n\n' + marker, 1)

import layout_gen
LAYOUT = layout_gen.build()

if __name__ == '__main__':
    main(sys.argv[1])
