# Herramientas de diagramas

Scripts para regenerar y auditar los diagramas PlantUML de `DIAGRAMAS/`. Todas las rutas son relativas a la ubicación de los scripts; se pueden ejecutar desde cualquier carpeta.

## Criterio

**El código manda; se corrigen diagramas.** Si un diagrama no coincide con el código real, se arregla el diagrama (o su fuente), nunca al revés.

## Requisitos

- Python 3.12 o superior (se probó con 3.12).
- `Pillow` (`pip install pillow`) para leer el tamaño del PNG generado.
- Acceso a internet: el render hace un POST del texto PlantUML a `https://kroki.io/plantuml/svg` (con `User-Agent: Mozilla/5.0`).
- Microsoft Edge en `C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe`: convierte el SVG a PNG en modo headless (el PNG de PlantUML se recorta a 4096 px y cairosvg no funciona en este equipo).

Ejecutar con `python -I` para que Python no cargue módulos de la carpeta actual.

## Secuencia (`secuencia/`)

| Archivo | Qué hace |
|---|---|
| `seq_src/*.puml` | Fuentes editables de los 32 diagramas de secuencia. Usan macros (`@IR`, `@RC`, `@RB`, `@LU`, etc.) para no repetir bloques comunes. |
| `seq_build.py` | Expande las macros de `seq_src/` y escribe los `.puml` finales en `DIAGRAMAS/`. Conserva el fin de línea (CRLF/LF) del archivo destino. |
| `render_seq.py` | Renderiza `DiagramaSecuencia_CU*.puml` a `.png` (kroki + Edge headless). |

Orden: editar `seq_src/` → `seq_build.py` → `render_seq.py`.

```powershell
python -I secuencia/seq_build.py            # todos
python -I secuencia/seq_build.py CU03 CU11  # solo algunos
python -I secuencia/render_seq.py CU03 CU11
```

Opciones de prueba: `seq_build.py --out CARPETA` escribe en otra carpeta; `render_seq.py --dir CARPETA` renderiza desde otra carpeta.

Nunca editar a mano los `DiagramaSecuencia_CU*.puml` de `DIAGRAMAS/`: se pisan al reconstruir.

## Clases (`clases/`)

| Archivo | Qué hace |
|---|---|
| `csparse.py` | Lee los `.cs` del repo y arma un índice de tipos y miembros (raíz del repo calculada desde la ubicación del script). |
| `pumlparse.py` | Lee un `.puml` de clases (clases, miembros, relaciones). |
| `gen_master.py` | Genera `DiagramaClases.puml` (maestro) desde el código. Los miembros privados ya dibujados se toman del propio `DiagramaClases.puml` vigente. |
| `layout_gen.py` | Genera el bloque final de disposición vertical (capas apiladas CAPAS, SeguridadYServicios, BLL, DAL, BE con enlaces ocultos; las relaciones entre capas se resumen como flechas entre paquetes y las internas usan `[norank]`); lo usa `gen_master.py`. |
| `compare.py` | Compara cada diagrama de clases contra el código y escribe un informe Markdown. |
| `arrowchk.py` | Verifica que el tipo de flecha (`-->` asociación / `..>` dependencia) coincida con el código. |
| `render_clases.py` | Renderiza un `.puml` grande a PNG (y SVG opcional). |

Orden: `gen_master.py` → `compare.py` / `arrowchk.py` → render.

```powershell
python -I clases/gen_master.py salida.puml          # revisar y recién después copiar a DIAGRAMAS/DiagramaClases.puml
python -I clases/compare.py informe.md              # informe de diferencias código vs. diagramas
python -I clases/arrowchk.py                        # imprime solo las flechas que no coinciden
python -I clases/render_clases.py entrada.puml salida.png [salida.svg|-] [escala]
```

`gen_master.py` nunca escribe en `DIAGRAMAS/` por su cuenta: siempre recibe la ruta de salida.

## Otros scripts de `DIAGRAMAS/`

`generar_png.py`, `generar_pngs_lote.py`, `generar_pdf.py`, `generar_casos_uso_docx.py` y `check_dims.py` se mantienen en `DIAGRAMAS/`.
