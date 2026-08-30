# -*- coding: utf-8 -*-
"""
Genera CasosDeUso.docx con la descripción de los 20 casos de uso del TP.
"""

from docx import Document
from docx.shared import Pt, Cm, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_ALIGN_VERTICAL
from docx.oxml.ns import qn
from docx.oxml import OxmlElement


# ─────────────────────────────────────────────────────────────────────
# Datos de los casos de uso
# ─────────────────────────────────────────────────────────────────────

CUS = [
    # ───── CU-01 ─────────────────────────────────────────────────────
    {
        "id": "CU-01",
        "nombre": "Iniciar Sesión",
        "actor_primario": "Usuario",
        "actor_secundario": "Administrador (solo en flujo alternativo 9a)",
        "frecuencia": "Alta",
        "prioridad": "Crítica",
        "proposito": (
            "Permitir que un usuario registrado se autentique en el sistema para "
            "acceder a las funcionalidades correspondientes a su rol."
        ),
        "precondiciones": [
            "La aplicación CAPAS.exe está iniciada.",
            "La base de datos BDCAPAS está accesible.",
            "El sistema ya ejecutó la verificación de integridad inicial y conoce su resultado.",
            "El usuario existe en la base de datos.",
        ],
        "postcondiciones_exito": [
            "Existe una sesión activa en SessionManager con el usuario autenticado y su lista de permisos.",
            "El contador de intentos fallidos del usuario queda en cero.",
            "El evento de login queda registrado en la bitácora.",
            "Los dígitos verificadores de la tabla USUARIO quedan recalculados.",
            "Se muestra el menú principal.",
        ],
        "postcondiciones_fallo": [
            "No se crea sesión.",
            "Si las credenciales son inválidas: el contador de intentos fallidos se incrementa y queda en bitácora el evento LOGIN_FALLIDO.",
            "Si se alcanzó el límite de intentos: el usuario queda bloqueado, se registra USUARIO_BLOQUEADO en bitácora y BLOQUEO en el historial del usuario.",
            "Si la integridad está comprometida y el usuario no puede restaurar: queda registrado el error en integridad_error.log y se cierra la sesión.",
        ],
        "disparador": "El usuario presiona el botón Ingresar en el formulario de login.",
        "flujo_principal": [
            "El usuario ingresa su nombre de usuario y contraseña.",
            "El usuario presiona Ingresar.",
            "El sistema valida que ambos campos no estén vacíos.",
            "El sistema verifica que el usuario no se encuentre bloqueado.",
            "El sistema valida las credenciales contra la base de datos.",
            "El sistema resetea el contador de intentos fallidos del usuario.",
            "El sistema carga los permisos del usuario y los almacena en la sesión.",
            "El sistema registra el evento de login en la bitácora.",
            "El sistema evalúa el resultado de la verificación de integridad realizada al arranque.",
            "El sistema recalcula los dígitos verificadores de la tabla USUARIO.",
            "El sistema muestra un mensaje de bienvenida y abre el menú principal.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Algún campo está vacío",
                "pasos": [
                    "El sistema muestra el mensaje \"Completá usuario y contraseña\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "4a", "nombre": "El usuario está bloqueado",
                "pasos": [
                    "El sistema no valida las credenciales.",
                    "Si la integridad es válida, el sistema recalcula los dígitos verificadores.",
                    "El sistema muestra el mensaje \"Usuario bloqueado por intentos fallidos. Contactate con un administrador.\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "5a", "nombre": "Credenciales inválidas — sin alcanzar el límite de intentos",
                "pasos": [
                    "El sistema incrementa el contador de intentos fallidos.",
                    "El sistema registra el evento LOGIN_FALLIDO en la bitácora.",
                    "Si la integridad es válida, el sistema recalcula los dígitos verificadores.",
                    "El sistema muestra el mensaje \"Usuario o contraseña incorrectos\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "5b", "nombre": "Credenciales inválidas — se alcanzó el límite de intentos",
                "pasos": [
                    "El sistema incrementa el contador y bloquea al usuario.",
                    "El sistema registra LOGIN_FALLIDO y USUARIO_BLOQUEADO en la bitácora.",
                    "El sistema registra BLOQUEO en el historial del usuario.",
                    "Si la integridad es válida, el sistema recalcula los dígitos verificadores.",
                    "El sistema muestra el mensaje \"Usuario bloqueado por intentos fallidos\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "9a", "nombre": "Integridad comprometida — admin con datos íntegros",
                "pasos": [
                    "El sistema invoca el caso de uso CU-02 Restaurar Integridad del Sistema («extend»).",
                    "Si la restauración finaliza con éxito, el caso de uso continúa en el paso 11.",
                    "Si el administrador cancela, el sistema cierra la sesión y el caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "9b", "nombre": "Integridad comprometida — usuario no habilitado",
                "pasos": [
                    "El sistema muestra \"El sistema no puede iniciarse debido a un problema interno. Comuníquese con el administrador del sistema.\".",
                    "El sistema cierra la sesión.",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos.",
             "manejo": "Mensaje genérico al usuario, registro en log de la aplicación, vuelve al paso 1."},
            {"codigo": "EX-02", "descripcion": "Error inesperado durante la verificación de integridad.",
             "manejo": "El resultado se marca como inválido con el detalle del error y se sigue el flujo 9a / 9b."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Las contraseñas se almacenan con hash SHA-256 unidireccional. La contraseña en claro nunca se guarda ni se loguea."},
            {"codigo": "RN-02", "regla": "Un usuario queda bloqueado automáticamente al alcanzar el límite de intentos fallidos consecutivos."},
            {"codigo": "RN-03", "regla": "La verificación de integridad se ejecuta una sola vez, al arrancar la aplicación. Su resultado no impide abrir el formulario de login pero sí condiciona el acceso al menú."},
            {"codigo": "RN-04", "regla": "El recálculo de dígitos verificadores se realiza después de toda mutación de USUARIO, salvo cuando la integridad ya estaba comprometida (no se debe sanar silenciosamente datos corruptos)."},
            {"codigo": "RN-05", "regla": "Solo un administrador cuyos propios datos no estén entre los afectados por la corrupción puede acceder al sistema cuando la integridad está comprometida, y únicamente para restaurarla."},
        ],
        "relaciones": [
            {"tipo": "«extend»", "destino": "CU-02 Restaurar Integridad del Sistema",
             "condicion": "Integridad inválida y el usuario es Administrador con sus propios datos íntegros."},
        ],
        "observaciones": (
            "El formulario de login siempre está disponible aunque la integridad falle; la decisión de "
            "permitir o no el acceso se toma después de validar las credenciales. El recálculo en los "
            "flujos 4a, 5a y 5b se realiza incluso ante un intento fallido porque la operación de "
            "incrementar intentos modifica la tabla USUARIO."
        ),
    },
    # ───── CU-02 ─────────────────────────────────────────────────────
    {
        "id": "CU-02",
        "nombre": "Restaurar Integridad del Sistema",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja (solo ante detección de corrupción)",
        "prioridad": "Crítica",
        "proposito": (
            "Permitir que un administrador autorizado restablezca la integridad del sistema "
            "cuando la verificación al arranque detectó que la tabla USUARIO fue modificada "
            "fuera de la aplicación."
        ),
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "La verificación de integridad al arranque devolvió un resultado inválido.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El ID del administrador no figura en la lista de usuarios con DVH inválido.",
        ],
        "postcondiciones_exito": [
            "Los dígitos verificadores horizontal y vertical de la tabla USUARIO quedan recalculados.",
            "Si se eligió Restaurar: los usuarios afectados quedan en el estado del snapshot seleccionado y se registra una entrada ROLLBACK en su historial.",
            "El error inicial queda registrado en integridad_error.log.",
            "El administrador accede al menú principal.",
        ],
        "postcondiciones_fallo": [
            "Si el administrador cancela: la sesión queda cerrada y el sistema vuelve al login. La integridad permanece comprometida.",
        ],
        "disparador": (
            "Tras un login exitoso, el sistema detecta integridad inválida y el usuario tiene "
            "permiso para restaurar; se abre frmRestaurarIntegridad."
        ),
        "flujo_principal": [
            "El sistema muestra el detalle de los errores detectados.",
            "El sistema registra los errores en integridad_error.log.",
            "El sistema habilita el botón \"Restaurar desde historial\" solo si hay al menos un usuario afectado.",
            "El administrador elige \"Recalcular y continuar\".",
            "El sistema recalcula los dígitos verificadores horizontal y vertical de la tabla USUARIO.",
            "El sistema cierra el formulario y continúa con la apertura del menú principal.",
        ],
        "flujos_alternativos": [
            {
                "id": "4a", "nombre": "Restaurar desde historial",
                "pasos": [
                    "Para cada usuario afectado con historial disponible, el sistema abre el formulario de Historial de Usuario (CU-10).",
                    "El administrador selecciona la versión a restaurar y confirma (CU-11).",
                    "Una vez procesados todos los afectados, el sistema recalcula los dígitos verificadores.",
                    "El sistema cierra el formulario y continúa al paso 6.",
                ],
            },
            {
                "id": "4a.i", "nombre": "Algún usuario afectado no tiene historial",
                "pasos": [
                    "El sistema muestra \"El usuario 'X' no tiene historial de cambios registrado. No es posible restaurarlo desde historial. Use 'Recalcular y continuar'\".",
                    "El sistema continúa con el siguiente usuario afectado.",
                ],
            },
            {
                "id": "4b", "nombre": "Cancelar",
                "pasos": [
                    "El administrador presiona Cancelar.",
                    "El sistema cierra el formulario, cierra la sesión y vuelve al formulario de login.",
                ],
            },
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error al escribir en integridad_error.log.",
             "manejo": "El sistema continúa sin interrumpir el flujo (el log es best-effort)."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "La opción \"Restaurar desde historial\" requiere que al menos un usuario afectado tenga registros en USUARIO_HISTORIAL. Si ninguno tiene, queda deshabilitada."},
            {"codigo": "RN-02", "regla": "El rollback no restaura la contraseña — siempre se mantiene la actual en USUARIO."},
            {"codigo": "RN-03", "regla": "Si la corrupción ocurrió antes de que se implementara el historial, la única opción es \"Recalcular y continuar\" (acepta el estado actual como nuevo baseline)."},
        ],
        "relaciones": [
            {"tipo": "«extend» de", "destino": "CU-01 Iniciar Sesión",
             "condicion": "Solo se invoca cuando la integridad está comprometida tras un login exitoso de un admin habilitado."},
            {"tipo": "«include»", "destino": "CU-10 Ver Historial de Usuario",
             "condicion": "Se usa para mostrar y elegir la versión a restaurar."},
            {"tipo": "«include»", "destino": "CU-11 Restaurar Usuario desde Historial",
             "condicion": "Se ejecuta al confirmar el rollback de cada usuario afectado."},
        ],
        "observaciones": (
            "Recalcular acepta el estado actual de la BD como legítimo: si la modificación externa "
            "fue malintencionada, el atacante \"sale ganando\". Restaurar desde historial es la opción "
            "segura cuando existen snapshots previos. La decisión queda a cargo del administrador."
        ),
    },
    # ───── CU-03 ─────────────────────────────────────────────────────
    {
        "id": "CU-03",
        "nombre": "Cerrar Sesión",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Alta",
        "prioridad": "Alta",
        "proposito": "Permitir al usuario finalizar su sesión y volver al formulario de login.",
        "precondiciones": [
            "Existe una sesión activa en SessionManager.",
            "El menú principal está abierto.",
        ],
        "postcondiciones_exito": [
            "El evento LOGOUT queda registrado en la bitácora con el usuario.",
            "La sesión activa queda anulada (SessionManager._instance = null) para permitir una nueva autenticación.",
            "Se cierra el menú principal y se abre el formulario de login.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El usuario selecciona la opción \"Cerrar sesión\" desde el menú principal.",
        "flujo_principal": [
            "El usuario selecciona \"Cerrar sesión\" desde el menú.",
            "El sistema obtiene el nombre del usuario actual.",
            "El sistema registra el evento LOGOUT en la bitácora.",
            "El sistema invoca SessionManager.cerrarSesion() para anular la sesión.",
            "El sistema abre el formulario de login y cierra el menú principal.",
        ],
        "flujos_alternativos": [],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El cierre de sesión no requiere confirmación: es una operación reversible (el usuario puede volver a iniciar sesión)."},
            {"codigo": "RN-02", "regla": "La sesión se anula completamente — los permisos cargados en memoria se descartan."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-04 ─────────────────────────────────────────────────────
    {
        "id": "CU-04",
        "nombre": "Cambiar Contraseña",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "proposito": "Permitir al usuario modificar su propia contraseña, garantizando su autenticación previa y la validez de la nueva clave.",
        "precondiciones": [
            "El usuario inició sesión correctamente.",
            "El usuario tiene el permiso \"Cambiar contraseña\".",
        ],
        "postcondiciones_exito": [
            "La contraseña del usuario queda actualizada en la base de datos con su nuevo hash SHA-256.",
            "Se registra el evento CAMBIO_CONTRASENA en la bitácora.",
            "Se registra una entrada CAMBIO_CLAVE en el historial del usuario.",
            "Los dígitos verificadores de USUARIO quedan recalculados.",
        ],
        "postcondiciones_fallo": [
            "La contraseña no se modifica.",
            "Se muestra el mensaje de error correspondiente.",
        ],
        "disparador": "El usuario selecciona \"Cambiar contraseña\" en el menú principal y presiona Continuar.",
        "flujo_principal": [
            "El usuario ingresa contraseña actual, nueva contraseña y confirmación.",
            "El usuario presiona Continuar.",
            "El sistema valida que ningún campo esté vacío.",
            "El sistema valida que la nueva contraseña coincida con la confirmación.",
            "El sistema valida que la nueva contraseña cumpla las reglas de fortaleza (mínimo 6 caracteres, al menos una mayúscula, al menos un dígito).",
            "El sistema verifica que la contraseña actual ingresada coincida con la almacenada.",
            "El sistema actualiza la contraseña en la base de datos con el nuevo hash.",
            "El sistema registra el evento en la bitácora y crea un snapshot en el historial.",
            "El sistema recalcula los dígitos verificadores.",
            "El sistema muestra \"Contraseña cambiada exitosamente\" y cierra el formulario.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Algún campo vacío",
                "pasos": [
                    "El sistema muestra \"Completá todos los campos\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "4a", "nombre": "La nueva contraseña no coincide con la confirmación",
                "pasos": [
                    "El sistema muestra \"Las contraseñas no coinciden\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "5a", "nombre": "La nueva contraseña no cumple las reglas de fortaleza",
                "pasos": [
                    "El sistema muestra el detalle del incumplimiento (mínimo 6 caracteres, una mayúscula, un dígito).",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "6a", "nombre": "La contraseña actual es incorrecta",
                "pasos": [
                    "El sistema muestra \"La contraseña actual es incorrecta\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
            {
                "id": "7a", "nombre": "Error al persistir el cambio",
                "pasos": [
                    "El sistema muestra \"Error al cambiar la contraseña\".",
                    "El caso de uso vuelve al paso 1.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "La contraseña en claro nunca se persiste ni se guarda en el historial — solo se almacena su hash SHA-256."},
            {"codigo": "RN-02", "regla": "La nueva contraseña debe tener mínimo 6 caracteres, al menos una mayúscula y al menos un dígito."},
            {"codigo": "RN-03", "regla": "El usuario solo puede cambiar su propia contraseña — un administrador no puede cambiarle la clave a otro."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-05 ─────────────────────────────────────────────────────
    {
        "id": "CU-05",
        "nombre": "Cambiar Idioma",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": (
            "Permitir al usuario cambiar el idioma de la interfaz de la aplicación. "
            "El cambio impacta inmediatamente en todos los formularios abiertos."
        ),
        "precondiciones": [
            "El usuario está autenticado.",
            "Existe al menos un idioma habilitado distinto del idioma activo.",
        ],
        "postcondiciones_exito": [
            "El IdiomaManager registra el nuevo idioma activo y su diccionario de traducciones.",
            "Todos los formularios registrados como observadores actualizan sus textos al nuevo idioma.",
            "Los controles sin traducción muestran el texto por defecto.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El usuario selecciona un idioma distinto en el combo de idiomas presente en cualquier formulario.",
        "flujo_principal": [
            "El usuario despliega el combo de idiomas en un formulario.",
            "El usuario selecciona un idioma de la lista.",
            "El sistema obtiene el diccionario de traducciones del idioma elegido.",
            "El sistema actualiza el IdiomaManager con el nuevo idioma activo.",
            "El sistema notifica a todos los formularios registrados.",
            "Cada formulario recorre sus controles y aplica la traducción correspondiente, o el texto por defecto si no hay traducción.",
        ],
        "flujos_alternativos": [
            {
                "id": "2a", "nombre": "El usuario selecciona el mismo idioma activo",
                "pasos": [
                    "No se dispara ningún evento de cambio (la lógica de la UI ignora la selección redundante).",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El combo de idiomas solo muestra los idiomas marcados como habilitados."},
            {"codigo": "RN-02", "regla": "Cada formulario implementa la interfaz IObservadorIdioma y se registra al cargar / desregistra al cerrarse."},
            {"codigo": "RN-03", "regla": "Si una clave no tiene traducción en el idioma activo, se usa el texto de diseño como fallback."},
        ],
        "relaciones": [],
        "observaciones": (
            "El selector de idioma se agrega dinámicamente al final del Load de cada formulario "
            "porque los diálogos modales deshabilitan el form padre y la barra de estado deja de ser "
            "accesible."
        ),
    },
    # ───── CU-06 ─────────────────────────────────────────────────────
    {
        "id": "CU-06",
        "nombre": "Crear Usuario",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "proposito": "Permitir al administrador dar de alta un nuevo usuario en el sistema.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
        ],
        "postcondiciones_exito": [
            "El usuario queda creado con su contraseña hasheada, su rol y los contadores en valores iniciales (intentos = 0, bloqueado = false).",
            "Se registra una entrada ALTA en el historial del usuario.",
            "Los dígitos verificadores de USUARIO quedan recalculados.",
            "El usuario nuevo aparece en la grilla de administración de usuarios.",
        ],
        "postcondiciones_fallo": [
            "Si el nombre ya existe: no se crea el usuario y se muestra un aviso.",
        ],
        "disparador": "El administrador presiona el botón \"Nuevo\" en el formulario de administración de usuarios.",
        "flujo_principal": [
            "El administrador presiona \"Nuevo\" en frmAdminUsuarios.",
            "El sistema abre el formulario frmNuevoUsuario.",
            "El administrador ingresa nombre de usuario, contraseña y selecciona un rol (admin o usuario).",
            "El administrador presiona Aceptar.",
            "El sistema valida que el nombre y la contraseña no estén vacíos.",
            "El sistema calcula el hash SHA-256 de la contraseña.",
            "El sistema inserta el nuevo registro en USUARIO con los valores iniciales.",
            "El sistema recalcula los dígitos verificadores y registra el alta en el historial.",
            "El sistema muestra \"Usuario creado correctamente\" y refresca la grilla.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Nombre o contraseña vacíos",
                "pasos": [
                    "El sistema muestra \"Ingresá un nombre de usuario\" o \"Ingresá una contraseña\".",
                    "El caso de uso vuelve al paso 3.",
                ],
            },
            {
                "id": "4a", "nombre": "El administrador cancela",
                "pasos": [
                    "El sistema cierra frmNuevoUsuario sin crear ningún registro.",
                ],
            },
            {
                "id": "7a", "nombre": "El nombre de usuario ya existe",
                "pasos": [
                    "El sistema muestra \"El nombre de usuario ya existe\".",
                    "El caso de uso vuelve al paso 3.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El nombre de usuario debe ser único en el sistema."},
            {"codigo": "RN-02", "regla": "La contraseña se almacena únicamente como hash SHA-256, no en claro."},
            {"codigo": "RN-03", "regla": "Al crear el usuario, el campo Rol acepta solo los valores 'admin' o 'usuario'. Este campo es informativo: los permisos reales se asignan vía CU-09."},
            {"codigo": "RN-04", "regla": "Toda alta queda registrada en el historial con tipo ALTA y nombre del administrador que la realizó."},
        ],
        "relaciones": [],
        "observaciones": (
            "El campo 'Rol' del usuario es un descriptor general (admin/usuario) que no determina "
            "los permisos efectivos — esos se obtienen vía USUARIO_PERFIL → ROL_PERMISO. Para "
            "darle permisos reales al usuario nuevo, ver CU-09."
        ),
    },
    # ───── CU-07 ─────────────────────────────────────────────────────
    {
        "id": "CU-07",
        "nombre": "Eliminar Usuario",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Alta",
        "proposito": "Permitir al administrador eliminar un usuario del sistema.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El usuario a eliminar existe y no es el propio administrador que ejecuta la acción.",
        ],
        "postcondiciones_exito": [
            "Se registra una entrada BAJA en el historial del usuario antes de eliminarlo (snapshot del último estado conocido).",
            "El usuario queda eliminado de la tabla USUARIO.",
            "Los dígitos verificadores quedan recalculados.",
            "La grilla de usuarios se refresca y ya no muestra al usuario eliminado.",
        ],
        "postcondiciones_fallo": [
            "El usuario no se elimina.",
        ],
        "disparador": "El administrador selecciona un usuario en la grilla y presiona el botón \"Eliminar\".",
        "flujo_principal": [
            "El administrador selecciona un usuario en la grilla.",
            "El administrador presiona \"Eliminar\".",
            "El sistema verifica que el usuario seleccionado no sea el propio administrador.",
            "El sistema solicita confirmación al administrador.",
            "El administrador confirma la eliminación.",
            "El sistema registra una entrada BAJA en el historial del usuario.",
            "El sistema elimina al usuario de la base de datos.",
            "El sistema recalcula los dígitos verificadores.",
            "El sistema refresca la grilla de usuarios.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún usuario seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un usuario\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "3a", "nombre": "El usuario seleccionado es el propio administrador",
                "pasos": [
                    "El sistema muestra \"No podés eliminar tu propio usuario\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "5a", "nombre": "El administrador cancela la confirmación",
                "pasos": [
                    "El sistema no elimina al usuario.",
                    "El caso de uso termina.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Un administrador no puede eliminarse a sí mismo."},
            {"codigo": "RN-02", "regla": "La entrada BAJA del historial se registra antes de la eliminación, para preservar el snapshot del último estado del usuario."},
            {"codigo": "RN-03", "regla": "El historial sobrevive a la eliminación del usuario — no hay FK entre USUARIO_HISTORIAL y USUARIO."},
            {"codigo": "RN-04", "regla": "La eliminación física borra el registro en USUARIO; sus asignaciones en USUARIO_PERFIL se eliminan por cascada del SP."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-08 ─────────────────────────────────────────────────────
    {
        "id": "CU-08",
        "nombre": "Desbloquear Usuario",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador desbloquear un usuario que quedó bloqueado por exceder el límite de intentos fallidos.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El usuario seleccionado está bloqueado.",
        ],
        "postcondiciones_exito": [
            "El campo BLOQUEADO del usuario queda en false y los intentos fallidos en 0.",
            "Se registra el evento DESBLOQUEO_USUARIO en la bitácora a nombre del administrador.",
            "Se registra una entrada DESBLOQUEO en el historial del usuario desbloqueado.",
            "Los dígitos verificadores quedan recalculados.",
            "La grilla se refresca y el usuario aparece como desbloqueado.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un usuario y presiona \"Desbloquear\".",
        "flujo_principal": [
            "El administrador selecciona un usuario en la grilla.",
            "El administrador presiona \"Desbloquear\".",
            "El sistema verifica que el usuario esté efectivamente bloqueado.",
            "El sistema solicita confirmación.",
            "El administrador confirma.",
            "El sistema desbloquea al usuario y resetea los intentos.",
            "El sistema registra el evento en la bitácora y en el historial.",
            "El sistema recalcula los dígitos verificadores.",
            "El sistema muestra \"Usuario desbloqueado\" y refresca la grilla.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún usuario seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un usuario\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "3a", "nombre": "El usuario no está bloqueado",
                "pasos": [
                    "El sistema muestra \"El usuario no está bloqueado\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "5a", "nombre": "El administrador cancela",
                "pasos": [
                    "El sistema no realiza el desbloqueo.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Solo se puede desbloquear un usuario que esté efectivamente bloqueado."},
            {"codigo": "RN-02", "regla": "El desbloqueo resetea también los intentos fallidos a 0."},
            {"codigo": "RN-03", "regla": "La acción queda registrada con el nombre del administrador que la ejecutó, tanto en bitácora como en historial."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-09 ─────────────────────────────────────────────────────
    {
        "id": "CU-09",
        "nombre": "Asignar Perfiles a Usuario",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "proposito": (
            "Permitir al administrador asignar uno o varios roles a un usuario para determinar "
            "los permisos efectivos con los que operará en el sistema."
        ),
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El usuario a modificar existe.",
            "Existe al menos un rol en el sistema.",
        ],
        "postcondiciones_exito": [
            "Las asignaciones previas en USUARIO_PERFIL para ese usuario quedan reemplazadas por la nueva selección.",
            "Se registra una entrada ASIGNACION_PERFIL en el historial del usuario.",
            "Los dígitos verificadores quedan recalculados (PERFILES forma parte del DVH).",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un usuario y presiona \"Modificar perfiles\".",
        "flujo_principal": [
            "El administrador selecciona un usuario en la grilla.",
            "El administrador presiona \"Modificar perfiles\".",
            "El sistema abre frmAsignarPerfiles y muestra el árbol de roles disponibles.",
            "El sistema marca con check los roles actualmente asignados al usuario.",
            "El administrador marca o desmarca roles en el árbol.",
            "El administrador presiona \"Guardar\".",
            "El sistema borra las asignaciones previas y guarda las nuevas dentro de una operación atómica.",
            "El sistema recalcula los dígitos verificadores y registra la asignación en el historial.",
            "El sistema muestra \"Asignaciones guardadas\".",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún usuario seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un usuario\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "6a", "nombre": "El administrador cierra el formulario sin guardar",
                "pasos": [
                    "El sistema cierra frmAsignarPerfiles sin modificar las asignaciones existentes.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El árbol muestra solo nodos rama (roles), no las hojas (permisos del catálogo)."},
            {"codigo": "RN-02", "regla": "Guardar reemplaza completamente la lista anterior — no es una operación incremental."},
            {"codigo": "RN-03", "regla": "Los permisos efectivos del usuario se derivan en login a partir de los roles asignados y sus permisos relacionados."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-10 ─────────────────────────────────────────────────────
    {
        "id": "CU-10",
        "nombre": "Ver Historial de Usuario",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": "Permitir al administrador consultar los cambios registrados sobre un usuario a lo largo del tiempo.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El usuario a consultar existe.",
        ],
        "postcondiciones_exito": [
            "Se muestra al administrador la lista de snapshots históricos del usuario, ordenados por fecha.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un usuario y presiona \"Historial\".",
        "flujo_principal": [
            "El administrador selecciona un usuario en la grilla.",
            "El administrador presiona \"Historial\".",
            "El sistema abre frmHistorialUsuario y carga las entradas del historial para ese usuario.",
            "El sistema muestra una grilla con fecha, tipo de cambio, rol, bloqueado, intentos fallidos, perfiles, realizado por y versión origen (cuando aplica).",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún usuario seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un usuario\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "3a", "nombre": "El usuario no tiene historial",
                "pasos": [
                    "El sistema abre el formulario con la grilla vacía. El botón Rollback queda deshabilitado.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "La tabla USUARIO_HISTORIAL es append-only: no se editan ni eliminan entradas históricas."},
            {"codigo": "RN-02", "regla": "El campo PASS nunca se almacena en el historial."},
            {"codigo": "RN-03", "regla": "Las entradas con TipoCambio = BAJA capturan el último estado conocido antes de eliminar al usuario."},
        ],
        "relaciones": [
            {"tipo": "«include» en", "destino": "CU-02 Restaurar Integridad",
             "condicion": "Se invoca cuando el admin elige Restaurar desde historial."},
        ],
        "observaciones": None,
    },
    # ───── CU-11 ─────────────────────────────────────────────────────
    {
        "id": "CU-11",
        "nombre": "Restaurar Usuario desde Historial (Rollback)",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Alta",
        "proposito": (
            "Permitir al administrador restaurar el estado de un usuario a una versión histórica "
            "anterior cuando se detecta una modificación indebida o un error operativo."
        ),
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Administrar usuarios\".",
            "El usuario tiene al menos una entrada en USUARIO_HISTORIAL.",
            "La entrada seleccionada no es de tipo BAJA.",
            "La entrada seleccionada no es de tipo ROLLBACK.",
        ],
        "postcondiciones_exito": [
            "Los campos ROL, BLOQUEADO, INTENTOS_FALLIDOS del usuario quedan iguales al snapshot elegido.",
            "Las asignaciones en USUARIO_PERFIL del usuario coinciden con las del snapshot.",
            "Se registra una nueva entrada ROLLBACK en el historial con VersionOrigen apuntando al snapshot restaurado.",
            "Los dígitos verificadores quedan recalculados.",
            "La contraseña del usuario permanece inalterada (no se restaura).",
        ],
        "postcondiciones_fallo": [
            "Si la entrada seleccionada no se encuentra: se lanza una excepción y no se aplica ningún cambio.",
            "Si la entrada seleccionada es de tipo ROLLBACK: el sistema rechaza la operación y no se aplica ningún cambio.",
        ],
        "disparador": "El administrador selecciona una entrada del historial y presiona \"Rollback\".",
        "flujo_principal": [
            "El administrador, dentro de CU-10, selecciona una entrada del historial.",
            "El administrador presiona \"Rollback\".",
            "El sistema solicita confirmación, aclarando que la contraseña no se restaurará.",
            "El administrador confirma.",
            "El sistema aplica al usuario el estado del snapshot (ROL, BLOQUEADO, INTENTOS_FALLIDOS).",
            "El sistema borra las asignaciones actuales de perfiles y reasigna las del snapshot.",
            "El sistema registra una nueva entrada ROLLBACK en el historial con VersionOrigen apuntando al snapshot original.",
            "El sistema recalcula los dígitos verificadores.",
            "El sistema muestra \"Estado restaurado correctamente\" y refresca el historial.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "La entrada seleccionada es de tipo BAJA",
                "pasos": [
                    "El sistema mantiene deshabilitado el botón Rollback.",
                    "El administrador no puede iniciar el caso de uso sobre esa entrada.",
                ],
            },
            {
                "id": "2a", "nombre": "La entrada seleccionada es de tipo ROLLBACK",
                "pasos": [
                    "El sistema muestra el mensaje \"No se puede restaurar una versión que ya es un rollback\" (Operación no permitida).",
                    "El sistema no solicita confirmación ni aplica ningún cambio.",
                ],
            },
            {
                "id": "4a", "nombre": "El administrador cancela la confirmación",
                "pasos": [
                    "El sistema no aplica ningún cambio.",
                ],
            },
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "La entrada histórica no existe o no se puede recuperar.",
             "manejo": "Se lanza una excepción InvalidOperationException con el mensaje \"Versión histórica no encontrada\"."},
            {"codigo": "EX-02", "descripcion": "La entrada seleccionada es de tipo ROLLBACK (validación de respaldo en la capa de negocio).",
             "manejo": "UsuarioHistorialBLL.Rollback lanza InvalidOperationException con el mensaje \"No se puede restaurar una versión que ya es un rollback\"; la UI la captura y la muestra en un MessageBox de error."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Las entradas de tipo BAJA no son restaurables — el usuario ya no existe."},
            {"codigo": "RN-02", "regla": "La contraseña nunca se restaura en un rollback, porque no se almacena en el historial."},
            {"codigo": "RN-03", "regla": "Cada rollback genera una entrada nueva en el historial; los snapshots anteriores nunca se borran."},
            {"codigo": "RN-04", "regla": "Las entradas de tipo ROLLBACK no son restaurables, para evitar cadenas de rollbacks recursivos. La validación se aplica tanto en la UI (antes de pedir confirmación) como en la capa de negocio (defensa en profundidad)."},
        ],
        "relaciones": [
            {"tipo": "«include» en", "destino": "CU-02 Restaurar Integridad",
             "condicion": "Se invoca para cada usuario afectado al elegir Restaurar desde historial."},
        ],
        "observaciones": None,
    },
    # ───── CU-12 ─────────────────────────────────────────────────────
    {
        "id": "CU-12",
        "nombre": "Crear Rol",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador crear un nuevo rol en la jerarquía de roles del sistema.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de roles\".",
        ],
        "postcondiciones_exito": [
            "Existe un nuevo nodo de tipo PERFIL en NODO_PERMISO con el nombre elegido y el padre indicado.",
            "El árbol de roles se refresca y muestra el nuevo rol.",
        ],
        "postcondiciones_fallo": [
            "Si el padre elegido generaría una referencia circular: no se crea y se muestra el error.",
        ],
        "disparador": "El administrador presiona \"Agregar rol\" en el formulario de gestión de perfiles.",
        "flujo_principal": [
            "El administrador presiona \"Agregar rol\".",
            "El sistema solicita el nombre del nuevo rol.",
            "El administrador ingresa el nombre y confirma.",
            "El sistema toma como padre el rol seleccionado en el árbol (o ninguno si no hay selección).",
            "El sistema verifica que el padre elegido no genere una referencia circular.",
            "El sistema inserta el nuevo nodo con TIPO = PERFIL.",
            "El sistema refresca el árbol y deja visible el nuevo rol.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Nombre vacío",
                "pasos": [
                    "El sistema no crea el rol y vuelve al estado anterior.",
                ],
            },
            {
                "id": "5a", "nombre": "El padre elegido generaría un ciclo",
                "pasos": [
                    "El sistema muestra \"No se puede asignar ese padre: generaría una referencia circular\".",
                    "El caso de uso termina sin crear el rol.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Un rol no puede tener como padre (directo o indirecto) a sí mismo."},
            {"codigo": "RN-02", "regla": "Los nuevos roles se crean sin permisos asignados — se asignan luego vía CU-15."},
            {"codigo": "RN-03", "regla": "Los nuevos roles tienen PROTEGIDO = 0 por defecto."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-13 ─────────────────────────────────────────────────────
    {
        "id": "CU-13",
        "nombre": "Eliminar Rol",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador eliminar un rol y todos sus sub-roles del sistema.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de roles\".",
            "El rol no está marcado como protegido (PROTEGIDO = 0).",
            "Ningún rol del subárbol del rol seleccionado tiene usuarios asignados.",
        ],
        "postcondiciones_exito": [
            "El rol seleccionado y todos sus descendientes quedan eliminados de NODO_PERMISO.",
            "Sus vínculos en ROL_PERMISO y USUARIO_PERFIL quedan eliminados en cascada.",
            "El árbol de roles se refresca.",
        ],
        "postcondiciones_fallo": [
            "Si el rol es protegido o tiene usuarios asignados (directa o indirectamente): no se elimina.",
        ],
        "disparador": "El administrador selecciona un rol en el árbol y presiona \"Eliminar\".",
        "flujo_principal": [
            "El administrador selecciona un rol en el árbol.",
            "El administrador presiona \"Eliminar\".",
            "El sistema solicita confirmación.",
            "El administrador confirma.",
            "El sistema verifica que el rol no sea protegido y que ningún descendiente tenga usuarios asignados.",
            "El sistema elimina el subárbol en una sola operación atómica (CTE recursiva).",
            "El sistema refresca el árbol de roles.",
        ],
        "flujos_alternativos": [
            {
                "id": "2a", "nombre": "El nodo seleccionado es un Permiso del catálogo (no un rol)",
                "pasos": [
                    "El sistema muestra \"Los permisos del catálogo no se pueden eliminar. Desasignalo usando los checkboxes\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "4a", "nombre": "El administrador cancela la confirmación",
                "pasos": [
                    "El sistema no elimina el rol.",
                ],
            },
            {
                "id": "5a", "nombre": "El rol es protegido",
                "pasos": [
                    "El botón Eliminar está deshabilitado para roles protegidos.",
                    "El caso de uso no puede ejecutarse sobre roles del sistema.",
                ],
            },
            {
                "id": "5b", "nombre": "Hay usuarios asignados en el subárbol",
                "pasos": [
                    "El sistema muestra \"No se puede eliminar un rol que tiene usuarios asignados. Reasignálos primero\".",
                    "El caso de uso termina sin eliminar.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Los roles marcados como protegidos (PROTEGIDO = 1) no pueden eliminarse — solo Administrador lo es por defecto."},
            {"codigo": "RN-02", "regla": "Antes de eliminar, se valida que ningún rol del subárbol tenga usuarios asignados (chequeo recursivo)."},
            {"codigo": "RN-03", "regla": "La eliminación borra el subárbol completo y limpia sus vínculos en ROL_PERMISO y USUARIO_PERFIL."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-14 ─────────────────────────────────────────────────────
    {
        "id": "CU-14",
        "nombre": "Cambiar Padre de Rol",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador reorganizar la jerarquía de roles cambiando el padre de un rol existente.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de roles\".",
            "El rol a mover existe.",
            "El padre candidato no es el propio rol ni ninguno de sus descendientes.",
            "El padre candidato no es un ancestro por encima del padre actual del rol.",
        ],
        "postcondiciones_exito": [
            "El rol queda con su nuevo PADRE_ID en ROL.",
            "El árbol se refresca y muestra la nueva jerarquía.",
        ],
        "postcondiciones_fallo": [
            "Si el padre elegido generaría un ciclo: no se actualiza y se muestra el error.",
            "Si el padre elegido es un ancestro por encima del padre actual: no se actualiza y se muestra el error.",
        ],
        "disparador": "El administrador selecciona un rol, elige un nuevo padre en el combo y presiona \"Asignar padre\".",
        "flujo_principal": [
            "El administrador selecciona un rol en el árbol.",
            "El sistema muestra el combo de padres con candidatos que excluyen el propio rol y su subárbol; los ancestros por encima de su padre actual se muestran pero deshabilitados.",
            "El administrador elige un padre (o \"(ninguno)\" para dejarlo como raíz).",
            "El administrador presiona \"Asignar padre\".",
            "El sistema verifica que la nueva relación no genere un ciclo.",
            "El sistema verifica que el nuevo padre no sea un ancestro por encima del padre actual del rol.",
            "El sistema actualiza el campo PADRE_ID del rol.",
            "El sistema refresca el árbol y mantiene la selección sobre el rol movido.",
        ],
        "flujos_alternativos": [
            {
                "id": "5a", "nombre": "La nueva relación generaría un ciclo",
                "pasos": [
                    "El sistema muestra \"No se puede asignar ese padre: generaría una referencia circular\".",
                    "El caso de uso termina sin modificar la jerarquía.",
                ],
            },
            {
                "id": "5b", "nombre": "El nuevo padre es un ancestro por encima del padre actual del rol",
                "pasos": [
                    "El sistema muestra \"No se puede asignar como padre a un ancestro del padre actual del rol\".",
                    "El caso de uso termina sin modificar la jerarquía.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El combo de padres excluye al propio rol y a todos sus descendientes."},
            {"codigo": "RN-02", "regla": "La validación de ciclos se hace en memoria recorriendo la cadena de padres hacia arriba."},
            {"codigo": "RN-03", "regla": "Asignar (ninguno) como padre hace al rol raíz del árbol."},
            {"codigo": "RN-04", "regla": "No se puede asignar como nuevo padre a un ancestro ubicado por encima del padre actual del rol; el padre actual sigue siendo una opción válida. El combo muestra esos ancestros pero deshabilitados (no seleccionables)."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-15 ─────────────────────────────────────────────────────
    {
        "id": "CU-15",
        "nombre": "Asignar Permisos a Rol",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "proposito": "Permitir al administrador definir qué permisos del catálogo tiene asociados un rol.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de roles\".",
            "El rol seleccionado existe.",
        ],
        "postcondiciones_exito": [
            "La tabla ROL_PERMISO queda con las asignaciones exactas indicadas por el administrador para ese rol.",
            "El árbol se refresca y los permisos asignados quedan visibles como hijos del rol.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un rol, marca/desmarca permisos del catálogo y presiona \"Guardar\".",
        "flujo_principal": [
            "El administrador selecciona un rol en el árbol.",
            "El sistema muestra la lista de permisos del catálogo, marcando con check los que el rol ya tiene.",
            "El administrador modifica las marcas y presiona \"Guardar\".",
            "El sistema reemplaza todas las asignaciones previas del rol por la nueva selección dentro de una transacción.",
            "El sistema refresca el árbol y mantiene la selección sobre el rol.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "El nodo seleccionado es un permiso (no un rol)",
                "pasos": [
                    "El panel de permisos no se muestra.",
                    "El caso de uso no puede ejecutarse sobre permisos del catálogo.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "El catálogo de permisos es estático: \"Administrar usuarios\", \"Gestión de roles\", \"Gestión de idiomas\", \"Ver bitácora\", \"Cambiar contraseña\". No se crean ni se eliminan desde la UI."},
            {"codigo": "RN-02", "regla": "Guardar reemplaza completamente la lista anterior — no es incremental."},
            {"codigo": "RN-03", "regla": "La operación se ejecuta en una transacción (LIMPIAR + N × INSERTAR) para garantizar consistencia."},
        ],
        "relaciones": [],
        "observaciones": (
            "Los permisos efectivos de un usuario se calculan combinando los roles asignados al "
            "usuario y los permisos asociados a cada rol vía ROL_PERMISO."
        ),
    },
    # ───── CU-16 ─────────────────────────────────────────────────────
    {
        "id": "CU-16",
        "nombre": "Crear Idioma",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador agregar un nuevo idioma al sistema.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de idiomas\".",
        ],
        "postcondiciones_exito": [
            "El nuevo idioma queda registrado en la tabla IDIOMA con Habilitado = false.",
            "El idioma aparece en la grilla de gestión.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador presiona \"Agregar idioma\".",
        "flujo_principal": [
            "El administrador presiona \"Agregar idioma\".",
            "El sistema solicita el nombre del nuevo idioma.",
            "El administrador ingresa el nombre y confirma.",
            "El sistema crea el idioma con Habilitado = false.",
            "El sistema refresca la grilla.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Nombre vacío",
                "pasos": [
                    "El sistema cancela la operación.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Un idioma recién creado nace deshabilitado: no aparece en el combo hasta que se lo habilite (ver CU-17) y se cargue al menos una traducción (CU-19)."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-17 ─────────────────────────────────────────────────────
    {
        "id": "CU-17",
        "nombre": "Editar Idioma (renombrar o habilitar/deshabilitar)",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador renombrar un idioma o alternar su estado habilitado/deshabilitado.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de idiomas\".",
            "Existe al menos un idioma en el sistema.",
        ],
        "postcondiciones_exito": [
            "El idioma queda con su nuevo nombre o nuevo estado en la tabla IDIOMA.",
            "Si se habilitó: el idioma aparece en los combos de selección de los formularios.",
            "Si se deshabilitó: deja de aparecer en los combos (pero sus traducciones se conservan).",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un idioma y presiona \"Renombrar\" o \"Habilitar/Deshabilitar\".",
        "flujo_principal": [
            "El administrador selecciona un idioma en la grilla.",
            "El administrador presiona la acción deseada (Renombrar o Habilitar/Deshabilitar).",
            "Si renombró: el sistema solicita el nuevo nombre. Si toggleó habilitado: aplica directamente.",
            "El sistema persiste el cambio y refresca la grilla.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún idioma seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un idioma\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "3a", "nombre": "El nuevo nombre es vacío o igual al actual",
                "pasos": [
                    "El sistema cancela la operación sin modificar el idioma.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Solo los idiomas habilitados aparecen en el selector de los formularios."},
            {"codigo": "RN-02", "regla": "Deshabilitar no elimina las traducciones — el idioma puede rehabilitarse sin pérdida."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-18 ─────────────────────────────────────────────────────
    {
        "id": "CU-18",
        "nombre": "Eliminar Idioma",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "proposito": "Permitir al administrador eliminar un idioma del sistema junto con todas sus traducciones.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de idiomas\".",
            "Existe al menos un idioma.",
        ],
        "postcondiciones_exito": [
            "El idioma y todas sus traducciones en CONTROL_IDIOMA quedan eliminados.",
            "Si era el idioma activo: el sistema vuelve al idioma por defecto (textos de diseño).",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona un idioma y presiona \"Eliminar\".",
        "flujo_principal": [
            "El administrador selecciona un idioma en la grilla.",
            "El administrador presiona \"Eliminar\".",
            "El sistema solicita confirmación, advirtiendo que se borrarán las traducciones.",
            "El administrador confirma.",
            "Si el idioma era el activo: el sistema lo desactiva en IdiomaManager (vuelve a textos por defecto).",
            "El sistema elimina el idioma y sus traducciones.",
            "El sistema refresca la grilla y limpia la grilla de traducciones.",
        ],
        "flujos_alternativos": [
            {
                "id": "1a", "nombre": "Ningún idioma seleccionado",
                "pasos": [
                    "El sistema muestra \"Seleccioná un idioma\".",
                    "El caso de uso termina.",
                ],
            },
            {
                "id": "4a", "nombre": "El administrador cancela la confirmación",
                "pasos": [
                    "El sistema no elimina el idioma.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Eliminar un idioma borra todas sus traducciones en CONTROL_IDIOMA — la operación no es reversible."},
            {"codigo": "RN-02", "regla": "Si el idioma a eliminar es el activo, primero se desactiva en IdiomaManager antes de borrar la BD."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-19 ─────────────────────────────────────────────────────
    {
        "id": "CU-19",
        "nombre": "Cargar o Editar Traducciones",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": "Permitir al administrador definir o actualizar el texto traducido de cada control para un idioma dado.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Gestión de idiomas\".",
            "Existe al menos un idioma.",
            "Los controles a traducir ya están registrados en la BD (por el script o por uso previo de la app).",
        ],
        "postcondiciones_exito": [
            "Las traducciones ingresadas quedan persistidas en CONTROL_IDIOMA para el idioma seleccionado.",
            "Si el idioma editado coincide con el activo: el sistema refresca la interfaz para que las nuevas traducciones se reflejen inmediatamente.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador edita la columna \"Traducción\" en la grilla y presiona \"Guardar traducciones\".",
        "flujo_principal": [
            "El administrador selecciona un idioma en la grilla superior.",
            "El sistema carga la lista de controles con sus traducciones actuales en la grilla inferior.",
            "El administrador edita la columna \"Traducción\" de las filas deseadas.",
            "El administrador presiona \"Guardar traducciones\".",
            "El sistema persiste cada fila con texto no vacío.",
            "El sistema muestra \"Traducciones guardadas\".",
            "Si el idioma editado es el activo: el sistema recarga el diccionario y notifica a los observadores.",
        ],
        "flujos_alternativos": [
            {
                "id": "3a", "nombre": "Una fila tiene la traducción vacía",
                "pasos": [
                    "El sistema omite esa fila y no la persiste.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Una traducción vacía no se guarda — se mantiene el texto por defecto del control como fallback."},
            {"codigo": "RN-02", "regla": "Los controles disponibles para traducción son aquellos registrados previamente en la BD vía CONTROL_REGISTRAR."},
            {"codigo": "RN-03", "regla": "Si se editan las traducciones del idioma activo, la UI se actualiza en tiempo real sin reiniciar el sistema."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
    # ───── CU-20 ─────────────────────────────────────────────────────
    {
        "id": "CU-20",
        "nombre": "Ver Bitácora",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": "Permitir al administrador consultar los eventos registrados en la bitácora del sistema, con filtros por usuario, acción y rango de fechas.",
        "precondiciones": [
            "El administrador inició sesión correctamente.",
            "El administrador tiene el permiso \"Ver bitácora\".",
        ],
        "postcondiciones_exito": [
            "Se muestra al administrador la lista de eventos que cumplen los filtros aplicados.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El administrador selecciona \"Bitácora\" en el menú principal.",
        "flujo_principal": [
            "El administrador selecciona \"Bitácora\".",
            "El sistema carga todas las entradas de la tabla BITACORA.",
            "El sistema puebla los combos de filtros de Usuario y Acción con los valores distintos encontrados.",
            "El sistema muestra todas las entradas en la grilla.",
            "El administrador opcionalmente ajusta filtros (usuario, acción, fecha desde, fecha hasta) y presiona \"Filtrar\".",
            "El sistema aplica los filtros y refresca la grilla.",
        ],
        "flujos_alternativos": [
            {
                "id": "5a", "nombre": "El administrador presiona \"Limpiar filtros\"",
                "pasos": [
                    "El sistema vuelve los filtros a su estado inicial y muestra todas las entradas.",
                ],
            },
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "La bitácora es solo de lectura desde la UI — no se pueden editar ni eliminar entradas."},
            {"codigo": "RN-02", "regla": "Los filtros por fecha son inclusivos en \"desde\" y exclusivos en el día siguiente al \"hasta\" (rango cerrado por día)."},
            {"codigo": "RN-03", "regla": "Los filtros de usuario y acción son por valor exacto, no por subcadena."},
        ],
        "relaciones": [],
        "observaciones": None,
    },
]


# ─────────────────────────────────────────────────────────────────────
# Render del .docx
# ─────────────────────────────────────────────────────────────────────

AZUL_OSCURO   = RGBColor(0x1F, 0x38, 0x64)
AZUL_CLARO_BG = "DAE3F3"
GRIS_BG       = "F2F2F2"


def set_cell_bg(cell, color_hex):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), color_hex)
    tc_pr.append(shd)


def configurar_estilos(doc):
    style = doc.styles['Normal']
    style.font.name = 'Calibri'
    style.font.size = Pt(11)

    for nivel, tam in [('Heading 1', 18), ('Heading 2', 14), ('Heading 3', 12)]:
        s = doc.styles[nivel]
        s.font.name  = 'Calibri'
        s.font.size  = Pt(tam)
        s.font.color.rgb = AZUL_OSCURO
        s.font.bold  = True


def agregar_tabla_resumen(doc, cu):
    tabla = doc.add_table(rows=0, cols=2)
    tabla.style = 'Light Grid Accent 1'

    filas = [
        ("Identificador",    cu["id"]),
        ("Nombre",           cu["nombre"]),
        ("Actor primario",   cu["actor_primario"]),
    ]
    if cu.get("actor_secundario"):
        filas.append(("Actor secundario", cu["actor_secundario"]))
    filas += [
        ("Frecuencia", cu["frecuencia"]),
        ("Prioridad",  cu["prioridad"]),
    ]

    for etiqueta, valor in filas:
        fila = tabla.add_row()
        fila.cells[0].text = etiqueta
        fila.cells[1].text = valor
        for run in fila.cells[0].paragraphs[0].runs:
            run.bold = True
        set_cell_bg(fila.cells[0], AZUL_CLARO_BG)

    doc.add_paragraph()


def agregar_seccion_lista(doc, titulo, items, numerada=False):
    if not items:
        return
    doc.add_heading(titulo, level=2)
    estilo = 'List Number' if numerada else 'List Bullet'
    for item in items:
        doc.add_paragraph(item, style=estilo)


def agregar_seccion_parrafo(doc, titulo, texto):
    if not texto:
        return
    doc.add_heading(titulo, level=2)
    doc.add_paragraph(texto)


def agregar_flujos_alternativos(doc, flujos):
    if not flujos:
        return
    doc.add_heading("Flujos alternativos", level=2)
    for f in flujos:
        doc.add_heading(f"{f['id']}. {f['nombre']}", level=3)
        for paso in f["pasos"]:
            doc.add_paragraph(paso, style='List Bullet')


def agregar_excepciones(doc, excepciones):
    if not excepciones:
        return
    doc.add_heading("Excepciones", level=2)
    tabla = doc.add_table(rows=1, cols=3)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    headers[0].text = "Código"
    headers[1].text = "Descripción"
    headers[2].text = "Manejo"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    for ex in excepciones:
        fila = tabla.add_row().cells
        fila[0].text = ex["codigo"]
        fila[1].text = ex["descripcion"]
        fila[2].text = ex["manejo"]
    doc.add_paragraph()


def agregar_reglas_negocio(doc, reglas):
    if not reglas:
        return
    doc.add_heading("Reglas de negocio", level=2)
    tabla = doc.add_table(rows=1, cols=2)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    headers[0].text = "Código"
    headers[1].text = "Regla"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    for r in reglas:
        fila = tabla.add_row().cells
        fila[0].text = r["codigo"]
        fila[1].text = r["regla"]
    doc.add_paragraph()


def agregar_relaciones(doc, relaciones):
    if not relaciones:
        return
    doc.add_heading("Relaciones con otros casos de uso", level=2)
    tabla = doc.add_table(rows=1, cols=3)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    headers[0].text = "Tipo"
    headers[1].text = "Destino"
    headers[2].text = "Condición"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    for r in relaciones:
        fila = tabla.add_row().cells
        fila[0].text = r["tipo"]
        fila[1].text = r["destino"]
        fila[2].text = r["condicion"]
    doc.add_paragraph()


def agregar_cu(doc, cu, es_primero=False):
    if not es_primero:
        doc.add_page_break()

    doc.add_heading(f"{cu['id']} — {cu['nombre']}", level=1)
    agregar_tabla_resumen(doc, cu)
    agregar_seccion_parrafo(doc, "Propósito", cu["proposito"])
    agregar_seccion_lista(doc, "Precondiciones", cu["precondiciones"])

    if cu.get("postcondiciones_exito"):
        doc.add_heading("Postcondiciones", level=2)
        doc.add_heading("En caso de éxito", level=3)
        for it in cu["postcondiciones_exito"]:
            doc.add_paragraph(it, style='List Bullet')
        if cu.get("postcondiciones_fallo"):
            doc.add_heading("En caso de fallo", level=3)
            for it in cu["postcondiciones_fallo"]:
                doc.add_paragraph(it, style='List Bullet')

    agregar_seccion_parrafo(doc, "Disparador", cu["disparador"])
    agregar_seccion_lista(doc, "Flujo principal", cu["flujo_principal"], numerada=True)
    agregar_flujos_alternativos(doc, cu.get("flujos_alternativos", []))
    agregar_excepciones(doc, cu.get("excepciones", []))
    agregar_reglas_negocio(doc, cu.get("reglas_negocio", []))
    agregar_relaciones(doc, cu.get("relaciones", []))
    agregar_seccion_parrafo(doc, "Observaciones", cu.get("observaciones"))


def generar_tabla_de_contenidos(doc):
    doc.add_heading("Casos de Uso", level=0)
    doc.add_paragraph(
        "Sistema de Gestión de Usuarios — TP Ingeniería de Software.\n"
        "Documento de descripción de los 20 casos de uso identificados, organizados "
        "por subsistema."
    )

    doc.add_heading("Actores", level=1)
    actores = doc.add_table(rows=1, cols=2)
    actores.style = 'Light Grid Accent 1'
    headers = actores.rows[0].cells
    headers[0].text = "Actor"
    headers[1].text = "Descripción"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    actores.add_row().cells[0].text, actores.rows[-1].cells[1].text = (
        "Usuario",
        "Persona registrada en el sistema. Puede iniciar sesión, cerrar sesión, cambiar su contraseña y cambiar el idioma de la interfaz.",
    )
    actores.add_row().cells[0].text, actores.rows[-1].cells[1].text = (
        "Administrador",
        "Especialización de Usuario. Tiene a su cargo la gestión de usuarios, roles, idiomas y la consulta de la bitácora, además de poder restaurar la integridad del sistema.",
    )

    doc.add_paragraph()
    doc.add_heading("Índice de casos de uso", level=1)
    tabla = doc.add_table(rows=1, cols=3)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    headers[0].text = "ID"
    headers[1].text = "Nombre"
    headers[2].text = "Actor primario"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    for cu in CUS:
        fila = tabla.add_row().cells
        fila[0].text = cu["id"]
        fila[1].text = cu["nombre"]
        fila[2].text = cu["actor_primario"]


def main():
    doc = Document()

    secciones = doc.sections
    for sec in secciones:
        sec.left_margin   = Cm(2.5)
        sec.right_margin  = Cm(2.5)
        sec.top_margin    = Cm(2.5)
        sec.bottom_margin = Cm(2.5)

    configurar_estilos(doc)
    generar_tabla_de_contenidos(doc)

    for i, cu in enumerate(CUS):
        agregar_cu(doc, cu, es_primero=False)

    salida = "CasosDeUso.docx"
    doc.save(salida)
    print(f"OK: {salida} ({len(CUS)} casos de uso)")


if __name__ == "__main__":
    main()
