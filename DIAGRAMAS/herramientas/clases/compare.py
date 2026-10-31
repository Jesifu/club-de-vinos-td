import re, os, sys
sys.path.insert(0, os.path.dirname(__file__))
from csparse import load_all, ROOT, split_top, norm_ws, parse_param
from pumlparse import parse_puml

DIAG = os.path.join(ROOT, 'DIAGRAMAS')
FILES = ['DiagramaClases', 'DiagramaClases_CatalogoStock', 'DiagramaClases_CuracionCajas',
         'DiagramaClases_Composite', 'DiagramaClases_Observer', 'DiagramaClases_Singleton']

allt, codes = load_all()
by_name = {}
for t in allt:
    by_name.setdefault(t['name'], []).append(t)

def rel(p): return os.path.relpath(p, ROOT).replace('\\', '/')

PRIM = {'String': 'string', 'Int32': 'int', 'Boolean': 'bool', 'Int64': 'long', 'Decimal': 'decimal', 'Double': 'double', 'Object': 'object', 'Void': 'void'}
def nt(t):
    t = (t or '').replace(' ', '')
    t = re.sub(r'\b(BE|DAL|BLL|SeguridadYServicios|CAPAS)\.', '', t)
    t = re.sub(r'\bSystem\.(Collections\.Generic\.|Data\.SqlClient\.|Data\.|Windows\.Forms\.)?', '', t)
    for k, v in PRIM.items(): t = re.sub(r'\b' + k + r'\b', v, t)
    return t

def merged(name):
    ts = by_name.get(name, [])
    return ts

def members_of(ts):
    res = []
    for t in ts:
        for m in t['members']:
            m = dict(m)
            if t['kind']=='interface' and m.get('vis')=='private': m['vis']='public'
            m['file'] = t['file']; m['designer'] = t['designer']; m['owner'] = t['name']
            res.append(m)
        for nt_ in t['nested']:
            pass
    return res

RAWCACHE = {}
def raw_line(m):
    f = m['file']
    if f not in RAWCACHE:
        RAWCACHE[f] = open(f, encoding='utf-8-sig', errors='replace').read().splitlines()
    return RAWCACHE[f][m.get('line', 1) - 1] if m.get('line') else ''
def raw_init(m):
    if m.get('init') is None: return None
    l = raw_line(m)
    mm = re.search(r'=\s*(.*?);\s*(//.*)?$', l)
    return mm.group(1) if mm else m['init']
def code_ref(m):
    return '%s:%s' % (rel(m['file']), m.get('line', '?'))

def is_handler(m):
    if m['kind'] != 'method': return False
    ps = m.get('params', [])
    return len(ps) == 2 and re.search(r'EventArgs$', ps[1]['type'] or '') is not None and (ps[0]['type'] in ('object', 'Object'))

def sig_code(m):
    ps = ', '.join(('%s %s' % (p['type'], p['name'])) + ((' = ' + p['default']) if p['default'] else '') for p in m['params'])
    return '%s%s %s(%s)' % ((m['vis'] + ' ') , ('static ' if m['static'] else '') + m['ret'], m['name'], ps)

def parse_dparams(s):
    out = []
    for p in split_top(s):
        p = p.strip()
        if not p: continue
        dflt = None
        if '=' in p:
            p, dflt = p.split('=', 1); p = p.strip(); dflt = dflt.strip()
        if ':' in p:
            n, t = p.split(':', 1)
            out.append({'name': n.strip(), 'type': t.strip(), 'default': dflt})
        else:
            out.append({'name': p, 'type': '', 'default': dflt})
    return out

VISMAP = {'+': 'public', '-': 'private', '#': 'protected', '~': 'internal'}

def type_status(dt, ct):
    a, b = nt(dt), nt(ct)
    if a == b: return True
    # allow nullable-ref equivalence for diagram '?' on ref types
    if a.rstrip('?') == b.rstrip('?') and not a.rstrip('?') in ('int', 'bool', 'decimal', 'DateTime', 'long', 'double'): return True
    return False

rows_all = {}
summary = {}

def add(rows, item, diag, code, status, fix):
    rows.append((item, diag, code, status, fix))

def stem(n): return re.sub(r'(DAL|BLL)$', '', n)
def other_class_with_method(name, exclude):
    found = []
    for t in allt:
        if t['name'] == exclude: continue
        for m in t['members']:
            if m['kind'] == 'method' and m['name'] == name:
                found.append(t['name'])
    return sorted(set(found))

CUR_RELS = []
GENERIC = set()
def compare_class(dname, d, dfile):
    rows = []
    ts = by_name.get(dname)
    note = ''
    codename = dname
    if not ts:
        ci = [k for k in by_name if k.lower() == dname.lower()]
        if ci:
            ts = by_name[ci[0]]; codename = ci[0]
            note = 'NAME_CASE'
    if not ts:
        return None, rows, None
    mem = members_of(ts)
    # type-level
    t0 = [t for t in ts if not t['designer']][0] if [t for t in ts if not t['designer']] else ts[0]
    kind_code = t0['kind']
    mods = set(sum([t['mods'] for t in ts], []))
    kd = d['kind']; st = d['stereo']
    kcode_full = ('abstract class' if 'abstract' in mods and kind_code == 'class' else kind_code)
    if 'static' in mods: kcode_full = 'static class'
    if 'sealed' in mods: kcode_full += ' (sealed)'
    # kind check
    ok = True
    exp = kd
    if kd.startswith('abstract') and 'abstract' not in mods: ok = False
    if kd == 'class' and kind_code != 'class': ok = False
    if kd == 'enum' and kind_code != 'enum': ok = False
    if kd == 'interface' and kind_code != 'interface': ok = False
    stero_static = 'static' in st.lower()
    if stero_static and 'static' not in mods: ok = False
    if (not stero_static) and 'static' in mods and kind_code == 'class': ok = False
    if 'internal' in st.lower() and not ('internal' in mods or t0.get('mods') == [] ): pass
    add(rows, '[tipo] %s' % dname, '%s %s%s' % (kd, dname, (' ' + st) if st else ''), '%s %s (%s) %s:%s' % (' '.join(sorted(mods)) or '(sin modificadores = internal)', kcode_full, ', '.join(t['file'].split('\\')[-1] for t in ts), rel(t0['file']), t0['line']) + ('; base/ifaces: ' + str(t0['bases']) if t0['bases'] else ''), 'OK' if ok and not note else ('NAME_MISMATCH' if note else 'KIND_MISMATCH'), '' if ok and not note else ('Renombrar en diagrama a `%s`' % codename if note else 'Ajustar tipo/estereotipo a `%s`' % kcode_full))
    # paquete vs namespace
    nsc = t0.get('ns')
    folder = rel(t0['file']).split('/')[0]
    pk = d['pkg']
    okp = (pk == nsc) or (pk == folder)
    if pk is None:
        add(rows, '[paquete]', '(fuera de todo package)', 'namespace %s (carpeta/proyecto %s)' % (nsc, folder), 'PACKAGE_MISSING', 'Envolver en package "%s" (cliente dibujado suelto)' % (nsc or folder))
    else:
        pass
    if pk is not None: add(rows, '[paquete]', 'package "%s"' % pk, 'namespace %s (carpeta/proyecto %s)' % (nsc, folder), 'OK' if okp else 'WRONG_PACKAGE', '' if okp else 'Mover la clase al paquete `%s`' % (nsc or folder))
    # herencia / interfaces
    if t0['kind'] != 'enum':
        bases = []
        for t in ts:
            if t['bases']:
                bases += [norm_ws(b) for b in split_top(t['bases'])]
        bases = [re.sub(r'^.*\.', '', b) for b in bases]
        for b in sorted(set(bases)):
            drawn = [r for r in CUR_RELS if ((r['a'] == b and r['b'] == dname and r['arrow'] in ('<|--', '<|..')) or (r['b'] == b and r['a'] == dname and r['arrow'] in ('--|>', '..|>')))]
            known = b in by_name
            add(rows, '[hereda/implementa] ' + b, 'dibujado' if drawn else '-', 'en código: %s : %s (%s)' % (dname, b, 'tipo del proyecto' if known else 'base externa de .NET/MaterialSkin'), 'OK' if drawn else 'NOT_IN_DIAGRAM [herencia]', '' if drawn else ('Agregar `%s %s %s`' % (b, '<|..' if known and by_name[b][0]['kind'] == 'interface' else '<|--', dname) if known else 'Agregar estereotipo/nota "extiende %s" (opcional, clase externa)' % b))
    for t in ts:
        for nn in t['nested']:
            add(rows, '[tipo anidado] ' + nn['name'], '-', '%s %s anidado en %s, %s:%s' % (' '.join(nn['mods']), nn['kind'], dname, rel(t['file']), nn['line']), 'NOT_IN_DIAGRAM [tipo anidado ' + ('private' if 'private' in nn['mods'] else 'internal') + ']', 'Opcional (tipo anidado)')
    if d['kind'] == 'enum' or kind_code == 'enum':
        dvals = [m['name'] for m in d['members'] if m['kind'] == 'enumval']
        cvals = [m['name'] for m in mem if m['kind'] == 'enumval']
        for v in dvals:
            add(rows, 'valor %s' % v, v, ('presente' if v in cvals else '-'), 'OK' if v in cvals else 'MISSING_IN_CODE', '' if v in cvals else 'Quitar `%s` del diagrama' % v)
        for v in cvals:
            if v not in dvals:
                add(rows, 'valor %s' % v, '-', 'presente', 'NOT_IN_DIAGRAM', 'Agregar `%s`' % v)
        if dvals != [v for v in cvals if v in dvals]: add(rows, 'orden valores', ','.join(dvals), ','.join(cvals), 'ORDER', 'Reordenar (informativo)')
        return codename, rows, ts
    used = set()
    # attrs
    dattrs = [m for m in d['members'] if m['kind'] == 'attr']
    dmeth = [m for m in d['members'] if m['kind'] == 'method']
    for a in dattrs:
        cands = [m for m in mem if m['kind'] in ('field_or_prop', 'property') and m['name'] == a['name']]
        dtxt = '%s %s%s : %s%s' % (a['vis'], ''.join(x + ' ' for x in a['mods']), a['name'], a['type'], (' = ' + a['init']) if a['init'] else '')
        if not cands:
            # maybe a method with same name or lives elsewhere
            oth = [t['name'] for t in allt if t['name'] != codename and any(m['kind'] in ('field_or_prop', 'property') and m['name'] == a['name'] for m in t['members'])]
            asmeth = [m for m in mem if m['kind'] == 'method' and m['name'] == a['name']]
            if asmeth:
                add(rows, a['name'], dtxt, 'es MÉTODO: ' + sig_code(asmeth[0]) + ' ' + code_ref(asmeth[0]), 'KIND_MISMATCH', 'Dibujar como método')
            elif oth:
                add(rows, a['name'], dtxt, 'no existe en esta clase (el mismo nombre existe en: ' + ', '.join(oth) + ')', 'MISSING_IN_CODE', 'Quitar `%s` del diagrama' % a['name'])
            else:
                add(rows, a['name'], dtxt, '-', 'MISSING_IN_CODE', 'Quitar `%s` del diagrama' % a['name'])
            continue
        m = cands[0]; used.add(id(m))
        probs = []
        if not type_status(a['type'], m['type']): probs.append('tipo: diagrama `%s` vs código `%s`' % (a['type'], m['type']))
        dv = VISMAP[a['vis']]
        if dv != m['vis']: probs.append('visibilidad: diagrama %s vs código %s' % (dv, m['vis']))
        ds = '{static}' in a['mods']
        if ds != m['static']: probs.append('static: diagrama %s vs código %s' % (ds, m['static']))
        ctxt = '%s %s%s%s %s : %s%s [%s] %s' % (m['vis'], 'static ' if 'static' in m['mods'] else '', 'readonly ' if m['readonly'] and not m.get('const') else ('const ' if m.get('const') else ''), '', m['name'], m['type'], (' = ' + raw_init(m)) if m.get('init') else '', 'prop' if m['kind'] == 'property' else 'field', code_ref(m))
        if a['init'] and not (m.get('init') and nt(a['init']) == nt(raw_init(m))):
            probs.append('valor inicial: diagrama `%s` vs código `%s`' % (a['init'], raw_init(m)))
        add(rows, a['name'], dtxt, ctxt, 'OK' if not probs else ('TYPE_MISMATCH' if any(p.startswith('tipo') for p in probs) else 'SIGNATURE_MISMATCH'), '; '.join(probs))
    # methods
    for dm in dmeth:
        dp = parse_dparams(dm['params'])
        cands = [m for m in mem if m['kind'] in ('method', 'ctor') and m['name'] == dm['name']]
        dtxt = '%s %s%s(%s)%s' % (dm['vis'], ''.join(x + ' ' for x in dm['mods']), dm['name'], dm['params'], (' : ' + dm['ret']) if dm['ret'] else '')
        if not cands:
            oth = other_class_with_method(dm['name'], codename)
            oths = [o for o in oth if stem(o) == stem(codename)]
            if oths:
                add(rows, dm['name'] + '()', dtxt, 'no declarado en esta clase; está en la misma entidad de otra capa: ' + ', '.join(oths), 'WRONG_CLASS', 'Quitar de `%s` y dibujarlo en %s' % (dname, ', '.join(oths)))
            elif oth:
                add(rows, dm['name'] + '()', dtxt, 'no existe en esta clase (el mismo nombre existe solo en clases de otras entidades: ' + ', '.join(oth) + ')', 'MISSING_IN_CODE', 'Quitar `%s()` del diagrama' % dm['name'])
            else:
                # maybe property
                pm = [m for m in mem if m['kind'] in ('property', 'field_or_prop') and m['name'] == dm['name']]
                if pm:
                    add(rows, dm['name'] + '()', dtxt, 'es PROPIEDAD/CAMPO: %s %s %s' % (pm[0]['vis'], pm[0]['type'], code_ref(pm[0])), 'KIND_MISMATCH', 'Dibujar como atributo')
                else:
                    add(rows, dm['name'] + '()', dtxt, '-', 'MISSING_IN_CODE', 'Quitar del diagrama')
            continue
        # choose best by param count
        same = [m for m in cands if len(m['params']) == len(dp)]
        # try match by types if multiple
        best = None
        if same:
            if len(same) > 1:
                for m in same:
                    if [nt(p['type']) for p in m['params']] == [nt(p['type']) for p in dp] and id(m) not in used:
                        best = m; break
            if best is None:
                free = [m for m in same if id(m) not in used]
                best = (free or same)[0]
        else:
            # handler convention: diagram without params but code (object, EventArgs)
            free = [m for m in cands if id(m) not in used]
            best = (free or cands)[0]
        used.add(id(best))
        m = best
        probs = []
        if len(m['params']) != len(dp):
            if is_handler(m) and len(dp) == 0:
                probs.append('handler dibujado sin parámetros; código `(%s)`' % ', '.join('%s %s' % (p['type'], p['name']) for p in m['params']))
            else:
                probs.append('cantidad de parámetros: diagrama %d vs código %d' % (len(dp), len(m['params'])))
        else:
            for i, (p, q) in enumerate(zip(dp, m['params'])):
                if p['name'] != q['name']: probs.append('param %d nombre: `%s` vs `%s`' % (i+1, p['name'], q['name']))
                if not type_status(p['type'], q['type']): probs.append('param %d `%s` tipo: `%s` vs `%s`' % (i+1, q['name'], p['type'], q['type']))
                if (p['default'] is None) != (q['default'] is None): probs.append('param %d `%s` valor por defecto: diagrama `%s` vs código `%s`' % (i+1, q['name'], p['default'] if p['default'] is not None else '(sin default)', q['default'] if q['default'] is not None else '(sin default)'))
        if m['kind'] == 'method':
            if dm['ret'] == '' and m['ret'] == 'void': probs.append('retorno omitido en diagrama (código void)') if False else None
            elif dm['ret'] == '' and m['ret'] != 'void': probs.append('retorno omitido; código `%s`' % m['ret'])
            elif dm['ret'] and not type_status(dm['ret'], m['ret']): probs.append('retorno: diagrama `%s` vs código `%s`' % (dm['ret'], m['ret']))
        if VISMAP[dm['vis']] != m['vis']: probs.append('visibilidad: diagrama %s vs código %s' % (VISMAP[dm['vis']], m['vis']))
        if ('{static}' in dm['mods']) != m['static']: probs.append('static: diagrama %s vs código %s' % ('{static}' in dm['mods'], m['static']))
        if ('{abstract}' in dm['mods']) != m['abstract']: probs.append('abstract: diagrama %s vs código %s' % ('{abstract}' in dm['mods'], m['abstract']))
        ctxt = sig_code(m) + ' ' + code_ref(m) + (' [designer]' if m['designer'] else '')
        st_ = 'OK' if not probs else 'SIGNATURE_MISMATCH'
        add(rows, dm['name'] + '()', dtxt, ctxt, st_, '; '.join(probs))
    # raw members
    for rm in [m for m in d['members'] if m['kind'] == 'raw']:
        add(rows, 'línea cruda', rm['raw'], '-', 'REVIEW', 'Revisar sintaxis')
    # code members not in diagram
    dnames = {m['name'] for m in d['members'] if m['kind'] in ('attr', 'method')}
    for m in mem:
        if id(m) in used: continue
        if m['kind'] in ('field_or_prop', 'property', 'method', 'ctor', 'event', 'indexer', 'operator', 'dtor'):
            if m['kind'] in ('field_or_prop', 'property') and m['name'] in dnames and any(x['kind'] == 'attr' and x['name'] == m['name'] for x in d['members']): continue
            if m['kind'] in ('method', 'ctor') and any(x['kind'] == 'method' and x['name'] == m['name'] for x in d['members']) and id(m) not in used:
                tag = 'overload no dibujado'
            else:
                tag = None
            if m['designer']:
                tag = 'designer'
            elif m['kind'] == 'ctor':
                tag = 'constructor ' + m['vis']
            elif m['kind'] == 'method' and is_handler(m):
                tag = 'event handler ' + m['vis']
            elif m['kind'] == 'method':
                tag = tag or ('método ' + ('public' if m['vis'] == 'public' else m['vis'] + ' helper'))
            elif m['kind'] in ('field_or_prop', 'property', 'event'):
                tag = tag or (('propiedad ' if m['kind'] == 'property' else 'campo ') + m['vis'])
            if m['kind'] in ('method', 'ctor'):
                txt = sig_code(m)
            else:
                txt = '%s %s%s%s : %s' % (m['vis'], 'static ' if 'static' in m['mods'] else '', 'const ' if m.get('const') else '', m['name'], m['type'])
            add(rows, m['name'], '-', txt + ' ' + code_ref(m), 'NOT_IN_DIAGRAM [%s]' % tag, 'Agregar al diagrama' if tag and tag.split()[0] in ('método', 'propiedad', 'campo') and 'public' in tag else 'Opcional (%s)' % tag)
    return codename, rows, ts

def code_deps(ts):
    txt = ''
    for t in ts:
        s = codes[t['file']]
        if len(t.get('span', ())) == 2 and sum(1 for x in allt if x['file'] == t['file'] and not x['designer']) > 1:
            txt += s[t['span'][0]:t['span'][1]]
        else:
            txt += s
    return txt

def main():
    out = []
    stats = {}
    allclasses_covered = set()
    for f in FILES:
        classes, rels = parse_puml(os.path.join(DIAG, f + '.puml'))
        CUR_RELS[:] = rels
        out.append('\n\n# DIAGRAMA: %s.puml (%d clases, %d relaciones)\n' % (f, len(classes), len(rels)))
        cnt = {'OK': 0, 'bad': 0, 'notin': 0}
        classes_missing = []
        for name, d in classes.items():
            key = d['label'] and name or name
            cname, rows, ts = compare_class(name, d, f)
            pkgtxt = d['pkg']
            if ts is None:
                alt = None
                if d['label']:
                    out.append('\n## `%s` [alias "%s"] (%s %s, línea %d, paquete %s)\n\nClase GENÉRICA/representativa que agrupa varios forms (no es una clase real). Sus miembros se verifican en la sección "Verificación de patrones sobre forms". Estado: **REVIEW** (el nombre genérico no existe como clase).\n' % (name, d['label'], d['kind'], d['stereo'], d['line'], pkgtxt))
                    classes_missing.append(name + ' (alias genérico)')
                    cnt['bad'] += 1
                    GENERIC.add(name)
                    continue
                out.append('\n## `%s` (%s %s, línea %d, paquete %s)\n\nNO EXISTE EN EL CÓDIGO (ninguna clase/enum/interface con ese nombre). Estado: **MISSING_IN_CODE** (CLASE). Corrección en diagrama: quitar la clase o renombrar.\n' % (name + (' ["%s"]' % d['label'] if d['label'] else ''), d['kind'], d['stereo'], d['line'], pkgtxt))
                classes_missing.append(name)
                cnt['bad'] += 1
                continue
            allclasses_covered.add(cname)
            t0 = ts[0]
            out.append('\n## `%s` -> código `%s` (%s, línea %d del puml, paquete %s; namespace %s)\n' % (name, cname, d['kind'] + (' ' + d['stereo'] if d['stereo'] else ''), d['line'], pkgtxt, t0.get('ns')))
            out.append('| ítem | diagrama dice | código dice (archivo:línea) | estado | corrección al DIAGRAMA |')
            out.append('|---|---|---|---|---|')
            for r in rows:
                out.append('| ' + ' | '.join(str(x).replace('|', '\\|') for x in r) + ' |')
                s = r[3]
                if s == 'OK': cnt['OK'] += 1
                elif s.startswith('NOT_IN_DIAGRAM'): cnt['notin'] += 1
                else: cnt['bad'] += 1
                stats.setdefault((f, s.split(' ')[0]), 0)
                stats[(f, s.split(' ')[0])] += 1
        # relationships
        out.append('\n## Relaciones de %s\n' % f)
        out.append('| línea | relación dibujada | verificación en código | estado | corrección |')
        out.append('|---|---|---|---|---|')
        for r in rels:
            a, b, ar = r['a'], r['b'], r['arrow']
            desc = '%s %s%s %s%s %s%s' % (a, ('"%s" ' % r['ma']) if r['ma'] else '', ar, ('"%s" ' % r['mb']) if r['mb'] else '', '', b, (' : ' + r['label']) if r['label'] else '')
            ta = classes.get(a); tb = classes.get(b)
            if re.match(r'^N_\w+$', a) or re.match(r'^N_\w+$', b):
                out.append('| %d | `%s` | conexión de nota (no es relación entre clases) | N/A | - |' % (r['line'], desc.replace('|', '\\|')))
                continue
            ca = a if a in by_name else next((k for k in by_name if k.lower() == a.lower()), None)
            cb = b if b in by_name else next((k for k in by_name if k.lower() == b.lower()), None)
            status = 'OK'; ver = ''; fix = ''
            if a in GENERIC or b in GENERIC:
                status = 'REVIEW'; ver = 'una de las puntas es una clase genérica (alias de varios forms); ver sección patrones'; fix = 'Aceptable como representación; opcional dibujar los forms reales'
            elif ca is None or cb is None:
                miss = [x for x, c in ((a, ca), (b, cb)) if c is None]
                status = 'MISSING_IN_CODE'; ver = 'clase(s) sin equivalente en código: ' + ', '.join(miss); fix = 'Quitar relación (o corregir nombres)'
            else:
                if ar in ('<|--', '--|>', '<|..', '..|>'):
                    parent, child = (a, b) if ar in ('<|--', '<|..') else (b, a)
                    cp = parent if parent in by_name else None
                    cc = child if child in by_name else next((k for k in by_name if k.lower() == child.lower()), child)
                    cpp = parent if parent in by_name else next((k for k in by_name if k.lower() == parent.lower()), parent)
                    bases = ' '.join(str(t['bases']) for t in by_name.get(cc, []))
                    ok = re.search(r'\b' + re.escape(cpp) + r'\b', bases) is not None
                    ver = 'bases de %s en código: `%s`' % (cc, bases.strip())
                    if not ok: status = 'MISSING_IN_CODE'; fix = 'La clase %s no hereda/implementa %s: quitar relación' % (cc, cpp)
                else:
                    txt = code_deps(by_name[ca])
                    hits = len(re.findall(r'\b' + re.escape(cb) + r'\b', txt))
                    # field/prop types
                    ftypes = []
                    for t in by_name[ca]:
                        for m in t['members']:
                            if m['kind'] in ('field_or_prop', 'property') and re.search(r'\b' + re.escape(cb) + r'\b', m.get('type', '')):
                                ftypes.append('%s %s:%s' % (m['name'], m['type'], m.get('line')))
                    ver = 'referencias a %s en código de %s: %d; campos/props tipados: %s' % (cb, ca, hits, '; '.join(ftypes) or 'ninguno')
                    if hits == 0 and not (r['label'] and r['label'].strip().endswith('>')):
                        status = 'MISSING_IN_CODE'; fix = 'Código de %s no usa %s: quitar la flecha' % (ca, cb)
                    if r['label'] and re.match(r'^\w+Id >$', r['label'].strip()):
                        fk = r['label'].strip()[:-1].strip()
                        # FK is in many side
                        many = b if (ar in ('o--', '*--') and True) else None
                        # find FK in either
                        found = [x for x in (ca, cb) if any(m['name'] == fk for t in by_name[x] for m in t['members'])]
                        ver += '; FK `%s` presente en: %s' % (fk, ', '.join(found) or 'ninguna')
                        if not found: status = 'MISSING_IN_CODE'; fix = 'FK `%s` no existe' % fk
                    if ar in ('o--', '*--') and not ftypes:
                        # aggregation: check collection member of 'whole' side referencing part
                        ver += ' [AGREGACIÓN sin colección/ref en el lado "todo"]'
                        if status == 'OK': status = 'REVIEW'; fix = 'Agregación/composición no se materializa como campo en %s; se cumple solo por FK. Evaluar cambiar a asociación (-->) con etiqueta de FK' % ca
            out.append('| %d | `%s` | %s | %s | %s |' % (r['line'], desc.replace('|', '\\|'), ver.replace('|', '\\|'), status, fix))
            stats.setdefault((f, 'REL_' + status), 0); stats[(f, 'REL_' + status)] += 1
        # missing dependencies in diagram vs code (among classes present in diagram)
        out.append('\n### Dependencias presentes en código entre clases de este diagrama y NO dibujadas (%s)\n' % f)
        out.append('| origen | destino | evidencia en código |')
        out.append('|---|---|---|')
        drawn = set()
        for r in rels:
            drawn.add((r['a'], r['b'])); drawn.add((r['b'], r['a']))
        names = [n for n in classes]
        cn = {}
        for n in names:
            c = n if n in by_name else next((k for k in by_name if k.lower() == n.lower()), None)
            if c: cn[n] = c
        for a in names:
            if a not in cn: continue
            if classes[a]['kind'] == 'enum': continue
            txt = code_deps(by_name[cn[a]])
            for b in names:
                if b == a or b not in cn: continue
                if (a, b) in drawn: continue
                if re.search(r'\b' + re.escape(cn[b]) + r'\b', txt):
                    # skip enum-use for tiny noise? keep, flagged
                    n = len(re.findall(r'\b' + re.escape(cn[b]) + r'\b', txt))
                    out.append('| %s | %s | %d referencias (%s) |' % (a, b, n, classes[b]['kind']))
        out.append('\nClases del diagrama sin equivalente en el código: %s\n' % (', '.join(classes_missing) or 'ninguna'))
    # code classes not in any diagram
    return out, stats, allclasses_covered

if __name__ == '__main__':
    out, stats, cov = main()
    open(sys.argv[1], 'w', encoding='utf-8').write('\n'.join(out))
    for k in sorted(stats): print(k, stats[k])
    notcov = sorted(set(by_name) - cov)
    print('CODE TYPES NOT IN ANY CLASS DIAGRAM:', notcov)
