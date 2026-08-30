import re, struct

with open('DiagramaClases.svg', encoding='utf-8') as f:
    svg = f.read()
m = re.search(r'width="([^"]+)".*?height="([^"]+)"', svg)
if m:
    print(f'SVG: {m.group(1)} x {m.group(2)}')

with open('DiagramaClases.png', 'rb') as f:
    f.read(8)
    f.read(4)
    f.read(4)
    w = struct.unpack('>I', f.read(4))[0]
    h = struct.unpack('>I', f.read(4))[0]
print(f'PNG: {w} x {h} px')
