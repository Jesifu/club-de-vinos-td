"""Renderiza DiagramaSecuencia_CU*.puml -> .png: POST a kroki (svg) + Edge headless (el png de PlantUML se recorta a 4096 px).
Uso: python -I render_seq.py [CU01 CU02 ...]"""
import sys, re, os, pathlib, subprocess, urllib.request, tempfile, time, glob
D = pathlib.Path(__file__).resolve().parents[2]   # DIAGRAMAS/ (secuencia -> herramientas -> DIAGRAMAS)
if '--dir' in sys.argv:   # carpeta alternativa (pruebas): python render_seq.py --dir RUTA CU03
    i = sys.argv.index('--dir'); D = pathlib.Path(sys.argv[i + 1]).resolve(); del sys.argv[i:i + 2]
EDGE = 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'
pats = sys.argv[1:]
files = sorted(D.glob('DiagramaSecuencia_CU*.puml'))
if pats: files = [f for f in files if any(p.lower() in f.name.lower() for p in pats)]
from PIL import Image
for f in files:
    data = f.read_text(encoding='utf-8').encode('utf-8')
    svg = None
    for k in range(4):
        try:
            req = urllib.request.Request('https://kroki.io/plantuml/svg', data=data, method='POST',
                headers={'User-Agent': 'Mozilla/5.0', 'Content-Type': 'text/plain'})
            svg = urllib.request.urlopen(req, timeout=180).read(); break
        except Exception as e:
            print('retry', f.name, str(e)[:60]); time.sleep(5)
    if svg is None: print('FAIL', f.name); continue
    txt = svg.decode('utf-8')
    m = re.search(r'<svg[^>]*?width="([\d.]+)(?:px)?"[^>]*?height="([\d.]+)(?:px)?"', txt) or re.search(r'<svg[^>]*?height="([\d.]+)(?:px)?"[^>]*?width="([\d.]+)(?:px)?"', txt)
    w, h = float(m.group(1)), float(m.group(2))
    tmp = tempfile.mkdtemp()
    open(os.path.join(tmp, 'd.svg'), 'wb').write(svg)
    html = os.path.join(tmp, 'd.html')
    open(html, 'w', encoding='utf-8').write('<html><body style="margin:0;background:#fff"><img src="d.svg" width="%d" height="%d"></body></html>' % (w, h))
    out = f.with_suffix('.png')
    subprocess.run([EDGE, '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
        '--window-size=%d,%d' % (int(w) + 2, int(h) + 2), '--screenshot=' + str(out), 'file:///' + pathlib.Path(html).as_posix()],
        check=True, timeout=300, capture_output=True)
    print('OK', f.name, Image.open(out).size)
