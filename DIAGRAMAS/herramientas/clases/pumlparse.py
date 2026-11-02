import re

HDR = re.compile(r'^\s*(abstract\s+class|class|interface|enum|abstract)\s+(?:"([^"]+)"\s+as\s+(\w+)|(\w+))(?:\s+as\s+(\w+))?\s*(<<[^>]*>>)?\s*(\{)?\s*$')

def parse_puml(path):
    lines = open(path, encoding='utf-8').read().split('\n')
    classes = {}
    rels = []
    pkg = None
    cur = None
    stack = []
    for ln, line in enumerate(lines, 1):
        t = line.strip()
        if cur is None:
            mp = re.match(r'^package\s+"([^"]+)"', t)
            if mp:
                pkg = mp.group(1); stack.append('pkg'); continue
            if re.match(r'^together\s*\{', t):
                stack.append('tog'); continue
            if t == '}' and stack:
                k = stack.pop()
                if k == 'pkg': pkg = None
                continue
            m = HDR.match(line)
            if m:
                kind = m.group(1).replace('  ', ' ')
                name = m.group(3) or m.group(4)
                label = m.group(2)
                st = m.group(6) or (re.search(r'<<[^>]*>>', t).group(0) if '<<' in t else '')
                cur = {'kind': kind, 'name': name, 'label': label, 'stereo': st, 'pkg': pkg, 'line': ln, 'members': []}
                classes[name] = cur
                if not m.group(7):
                    cur = None
                continue
            # relationship ([norank] solo afecta al layout: se ignora al leer)
            line = line.replace('[norank]', '')
            mr = re.match(r'^\s*(\w+)\s*(?:"([^"]*)")?\s*(<\|--|--\|>|<\|\.\.|\.\.\|>|o--|--o|\*--|--\*|-->|<--|\.\.>|<\.\.|--|\.\.)\s*(?:"([^"]*)")?\s*(\w+)\s*(?::\s*(.*))?$', line)
            if mr and '[hidden]' not in line:
                rels.append({'a': mr.group(1), 'ma': mr.group(2), 'arrow': mr.group(3), 'mb': mr.group(4), 'b': mr.group(5), 'label': mr.group(6), 'line': ln})
            continue
        # inside class
        if t == '}':
            cur = None; continue
        if not t or t.startswith("'"): continue
        if re.match(r'^(--+|\.\.+|==+|__+)(\s.*)?$', t):
            continue
        mm = re.match(r'^([+\-#~])\s*(\{static\}|\{abstract\})?\s*(\{static\}|\{abstract\})?\s*(.*)$', t)
        if mm:
            vis = mm.group(1); mods = [x for x in (mm.group(2), mm.group(3)) if x]
            rest = mm.group(4)
            if '(' in rest:
                mt = re.match(r'^(.*?)\((.*)\)\s*(?::\s*(.*))?$', rest)
                name = mt.group(1).strip(); params = mt.group(2); ret = (mt.group(3) or '').strip()
                cur['members'].append({'kind': 'method', 'vis': vis, 'mods': mods, 'name': name, 'params': params, 'ret': ret, 'line': ln})
            else:
                ma = re.match(r'^(\w+)\s*(?::\s*([^=]+?))?\s*(?:=\s*(.*))?$', rest)
                if ma:
                    cur['members'].append({'kind': 'attr', 'vis': vis, 'mods': mods, 'name': ma.group(1), 'type': (ma.group(2) or '').strip(), 'init': ma.group(3), 'line': ln})
                else:
                    cur['members'].append({'kind': 'raw', 'raw': t, 'line': ln})
        else:
            if cur['kind'] == 'enum':
                cur['members'].append({'kind': 'enumval', 'name': t, 'line': ln})
            else:
                cur['members'].append({'kind': 'raw', 'raw': t, 'line': ln})
    return classes, rels

if __name__ == '__main__':
    import sys
    c, r = parse_puml(sys.argv[1])
    print(len(c), len(r))
    for k, v in c.items(): print(v['kind'], k, len(v['members']))
    for x in r[:5]: print(x)
