"""Expande macros @IR / @RC / @RB de seq_src/*.puml y escribe los .puml finales en DIAGRAMAS/.
Uso: python -I seq_build.py [--out DIR] [CU01 CU02 ...]   (por defecto escribe en DIAGRAMAS/)
Macros (una por linea, con sangria libre):
  @IR caller
  @RC caller | usuarioId | TIPO | realizadoPor [| versionOrigen]
  @RB            (cuerpo de UsuarioHistorialBLL.Rollback despues de las guardas, participante UH)
"""
import sys, re, pathlib
HERE = pathlib.Path(__file__).parent
SRC = HERE / 'seq_src'
OUT = HERE.resolve().parents[1]   # DIAGRAMAS/ (secuencia -> herramientas -> DIAGRAMAS)

NL = '\\n'   # salto de linea literal de PlantUML


def IR(c):
    return f"""{c} -> IB : RecalcularIntegridadUsuarios()
activate IB
IB -> IND : ListarUsuariosParaIntegridad()
IND -> DB : USUARIO_LISTAR_PARA_INTEGRIDAD
DB --> IND : filas de USUARIO
IND --> IB : DataTable
loop por cada usuario
  IB -> UPD : ListarPerfilesPorUsuario(id)
  UPD -> DB : USUARIO_PERFIL_LISTAR(@usuario_id)
  DB --> UPD : PERFIL_ID
  UPD --> IB : List<int>
  IB -> CD : Calcular(atributos)
  CD --> IB : int dvh
  IB -> IND : ActualizarDVHUsuario(id, dvh)
  IND -> DB : USUARIO_ACTUALIZAR_DVH(@id, @dvh)
end
loop por cada una de las 7 columnas (c = índice)
  IB -> CD : CalcularVertical(todasFilas, c)
  CD --> IB : int dvv
  IB -> IND : ActualizarDVV("USUARIO", columnas[c], dvv)
  IND -> DB : DVV_ACTUALIZAR(@tabla, @columna, @dvv)
end
IB --> {c} : (DVH / DVV recalculados)
deactivate IB"""


def RC(c, uid, tipo, por, ver=None):
    args = f'{uid}, "{tipo}", {por}' + (f', {ver}' if ver else '')
    vo = ver if ver else 'null'
    selfcall = (c == 'UH')
    head = f"{c} -> UH : RegistrarCambio({args})"
    ret_null = (f"note right of UH : return (no inserta nada)" if selfcall
                else f"UH --> {c} : return (no inserta nada)")
    ret_ok = "" if selfcall else f"\n  UH --> {c} : (cambio registrado)"
    return f"""{head}
activate UH
UH -> UD : ObtenerPorId(usuarioId)
UD -> DB : USUARIO_OBTENER_POR_ID(@id)
DB --> UD : fila de USUARIO
UD --> UH : BE.USUARIO / null
alt el usuario no existe (u == null)
  {ret_null}
else el usuario existe
  UH -> UPD : ListarPerfilesPorUsuario(usuarioId)
  UPD -> DB : USUARIO_PERFIL_LISTAR(@usuario_id)
  DB --> UPD : PERFIL_ID
  UPD --> UH : List<int> perfiles
  UH -> UH : ordenar los ids y unirlos en CSV (Perfiles)
  UH -> UHD : Insertar(UsuarioHistorial)
  UHD -> DB : USUARIO_HISTORIAL_INSERTAR(@usuario_id, @usuario_login, @rol,{NL}@bloqueado, @intentos_fallidos, @perfiles, @nombre, @apellido,{NL}@telefono, @email, @realizado_por, @tipo_cambio = "{tipo}",{NL}@version_origen = {vo}, @rol_id){ret_ok}
end
deactivate UH"""


def RB(notas=None):
    rn6 = "RT-06: " if notas else ""
    n1 = ""
    n2 = ""
    if notas:
        n1 = f"\nnote right of DB : RT-02: la contraseña (PASS) nunca se restaura —{NL}no se almacena en el historial. Si el snapshot tiene{NL}RolId NULL se conserva el ROL_ID actual (ISNULL en el SP)"
        n2 = f"\nnote right of DB : RT-05: los datos personales (Nombre, Apellido,{NL}Teléfono, Email) también se restauran del snapshot"
    return f"""UH -> UD : AplicarEstado(usuarioId, rol, bloqueado,{NL}intentosFallidos, rolId)
UD -> DB : USUARIO_APLICAR_ESTADO(@id, @rol, @bloqueado,{NL}@intentos_fallidos, @rol_id){n1}
UH -> UPD : BorrarTodos(usuarioId)
UPD -> DB : USUARIO_PERFIL_BORRAR_TODOS(@usuario_id)
UH -> UH : ParsearPerfiles(snapshot.Perfiles)
loop por cada perfil del snapshot
  UH -> UPD : Asignar(usuarioId, perfilId)
  UPD -> DB : USUARIO_PERFIL_ASIGNAR(@usuario_id, @perfil_id)
end
UH -> UD : ActualizarDatos(usuarioId, nombre, apellido,{NL}telefono, email)
UD -> DB : USUARIO_ACTUALIZAR_DATOS(@id, @nombre, @apellido,{NL}@telefono, @email){n2}
@RC UH | usuarioId | ROLLBACK | realizadoPor | historialId
@IR UH
UH -> SM : getUsuario()
SM --> UH : usuarioSesion
opt usuarioSesion != null y{NL}usuarioSesion.Id == snapshot.UsuarioId
  UH -> UD : ObtenerPorId(usuarioId)
  UD -> DB : USUARIO_OBTENER_POR_ID(@id)
  DB --> UD : fila de USUARIO
  UD --> UH : usuarioActualizado
  UH -> SM : setUsuario(usuarioActualizado)
  UH -> UPD : ListarPermisosDeUsuario(usuarioId)
  UPD -> DB : USUARIO_PERMISOS_LISTAR(@usuario_id)
  DB --> UPD : NOMBRE de cada permiso
  UPD --> UH : permisosActualizados
  UH -> SM : setPermisos(permisosActualizados)
  note right of SM : {rn6}refresco en caliente: no hace falta{NL}desloguearse y volver a entrar
end"""


def LU():
    return f"""FA -> FA : CargarUsuarios()
FA -> UB : ListarPaginado(_busquedaActual,{NL}_paginaActual, TAMANIO_PAGINA)
UB -> UD : ListarPaginado(busqueda, pagina, tamanio)
UD -> DB : USUARIO_LISTAR_PAGINADO(@busqueda,{NL}@pagina, @tamanio)
DB --> UD : filas + TOTAL_FILAS
UD --> UB : PaginaResultado<USUARIO>
UB --> FA : PaginaResultado<USUARIO>
FA -> FA : dgvUsuarios.DataSource = resultado.Items;{NL}ocultar columnas, ActualizarEncabezados() y{NL}ActualizarControlesPaginacion()"""


def CA():
    return f"""FP -> FP : CargarArbol()
FP -> PB : ObtenerArbol()
PB -> PD : ListarRoles()
PD -> DB : ROL_LISTAR_TODOS
DB --> PD : filas de ROL
PD --> PB : List<NodoPermiso>
PB -> PB : armar la jerarquía de roles{NL}(PadreId → Agregar hijos)
PB -> PD : ListarRolPermisos()
PD -> DB : ROL_PERMISO_LISTAR_TODOS
DB --> PD : filas de ROL_PERMISO + PERMISO
PD --> PB : List<Permiso>
PB -> PB : adjuntar cada permiso a su rol{NL}(PadreId = ROL_ID → Agregar)
PB --> FP : List<NodoPermiso> (raíces)
FP -> FP : armar los TreeNode (CrearNodoVisual con prefijo{NL}"[Rol] " / "[Permiso] " vía Traducir) y ExpandAll()"""


def CI():
    return f"""FI -> FI : CargarIdiomas()
FI -> IB : ListarTodos()
IB -> IDD : ListarTodos()
IDD -> DB : IDIOMA_LISTAR_TODOS
DB --> IDD : filas de IDIOMA
IDD --> IB : List<IDIOMA>
IB --> FI : List<IDIOMA>
FI -> FI : dgvIdiomas.DataSource = null;{NL}dgvIdiomas.DataSource = idiomas
FI -> FI : dgvIdiomas_SelectionChanged{NL}(al re-enlazar la grilla queda seleccionada{NL}la primera fila: _idiomaSeleccionado)
@CT"""


def CT():
    return f"""FI -> FI : CargarTraducciones()
FI -> IB : ListarControlesConTraduccion(idiomaId)
IB -> IDD : ListarControlesConTraduccion(idiomaId)
IDD -> DB : TRADUCCION_LISTAR_CONTROLES(@idioma_id)
DB --> IDD : filas (ID, CLAVE, TEXTO_DEFAULT, TEXTO_TRADUCCION)
IDD --> IB : List<CONTROL_IDIOMA>
IB --> FI : List<CONTROL_IDIOMA>
FI -> FI : dgvTraducciones.DataSource = null;{NL}dgvTraducciones.DataSource = controles"""


def LC():
    return f"""FC -> FC : CargarVinos()
FC -> VB : ListarCatalogo(sesion)
VB -> VD : ListarCatalogo(sesion.Id)
VD -> DB : VINO_LISTAR_CATALOGO(@usuario_id)
DB --> VD : filas de VINO con su última solicitud
VD --> VB : List<Vino>
VB --> FC : List<Vino>
FC -> FC : dgvVinos.DataSource = vinos; ocultar columnas;{NL}ActualizarEncabezados() y ActualizarEstadoBajaBoton()"""


def LP(tipo='Alta'):
    g = 'dgvPendientes' if tipo == 'Alta' else 'dgvPendientesBaja'
    return f"""FA -> FA : CargarPendientes()
FA -> VB : ListarSolicitudesPendientes(TipoSolicitud.{tipo})
VB -> SD : ListarPendientes(TipoSolicitud.{tipo})
SD -> DB : SOLICITUD_LISTAR_PENDIENTES(@tipo)
DB --> SD : filas (solicitud + emisor + datos del vino + stock actual)
SD --> VB : List<SolicitudAutorizacion>
VB --> FA : List<SolicitudAutorizacion>
FA -> FA : {g}.DataSource = solicitudes; ocultar columnas;{NL}ActualizarEncabezados(); ActualizarSeleccion(){NL}(MostrarDetalle + ActualizarEstadoBotones)"""


def AV(f='FE'):
    return f"""{f} -> {f} : ActualizarVinoSeleccionado()
alt cboVino.SelectedItem no es un Vino (sin vinos)
  {f} -> {f} : lblStockActualValor = "-";{NL}dgvMovimientos.DataSource = null; return
else hay un vino seleccionado
  {f} -> MB : ObtenerStockActual(vino.Id)
  MB -> MD : ObtenerStockActual(vinoId)
  MD -> DB : STOCK_ACTUAL_OBTENER(@vino_id)
  DB --> MD : STOCK (suma de entradas menos salidas)
  MD --> MB : int
  MB --> {f} : int
  {f} -> {f} : lblStockActualValor = stock
  {f} -> MB : ObtenerMovimientos(vino.Id)
  MB -> MD : ListarPorVino(vinoId)
  MD -> DB : MOVIMIENTO_STOCK_LISTAR_POR_VINO(@vino_id)
  DB --> MD : filas
  MD --> MB : List<MovimientoStock>
  MB --> {f} : List<MovimientoStock>
  {f} -> {f} : dgvMovimientos.DataSource = movimientos{NL}ordenados por Fecha descendente (en la UI)
end"""


def AT(modo=None):
    ini = "" if modo == 'load' else "FA -> FA : RefreshLineas()" + chr(10)
    return f"""{ini}FA -> FA : ActualizarTotales()
FA -> CB : CalcularTotal(_lineas)
CB --> FA : decimal total
FA -> FA : lblPresupuestoValor y lblTotalValor{NL}(en rojo si total > presupuesto); btnConfirmarArmado.Enabled ={NL}socio seleccionado y _lineas.Count > 0 y total <= presupuesto{NL}y período con formato YYYY-MM"""


def SS():
    return f"""FA -> FA : cboSocio_SelectedIndexChanged: _lineas.Clear();{NL}presupuesto = socio.PresupuestoMensual
FA -> CB : ListarCandidatos(socio.Id)
CB -> VD : ListarCandidatos(socioId)
VD -> DB : VINO_LISTAR_CANDIDATOS(@socio_id)
DB --> VD : filas de vinos Activos con stock derivado > 0 (RN-06),{NL}ordenadas por PREFERIDO DESC, PRECIO ASC, NOMBRE ASC (RN-05)
VD --> CB : List<VinoCandidato>
CB --> FA : List<VinoCandidato>
FA -> FA : dgvCandidatos.DataSource = candidatos{NL}(columna Preferido resaltada)
@AT"""


def CC(modo=None):
    ini = 'dgvCajasArmadas_SelectionChanged → CargarComposicion()' if modo != 'directo' else 'CargarComposicion()'
    return f"""FP -> FP : {ini}
alt no hay caja seleccionada (CurrentRow nulo)
  FP -> FP : limpia las grillas y el combo de reemplazos;{NL}btnRegistrarSustitucion.Enabled = false
else hay una caja seleccionada (cajaFila)
  FP -> CB : ObtenerConComposicionEfectiva(cajaFila.Id)
  CB -> CD : ObtenerPorId(cajaId)
  CD -> DB : CAJA_OBTENER_POR_ID(@id)
  DB --> CD : fila
  CD --> CB : CajaMensual (o null)
  opt la caja no existe
    CB --> FP : throw InvalidOperationException("La caja no existe.")
    note right of FP : esta llamada de la UI no tiene try/catch
  end
  CB -> CD : ListarComposicionEfectiva(cajaId)
  CD -> DB : CAJA_VINO_LISTAR_EFECTIVO(@caja_id)
  DB --> CD : composición efectiva + stock disponible por línea
  CD --> CB : List<CajaVino>
  note right of CD
    RN-07 — la composición efectiva se DERIVA
    de CAJA_VINO y SUSTITUCION en el SP; nunca se edita la línea.
  end note
  CB -> CB : caja.Lineas = composición efectiva
  CB --> FP : CajaMensual
  FP -> FP : dgvComposicionEfectiva.DataSource = caja.Lineas;{NL}CellFormatting resalta las líneas con{NL}StockDisponible < Cantidad (RN-03: no bloquea)
  FP -> CB : ListarCandidatos(caja.SocioId)
  CB -> VD : ListarCandidatos(socioId)
  VD -> DB : VINO_LISTAR_CANDIDATOS(@socio_id)
  DB --> VD : filas
  VD --> CB : List<VinoCandidato>
  CB --> FP : List<VinoCandidato>
  FP -> FP : cboVinoReemplazo.DataSource = candidatos{NL}(todos los candidatos del socio, sin filtrar por presupuesto)
  FP -> CB : ListarTrazaSustituciones(caja.Id)
  CB -> SD : ListarPorCaja(cajaId)
  SD -> DB : SUSTITUCION_LISTAR_POR_CAJA(@caja_id)
  DB --> SD : filas
  SD --> CB : List<Sustitucion>
  CB --> FP : List<Sustitucion>
  FP -> FP : dgvTrazaSustituciones.DataSource = traza
  FP -> FP : btnRegistrarSustitucion.Enabled = (caja.ArmadoPor != _sesion.Id){NL}(RN-10, capa UI)
end"""


def VS():
    return f"""SB -> SB : Validar(s)
break 30a. (RN-13) el nombre está vacío o solo espacios
  SB --> FS : throw InvalidOperationException{NL}("El nombre es obligatorio.")
end
break 30a. (RN-13) el apellido está vacío o solo espacios
  SB --> FS : throw InvalidOperationException{NL}("El apellido es obligatorio.")
end
break 30a. (RN-13) el domicilio está vacío o solo espacios
  SB --> FS : throw InvalidOperationException{NL}("El domicilio es obligatorio.")
end
break 30b. el presupuesto mensual es menor o igual a cero
  SB --> FS : throw InvalidOperationException{NL}("El presupuesto mensual debe ser mayor a cero.")
end"""


def CS():
    return f"""FS -> FS : dgvSocios_SelectionChanged → CargarParaEdicion(seleccionado.Id)
break la fila actual no es un Socio (grilla sin filas o sin fila actual)
  FS -> FS : return
end
FS -> SB : ObtenerPorId(id)
SB -> SD : ObtenerPorId(id)
SD -> DB : SOCIO_OBTENER_POR_ID(@id)
DB --> SD : filas
opt el socio existe (hay filas)
  SD -> DB : SOCIO_VARIETAL_LISTAR(@socio_id)
  DB --> SD : filas (VARIETAL)
end
SD --> SB : Socio con VarietalesPreferidos (o null)
SB --> FS : Socio (o null)
break el socio no existe (s == null)
  FS -> FS : return
end
FS -> FS : _socioIdActual = s.Id; carga txtNombre, txtApellido, txtEmail,{NL}txtTelefono, txtDomicilio, numPresupuesto y chkActivo;{NL}tilda en chkVarietales los varietales preferidos del socio"""


def LS():
    return f"""FS -> FS : CargarSocios()
FS -> SB : ListarActivos()
SB -> SD : ListarActivos()
SD -> DB : SOCIO_LISTAR_ACTIVOS
DB --> SD : filas de SOCIO (ACTIVO = 1)
SD --> SB : List<Socio>
SB --> FS : List<Socio>
FS -> FS : dgvSocios.DataSource = socios; oculta Id, Activo, CreadoPor{NL}y VarietalesPreferidos; ActualizarEncabezados()
note over FS, DB
  Al enlazar la grilla queda seleccionada la primera fila y se dispara
  dgvSocios_SelectionChanged, sin acción del usuario.
end note
@CS"""


def OC(c, arg, ctx='sinTry'):
    nota = ("esta llamada de la UI no tiene try/catch" if ctx == 'sinTry'
            else "la excepción salta al catch (Exception) de este bloque")
    return f"""{c} -> CB : ObtenerConComposicionEfectiva({arg})
CB -> CD : ObtenerPorId(cajaId)
CD -> DB : CAJA_OBTENER_POR_ID(@id)
DB --> CD : fila
CD --> CB : CajaMensual (o null)
break la caja no existe
  CB --> {c} : throw InvalidOperationException{NL}("La caja no existe.")
  note right of {c} : {nota}
end
CB -> CD : ListarComposicionEfectiva(cajaId)
CD -> DB : CAJA_VINO_LISTAR_EFECTIVO(@caja_id)
DB --> CD : composición efectiva + stock disponible por línea
CD --> CB : List<CajaVino>
CB -> CB : caja.Lineas = composición efectiva
CB --> {c} : CajaMensual (con Lineas)"""


def DS():
    return f"""FD -> FD : dgvCajasArmadas_SelectionChanged
alt no hay una caja seleccionada (la fila actual no es una CajaMensual)
  FD -> FD : dgvComposicionEfectiva.DataSource = null;{NL}btnDespachar.Enabled = false
else hay una caja seleccionada (cajaFila)
  @OC FD | cajaFila.Id | sinTry
  FD -> FD : dgvComposicionEfectiva.DataSource = caja.Lineas; oculta columnas{NL}y ActualizarEncabezados(); CellFormatting resalta en rojo{NL}las líneas con StockDisponible < Cantidad
  FD -> FD : btnDespachar.Enabled = (caja.ArmadoPor != _sesion.Id){NL}(RN-10, capa UI)
end"""


def LA():
    return f"""FD -> FD : CargarCajasArmadas()
FD -> CB : ListarArmadas()
CB -> CD : ListarPorEstado("Armada")
CD -> DB : CAJA_LISTAR_POR_ESTADO(@estado)
DB --> CD : filas
CD --> CB : List<CajaMensual>
CB --> FD : List<CajaMensual>
FD -> FD : dgvCajasArmadas.DataSource = cajas; oculta columnas{NL}y ActualizarEncabezados()
note over FD, DB
  Al enlazar la grilla queda seleccionada la primera fila y se dispara
  dgvCajasArmadas_SelectionChanged, sin acción del usuario.
end note
@DS"""


def expand(text):
    out = []
    for line in text.split('\n'):
        m = re.match(r'^(\s*)@(IR|RC|RB|LU|CA|CI|CT|LC|LP|AV|AT|SS|CC|VS|CS|LS|OC|DS|LA)\s*(.*)$', line.rstrip('\r'))
        if not m:
            out.append(line)
            continue
        ind, kind, rest = m.groups()
        a = [x.strip() for x in rest.split('|')] if rest.strip() else []
        blk = {'IR': IR, 'RC': RC, 'RB': RB, 'LU': LU, 'CA': CA, 'CI': CI, 'CT': CT, 'LC': LC, 'LP': LP, 'AV': AV, 'AT': AT, 'SS': SS, 'CC': CC, 'VS': VS, 'CS': CS, 'LS': LS, 'OC': OC, 'DS': DS, 'LA': LA}[kind](*a)
        blk = expand(blk)
        out.extend((ind + l if l else l) for l in blk.split('\n'))
    return '\n'.join(out)


if __name__ == '__main__':
    pats = sys.argv[1:]
    if '--out' in pats:
        i = pats.index('--out')
        OUT = pathlib.Path(pats[i + 1]).resolve()
        OUT.mkdir(parents=True, exist_ok=True)
        pats = pats[:i] + pats[i + 2:]
    for f in sorted(SRC.glob('*.puml')):
        if pats and not any(p.lower() in f.name.lower() for p in pats):
            continue
        txt = expand(f.read_text(encoding='utf-8'))
        tgt = OUT / f.name
        nl = '\r\n' if tgt.exists() and b'\r\n' in tgt.read_bytes() else '\n'
        tgt.write_bytes(txt.replace('\r\n', '\n').replace('\n', nl).encode('utf-8'))
        print('built', f.name, len(txt.split('\n')), 'lines')
