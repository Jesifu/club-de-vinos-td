"""
Regenera en lote los PNG de todos los diagramas de secuencia (DIAGRAMAS/*.puml)
usando kroki.io. Reintenta cada archivo varias veces ante errores de red
(timeouts) antes de pasar al siguiente.

Uso:
    python generar_pngs_lote.py                # todos los .puml de DIAGRAMAS/
    python generar_pngs_lote.py CU04 CU11 CU20 # solo los que coincidan con esos patrones
"""
import sys
import time
import zlib
import base64
import urllib.request
from pathlib import Path

DIAGRAMAS_DIR = Path(__file__).parent / "DIAGRAMAS"
INTENTOS = 3
ESPERA_ENTRE_INTENTOS = 5  # segundos


def plantuml_encode(text):
    compressed = zlib.compress(text.encode("utf-8"))
    return base64.urlsafe_b64encode(compressed).decode("ascii")


def generar_png(puml_file: Path, out_png: Path):
    content = puml_file.read_text(encoding="utf-8")
    encoded = plantuml_encode(content)
    url = f"https://kroki.io/plantuml/png/{encoded}"
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=60) as resp:
        data = resp.read()
    out_png.write_bytes(data)
    return len(data)


def main():
    filtros = sys.argv[1:]

    archivos = sorted(DIAGRAMAS_DIR.glob("*.puml"))
    if filtros:
        archivos = [p for p in archivos if any(f.lower() in p.name.lower() for f in filtros)]

    if not archivos:
        print("No se encontraron archivos .puml para procesar.")
        return

    ok, fallidos = [], []

    for puml_file in archivos:
        out_png = puml_file.with_suffix(".png")
        for intento in range(1, INTENTOS + 1):
            try:
                tamanio = generar_png(puml_file, out_png)
                print(f"OK  {out_png.name} ({tamanio} bytes)")
                ok.append(out_png.name)
                break
            except Exception as ex:
                print(f"  intento {intento}/{INTENTOS} falló para {puml_file.name}: {ex}")
                if intento < INTENTOS:
                    time.sleep(ESPERA_ENTRE_INTENTOS)
                else:
                    fallidos.append(puml_file.name)

    print("\n--- Resumen ---")
    print(f"Generados correctamente: {len(ok)}")
    if fallidos:
        print(f"Fallaron ({len(fallidos)}):")
        for f in fallidos:
            print(f"  - {f}")
    else:
        print("Sin errores.")


if __name__ == "__main__":
    main()
