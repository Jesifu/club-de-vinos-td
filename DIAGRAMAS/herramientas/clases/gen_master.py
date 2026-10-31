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
    'AppTheme': STATIC, 'MsgBox': STATIC, 'RemitoDespachoPdf': STATIC, 'Program': STATIC,
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
    names_bll = ['IntegridadBLL', 'UsuarioBLL', 'LoginBLL', 'PerfilBLL', 'UsuarioPerfilBLL', 'BitacoraBLL', 'IdiomaBLL', 'UsuarioHistorialBLL', 'VinoBLL', 'MovimientoStockBLL', 'SocioBLL', 'CajaMensualBLL']
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
    w('}\n')
    # ----- CAPAS
    w(SEP + '\n\' CAPA CAPAS (presentación)\n' + SEP)
    w('package "CAPAS" #F8CECC {\n')
    w(C('IdiomaUIHelper'))
    for n in ('FormBase',):
        w('\n' + C(n))
    w('\n  note right of FormBase : Hereda de ReaLTaiizor.Forms.MaterialForm\n')
    for n in ('AppTheme', 'MsgBox', 'FrmMensaje', 'Program', 'RemitoDespachoPdf'):
        w(C(n) + '\n')
    def tog(names):
        w('  together {')
        for n in names:
            w(C(n, indent='    '))
        w('  }\n')
    tog(['LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña'])
    tog(['frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas'])
    w(C('frmRestaurarIntegridad') + '\n')
    w('  \' ── Dominio: catálogo y stock de vinos ──')
    tog(['frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo'])
    w('  \' ── Dominio: curación y armado de cajas mensuales ──')
    tog(['frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos'])
    # herencia
    allforms = ['LogIn', 'frmMenu', 'frmAdminUsuarios', 'frmNuevoUsuario', 'frmEditarUsuario', 'frmContraseña', 'frmHistorialUsuario', 'frmBitacora', 'frmPerfiles', 'frmAsignarPerfiles', 'frmIdiomas', 'frmRestaurarIntegridad',
                'frmCatalogoVinos', 'frmAutorizarAltaVino', 'frmAutorizarBajaVino', 'frmRegistrarEntradaStock', 'frmRegistrarAjusteInventario', 'frmAlertaStockMinimo',
                'frmSocios', 'frmArmarCaja', 'frmPickingCaja', 'frmDespacharCaja', 'frmHistorialDespachos']
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
        w('')
    # navegacion y utilidades de UI
    nav = [('Program', 'LogIn'), ('LogIn', 'frmMenu'), ('frmMenu', 'LogIn'), ('frmRestaurarIntegridad', 'ResultadoIntegridad'), ('LogIn', 'frmRestaurarIntegridad'), ('frmRestaurarIntegridad', 'frmHistorialUsuario'),
           ('frmAdminUsuarios', 'frmNuevoUsuario'), ('frmAdminUsuarios', 'frmEditarUsuario'), ('frmAdminUsuarios', 'frmHistorialUsuario'), ('frmAdminUsuarios', 'frmAsignarPerfiles'),
           ('Program', 'ResultadoIntegridad'), ('Program', 'AppTheme'), ('FormBase', 'AppTheme'), ('MsgBox', 'FrmMensaje')]
    w('\' Navegación entre formularios y utilidades de presentación')
    for a, b in nav:
        assert uses(a, b), (a, b)
        w(f'{a} {arrow(a, b)} {b}')
    w(FRMMENU_NOTE)

    # ----- layout
    w(LAYOUT)
    w('@enduml\n')
    open(outpath, 'w', encoding='utf-8', newline='\n').write('\n'.join(P))

import layout_gen
LAYOUT = layout_gen.build()

if __name__ == '__main__':
    main(sys.argv[1])
