# -*- coding: utf-8 -*-
"""
Genera el entregable D03 "Material de apoyo al usuario final" del sistema Club de Vinos:

    Guia_del_Usuario.docx, Guia_de_Operacion.docx, Guia_de_Mantenimiento.docx

Uso (desde cualquier carpeta):
    python -I Documentacion/generar_guias.py            # genera los tres .docx
    python -I Documentacion/generar_guias.py --sin-word # no usa Word para actualizar el indice

Requisitos: Python 3.12, python-docx. Si Microsoft Word esta instalado, el script lo usa (COM) para
actualizar el indice, contar paginas y exportar un PDF temporal que borra al terminar.
Las cifras y los mensajes citados en las guias se tomaron del codigo del repositorio; ante una
diferencia entre este texto y el codigo, manda el codigo y se corrige este generador.
"""
import re
import subprocess
import sys
import tempfile
from pathlib import Path

from docx import Document
from docx.enum.section import WD_ORIENT
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK, WD_TAB_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor, Twips

RAIZ = Path(__file__).resolve().parent.parent
SALIDA = Path(__file__).resolve().parent

BORDO = "7B1E3A"
BORDO_RGB = RGBColor(0x7B, 0x1E, 0x3A)
GRIS_RGB = RGBColor(0x40, 0x40, 0x40)
FUENTE = "Calibri"
FECHA_ENTREGA = "2 de noviembre de 2026"
VERSION_DOC = "1.0"
ANCHO_UTIL_CM = 16.6


# --------------------------------------------------------------------------------------
# Datos tomados del repositorio en el momento de generar (para que las guias no se desfasen)
# --------------------------------------------------------------------------------------
def _leer(ruta):
    try:
        return (RAIZ / ruta).read_text(encoding="utf-8-sig")
    except OSError:
        return ""


def _version_producto():
    m = re.search(r'<Package[^>]*?\bVersion="([\d.]+)"', _leer("Instalador/Package.wxs"), re.S)
    return m.group(1) if m else "no determinada"


VERSION_PRODUCTO = _version_producto()
# La ayuda en linea (CAPAS/Ayuda) la agrega otro frente de trabajo; solo se documenta si el menu ya la tiene.
HAY_AYUDA = "ayudaEnLineaToolStripMenuItem" in _leer("CAPAS/frmMenu.cs")


# --------------------------------------------------------------------------------------
# Utilidades de formato (python-docx + XML)
# --------------------------------------------------------------------------------------
def _fuente_estilo(estilo, tam=None, negrita=None, color=None, cursiva=None):
    f = estilo.font
    f.name = FUENTE
    if tam is not None:
        f.size = Pt(tam)
    if negrita is not None:
        f.bold = negrita
    if cursiva is not None:
        f.italic = cursiva
    if color is not None:
        f.color.rgb = color
    rpr = estilo.element.get_or_add_rPr()
    rfonts = rpr.find(qn("w:rFonts"))
    if rfonts is None:
        rfonts = OxmlElement("w:rFonts")
        rpr.insert(0, rfonts)
    for a in ("w:ascii", "w:hAnsi", "w:cs", "w:eastAsia"):
        rfonts.set(qn(a), FUENTE)
    for a in ("w:asciiTheme", "w:hAnsiTheme", "w:cstheme", "w:eastAsiaTheme"):
        if rfonts.get(qn(a)) is not None:
            del rfonts.attrib[qn(a)]


def _sombrear_celda(celda, hex_color):
    tcpr = celda._tc.get_or_add_tcPr()
    for viejo in tcpr.findall(qn("w:shd")):
        tcpr.remove(viejo)
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), hex_color)
    tcpr.append(shd)


def _sombrear_parrafo(parrafo, hex_color):
    ppr = parrafo._p.get_or_add_pPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), hex_color)
    ppr.append(shd)


def _borde_parrafo(parrafo, lado, tam=8, color=BORDO, espacio=1):
    ppr = parrafo._p.get_or_add_pPr()
    pbdr = ppr.find(qn("w:pBdr"))
    if pbdr is None:
        pbdr = OxmlElement("w:pBdr")
        ppr.append(pbdr)
    b = OxmlElement("w:" + lado)
    b.set(qn("w:val"), "single")
    b.set(qn("w:sz"), str(tam))
    b.set(qn("w:space"), str(espacio))
    b.set(qn("w:color"), color)
    pbdr.append(b)


def _bordes_tabla(tabla, color="BFBFBF", tam=4):
    tblpr = tabla._tbl.tblPr
    borders = OxmlElement("w:tblBorders")
    for lado in ("top", "left", "bottom", "right", "insideH", "insideV"):
        b = OxmlElement("w:" + lado)
        b.set(qn("w:val"), "single")
        b.set(qn("w:sz"), str(tam))
        b.set(qn("w:space"), "0")
        b.set(qn("w:color"), color)
        borders.append(b)
    tblpr.append(borders)


def _margenes_celdas(tabla, arriba=50, lados=90):
    tblpr = tabla._tbl.tblPr
    mar = OxmlElement("w:tblCellMar")
    for lado, v in (("top", arriba), ("left", lados), ("bottom", arriba), ("right", lados)):
        e = OxmlElement("w:" + lado)
        e.set(qn("w:w"), str(v))
        e.set(qn("w:type"), "dxa")
        mar.append(e)
    tblpr.append(mar)


def _tabla_fija(tabla, anchos_cm):
    tblpr = tabla._tbl.tblPr
    lay = OxmlElement("w:tblLayout")
    lay.set(qn("w:type"), "fixed")
    tblpr.append(lay)
    for viejo in tblpr.findall(qn("w:tblW")):
        tblpr.remove(viejo)
    w = OxmlElement("w:tblW")
    w.set(qn("w:w"), str(int(sum(anchos_cm) * 567)))
    w.set(qn("w:type"), "dxa")
    tblpr.append(w)
    for i, a in enumerate(anchos_cm):
        tabla.columns[i].width = Cm(a)
        for fila in tabla.rows:
            fila.cells[i].width = Cm(a)


def _fila_encabezado(fila):
    trpr = fila._tr.get_or_add_trPr()
    h = OxmlElement("w:tblHeader")
    trpr.append(h)


def _fila_sin_cortar(fila):
    trpr = fila._tr.get_or_add_trPr()
    c = OxmlElement("w:cantSplit")
    trpr.append(c)


def _campo(parrafo, instruccion, texto_previo="1", negrita=False, tam=None):
    """Inserta un campo de Word (PAGE, NUMPAGES, TOC...) con un resultado provisorio."""
    def run_con(elem):
        r = parrafo.add_run()
        if negrita:
            r.bold = True
        if tam:
            r.font.size = Pt(tam)
        r._r.append(elem)
        return r

    ini = OxmlElement("w:fldChar")
    ini.set(qn("w:fldCharType"), "begin")
    run_con(ini)
    it = OxmlElement("w:instrText")
    it.set(qn("xml:space"), "preserve")
    it.text = " " + instruccion + " "
    run_con(it)
    sep = OxmlElement("w:fldChar")
    sep.set(qn("w:fldCharType"), "separate")
    run_con(sep)
    r = parrafo.add_run(texto_previo)
    if tam:
        r.font.size = Pt(tam)
    fin = OxmlElement("w:fldChar")
    fin.set(qn("w:fldCharType"), "end")
    run_con(fin)


# Orden de hijos que exige el esquema de WordprocessingML (Word es estricto con algunos).
_ORDEN_HIJOS = {
    "w:pPr": ["pStyle", "keepNext", "keepLines", "pageBreakBefore", "framePr", "widowControl", "numPr",
              "suppressLineNumbers", "pBdr", "shd", "tabs", "suppressAutoHyphens", "kinsoku", "wordWrap",
              "overflowPunct", "topLinePunct", "autoSpaceDE", "autoSpaceDN", "bidi", "adjustRightInd",
              "snapToGrid", "spacing", "ind", "contextualSpacing", "mirrorIndents", "suppressOverlap", "jc",
              "textDirection", "textAlignment", "textboxTightWrap", "outlineLvl", "divId", "cnfStyle", "rPr",
              "sectPr", "pPrChange"],
    "w:rPr": ["rStyle", "rFonts", "b", "bCs", "i", "iCs", "caps", "smallCaps", "strike", "dstrike", "outline",
              "shadow", "emboss", "imprint", "noProof", "snapToGrid", "vanish", "webHidden", "color", "spacing",
              "w", "kern", "position", "sz", "szCs", "highlight", "u", "effect", "bdr", "shd", "fitText",
              "vertAlign", "rtl", "cs", "em", "lang", "eastAsianLayout", "specVanish", "oMath"],
    "w:tcPr": ["cnfStyle", "tcW", "gridSpan", "hMerge", "vMerge", "tcBorders", "shd", "noWrap", "tcMar",
               "textDirection", "tcFitText", "vAlign", "hideMark"],
    "w:tblPr": ["tblStyle", "tblpPr", "tblOverlap", "bidiVisual", "tblStyleRowBandSize", "tblStyleColBandSize",
                "tblW", "jc", "tblCellSpacing", "tblInd", "tblBorders", "shd", "tblLayout", "tblCellMar",
                "tblLook", "tblCaption", "tblDescription"],
    "w:trPr": ["cnfStyle", "divId", "gridBefore", "gridAfter", "wBefore", "wAfter", "cantSplit", "trHeight",
               "tblHeader", "tblCellSpacing", "jc", "hidden"],
}


def _ordenar_hijos(raiz):
    for etiqueta, orden in _ORDEN_HIJOS.items():
        for el in raiz.iter(qn(etiqueta)):
            hijos = list(el)

            def clave(h, orden=orden):
                nombre = h.tag.split("}")[1]
                return orden.index(nombre) if nombre in orden else len(orden)

            ordenados = sorted(hijos, key=clave)
            if ordenados != hijos:
                for h in hijos:
                    el.remove(h)
                for h in ordenados:
                    el.append(h)


_TOKEN = re.compile(r"(\*\*.+?\*\*|`.+?`)")


def _runs(parrafo, texto, tam=None, color=None, negrita_base=False, cursiva=False):
    """Escribe texto con marcas simples: **negrita** (puede contener `codigo`) y `codigo`."""
    for trozo in _TOKEN.split(texto):
        if not trozo:
            continue
        if trozo.startswith("**") and trozo.endswith("**") and len(trozo) > 4:
            _runs(parrafo, trozo[2:-2], tam=tam, color=color, negrita_base=True, cursiva=cursiva)
            continue
        if trozo.startswith("`") and trozo.endswith("`") and len(trozo) > 2:
            r = parrafo.add_run(trozo[1:-1])
            r.font.name = "Consolas"
            r._r.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), "Consolas")
            r.font.size = Pt((tam or 11) - 1)
            if negrita_base:
                r.bold = True
            if color is not None:
                r.font.color.rgb = color
            continue
        r = parrafo.add_run(trozo)
        if negrita_base:
            r.bold = True
        if tam:
            r.font.size = Pt(tam)
        if color is not None:
            r.font.color.rgb = color
        if cursiva:
            r.italic = True


class Guia:
    """Constructor de un documento Word con el formato comun de las tres guias."""

    def __init__(self, titulo, descripcion, nombre_archivo):
        self.titulo = titulo
        self.descripcion = descripcion
        self.archivo = SALIDA / nombre_archivo
        self.doc = Document()
        self._n1 = 0
        self._n2 = 0
        self._configurar()
        self._portada()
        self._indice()

    # ---- configuracion general -------------------------------------------------------
    def _configurar(self):
        d = self.doc
        sec = d.sections[0]
        sec.orientation = WD_ORIENT.PORTRAIT
        sec.page_width, sec.page_height = Cm(21.0), Cm(29.7)
        sec.left_margin = sec.right_margin = Cm(2.2)
        sec.top_margin, sec.bottom_margin = Cm(2.4), Cm(2.2)
        sec.header_distance, sec.footer_distance = Cm(1.1), Cm(1.0)
        sec.different_first_page_header_footer = True

        normal = d.styles["Normal"]
        _fuente_estilo(normal, tam=11)
        normal.paragraph_format.space_after = Pt(6)
        normal.paragraph_format.line_spacing = 1.08
        rpr = normal.element.get_or_add_rPr()
        lang = OxmlElement("w:lang")
        lang.set(qn("w:val"), "es-ES")
        lang.set(qn("w:eastAsia"), "es-ES")
        rpr.append(lang)

        h1, h2, h3 = d.styles["Heading 1"], d.styles["Heading 2"], d.styles["Heading 3"]
        _fuente_estilo(h1, tam=18, negrita=True, color=BORDO_RGB)
        _fuente_estilo(h2, tam=14, negrita=True, color=BORDO_RGB)
        _fuente_estilo(h3, tam=12, negrita=True, color=GRIS_RGB)
        for h, antes, despues in ((h1, 20, 8), (h2, 14, 5), (h3, 10, 4)):
            h.paragraph_format.space_before = Pt(antes)
            h.paragraph_format.space_after = Pt(despues)
            h.paragraph_format.keep_with_next = True
            h.paragraph_format.line_spacing = 1.0
        lb = d.styles["List Bullet"]
        _fuente_estilo(lb, tam=11)
        lb.paragraph_format.space_after = Pt(3)

        cp = d.core_properties
        cp.title = self.titulo + " - Club de Vinos"
        cp.subject = "Entregable D03 - Material de apoyo al usuario final"
        cp.author = ""
        cp.last_modified_by = ""
        cp.comments = ""
        cp.keywords = "Club de Vinos; D03; " + self.titulo

        # Encabezado y pie (la portada usa la "primera pagina" vacia).
        sec = d.sections[0]
        hp = sec.header.paragraphs[0]
        hp.text = ""
        _runs(hp, "Club de Vinos  |  " + self.titulo, tam=9, color=GRIS_RGB)
        _borde_parrafo(hp, "bottom", tam=6, espacio=4)
        fp = sec.footer.paragraphs[0]
        fp.text = ""
        ts = fp.paragraph_format.tab_stops
        ts.add_tab_stop(Twips(4680), WD_TAB_ALIGNMENT.CLEAR)
        ts.add_tab_stop(Twips(9360), WD_TAB_ALIGNMENT.CLEAR)
        ts.add_tab_stop(Cm(ANCHO_UTIL_CM), WD_TAB_ALIGNMENT.RIGHT)
        _runs(fp, "Versión %s  |  %s\t" % (VERSION_DOC, FECHA_ENTREGA), tam=9, color=GRIS_RGB)
        r = fp.add_run("Página ")
        r.font.size = Pt(9)
        _campo(fp, "PAGE", "2", tam=9)
        r = fp.add_run(" de ")
        r.font.size = Pt(9)
        _campo(fp, "NUMPAGES", "10", tam=9)

    def _portada(self):
        d = self.doc
        p = d.add_paragraph()
        p.paragraph_format.space_before = Pt(110)
        p.paragraph_format.space_after = Pt(4)
        r = p.add_run("CLUB DE VINOS")
        r.bold = True
        r.font.size = Pt(16)
        r.font.color.rgb = BORDO_RGB
        rpr = r._r.get_or_add_rPr()
        sp = OxmlElement("w:spacing")
        sp.set(qn("w:val"), "60")
        rpr.append(sp)
        regla = d.add_paragraph()
        regla.paragraph_format.space_after = Pt(26)
        _borde_parrafo(regla, "bottom", tam=24, espacio=1)

        p = d.add_paragraph()
        p.paragraph_format.space_after = Pt(6)
        p.paragraph_format.line_spacing = 1.0
        r = p.add_run(self.titulo)
        r.bold = True
        r.font.size = Pt(36)
        r.font.color.rgb = BORDO_RGB

        p = d.add_paragraph()
        p.paragraph_format.space_after = Pt(150)
        r = p.add_run(self.descripcion)
        r.font.size = Pt(15)
        r.font.color.rgb = GRIS_RGB

        meta = [
            ("Entregable", "D03 - Material de apoyo al usuario final"),
            ("Versión del documento", "%s (versión final)" % VERSION_DOC),
            ("Fecha de entrega", FECHA_ENTREGA),
            ("Producto", "Club de Vinos, versión %s (instalador MSI)" % VERSION_PRODUCTO),
            ("Idioma", "Español (neutro)"),
        ]
        for k, v in meta:
            p = d.add_paragraph()
            p.paragraph_format.space_after = Pt(3)
            p.paragraph_format.tab_stops.add_tab_stop(Cm(5.2))
            r = p.add_run(k + ":")
            r.bold = True
            r.font.size = Pt(11)
            r.font.color.rgb = BORDO_RGB
            r = p.add_run("\t" + v)
            r.font.size = Pt(11)
        d.add_page_break()

    def _indice(self):
        d = self.doc
        p = d.add_paragraph()
        p.paragraph_format.space_after = Pt(10)
        r = p.add_run("Contenido")
        r.bold = True
        r.font.size = Pt(20)
        r.font.color.rgb = BORDO_RGB
        _borde_parrafo(p, "bottom", tam=8, espacio=4)
        p = d.add_paragraph()
        _campo(p, 'TOC \\o "1-2" \\h \\z \\u',
               "Para generar el índice: clic derecho sobre este texto y elegir «Actualizar campo».")
        d.add_page_break()

    # ---- bloques de contenido --------------------------------------------------------
    def h1(self, texto):
        self._n1 += 1
        self._n2 = 0
        self.doc.add_heading("%d. %s" % (self._n1, texto.replace("`", "")), level=1)

    def h2(self, texto):
        self._n2 += 1
        self.doc.add_heading("%d.%d %s" % (self._n1, self._n2, texto.replace("`", "")), level=2)

    def h3(self, texto):
        self.doc.add_heading(texto.replace("`", ""), level=3)

    def p(self, texto, tam=None, cursiva=False, mantener=False):
        par = self.doc.add_paragraph()
        _runs(par, texto, tam=tam, cursiva=cursiva)
        if mantener:
            par.paragraph_format.keep_with_next = True
        return par

    def bullets(self, items):
        for t in items:
            par = self.doc.add_paragraph(style="List Bullet")
            _runs(par, t)

    def pasos(self, items, inicio=1):
        """Lista numerada que siempre arranca en `inicio` (numeracion manual, sin listas de Word)."""
        for i, t in enumerate(items, start=inicio):
            par = self.doc.add_paragraph()
            pf = par.paragraph_format
            pf.left_indent = Cm(1.0)
            pf.first_line_indent = Cm(-0.7)
            pf.space_after = Pt(3)
            pf.tab_stops.add_tab_stop(Cm(1.0))
            r = par.add_run("%d.\t" % i)
            r.bold = True
            r.font.color.rgb = BORDO_RGB
            _runs(par, t)
        self._espacio(4)

    def _espacio(self, pt):
        par = self.doc.add_paragraph()
        par.paragraph_format.space_after = Pt(0)
        par.paragraph_format.space_before = Pt(0)
        par.paragraph_format.line_spacing = Pt(pt)

    def codigo(self, texto):
        par = self.doc.add_paragraph()
        pf = par.paragraph_format
        pf.left_indent = Cm(0.4)
        pf.right_indent = Cm(0.2)
        pf.space_before = Pt(2)
        pf.space_after = Pt(8)
        pf.line_spacing = 1.0
        pf.keep_together = True
        _sombrear_parrafo(par, "F2F2F2")
        _borde_parrafo(par, "left", tam=18, espacio=6)
        lineas = texto.split("\n")
        for i, linea in enumerate(lineas):
            r = par.add_run(linea)
            r.font.name = "Consolas"
            r._r.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), "Consolas")
            r.font.size = Pt(9)
            if i < len(lineas) - 1:
                r.add_break()

    def nota(self, texto, tipo="Nota"):
        t = self.doc.add_table(rows=1, cols=1)
        t.alignment = WD_TABLE_ALIGNMENT.CENTER
        _tabla_fija(t, [ANCHO_UTIL_CM])
        _margenes_celdas(t, arriba=70, lados=140)
        celda = t.rows[0].cells[0]
        _sombrear_celda(celda, "F7ECEF" if tipo != "Importante" else "FFF2DC")
        tcpr = celda._tc.get_or_add_tcPr()
        bord = OxmlElement("w:tcBorders")
        for lado in ("top", "bottom", "right"):
            b = OxmlElement("w:" + lado)
            b.set(qn("w:val"), "nil")
            bord.append(b)
        izq = OxmlElement("w:left")
        izq.set(qn("w:val"), "single")
        izq.set(qn("w:sz"), "24")
        izq.set(qn("w:space"), "0")
        izq.set(qn("w:color"), BORDO if tipo != "Importante" else "C77700")
        bord.append(izq)
        tcpr.append(bord)
        par = celda.paragraphs[0]
        par.paragraph_format.space_after = Pt(0)
        r = par.add_run(tipo + ". ")
        r.bold = True
        r.font.color.rgb = BORDO_RGB if tipo != "Importante" else RGBColor(0x9A, 0x5B, 0x00)
        _runs(par, texto)
        _fila_sin_cortar(t.rows[0])
        self._espacio(6)

    def tabla(self, encabezados, filas, anchos, tam=9.5, zebra=True, centrar_desde=None):
        assert abs(sum(anchos) - ANCHO_UTIL_CM) < 0.35, "anchos de tabla: %.2f cm" % sum(anchos)
        t = self.doc.add_table(rows=1 + len(filas), cols=len(encabezados))
        t.alignment = WD_TABLE_ALIGNMENT.CENTER
        _tabla_fija(t, anchos)
        _bordes_tabla(t)
        _margenes_celdas(t)
        for j, h in enumerate(encabezados):
            c = t.rows[0].cells[j]
            _sombrear_celda(c, BORDO)
            par = c.paragraphs[0]
            par.paragraph_format.space_after = Pt(0)
            par.paragraph_format.keep_with_next = True
            r = par.add_run(h.replace("`", ""))
            r.bold = True
            r.font.size = Pt(tam)
            r.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
            if centrar_desde is not None and j >= centrar_desde:
                par.alignment = WD_ALIGN_PARAGRAPH.CENTER
        _fila_encabezado(t.rows[0])
        _fila_sin_cortar(t.rows[0])
        for i, fila in enumerate(filas, start=1):
            _fila_sin_cortar(t.rows[i])
            for j, valor in enumerate(fila):
                c = t.rows[i].cells[j]
                if zebra and i % 2 == 0:
                    _sombrear_celda(c, "F8F3F4")
                partes = str(valor).split("\n")
                for k, parte in enumerate(partes):
                    par = c.paragraphs[0] if k == 0 else c.add_paragraph()
                    par.paragraph_format.space_after = Pt(2)
                    par.paragraph_format.line_spacing = 1.0
                    if centrar_desde is not None and j >= centrar_desde:
                        par.alignment = WD_ALIGN_PARAGRAPH.CENTER
                    _runs(par, parte, tam=tam)
        # Entre dos tablas siempre debe haber un parrafo vacio (si no, Word las fusiona).
        self._espacio(8)

    def salto(self):
        self.doc.add_page_break()

    def guardar(self):
        raices = [self.doc.element, self.doc.styles.element]
        for sec in self.doc.sections:
            raices += [sec.header._element, sec.footer._element]
        for raiz in raices:
            _ordenar_hijos(raiz)
        self.doc.save(str(self.archivo))
        return self.archivo


# --------------------------------------------------------------------------------------
# GUIA DEL USUARIO
# --------------------------------------------------------------------------------------
def guia_usuario():
    g = Guia("Guía del Usuario",
             "Cómo usar el sistema Club de Vinos: catálogo y stock, cajas mensuales y análisis de rotación",
             "Guia_del_Usuario.docx")

    # ---------------- 1 ----------------
    g.h1("Introducción")
    g.h2("Para qué sirve el sistema")
    g.p("Club de Vinos es una aplicación de escritorio para Windows que apoya la operación de un club de vinos. "
        "Permite:")
    g.bullets([
        "**Mantener el catálogo de vinos y controlar su stock**, con un circuito de autorización para las altas y las bajas "
        "(Proceso 1: Catálogo y stock).",
        "**Registrar a los socios del club y preparar su caja mensual**: armarla, reemplazar vinos sin stock, despacharla y "
        "emitir el remito en PDF (Proceso 2: Curación y armado de cajas).",
        "**Analizar la rotación de los vinos** y obtener recomendaciones de reposición, con un informe en PDF "
        "(Proceso 3: Análisis de rotación y reposición).",
        "**Administrar la seguridad**: usuarios, roles y permisos, idiomas y bitácora de actividad. Estas funciones son "
        "propias del Administrador y se explican en la Guía de Operación.",
    ])
    g.p("Cada persona ve únicamente las opciones que su rol le permite usar. Por eso es posible que algunas secciones de esta "
        "guía describan pantallas que usted no tiene en su menú.")

    g.h2("Cómo usar esta guía")
    g.p("La guía está organizada por tareas. Cada procedimiento indica el menú por el que se accede, los pasos numerados, el "
        "resultado esperado y los mensajes que puede mostrar el sistema. Las convenciones son las siguientes:")
    g.tabla(["Formato", "Significado"], [
        ["**Texto en negrita**", "Nombre de un botón, campo u opción de menú, tal como aparece en pantalla."],
        ["Menú > Opción", "Ruta de menú. Por ejemplo: **Catálogo > Gestionar catálogo de vinos**."],
        ["“Texto entre comillas”", "Mensaje que muestra el sistema, transcripto tal como aparece."],
        ["Nota", "Información complementaria."],
        ["Importante", "Regla o advertencia que conviene conocer antes de actuar."],
    ], [4.6, 12.0])
    g.p("La guía describe la interfaz en español, que es el idioma predeterminado. Si cambia el idioma de la interfaz "
        "(sección 2.5), los rótulos de pantallas y menús se muestran traducidos, pero los procedimientos son los mismos. "
        "Los **mensajes de aviso y de error** se muestran siempre en español, y el remito de despacho en PDF también. El informe PDF de rotación, en cambio, "
        "sale en el idioma de la interfaz activo al exportarlo.")

    g.h2("Qué necesita para usar el sistema")
    g.bullets([
        "Una computadora con Windows 10 u 11 con el sistema ya instalado por el administrador (ver la Guía de Operación).",
        "Un nombre de usuario y una contraseña asignados por el administrador.",
        "Para abrir los PDF generados por el sistema, un lector de PDF instalado en la computadora.",
    ])

    # ---------------- 2 ----------------
    g.h1("Primeros pasos")
    g.h2("Iniciar la aplicación")
    g.p("Abra el acceso directo **Club de Vinos** del Escritorio o del menú Inicio de Windows. Se muestra la ventana "
        "**Iniciar Sesión**.")
    g.h2("Iniciar sesión")
    g.pasos([
        "Escriba su nombre de usuario en el campo **Usuario**.",
        "Escriba su contraseña en el campo **Contraseña**. Las mayúsculas y minúsculas se distinguen.",
        "Presione **Ingresar**.",
        "Si los datos son correctos, el sistema muestra “Bienvenido, <su usuario>!”. Acepte el mensaje para abrir el menú principal.",
    ])
    g.p("Si algo no sale como se espera, el sistema informa el motivo:", mantener=True)
    g.tabla(["Mensaje", "Causa", "Qué hacer"], [
        ["“Completá usuario y contraseña.”", "Dejó vacío el usuario o la contraseña.", "Complete ambos campos."],
        ["“Usuario o contraseña incorrectos.”", "Los datos no coinciden con ningún usuario.",
         "Revise mayúsculas y minúsculas y vuelva a intentar. Tres intentos fallidos consecutivos bloquean la cuenta."],
        ["“Usuario bloqueado por intentos fallidos. Contactate con un administrador.”",
         "La cuenta se bloqueó por errores de contraseña consecutivos (límite: 3).",
         "Pida a un administrador que desbloquee su usuario. No hay desbloqueo automático."],
        ["“El sistema no puede iniciarse debido a un problema interno. Comuníquese con el administrador del sistema.”",
         "El control de integridad de la base de datos detectó una inconsistencia y su usuario no puede resolverla.",
         "Avise al administrador del sistema y no insista: el problema debe revisarse en la base de datos."],
    ], [5.3, 5.6, 5.7])
    g.nota("Un ingreso correcto reinicia el contador de intentos fallidos. El contador solo se incrementa cuando el usuario "
           "existe y la contraseña es incorrecta.")

    g.h2("El menú principal")
    g.p("Tras iniciar sesión se abre una ventana con la barra de menú. La barra muestra solamente las opciones permitidas "
        "para su rol; si no ve una opción, su usuario no tiene el permiso correspondiente. En la parte inferior hay un "
        "selector **Idioma**.", mantener=True)
    filas = [
        ["Usuario", "Cerrar Sesion", "Termina la sesión y vuelve a la ventana de inicio de sesión.", "Todos los usuarios"],
        ["Configuración", "Cambiar Contraseña", "Cambia su propia contraseña.", "Todos los usuarios"],
        ["Bitácora", "(opción directa)", "Consulta el registro de actividad del sistema.", "Ver bitácora"],
        ["Administración", "Gestión de usuarios", "Alta, edición, baja y desbloqueo de usuarios.", "Administrar usuarios"],
        ["", "Roles y Permisos", "Roles, jerarquía y permisos de cada rol.", "Gestión de roles"],
        ["", "Gestión de idiomas", "Idiomas y traducciones de la interfaz.", "Gestión de idiomas"],
        ["Catálogo", "Gestionar catálogo de vinos", "Proponer altas, corregir rechazos y solicitar descontinuaciones.", "Gestionar catálogo de vinos"],
        ["", "Registrar entrada de stock", "Ingreso de mercadería.", "Gestionar catálogo de vinos"],
        ["", "Registrar ajuste de inventario", "Corrección de stock (roturas, mermas, conteos).", "Gestionar catálogo de vinos"],
        ["", "Consultar alerta de stock mínimo", "Vinos bajo su stock mínimo.", "Gestionar catálogo de vinos"],
        ["", "Autorizar alta de vinos", "Autorizar o rechazar altas.", "Autorizar alta de vinos"],
        ["", "Autorizar descontinuación de vinos", "Autorizar o rechazar bajas.", "Autorizar alta de vinos"],
        ["", "Analizar rotación de stock", "Análisis de rotación y reposición.", "Analizar rotación de stock"],
        ["Club de Socios", "Gestionar Socios", "Alta y edición de socios y sus varietales preferidos.", "Gestionar socios"],
        ["", "Armar Caja Mensual", "Arma la caja de un socio para un período.", "Armar cajas mensuales"],
        ["", "Picking y Sustituciones", "Revisa cajas armadas y registra reemplazos.", "Registrar picking y sustituciones"],
        ["", "Despachar Cajas", "Despacha una caja y emite el remito.", "Despachar cajas"],
        ["", "Historial de Despachos", "Consulta despachos y regenera remitos.", "Despachar cajas"],
    ]
    if HAY_AYUDA:
        filas.append(["Ayuda", "Ayuda en línea (F1)", "Abre la ayuda del sistema.", "Todos los usuarios"])
        filas.append(["", "Acerca de", "Muestra el nombre del producto y su versión.", "Todos los usuarios"])
    g.tabla(["Menú", "Opción", "Para qué sirve", "Permiso requerido"], filas, [2.9, 4.6, 5.6, 3.5], tam=9)
    g.nota("Los menús **Catálogo**, **Club de Socios** y **Administración** aparecen únicamente si usted tiene al menos un "
           "permiso de los que contienen.")

    g.h2("Cambiar la contraseña")
    g.pasos([
        "Elija **Configuración > Cambiar Contraseña**.",
        "Escriba la **Contraseña Actual**.",
        "Escriba la **Nueva Contraseña** y repítala en **Confirmar Contraseña**.",
        "Presione **Continuar**. Se muestra “Contraseña cambiada exitosamente.” y la ventana se cierra.",
    ])
    g.p("La nueva contraseña debe cumplir tres reglas: tener **6 o más caracteres**, **al menos una letra mayúscula** y "
        "**al menos un número**. Cada persona solo puede cambiar su propia contraseña.", mantener=True)
    g.tabla(["Mensaje", "Causa"], [
        ["“Completá todos los campos.”", "Falta alguno de los tres campos."],
        ["“Las contraseñas no coinciden.”", "La nueva contraseña y su confirmación son distintas."],
        ["“La contraseña debe tener al menos 6 caracteres.”", "La nueva contraseña es demasiado corta."],
        ["“La contraseña debe contener al menos una letra mayúscula.”", "Falta una mayúscula."],
        ["“La contraseña debe contener al menos un número.”", "Falta un dígito."],
        ["“La contraseña actual es incorrecta.”", "La contraseña actual no coincide con la registrada."],
    ], [8.2, 8.4])
    g.nota("El sistema no permite ver ni recuperar una contraseña olvidada, y ningún administrador puede cambiar la contraseña "
           "de otra persona desde la aplicación. Si la olvidó, consulte con el administrador del sistema (Guía de Operación, "
           "sección sobre administración de usuarios).", tipo="Importante")

    g.h2("Cambiar el idioma de la interfaz")
    g.pasos([
        "En la parte inferior de cualquier ventana, abra el selector **Idioma** y elija un idioma de la lista.",
        "Los rótulos de menús, botones y encabezados de tablas cambian de inmediato.",
    ])
    g.bullets([
        "En cada ventana el selector está disponible por separado, porque las ventanas de diálogo bloquean a la ventana que las abrió.",
        "El selector lista solo los idiomas **habilitados** por el administrador (el sistema incluye Español, Inglés y Portugués).",
        "Si elige un idioma después de iniciar sesión, el sistema lo recuerda: al ingresar la próxima vez, la interfaz ya aparece en ese idioma. "
        "La elección hecha antes de iniciar sesión (en la ventana de acceso) no se guarda.",
        "Si un texto no tiene traducción en el idioma elegido, se muestra en español.",
    ])
    g.h2("Cerrar la sesión")
    g.p("Elija **Usuario > Cerrar Sesion**. La sesión termina sin pedir confirmación y vuelve la ventana de inicio de "
        "sesión, desde la cual otra persona puede ingresar. Los datos ya guardados no se pierden.")

    # ---------------- 3 ----------------
    g.h1("Roles y funciones")
    g.p("Los permisos se agrupan en **roles**. A cada usuario el administrador le asigna uno o más roles y su conjunto de permisos "
        "es la suma de los permisos de esos roles. Los roles no heredan permisos entre sí.")
    g.h2("Roles que trae el sistema")
    g.tabla(["Rol", "Para qué se usa", "Permisos"], [
        ["Administrador", "Administración del sistema y autorización de altas y bajas de vinos.", "Todos"],
        ["Usuario", "Usuario común, sin funciones operativas.", "Cambiar contraseña"],
        ["Encargado de Compras/Bodega", "Propone vinos y bajas, registra entradas y ajustes de stock, analiza la rotación.",
         "Gestionar catálogo de vinos; Analizar rotación de stock"],
        ["Atención al Socio", "Mantiene el registro de socios y sus preferencias.", "Gestionar socios"],
        ["Curador de Producto", "Arma las cajas mensuales.", "Armar cajas mensuales"],
        ["Encargado de Depósito", "Prepara las cajas y registra sustituciones.", "Registrar picking y sustituciones"],
        ["Logística", "Despacha cajas y consulta despachos.", "Despachar cajas"],
    ], [4.2, 7.4, 5.0])
    g.nota("Esta es la configuración inicial. El administrador puede crear otros roles y cambiar los permisos de cada uno, de modo "
           "que en su organización la asignación puede diferir.")

    g.h2("Qué puede hacer cada rol")
    SI, NO = "Sí", "–"
    g.tabla(["Función", "Adm.", "Usu.", "Comp.", "Dep.", "Cur.", "Aten.", "Log."], [
        ["Iniciar y cerrar sesión, cambiar contraseña e idioma", SI, SI, SI, SI, SI, SI, SI],
        ["Proponer alta, corregir y reproponer, solicitar descontinuación", SI, NO, SI, NO, NO, NO, NO],
        ["Registrar entradas de stock y ajustes de inventario", SI, NO, SI, NO, NO, NO, NO],
        ["Consultar la alerta de stock mínimo", SI, NO, SI, NO, NO, NO, NO],
        ["Autorizar o rechazar altas y descontinuaciones", SI, NO, NO, NO, NO, NO, NO],
        ["Analizar rotación y exportar el informe", SI, NO, SI, NO, NO, NO, NO],
        ["Gestionar socios", SI, NO, NO, NO, NO, SI, NO],
        ["Armar caja mensual", SI, NO, NO, NO, SI, NO, NO],
        ["Picking y sustituciones", SI, NO, NO, SI, NO, NO, NO],
        ["Despachar cajas, historial de despachos y remitos", SI, NO, NO, NO, NO, NO, SI],
        ["Usuarios, roles y permisos, idiomas, bitácora", SI, NO, NO, NO, NO, NO, NO],
    ], [7.1, 1.35, 1.35, 1.35, 1.35, 1.35, 1.35, 1.35], tam=9, centrar_desde=1)
    g.p("Referencias: Adm. = Administrador; Usu. = Usuario; Comp. = Encargado de Compras/Bodega; Dep. = Encargado de Depósito; "
        "Cur. = Curador de Producto; Aten. = Atención al Socio; Log. = Logística.", tam=9, cursiva=True)

    g.h2("Separación de funciones")
    g.p("El sistema impide que una misma persona realice ambos extremos de ciertas operaciones. Estas reglas se aplican siempre, "
        "incluso a los administradores:", mantener=True)
    g.tabla(["Regla", "Qué significa", "Cómo se nota"], [
        ["RN-01", "Quien emite una solicitud de alta o de descontinuación de un vino no puede resolverla (ni autorizarla ni rechazarla).",
         "Los botones **Autorizar** y **Rechazar** aparecen deshabilitados para sus propias solicitudes."],
        ["RN-10", "Quien armó una caja no puede registrar sustituciones sobre ella ni despacharla.",
         "Los botones **Registrar sustitución** y **Despachar caja** aparecen deshabilitados para las cajas que usted armó."],
    ], [2.0, 8.0, 6.6])
    g.p("Por este motivo el sistema necesita más de una cuenta con autoridad (por ejemplo, dos administradores) y más de una "
        "persona en el circuito de cajas.")

    # ---------------- 4 ----------------
    g.h1("Proceso 1: Catálogo y stock")
    g.h2("Cómo funciona el proceso")
    g.p("Un vino ingresa al catálogo mediante una **solicitud de alta** que debe aprobar un administrador distinto de quien la "
        "emitió. Mientras no esté aprobado, el vino no puede recibir stock ni incluirse en cajas. Para retirarlo del catálogo "
        "se sigue el camino inverso con una **solicitud de baja** (descontinuación), que exige que el vino no tenga stock. "
        "Los vinos nunca se borran: la descontinuación solo cambia su estado.")
    g.tabla(["Estado del vino", "Significado", "Quién lo ve en el catálogo"], [
        ["Pendiente", "Alta propuesta, a la espera de autorización.", "Solo quien emitió la solicitud."],
        ["Activo", "Alta aprobada. Es el único estado en el que se puede operar (stock, cajas, alertas).", "Todos los usuarios con acceso al catálogo."],
        ["Rechazado", "El administrador rechazó el alta y dejó un motivo. Puede corregirse y reproponerse con el mismo SKU.", "Todos los usuarios con acceso al catálogo."],
        ["Descontinuado", "Baja autorizada. Ya no se opera con el vino.", "No se muestra en el catálogo."],
    ], [3.0, 8.2, 5.4])
    g.p("Cada propuesta o baja genera una **solicitud** con su propio estado: Pendiente, Aprobada o Rechazada. El stock de un vino "
        "no se escribe directamente: se calcula sumando las entradas y restando las salidas registradas (kardex), por lo que "
        "todo cambio de stock deja un movimiento con fecha, motivo y responsable.")

    g.h2("Proponer el alta de un vino")
    g.p("Permiso requerido: **Gestionar catálogo de vinos**.", mantener=True)
    g.pasos([
        "Elija **Catálogo > Gestionar catálogo de vinos**.",
        "Complete los datos del vino: **SKU** (código único), **Nombre**, **Bodega** (lista de bodegas habilitadas), **Varietal**, **Añada**, **Precio**, **Stock mínimo** y, opcionalmente, **Maridaje**.",
        "Si el vino tiene puntaje, marque **Incluir puntaje** y cargue el valor (de 0 a 100).",
        "Presione **Proponer alta**.",
        "El sistema muestra “Vino propuesto correctamente. Queda pendiente de autorización.” El vino queda en estado Pendiente y se emite la solicitud.",
    ])
    g.p("Use **Limpiar** para vaciar el formulario. En la tabla del catálogo ve los vinos activos, los rechazados y sus propios pendientes, "
        "con la última solicitud de cada uno y, si fue rechazada, el motivo.")
    g.tabla(["Mensaje", "Causa"], [
        ["“El SKU es obligatorio.” / “El nombre es obligatorio.” / “El varietal es obligatorio.”", "Falta un dato obligatorio."],
        ["“Debe seleccionar una bodega válida.”", "No hay una bodega seleccionada."],
        ["“El año de añada no es válido.”", "La añada debe estar entre 1900 y el año siguiente al actual."],
        ["“El precio debe ser mayor a cero.”", "Precio igual a cero."],
        ["“El stock mínimo no puede ser negativo.”", "Stock mínimo menor que cero."],
        ["“El puntaje debe estar entre 0 y 100.”", "Puntaje fuera de rango."],
        ["“Ya existe un vino con el SKU indicado.”", "El SKU ya está en uso por otro vino."],
        ["“El SKU indicado corresponde a un vino rechazado. Seleccione ese vino en el catálogo y use 'Corregir y reproponer'.”",
         "Intentó dar de alta de nuevo un SKU cuyo vino fue rechazado."],
    ], [9.6, 7.0])

    g.h2("Autorizar o rechazar un alta")
    g.p("Permiso requerido: **Autorizar alta de vinos** (rol Administrador). No puede resolver solicitudes que usted mismo emitió.",
        mantener=True)
    g.pasos([
        "Elija **Catálogo > Autorizar alta de vinos**. La tabla lista las solicitudes de alta pendientes.",
        "Seleccione una solicitud. El panel **Detalle de la solicitud** muestra el tipo, quién la emitió y todos los datos del vino.",
        "Para aprobar, presione **Autorizar**. El sistema muestra “Vino autorizado correctamente.” y el vino pasa a Activo.",
        "Para rechazar, escriba el **Motivo de rechazo** (obligatorio, hasta 200 caracteres) y presione **Rechazar alta**. Confirme la pregunta; el vino pasa a Rechazado y el sistema muestra “Alta rechazada. El vino quedó rechazado.”",
    ])
    g.nota("Si la solicitud fue emitida por usted, los botones **Autorizar** y **Rechazar alta** están deshabilitados (RN-01). "
           "Si otra persona resolvió la misma solicitud antes, el sistema lo informa y actualiza la lista.")

    g.h2("Corregir y reproponer un vino rechazado")
    g.pasos([
        "En **Catálogo > Gestionar catálogo de vinos**, seleccione el vino con estado Rechazado. La columna de motivo de rechazo explica qué corregir.",
        "Presione **Corregir y reproponer**. El formulario se carga con los datos del vino; el SKU queda bloqueado y se muestra el aviso “Corrigiendo un vino rechazado: al guardar se emite una nueva solicitud de alta.”",
        "Corrija los datos necesarios.",
        "Presione **Proponer alta**. El sistema muestra “Vino repropuesto correctamente. Se emitió una nueva solicitud de alta, pendiente de autorización.”",
    ])
    g.p("El vino vuelve a Pendiente con una nueva solicitud; la anterior queda como historial. Quien repropone pasa a ser el emisor de la "
        "nueva solicitud, por lo que no podrá resolverla.")

    g.h2("Solicitar la descontinuación de un vino (baja)")
    g.p("Permiso requerido: **Gestionar catálogo de vinos**. Regla RN-15: un vino solo puede descontinuarse con stock cero.", mantener=True)
    g.pasos([
        "Si el vino tiene botellas, primero deje el stock en cero con un ajuste de inventario (sección 4.7).",
        "En **Catálogo > Gestionar catálogo de vinos**, seleccione el vino activo.",
        "Presione **Solicitar descontinuación** y confirme “¿Solicitar la descontinuación del vino '…'?”.",
        "El sistema muestra “Descontinuación solicitada, pendiente de autorización.” El vino sigue Activo hasta que se autorice la baja.",
    ])
    g.p("El botón solo está habilitado para un vino Activo que no tenga ya una baja pendiente. Si el vino conserva stock, el sistema muestra: "
        "“No se puede descontinuar un vino con stock (N botellas). Registre un ajuste de inventario para dejarlo en 0 y luego solicite la baja.”")

    g.h2("Autorizar o rechazar una descontinuación")
    g.pasos([
        "Elija **Catálogo > Autorizar descontinuación de vinos** (permiso **Autorizar alta de vinos**).",
        "Seleccione la solicitud y revise el **Detalle de la solicitud**.",
        "Para aprobar, presione **Autorizar baja** y confirme. El sistema muestra “Vino descontinuado correctamente.”",
        "Para rechazar, escriba el **Motivo de rechazo** y presione **Rechazar baja**. El sistema muestra “Solicitud de descontinuación rechazada. El vino sigue activo.”",
    ])
    g.nota("Al autorizar, el sistema vuelve a verificar que el stock sea cero, porque pudo ingresar mercadería desde la solicitud. "
           "Si cambió, se informa “No se pudo autorizar la baja: el vino cambió de estado o de stock…” y el trámite no se completa. "
           "La regla de separación de funciones (RN-01) también rige para las bajas.")

    g.h2("Registrar una entrada de stock")
    g.p("Permiso requerido: **Gestionar catálogo de vinos**. Solo se pueden registrar entradas para vinos Activos.", mantener=True)
    g.pasos([
        "Elija **Catálogo > Registrar entrada de stock**.",
        "Seleccione el **Vino**. El sistema muestra el **Stock actual** y, en la tabla, los movimientos del vino (más recientes primero) con fecha y hora, tipo, cantidad, motivo y responsable.",
        "Indique la **Cantidad** (mayor que cero) y el **Motivo** (obligatorio; por ejemplo, el número de remito de la bodega).",
        "Presione **Registrar entrada**. El sistema muestra “Entrada de stock registrada correctamente.” y actualiza el stock.",
    ])
    g.p("Mensajes posibles: “La cantidad debe ser mayor a cero.”, “El motivo es obligatorio.” y “El vino debe estar autorizado para registrar movimientos de stock.”")

    g.h2("Registrar un ajuste de inventario")
    g.p("Sirve para corregir diferencias entre el stock del sistema y el real (rotura, merma, conteo físico). Permiso: **Gestionar catálogo de vinos**.",
        mantener=True)
    g.pasos([
        "Elija **Catálogo > Registrar ajuste de inventario**.",
        "Seleccione el **Vino** y, en **Sentido**, indique **Aumenta el stock** o **Disminuye el stock**.",
        "Cargue la **Cantidad** (mayor que cero) y el **Motivo** (obligatorio, por ejemplo: rotura, merma o corrección de conteo).",
        "Presione **Registrar ajuste**. El sistema muestra “Ajuste de inventario registrado correctamente.”",
    ])
    g.nota("Un ajuste que disminuya el stock por debajo de cero se rechaza con el mensaje “Stock insuficiente: el ajuste dejaría el stock por debajo de cero (stock actual: N).” "
           "Los movimientos no se pueden editar ni borrar; un error se corrige con otro movimiento en sentido contrario.")

    g.h2("Consultar la alerta de stock mínimo")
    g.pasos([
        "Elija **Catálogo > Consultar alerta de stock mínimo**.",
        "La tabla lista los vinos Activos cuyo stock actual está por debajo de su stock mínimo, con SKU, nombre, bodega, stock mínimo, stock actual y el faltante. Los de mayor faltante aparecen primero.",
        "Si no hay ninguno, el sistema muestra “No hay vinos por debajo del stock mínimo.”",
    ])
    g.p("La alerta es una consulta que usted realiza cuando la necesita: el sistema no emite avisos automáticos al iniciar sesión.")

    # ---------------- 5 ----------------
    g.h1("Proceso 2: Curación y armado de cajas")
    g.h2("Cómo funciona el proceso")
    g.p("Cada mes el club prepara una caja de vinos para cada socio, ajustada a su presupuesto y a sus gustos. El trabajo se reparte entre "
        "cuatro funciones, cada una con su pantalla:", mantener=True)
    g.tabla(["Paso", "Rol habitual", "Pantalla", "Resultado"], [
        ["1. Registrar al socio y sus preferencias", "Atención al Socio", "Gestionar Socios", "Socio activo con presupuesto mensual y varietales preferidos."],
        ["2. Armar la caja del período", "Curador de Producto", "Armar Caja Mensual", "Caja en estado Armada."],
        ["3. Revisar la caja y reemplazar vinos sin stock", "Encargado de Depósito", "Picking y Sustituciones", "Sustituciones registradas (opcional)."],
        ["4. Despachar la caja", "Logística", "Despachar Cajas", "Caja Despachada, stock descontado y remito PDF."],
        ["5. Consultar despachos y reimprimir remitos", "Logística", "Historial de Despachos", "Remito regenerado."],
    ], [4.6, 3.3, 3.7, 5.0])
    g.p("Reglas que conviene conocer:")
    g.bullets([
        "Solo se pueden incluir en una caja vinos **Activos con stock mayor que cero**.",
        "El total de la caja **no puede superar el presupuesto mensual** del socio.",
        "Un socio no puede tener más de una caja no cancelada por período.",
        "**Armar una caja no descuenta stock**. La salida de stock se registra recién al despachar.",
        "Al armar la caja, el nombre y el precio de cada vino se congelan: cambios posteriores en el catálogo no alteran cajas ya armadas.",
        "Las sustituciones nunca modifican la caja original: se agregan como registros y la composición final se calcula a partir de ellos.",
    ])
    g.p("Una caja pasa por los estados **Armada**, **Despachada** y, de manera excepcional, **Cancelada**. La versión actual no tiene una pantalla "
        "para cancelar cajas: consulte al administrador si necesita hacerlo.")

    g.h2("Gestionar socios")
    g.p("Permiso requerido: **Gestionar socios**.", mantener=True)
    g.h3("Dar de alta un socio")
    g.pasos([
        "Elija **Club de Socios > Gestionar Socios**.",
        "Presione **Nuevo** si el formulario conserva datos de otro socio.",
        "Complete **Apellido**, **Nombre**, **Domicilio** y **Presupuesto mensual** (obligatorios) y, si corresponde, **Email** y **Teléfono**.",
        "En **Varietales preferidos** marque los varietales que el socio prefiere (la lista contiene los varietales presentes en el catálogo).",
        "Deje marcada la casilla **Activo** y presione **Guardar**. El sistema muestra “Socio guardado correctamente.”",
    ])
    g.h3("Modificar un socio")
    g.pasos([
        "En la tabla de socios, seleccione la fila. Sus datos se cargan en el formulario.",
        "Cambie lo necesario y presione **Guardar**.",
    ])
    g.nota("La tabla muestra solo socios activos. Si desmarca **Activo** y guarda, el socio deja de aparecer en la tabla y en **Armar Caja Mensual**. "
           "Las preferencias de varietales solo ordenan los vinos candidatos: nunca excluyen otros vinos.")
    g.tabla(["Mensaje", "Causa"], [
        ["“El nombre es obligatorio.” / “El apellido es obligatorio.” / “El domicilio es obligatorio.”", "Falta un dato obligatorio."],
        ["“El presupuesto mensual debe ser mayor a cero.”", "El presupuesto es cero."],
    ], [10.6, 6.0])

    g.h2("Armar la caja mensual")
    g.p("Permiso requerido: **Armar cajas mensuales**.", mantener=True)
    g.pasos([
        "Elija **Club de Socios > Armar Caja Mensual**.",
        "Seleccione el **Socio**. El sistema muestra su **Presupuesto mensual** y la lista **Vinos disponibles para el socio**, con los varietales preferidos primero y resaltados en la columna **Preferido**.",
        "Escriba el **Período (AAAA-MM)**, por ejemplo 2026-11. El mes debe estar entre 01 y 12.",
        "Seleccione un vino de la lista, indique la **Cantidad** y presione **Agregar a la caja**. Repita para cada vino. Si agrega dos veces el mismo vino, las cantidades se suman en una sola línea.",
        "Controle el **Total**: se muestra en rojo si supera el presupuesto. Para quitar una línea por error, haga doble clic sobre ella en **Vinos en la caja**.",
        "Presione **Confirmar armado**. El sistema muestra “Caja armada correctamente.” y limpia el formulario.",
    ])
    g.nota("El botón **Confirmar armado** permanece deshabilitado mientras falte un socio, no haya ninguna línea, el período no tenga el formato "
           "AAAA-MM o el total supere el presupuesto.")
    g.tabla(["Mensaje", "Causa"], [
        ["“La caja debe tener al menos un vino.”", "No se agregó ninguna línea."],
        ["“Uno de los vinos seleccionados ya no está disponible para este socio.”", "El vino dejó de ser candidato (sin stock o descontinuado) mientras armaba la caja."],
        ["“El total de la caja supera el presupuesto mensual del socio.”", "La suma de cantidad por precio excede el presupuesto."],
        ["“El socio ya tiene una caja armada para ese período.”", "Ya existe una caja no cancelada del socio para ese AAAA-MM."],
    ], [8.6, 8.0])

    g.h2("Picking y sustituciones")
    g.p("Permiso requerido: **Registrar picking y sustituciones**. La pantalla permite revisar qué debe prepararse y reemplazar vinos "
        "cuando falta stock.", mantener=True)
    g.pasos([
        "Elija **Club de Socios > Picking y Sustituciones**.",
        "En **Cajas armadas**, seleccione una caja. Se muestran su **Composición efectiva** (vino, cantidad, **Stock disponible** y **Sustituido**) y el **Historial de sustituciones**. Las líneas cuyo stock no alcanza se resaltan en rojo claro.",
        "Para reemplazar un vino, seleccione la línea en la composición.",
        "En **Reemplazar por** elija el vino sustituto (la lista contiene los vinos candidatos del socio).",
        "Escriba el **Motivo** y presione **Registrar sustitución**. El sistema muestra “Sustitución registrada correctamente.”",
    ])
    g.bullets([
        "El reemplazo debe ser un vino válido para ese socio y el nuevo total no puede superar el presupuesto con el que se armó la caja.",
        "El sistema no exige que falte stock para sustituir: el resaltado en rojo es solo una guía.",
        "Una línea ya sustituida puede volver a sustituirse; el historial conserva toda la cadena.",
        "Las sustituciones solo se registran sobre cajas Armadas y no pueden editarse ni borrarse.",
        "Si usted armó la caja, el botón **Registrar sustitución** está deshabilitado (RN-10).",
    ])
    g.tabla(["Mensaje", "Causa"], [
        ["“Seleccioná una línea de la composición.” / “Seleccioná el vino de reemplazo.” / “Ingresá un motivo para la sustitución.”", "Falta una selección o el motivo."],
        ["“Quien armó la caja no puede registrar sustituciones sobre ella.”", "Regla RN-10."],
        ["“Solo se pueden registrar sustituciones sobre una caja Armada.”", "La caja ya fue despachada o cancelada."],
        ["“El vino de reemplazo no está disponible para este socio.”", "El sustituto ya no es candidato."],
        ["“El reemplazo supera el presupuesto de la caja.”", "El total con el reemplazo excede el presupuesto congelado al armar."],
    ], [9.6, 7.0])

    g.h2("Despachar una caja")
    g.p("Permiso requerido: **Despachar cajas**.", mantener=True)
    g.pasos([
        "Elija **Club de Socios > Despachar Cajas**.",
        "En **Cajas armadas**, seleccione la caja. Revise la **Composición efectiva**: las líneas sin stock suficiente se resaltan en rojo y bloquean el despacho.",
        "Presione **Despachar caja** y confirme “¿Confirma el despacho de esta caja? La acción no se puede deshacer.”",
        "El sistema registra la salida de stock de cada vino, pasa la caja a Despachada, genera el remito PDF y lo abre automáticamente.",
        "Se muestra “Caja despachada correctamente.” y la caja desaparece de la lista de armadas.",
    ])
    g.nota("El despacho es atómico: si falta stock en cualquier línea no se descuenta nada. Si el despacho se confirma pero el remito no puede "
           "generarse, el sistema avisa “No se pudo generar el remito de despacho; la caja fue despachada igualmente.” y la caja queda despachada; "
           "el remito puede regenerarse desde el historial.")
    g.tabla(["Mensaje", "Causa"], [
        ["“Solo se puede despachar una caja Armada.”", "La caja ya fue despachada o cancelada."],
        ["“Quien armó la caja no puede despacharla.”", "Regla RN-10."],
        ["“No hay stock suficiente de \"…\" para despachar la caja.”", "Alguna línea efectiva no tiene stock disponible. Reponga stock (entrada o ajuste) o registre una sustitución."],
        ["“No se pudo despachar la caja (verificar estado o permisos).”", "La caja cambió de estado mientras la operaba. Actualice e intente de nuevo."],
    ], [8.6, 8.0])

    g.h2("El remito de despacho (PDF)")
    g.p("El remito se genera en formato A4 vertical, siempre en español, con la composición **efectiva** de la caja (incluyendo sustituciones y "
        "sus precios). Contiene:", mantener=True)
    g.bullets([
        "Título “REMITO DE DESPACHO” y “Club de Vinos”.",
        "Número de caja, socio, período, fecha de despacho, domicilio y teléfono del socio (si no están cargados se imprime “(no informado)”) y el usuario responsable del despacho.",
        "Una tabla con cantidad, vino, precio unitario y subtotal por línea, y el total.",
        "Una línea para la “Firma de recepción”.",
    ])
    g.p("El archivo se guarda en **Documentos\\ClubDeVinos\\Remitos** con el nombre `Remito_Caja<N>.pdf`, donde N es el número de caja, y se abre con "
        "el lector de PDF predeterminado de Windows.")

    g.h2("Historial de despachos y regeneración del remito")
    g.pasos([
        "Elija **Club de Socios > Historial de Despachos**. La tabla lista las cajas despachadas, de la más reciente a la más antigua, con socio, período, fecha de despacho y quién la despachó.",
        "Seleccione una caja y presione **Regenerar remito**. Se vuelve a crear el PDF y se abre.",
    ])
    g.p("Regenerar sobrescribe el archivo `Remito_Caja<N>.pdf` existente: no se crean copias con fecha. El remito regenerado refleja la composición "
        "efectiva de la caja, igual que el original.")

    # ---------------- 6 ----------------
    g.h1("Proceso 3: Análisis de rotación y reposición")
    g.h2("Para qué sirve")
    g.p("El análisis ayuda a decidir qué vinos reponer y cuáles no. Cruza el stock actual con lo despachado en un período y entrega, para cada vino "
        "activo, su consumo mensual, cuántos meses de cobertura le quedan, una **clasificación** de su rotación y una **recomendación** de reposición. "
        "Permiso requerido: **Analizar rotación de stock** (menú **Catálogo > Analizar rotación de stock**).")
    g.h2("Cómo se calcula")
    g.tabla(["Dato", "Definición"], [
        ["Vendidas", "Unidades despachadas en cajas durante el período (se respetan las sustituciones)."],
        ["Pérdidas", "Unidades que salieron por ajustes de inventario negativos en el período (roturas, mermas)."],
        ["Socios atendidos", "Cantidad de socios distintos que recibieron el vino en cajas despachadas del período."],
        ["Consumo mensual", "Vendidas dividido por (días del período / 30)."],
        ["Cobertura (meses)", "Stock actual dividido por el consumo mensual. Se muestra “—” si no hubo consumo."],
        ["Stock actual", "Stock del día de hoy, no el del final del período analizado."],
        ["Cantidad sugerida", "Solo para vinos con recomendación Reponer urgente o Reponer: lo que falta para llegar a la cobertura objetivo sin quedar bajo el stock mínimo. "
                              "Fórmula: máx(consumo mensual × cobertura objetivo; stock mínimo) − stock actual, redondeado hacia arriba, nunca menor a 0 en Reponer y nunca menor a 1 en Reponer urgente. Para Mantener y Sobrestock es 0."],
    ], [4.0, 12.6])
    g.h3("Clasificación de la rotación")
    g.tabla(["Clasificación", "Condición", "Umbral inicial"], [
        ["Sin movimiento", "No se vendió ninguna unidad en el período.", "—"],
        ["Alta rotación", "Consumo mensual mayor o igual al umbral de alta rotación.", "4 unidades por mes"],
        ["Baja rotación", "Hubo ventas, pero el consumo mensual es menor al umbral de baja rotación.", "1,5 unidades por mes"],
        ["Rotación media", "El resto de los casos con ventas.", "—"],
    ], [3.6, 9.0, 4.0])
    g.h3("Recomendación de reposición")
    g.p("Se evalúa en el orden siguiente; la primera condición que se cumple determina la recomendación:", mantener=True)
    g.tabla(["Prioridad", "Recomendación", "Condición", "Umbral inicial"], [
        ["1", "Reponer urgente", "El stock actual es menor o igual al stock mínimo del vino, o la cobertura es menor al umbral urgente.", "1 mes"],
        ["2", "Sobrestock / no reponer", "No hubo consumo (y el stock supera el mínimo), o la cobertura es mayor al umbral de sobrestock.", "6 meses"],
        ["3", "Reponer", "La cobertura es menor al umbral de reposición.", "2 meses"],
        ["4", "Mantener", "Ninguna de las anteriores.", "—"],
    ], [1.9, 4.2, 8.2, 2.3], centrar_desde=None)

    g.h2("Configurar los umbrales")
    g.p("En el recuadro **Configuración del análisis** puede ajustar los umbrales. Los valores iniciales son los de las tablas anteriores:", mantener=True)
    g.tabla(["Campo", "Valor inicial", "Condición de validez"], [
        ["Días de análisis", "90", "Entre 1 y 3650."],
        ["Alta rotación desde (u/mes)", "4", "Debe ser mayor que el umbral de baja rotación."],
        ["Baja rotación bajo (u/mes)", "1,5", "Mayor que cero."],
        ["Reposición urgente bajo (meses)", "1", "Mayor que cero."],
        ["Reponer bajo (meses)", "2", "Mayor que el de reposición urgente."],
        ["Sobrestock sobre (meses)", "6", "Mayor que el de reponer."],
        ["Cobertura objetivo (meses)", "3", "Mayor que el de reponer y no mayor que el de sobrestock."],
    ], [6.4, 3.0, 7.2])
    g.p("Presione **Guardar configuración** para conservar los valores. Si alguna condición no se cumple, el sistema lo informa, por ejemplo: "
        "“El umbral de alta rotación debe ser mayor al de baja rotación.” o “La cobertura de sobrestock debe ser mayor a la de reposición.”")
    g.h3("Dónde se guarda la configuración")
    g.p("La configuración se guarda como un archivo XML en `%LocalAppData%\\ClubDeVinos\\configuracion_rotacion.xml` (por ejemplo, "
        "`C:\\Users\\<usuario>\\AppData\\Local\\ClubDeVinos`). Tenga en cuenta que:")
    g.bullets([
        "El archivo se crea solo, con los valores iniciales, la primera vez que abre la pantalla.",
        "Pertenece al usuario de Windows y a la computadora: cada persona puede tener sus propios umbrales.",
        "Al presionar **Analizar**, el sistema guarda en el archivo lo que hay en pantalla y analiza con lo que releyó del archivo. Si no puede guardarlo o leerlo, no realiza el análisis.",
        "Si el archivo está dañado o tiene valores inválidos, el sistema avisa “El archivo de configuración de rotación estaba dañado o tenía valores inválidos. Se usan los valores por defecto hasta que guarde la configuración.”",
    ])

    g.h2("Ejecutar el análisis")
    g.pasos([
        "Elija **Catálogo > Analizar rotación de stock**.",
        "Revise el período. Por omisión es **Desde** (hoy menos los días de análisis más uno) y **Hasta** (hoy), de modo que el período incluye el día de hoy. Si cambia **Días de análisis**, la fecha **Desde** se recalcula; también puede editar ambas fechas a mano.",
        "Si lo desea, ajuste los umbrales.",
        "Presione **Analizar**.",
        "Lea el resumen sobre la tabla (cantidad de vinos por recomendación) y la tabla de resultados. Las filas se colorean según la recomendación: rojo, Reponer urgente; ámbar, Reponer; verde, Mantener; azul, Sobrestock / no reponer.",
    ])
    g.p("La tabla muestra SKU, nombre, bodega, stock actual, stock mínimo, vendidas, pérdidas, consumo mensual, cobertura, socios atendidos, "
        "clasificación, recomendación y cantidad sugerida a reponer; el resumen sobre la tabla agrega el total de unidades sugeridas a reponer. Se ordena de la recomendación más urgente a la menos urgente y, dentro de cada una, por menor cobertura. "
        "Solo se incluyen vinos Activos.")
    g.nota("Si la fecha **Hasta** es anterior a hoy, aparece un aviso: el stock considerado es el de hoy, por lo que la cobertura puede no ser representativa "
           "para períodos pasados. El aviso no impide el análisis.", tipo="Importante")

    g.h3("Pestañas Detalle y Gráficos")
    g.p("Los resultados se muestran en dos pestañas: **Detalle** (la tabla) y **Gráficos**. Los gráficos se actualizan cada vez que presiona **Analizar** y cambian de idioma junto con la interfaz. "
        "Si no entran todos en la ventana, use la barra de desplazamiento. Los colores son siempre los mismos: rojo, Reponer urgente; naranja, Reponer; verde, Mantener; azul grisáceo, Sobrestock / no reponer.")
    g.tabla(["Gráfico", "Qué muestra"], [
        ["Consumo mensual vs stock actual", "Barras por vino (los más urgentes, hasta 10; el título indica cuántos se muestran) con el consumo mensual y el stock actual, y un rombo negro con el stock mínimo. Un stock por debajo del rombo indica reposición."],
        ["Distribución por recomendación", "Dona con la cantidad de vinos de cada recomendación y su porcentaje."],
        ["Unidades sugeridas a reponer", "Barras con la cantidad sugerida de los vinos a reponer (hasta 10), coloreadas por recomendación. Si ningún vino necesita reposición, muestra “Sin datos para graficar”."],
    ], [5.6, 11.0])
    g.tabla(["Mensaje", "Causa"], [
        ["“La fecha desde no puede ser posterior a la fecha hasta.”", "El rango de fechas está invertido."],
        ["“No se pudo guardar o leer el archivo de configuración, por lo que no se realizó el análisis. Verifique los permisos de la carpeta …”", "No se puede escribir o leer en %LocalAppData%\\ClubDeVinos."],
        ["“No se pudo realizar el análisis de rotación.”", "Error inesperado al consultar los datos. Reintente y, si persiste, avise al administrador."],
    ], [9.6, 7.0])

    g.h2("Exportar el informe en PDF")
    g.pasos([
        "Después de ejecutar un análisis con resultados, presione **Exportar a PDF**.",
        "El sistema genera el informe, lo abre y muestra “Informe generado en:” seguido de la ruta del archivo.",
    ])
    g.p("El informe se guarda en **Documentos\\ClubDeVinos\\Informes** con el nombre `Rotacion_AAAAMMDD_HHMMSS_mmm.pdf` (fecha y hora de generación, con milisegundos), "
        "de modo que cada exportación crea un archivo nuevo. Es un documento A4 apaisado que se emite en el idioma de la interfaz activo al exportar (los textos sin traducción salen en español; fechas y números mantienen el formato es-AR) y contiene: el título (en español, “INFORME DE ROTACIÓN Y REPOSICIÓN DE STOCK”), "
        "el período analizado, la fecha de generación, los umbrales usados, un resumen con la cantidad de vinos por recomendación y el total de unidades sugeridas a reponer, "
        "los gráficos (la primera página reúne el encabezado, el resumen y los gráficos de consumo vs stock y de distribución por recomendación; la segunda abre con las unidades sugeridas a reponer), "
        "la tabla completa con color por recomendación (incluye la columna “A reponer”) y una nota con las fórmulas. Refleja el **último análisis realizado**, no los cambios que haya hecho después en pantalla. "
        "Si no puede generarse, el sistema indica “No se pudo generar el informe en PDF. Verifique que la carpeta Documentos sea escribible.”")

    # ---------------- 7 ----------------
    if HAY_AYUDA:
        g.h1("Ayuda dentro del sistema")
        g.p("El sistema incluye una ayuda en línea, disponible para cualquier usuario con sesión iniciada y sin permisos especiales. Explica, por área, qué hace cada función, "
            "quién puede usarla, los pasos, las reglas de negocio y el significado de los mensajes. Esta guía y la ayuda en línea son complementarias.", mantener=True)
        g.pasos([
            "Presione **F1** en cualquier ventana: la ayuda se abre en el tema de esa pantalla. En el menú principal, F1 abre la página de bienvenida. También puede usar **Ayuda > Ayuda en línea (F1)**.",
            "Elija un tema en el árbol de la izquierda para leerlo a la derecha.",
            "Para buscar, escriba una palabra en el cuadro **Buscar**: el árbol muestra solo los temas que la incluyen. Borre el texto para ver todos.",
            "**Ayuda > Acerca de** muestra el nombre del producto y su versión.",
        ])
        g.p("El contenido de la ayuda está en español aunque cambie el idioma de la interfaz.")

    # ---------------- 8 ----------------
    g.h1("Preguntas frecuentes y mensajes de error")
    g.h2("Preguntas frecuentes")
    g.tabla(["Pregunta", "Respuesta"], [
        ["No veo una opción del menú que necesito.",
         "El menú solo muestra las funciones permitidas por sus permisos. Pida al administrador que revise los roles asignados a su usuario."],
        ["¿Por qué no puedo autorizar una solicitud que cargué yo?",
         "Por la regla de separación de funciones RN-01: quien emite una solicitud no puede resolverla. Debe hacerlo otro administrador."],
        ["¿Por qué está deshabilitado el botón de despachar o de registrar sustituciones?",
         "Porque usted armó esa caja (RN-10). Otra persona debe despacharla o sustituir sus vinos."],
        ["Propuse un vino y no lo veo otro compañero.",
         "Un vino Pendiente solo lo ve quien emitió la solicitud. Cuando se aprueba, pasa a Activo y lo ven todos."],
        ["No puedo solicitar la descontinuación de un vino.",
         "El vino debe estar Activo, sin una baja pendiente y con stock cero. Si tiene botellas, registre antes un ajuste de inventario."],
        ["Un vino no aparece entre los candidatos al armar la caja.",
         "Los candidatos son los vinos Activos con stock mayor que cero. Registre una entrada de stock o verifique que el vino esté aprobado."],
        ["¿Se descuenta el stock al armar la caja?",
         "No. El stock se descuenta recién al despachar."],
        ["¿Puedo deshacer un despacho?",
         "No. El sistema lo advierte antes de confirmar y no incluye una función de reversión."],
        ["¿Dónde quedan los PDF?",
         "Los remitos en Documentos\\ClubDeVinos\\Remitos y los informes de rotación en Documentos\\ClubDeVinos\\Informes."],
        ["Cambié el idioma pero el PDF sigue en español.",
         "El remito de despacho siempre se emite en español. El informe de rotación sale en el idioma activo al exportar: cambie el idioma y vuelva a exportar. Los mensajes de aviso del resto del sistema también se muestran en español."],
        ["El análisis de rotación no muestra todos los vinos.",
         "Solo analiza vinos Activos. Los Pendientes, Rechazados y Descontinuados no se incluyen."],
        ["Olvidé mi contraseña.",
         "La aplicación no permite recuperarla. Consulte con el administrador del sistema (Guía de Operación)."],
        ["Mi cuenta está bloqueada.",
         "Se bloquea tras tres contraseñas incorrectas consecutivas. Un administrador puede desbloquearla desde Administración > Gestión de usuarios."],
    ], [5.6, 11.0])

    g.h2("Resumen de mensajes de error y de validación")
    g.p("Además de los mensajes ya detallados en cada procedimiento, estos son otros avisos frecuentes:", mantener=True)
    g.tabla(["Mensaje", "Significado y acción"], [
        ["“Seleccione un vino rechazado.”", "Presionó **Corregir y reproponer** sin elegir un vino en estado Rechazado."],
        ["“Seleccioná un vino.” / “Seleccione una solicitud de alta pendiente.”", "Falta elegir una fila de la tabla antes de actuar."],
        ["“Debe indicar el motivo del rechazo.”", "El rechazo de una solicitud exige un motivo."],
        ["“El motivo del rechazo no puede superar los 200 caracteres.”", "Acorte el motivo."],
        ["“No puede autorizar/rechazar una solicitud que usted mismo emitió.”", "Regla RN-01. Pida a otra persona con permiso que la resuelva."],
        ["“La solicitud ya fue resuelta.”", "Otra persona resolvió la solicitud antes. Actualice la lista."],
        ["“El vino ya tiene una baja solicitada, pendiente de autorización.”", "Ya existe una baja pendiente para ese vino."],
        ["“Solo se puede solicitar la baja de un vino activo.”", "El vino está Pendiente, Rechazado o Descontinuado."],
        ["“Stock insuficiente: el ajuste dejaría el stock por debajo de cero…”", "Reduzca la cantidad del ajuste."],
        ["“Informe generado en: …”", "El informe se creó correctamente en la ruta indicada."],
    ], [7.4, 9.2])
    g.p("Si el sistema muestra un error que no figura en esta guía, anote el mensaje exacto, la pantalla y la acción que estaba realizando, y "
        "comuníquese con el administrador del sistema.")

    # ---------------- 9 ----------------
    g.h1("Glosario")
    g.tabla(["Término", "Definición"], [
        ["Alta", "Incorporación de un vino al catálogo. Requiere autorización de un administrador."],
        ["Añada", "Año de cosecha del vino."],
        ["Baja (descontinuación)", "Retiro de un vino del catálogo. Requiere stock cero y autorización; el vino no se borra, pasa a Descontinuado."],
        ["Bitácora", "Registro de actividad del sistema (inicios de sesión y operaciones relevantes). Es de solo lectura."],
        ["Bodega", "Productor o proveedor del vino."],
        ["Caja mensual", "Conjunto de vinos que se prepara para un socio en un período (AAAA-MM)."],
        ["Candidato", "Vino que cumple las condiciones para entrar en una caja: Activo y con stock."],
        ["Cobertura", "Cantidad de meses que alcanza el stock actual al ritmo de consumo del período."],
        ["Composición efectiva", "Contenido real de una caja, una vez aplicadas las sustituciones."],
        ["Consumo mensual", "Unidades vendidas por mes, calculadas sobre el período analizado."],
        ["Despacho", "Entrega de la caja: descuenta el stock y emite el remito."],
        ["Dígitos verificadores", "Mecanismo con el que el sistema detecta modificaciones de los datos de usuarios hechas fuera de la aplicación."],
        ["Kardex", "Registro cronológico de entradas y salidas de stock de cada vino."],
        ["Maridaje", "Comidas con las que se recomienda el vino."],
        ["Permiso", "Autorización para usar una función concreta del sistema."],
        ["Picking", "Preparación física de una caja a partir de su composición."],
        ["Presupuesto mensual", "Tope de gasto de un socio para su caja de cada período."],
        ["Remito", "Documento PDF que acompaña la entrega de una caja."],
        ["Rol", "Conjunto de permisos que se asigna a los usuarios."],
        ["RN (regla de negocio)", "Regla que el sistema aplica siempre, por ejemplo RN-01 (separación de funciones) o RN-15 (baja solo con stock cero)."],
        ["Separación de funciones", "Principio por el cual quien inicia una operación no puede ser quien la autoriza o la completa."],
        ["SKU", "Código único que identifica a un vino en el catálogo."],
        ["Snapshot (dato congelado)", "Copia del nombre y precio de un vino tomada al armar la caja."],
        ["Sobrestock", "Exceso de stock respecto del consumo."],
        ["Socio", "Persona inscripta en el club que recibe una caja mensual."],
        ["Solicitud de autorización", "Trámite de alta o de baja de un vino que queda Pendiente hasta que un administrador lo apruebe o lo rechace."],
        ["Stock mínimo", "Cantidad por debajo de la cual el vino aparece en la alerta de reposición."],
        ["Sustitución", "Reemplazo de un vino de la caja por otro, con motivo y responsable registrados."],
        ["Umbral", "Valor límite configurable que usa el análisis de rotación para clasificar y recomendar."],
        ["Varietal", "Variedad de uva del vino (Malbec, Chardonnay, etc.)."],
    ], [4.4, 12.2], tam=9.5)
    return g.guardar()


# --------------------------------------------------------------------------------------
# GUIA DE OPERACION
# --------------------------------------------------------------------------------------
def guia_operacion():
    g = Guia("Guía de Operación",
             "Instalación, puesta en marcha, operación diaria, respaldos e integridad del sistema Club de Vinos",
             "Guia_de_Operacion.docx")

    # ---------------- 1 ----------------
    g.h1("Introducción")
    g.h2("Propósito y destinatarios")
    g.p("Esta guía está dirigida a quien instala, administra y da soporte al sistema Club de Vinos: el **administrador del sistema** "
        "(responsable de la computadora, de SQL Server y de los respaldos) y las personas con el rol **Administrador** de la aplicación "
        "(responsables de usuarios, roles, idiomas y bitácora). Cubre la instalación con el paquete MSI, el primer inicio, los datos de "
        "demostración, la operación diaria, los respaldos, el control de integridad, la administración de usuarios y perfiles, y el "
        "procedimiento de actualización.")
    g.p("Para el uso funcional de la aplicación consulte la Guía del Usuario; para compilar, extender o diagnosticar el código, la Guía de Mantenimiento.")
    g.h2("Modelo de despliegue")
    g.p("El sistema se despliega en **una sola computadora**: la aplicación de escritorio (`CAPAS.exe`) y la base de datos SQL Server (`BDCAPAS`) "
        "residen en el mismo equipo. La aplicación se conecta con autenticación de Windows a la instancia **predeterminada** (`.`) del equipo "
        "(cadena de conexión fija en `DAL\\Acceso.cs`). Esto tiene dos consecuencias que se retoman más adelante:")
    g.bullets([
        "Si la base se crea en una instancia con nombre (por ejemplo `.\\SQLEXPRESS`) o en un servidor remoto, la aplicación **no se conectará** hasta que se modifique la cadena de conexión y se recompile (Guía de Mantenimiento).",
        "No hay una arquitectura cliente-servidor lista para varios equipos: cada equipo con la aplicación necesita su propia base local.",
    ])
    g.h2("Requisitos")
    g.tabla(["Componente", "Requisito"], [
        ["Sistema operativo", "Windows 10 u 11."],
        ["Plataforma", ".NET Framework 4.8."],
        ["Base de datos", "SQL Server local (por ejemplo, SQL Server Express) con la instancia predeterminada para que la aplicación pueda conectarse."],
        ["Permisos para instalar", "Cuenta de Windows con privilegios de administrador (el MSI es por equipo y pide elevación). La base se crea con autenticación de Windows."],
        ["Herramienta de línea de comandos", "`sqlcmd` (herramientas de SQL Server), necesaria solo para ejecutar el script a mano o usar `EjecutarScript.cmd`."],
        ["Visor de PDF", "Cualquier lector de PDF, para abrir remitos e informes."],
    ], [4.4, 12.2])

    # ---------------- 2 ----------------
    g.h1("Instalación con el paquete MSI")
    g.h2("Qué instala el paquete")
    g.p("El instalador es el archivo `ClubDeVinos.msi` (interfaz en español), generado en `Instalador\\bin\\x64\\Release\\es-ES\\`. "
        "La versión del paquete en el momento de redactar esta guía es **%s**." % VERSION_PRODUCTO, mantener=True)
    g.tabla(["Elemento", "Detalle"], [
        ["Carpeta de la aplicación", "`C:\\Program Files\\Club de Vinos` (modificable en el asistente): `CAPAS.exe`, bibliotecas y configuración."],
        ["Accesos directos", "“Club de Vinos” en el menú Inicio y en el Escritorio."],
        ["Base de datos", "`BDCAPAS` en la instancia elegida, creada si no existe, y ejecución de `DAL\\script.sql` (tablas, procedimientos, datos iniciales, datos de demostración)."],
        ["Carpeta `BaseDeDatos`", "Dentro de la carpeta de instalación: copia de `script.sql` y `EjecutarScript.cmd`, útiles para crear la base más tarde."],
        ["Registro de Windows", "`HKLM\\Software\\Club de Vinos`, valor `DatabaseInstalled = 1` (cuando se crea la base)."],
    ], [4.4, 12.2])
    g.nota("La base de datos **nunca se elimina** al desinstalar o actualizar. La carpeta de instalación es de solo lectura para los usuarios comunes, por lo que "
           "la aplicación escribe sus archivos propios en la carpeta del usuario (sección 8).")

    g.h2("Antes de instalar")
    g.bullets([
        "Verifique que el equipo tenga Windows 10/11 y .NET Framework 4.8.",
        "Verifique que haya una instancia de SQL Server instalada. Para el funcionamiento normal use la instancia predeterminada (`MSSQLSERVER`, que el asistente muestra como “(local) - instancia predeterminada (MSSQLSERVER)”).",
        "Cierre la aplicación si ya estaba instalada y haga un respaldo de la base si se trata de una actualización (sección 6).",
        "Ejecute el instalador con una cuenta con privilegios de administrador o acepte el aviso de Control de cuentas de usuario (UAC).",
    ])

    g.h2("Procedimiento de instalación")
    g.pasos([
        "Haga doble clic en `ClubDeVinos.msi` y acepte el aviso de UAC.",
        "En la pantalla de bienvenida presione **Siguiente**. El asistente no muestra pantalla de licencia.",
        "Elija la carpeta de instalación (por omisión, `C:\\Program Files\\Club de Vinos`) y presione **Siguiente**. Una ruta inválida bloquea el avance.",
        "En la pantalla **Instancia de SQL Server**, elija la instancia (ver 2.4) y presione **Siguiente**. El asistente valida el servicio y la conexión; este paso puede demorar hasta unos 35 segundos y, si hay un problema, muestra el motivo en español para corregirlo.",
        "En la pantalla de confirmación presione **Instalar**.",
        "Al finalizar, abra **Club de Vinos** desde el menú Inicio o el Escritorio (sección 3).",
    ])

    g.h2("La pantalla “Instancia de SQL Server”")
    g.p("Contiene una lista editable: se muestran las instancias detectadas en el equipo (registro de Windows en sus vistas de 64 y 32 bits más los "
        "servicios `MSSQLSERVER` y `MSSQL$*`) y también puede **escribir** un servidor o instancia (`servidor\\instancia`; use `.` para la predeterminada). "
        "Los nombres con instancia se traducen a `.\\NOMBRE`. Si existe la instancia predeterminada, queda seleccionada por omisión. Además hay:", mantener=True)
    g.bullets([
        "Un botón **Reintentar detección**, que vuelve a buscar instancias sin pisar lo que usted escribió.",
        "Una casilla **Instalar solo la aplicación, sin crear la base de datos ahora…**.",
    ])
    g.p("Al presionar **Siguiente** el instalador comprueba el estado del servicio de SQL Server (intenta iniciarlo, esperando hasta 20 segundos) y "
        "prueba un inicio de sesión de Windows contra `master`. Solo si la prueba es correcta se avanza.")

    g.h2("Casos especiales")
    g.p("El instalador contempla tres situaciones frecuentes. En ninguna deja la instalación a medias: si algo falla antes de copiar archivos, no queda nada instalado.",
        mantener=True)
    g.tabla(["Caso", "Qué ve el usuario", "Qué hace el instalador"], [
        ["1. No se pudo leer la lista de instancias (o se usa un servidor remoto)",
         "El texto “No se pudo obtener la lista de instancias de SQL Server de este equipo. Escriba el servidor y la instancia manualmente (por ejemplo .\\SQLEXPRESS) y presione Siguiente, o presione “Reintentar detección”.”",
         "No aborta. Valida el valor escrito al presionar Siguiente (prueba de conexión). **Reintentar detección** vuelve a buscar y reconstruye el diálogo."],
        ["2. El motor de SQL Server no está instalado",
         "Un mensaje que indica que el motor es necesario y sugiere descargar SQL Server Express (gratuito), con la casilla **Instalar solo la aplicación** y el botón **Reintentar detección**. Si presiona Siguiente con un servidor que no responde, el aviso dice: “No hay un motor de SQL Server instalado en este equipo (no se detectó ninguno). Instale SQL Server Express y presione Reintentar detección, o marque la casilla para instalar solo la aplicación.”",
         "Por seguridad, bloquea el avance mientras no haya un servidor válido (al cancelar no queda nada instalado). Con la casilla marcada, instala la aplicación y la carpeta `BaseDeDatos` sin crear la base."],
        ["3. SQL Server instalado pero el servicio está detenido",
         "Si logra iniciarlo, nada: la instalación continúa. Si no, “El servicio de SQL Server … está detenido y no se pudo iniciar…” con instrucciones (`services.msc` o `net start`) o, en la fase de ejecución, un cuadro con **Reintentar** y **Cancelar**.",
         "Inicia el servicio y espera hasta 20 segundos. Si el usuario no es administrador, la acción elevada lo inicia antes de copiar archivos. **Cancelar** revierte la instalación antes de copiar nada."],
    ], [3.6, 7.0, 6.0], tam=9)

    g.h3("Caso 1: no se pudo obtener la lista de instancias")
    g.pasos([
        "Escriba en la lista el servidor y la instancia en el formato `servidor\\instancia` (por ejemplo `.\\SQLEXPRESS`; para la instancia predeterminada, un punto).",
        "Presione **Siguiente**. Si la conexión falla, se muestra el error en español; corrija el nombre y vuelva a presionar **Siguiente**.",
        "Si acaba de instalar o iniciar SQL Server con el asistente abierto, presione **Reintentar detección**.",
    ])
    g.h3("Caso 2: el motor de SQL Server no está instalado")
    g.p("Tiene dos caminos:", mantener=True)
    g.bullets([
        "**Recomendado.** Cancele o deje abierto el asistente, instale SQL Server Express (instancia predeterminada), presione **Reintentar detección** y continúe.",
        "**Instalar solo la aplicación.** Marque la casilla y finalice la instalación. Más tarde, cuando haya un SQL Server, cree la base con `EjecutarScript.cmd` (sección 2.7).",
    ])
    g.p("Si abre la aplicación antes de crear la base, **Club de Vinos** no muestra la ventana de error de .NET: al iniciar prueba la conexión (espera unos segundos como máximo) y muestra el aviso "
        "“No se pudo conectar con la base de datos de Club de Vinos”, con el servidor o instancia probado, el motivo y los pasos a seguir. Elija **Reintentar** una vez creada la base o **Cancelar** para salir.")
    g.h3("Caso 3: el servicio de SQL Server está detenido")
    g.pasos([
        "Si el instalador no logra iniciarlo, abra `services.msc`, busque el servicio indicado en el mensaje (por ejemplo `SQL Server (MSSQLSERVER)`) y presione **Iniciar**; o bien, en una consola de administrador, ejecute `net start \"MSSQLSERVER\"` (para una instancia con nombre, `net start \"MSSQL$NOMBRE\"`).",
        "Presione **Aceptar** y luego **Siguiente** para reintentar. En la fase de instalación, elija **Reintentar** en el cuadro de aviso.",
        "Si el servicio está deshabilitado, cambie su tipo de inicio a Manual o Automático antes de reintentar.",
    ])

    g.h2("Errores durante la instalación")
    g.bullets([
        "Cada acción personalizada convierte sus errores en un mensaje legible en español: nada falla en silencio.",
        "Si el inicio de sesión en SQL Server falla, el mensaje indica que se probó con la cuenta con la que se instala la base (la cuenta del sistema; en un servidor remoto, la cuenta del equipo). Esa cuenta debe poder iniciar sesión y crear bases de datos. Si no es posible otorgárselo, instale solo la aplicación y cree la base con `EjecutarScript.cmd`, que usa **su propia cuenta de Windows**.",
        "Si la creación de la base o la ejecución del script fallan igualmente, Windows Installer **revierte toda la instalación** y la extensión de SQL muestra su propio mensaje con el error de SQL (en inglés). Para el detalle genere un registro: `msiexec /i ClubDeVinos.msi /l*v install.log`.",
        "LocalDB no se detecta automáticamente; puede escribirse (`(localdb)\\MSSQLLocalDB`) pero el instalador no la valida para la creación de la base.",
    ])

    g.h2("Instalar solo la aplicación y crear la base después")
    g.p("Con la casilla **Instalar solo la aplicación** el instalador copia los archivos y deja en `C:\\Program Files\\Club de Vinos\\BaseDeDatos` el `script.sql` y el programa `EjecutarScript.cmd`. Para crear la base:", mantener=True)
    g.pasos([
        "Abra una consola (símbolo del sistema) y ubíquese en `C:\\Program Files\\Club de Vinos\\BaseDeDatos`.",
        "Ejecute `EjecutarScript.cmd` (instancia predeterminada) o `EjecutarScript.cmd .\\SQLEXPRESS` (instancia indicada).",
        "El programa crea `BDCAPAS` si falta y ejecuta el script. Guarda la salida de `sqlcmd` en `%TEMP%\\EjecutarScript_BDCAPAS.log`.",
    ])
    g.tabla(["Código de salida", "Significado"], [
        ["0", "“Base de datos BDCAPAS lista en <servidor>.”"],
        ["1", "No se pudo completar: verifique que SQL Server esté iniciado, el nombre servidor\\instancia y sus permisos. El detalle queda en el registro."],
        ["3", "Terminó con advertencias: aparecieron errores inesperados (se ignoran los esperables al reejecutar: 2714, 1913, 1779 y 1781). Revise el registro."],
        ["9", "No se encontró `sqlcmd`. Instale las herramientas de línea de comandos de SQL Server."],
    ], [3.4, 13.2])

    g.h2("Instalación silenciosa")
    g.p("Una instalación silenciosa (`/qn`) no muestra el asistente: usa la instancia predeterminada (`.`) o el valor pasado por línea de comandos. "
        "Aun así se valida el servicio y la conexión antes de copiar archivos; si no hay SQL Server instalado, fallará salvo que se indique `SQLSKIPDB=1`. "
        "Un cuadro Reintentar/Cancelar se responde automáticamente en modo silencioso (la instalación falla y se revierte).", mantener=True)
    g.codigo("msiexec /i ClubDeVinos.msi /qn /l*v install.log\n"
             "msiexec /i ClubDeVinos.msi /qn SQLINSTANCE=. /l*v install.log\n"
             "msiexec /i ClubDeVinos.msi /qn SQLSKIPDB=1 /l*v install.log")

    g.h2("Desinstalación")
    g.p("Desinstale **Club de Vinos** desde Configuración de Windows > Aplicaciones. Se eliminan los archivos de la carpeta de instalación y los accesos directos. "
        "**No se eliminan** la base de datos `BDCAPAS`, los PDF de la carpeta Documentos ni los archivos de `%LocalAppData%\\ClubDeVinos`.")

    # ---------------- 3 ----------------
    g.h1("Primer inicio")
    g.pasos([
        "Verifique que el servicio de SQL Server esté en ejecución.",
        "Abra **Club de Vinos**. Antes de mostrar la ventana de acceso, la aplicación inicializa el control de integridad: si no existen los dígitos verificadores de usuarios (primera ejecución) los calcula automáticamente.",
        "Inicie sesión con el usuario `admin` y la contraseña `1234`.",
        "Cambie de inmediato la contraseña de `admin` en **Configuración > Cambiar Contraseña**. La contraseña inicial `1234` **no cumple** la política de contraseñas, por lo que la nueva debe tener 6 o más caracteres, una mayúscula y un número.",
        "Revise los usuarios de prueba (sección 4) y decida cuáles conservar.",
    ])
    g.nota("Si el servicio de SQL Server está detenido o la base no es accesible en la instancia predeterminada, la aplicación no podrá iniciar sesión: la ventana de acceso se muestra, "
           "pero al presionar **Ingresar** el error de conexión no se controla con un mensaje propio y .NET muestra su ventana estándar de excepción no controlada (sección 9).",
           tipo="Importante")

    # ---------------- 4 ----------------
    g.h1("Datos de demostración y cuentas de prueba")
    g.p("El script de base de datos incluye una sección **DATOS DE PRUEBA** que permite recorrer todos los procesos sin cargar nada a mano. "
        "Esa sección se ejecuta junto con el resto del script en cada instalación o actualización.")
    g.h2("Cuentas de prueba")
    g.p("Todas las cuentas de prueba tienen la contraseña **Club2026**, excepto `admin`, que conserva `1234`.", mantener=True)
    g.tabla(["Usuario", "Rol", "Uso previsto"], [
        ["admin", "Administrador", "Cuenta inicial del sistema (contraseña `1234`)."],
        ["admin2", "Administrador", "Segundo administrador, necesario por la regla RN-01 (quien emite una solicitud no puede resolverla)."],
        ["compras / compras2", "Encargado de Compras/Bodega", "Proponen altas y bajas, registran entradas y ajustes, analizan la rotación."],
        ["deposito", "Encargado de Depósito", "Picking y sustituciones."],
        ["curador", "Curador de Producto", "Armado de cajas mensuales."],
        ["atencion", "Atención al Socio", "Gestión de socios."],
        ["logistica", "Logística", "Despacho de cajas e historial."],
        ["usuario1", "Usuario", "Usuario común (solo cambia su contraseña)."],
        ["bloqueado1", "Usuario", "Cuenta bloqueada (3 intentos fallidos), para probar el desbloqueo."],
    ], [3.6, 4.4, 8.6])
    g.h2("Qué contienen los datos de prueba")
    g.bullets([
        "**Vinos** `DEMO-001` a `DEMO-017` en todos los estados: Activo, Pendiente (con solicitudes de alta por autorizar), Rechazado (con motivo) y Descontinuado; con bodegas, kardex, stock cero en algunos y por debajo del mínimo en otros.",
        "**Solicitudes**: una baja pendiente (`DEMO-008`, sin stock) y una baja rechazada (`DEMO-009`, que tiene stock y sirve para ver la regla RN-15).",
        "**Socios**: siete socios base (uno inactivo) y seis más que alimentan el análisis de rotación.",
        "**Cajas**: dos despachadas, tres armadas (una de ellas con una línea sin stock que exige sustitución y otra con la sustitución ya registrada), una cancelada y un socio sin caja para armarla desde cero. Los datos de rotación incluyen cajas despachadas repartidas en unos seis meses.",
        "**Bitácora** de ejemplo e **historial** de usuarios.",
    ])
    g.h2("Recorridos de prueba sugeridos")
    g.tabla(["Para probar…", "Ingrese como…", "Qué hacer"], [
        ["Autorizar un alta", "admin", "Autorizar `DEMO-010` o `DEMO-011` (emitidos por compras y compras2)."],
        ["Separación de funciones RN-01", "admin y admin2", "`DEMO-012` fue emitido por admin: solo admin2 puede resolverlo."],
        ["Corregir y reproponer", "compras / compras2", "Corregir `DEMO-013` o `DEMO-014` (rechazados) y reproponerlos."],
        ["Baja de un vino", "admin", "Autorizar la baja pendiente de `DEMO-008`; verificar que `DEMO-009` exige primero un ajuste de stock."],
        ["Alerta de stock mínimo", "compras", "Consultar la alerta: `DEMO-005` a `DEMO-008` están por debajo del mínimo."],
        ["Armar una caja", "curador", "Armar la caja del período actual para el socio sin caja."],
        ["Sustitución", "deposito", "Reemplazar la línea sin stock (`DEMO-008`) en la caja que la contiene."],
        ["Despacho y remito", "logistica", "Despachar una caja armada y revisar el PDF."],
        ["Historial de despachos", "logistica", "Regenerar el remito de una caja despachada."],
        ["Análisis de rotación", "compras o admin", "Analizar con 90 días; con los umbrales iniciales aparecen las cuatro clasificaciones y las cuatro recomendaciones."],
        ["Desbloqueo", "admin", "Desbloquear a `bloqueado1`."],
    ], [4.2, 3.6, 8.8], tam=9)
    g.nota("Cada ejecución del script **vuelve a crear** los usuarios de prueba que falten (aunque se hayan eliminado desde la aplicación) y vuelve a sembrar sus datos. "
           "Esto ocurre en cada instalación o actualización. En un entorno real, además de cambiar las contraseñas, tenga en cuenta que eliminar esas cuentas no es permanente mientras el script siga formando parte de la instalación. "
           "Lo que ya existe (vinos, socios y cajas) no se vuelve a tocar, por lo que reejecutar el script no pisa lo operado desde la aplicación.", tipo="Importante")

    # ---------------- 5 ----------------
    g.h1("Operación diaria")
    g.h2("Rutina recomendada")
    g.p("Las tareas siguientes son recomendaciones operativas; el sistema no las impone.", mantener=True)
    g.tabla(["Cuándo", "Tarea"], [
        ["Al iniciar la jornada", "Verificar que el servicio de SQL Server esté en ejecución y que el último respaldo se haya completado."],
        ["Durante la jornada", "Cada persona opera según su rol (Guía del Usuario). El administrador atiende autorizaciones pendientes y desbloqueos de usuarios."],
        ["Al finalizar", "Cerrar sesión (**Usuario > Cerrar Sesion**) y cerrar la ventana de inicio de sesión."],
        ["Periódicamente", "Revisar la bitácora; revisar la alerta de stock mínimo; copiar la carpeta `Documentos\\ClubDeVinos` si se desea conservar remitos e informes; depurar PDF antiguos si fuera necesario."],
        ["Mensualmente", "Seguir el ciclo de cajas: alta de socios, armado, picking, despacho; y ejecutar el análisis de rotación para planificar compras."],
    ], [3.8, 12.8])
    g.h2("Administrar usuarios")
    g.p("Menú **Administración > Gestión de usuarios** (permiso **Administrar usuarios**). La tabla muestra de a 15 usuarios por página, con un campo **Buscar** (se aplica con el botón o con Enter), "
        "botones de paginación y **Refrescar**.", mantener=True)
    g.h3("Crear un usuario")
    g.pasos([
        "Presione **Nuevo usuario**.",
        "Complete **Nombre**, **Apellido**, **Teléfono** y **Email** (datos personales opcionales), **Usuario**, **Contraseña** y **Rol**.",
        "Presione **Crear**. El sistema muestra “Usuario creado correctamente.” o “El nombre de usuario ya existe.”",
    ])
    g.p("El rol elegido se asigna también como perfil del usuario, que de inmediato cuenta con sus permisos. El alta queda registrada en el historial del usuario. "
        "**La ventana de alta solo exige que la contraseña no esté vacía**; para que cumpla la política (6 o más caracteres, una mayúscula, un número) indíquelo al usuario y pídale que la cambie en su primer ingreso. "
        "Mensajes de validación: “Ingresá un nombre de usuario.”, “Ingresá una contraseña.”, “Seleccioná un rol.”")
    g.h3("Editar datos personales, asignar roles, desbloquear y eliminar")
    g.tabla(["Acción", "Procedimiento", "Observaciones"], [
        ["Editar datos", "Seleccione el usuario y presione **Editar datos**. Modifique nombre, apellido, teléfono y email.", "Queda registrada como EDICION_DATOS en el historial."],
        ["Asignar roles", "Seleccione el usuario y presione **Modificar perfiles**. Marque o desmarque los roles del árbol y presione **Guardar** (“Asignaciones guardadas.”).", "Los permisos del usuario son la unión de los permisos de sus roles."],
        ["Desbloquear", "Seleccione el usuario bloqueado y presione **Desbloquear seleccionado**; confirme.", "Reinicia los intentos fallidos. Si no está bloqueado: “El usuario no está bloqueado.”"],
        ["Eliminar", "Seleccione el usuario y presione **Eliminar usuario**; confirme “¿Eliminar al usuario '…'? Esta acción no se puede deshacer.”", "No puede eliminarse a sí mismo (“No podés eliminar tu propio usuario.”). Su historial se conserva."],
        ["Ver historial", "Presione **Ver historial** (ver el apartado siguiente).", "Los cambios se registran con el administrador que los realizó."],
    ], [2.8, 8.6, 5.2], tam=9)
    g.nota("La aplicación no permite restablecer la contraseña de otra persona ni recuperar una contraseña olvidada, porque cada usuario solo cambia la suya. "
           "El procedimiento soportado es: eliminar el usuario y crearlo de nuevo con una contraseña inicial, que la persona cambiará en su primer ingreso. "
           "No se recomienda modificar la contraseña directamente en la base de datos: provoca una alerta de integridad (sección 7).", tipo="Importante")
    g.h3("Historial de usuario y restauración de versiones")
    g.p("**Ver historial** muestra, de a 20 registros, cada cambio con su fecha, tipo y responsable. Los tipos son: ALTA, BAJA, BLOQUEO, DESBLOQUEO, CAMBIO_CLAVE, ASIGNACION_PERFIL, EDICION_DATOS y ROLLBACK. "
        "Cada registro es una fotografía completa del usuario en ese momento (nunca incluye la contraseña). Para volver a una versión:", mantener=True)
    g.pasos([
        "Seleccione el registro y presione **Restaurar versión**. No es posible con registros de tipo BAJA ni sobre uno de tipo ROLLBACK (“No se puede restaurar una versión que ya es un rollback.”).",
        "Lea el aviso (“…la contraseña NO se restaurará (se mantendrá la actual)”) y confirme.",
        "El sistema restaura rol, bloqueo, intentos fallidos, perfiles y datos personales, registra un nuevo cambio ROLLBACK y recalcula la integridad: “Estado restaurado correctamente.”",
    ])
    g.h2("Administrar roles y permisos")
    g.p("Menú **Administración > Roles y Permisos** (permiso **Gestión de roles**). Muestra un árbol con los roles y sus permisos.", mantener=True)
    g.tabla(["Acción", "Procedimiento"], [
        ["Crear un rol", "Presione **Agregar Rol**, escriba el nombre en el cuadro “Nombre del rol:”. Si hay un rol seleccionado, el nuevo queda dentro de él; si no, en la raíz. El rol nace sin permisos."],
        ["Asignar padre", "Seleccione el rol, elija el **Rol padre** y presione **Asignar padre**. El combo excluye al propio rol y sus descendientes; los ancestros por encima del padre actual aparecen en gris y no se pueden elegir. Mensajes: “No se puede asignar ese padre: generaría una referencia circular.” y “No se puede asignar como padre a un ancestro del padre actual del rol.”"],
        ["Asignar permisos", "Seleccione el rol, marque los **Permisos del rol** y presione **Guardar permisos**. La lista guardada reemplaza a la anterior."],
        ["Eliminar un rol", "Seleccione el rol y presione **Eliminar**; confirme “¿Eliminar el rol '…' y todos sus sub-roles?”. Se eliminan también los roles descendientes."],
    ], [3.2, 13.4], tam=9)
    g.bullets([
        "**La jerarquía es solo organizativa: no hay herencia de permisos.** Los permisos de un rol son exactamente los que tiene marcados.",
        "El rol **Administrador** es de sistema y no puede eliminarse (“Este rol es del sistema y no puede eliminarse.”).",
        "No se puede eliminar un rol (ni uno de sus descendientes) que tenga usuarios asignados (“No se puede eliminar un rol que tiene usuarios asignados. Reasignálos primero.”).",
        "Los permisos del catálogo son fijos: no se crean ni se eliminan desde la aplicación (“Los permisos del catálogo no se pueden eliminar.”).",
    ])
    g.p("Catálogo de permisos del sistema:", mantener=True)
    g.tabla(["Permiso", "Habilita"], [
        ["Cambiar contraseña", "Pertenece al rol Usuario (la opción de menú está disponible para todos)."],
        ["Ver bitácora", "Menú Bitácora."],
        ["Administrar usuarios", "Administración > Gestión de usuarios. También habilita la restauración de integridad al iniciar."],
        ["Gestión de roles", "Administración > Roles y Permisos."],
        ["Gestión de idiomas", "Administración > Gestión de idiomas."],
        ["Gestionar catálogo de vinos", "Gestionar catálogo, entradas, ajustes y alerta de stock mínimo."],
        ["Autorizar alta de vinos", "Autorizar altas y descontinuaciones."],
        ["Analizar rotación de stock", "Catálogo > Analizar rotación de stock."],
        ["Gestionar socios", "Club de Socios > Gestionar Socios."],
        ["Armar cajas mensuales", "Club de Socios > Armar Caja Mensual."],
        ["Registrar picking y sustituciones", "Club de Socios > Picking y Sustituciones."],
        ["Despachar cajas", "Despachar Cajas e Historial de Despachos."],
    ], [5.4, 11.2], tam=9)
    g.h2("Administrar idiomas")
    g.p("Menú **Administración > Gestión de idiomas** (permiso **Gestión de idiomas**). El sistema trae Español (idioma predeterminado), Inglés y Portugués.", mantener=True)
    g.tabla(["Acción", "Procedimiento y reglas"], [
        ["Agregar", "**Agregar idioma** y escribir el nombre. Un idioma nuevo nace **deshabilitado**: no aparece en los selectores hasta habilitarlo."],
        ["Habilitar o deshabilitar", "Seleccione el idioma y presione **Habilitar/Deshabilitar**. Deshabilitar no borra las traducciones."],
        ["Renombrar", "**Renombrar idioma** y escribir el nuevo nombre."],
        ["Eliminar", "**Eliminar idioma**. Borra todas sus traducciones y no es reversible. No se puede eliminar el idioma predeterminado (“No se puede eliminar el idioma predeterminado del sistema.”) ni uno que algún usuario tenga como preferencia (“No se puede eliminar un idioma que está en uso por algún usuario.”)."],
        ["Traducciones", "Con el idioma seleccionado, edite la columna **Traducción** de la tabla de textos (las columnas Clave y Texto por defecto son de solo lectura) y presione **Guardar traducciones** (“Traducciones guardadas.”). Una traducción vacía no se guarda y el texto se muestra en el idioma por omisión. Si edita el idioma activo, la pantalla se actualiza al instante."],
    ], [3.6, 13.0], tam=9)
    g.nota("Las traducciones que usted edite para textos que ya trae el script **se sobrescriben** con la traducción original cada vez que se vuelve a ejecutar el script (instalación o actualización). "
           "Tras actualizar, revise las que haya personalizado.", tipo="Importante")

    g.h2("Usar la bitácora")
    g.p("Menú **Bitácora** (permiso **Ver bitácora**). Es un registro de solo lectura que muestra de a 30 entradas con **Id**, **Usuario**, **Acción** y **Fecha y hora**.", mantener=True)
    g.pasos([
        "Elija el **Usuario** y/o la **Acción** en las listas (se cargan con los valores existentes; “(Todos)” no filtra). El filtro es por **valor exacto**, no por parte del texto.",
        "Para filtrar por fecha, tilde la casilla de **Desde** y/o **Hasta** y elija el día. Los límites son inclusivos por día completo.",
        "Presione **Filtrar**. Use **Limpiar** para quitar todos los filtros y **Siguiente/Anterior** para paginar.",
    ])
    g.p("Acciones que registra la bitácora:", mantener=True)
    g.tabla(["Acción", "Se registra cuando…"], [
        ["LOGIN, LOGOUT", "Un usuario inicia o cierra sesión."],
        ["LOGIN_FALLIDO, USUARIO_BLOQUEADO", "Contraseña incorrecta de un usuario existente; la cuenta se bloquea al tercer intento."],
        ["CAMBIO_CONTRASENA", "Un usuario cambia su contraseña."],
        ["DESBLOQUEO_USUARIO:<usuario>", "Un administrador desbloquea una cuenta."],
        ["ALTA_VINO:<SKU>, REPROPUESTA_ALTA_VINO:<SKU>", "Se propone un vino o se repropone uno rechazado."],
        ["AUTORIZACION_VINO:<SKU>, RECHAZO_ALTA_VINO:<SKU>", "Se autoriza o rechaza un alta."],
        ["BAJA_SOLICITADA_VINO:<SKU>, AUTORIZACION_BAJA_VINO:<SKU>, RECHAZO_BAJA_VINO:<SKU>", "Circuito de descontinuación."],
        ["ENTRADA_STOCK:<SKU>:<cantidad>, AJUSTE_STOCK:<SKU>:+/-<cantidad>", "Movimientos de stock manuales."],
        ["ALTA_SOCIO:<apellido>, EDICION_SOCIO:<id>", "Alta y edición de socios."],
        ["ARMADO_CAJA:<período>, SUSTITUCION_CAJA:<id>, DESPACHO_CAJA:<id>", "Armado, sustitución y despacho de cajas."],
    ], [8.4, 8.2], tam=9)
    g.p("Las altas, bajas, bloqueos, cambios de clave, asignaciones de roles y ediciones de **usuarios** no se anotan en la bitácora sino en el **historial del usuario**. "
        "Como la acción incluye el SKU o el identificador, el filtro por acción exacta requiere elegir el valor completo de la lista.")

    # ---------------- 6 ----------------
    g.h1("Respaldos y restauración")
    g.p("Toda la información del sistema reside en la base `BDCAPAS`. La aplicación **no incluye** una función de respaldo ni de restauración; se utilizan las herramientas estándar de SQL Server "
        "(T-SQL con `sqlcmd`, SQL Server Management Studio o Azure Data Studio). Los comandos siguientes son los habituales de SQL Server y se deben adaptar a su entorno.")
    g.h2("Qué respaldar")
    g.bullets([
        "**La base de datos `BDCAPAS`** (imprescindible): usuarios, catálogo, stock, socios, cajas, historial, bitácora, idiomas y dígitos verificadores.",
        "La carpeta `Documentos\\ClubDeVinos` de cada usuario de Windows (remitos e informes en PDF). Los remitos pueden regenerarse desde el historial; los informes de rotación, en cambio, dependen del stock del día.",
        "Opcional: `%LocalAppData%\\ClubDeVinos\\configuracion_rotacion.xml` (umbrales del análisis; se recrea con valores iniciales si falta).",
    ])
    g.h2("Hacer un respaldo")
    g.pasos([
        "Elija una carpeta de destino que exista y sea escribible por el servicio de SQL Server (por ejemplo `D:\\Respaldos`).",
        "Ejecute el respaldo completo con suma de verificación:",
    ])
    g.codigo("sqlcmd -S . -E -Q \"BACKUP DATABASE [BDCAPAS] TO DISK = N'D:\\Respaldos\\BDCAPAS_2026-11-02.bak' WITH INIT, CHECKSUM, STATS = 10\"")
    g.pasos([
        "Verifique que el archivo sea legible:",
    ], inicio=3)
    g.codigo("sqlcmd -S . -E -Q \"RESTORE VERIFYONLY FROM DISK = N'D:\\Respaldos\\BDCAPAS_2026-11-02.bak' WITH CHECKSUM\"")
    g.p("Con SQL Server Express no hay Agente de SQL Server: para programar respaldos diarios puede usar el Programador de tareas de Windows que ejecute el comando anterior. "
        "Conserve copias en un equipo o disco distinto del que aloja la base.")
    g.h2("Restaurar un respaldo")
    g.pasos([
        "Pida a todos los usuarios que cierren la aplicación.",
        "Obtenga acceso exclusivo a la base y restaure:",
    ])
    g.codigo("sqlcmd -S . -E -Q \"ALTER DATABASE [BDCAPAS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE\"\n"
             "sqlcmd -S . -E -Q \"RESTORE DATABASE [BDCAPAS] FROM DISK = N'D:\\Respaldos\\BDCAPAS_2026-11-02.bak' WITH REPLACE, STATS = 10\"\n"
             "sqlcmd -S . -E -Q \"ALTER DATABASE [BDCAPAS] SET MULTI_USER\"")
    g.pasos([
        "Abra la aplicación e inicie sesión con una cuenta Administrador. Los dígitos verificadores forman parte de la base, de modo que una restauración consistente no genera alertas de integridad.",
        "Si el respaldo proviene de una versión anterior del sistema, ejecute `DAL\\script.sql` (sección 8) para actualizar procedimientos y datos de referencia. Restaure siempre respaldos de la misma versión de la aplicación o anteriores.",
    ], inicio=3)

    # ---------------- 7 ----------------
    g.h1("Control de integridad (dígitos verificadores)")
    g.h2("Qué protege")
    g.p("El sistema detecta modificaciones de los **datos de usuarios** hechas fuera de la aplicación (por ejemplo, con una herramienta de base de datos). Para ello usa dos tipos de dígitos:", mantener=True)
    g.bullets([
        "**Dígito verificador horizontal (DVH)**: uno por usuario, calculado con sus datos (identificador, nombre de usuario, hash de la contraseña, intentos fallidos, bloqueo, rol y roles asignados). Detecta cambios en un registro.",
        "**Dígito verificador vertical (DVV)**: uno por columna de la tabla de usuarios. Detecta altas, bajas o reordenamientos de filas.",
    ])
    g.nota("La protección alcanza a la tabla de usuarios (incluidos los roles asignados a cada usuario). La bitácora, los roles, los permisos y la relación entre roles y permisos **no** están protegidos por este mecanismo. "
           "Los datos personales (nombre, apellido, teléfono, email) tampoco entran en el cálculo.")
    g.h2("Cuándo se verifica")
    g.p("La verificación se ejecuta **una sola vez, al iniciar la aplicación**; no se repite durante la sesión. El inicio nunca se bloquea: siempre se muestra la ventana de acceso y el resultado "
        "se evalúa cuando alguien inicia sesión. Los dígitos se recalculan automáticamente después de cada alta, baja, desbloqueo, cambio de contraseña o de roles de un usuario, y tras un inicio de sesión correcto con la integridad en orden.")
    g.h2("Qué ocurre cuando se detecta un problema")
    g.tabla(["Quién inicia sesión", "Qué ve", "Qué sucede"], [
        ["Una persona con permiso **Administrar usuarios** cuyos propios datos no están afectados",
         "La ventana **Error de integridad** con el detalle de los problemas y tres botones.",
         "Puede resolverlo (ver abajo). Los errores se anotan en `%LocalAppData%\\ClubDeVinos\\integridad_error.log`."],
        ["Cualquier otro usuario (o un administrador cuyos datos están afectados)",
         "“El sistema no puede iniciarse debido a un problema interno. Comuníquese con el administrador del sistema.”",
         "Se cierra la sesión. No puede trabajar hasta que un administrador íntegro resuelva el problema."],
    ], [5.4, 5.6, 5.6], tam=9)
    g.h2("Qué hacer ante una advertencia de integridad")
    g.pasos([
        "**No elija una opción todavía.** Lea el detalle: las líneas “USUARIO ID=n: DVH inválido.” identifican a los usuarios cuyos datos cambiaron; “USUARIO columna X: DVV inválido. Posible inserción, eliminación o reordenamiento de filas.” indica altas, bajas o cambios de orden; “dígitos verificadores verticales ausentes” indica que faltan los dígitos de la tabla.",
        "Determine el origen. Consulte quién tuvo acceso a la base (herramientas, scripts, restauraciones) y revise el **historial** de los usuarios afectados.",
        "Si el cambio **no fue legítimo**, y los usuarios afectados tienen historial, elija **Restaurar desde historial**: se abre la ventana de historial de cada usuario afectado para que restaure la versión correcta (sección 5.2, “Historial de usuario y restauración de versiones”).",
        "Si el cambio **fue legítimo** (por ejemplo, una corrección autorizada hecha directamente en la base), elija **Recalcular y continuar**: el sistema acepta el estado actual como válido y recalcula todos los dígitos.",
        "Si no puede determinarlo, elija **Cancelar**: se cierra la sesión y se vuelve al acceso sin modificar nada.",
    ])
    g.tabla(["Opción", "Efecto", "Cuándo se habilita"], [
        ["Recalcular y continuar", "Acepta el estado actual de la base, recalcula todos los dígitos y abre el menú.", "Siempre."],
        ["Restaurar desde historial", "Abre el historial de cada usuario afectado; al cerrarlas, recalcula y abre el menú.", "Solo si hay al menos un usuario afectado. Si ninguno tiene historial se informa “El usuario '…' no tiene historial de cambios registrado…”."],
        ["Cancelar", "Cierra la sesión y vuelve al acceso.", "Siempre."],
    ], [4.2, 8.0, 4.4], tam=9)
    g.nota("**Restaurar desde historial** recalcula y continúa **al cerrar las ventanas de historial, aunque no haya restaurado ninguna versión**: en ese caso acepta el estado actual, igual que la opción anterior. "
           "Los usuarios creados antes de que existiera el historial solo pueden resolverse con **Recalcular y continuar**.", tipo="Importante")
    g.h3("Si no hay un administrador íntegro")
    g.p("Si todos los administradores están afectados, nadie puede resolver el problema desde la aplicación. Como último recurso, con acceso a la base de datos, puede forzar la reinicialización "
        "(**acepta el estado actual sin advertir**, por lo que debe hacerse solo tras investigar el origen):", mantener=True)
    g.codigo("sqlcmd -S . -d BDCAPAS -E -Q \"DELETE FROM DIGITO_VERIFICADOR_VERTICAL WHERE TABLA = 'USUARIO'\"")
    g.p("En el siguiente inicio la aplicación detecta que no está inicializada y recalcula todos los dígitos sin mostrar advertencia.")

    # ---------------- 8 ----------------
    g.h1("Archivos, registros y scripts")
    g.h2("Dónde se guardan los archivos")
    g.tabla(["Qué", "Dónde"], [
        ["Aplicación", "`C:\\Program Files\\Club de Vinos` (solo lectura para usuarios comunes)."],
        ["Script de base de datos y ayudante", "`C:\\Program Files\\Club de Vinos\\BaseDeDatos\\script.sql` y `EjecutarScript.cmd`; en el repositorio, `DAL\\script.sql`."],
        ["Base de datos", "Instancia de SQL Server elegida, base `BDCAPAS` (archivos `.mdf`/`.ldf` en la carpeta de datos de SQL Server)."],
        ["Remitos de despacho", "`Documentos\\ClubDeVinos\\Remitos\\Remito_Caja<N>.pdf` (carpeta Documentos del usuario de Windows)."],
        ["Informes de rotación", "`Documentos\\ClubDeVinos\\Informes\\Rotacion_AAAAMMDD_HHMMSS_mmm.pdf`."],
        ["Configuración del análisis de rotación", "`%LocalAppData%\\ClubDeVinos\\configuracion_rotacion.xml`."],
        ["Registro de problemas de integridad", "`%LocalAppData%\\ClubDeVinos\\integridad_error.log` (se agrega una entrada con fecha cada vez que se muestra la ventana de integridad)."],
        ["Registro de `EjecutarScript.cmd`", "`%TEMP%\\EjecutarScript_BDCAPAS.log` (y `.err` con los errores inesperados)."],
        ["Registro del instalador", "Solo si se genera con `msiexec /i ClubDeVinos.msi /l*v install.log`."],
    ], [4.8, 11.8], tam=9)
    g.p("`%LocalAppData%` equivale a `C:\\Users\\<usuario>\\AppData\\Local`. Estas carpetas pertenecen al usuario de Windows que ejecuta la aplicación: si hay varios usuarios en el mismo equipo, cada uno tiene las suyas.")
    g.h2("Ejecutar el script de base de datos manualmente")
    g.p("El script `DAL\\script.sql` crea y actualiza todos los objetos y los datos de referencia. Es de solo agregar y mayormente idempotente. La base `BDCAPAS` debe existir antes (el script comienza con `USE [BDCAPAS]`).", mantener=True)
    g.pasos([
        "Si la base no existe, créela:",
    ])
    g.codigo("sqlcmd -S . -E -Q \"IF DB_ID(N'BDCAPAS') IS NULL CREATE DATABASE [BDCAPAS]\"")
    g.pasos([
        "Desde la carpeta que contiene `DAL`, ejecute el script:",
    ], inicio=2)
    g.codigo("sqlcmd -S . -d BDCAPAS -E -I -f 65001 -i \"DAL\\script.sql\"")
    g.tabla(["Parámetro", "Por qué es necesario"], [
        ["-S .", "Servidor o instancia (`.` es la predeterminada; para otra, `.\\NOMBRE`)."],
        ["-d BDCAPAS", "Base de datos de trabajo."],
        ["-E", "Autenticación de Windows."],
        ["-I", "Activa QUOTED_IDENTIFIER. **Obligatorio**: sin él falla el DML sobre el índice filtrado (error 1934)."],
        ["-f 65001", "Lee el archivo como UTF-8. **Obligatorio**: el script es UTF-8 con BOM; sin esto se corrompen los textos con tildes (por ejemplo ‘Español’) y fallan las traducciones del idioma."],
        ["-i", "Archivo de entrada."],
    ], [3.0, 13.6], tam=9)
    g.p("Es normal que al reejecutarlo aparezcan algunos errores de objetos existentes (números 2714, 1913, 1779 y 1781; `sqlcmd` los rotula como “Msg” o “Mensaje” según el idioma): el resto del lote se sigue ejecutando. "
        "Para comprobar el resultado:", mantener=True)
    g.codigo("sqlcmd -S . -d BDCAPAS -E -Q \"SELECT USUARIO, ROL FROM USUARIO ORDER BY ID\"")
    g.h2("Actualizar a una nueva versión")
    g.pasos([
        "Haga un respaldo de la base (sección 6) y avise a los usuarios para que cierren la aplicación.",
        "Verifique que el nuevo MSI tenga una versión **mayor** que la instalada. El instalador impide instalar una versión anterior (“Ya hay instalada una versión más reciente de Club de Vinos.”).",
        "Ejecute el nuevo `ClubDeVinos.msi`. Windows Installer reemplaza la versión anterior (actualización mayor) y vuelve a ejecutar `script.sql` sobre `BDCAPAS`, que se conserva.",
        "Abra la aplicación con una cuenta Administrador y verifique el ingreso, el menú y las funciones principales. Si se agregaron usuarios de prueba, la aplicación recalcula la integridad sola.",
        "Revise lo que el script pudo reponer: asignaciones de permisos de los roles de fábrica, traducciones personalizadas y cuentas de demostración (sección 4).",
    ])
    g.p("**Para volver atrás:** desinstale la versión nueva, instale la anterior y restaure el respaldo de la base hecho en el paso 1 (sección 6). Una versión anterior no puede instalarse encima de una más nueva.")
    g.nota("Los permisos de los roles que trae el sistema (Administrador, Usuario y los de Compras, Depósito, Curador, Atención y Logística) se **reponen** si el script los encuentra faltantes; "
           "si quitó adrede un permiso a uno de esos roles, se volverá a asignar al actualizar.", tipo="Importante")

    # ---------------- 9 ----------------
    g.h1("Resolución de problemas")
    g.tabla(["Síntoma", "Causa probable", "Qué hacer"], [
        ["Al abrir la aplicación aparece “No se pudo conectar con la base de datos de Club de Vinos” (con **Reintentar** y **Cancelar**).",
         "El servicio de SQL Server está detenido o no está instalado, la base `BDCAPAS` no existe (por ejemplo tras instalar “solo la aplicación”) o está en otra instancia distinta de la indicada en el aviso.",
         "Inicie el servicio; cree la base con `EjecutarScript.cmd` (sección 2.7); verifique con `sqlcmd -S . -E -Q \"SELECT DB_ID('BDCAPAS')\"` (un resultado vacío o NULL indica que no existe). Si la base está en otra instancia, defina la variable de entorno `CLUBVINOS_SQL` (`servidor\\instancia`) y vuelva a abrir la aplicación (la variable no se aplica al **Reintentar** de una sesión ya abierta)."],
        ["“Usuario bloqueado por intentos fallidos…”", "Tres contraseñas incorrectas consecutivas.", "Un administrador lo desbloquea desde Gestión de usuarios (sección 5.2)."],
        ["“El sistema no puede iniciarse debido a un problema interno…”", "Alerta de integridad y el usuario no puede resolverla.", "Un administrador íntegro debe iniciar sesión y resolverla (sección 7)."],
        ["Textos con caracteres extraños o errores al guardar traducciones tras ejecutar el script a mano.", "Se ejecutó sin `-f 65001`.", "Reejecute el script con `-I -f 65001`."],
        ["Error 1934 o “SET options have incorrect settings” al ejecutar el script.", "Falta el parámetro `-I`.", "Reejecute con `-I`."],
        ["La instalación se revierte con un mensaje de SQL.", "Falló la creación de la base o el script (permisos de la cuenta del sistema, error de SQL).", "Revise el mensaje y un registro `/l*v`; alternativa: instale solo la aplicación y use `EjecutarScript.cmd`."],
        ["“No se pudo generar el remito de despacho…” / “No se pudo generar el informe en PDF…”", "La carpeta Documentos del usuario no es escribible o está redirigida a un lugar sin acceso.", "Verifique permisos y espacio de `Documentos\\ClubDeVinos`. El remito puede regenerarse desde el historial."],
        ["“El archivo de configuración de rotación estaba dañado o tenía valores inválidos…”", "El XML de configuración fue editado o dañado.", "Guarde la configuración desde la pantalla (lo reescribe) o borre `configuracion_rotacion.xml`; se recrea con valores iniciales."],
        ["Un usuario no ve opciones del menú.", "Su rol no tiene el permiso.", "Revise roles y permisos (sección 5.3)."],
        ["Un usuario eliminado reaparece tras actualizar.", "Era una cuenta de demostración que el script vuelve a crear.", "Cambie su contraseña o gestione esas cuentas (sección 4)."],
        ["El proceso `CAPAS.exe` sigue en el Administrador de tareas tras cerrar todas las ventanas.", "Por inspección del código, la ventana de acceso original queda oculta al iniciar sesión y la aplicación no invoca una salida explícita (no verificado en ejecución).", "Finalice el proceso desde el Administrador de tareas; avise a Mantenimiento."],
    ], [5.4, 5.2, 6.0], tam=9)
    return g.guardar()


# --------------------------------------------------------------------------------------
# GUIA DE MANTENIMIENTO
# --------------------------------------------------------------------------------------
def guia_mantenimiento():
    g = Guia("Guía de Mantenimiento",
             "Arquitectura, compilación, base de datos, instalador y procedimientos de extensión del sistema Club de Vinos",
             "Guia_de_Mantenimiento.docx")

    # ---------------- 1 ----------------
    g.h1("Introducción")
    g.p("Esta guía está dirigida a quienes mantienen y amplían el código de Club de Vinos. Explica cómo está construido el sistema, cómo compilarlo y empaquetarlo, "
        "las convenciones de la base de datos, cómo agregar formularios, permisos y traducciones, y qué limitaciones y deudas técnicas existen. "
        "Se apoya en el archivo `CLAUDE.md` del repositorio (convenciones de trabajo), en `Instalador\\README.md` y en el código fuente, que es la autoridad ante cualquier diferencia.")
    g.h2("Estructura del repositorio")
    g.tabla(["Carpeta o archivo", "Contenido"], [
        ["`TP_IS.sln`", "Solución con los cinco proyectos de la aplicación (BE, DAL, BLL, SeguridadYServicios y UI)."],
        ["`BE\\`", "Entidades y enumeraciones, sin lógica."],
        ["`DAL\\`", "Acceso a datos (clases DAL y `Acceso`), `script.sql` (esquema, procedimientos, semillas y datos de prueba) y scripts auxiliares."],
        ["`BLL\\`", "Lógica de negocio, validaciones y auditoría."],
        ["`SeguridadYServicios\\`", "Sesión, idioma, hash de contraseñas, validación de contraseñas y dígitos verificadores."],
        ["`CAPAS\\`", "Interfaz Windows Forms (proyecto `UI.csproj`, ensamblado `CAPAS.exe`), tema visual, generación de PDF y almacén de la configuración XML."],
        ["`Instalador\\`", "Proyecto WiX 7 del MSI y acciones personalizadas; **fuera** de la solución."],
        ["`DIAGRAMAS\\`", "Fuentes PlantUML, imágenes, documento de casos de uso y las herramientas que los generan."],
        ["`Documentacion\\`", "Guías de usuario, operación y mantenimiento (entregable D03) y su generador."],
        ["`CLAUDE.md`, `README.md`", "Guía técnica de trabajo y descripción general (ver la sección de deuda técnica sobre el estado de `README.md`)."],
    ], [4.6, 12.0], tam=9)

    # ---------------- 2 ----------------
    g.h1("Arquitectura")
    g.h2("Capas y dependencias")
    g.p("La aplicación sigue una arquitectura en capas. Cada capa solo conoce a las inferiores:", mantener=True)
    g.codigo("CAPAS (UI / WinForms)\n"
             "  ├── BLL\n"
             "  │     ├── DAL\n"
             "  │     │     └── BE\n"
             "  │     ├── SeguridadYServicios ── BE\n"
             "  │     └── BE\n"
             "  ├── SeguridadYServicios\n"
             "  └── BE")
    g.tabla(["Proyecto", "Responsabilidad", "Referencia a"], [
        ["BE", "Clases de datos (`USUARIO`, `Vino`, `CajaMensual`, `SolicitudAutorizacion`, `ConfiguracionRotacion`, `NodoPermiso`/`Rol`/`Permiso`, etc.). Sin lógica.", "—"],
        ["DAL", "Todo el acceso a SQL Server mediante procedimientos almacenados. Clase interna `Acceso` más una clase DAL por agregado.", "BE"],
        ["BLL", "Orquesta DAL y servicios; valida reglas de negocio; registra en bitácora e historial; recalcula integridad. Instancia los DAL directamente (`new DAL.UsuarioDAL()`).", "BE, DAL, SeguridadYServicios"],
        ["SeguridadYServicios", "Servicios transversales: `SessionManager`, `IdiomaManager`, `IObservadorIdioma`, `Hasher`, `ValidadorContrasena`, `CalculadorDVH`. No referencia BLL ni DAL: recibe los datos desde quien lo llama.", "BE"],
        ["CAPAS (UI.csproj)", "Formularios WinForms, tema, PDF y configuración XML. Todo formulario hereda de `FormBase`.", "BE, BLL, SeguridadYServicios"],
    ], [3.2, 9.4, 4.0], tam=9)
    g.h2("Pila tecnológica")
    g.tabla(["Componente", "Detalle"], [
        ["Lenguaje y plataforma", "C# sobre .NET Framework: la interfaz (`CAPAS`) apunta a 4.8 y las bibliotecas (BE, DAL, BLL, SeguridadYServicios) a 4.7.2."],
        ["Interfaz", "Windows Forms con ReaLTaiizor 3.8.1.8 (`MaterialForm`), tema oscuro con paleta “Bordó”. `Microsoft.VisualBasic` para los cuadros de entrada de texto (`InputBox`)."],
        ["Base de datos", "SQL Server (autenticación de Windows), base `BDCAPAS`, acceso con `System.Data.SqlClient` y solo procedimientos almacenados."],
        ["PDF", "PDFsharp-MigraDoc 1.50.5147."],
        ["Gráficos", "`System.Windows.Forms.DataVisualization.Charting.Chart` (referencia de framework incluida en .NET Framework 4.x; el instalador no agrega nada). Los arma `CAPAS\\RotacionGraficos`, usada por la pantalla de rotación y por el informe PDF."],
        ["Instalador", "WiX Toolset 7.0.0 (SDK, `WixToolset.UI.wixext` y `WixToolset.Sql.wixext`) y acciones personalizadas DTF (`WixToolset.Dtf.CustomAction` 7.0.0, net472)."],
        ["Documentación", "PlantUML (kroki.io), Python 3.12 con `python-docx`, `reportlab` y `Pillow`."],
    ], [4.4, 12.2], tam=9)
    g.h2("Acceso a datos")
    g.bullets([
        "**Solo procedimientos almacenados**: ninguna consulta SQL en línea. Los nombres siguen el patrón `ENTIDAD_ACCION` (por ejemplo `USUARIO_LOGIN`, `SOLICITUD_APROBAR_ALTA`, `ROTACION_ANALIZAR`).",
        "**Siempre con `SqlParameter`** (previene inyección SQL). `Acceso.CrearParametro` solo tiene sobrecargas para `string` e `int`: un `bool` se pasa como `valor ? 1 : 0` y un `int?` o un `decimal` se construyen a mano (`new SqlParameter(\"@x\", SqlDbType.Int) { Value = (object)v ?? DBNull.Value }`).",
        "`Acceso.Leer()` devuelve un `DataTable`; `Acceso.Escribir()` devuelve el número de filas. Los SP de escritura devuelven `SELECT @filas AS FILAS` y se leen con `Leer` (porque `ExecuteNonQuery` suma los conteos de todas las sentencias): **0 filas = operación rechazada**.",
        "Patrón de uso: `Abrir()` → comando → `Cerrar()` en `finally`. Las operaciones de varios pasos usan `IniciarTx()` / `ConfirmarTX()` / `DeshacerTX()` en el DAL, o una transacción dentro del propio SP.",
        "La cadena de conexión está fija en `DAL\\Acceso.cs`: `initial catalog=BDCAPAS; Data Source=.; Integrated Security=SSPI`.",
    ])
    g.h2("Seguridad")
    g.bullets([
        "**Sesión y permisos**: `SessionManager` (singleton) guarda el usuario y su lista de permisos, cargada al iniciar sesión con `USUARIO_PERMISOS_LISTAR` (usuario → roles asignados → permisos de esos roles, sin herencia). La interfaz consulta `SessionManager.getInstance().TienePermiso(\"nombre\")`. **No debe usarse `EsAdmin()` para controlar visibilidad**: compara un campo heredado y no está alineado con el sistema de permisos.",
        "**Contraseñas**: SHA-256 en hexadecimal minúscula (64 caracteres). Política: 6 o más caracteres, una mayúscula y un número (`ValidadorContrasena`); se aplica al cambiar la contraseña, no al crear usuarios.",
        "**Bloqueo**: al tercer intento fallido (`INTENTOS_FALLIDOS + 1 >= 3`) el usuario queda bloqueado.",
        "**Integridad**: dígitos verificadores horizontales (DVH) y verticales (DVV) sobre `USUARIO`. Fórmula DVH: Σ Unicode(carácter) × posición del atributo × posición del carácter; DVV: Σ por columna y fila en orden de `ID`. Atributos canónicos (el orden **no puede cambiar**): `ID, USUARIO, PASS, INTENTOS_FALLIDOS, BLOQUEADO, ROL, PERFILES`. `NOMBRE`, `APELLIDO`, teléfono y email quedan fuera. La verificación corre una vez al arrancar (`Program.InicializarIntegridad`) y nunca bloquea la apertura del login.",
        "**Recálculo**: debe ejecutarse tras toda mutación de `USUARIO` o `USUARIO_PERFIL` (`UsuarioBLL`, `UsuarioPerfilBLL.GuardarAsignaciones`, `UsuarioHistorialBLL.Rollback`, `frmRestaurarIntegridad`, `LogIn`). `LoginBLL.AutenticarUsuario` **nunca** recalcula: esa responsabilidad es de `LogIn.cs`, y solo cuando la integridad estaba en orden, para no “curar” datos corruptos en silencio.",
        "**Control de cambios**: `USUARIO_HISTORIAL` es solo de agregado (sin UPDATE ni DELETE); cada fila es una fotografía del usuario, sin contraseña. `BAJA` se registra antes de eliminar; el resto, después del cambio. Un `ROLLBACK` no puede restaurarse a su vez.",
    ])
    g.h2("Patrones de diseño")
    g.tabla(["Patrón", "Dónde", "Idea"], [
        ["Composite", "`NodoPermiso` (componente), `Rol` (rama, tabla `ROL`) y `Permiso` (hoja, tabla `PERMISO`)", "`PerfilBLL.ObtenerArbol()` arma el árbol en memoria. La jerarquía es organizativa; `CambiarPadre` impide ciclos y mover un rol bajo un ancestro de su padre actual."],
        ["Observer", "`IdiomaManager` (sujeto) y los formularios (`IObservadorIdioma.ActualizarIdioma()`)", "Al cambiar de idioma se notifica a todos los formularios registrados; cada uno busca la traducción por clave y usa el texto de diseño como alternativa."],
        ["Singleton", "`SessionManager`, `IdiomaManager`", "Instancia única con doble verificación (`double-checked locking`)."],
    ], [2.6, 6.2, 7.8], tam=9)
    g.h2("Modelo de datos")
    g.p("El esquema (21 tablas creadas por `script.sql`) se documenta en `DIAGRAMAS\\DER.puml` (renderizado en `DER.png`).", mantener=True)
    g.tabla(["Área", "Tablas"], [
        ["Seguridad", "`USUARIO`, `USUARIO_PERFIL`, `USUARIO_HISTORIAL`, `ROL`, `PERMISO`, `ROL_PERMISO`, `DIGITO_VERIFICADOR_VERTICAL`, `BITACORA`. `NODO_PERMISO` es un remanente sin uso."],
        ["Idiomas", "`IDIOMA`, `CONTROL_IDIOMA` (claves y texto por defecto), `TRADUCCION`."],
        ["Catálogo y stock", "`BODEGA`, `VINO`, `SOLICITUD_AUTORIZACION`, `MOVIMIENTO_STOCK` (kardex, solo de agregado)."],
        ["Club de socios", "`SOCIO`, `SOCIO_VARIETAL`, `CAJA_MENSUAL`, `CAJA_VINO`, `SUSTITUCION`."],
    ], [3.4, 13.2], tam=9)
    g.p("Puntos de diseño relevantes: el estado del vino (`VINO.ESTADO`) solo guarda el ciclo de vida (Pendiente, Activo, Rechazado, Descontinuado); las aprobaciones viven en `SOLICITUD_AUTORIZACION`. "
        "El stock se deriva del kardex (Entrada suma, el resto resta) y nunca se almacena. La composición efectiva de una caja se calcula con `CAJA_VINO` y `SUSTITUCION` (solo de agregado).")
    g.h2("Reglas de negocio y dónde se hacen cumplir")
    g.p("Las reglas se verifican en varias capas (defensa en profundidad). La numeración RN corresponde a los casos de uso CU-21 a CU-32 (`DIAGRAMAS\\CasosDeUso.docx`).", mantener=True)
    g.tabla(["Regla", "Contenido", "Dónde se aplica"], [
        ["RN-01", "Quien resuelve una solicitud de alta o baja no puede ser su emisor.", "UI (botones), `VinoBLL.ValidarSeparacionDeFunciones`, `WHERE EMISOR_ID <> @resolutor_id` en el SP y CHECK `CK_SOLAUT_RN01`."],
        ["RN-03", "Ninguna salida deja el stock por debajo de cero.", "BLL (mensaje claro) y SP atómico (inserta solo si alcanza)."],
        ["RN-04", "Baja lógica del vino; las cajas congelan nombre y precio.", "SP de baja (cambia `ESTADO`, nunca borra); snapshots en `CajaMensualBLL.Armar`."],
        ["RN-05 / RN-06", "Total de caja ≤ presupuesto; solo vinos Activos con stock.", "UI, `CajaMensualBLL` y SP de candidatos."],
        ["RN-08 / RN-09", "Una caja no cancelada por socio y período; armar no descuenta stock.", "Índice filtrado `UQ_CAJA_SOCIO_PERIODO`; el único movimiento de kardex del dominio lo genera `CAJA_DESPACHAR`."],
        ["RN-10", "Quien armó la caja no sustituye ni despacha.", "UI, BLL y SP (triple guardia)."],
        ["RN-11 / RN-12", "El remito es best-effort; regenerar sobrescribe `Remito_Caja<id>.pdf`.", "`frmDespacharCaja`, `frmHistorialDespachos`, `RemitoDespachoPdf`."],
        ["RN-15", "No se descontinúa un vino con stock.", "`VinoBLL.ValidarSinStock` (al solicitar y al autorizar) y SP."],
        ["RN-16", "Una solicitud pendiente por vino y tipo.", "BLL y índice único filtrado `UX_SOLAUT_PENDIENTE`."],
        ["RN-17", "El rechazo exige motivo de 1 a 200 caracteres.", "UI, `VinoBLL.ValidarMotivo`, SP y CHECK `CK_SOLAUT_MOTIVO`."],
    ], [2.2, 6.4, 8.0], tam=9)
    g.nota("Los SP de resolución bloquean primero `VINO` (`UPDLOCK, HOLDLOCK`) y recién después actualizan `SOLICITUD_AUTORIZACION`, para evitar interbloqueos entre aprobar/rechazar y proponer/reproponer. Respete este orden en cualquier SP nuevo.")

    # ---------------- 3 ----------------
    g.h1("Compilación y ejecución")
    g.h2("Requisitos del equipo de desarrollo")
    g.bullets([
        "Visual Studio 2022 (Community o superior) con la carga de trabajo de escritorio .NET y el paquete de desarrollo de .NET Framework 4.8.",
        "Acceso a NuGet para restaurar `ReaLTaiizor` 3.8.1.8 y `PDFsharp-MigraDoc` 1.50.5147.",
        "SQL Server local con la instancia predeterminada y `sqlcmd`.",
        "Para el MSI: .NET SDK y la herramienta WiX (sección 3.5). Para la documentación: Python 3.12 (sección 8).",
    ])
    g.h2("Compilar la solución")
    g.p("Desde la raíz del repositorio, con MSBuild de Visual Studio (la ruta puede variar según la edición instalada):", mantener=True)
    g.codigo("& \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe\" TP_IS.sln /p:Configuration=Debug /v:minimal\n"
             "& \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe\" BE\\BE.csproj /p:Configuration=Debug")
    g.bullets([
        "El proyecto de arranque es `CAPAS` (`UI.csproj`, ensamblado `CAPAS.exe`). Salida: `CAPAS\\bin\\Debug\\CAPAS.exe` o `CAPAS\\bin\\Release\\CAPAS.exe`.",
        "Si faltan paquetes NuGet, agregue `-restore` al comando de MSBuild (así lo hace `Instalador\\build.ps1`) o restaure desde Visual Studio.",
        "Se puede abrir `TP_IS.sln` directamente en Visual Studio (no requiere la extensión de WiX porque el instalador está fuera de la solución).",
    ])
    g.h2("Preparar la base y ejecutar")
    g.pasos([
        "Cree la base y ejecute el script (sección 4): `sqlcmd -S . -d BDCAPAS -E -I -f 65001 -i \"DAL\\script.sql\"`.",
        "Ejecute `CAPAS.exe` (F5 en Visual Studio) e ingrese con `admin` / `1234`, o con las cuentas de prueba (contraseña `Club2026`).",
    ])
    g.h2("Pruebas")
    g.p("**No hay pruebas automatizadas**: la verificación es manual sobre la aplicación en ejecución. Los datos de demostración del script permiten recorrer los casos de uso CU-21 a CU-32 "
        "(Guía de Operación, sección “Datos de demostración y cuentas de prueba”). Tras un cambio, recorra como mínimo: inicio de sesión y menú por rol; el circuito de alta, autorización y baja de vinos; "
        "armado, sustitución y despacho de una caja (con remito); el análisis de rotación con su exportación; y el cambio de idioma.")

    g.h2("Generar el instalador MSI")
    g.p("El instalador vive en `Instalador\\`, deliberadamente fuera de `TP_IS.sln`.", mantener=True)
    g.tabla(["Archivo", "Función"], [
        ["`Instalador.wixproj`", "Proyecto WiX SDK 7.0.0 (x64, cultura es-ES). Convierte `DAL\\script.sql` a UTF-16 LE en `Instalador\\obj\\script.utf16.sql` (la acción SQL de WiX no lee UTF-8)."],
        ["`Package.wxs`", "Definición del paquete: versión, actualización mayor, interfaz con el diálogo `SqlInstanceDlg`, carpetas, accesos directos, características `Main` y `Database`, creación de la base y ejecución del script."],
        ["`Package.es-es.wxl`", "Cadenas propias en español. Para agregar un idioma: otro `.wxl` con su `Culture` y alta en `<Cultures>`."],
        ["`EjecutarScript.cmd`", "Ayudante que se instala en `BaseDeDatos\\` para crear la base más tarde (modo “solo aplicación”)."],
        ["`CustomActions\\`", "Biblioteca net472 (DTF) con `DetectSqlInstances`, `ValidateSqlInstance`, `PrepareSqlService` y `EnsureSqlService` (detección de instancias, validación y arranque del servicio, prueba de conexión)."],
        ["`build.ps1`", "Compila la solución en Release con MSBuild (ubicado con `vswhere`) y luego el MSI."],
    ], [4.2, 12.4], tam=9)
    g.p("Requisitos únicos por equipo y comandos:", mantener=True)
    g.codigo("dotnet tool install --global wix --version 7.0.0\n"
             "wix eula accept wix7\n"
             ".\\Instalador\\build.ps1")
    g.p("El resultado es `Instalador\\bin\\x64\\Release\\es-ES\\ClubDeVinos.msi`. Notas de mantenimiento:", mantener=True)
    g.bullets([
        "**Versión**: cambie el atributo `Version` de `<Package>` en `Package.wxs` (actualmente %s). El `UpgradeCode` no debe cambiar nunca. Windows Installer solo compara los **tres primeros campos** de la versión: incremente al menos uno de ellos para que la actualización se reconozca." % VERSION_PRODUCTO,
        "La aplicación no escribe en su carpeta (Program Files es de solo lectura para usuarios comunes): remitos en `Documentos\\ClubDeVinos\\Remitos`, informes en `...\\Informes`, y configuración y registro de integridad en `%LocalAppData%\\ClubDeVinos`. Mantenga esa regla en cualquier función nueva.",
        "La base de datos nunca se elimina al desinstalar (`DropOnUninstall=\"no\"`) y el script se ejecuta de nuevo en cada instalación o actualización.",
        "El instalador incluye todo el contenido de `CAPAS\\bin\\Release` excepto `.pdb`, `app.publish` y `Remitos`. Si agrega un archivo de datos a la aplicación, verifique que llegue a esa carpeta.",
        "Las acciones personalizadas esperan hasta 20 s por el servicio de SQL Server y hacen 2 intentos de conexión de 5 s; el peor caso en el asistente ronda los 35 s.",
        "Pruebe siempre el MSI en una **máquina virtual**, no en el equipo de desarrollo (casos: sin SQL Server, servicio detenido, lista de instancias ilegible; ver `Instalador\\README.md`).",
    ])

    # ---------------- 4 ----------------
    g.h1("Base de datos")
    g.h2("El script `DAL\\script.sql`")
    g.p("Es el único archivo de esquema y datos. Se **agrega al final** (solo de agregado) y se organiza, de arriba hacia abajo, en: tablas; procedimientos almacenados por área; datos iniciales (idiomas, permisos, roles, administrador); "
        "controles y traducciones de cada pantalla; migraciones (separación de `ROL` y `PERMISO`, solicitudes de autorización); bloques por proceso (catálogo y stock, club de socios, rotación); y, al final, los **datos de prueba**.")
    g.h2("Convenciones")
    g.bullets([
        "**Idempotencia**: el script debe poder ejecutarse repetidamente. Tablas con `IF OBJECT_ID('dbo.X','U') IS NULL CREATE TABLE`; columnas con `IF COL_LENGTH('dbo.X','COL') IS NULL ALTER TABLE ... ADD`; procedimientos con `IF OBJECT_ID('dbo.SP','P') IS NOT NULL DROP PROCEDURE` seguido de `GO` y `CREATE PROCEDURE`; semillas con `IF NOT EXISTS (...) INSERT`.",
        "**Solo agregar**: no edite bloques históricos salvo para corregir errores; agregue bloques nuevos al final. Para quitar una columna con `DEFAULT`, elimine antes la restricción de forma dinámica (su nombre autogenerado varía por instancia) y verifique con `COL_LENGTH`.",
        "**`GO` reinicia el ámbito** de las variables: cada lote posterior debe volver a declarar las que necesite. `EXEC` no acepta expresiones como parámetros: asígnelas antes a una variable.",
        "**Codificación**: el archivo es **UTF-8 con BOM** (primeros bytes `EF BB BF`). Conserve el BOM al guardar y ejecútelo siempre con `-f 65001`; sin ese parámetro los literales con tildes (`'Español'`, `'Inglés'`) se corrompen y las variables de idioma quedan en `NULL` (“cannot insert NULL into IDIOMA_ID”).",
        "**`-I`**: ejecútelo con `QUOTED_IDENTIFIER ON`; sin él falla el DML sobre el índice filtrado (error 1934).",
        "**Procedimientos de escritura**: devuelven exactamente un `SELECT @filas AS FILAS`; las operaciones críticas son transaccionales (`SET XACT_ABORT ON`, `BEGIN TRY`) y verifican las reglas en el `WHERE`.",
        "Los errores 2714, 1913, 1779 y 1781 al reejecutar (objeto, índice, clave primaria o valor por defecto ya existentes) son esperables y no interrumpen el resto del lote.",
        "En el MSI el script se convierte a UTF-16 LE en tiempo de compilación; el original nunca se modifica.",
    ])
    g.h2("Scripts auxiliares")
    g.tabla(["Archivo", "Propósito"], [
        ["`DAL\\fix_duplicados_mojibake.sql`", "Limpieza puntual de permisos duplicados causados por una mala codificación de tildes en una ejecución previa. Se ejecuta una sola vez y no usa caracteres acentuados literales (los construye con `NCHAR()`)."],
        ["`DAL\\regenerar_idioma_espanol.sql`", "Regenera el idioma Español y sus traducciones tras una eliminación accidental. Uso excepcional."],
    ], [5.6, 11.0], tam=9)
    g.h2("Reiniciar la integridad en pruebas")
    g.p("Tras editar datos de usuarios directamente en la base, fuerce la reinicialización de los dígitos:", mantener=True)
    g.codigo("DELETE FROM DIGITO_VERIFICADOR_VERTICAL WHERE TABLA = 'USUARIO';")
    g.p("En el siguiente arranque `EstaInicializado()` devuelve falso y `RecalcularIntegridad()` recalcula todo. El propio script de datos de prueba hace esto cuando agrega usuarios nuevos (los inserta con `DVH = 0`).")
    g.h2("Datos de prueba")
    g.p("La sección final del script (“DATOS DE PRUEBA”) siembra bodegas, usuarios, vinos `DEMO-001..017`, solicitudes, kardex, socios y cajas. Cada bloque inserta solo lo que falta y no vuelve a tocar un vino, socio o caja existente; "
        "los datos de rotación (N03) están protegidos por el motivo `Compra a bodega - demo rotación`. Si agrega semillas, mantenga ese criterio de idempotencia.")

    # ---------------- 5 ----------------
    g.h1("Cómo extender el sistema")
    g.h2("Agregar un formulario con soporte de idioma")
    g.pasos([
        "Cree el formulario heredando de **`FormBase`** (no de `MaterialForm` ni `Form`) y agregue `using ReaLTaiizor.Manager;`.",
        "Implemente `SeguridadYServicios.IObservadorIdioma` y agregue los campos `Dictionary<string, Control> _controles` y `Dictionary<string, string> _defaults`.",
        "En el evento `Load`, en este orden: `GuardarDefaults(this.Controls)`; registrar el título (`_controles[this.Name] = this; _defaults[this.Name] = this.Text;`); `IdiomaManager.getInstance().Registrar(this)` y `ActualizarIdioma()`; `IdiomaUIHelper.AgregarSelector(this)`; y **al final** `MaterialSkinManager.Instance.AddFormToManage(this);` seguido de `AppTheme.AplicarTema(this);`.",
        "En `FormClosed`, `IdiomaManager.getInstance().Desregistrar(this)`.",
        "`ActualizarIdioma()` recorre `_controles` aplicando `Traducir(clave) ?? _defaults[clave]`; si hay `DataGridView`, llame también a `ActualizarEncabezados()`.",
        "En el `.Designer.cs`, ubique **todos los controles con `Location.Y` de 70 o más**: `MaterialForm` dibuja su barra de título (unos 64 px) dentro del área cliente. Aumente `ClientSize.Height` en la misma medida y conecte los eventos `Load` y `FormClosed`.",
        "Agregue el `.cs` y el `.Designer.cs` a `CAPAS\\UI.csproj`.",
        "Registre las claves y traducciones en `DAL\\script.sql` (sección siguiente) y vuelva a ejecutar el script.",
        "Si el formulario se abre desde el menú, agregue el elemento en `frmMenu` (Designer, `Click`) y su visibilidad por permiso en `ActualizarVisibilidadMenu`.",
    ])
    g.nota("Los `NumericUpDown` y `DateTimePicker` exponen su valor en `.Text`: no los registre en `_controles` (un cambio de idioma les restituiría el valor de diseño). Si dos formularios tienen un control con el mismo nombre (por ejemplo `lblTitulo`), "
           "elimine la clave genérica y registre una propia: `_controles.Remove(\"lblTitulo\"); _controles[\"lblTitulo_MiForm\"] = lblTitulo;` (y lo mismo en `_defaults`).")
    g.h2("Agregar o cambiar una traducción")
    g.p("Los textos traducibles se registran con claves y se traducen por idioma. Agregue al final de `DAL\\script.sql`:", mantener=True)
    g.codigo("EXEC CONTROL_REGISTRAR 'btnMiAccion', 'Texto por defecto'\n"
             "GO\n"
             "DECLARE @esp INT = (SELECT ID FROM IDIOMA WHERE NOMBRE = 'Español')\n"
             "DECLARE @cid INT\n"
             "SET @cid = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnMiAccion')\n"
             "EXEC TRADUCCION_GUARDAR @esp, @cid, 'Texto en español'\n"
             "GO")
    g.bullets([
        "`CONTROL_REGISTRAR(clave, texto_default)` es idempotente (no duplica). `TRADUCCION_GUARDAR(idioma_id, control_id, texto)` es un upsert: **sobrescribe** una traducción existente, lo que significa que las ediciones hechas desde la pantalla de idiomas se pierden al reejecutar el script.",
        "Repita el bloque de traducción para Inglés y Portugués si el texto debe estar en ellos. Un idioma nuevo puede crearse desde la aplicación (nace deshabilitado), pero sus traducciones no quedan en el script salvo que las agregue.",
        "Encabezados de `DataGridView`: claves `colhdr_*`, traducidas en `ActualizarEncabezados()` (no están en `Controls`). Texto dinámico (nodos de árbol, mensajes armados en tiempo de ejecución): `IdiomaManager.getInstance().Traducir(\"clave\")` directamente.",
        "Los mensajes de `MsgBox` y de las excepciones de BLL son texto fijo en español y **no** pasan por el sistema de traducción.",
    ])
    g.h2("Agregar un permiso")
    g.p("El catálogo de permisos es fijo desde la interfaz: se amplía por script.", mantener=True)
    g.pasos([
        "Agregue la semilla del permiso y su asignación a los roles que corresponda (siempre idempotente):",
    ])
    g.codigo("IF NOT EXISTS (SELECT 1 FROM PERMISO WHERE NOMBRE = 'Mi permiso')\n"
             "    INSERT INTO PERMISO (NOMBRE) VALUES ('Mi permiso')\n"
             "GO\n"
             "DECLARE @rol  INT = (SELECT TOP 1 ID FROM ROL WHERE NOMBRE = 'Administrador' AND PADRE_ID IS NULL)\n"
             "DECLARE @perm INT = (SELECT ID FROM PERMISO WHERE NOMBRE = 'Mi permiso')\n"
             "IF NOT EXISTS (SELECT 1 FROM ROL_PERMISO WHERE ROL_ID = @rol AND PERMISO_ID = @perm)\n"
             "    INSERT INTO ROL_PERMISO (ROL_ID, PERMISO_ID) VALUES (@rol, @perm)\n"
             "GO")
    g.pasos([
        "En la interfaz, proteja la función con `SessionManager.getInstance().TienePermiso(\"Mi permiso\")` y, si es una opción de menú, agregue su visibilidad en `frmMenu.ActualizarVisibilidadMenu()` (incluida la del menú padre).",
        "Si el permiso introduce un rol nuevo, créelo con `INSERT INTO ROL (NOMBRE, PADRE_ID, PROTEGIDO) VALUES (..., NULL, 0)` en un bloque idempotente, como los de Club de Socios.",
        "Actualice los documentos (catálogo de permisos del caso de uso de roles) y las guías.",
    ], inicio=2)
    g.h2("Agregar una operación de negocio")
    g.pasos([
        "**SP**: cree `ENTIDAD_ACCION` en `script.sql` con `IF OBJECT_ID ... DROP` y `CREATE PROCEDURE`; para escrituras devuelva `SELECT @filas AS FILAS` y exija las reglas en el `WHERE`.",
        "**DAL**: agregue el método en la clase de la entidad, con `SqlParameter` y `Abrir()`/`Cerrar()` en `finally`.",
        "**BLL**: valide reglas y lance `InvalidOperationException` con mensajes claros; traduzca `SqlException` 2601/2627 (clave duplicada) y 547 (clave foránea) a mensajes de negocio; registre la acción con `BitacoraBLL.RegistrarAccion(usuario, \"ACCION:dato\")`; si muta `USUARIO`, recalcule la integridad.",
        "**UI**: capture `InvalidOperationException` y muéstrela con `MsgBox.Show(ex.Message, ...)`; deshabilite los botones según las reglas (primera capa de la defensa en profundidad).",
        "Mantenga alineados los nombres de métodos entre el código, los diagramas de clases y los de secuencia (`DIAGRAMAS\\`).",
    ])
    g.h2("Extender la integridad a otra entidad")
    g.pasos([
        "Agregue una columna `DVH INT` a la tabla.",
        "Agregue las filas correspondientes en `DIGITO_VERIFICADOR_VERTICAL`.",
        "En `IntegridadBLL`, defina el arreglo canónico de atributos (**el orden no podrá cambiar** una vez almacenados datos) y un método `ExtraerAtributos*`.",
        "Llame a los ayudantes genéricos `VerificarTabla` y `RecalcularTabla` siguiendo el patrón de `USUARIO`, e incluya la nueva entidad en `RecalcularIntegridad()` y `VerificarIntegridad()`.",
        "Recalcule tras cada mutación de la entidad.",
    ])

    # ---------------- 6 ----------------
    g.h1("Serialización de la configuración (XML)")
    g.p("El análisis de rotación lee todos sus umbrales de un archivo XML serializado con `System.Xml.Serialization.XmlSerializer`. Es la configuración real del proceso, no un adorno: el análisis usa lo que se releyó del archivo.")
    g.tabla(["Elemento", "Detalle"], [
        ["Clase", "`BE.ConfiguracionRotacion`: pública, con constructor sin parámetros y propiedades públicas de lectura y escritura (requisitos de `XmlSerializer`); `[Serializable]` y `[XmlRoot(\"ConfiguracionRotacion\")]`. Cada propiedad trae su valor inicial."],
        ["Almacén", "`CAPAS\\ConfiguracionRotacionStore` (interna): `Cargar(out bool archivoInvalido)` y `Guardar(config)`."],
        ["Archivo", "`%LocalAppData%\\ClubDeVinos\\configuracion_rotacion.xml` (no la carpeta de la aplicación, que no es escribible)."],
        ["Carga", "Si el archivo falta, lo crea con los valores por defecto. Si está dañado o es incoherente (`RotacionBLL.Validar` lanza), devuelve los valores por defecto sin fallar y marca `archivoInvalido = true`; no pisa el archivo hasta que el usuario guarde."],
        ["Guardado", "Atómico: serializa a un archivo temporal (`.tmp` con GUID, UTF-8 sin BOM, con sangría) y lo reemplaza con `File.Replace` (o `File.Move` la primera vez). Si falla, borra el temporal y propaga `IOException` o `UnauthorizedAccessException`."],
        ["Uso", "**El XML manda**: **Analizar** valida lo que hay en pantalla, lo guarda y analiza con la configuración releída. Si no puede guardar o leer, no analiza."],
    ], [3.0, 13.6], tam=9)
    g.p("Estructura del archivo generado (valores por defecto):", mantener=True)
    g.codigo("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
             "<ConfiguracionRotacion xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n"
             "  <DiasAnalisis>90</DiasAnalisis>\n"
             "  <UmbralAltaRotacion>4</UmbralAltaRotacion>\n"
             "  <UmbralBajaRotacion>1.5</UmbralBajaRotacion>\n"
             "  <MesesCoberturaUrgente>1</MesesCoberturaUrgente>\n"
             "  <MesesCoberturaReposicion>2</MesesCoberturaReposicion>\n"
             "  <MesesCoberturaSobrestock>6</MesesCoberturaSobrestock>\n"
             "  <MesesCoberturaObjetivo>3</MesesCoberturaObjetivo>\n"
             "</ConfiguracionRotacion>")
    g.p("Para agregar un parámetro: (1) agregue la propiedad a `ConfiguracionRotacion` con su valor inicial (un archivo antiguo sin el elemento conserva ese valor); (2) agregue su validación en `RotacionBLL.Validar`; "
        "(3) agregue el control en `frmAnalisisRotacion`, y en `LeerConfiguracionDePantalla` y `AplicarConfiguracionEnPantalla`; (4) úselo en `RotacionBLL.Evaluar` y, si corresponde, en el encabezado de `RotacionReposicionPdf`; "
        "(5) registre las claves de idioma y actualice las guías.")

    # ---------------- 7 ----------------
    g.h1("Generación de PDF")
    g.p("Los documentos PDF se generan con **PDFsharp-MigraDoc 1.50.5147** (`MigraDoc.DocumentObjectModel`, `MigraDoc.Rendering`, `PdfSharp`). Se construye un `Document` con secciones, párrafos y tablas y se renderiza con "
        "`PdfDocumentRenderer(true)` (Unicode) y `renderer.PdfDocument.Save(ruta)`.")
    g.tabla(["Clase (en `CAPAS`)", "Documento", "Formato y destino"], [
        ["`RemitoDespachoPdf`", "Remito de despacho de una caja", "A4 vertical. `Documentos\\ClubDeVinos\\Remitos\\Remito_Caja{id}.pdf` (se sobrescribe al regenerar)."],
        ["`RotacionReposicionPdf`", "Informe de rotación y reposición", "A4 apaisado. `Documentos\\ClubDeVinos\\Informes\\Rotacion_{yyyyMMdd_HHmmss_fff}.pdf` (nombre único). Incluye tres gráficos de `RotacionGraficos`, incrustados como PNG en memoria (`base64:`), sin archivos temporales."],
    ], [4.4, 5.0, 7.2], tam=9)
    g.bullets([
        "Son clases estáticas que no implementan `IObservadorIdioma`. `RemitoDespachoPdf` es **independiente del idioma de sesión** (siempre español, es-AR). `RotacionReposicionPdf.Generar` recibe un `Func<string,string> traducir` opcional (el mismo traductor del formulario, claves `rot_pdf_*` más las `colhdr_*`/`rot_rec_*`/`rot_clas_*` existentes): sin traductor o sin traducción de una clave, el texto sale en español; fechas y números siguen en es-AR porque `IDIOMA` no tiene cultura. `TRADUCCION.TEXTO` admite 200 caracteres y trunca en silencio: los textos largos se dividen en varias claves.",
        "La carpeta se obtiene de `Environment.SpecialFolder.MyDocuments`; se crea con `Directory.CreateDirectory`. **No escriba en la carpeta de la aplicación.**",
        "La generación es best-effort: `Generar` propaga las excepciones de E/S y el formulario que la invoca decide cómo informarlas. El despacho de una caja nunca se revierte por un fallo del remito (RN-11).",
        "`Abrir(ruta)` usa `Process.Start` y traga su propia excepción: el archivo ya existe aunque no haya lector de PDF.",
        "El total del remito usa `Cantidad × PrecioEfectivo` (con sustituciones), no `CajaMensualBLL.CalcularTotal`, que suma precios congelados originales.",
        "Para un informe nuevo: cree una clase estática con `Generar(...)` y `Abrir(...)`, reutilice la carpeta `ClubDeVinos` bajo Documentos, y capture las excepciones en el formulario con un mensaje claro.",
        "Los ensamblados de MigraDoc y PDFsharp (y sus carpetas de recursos satélite `de`) deben llegar a la carpeta de salida: el instalador los toma de `CAPAS\\bin\\Release`.",
    ])

    # ---------------- 8 ----------------
    g.h1("Herramientas de documentación")
    g.p("Los diagramas, el documento de casos de uso y las guías se generan con scripts. **El código manda**: si un diagrama no coincide con el código, se corrige el diagrama o su fuente, nunca al revés. "
        "Ejecute los scripts de Python con `python -I` para que no cargue módulos de la carpeta actual.", mantener=True)
    g.tabla(["Herramienta", "Qué hace", "Requisitos"], [
        ["`DIAGRAMAS\\generar_png.py`", "Renderiza un `.puml` a PNG (y SVG opcional) con kroki.io. `python generar_png.py <archivo.puml> <archivo.png>`.", "Python 3.12, internet."],
        ["`DIAGRAMAS\\generar_pngs_lote.py`", "Regenera en lote los PNG de los diagramas de secuencia, con reintentos. Acepta patrones (`CU04 CU11`).", "Internet."],
        ["`DIAGRAMAS\\herramientas\\secuencia\\`", "`seq_src\\*.puml` (fuentes con macros) → `seq_build.py` genera los `.puml` finales en `DIAGRAMAS\\` → `render_seq.py` los renderiza. **Nunca edite a mano los `DiagramaSecuencia_CU*.puml`**: se pisan.", "Python, Pillow, internet, Microsoft Edge (conversión SVG a PNG)."],
        ["`DIAGRAMAS\\herramientas\\clases\\`", "`gen_master.py` genera el diagrama de clases maestro desde el código; `compare.py` y `arrowchk.py` lo contrastan con el código; `render_clases.py` lo renderiza (el PNG de PlantUML se recorta a 4096 px, por eso se usa SVG y Edge).", "Igual que arriba."],
        ["`DIAGRAMAS\\generar_casos_uso_docx.py`", "Regenera `CasosDeUso.docx` (32 casos de uso, plantilla Cockburn) desde el diccionario `CUS` del script.", "`python-docx`."],
        ["`DIAGRAMAS\\generar_pdf.py`", "Genera una documentación técnica en PDF.", "`reportlab`."],
        ["`Documentacion\\generar_guias.py`", "Genera las tres guías Word de este entregable (`python -I Documentacion\\generar_guias.py`). Si Word está instalado, actualiza el índice, cuenta las páginas y borra el PDF temporal; con `--sin-word` omite ese paso.", "`python-docx`; Word (opcional)."],
    ], [5.4, 7.8, 3.4], tam=9)
    g.p("Convenciones de modelado: solo dos actores (**Usuario** y **Administrador**; los permisos son precondiciones, no actores); diagramas de secuencia al nivel UI / BLL / DAL / DB; diagramas de actividad sin carril “Sistema”, "
        "con documentos como nodos de objeto de flechas punteadas. Los detalles están en `CLAUDE.md`.")

    # ---------------- 9 ----------------
    g.h1("Limitaciones conocidas y deuda técnica")
    g.tabla(["Tema", "Descripción", "Impacto y sugerencia"], [
        ["Cadena de conexión fija", "`DAL\\Acceso.cs` apunta a `Data Source=.` (instancia predeterminada); hay una alternativa comentada para otra instancia.",
         "Una instancia con nombre o un servidor remoto no funcionan aunque el instalador cree allí la base. Leer la cadena de `App.config` o de una variable de entorno."],
        ["Sin pruebas automatizadas", "La verificación es manual.", "Riesgo de regresiones. Empezar por pruebas de BLL con una base de prueba."],
        ["Integridad acotada", "Los dígitos verificadores cubren solo `USUARIO` (más sus roles asignados) y se verifican solo al arrancar. `BITACORA`, `ROL`, `PERMISO` y `ROL_PERMISO` no están protegidos. Una mutación posterior acepta como válidos datos ya corrompidos.",
         "Ampliar la cobertura (sección 5.5) o verificar periódicamente."],
        ["Cuentas de demostración", "El script reinserta usuarios de prueba (contraseña `Club2026`) y el administrador inicial usa `1234`, que incumple la política.", "Separar los datos de prueba del script de instalación o parametrizarlos; forzar cambio de contraseña inicial."],
        ["Política de contraseñas parcial", "`frmNuevoUsuario` y `UsuarioBLL.Crear` solo exigen contraseña no vacía.", "Aplicar `ValidadorContrasena` al crear."],
        ["Sin restablecimiento de contraseña", "No existe una función para que un administrador restablezca la contraseña de otro usuario.", "Agregar un procedimiento controlado con registro en historial."],
        ["Cajas sin cancelación en la UI", "`CajaMensualBLL.Cancelar` existe, pero no hay formulario; las cajas canceladas solo vienen de los datos de prueba.", "Agregar la pantalla (con motivo y separación de funciones)."],
        ["Manejo de errores", "No hay `try/catch` global ni `Application.ThreadException`; un error de conexión o inesperado muestra el diálogo estándar de .NET.", "Agregar un manejador global y mensajes claros."],
        ["Cierre de la aplicación", "El formulario de login original queda oculto tras iniciar sesión y no se encontró una llamada a `Application.Exit`; por inspección del código, el proceso podría seguir activo tras cerrar el menú (no verificado en ejecución).", "Verificar el comportamiento y cerrar la aplicación al cerrar el menú."],
        ["Mensajes sin traducción", "Los textos de `MsgBox` y de las excepciones están en español fijo.", "Pasar los mensajes por `IdiomaManager`."],
        ["Remanentes legados", "`NODO_PERMISO` (sin uso desde la migración a `ROL`/`PERMISO`), `PERSONA` (puede existir en bases creadas antes), `UI_TemporaryKey.pfx` en `CAPAS\\` (no referenciado por `UI.csproj`).", "Eliminar tras verificar."],
        ["Versiones de .NET", "Las bibliotecas apuntan a 4.7.2 y la interfaz a 4.8; `App.config` declara `sku=\".NETFramework,Version=v4.7.2\"`.", "Unificar (compatible en la práctica porque 4.8 es una actualización de 4.7.2)."],
        ["Documentación desactualizada", "`README.md` aún describe `NODO_PERMISO`, cinco permisos, rutas de diagramas en la raíz, `sqlcmd` sin `-I`/`-f 65001` y la firma antigua de `TRADUCCION_GUARDAR` (la real recibe `idioma_id`, `control_id`, `texto`). `CLAUDE.md` describe en “Permissions check flow” una herencia de roles ya eliminada.",
         "Actualizar ambos archivos; mientras tanto, prevalece el código y `script.sql`."],
        ["Despliegue de un solo equipo", "Aplicación y base locales; no hay modo cliente-servidor.", "Parametrizar la conexión y revisar permisos de SQL Server si se necesita acceso en red."],
    ], [3.2, 7.4, 6.0], tam=9)

    # ---------------- 10 ----------------
    g.h1("Resolución de problemas")
    g.tabla(["Síntoma", "Causa probable", "Solución"], [
        ["Faltan paquetes o no compila tras clonar.", "No se restauraron los paquetes NuGet.", "Agregar `-restore` a MSBuild o restaurar desde Visual Studio."],
        ["Los controles de un formulario nuevo quedan tapados.", "`Location.Y` menor que unos 70 px (barra de título de `MaterialForm`).", "Desplazar el contenido 70 px o más y ampliar `ClientSize.Height`."],
        ["Un formulario se ve gris y sin tema.", "No hereda de `FormBase` o falta `AddFormToManage`/`AplicarTema` en `Load`.", "Heredar de `FormBase` y llamar a ambos al final de `Load`."],
        ["Un texto no se traduce.", "Clave sin `CONTROL_REGISTRAR`, sin traducción en el idioma, colisión de nombres entre formularios, formulario sin `Registrar`, o encabezado de grilla sin `ActualizarEncabezados`.", "Revisar cada punto; ver las secciones 5.1 y 5.2."],
        ["`cannot insert NULL into IDIOMA_ID` o tildes corruptas.", "Script ejecutado sin `-f 65001`.", "Reejecutar con `-I -f 65001`."],
        ["Error 1934 al ejecutar el script.", "Falta `-I`.", "Reejecutar con `-I`."],
        ["Aparece la ventana de integridad tras tocar la base a mano.", "DVH/DVV desactualizados.", "Elegir “Recalcular y continuar” o borrar los DVV de `USUARIO` (sección 4)."],
        ["La aplicación no conecta.", "Servicio detenido, base ausente o instancia distinta de `.`.", "Verificar con `sqlcmd -S . -E -Q \"SELECT DB_ID('BDCAPAS')\"`; ver la limitación de conexión fija."],
        ["`wix` falla por la licencia.", "No se aceptó la EULA.", "`wix eula accept wix7`."],
        ["El MSI no reemplaza a la versión anterior.", "La versión nueva no es mayor en alguno de los tres primeros campos.", "Incrementar `Version` en `Package.wxs`."],
        ["Textos con tildes corruptos tras instalar con el MSI.", "`script.sql` guardado sin codificación UTF-8 (la conversión a UTF-16 LE parte de UTF-8) o un `script.utf16.sql` obsoleto.", "Verificar que `script.sql` sea UTF-8 con BOM, borrar `Instalador\\obj` y recompilar."],
        ["Un elemento de menú se muestra pero no hace nada.", "Falta el manejador `Click` o el cableado en el `Designer`.", "Conectar el evento y verificar `ActualizarVisibilidadMenu`."],
        ["El remito o el informe no se generan.", "Documentos no escribible (permisos, redirección).", "Verificar la carpeta; el error se informa por `MsgBox`."],
    ], [5.0, 5.8, 5.8], tam=9)
    return g.guardar()

def main():
    sin_word = "--sin-word" in sys.argv
    archivos = []
    for fn in (guia_usuario, guia_operacion, guia_mantenimiento):
        ruta = fn()
        archivos.append(ruta)
        print("generado:", ruta)
    if not sin_word:
        actualizar_con_word(archivos)


def actualizar_con_word(archivos):
    """Actualiza el indice, cuenta paginas y exporta un PDF temporal (se borra) con Word por COM."""
    ps1 = r'''
param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Archivos)
$ErrorActionPreference = 'Stop'
$w = New-Object -ComObject Word.Application
$w.Visible = $false
$w.DisplayAlerts = 0
try {
  foreach ($f in $Archivos) {
    $d = $w.Documents.Open($f, $false, $false)
    $d.Repaginate()
    foreach ($t in $d.TablesOfContents) { $t.Update() }
    $d.Fields.Update() | Out-Null
    $d.Repaginate()
    foreach ($t in $d.TablesOfContents) { $t.Update() }
    $paginas = $d.ComputeStatistics(2)
    $d.Save()
    $pdf = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), ([guid]::NewGuid().ToString() + '.pdf'))
    $d.ExportAsFixedFormat($pdf, 17)
    $d.Close(0)
    Remove-Item $pdf -Force
    Write-Output ('PAGINAS|' + [System.IO.Path]::GetFileName($f) + '|' + $paginas)
  }
} finally { $w.Quit() }
'''
    try:
        with tempfile.TemporaryDirectory() as tmp:
            script = Path(tmp) / "actualizar_word.ps1"
            script.write_text(ps1, encoding="utf-8-sig")
            res = subprocess.run(
                ["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(script)] + [str(a) for a in archivos],
                capture_output=True, text=True, timeout=600)
        print(res.stdout.strip())
        if res.returncode != 0:
            print("Word no pudo actualizar los documentos:", res.stderr.strip())
    except Exception as exc:  # Word no instalado u otro problema de COM
        print("No se pudo usar Word (el indice quedara para actualizar a mano):", exc)


if __name__ == "__main__":
    main()
