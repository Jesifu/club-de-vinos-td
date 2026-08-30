"""
Genera la documentacion tecnica del proyecto en PDF usando reportlab.
"""

from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    HRFlowable, PageBreak, KeepTogether
)
from reportlab.platypus.tableofcontents import TableOfContents
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_JUSTIFY
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import datetime

# ─── Colores ────────────────────────────────────────────────────────────────
AZUL_OSCURO  = colors.HexColor("#1a3a5c")
AZUL_MEDIO   = colors.HexColor("#2e6da4")
AZUL_CLARO   = colors.HexColor("#dce9f5")
GRIS_LINEA   = colors.HexColor("#c0c8d0")
GRIS_CODIGO  = colors.HexColor("#f4f4f4")
GRIS_TEXTO   = colors.HexColor("#333333")
BLANCO       = colors.white

PAGE_W, PAGE_H = A4

# ─── Estilos ────────────────────────────────────────────────────────────────
base = getSampleStyleSheet()

def make_style(**kwargs):
    parent = kwargs.pop("parent", base["Normal"])
    return ParagraphStyle(parent=parent, **kwargs)

TITLE_STYLE = make_style(
    name="DocTitle",
    fontSize=28, leading=34, textColor=BLANCO,
    fontName="Helvetica-Bold", alignment=TA_CENTER, spaceAfter=8,
)
SUBTITLE_STYLE = make_style(
    name="DocSubtitle",
    fontSize=14, leading=18, textColor=BLANCO,
    fontName="Helvetica", alignment=TA_CENTER, spaceAfter=4,
)
H1 = make_style(
    name="H1", fontSize=16, leading=20, textColor=AZUL_OSCURO,
    fontName="Helvetica-Bold", spaceBefore=18, spaceAfter=6,
    borderPad=0,
)
H2 = make_style(
    name="H2", fontSize=13, leading=17, textColor=AZUL_MEDIO,
    fontName="Helvetica-Bold", spaceBefore=14, spaceAfter=4,
)
H3 = make_style(
    name="H3", fontSize=11, leading=15, textColor=AZUL_OSCURO,
    fontName="Helvetica-Bold", spaceBefore=10, spaceAfter=3,
)
BODY = make_style(
    name="Body", fontSize=10, leading=14, textColor=GRIS_TEXTO,
    fontName="Helvetica", spaceBefore=2, spaceAfter=4,
    alignment=TA_JUSTIFY,
)
BULLET = make_style(
    name="Bullet", fontSize=10, leading=14, textColor=GRIS_TEXTO,
    fontName="Helvetica", spaceBefore=1, spaceAfter=1,
    leftIndent=14, firstLineIndent=0,
)
CODE = make_style(
    name="Code", fontSize=8.5, leading=12, textColor=colors.HexColor("#1a1a1a"),
    fontName="Courier", spaceBefore=2, spaceAfter=2,
    leftIndent=8, backColor=GRIS_CODIGO,
)
NOTE = make_style(
    name="Note", fontSize=9, leading=13, textColor=colors.HexColor("#555555"),
    fontName="Helvetica-Oblique", spaceBefore=2, spaceAfter=4,
    leftIndent=12,
)
TOC_H1 = make_style(name="TOC1", fontSize=11, leading=15, fontName="Helvetica-Bold",
                    textColor=AZUL_OSCURO, spaceBefore=4)
TOC_H2 = make_style(name="TOC2", fontSize=10, leading=14, fontName="Helvetica",
                    textColor=GRIS_TEXTO, leftIndent=14, spaceBefore=1)

# ─── Helpers ────────────────────────────────────────────────────────────────
def p(text, style=BODY):
    return Paragraph(text, style)

def h1(text): return Paragraph(text, H1)
def h2(text): return Paragraph(text, H2)
def h3(text): return Paragraph(text, H3)
def sp(h=0.3): return Spacer(1, h * cm)
def hr(): return HRFlowable(width="100%", thickness=1, color=GRIS_LINEA,
                             spaceBefore=4, spaceAfter=6)

def bullet(items):
    return [p(f"• {item}", BULLET) for item in items]

def code_block(lines):
    text = "<br/>".join(
        line.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
        for line in lines
    )
    return Paragraph(text, CODE)

def nota(text):
    return p(f"<i>Nota: {text}</i>", NOTE)

def table(data, col_widths, header=True):
    t = Table(data, colWidths=col_widths)
    style = [
        ("BACKGROUND",  (0, 0), (-1, 0 if header else -1), AZUL_MEDIO),
        ("TEXTCOLOR",   (0, 0), (-1, 0 if header else -1), BLANCO),
        ("FONTNAME",    (0, 0), (-1, 0 if header else -1), "Helvetica-Bold"),
        ("FONTSIZE",    (0, 0), (-1, -1), 9),
        ("LEADING",     (0, 0), (-1, -1), 13),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [BLANCO, AZUL_CLARO]),
        ("GRID",        (0, 0), (-1, -1), 0.4, GRIS_LINEA),
        ("VALIGN",      (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING",(0, 0), (-1, -1), 6),
        ("TOPPADDING",  (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING",(0,0), (-1, -1), 4),
    ]
    t.setStyle(TableStyle(style))
    return t


# ─── Portada ────────────────────────────────────────────────────────────────
def portada():
    cover_table = Table(
        [[
            Paragraph("Sistema de Gestión de Usuarios", TITLE_STYLE),
            Spacer(1, 0.5*cm),
            Paragraph("Documentación Técnica", SUBTITLE_STYLE),
            Spacer(1, 0.3*cm),
            Paragraph("TP — Ingeniería de Software", SUBTITLE_STYLE),
            Spacer(1, 1.2*cm),
            Paragraph(datetime.date.today().strftime("%B %Y").capitalize(), SUBTITLE_STYLE),
        ]],
        colWidths=[PAGE_W - 4*cm],
    )
    cover_table.setStyle(TableStyle([
        ("BACKGROUND",   (0, 0), (-1, -1), AZUL_OSCURO),
        ("LEFTPADDING",  (0, 0), (-1, -1), 1.5*cm),
        ("RIGHTPADDING", (0, 0), (-1, -1), 1.5*cm),
        ("TOPPADDING",   (0, 0), (-1, -1), 2.5*cm),
        ("BOTTOMPADDING",(0, 0), (-1, -1), 2.5*cm),
        ("ROUNDEDCORNERS", (0, 0), (-1, -1), 8),
    ]))
    return [
        Spacer(1, 4*cm),
        cover_table,
        PageBreak(),
    ]


# ─── Numeración de páginas ──────────────────────────────────────────────────
def on_page(canvas, doc):
    canvas.saveState()
    canvas.setFont("Helvetica", 8)
    canvas.setFillColor(colors.HexColor("#888888"))
    canvas.drawRightString(PAGE_W - 2*cm, 1.2*cm,
                           f"Sistema de Gestión de Usuarios — pág. {doc.page}")
    canvas.drawString(2*cm, 1.2*cm, "Documentación Técnica")
    canvas.setStrokeColor(GRIS_LINEA)
    canvas.setLineWidth(0.5)
    canvas.line(2*cm, 1.5*cm, PAGE_W - 2*cm, 1.5*cm)
    canvas.restoreState()

def on_first_page(canvas, doc):
    pass  # portada sin footer


# ─── Contenido ──────────────────────────────────────────────────────────────
def contenido():
    e = []  # elementos

    # ── 1. Descripción general ──────────────────────────────────────────────
    e += [h1("1. Descripción general"), hr()]
    e += [p(
        "Este documento describe la arquitectura, el modelo de datos, los patrones "
        "de diseño y los procedimientos de instalación y extensión del <b>Sistema de "
        "Gestión de Usuarios</b>, desarrollado como trabajo práctico de la materia "
        "Ingeniería de Software."
    ), sp(0.2)]
    e += [p(
        "La aplicación es un sistema de escritorio construido con <b>C# y Windows Forms</b> "
        "sobre .NET Framework, con base de datos <b>SQL Server</b>. Implementa gestión de "
        "usuarios y roles, control de acceso basado en permisos, soporte multiidioma "
        "mediante el patrón Observer, e integridad de datos mediante dígitos verificadores "
        "horizontales y verticales."
    ), sp(0.4)]

    e += [h2("1.1 Tecnologías")]
    e += [table(
        [["Componente", "Tecnología"],
         ["Lenguaje", "C# (.NET Framework 4.x)"],
         ["Interfaz de usuario", "Windows Forms"],
         ["Base de datos", "SQL Server (autenticación Windows)"],
         ["IDE recomendado", "Visual Studio 2022 Community"],
         ["Diagramas", "PlantUML (renderizados vía kroki.io)"]],
        [5*cm, 10*cm],
    ), sp(0.4)]

    # ── 2. Requisitos e instalación ─────────────────────────────────────────
    e += [h1("2. Requisitos e instalación"), hr()]

    e += [h2("2.1 Requisitos previos")]
    e += bullet([
        "Visual Studio 2022 (Community o superior)",
        "SQL Server instalado localmente (autenticación de Windows habilitada)",
        ".NET Framework 4.x",
        "Python 3.x (solo para regenerar diagramas PNG)",
    ])
    e += [sp(0.3)]

    e += [h2("2.2 Configuración de la base de datos")]
    e += [p("Crear la base de datos manualmente antes de ejecutar el script:")]
    e += [code_block(["CREATE DATABASE BDCAPAS;"]), sp(0.2)]
    e += [p("Ejecutar el script completo desde la raíz del proyecto:")]
    e += [code_block(["sqlcmd -S . -d BDCAPAS -E -i \"DAL\\script.sql\""]), sp(0.2)]
    e += [p(
        "El script es <b>mayormente idempotente</b>: los bloques <code>CREATE TABLE</code> "
        "y <code>CREATE PROCEDURE</code> del inicio fallarán si los objetos ya existen "
        "(inofensivo). Los bloques posteriores utilizan guardas <code>IF OBJECT_ID … DROP</code> "
        "antes de recrear, por lo que son completamente idempotentes."
    ), sp(0.2)]
    e += [nota(
        "El script crea la cuenta de administrador por defecto. "
        "La contraseña inicial está definida en el script; se recomienda cambiarla al primer inicio."
    ), sp(0.3)]

    e += [h2("2.3 Compilación")]
    e += [code_block([
        "& \"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\",
        "  MSBuild\\Current\\Bin\\MSBuild.exe\" TP_IS.sln /p:Configuration=Debug /v:minimal",
    ]), sp(0.2)]

    e += [h2("2.4 Ejecución")]
    e += [p("El ejecutable de entrada es: <code>CAPAS\\bin\\Debug\\CAPAS.exe</code>")]
    e += [sp(0.2)]

    e += [h2("2.5 Resetear integridad para pruebas")]
    e += [p(
        "Si se modificó la base de datos directamente y se necesita forzar "
        "que la aplicación reinicialice los dígitos verificadores en el próximo arranque:"
    )]
    e += [code_block(["DELETE FROM DIGITO_VERIFICADOR_VERTICAL WHERE TABLA = 'USUARIO';"]), sp(0.2)]
    e += [p(
        "Al no encontrar registros DVV, <code>EstaInicializado()</code> retorna "
        "<code>false</code> y <code>RecalcularIntegridad()</code> se ejecuta automáticamente."
    ), sp(0.4)]

    # ── 3. Estructura del proyecto ──────────────────────────────────────────
    e += [h1("3. Estructura del proyecto"), hr()]
    e += [code_block([
        "TP_IS.sln",
        "├── BE/                  Entidades (Plain Objects, sin lógica)",
        "├── DAL/                 Acceso a datos (stored procedures exclusivamente)",
        "│   └── script.sql       Script completo de BD",
        "├── BLL/                 Lógica de negocio",
        "├── SeguridadYServicios/ Servicios transversales",
        "├── CAPAS/               UI — Windows Forms",
        "│   └── Program.cs       Punto de entrada",
        "├── DER.puml             Diagrama entidad-relación (PlantUML)",
        "└── DiagramaClases.puml  Diagrama de clases (PlantUML)",
    ]), sp(0.3)]
    e += [p(
        "Para regenerar los diagramas PNG (requiere Python e internet):"
    )]
    e += [code_block([
        "python generar_png.py DER.puml DER.png",
        "python generar_png.py DiagramaClases.puml DiagramaClases.png",
    ]), sp(0.4)]

    # ── 4. Arquitectura ─────────────────────────────────────────────────────
    e += [h1("4. Arquitectura por capas"), hr()]
    e += [code_block([
        "CAPAS  (UI / WinForms)",
        "  ├── BLL",
        "  │     ├── DAL",
        "  │     │     └── BE",
        "  │     └── BE",
        "  └── SeguridadYServicios",
        "              └── BE",
    ]), sp(0.2)]
    e += [p(
        "Cada capa solo depende de las capas inferiores. "
        "<b>SeguridadYServicios</b> no referencia BLL ni DAL: recibe los datos "
        "que necesita directamente desde CAPAS."
    ), sp(0.3)]

    e += [h2("4.1 BE — Entidades")]
    e += [p("Clases de datos sin lógica de negocio. Las principales son:")]
    e += [table(
        [["Clase", "Descripción"],
         ["USUARIO", "Usuario del sistema. Incluye DVH int para integridad."],
         ["BITACORA", "Registro de eventos de sesión."],
         ["IDIOMA, CONTROL_IDIOMA", "Soporte multiidioma."],
         ["NodoPermiso", "Clase abstracta base del árbol de roles/permisos."],
         ["Rol", "Nodo rama (TIPO='PERFIL' en BD)."],
         ["Permiso", "Nodo hoja (TIPO='PERMISO' en BD)."],
         ["UsuarioHistorial", "Snapshot de estado de un usuario en un momento dado."],
         ["LoginResultado", "Enum: Exito, CredencialesInvalidas, UsuarioBloqueado."]],
        [4.5*cm, 10.5*cm],
    ), sp(0.4)]

    e += [h2("4.2 DAL — Acceso a datos")]
    e += [p(
        "Todo el acceso a la base de datos pasa por la clase interna <code>Acceso</code>, "
        "que utiliza exclusivamente stored procedures (nunca SQL inline) y requiere "
        "<code>SqlParameter</code> para todos los parámetros, previniendo SQL injection."
    )]
    e += bullet([
        "<code>Acceso.Leer()</code> devuelve un <code>DataTable</code>.",
        "<code>Acceso.Escribir()</code> devuelve el número de filas afectadas.",
        "<code>CrearParametro</code> acepta overloads para <code>string</code> e <code>int</code>. "
        "Para <code>bool</code>: usar <code>valor ? 1 : 0</code>. "
        "Para <code>int?</code>: construir <code>SqlParameter</code> manualmente con <code>DBNull.Value</code>.",
    ])
    e += [sp(0.3)]

    e += [h2("4.3 BLL — Lógica de negocio")]
    e += [p(
        "Orquesta operaciones combinando DAL y SeguridadYServicios. "
        "No realiza llamadas directas a la base de datos. "
        "Instancia los DAL directamente (<code>new DAL.UsuarioDAL()</code>)."
    ), sp(0.3)]

    e += [h2("4.4 SeguridadYServicios — Servicios transversales")]
    e += [table(
        [["Componente", "Responsabilidad"],
         ["SessionManager", "Singleton. Almacena el usuario autenticado y su lista de permisos."],
         ["IdiomaManager", "Singleton/Subject Observer. Distribuye traducciones a los formularios."],
         ["IObservadorIdioma", "Interfaz Observer. Todo formulario con soporte de idioma la implementa."],
         ["Hasher", "SHA-256 unidireccional para contraseñas."],
         ["ValidadorContrasena", "Reglas: mínimo 6 caracteres, una mayúscula, un dígito."],
         ["CalculadorDVH", "Cálculo de dígitos verificadores horizontal y vertical."]],
        [4.5*cm, 10.5*cm],
    ), sp(0.4)]

    # ── 5. Modelo de datos ──────────────────────────────────────────────────
    e += [PageBreak(), h1("5. Modelo de datos"), hr()]

    e += [h2("5.1 Tablas")]
    e += [table(
        [["Tabla", "Descripción"],
         ["USUARIO", "Usuarios del sistema. Columna DVH para integridad."],
         ["NODO_PERMISO", "Árbol de roles y permisos. TIPO='PERFIL' = rol, TIPO='PERMISO' = permiso catálogo."],
         ["ROL_PERMISO", "Relación N:M entre roles y permisos del catálogo."],
         ["USUARIO_PERFIL", "Relación N:M entre usuarios y roles."],
         ["BITACORA", "Auditoría de eventos: login, logout y acciones."],
         ["IDIOMA", "Idiomas disponibles en el sistema."],
         ["CONTROL_IDIOMA", "Claves de traducción de controles de la UI."],
         ["TRADUCCION", "Texto traducido por idioma y control."],
         ["USUARIO_HISTORIAL", "Snapshots de estado de usuarios (append-only)."],
         ["DIGITO_VERIFICADOR_VERTICAL", "DVV por tabla y columna lógica."]],
        [5*cm, 10*cm],
    ), sp(0.3)]
    e += [nota("La tabla PERSONA existe en la BD como remanente del scaffold inicial. No se utiliza.")]
    e += [sp(0.3)]

    e += [h2("5.2 Catálogo de permisos")]
    e += [p(
        "Los permisos son estáticos: no se crean ni eliminan desde la interfaz. "
        "Solo se asignan a roles. Los permisos disponibles son:"
    )]
    e += bullet([
        "Administrar usuarios",
        "Gestión de roles",
        "Gestión de idiomas",
        "Ver bitácora",
        "Cambiar contraseña",
    ])
    e += [sp(0.4)]

    # ── 6. Patrones de diseño ───────────────────────────────────────────────
    e += [h1("6. Patrones de diseño"), hr()]

    e += [h2("6.1 Composite — Árbol de roles y permisos")]
    e += [p(
        "<code>NodoPermiso</code> es el componente abstracto. "
        "<code>Rol</code> (archivo <code>PerfilPermiso.cs</code>) es el nodo rama, "
        "almacenado con <code>TIPO='PERFIL'</code>. "
        "<code>Permiso</code> es la hoja, almacenada con <code>TIPO='PERMISO'</code>."
    )]
    e += [p(
        "El árbol se construye en memoria en <code>PerfilBLL.ObtenerArbol()</code>: "
        "primero se cargan los roles y se arma la jerarquía desde <code>PADRE_ID</code>; "
        "luego se adjuntan los permisos a su rol correspondiente usando el campo "
        "<code>PadreId</code> como valor carrier."
    ), sp(0.3)]

    e += [h2("6.2 Observer — Soporte multiidioma")]
    e += [p(
        "<code>IdiomaManager</code> es el Subject (Singleton). "
        "Los formularios son los Observers e implementan "
        "<code>IObservadorIdioma.ActualizarIdioma()</code>."
    )]
    e += [p("El flujo al cambiar de idioma es:")]
    e += bullet([
        "El usuario selecciona un idioma en el combo del status bar de cualquier formulario.",
        "CAPAS llama a <code>IdiomaBLL.CargarTraducciones(idiomaId)</code> y pasa el "
        "diccionario a <code>IdiomaManager.CambiarIdioma()</code>.",
        "<code>IdiomaManager</code> llama a <code>Notificar()</code>, que invoca "
        "<code>ActualizarIdioma()</code> en cada formulario registrado.",
        "Cada formulario aplica la traducción a sus controles; si no existe la clave, "
        "usa el texto de diseño como fallback.",
    ])
    e += [sp(0.2)]
    e += [nota(
        "Cada formulario tiene su propio selector de idioma agregado dinámicamente "
        "por IdiomaUIHelper.AgregarSelector(form) al final del Load, porque ShowDialog() "
        "deshabilita el formulario padre."
    ), sp(0.3)]

    e += [h2("6.3 Singleton — SessionManager e IdiomaManager")]
    e += [p(
        "Ambos implementan el patrón Singleton con double-checked locking. "
        "<code>SessionManager.cerrarSesion()</code> anula la instancia para "
        "permitir re-autenticación sin reiniciar la aplicación."
    ), sp(0.4)]

    # ── 7. Sistema de seguridad ─────────────────────────────────────────────
    e += [h1("7. Sistema de seguridad"), hr()]

    e += [h2("7.1 Autenticación y bloqueo")]
    e += [p(
        "<code>LoginBLL.AutenticarUsuario()</code> valida credenciales, resetea el "
        "contador de intentos en caso de éxito e incrementa en caso de fallo. "
        "Al superar el límite (3 intentos fallidos), el usuario queda bloqueado automáticamente "
        "y solo un administrador puede desbloquearlo."
    ), sp(0.3)]

    e += [h2("7.2 Sistema de permisos")]
    e += [p(
        "Los permisos efectivos se cargan en <code>SessionManager</code> al hacer login, "
        "mediante el SP <code>USUARIO_PERMISOS_LISTAR</code> "
        "(join <code>USUARIO_PERFIL → ROL_PERMISO → NODO_PERMISO</code>)."
    )]
    e += [p(
        "La UI consulta <code>SessionManager.TienePermiso(\"nombre\")</code> para "
        "mostrar u ocultar elementos. La visibilidad de los ítems del menú se controla "
        "exclusivamente por permisos, nunca por comparación directa del campo ROL."
    ), sp(0.3)]

    e += [h2("7.3 Contraseñas")]
    e += bullet([
        "Almacenadas como SHA-256 (hexadecimal minúscula, 64 caracteres).",
        "PASS nunca se almacena en USUARIO_HISTORIAL.",
        "Reglas validadas por ValidadorContrasena: mínimo 6 caracteres, al menos "
        "una letra mayúscula y al menos un dígito.",
    ])
    e += [sp(0.4)]

    # ── 8. Integridad ───────────────────────────────────────────────────────
    e += [PageBreak(), h1("8. Sistema de integridad (DVH/DVV)"), hr()]
    e += [p(
        "El sistema protege la tabla <b>USUARIO</b> contra modificaciones externas "
        "a la aplicación. <b>BITACORA</b>, <b>NODO_PERMISO</b> y <b>ROL_PERMISO</b> "
        "no están protegidas por integridad."
    ), sp(0.3)]

    e += [h2("8.1 Dígito Verificador Horizontal (DVH)")]
    e += [p(
        "Un valor <code>int</code> por fila, almacenado en la columna "
        "<code>DVH</code> de <code>USUARIO</code>. "
        "Detecta modificaciones en cualquier atributo de un registro."
    )]
    e += [p("Fórmula:")]
    e += [code_block(["DVH = Σᵢ Σⱼ  Unicode(atributo[i][j])  ×  (i+1)  ×  (j+1)"])]
    e += [p(
        "Donde <code>i</code> es el índice del atributo (base 0) y "
        "<code>j</code> es el índice del carácter dentro del atributo (base 0)."
    ), sp(0.3)]

    e += [h2("8.2 Dígito Verificador Vertical (DVV)")]
    e += [p(
        "Un valor <code>int</code> por columna lógica, almacenado en "
        "<code>DIGITO_VERIFICADOR_VERTICAL (TABLA, COLUMNA, DVV)</code>. "
        "Detecta inserciones, eliminaciones e intercambios de filas."
    )]
    e += [code_block(["DVV = Σₖ Σⱼ  Unicode(fila[k][col][j])  ×  (k+1)  ×  (j+1)"])]
    e += [p(
        "Donde <code>k</code> es la posición de la fila en el conjunto ordenado por ID (base 0)."
    ), sp(0.3)]

    e += [h2("8.3 Atributos de USUARIO (orden canónico)")]
    e += [p("El orden es fijo y no debe modificarse una vez que existan datos almacenados:")]
    e += [code_block(["ID | USUARIO | PASS | INTENTOS_FALLIDOS | BLOQUEADO('1'/'0') | ROL | PERFILES"])]
    e += [nota(
        "PERFILES es un atributo virtual: IDs de perfiles del usuario en USUARIO_PERFIL, "
        "ordenados ascendentemente y separados por coma. Cambios externos en USUARIO_PERFIL "
        "invalidan el DVH del usuario afectado."
    ), sp(0.3)]

    e += [h2("8.4 Flujo al iniciar la aplicación")]
    e += [table(
        [["Paso", "Descripción"],
         ["1", "InicializarIntegridad() se ejecuta en Program.cs antes de abrir el login."],
         ["2", "Si no hay DVVs → RecalcularIntegridad() (primera ejecución o reset manual)."],
         ["3", "VerificarIntegridad() se ejecuta y el resultado se almacena en Program.ResultadoIntegridad."],
         ["4", "La aplicación siempre abre el formulario de login, independientemente del resultado."],
         ["5a", "Login exitoso + integridad válida → RecalcularIntegridadUsuarios() y abrir menú."],
         ["5b", "Login exitoso + integridad inválida + admin no comprometido → frmRestaurarIntegridad."],
         ["5c", "Login exitoso + integridad inválida + cualquier otro caso → mensaje genérico, sesión bloqueada."]],
        [1.2*cm, 13.8*cm],
    ), sp(0.3)]

    e += [h2("8.5 Cuándo recalcular")]
    e += [p(
        "Después de toda mutación de <code>USUARIO</code> o <code>USUARIO_PERFIL</code>, "
        "se debe llamar a <code>RecalcularIntegridadUsuarios()</code>:"
    )]
    e += [table(
        [["Trigger", "Responsable"],
         ["Login exitoso o fallido (integridad OK)", "LogIn.cs"],
         ["Admin elige Recalcular o Restaurar", "frmRestaurarIntegridad"],
         ["Crear, eliminar, desbloquear usuario", "UsuarioBLL.*"],
         ["Cambiar contraseña", "UsuarioBLL.CambiarContrasena"],
         ["Asignar perfiles a un usuario", "UsuarioPerfilBLL.GuardarAsignaciones"],
         ["Rollback de historial", "UsuarioHistorialBLL.Rollback"]],
        [8*cm, 7*cm],
    ), sp(0.2)]
    e += [nota(
        "LoginBLL nunca llama a RecalcularIntegridadUsuarios(). "
        "Toda responsabilidad de recálculo vive en la capa CAPAS o en BLL post-mutación."
    ), sp(0.3)]

    e += [h2("8.6 frmRestaurarIntegridad")]
    e += [p(
        "Se muestra a un administrador cuya propia data no esté comprometida "
        "cuando la verificación detecta errores. Ofrece tres opciones:"
    )]
    e += [table(
        [["Opción", "Efecto"],
         ["Recalcular y continuar", "Acepta el estado actual de la BD como legítimo, recalcula todo y abre el menú."],
         ["Restaurar desde historial", "Abre frmHistorialUsuario por cada usuario afectado para hacer rollback. "
                                       "Deshabilitado si no existe historial para los afectados."],
         ["Cancelar", "Cierra sesión y retorna al formulario de login."]],
        [4.5*cm, 10.5*cm],
    ), sp(0.2)]
    e += [nota(
        "Los errores se registran en integridad_error.log junto al .exe "
        "cada vez que frmRestaurarIntegridad se carga."
    ), sp(0.3)]

    e += [h2("8.7 Limitación conocida")]
    e += [p(
        "La verificación de integridad ocurre solo al iniciar la aplicación. "
        "Si la base de datos se corrompe durante una sesión activa, la corrupción "
        "no se detecta en esa sesión. Peor aún: si ocurre una mutación posterior "
        "a la corrupción, <code>RecalcularIntegridadUsuarios()</code> acepta los "
        "datos corruptos como nuevo baseline, anulando la evidencia de la intrusión."
    ), sp(0.4)]

    # ── 9. Control de cambios ───────────────────────────────────────────────
    e += [h1("9. Control de cambios (historial)"), hr()]
    e += [p(
        "<code>USUARIO_HISTORIAL</code> es una tabla append-only: sin UPDATEs ni DELETEs. "
        "Cada fila es un snapshot completo del estado de un usuario. "
        "<code>PASS</code> nunca se almacena en el historial."
    ), sp(0.3)]

    e += [h2("9.1 Tipos de cambio")]
    e += [table(
        [["Tipo", "Descripción"],
         ["ALTA", "Creación de un nuevo usuario."],
         ["BAJA", "Eliminación de un usuario. Se registra antes del DELETE."],
         ["BLOQUEO", "El usuario fue bloqueado por intentos fallidos."],
         ["DESBLOQUEO", "Un administrador desbloqueó al usuario."],
         ["CAMBIO_CLAVE", "El usuario cambió su contraseña."],
         ["ASIGNACION_PERFIL", "Se modificaron los roles asignados al usuario."],
         ["ROLLBACK", "Se restauró un estado anterior desde el historial."]],
        [4*cm, 11*cm],
    ), sp(0.3)]

    e += [h2("9.2 Regla de timing")]
    e += [p(
        "<code>RegistrarCambio</code> se llama <b>después</b> del cambio, con una excepción: "
        "<b>BAJA</b> se registra <b>antes</b> de <code>_dal.Eliminar()</code> para capturar "
        "el último estado conocido mientras el registro aún existe en la base de datos."
    ), sp(0.3)]

    e += [h2("9.3 Rollback")]
    e += [p(
        "<code>UsuarioHistorialBLL.Rollback()</code> restaura <code>ROL</code>, "
        "<code>BLOQUEADO</code>, <code>INTENTOS_FALLIDOS</code> y <code>PERFILES</code> "
        "desde el snapshot elegido. <code>PASS</code> queda intacto. El rollback se "
        "registra como nueva entrada <code>ROLLBACK</code> con <code>VERSION_ORIGEN</code> "
        "apuntando al snapshot restaurado."
    )]
    e += [nota(
        "No existe FK entre USUARIO_HISTORIAL y USUARIO. Esto es intencional: "
        "el historial sobrevive a la eliminación del usuario."
    ), sp(0.4)]

    # ── 10. Soporte multiidioma ─────────────────────────────────────────────
    e += [PageBreak(), h1("10. Soporte multiidioma"), hr()]

    e += [h2("10.1 Registrar un formulario nuevo")]
    e += bullet([
        "Implementar <code>SeguridadYServicios.IObservadorIdioma</code>.",
        "Declarar <code>Dictionary&lt;string, Control&gt; _controles</code> y "
        "<code>Dictionary&lt;string, string&gt; _defaults</code>.",
        "En <code>Load</code>: llamar a <code>GuardarDefaults(this.Controls)</code>, "
        "registrar el título del formulario, llamar a "
        "<code>IdiomaManager.getInstance().Registrar(this)</code>, "
        "<code>ActualizarIdioma()</code> y al final <code>IdiomaUIHelper.AgregarSelector(this)</code>.",
        "En <code>FormClosed</code>: <code>IdiomaManager.getInstance().Desregistrar(this)</code>.",
    ])
    e += [sp(0.3)]

    e += [h2("10.2 Claves y colisiones")]
    e += [p(
        "Si dos formularios tienen un control con el mismo <code>Name</code>, "
        "el segundo sobreescribe la clave del primero en el diccionario global. "
        "La solución es eliminar la clave genérica y registrar una específica por formulario:"
    )]
    e += [code_block([
        "_controles.Remove(\"lblTitulo\");",
        "_defaults.Remove(\"lblTitulo\");",
        "_controles[\"lblTitulo_MiForm\"] = lblTitulo;",
        "_defaults[\"lblTitulo_MiForm\"]  = lblTitulo.Text;",
    ]), sp(0.2)]

    e += [h2("10.3 Casos especiales")]
    e += [table(
        [["Caso", "Cómo manejarlo"],
         ["Barra de título del formulario",
          "Registrar con _controles[this.Name] = this; _defaults[this.Name] = this.Text;"],
         ["Encabezados de DataGridView",
          "No están en Controls. Traducirlos en ActualizarEncabezados(), llamado desde "
          "ActualizarIdioma() y después de cada asignación de DataSource."],
         ["Texto dinámico (nodos TreeView, texto generado)",
          "Usar IdiomaManager.getInstance().Traducir(\"clave\") directamente, con fallback manual."]],
        [4.5*cm, 10.5*cm],
    ), sp(0.3)]

    e += [h2("10.4 Agregar traducciones a la base de datos")]
    e += [code_block([
        "EXEC CONTROL_REGISTRAR 'clave_control', '[Texto por defecto]';",
        "EXEC TRADUCCION_GUARDAR 1, (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE='clave_control'), 'Texto ES';",
        "EXEC TRADUCCION_GUARDAR 2, (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE='clave_control'), 'Texto EN';",
    ]), sp(0.4)]

    # ── 11. Convenciones de BD ──────────────────────────────────────────────
    e += [h1("11. Convenciones de base de datos"), hr()]
    e += bullet([
        "Solo stored procedures. Nunca SQL inline desde el código.",
        "Todo parámetro debe usar <code>SqlParameter</code> (prevención de SQL injection).",
        "<code>CrearParametro</code> acepta <code>string</code> e <code>int</code>. "
        "Para <code>bool</code>: <code>valor ? 1 : 0</code>. "
        "Para <code>int?</code>: construir <code>SqlParameter</code> con <code>DBNull.Value</code>.",
        "<code>EXEC</code> no acepta expresiones como parámetros — asignar a variable primero.",
        "<code>GO</code> reinicia el scope de <code>DECLARE</code>. Redeclarar variables en cada batch.",
        "Para eliminar una columna con DEFAULT constraint: primero eliminar el constraint "
        "dinámicamente (nombre autogenerado varía por instancia), luego la columna.",
        "Nuevas tablas y SPs se agregan al final de <code>DAL/script.sql</code>.",
    ])
    e += [sp(0.4)]

    # ── 12. Guías de extensión ──────────────────────────────────────────────
    e += [h1("12. Guías para extender el sistema"), hr()]

    e += [h2("12.1 Agregar un formulario con soporte de idioma")]
    e += [table(
        [["Paso", "Acción"],
         ["1", "Implementar SeguridadYServicios.IObservadorIdioma."],
         ["2", "Agregar Dictionary<string, Control> _controles y Dictionary<string, string> _defaults."],
         ["3", "En Load: GuardarDefaults → registrar título → Registrar + ActualizarIdioma → AgregarSelector al final."],
         ["4", "En FormClosed: IdiomaManager.getInstance().Desregistrar(this)."],
         ["5", "ActualizarIdioma() itera _controles aplicando Traducir(key) ?? _defaults[key]."],
         ["6", "Agregar los eventos Load y FormClosed en el .Designer.cs."],
         ["7", "Agregar .cs y .Designer.cs en CAPAS/UI.csproj."],
         ["8", "Agregar claves en script.sql con EXEC CONTROL_REGISTRAR y EXEC TRADUCCION_GUARDAR."]],
        [1.2*cm, 13.8*cm],
    ), sp(0.3)]

    e += [h2("12.2 Extender la integridad a una nueva entidad")]
    e += bullet([
        "Agregar columna <code>DVH INT NOT NULL DEFAULT 0</code> a la tabla.",
        "Agregar filas a <code>DIGITO_VERIFICADOR_VERTICAL</code> para la nueva tabla.",
        "Definir el array canónico de atributos y un método <code>ExtraerAtributos*</code> en <code>IntegridadBLL</code>.",
        "Llamar a <code>VerificarTabla</code> y <code>RecalcularTabla</code> (helpers internos genéricos) "
        "siguiendo el patrón existente de USUARIO.",
        "Actualizar <code>RecalcularIntegridad()</code> para incluir la nueva entidad.",
    ])
    e += [sp(0.4)]

    return e


# ─── Main ────────────────────────────────────────────────────────────────────
def main():
    output = r"C:\Users\jesic\1 MIS PROYECTOS\ingenieria_software\Documentacion_Tecnica.pdf"

    doc = SimpleDocTemplate(
        output,
        pagesize=A4,
        leftMargin=2*cm, rightMargin=2*cm,
        topMargin=2.2*cm, bottomMargin=2.2*cm,
        title="Documentación Técnica — Sistema de Gestión de Usuarios",
        author="Equipo TP Ingeniería de Software",
        subject="Documentación técnica para desarrolladores",
    )

    story = portada() + contenido()
    doc.build(story, onFirstPage=on_first_page, onLaterPages=on_page)
    print(f"PDF generado: {output}")


if __name__ == "__main__":
    main()
