import re, os, sys, json

import pathlib
ROOT = str(pathlib.Path(__file__).resolve().parents[3])  # raiz del repo (clases -> herramientas -> DIAGRAMAS -> repo)
MODS = {'public','private','protected','internal','static','readonly','abstract','virtual','override','sealed','async','new','const','partial','extern','unsafe','volatile'}

def strip_code(src):
    """Remove comments and string/char literal contents, preserving newlines and offsets."""
    out = []
    i = 0
    n = len(src)
    while i < n:
        c = src[i]
        two = src[i:i+2]
        if two == '//':
            while i < n and src[i] != '\n':
                out.append(' '); i += 1
        elif two == '/*':
            j = src.find('*/', i+2)
            j = n if j < 0 else j+2
            for ch in src[i:j]:
                out.append('\n' if ch == '\n' else ' ')
            i = j
        elif c == '@' and src[i+1:i+2] == '"' or (two in ('$@', '@$') and src[i+2:i+3] == '"'):
            k = i
            while src[k] != '"': k += 1
            out.append('"'); k += 1
            while k < n:
                if src[k] == '"':
                    if src[k+1:k+2] == '"':
                        out.append(' '); out.append(' '); k += 2; continue
                    break
                out.append('\n' if src[k] == '\n' else ' ')
                k += 1
            out.append('"'); i = k+1
        elif c == '$' and src[i+1:i+2] == '"' or c == '"':
            k = i + (1 if c == '$' else 0)
            out.append('"'); k += 1
            while k < n and src[k] != '"':
                if src[k] == '\\':
                    out.append(' '); k += 1
                out.append(' '); k += 1
            out.append('"'); i = k+1
        elif c == "'":
            k = i+1
            out.append("'")
            while k < n and src[k] != "'":
                if src[k] == '\\': out.append(' '); k += 1
                out.append(' '); k += 1
            out.append("'"); i = k+1
        else:
            out.append(c); i += 1
    return ''.join(out)

def match_brace(s, i):
    """s[i]=='{' -> index of matching '}'"""
    d = 0
    n = len(s)
    while i < n:
        if s[i] == '{': d += 1
        elif s[i] == '}':
            d -= 1
            if d == 0: return i
        i += 1
    return n-1

def split_top(s, sep=','):
    parts = []; d = 0; cur = []
    for ch in s:
        if ch in '<([{': d += 1
        elif ch in '>)]}': d -= 1
        if ch == sep and d == 0:
            parts.append(''.join(cur)); cur = []
        else: cur.append(ch)
    if ''.join(cur).strip(): parts.append(''.join(cur))
    return parts

def norm_ws(s): return re.sub(r'\s+', ' ', s).strip()

def parse_param(p):
    p = norm_ws(p)
    p = re.sub(r'^\[[^\]]*\]\s*', '', p)
    default = None
    # find = at depth 0
    d = 0
    for idx, ch in enumerate(p):
        if ch in '<([': d += 1
        elif ch in '>)]': d -= 1
        elif ch == '=' and d == 0:
            default = p[idx+1:].strip(); p = p[:idx].strip(); break
    toks = p.rsplit(' ', 1)
    mods = ''
    if len(toks) == 2:
        typ, name = toks
    else:
        typ, name = '', toks[0]
    m = re.match(r'^((?:this|ref|out|params|in)\s+)+', typ)
    if m:
        mods = m.group(0).strip(); typ = typ[m.end():]
    return {'type': typ.strip(), 'name': name, 'default': default, 'mods': mods}

def parse_header(h, tname):
    h = norm_ws(h)
    # strip attributes
    while h.startswith('['):
        d = 0
        for idx, ch in enumerate(h):
            if ch == '[': d += 1
            elif ch == ']':
                d -= 1
                if d == 0:
                    h = h[idx+1:].strip(); break
        else: break
    if not h: return None
    # explicit interface / operators / events
    kind = None
    # find '(' at angle depth 0 first
    mods = []
    toks_probe = h
    while True:
        m = re.match(r'^(\w+)\s+', toks_probe)
        if m and m.group(1) in MODS:
            mods.append(m.group(1)); toks_probe = toks_probe[m.end():]
        else: break
    rest = toks_probe
    isevent = False
    if rest.startswith('event '):
        isevent = True; rest = rest[6:]
    # '(' position
    d = 0; ppos = -1
    for idx, ch in enumerate(rest):
        if ch == '<': d += 1
        elif ch == '>': d -= 1
        elif ch == '=' and d == 0:
            break
        elif ch == '(' and d == 0:
            ppos = idx; break
    vis = 'private'
    for v in ('public','protected','internal','private'):
        if v in mods: vis = v; break
    if 'protected' in mods and 'internal' in mods: vis = 'protected internal'
    base = {'mods': mods, 'vis': vis, 'static': ('static' in mods) or ('const' in mods), 'abstract': 'abstract' in mods, 'readonly': 'readonly' in mods or 'const' in mods, 'override': 'override' in mods, 'const': 'const' in mods}
    if ppos >= 0:
        pre = rest[:ppos].strip()
        close = ppos; dd = 0
        for idx in range(ppos, len(rest)):  # parentesis que cierra la lista de parametros (no el de ': base(...)')
            if rest[idx] == '(': dd += 1
            elif rest[idx] == ')':
                dd -= 1
                if dd == 0: close = idx; break
        else:
            close = rest.rfind(')')
        params = [parse_param(p) for p in split_top(rest[ppos+1:close])]
        # pre = "ret Name" or "Name" (ctor) or "ret Iface.Name"
        pm = re.match(r'^(.*?)\s*([\w\.]+(?:<[^>]*>)?)$', pre)
        if not pm: return {'kind': 'unknown', 'raw': h, **base}
        ret, name = pm.group(1).strip(), pm.group(2)
        if ret == '' and re.sub(r'<.*','',name) == tname:
            return {'kind': 'ctor', 'name': name, 'params': params, 'ret': '', **base}
        if ret == '' and name.startswith('~'):
            return {'kind': 'dtor', 'name': name, 'params': params, 'ret': '', **base}
        if pre.startswith('operator') or ' operator ' in pre:
            return {'kind': 'operator', 'name': pre, 'params': params, 'ret': ret, **base}
        return {'kind': 'method', 'name': name, 'params': params, 'ret': ret, **base}
    # property / field
    d2 = 0; eq = -1
    for idx, ch in enumerate(rest):
        if ch in '<([': d2 += 1
        elif ch in '>)]': d2 -= 1
        elif ch == '=' and d2 == 0 and rest[idx+1:idx+2] != '>':
            eq = idx; break
    decl = rest if eq < 0 else rest[:eq]
    init = None if eq < 0 else rest[eq+1:].strip()
    decl = decl.strip()
    if 'this[' in decl or decl.endswith(']') and ' this' in decl:
        return {'kind': 'indexer', 'name': decl, 'type': '', **base}
    pm = re.match(r'^(.*?)\s+([\w\.@]+)$', decl)
    if not pm:
        return {'kind': 'unknown', 'raw': h, **base}
    typ, name = pm.group(1).strip(), pm.group(2)
    return {'kind': 'event' if isevent else 'field_or_prop', 'name': name, 'type': typ, 'init': init, **base}

TYPE_RE = re.compile(r'^((?:\[[^\]]*\]\s*)*)((?:(?:public|private|protected|internal|static|abstract|sealed|partial|unsafe|new)\s+)*)(class|struct|interface|enum)\s+(\w+)(?:<[^>{]*>)?\s*(?::\s*([^{]+?))?\s*(where\s+[^{]+)?$', re.S)

def parse_body(body, tname, kind, file, base_off, src_lines_off):
    """return members, nested types"""
    members = []; nested = []
    if kind == 'enum':
        items = [norm_ws(x) for x in split_top(body)]
        for it in items:
            it = re.sub(r'^\[[^\]]*\]\s*', '', it)
            nm = it.split('=')[0].strip()
            if nm: members.append({'kind': 'enumval', 'name': nm, 'line': 0})
        return members, nested
    i = 0; n = len(body); buf_start = 0
    def line_at(pos):
        return src_lines_off(base_off + pos)
    while i < n:
        ch = body[i]
        if ch == ';':
            h = body[buf_start:i]
            if h.strip():
                m = parse_header(h, tname)
                if m and not h.strip().startswith('='):
                    m['line'] = line_at(buf_start + len(h) - len(h.lstrip()))
                    members.append(m)
            i += 1; buf_start = i
        elif ch == '{':
            h = body[buf_start:i]
            hh = norm_ws(h)
            # initializer?
            d = 0; haseq = False
            for k, c2 in enumerate(h):
                if c2 in '<([': d += 1
                elif c2 in '>)]': d -= 1
                elif c2 == '=' and d == 0 and h[k+1:k+2] != '>':
                    haseq = True; break
            e = match_brace(body, i)
            tm = TYPE_RE.match(hh)
            if tm:
                kk = tm.group(3); nm = tm.group(4)
                inner = body[i+1:e]
                ms, ns = parse_body(inner, nm, kk, file, base_off + i + 1, src_lines_off)
                nested.append({'kind': kk, 'name': nm, 'mods': tm.group(2).split(), 'bases': tm.group(5), 'members': ms, 'nested': ns, 'file': file, 'line': line_at(buf_start + len(h) - len(h.lstrip()))})
                i = e + 1; buf_start = i
            elif haseq:
                # field initializer w/ braces -> consume until ;
                j = e + 1
                dd = 0
                while j < n and not (body[j] == ';' and dd == 0):
                    if body[j] in '([{': dd += 1
                    elif body[j] in ')]}': dd -= 1
                    j += 1
                m = parse_header(h, tname)
                if m:
                    m['line'] = line_at(buf_start + len(h) - len(h.lstrip()))
                    members.append(m)
                i = j + 1; buf_start = i
            else:
                m = parse_header(h, tname)
                if m:
                    m['line'] = line_at(buf_start + len(h) - len(h.lstrip()))
                    inner = body[i+1:e]
                    if m['kind'] == 'field_or_prop':
                        m['kind'] = 'property'
                        m['accessors'] = norm_ws(inner)[:60]
                    members.append(m)
                i = e + 1; buf_start = i
                # trailing initializer "= x;" handled: next ; yields header starting with '='
        elif ch == '=' and body[i+1:i+2] == '>' and any(c3 == '=' and body[buf_start+k3+1:buf_start+k3+2] != '>' for k3, c3 in enumerate(body[buf_start:i])):
            # lambda inside a field initializer ("= new Lazy<T>(() => ...)"): not an expression-bodied member
            h = body[buf_start:i]
            hh0 = h[:h.index('=')]
            m = parse_header(hh0, tname)
            j = i + 2; dd = sum(1 for c4 in h if c4 in '([{') - sum(1 for c4 in h if c4 in ')]}')
            while j < n and not (body[j] == ';' and dd == 0):
                if body[j] in '([{': dd += 1
                elif body[j] in ')]}': dd -= 1
                j += 1
            if m:
                m['line'] = line_at(buf_start + len(h) - len(h.lstrip()))
                members.append(m)
            i = j + 1; buf_start = i
        elif ch == '=' and body[i+1:i+2] == '>':
            h = body[buf_start:i]
            m = parse_header(h, tname)
            # consume to ; at depth 0
            j = i + 2; dd = 0
            while j < n and not (body[j] == ';' and dd == 0):
                if body[j] in '([{': dd += 1
                elif body[j] in ')]}': dd -= 1
                j += 1
            if m:
                m['line'] = line_at(buf_start + len(h) - len(h.lstrip()))
                if m['kind'] == 'field_or_prop': m['kind'] = 'property'; m['accessors'] = '=>'
                else: m['expr_body'] = True
                members.append(m)
            i = j + 1; buf_start = i
        else:
            i += 1
    return members, nested

def parse_file(path):
    raw = open(path, encoding='utf-8-sig', errors='replace').read()
    s = strip_code(raw)
    line_starts = [0]
    for m in re.finditer('\n', s): line_starts.append(m.end())
    import bisect
    def line_at(pos): return bisect.bisect_right(line_starts, pos)
    # namespaces
    types = []
    ns = None
    mns = re.search(r'\bnamespace\s+([\w\.]+)', s)
    if mns: ns = mns.group(1)
    # top-level (and namespace-level) type scan
    def scan(region, off):
        res = []
        i = 0; n = len(region); bs = 0
        while i < n:
            ch = region[i]
            if ch == '{':
                h = norm_ws(region[bs:i])
                e = match_brace(region, i)
                tm = TYPE_RE.match(h)
                nsm = re.match(r'^namespace\s+([\w\.]+)$', h)
                if nsm:
                    res += scan(region[i+1:e], off + i + 1)
                elif tm:
                    kk = tm.group(3); nm = tm.group(4)
                    ms, nn = parse_body(region[i+1:e], nm, kk, path, off + i + 1, line_at)
                    res.append({'kind': kk, 'name': nm, 'mods': tm.group(2).split(), 'bases': tm.group(5), 'members': ms, 'nested': nn, 'file': path, 'line': line_at(off + bs + (len(region[bs:i]) - len(region[bs:i].lstrip()))), 'ns': ns, 'span': (off+i+1, off+e)})
                i = e + 1; bs = i
            elif ch == ';':
                i += 1; bs = i
            else: i += 1
        return res
    types = scan(s, 0)
    return types, s

def load_all():
    allt = []
    codes = {}
    for dp, dn, fn in os.walk(ROOT):
        dn[:] = [d for d in dn if d not in ('obj', 'bin', '.git', 'packages', '.vs')]
        for f in fn:
            if f.endswith('.cs') and f != 'AssemblyInfo.cs':
                p = os.path.join(dp, f)
                try:
                    ts, s = parse_file(p)
                except Exception as ex:
                    print('PARSE FAIL', p, ex, file=sys.stderr); continue
                codes[p] = s
                for t in ts:
                    t['designer'] = f.endswith('.Designer.cs')
                    allt.append(t)
    return allt, codes

if __name__ == '__main__':
    allt, codes = load_all()
    out = {}
    for t in allt:
        print(t['kind'], t['name'], os.path.relpath(t['file'], ROOT), len(t['members']))
