"""Renderiza un .puml grande: POST a kroki (svg) y conversion a PNG con Edge headless.
Uso: python -I render_clases.py <entrada.puml> <salida.png> [salida.svg] [escala]"""
import sys, re, os, pathlib, subprocess, urllib.request, tempfile
src, out_png = sys.argv[1], sys.argv[2]
out_svg = sys.argv[3] if len(sys.argv) > 3 and sys.argv[3] != '-' else None
scale = float(sys.argv[4]) if len(sys.argv) > 4 else 1.0
data = open(src, encoding='utf-8').read().encode('utf-8')
req = urllib.request.Request('https://kroki.io/plantuml/svg', data=data, method='POST',
    headers={'User-Agent': 'Mozilla/5.0', 'Content-Type': 'text/plain'})
svg = urllib.request.urlopen(req, timeout=180).read()
if out_svg: open(out_svg, 'wb').write(svg)
txt = svg.decode('utf-8')
m = re.search(r'<svg[^>]*?width="([\d.]+)(?:px)?"[^>]*?height="([\d.]+)(?:px)?"', txt) or re.search(r'<svg[^>]*?height="([\d.]+)(?:px)?"[^>]*?width="([\d.]+)(?:px)?"', txt)
w, h = float(m.group(1)), float(m.group(2))
vb = re.search(r'viewBox="[\d.\-]+ [\d.\-]+ ([\d.]+) ([\d.]+)"', txt)
print('svg dims', w, h, 'viewBox', vb.groups() if vb else None)
W, H = int(w) + 2, int(h) + 2
tmp = tempfile.mkdtemp()
sp = os.path.join(tmp, 'd.svg'); open(sp, 'wb').write(svg)
html = os.path.join(tmp, 'd.html')
open(html, 'w', encoding='utf-8').write('<html><body style="margin:0;background:#fff"><img src="d.svg" width="%d" height="%d"></body></html>' % (w, h))
edge = 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'
subprocess.run([edge, '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=%s' % scale,
    '--window-size=%d,%d' % (W, H), '--screenshot=' + os.path.abspath(out_png), 'file:///' + pathlib.Path(html).as_posix()], check=True, timeout=300, capture_output=True)
from PIL import Image
print(out_png, Image.open(out_png).size)
