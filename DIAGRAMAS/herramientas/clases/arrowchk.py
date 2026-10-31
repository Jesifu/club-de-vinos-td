import sys,re,os
sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
sys_path_fix=1
sys.argv=['x']
import gen_master as g
D=g.os.path.join(g.ROOT,'DIAGRAMAS')
for fn in ['DiagramaClases','DiagramaClases_CatalogoStock','DiagramaClases_CuracionCajas','DiagramaClases_Composite','DiagramaClases_Observer','DiagramaClases_Singleton']:
    t=open(os.path.join(D,fn+'.puml'),encoding='utf-8').read()
    for m in re.finditer(r'^\s*(\w+)\s+(-->|\.\.>)\s+(\w+)\s*(:.*)?$',t,re.M):
        a,ar,b=m.group(1),m.group(2),m.group(3)
        if a not in g.by or b not in g.by: continue
        if g.layer(a)=='BE': continue
        exp=g.arrow(a,b)
        if exp!=ar: print(fn,a,ar,b,'expected',exp, m.group(4) or '')
print('done')
