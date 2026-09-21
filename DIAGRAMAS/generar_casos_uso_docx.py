# -*- coding: utf-8 -*-
"""
Genera CasosDeUso.docx con la descripción de los casos de uso del TP:
CU-01..CU-20 (esqueleto base) + CU-21..CU-25 y CU-27 (principales del
dominio de Gestión de Catálogo y Stock de Vinos, plantilla extendida)
+ CU-26 Consultar Alerta de Stock Mínimo (soporte, especificación
simple) + CU-28..CU-32 (dominio de Curación y Armado de Cajas
Mensuales, Club de Socios — CU-28/29 con plantilla extendida y
diagrama de secuencia, CU-30/31/32 con especificación simple).
"""

import os

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
    # ───────────────────────────────────────────────────────────────────
    # Dominio: Gestión de Catálogo y Stock de Vinos
    # (change: gestion-catalogo-stock-vinos — Entrega N01, análisis y diseño)
    # CU-21..CU-25 y CU-27 son los CU principales (plantilla extendida:
    # carátula, historial de revisión, puntos de extensión, gráfico del
    # CU, diagrama de clases afectadas, diagrama de secuencia, DER con
    # entidades afectadas y prototipo de interfaz). El soporte CU-26
    # "Consultar Alerta de Stock Mínimo" usa la plantilla simple (igual
    # que CU-01..CU-20). Split reconciliado el 31/08/2026 (design id 86):
    # el CU consolidado de alta se dividió en CU-21 (proponer) / CU-22
    # (autorizar); el de descontinuación en CU-24 (solicitar) / CU-25
    # (autorizar); y se agregó CU-27 (Registrar Ajuste de Inventario)
    # como caso independiente de CU-23 (Registrar Movimiento de Stock —
    # Entrada).
    # ───────────────────────────────────────────────────────────────────
    # ───── CU-21 ─────────────────────────────────────────────────────
    {
        "id": "CU-21",
        "nombre": "Proponer Alta de Vino",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial. Desdoblado de la versión consolidada previa (que fusionaba proponer y autorizar en un único CU) en dos casos de uso independientes: CU-21 (proponer) y CU-22 (autorizar), uno por actor."},
        ],
        "proposito": (
            "Permitir que un Usuario con el permiso \"Gestionar catálogo de vinos\" (rol Encargado de "
            "Compras/Bodega) proponga el alta de un vino nuevo en el catálogo, quedando pendiente de "
            "autorización hasta que un Administrador distinto lo apruebe (RN-01, ver CU-22)."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar catálogo de vinos\".",
            "La bodega del vino ya existe en BODEGA y está habilitada.",
            "El código/SKU del vino no está registrado previamente en VINO.",
        ],
        "postcondiciones_exito": [
            "El vino queda registrado en VINO con AUTORIZADO_POR = NULL y CREADO_POR = Id del Usuario, pendiente de autorización.",
        ],
        "postcondiciones_fallo": [
            "Si el código ya existe: no se crea el vino y se informa el conflicto.",
        ],
        "disparador": "El Usuario presiona \"Nuevo vino\" en frmCatalogoVinos.",
        "puntos_extension": [
            {"paso": "Tras el paso 7 (alta pendiente registrada)",
             "extension": "Continúa en CU-22 Autorizar Alta de Vino, ejecutado por un Administrador distinto."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Gestionar catálogo de vinos\") ──> (Proponer Alta de Vino)\n\n"
            "(Proponer Alta de Vino) ── «precede» ──> (Autorizar Alta de Vino, CU-22)"
        ),
        "flujo_principal": [
            "El Usuario abre frmCatalogoVinos y presiona \"Nuevo vino\".",
            "El sistema abre frmNuevoVino y carga el combo de bodegas habilitadas.",
            "El Usuario ingresa código/SKU, nombre, bodega, varietal, añada, precio y stock mínimo (maridaje y puntaje son opcionales) y presiona Aceptar.",
            "El sistema valida que los campos obligatorios estén completos.",
            "El sistema verifica que el código no exista previamente en VINO.",
            "El sistema inserta el vino con AUTORIZADO_POR = NULL y CREADO_POR = Id del Usuario.",
            "El sistema informa que el vino quedó pendiente de autorización.",
        ],
        "flujos_alternativos": [
            {"id": "4a", "nombre": "Datos obligatorios incompletos",
             "pasos": ["El sistema muestra \"Completá los datos obligatorios del vino.\"",
                       "El caso de uso vuelve al paso 3."]},
            {"id": "5a", "nombre": "Código de vino duplicado",
             "pasos": ["El sistema muestra \"Ya existe un vino con ese código.\"",
                       "El caso de uso vuelve al paso 3."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante el alta.",
             "manejo": "Mensaje genérico al Usuario; no se persiste ningún cambio parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-02", "regla": "El vino no es visible ni vendible en el catálogo hasta que AUTORIZADO_POR IS NOT NULL y ESTADO = 'Activo'."},
            {"codigo": "RN-03", "regla": "El código/SKU del vino es único en todo el catálogo."},
        ],
        "relaciones": [
            {"tipo": "«precede» a", "destino": "CU-22 Autorizar Alta de Vino",
             "condicion": "Toda propuesta de alta requiere autorización posterior de un Administrador distinto."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU21_ProponerAltaVino.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "BODEGA", "USUARIO"],
        "prototipo_interfaz": (
            "frmNuevoVino (WinForms — MaterialForm)\n"
            "+-------------------------------------------+\n"
            "|  Nuevo Vino                          [x]   |\n"
            "+---------------------------------------------+\n"
            "| Codigo/SKU:      [____________]              |\n"
            "| Nombre:          [____________]              |\n"
            "| Bodega:          [ ComboBox  v]              |\n"
            "| Varietal:        [____________]              |\n"
            "| Aniada:          [____]                      |\n"
            "| Precio:          [____________]              |\n"
            "| Stock minimo:    [____]                      |\n"
            "| Maridaje:        [____________] (opcional)   |\n"
            "| Puntaje:         [____] (opcional)           |\n"
            "|                                               |\n"
            "|              [ Aceptar ]  [ Cancelar ]        |\n"
            "+-----------------------------------------------+"
        ),
        "observaciones": (
            "El vino nunca se elimina físicamente en ningún momento de su ciclo de vida (RN-04, "
            "ver CU-24/CU-25) — solo cambia de estado. La separación de funciones (RN-01: quien "
            "autoriza debe ser distinto de quien propone) se valida en CU-22."
        ),
    },
    # ───── CU-22 ─────────────────────────────────────────────────────
    {
        "id": "CU-22",
        "nombre": "Autorizar Alta de Vino",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial. Desdoblado de la versión consolidada previa en dos casos de uso independientes: CU-21 (proponer) y CU-22 (autorizar), uno por actor."},
        ],
        "proposito": (
            "Permitir que un Administrador, con el permiso \"Autorizar catálogo de vinos\" y distinto "
            "del Usuario que propuso el alta, apruebe la publicación de un vino pendiente — "
            "garantizando la separación de funciones (RN-01)."
        ),
        "precondiciones": [
            "El Administrador inició sesión correctamente y tiene el permiso \"Autorizar catálogo de vinos\".",
            "Existe al menos un vino con AUTORIZADO_POR = NULL (propuesto en CU-21).",
        ],
        "postcondiciones_exito": [
            "AUTORIZADO_POR y FECHA_AUTORIZACION quedan completos y el vino es visible en el catálogo publicado.",
        ],
        "postcondiciones_fallo": [
            "Si el Administrador que intenta autorizar es el mismo Usuario que propuso el alta: el sistema rechaza la operación (RN-01) y el vino permanece pendiente.",
        ],
        "disparador": "El Administrador presiona \"Autorizar\" sobre un vino pendiente en frmAutorizarVinos.",
        "puntos_extension": [
            {"paso": "Tras el paso 5 (vino autorizado y publicado)",
             "extension": "Notificación automática al Usuario que propuso el alta — fuera de alcance de esta entrega (la alerta/notificación es pasiva, no push)."},
        ],
        "grafico_cu_desc": (
            "Actor: Administrador (permiso \"Autorizar catálogo de vinos\") ──> (Autorizar Alta de Vino)\n\n"
            "(Autorizar Alta de Vino) ── «precedido por» ──> (Proponer Alta de Vino, CU-21)"
        ),
        "flujo_principal": [
            "El Administrador abre frmAutorizarVinos y visualiza los vinos pendientes (AUTORIZADO_POR IS NULL).",
            "El Administrador selecciona un vino y presiona \"Autorizar\".",
            "El sistema verifica que el Administrador sea distinto del Usuario que propuso el alta (RN-01).",
            "El sistema completa AUTORIZADO_POR y FECHA_AUTORIZACION.",
            "El vino queda visible en el catálogo publicado.",
            "El sistema refresca la grilla de vinos pendientes.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "Autoautorización bloqueada (RN-01)",
             "pasos": ["El sistema muestra \"No podés autorizar un vino que vos mismo diste de alta.\"",
                       "El caso de uso vuelve al paso 2."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante la autorización.",
             "manejo": "Mensaje genérico al Administrador; no se persiste ningún cambio parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Separación de funciones: AUTORIZADO_POR nunca puede ser igual a CREADO_POR."},
            {"codigo": "RN-02", "regla": "El vino no es visible ni vendible en el catálogo hasta que AUTORIZADO_POR IS NOT NULL y ESTADO = 'Activo'."},
        ],
        "relaciones": [
            {"tipo": "«precedido por»", "destino": "CU-21 Proponer Alta de Vino",
             "condicion": "Solo se pueden autorizar vinos previamente propuestos y pendientes."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU22_AutorizarAltaVino.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "USUARIO"],
        "prototipo_interfaz": (
            "frmAutorizarVinos — pestaña \"Altas pendientes\"\n"
            "+-------------------------------------------------+\n"
            "|  Vinos pendientes de autorizacion         [x]    |\n"
            "+---------------------------------------------------+\n"
            "| [DataGridView: Codigo | Nombre | Bodega |          |\n"
            "|  Cargado por | Fecha alta]                         |\n"
            "|                                                     |\n"
            "|              [ Autorizar ]  [ Rechazar ]           |\n"
            "+-----------------------------------------------------+"
        ),
        "observaciones": "Simétrico a CU-25 (Autorizar Descontinuación de Vino): mismo patrón de autorización con separación de funciones, aplicado al extremo opuesto del ciclo de vida del vino.",
    },
    # ───── CU-23 ─────────────────────────────────────────────────────
    {
        "id": "CU-23",
        "nombre": "Registrar Movimiento de Stock (Entrada)",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Alta",
        "prioridad": "Alta",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial — Entrega 1 (análisis y diseño). Corresponde a la entrada de stock por compra a bodega/proveedor; los ajustes manuales (rotura, merma, corrección de conteo) se modelan aparte en CU-27 Registrar Ajuste de Inventario."},
        ],
        "proposito": (
            "Registrar la entrada de stock de un vino por compra a bodega/proveedor como un "
            "movimiento inmutable de kardex, del cual el stock actual es un valor derivado y "
            "reconciliable (RN-02 del dominio)."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar catálogo de vinos\".",
            "El vino sobre el que se registra el movimiento existe en VINO.",
        ],
        "postcondiciones_exito": [
            "Se crea un registro en MOVIMIENTO_STOCK con tipo Entrada (fecha, cantidad, motivo, responsable).",
            "El stock actual derivado del vino queda actualizado (SUM(Entrada) - SUM(Salida)).",
        ],
        "postcondiciones_fallo": [
            "Si la cantidad ingresada es menor o igual a cero: el movimiento se rechaza y no se persiste.",
        ],
        "disparador": "El Usuario abre frmMovimientoStock, selecciona un vino y registra una entrada.",
        "puntos_extension": [
            {"paso": "Tras cualquier movimiento registrado",
             "extension": "Recalcular y notificar si el vino queda por debajo de STOCK_MINIMO — la notificación activa es fuera de alcance; ver CU-26 Consultar Alerta de Stock Mínimo (consulta pasiva)."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Gestionar catálogo de vinos\")\n"
            "  ──> (Registrar Movimiento de Stock — Entrada)"
        ),
        "flujo_principal": [
            "El Usuario abre frmMovimientoStock y selecciona un vino.",
            "El sistema calcula y muestra el stock actual (derivado del kardex).",
            "El Usuario ingresa la cantidad recibida y el motivo (compra a bodega/proveedor) y presiona Registrar.",
            "El sistema valida que la cantidad sea mayor a cero.",
            "El sistema inserta el movimiento en MOVIMIENTO_STOCK con tipo Entrada.",
            "El sistema recalcula el stock actual y refresca la pantalla.",
        ],
        "flujos_alternativos": [
            {"id": "4a", "nombre": "Cantidad menor o igual a cero",
             "pasos": ["El sistema muestra \"La cantidad debe ser mayor a cero.\"",
                       "El caso de uso vuelve al paso 3."]},
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "MOVIMIENTO_STOCK es append-only: no se permiten UPDATE ni DELETE sobre movimientos ya registrados."},
            {"codigo": "RN-02", "regla": "El stock actual de un vino nunca se almacena directamente — se deriva de SUM(Entrada) - SUM(Salida) del kardex."},
        ],
        "relaciones": [
            {"tipo": "«extend» de", "destino": "CU-22 Autorizar Alta de Vino",
             "condicion": "Un vino recién autorizado y publicado suele recibir su primera entrada de stock."},
            {"tipo": "relacionado con", "destino": "CU-27 Registrar Ajuste de Inventario",
             "condicion": "Ambos insertan MOVIMIENTO_STOCK, pero CU-23 registra entradas por compra y CU-27 registra correcciones manuales (rotura, merma, conteo)."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU23_RegistrarMovimientoStock.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "MOVIMIENTO_STOCK"],
        "prototipo_interfaz": (
            "frmMovimientoStock (WinForms — MaterialForm)\n"
            "+-----------------------------------------------------+\n"
            "|  Registrar Entrada de Stock                   [x]    |\n"
            "+-------------------------------------------------------+\n"
            "| Vino:            [ ComboBox  v]                        |\n"
            "| Stock actual:    123 unidades  (solo lectura)           |\n"
            "|                                                         |\n"
            "| Cantidad recibida: [____]                              |\n"
            "| Motivo:          [____________] (compra a bodega)      |\n"
            "| Referencia:      [____________] (opcional)             |\n"
            "|                                                         |\n"
            "|              [ Registrar ]  [ Cancelar ]               |\n"
            "+---------------------------------------------------------+"
        ),
        "observaciones": (
            "RESPONSABLE se guarda como snapshot del login (VARCHAR), sin FK a USUARIO — mismo "
            "criterio que USUARIO_HISTORIAL: el registro histórico sobrevive a la baja del usuario."
        ),
    },
    # ───── CU-24 ─────────────────────────────────────────────────────
    {
        "id": "CU-24",
        "nombre": "Solicitar Descontinuación de Vino",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial. Desdoblado de la versión consolidada previa en dos casos de uso independientes: CU-24 (solicitar) y CU-25 (autorizar), simétrico al split CU-21/CU-22."},
        ],
        "proposito": (
            "Permitir que un Usuario proponga la descontinuación de un vino del catálogo, quedando "
            "pendiente de autorización hasta que un Administrador distinto la apruebe (RN-01, ver "
            "CU-25), sin eliminar físicamente el registro (RN-04)."
        ),
        "precondiciones": [
            "El vino a descontinuar existe, está Activo y autorizado en el catálogo.",
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar catálogo de vinos\".",
        ],
        "postcondiciones_exito": [
            "El vino queda con BAJA_SOLICITADA_POR y FECHA_SOLICITUD_BAJA completos, pendiente de autorización.",
        ],
        "postcondiciones_fallo": [
            "Si el vino ya está descontinuado o ya tiene una solicitud de baja pendiente: el sistema rechaza la operación.",
        ],
        "disparador": "El Usuario selecciona un vino activo en frmCatalogoVinos y presiona \"Solicitar descontinuación\".",
        "puntos_extension": [
            {"paso": "Tras el paso 3 (solicitud registrada)",
             "extension": "Continúa en CU-25 Autorizar Descontinuación de Vino, ejecutado por un Administrador distinto."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Gestionar catálogo de vinos\") ──> (Solicitar Descontinuación de Vino)\n\n"
            "(Solicitar Descontinuación de Vino) ── «precede» ──> (Autorizar Descontinuación de Vino, CU-25)"
        ),
        "flujo_principal": [
            "El Usuario selecciona un vino activo en frmCatalogoVinos y presiona \"Solicitar descontinuación\".",
            "El sistema registra BAJA_SOLICITADA_POR = Id del Usuario y FECHA_SOLICITUD_BAJA = fecha actual.",
            "El sistema informa que la descontinuación quedó pendiente de autorización.",
        ],
        "flujos_alternativos": [
            {"id": "1a", "nombre": "El vino ya está descontinuado o con baja ya solicitada",
             "pasos": ["El sistema muestra \"Este vino ya no está activo o ya tiene una solicitud de baja pendiente.\"",
                       "El caso de uso finaliza sin registrar cambios."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante la solicitud.",
             "manejo": "Mensaje genérico al Usuario; no se persiste ningún cambio parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-04", "regla": "El vino nunca se elimina físicamente; la descontinuación es siempre un cambio de ESTADO, preservando integridad referencial con movimientos de stock históricos."},
        ],
        "relaciones": [
            {"tipo": "«precede» a", "destino": "CU-25 Autorizar Descontinuación de Vino",
             "condicion": "Toda solicitud de baja requiere autorización posterior de un Administrador distinto."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU24_SolicitarDescontinuacion.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "USUARIO"],
        "prototipo_interfaz": (
            "frmCatalogoVinos (WinForms — MaterialForm)\n"
            "+-------------------------------------------------------+\n"
            "|  Catalogo de Vinos                                [x]  |\n"
            "+---------------------------------------------------------+\n"
            "| [DataGridView: Codigo | Nombre | Bodega | Estado |       |\n"
            "|  Autorizado por | Fecha alta]                            |\n"
            "|                                                           |\n"
            "|  [ Nuevo vino ]  [ Solicitar descontinuacion ]           |\n"
            "+-------------------------------------------------------------+"
        ),
        "observaciones": "Simétrico a CU-21 (Proponer Alta de Vino): mismo patrón de propone/autoriza, aplicado al extremo opuesto del ciclo de vida del vino.",
    },
    # ───── CU-25 ─────────────────────────────────────────────────────
    {
        "id": "CU-25",
        "nombre": "Autorizar Descontinuación de Vino",
        "actor_primario": "Administrador",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial. Desdoblado de la versión consolidada previa en dos casos de uso independientes: CU-24 (solicitar) y CU-25 (autorizar), simétrico al split CU-21/CU-22."},
        ],
        "proposito": (
            "Permitir que un Administrador, con el permiso \"Autorizar catálogo de vinos\" y distinto "
            "del Usuario que solicitó la baja, autorice la descontinuación de un vino — garantizando "
            "la separación de funciones (RN-01), sin eliminar físicamente el registro (RN-04)."
        ),
        "precondiciones": [
            "El Administrador inició sesión correctamente y tiene el permiso \"Autorizar catálogo de vinos\".",
            "Existe al menos un vino con BAJA_SOLICITADA_POR IS NOT NULL y DESCONTINUADO_POR IS NULL (solicitado en CU-24).",
        ],
        "postcondiciones_exito": [
            "ESTADO pasa a 'Descontinuado', y DESCONTINUADO_POR / FECHA_DESCONTINUACION quedan completos.",
            "El vino deja de listarse en el catálogo vendible, pero el registro nunca se elimina físicamente (RN-04).",
        ],
        "postcondiciones_fallo": [
            "Si el Administrador que intenta autorizar es el mismo Usuario que solicitó la baja: el sistema rechaza la operación (RN-01) y el vino permanece Activo.",
        ],
        "disparador": "El Administrador presiona \"Autorizar baja\" sobre una solicitud pendiente en frmAutorizarVinos.",
        "puntos_extension": [
            {"paso": "Tras el paso 5 (vino descontinuado)",
             "extension": "Notificación al Usuario que solicitó la baja — fuera de alcance de esta entrega."},
        ],
        "grafico_cu_desc": (
            "Actor: Administrador (permiso \"Autorizar catálogo de vinos\") ──> (Autorizar Descontinuación de Vino)\n\n"
            "(Autorizar Descontinuación de Vino) ── «precedido por» ──> (Solicitar Descontinuación de Vino, CU-24)"
        ),
        "flujo_principal": [
            "El Administrador abre frmAutorizarVinos y visualiza las solicitudes pendientes (BAJA_SOLICITADA_POR IS NOT NULL AND DESCONTINUADO_POR IS NULL).",
            "El Administrador selecciona un vino y presiona \"Autorizar baja\".",
            "El sistema verifica que el Administrador sea distinto del Usuario que solicitó la baja (RN-01).",
            "El sistema actualiza ESTADO a 'Descontinuado' y completa DESCONTINUADO_POR y FECHA_DESCONTINUACION.",
            "El vino deja de listarse en el catálogo vendible.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "Autoautorización bloqueada (RN-01)",
             "pasos": ["El sistema muestra \"No podés autorizar una descontinuación que vos mismo solicitaste.\"",
                       "El caso de uso vuelve al paso 2."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante la autorización.",
             "manejo": "Mensaje genérico al Administrador; no se persiste ningún cambio parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "Separación de funciones: DESCONTINUADO_POR nunca puede ser igual a BAJA_SOLICITADA_POR."},
            {"codigo": "RN-04", "regla": "El vino nunca se elimina físicamente; la descontinuación es siempre un cambio de ESTADO, preservando integridad referencial con movimientos de stock históricos."},
        ],
        "relaciones": [
            {"tipo": "«precedido por»", "destino": "CU-24 Solicitar Descontinuación de Vino",
             "condicion": "Solo se pueden autorizar bajas previamente solicitadas y pendientes."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU25_AutorizarDescontinuacion.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "USUARIO"],
        "prototipo_interfaz": (
            "frmAutorizarVinos — pestaña \"Descontinuaciones pendientes\"\n"
            "+---------------------------------------------------------+\n"
            "| [DataGridView: Codigo | Nombre | Solicitado por |         |\n"
            "|  Fecha solicitud]                                         |\n"
            "|                                                            |\n"
            "|              [ Autorizar baja ]  [ Rechazar ]            |\n"
            "+----------------------------------------------------------+"
        ),
        "observaciones": "Simétrico a CU-22 (Autorizar Alta de Vino): mismo patrón de autorización con separación de funciones, aplicado al extremo opuesto del ciclo de vida del vino.",
    },
    # ───── CU-26 (soporte, especificación simple) ──────────────────
    {
        "id": "CU-26",
        "nombre": "Consultar Alerta de Stock Mínimo",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": (
            "Permitir que el Usuario con el permiso \"Gestionar catálogo de vinos\" consulte, bajo "
            "demanda, los vinos cuyo stock actual esté por debajo de su STOCK_MINIMO configurado. "
            "Es una consulta pasiva (Decisión D9 del diseño): no hay un push automático al iniciar "
            "sesión, dado que la aplicación es WinForms de escritorio sin scheduler."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar catálogo de vinos\".",
        ],
        "postcondiciones_exito": [
            "Se muestra al Usuario la lista de vinos con stock actual menor a su STOCK_MINIMO.",
        ],
        "postcondiciones_fallo": [],
        "disparador": "El Usuario selecciona \"Alerta de stock mínimo\" en el menú principal.",
        "flujo_principal": [
            "El Usuario abre frmAlertaStockMinimo.",
            "El sistema calcula el stock actual de cada vino (derivado del kardex de MOVIMIENTO_STOCK).",
            "El sistema filtra los vinos cuyo stock actual es menor a su STOCK_MINIMO.",
            "El sistema muestra la lista de vinos pendientes de reposición.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "Ningún vino está por debajo de su stock mínimo",
             "pasos": ["El sistema muestra la grilla vacía con el mensaje \"No hay vinos con stock bajo mínimo.\""]},
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "La alerta es una consulta pasiva bajo demanda, no una notificación activa al iniciar sesión."},
        ],
        "relaciones": [
            {"tipo": "«extend» de", "destino": "CU-23 Registrar Movimiento de Stock (Entrada)",
             "condicion": "Se consulta habitualmente después de registrar movimientos de stock."},
            {"tipo": "«extend» de", "destino": "CU-27 Registrar Ajuste de Inventario",
             "condicion": "Se consulta habitualmente después de registrar ajustes que reducen stock."},
        ],
        "observaciones": "Especificación simple — no requiere diagrama de secuencia completo ni DER dedicado (ver diseño: solo los CU principales llevan modelado completo).",
    },
    # ───── CU-27 ─────────────────────────────────────────────────────
    {
        "id": "CU-27",
        "nombre": "Registrar Ajuste de Inventario",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Baja",
        "prioridad": "Media",
        "version": "1.0",
        "fecha_creacion": "31/08/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "31/08/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial — Entrega 1 (análisis y diseño). Cubre correcciones manuales de stock (rotura, merma, corrección de conteo) que no son entradas por compra (CU-23), en sentido positivo o negativo."},
        ],
        "proposito": (
            "Registrar una corrección manual de stock de un vino (rotura, merma, corrección de "
            "conteo físico) como un movimiento inmutable de kardex, en sentido positivo o negativo, "
            "respetando el bloqueo por stock cero (RN-03) en el sentido negativo."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar catálogo de vinos\".",
            "El vino sobre el que se registra el ajuste existe en VINO.",
        ],
        "postcondiciones_exito": [
            "Se crea un registro en MOVIMIENTO_STOCK (tipo Entrada o Salida según el sentido del ajuste, cantidad, motivo, responsable).",
            "El stock actual derivado del vino queda actualizado (SUM(Entrada) - SUM(Salida)).",
        ],
        "postcondiciones_fallo": [
            "Si el ajuste es de sentido negativo y dejaría el stock resultante por debajo de cero: el movimiento se rechaza y no se persiste (RN-03).",
        ],
        "disparador": "El Usuario abre frmMovimientoStock, selecciona un vino y registra un ajuste de inventario.",
        "puntos_extension": [
            {"paso": "Tras cualquier ajuste registrado",
             "extension": "Recalcular y notificar si el vino queda por debajo de STOCK_MINIMO — la notificación activa es fuera de alcance; ver CU-26 Consultar Alerta de Stock Mínimo (consulta pasiva)."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Gestionar catálogo de vinos\")\n"
            "  ──> (Registrar Ajuste de Inventario)\n\n"
            "(Registrar Ajuste de Inventario) incluye dos variantes internas:\n"
            "  - Ajuste positivo (corrección de conteo a favor)\n"
            "  - Ajuste negativo (rotura, merma), con rechazo si el stock resultante es negativo"
        ),
        "flujo_principal": [
            "El Usuario abre frmMovimientoStock y selecciona un vino.",
            "El sistema calcula y muestra el stock actual (derivado del kardex).",
            "El Usuario indica el sentido del ajuste (positivo o negativo), la cantidad y el motivo (rotura, merma, corrección de conteo) y presiona Registrar.",
            "Si el sentido es positivo, el sistema inserta el movimiento en MOVIMIENTO_STOCK con tipo Entrada.",
            "El sistema recalcula el stock actual y refresca la pantalla.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "Ajuste de sentido negativo",
             "pasos": [
                 "El sistema calcula el stock resultante (stock actual - cantidad).",
                 "Si el resultado es negativo (RN-03), el sistema rechaza el ajuste y muestra \"No hay stock suficiente para este ajuste.\"; no se persiste ningún registro.",
                 "Si el resultado es válido, el sistema inserta el movimiento con tipo Salida, recalcula el stock actual y refresca la pantalla.",
             ]},
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-01", "regla": "MOVIMIENTO_STOCK es append-only: no se permiten UPDATE ni DELETE sobre movimientos ya registrados."},
            {"codigo": "RN-02", "regla": "El stock actual de un vino nunca se almacena directamente — se deriva de SUM(Entrada) - SUM(Salida) del kardex."},
            {"codigo": "RN-03", "regla": "Todo ajuste negativo que dejaría el stock resultante por debajo de cero se rechaza; no hay reservas ni backorder en esta fase."},
        ],
        "relaciones": [
            {"tipo": "relacionado con", "destino": "CU-23 Registrar Movimiento de Stock (Entrada)",
             "condicion": "Ambos insertan MOVIMIENTO_STOCK, pero CU-23 registra entradas por compra y CU-27 registra correcciones manuales (rotura, merma, conteo)."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CatalogoStock.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU27_RegistrarAjusteInventario.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["VINO", "MOVIMIENTO_STOCK"],
        "prototipo_interfaz": (
            "frmMovimientoStock (WinForms — MaterialForm)\n"
            "+-----------------------------------------------------+\n"
            "|  Registrar Ajuste de Inventario               [x]    |\n"
            "+-------------------------------------------------------+\n"
            "| Vino:            [ ComboBox  v]                        |\n"
            "| Stock actual:    123 unidades  (solo lectura)           |\n"
            "|                                                         |\n"
            "| Sentido:         (o) Positivo   ( ) Negativo            |\n"
            "| Cantidad:        [____]                                |\n"
            "| Motivo:          [____________] (rotura/merma/conteo)  |\n"
            "|                                                         |\n"
            "|              [ Registrar ]  [ Cancelar ]               |\n"
            "+---------------------------------------------------------+"
        ),
        "observaciones": (
            "RESPONSABLE se guarda como snapshot del login (VARCHAR), sin FK a USUARIO — mismo "
            "criterio que USUARIO_HISTORIAL: el registro histórico sobrevive a la baja del usuario."
        ),
    },
    # ───── CU-28 ─────────────────────────────────────────────────────
    {
        "id": "CU-28",
        "nombre": "Armar Caja Mensual",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Alta",
        "prioridad": "Alta",
        "version": "1.0",
        "fecha_creacion": "18/09/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "18/09/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial — Entrega 2 (análisis y diseño). Dominio de curación y armado de cajas mensuales, Club de Socios."},
        ],
        "proposito": (
            "Permitir que un Usuario con el permiso \"Armar cajas mensuales\" (rol Curador de "
            "Producto) arme una caja mensual para un socio y un período, con líneas tomadas solo "
            "del catálogo gobernado (RN-06) y sin exceder el presupuesto mensual del socio (RN-05)."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Armar cajas mensuales\".",
            "Existe al menos un socio activo con perfil vigente (presupuesto mensual cargado).",
            "Existe al menos un vino que cumple RN-06 (Activo, autorizado, con stock derivado > 0).",
        ],
        "postcondiciones_exito": [
            "Se crea una CAJA_MENSUAL en estado Armada, con presupuesto congelado (PRESUPUESTO_SNAPSHOT).",
            "Cada línea CAJA_VINO queda con NOMBRE_SNAPSHOT y PRECIO_SNAPSHOT congelados (RN-04).",
        ],
        "postcondiciones_fallo": [
            "Si la suma de las líneas supera el presupuesto del socio: no se persiste ninguna línea ni la caja.",
            "Si el socio ya tiene una caja no cancelada para ese período: se rechaza el armado.",
            "Si una línea dejó de cumplir RN-06 durante la selección: se rechaza el armado.",
        ],
        "disparador": "El Usuario presiona \"Armar caja mensual\" en frmArmarCaja.",
        "puntos_extension": [
            {"paso": "Tras el paso 8 (caja armada)",
             "extension": "Continúa en CU-29 Registrar Sustitución por Falta de Stock (si el picking detecta faltantes) y en CU-31 Despachar Caja."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Armar cajas mensuales\") ──> (Armar Caja Mensual)\n\n"
            "(Armar Caja Mensual) ── «precede» ──> (Registrar Sustitución, CU-29)\n"
            "(Armar Caja Mensual) ── «precede» ──> (Despachar Caja, CU-31)"
        ),
        "flujo_principal": [
            "El Usuario abre \"Armar caja mensual\" en frmArmarCaja.",
            "El sistema lista los socios activos.",
            "El Usuario selecciona un socio y un período (formato YYYY-MM).",
            "El sistema lista los vinos candidatos que cumplen RN-06, ordenados con los varietales preferidos del socio primero (RN-05.2, ranking, no filtro).",
            "El sistema muestra el presupuesto mensual del socio y el total de la caja en $0.",
            "El Usuario agrega vinos con su cantidad; el sistema recalcula el total en cada línea y deshabilita \"Confirmar\" si se supera el presupuesto.",
            "El Usuario presiona \"Confirmar armado\".",
            "El sistema valida el total contra el presupuesto (RN-05.1), la unicidad del período (RN-08) y que cada línea siga cumpliendo RN-06, e inserta la caja y sus líneas en una única transacción, dejándola en estado Armada.",
            "El sistema informa que la caja quedó armada.",
        ],
        "flujos_alternativos": [
            {"id": "8a", "nombre": "El total supera el presupuesto mensual (RN-05.1)",
             "pasos": ["El sistema muestra \"El total de la caja supera el presupuesto mensual del socio.\"",
                       "El caso de uso vuelve al paso 6."]},
            {"id": "8b", "nombre": "Ya existe una caja no cancelada del socio para el período (RN-08)",
             "pasos": ["El sistema muestra \"El socio ya tiene una caja armada para ese período.\"",
                       "El caso de uso vuelve al paso 3."]},
            {"id": "8c", "nombre": "Una línea dejó de cumplir RN-06 durante la selección",
             "pasos": ["El sistema muestra \"El vino seleccionado no está disponible para armado.\"",
                       "El caso de uso vuelve al paso 6."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante el armado.",
             "manejo": "Mensaje genérico al Usuario; la transacción se revierte, no queda ninguna línea parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-04", "regla": "Cada línea de caja congela NOMBRE y PRECIO del vino al momento del armado (snapshot)."},
            {"codigo": "RN-05", "regla": "La suma de las líneas no puede exceder el presupuesto del socio (tope duro, RN-05.1); los varietales preferidos solo ordenan candidatos, nunca excluyen (RN-05.2)."},
            {"codigo": "RN-06", "regla": "Solo pueden incluirse vinos Activos, autorizados y con stock derivado mayor a cero (catálogo gobernado)."},
            {"codigo": "RN-08", "regla": "Un socio no puede tener más de una caja no cancelada por período."},
            {"codigo": "RN-09", "regla": "Armar una caja no reserva ni descuenta stock; la salida se registra recién al despachar (ver CU-31)."},
        ],
        "relaciones": [
            {"tipo": "«precede» a", "destino": "CU-29 Registrar Sustitución por Falta de Stock",
             "condicion": "Solo se pueden registrar sustituciones sobre cajas previamente armadas."},
            {"tipo": "«precede» a", "destino": "CU-31 Despachar Caja",
             "condicion": "Solo se pueden despachar cajas previamente armadas."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CuracionCajas.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU28_ArmarCajaMensual.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["SOCIO", "CAJA_MENSUAL", "CAJA_VINO", "VINO"],
        "prototipo_interfaz": (
            "frmArmarCaja (WinForms — MaterialForm)\n"
            "+-----------------------------------------------------+\n"
            "|  Armar Caja Mensual                            [x]   |\n"
            "+-------------------------------------------------------+\n"
            "| Socio:        [ ComboBox  v]   Periodo: [ 2026-10 ]   |\n"
            "| Presupuesto:  $ 15000.00        Total:  $ 0.00        |\n"
            "| [DataGridView: Vino | Precio | Preferido | Cantidad]  |\n"
            "|                                                       |\n"
            "|              [ Confirmar armado ]  [ Cancelar ]       |\n"
            "+---------------------------------------------------------+"
        ),
        "observaciones": (
            "RN-09 marca explícitamente que armar la caja no genera ningún MOVIMIENTO_STOCK — la "
            "primera salida de kardex de este dominio ocurre recién en el despacho (CU-31). El "
            "diseño también define una SP CAJA_CANCELAR y un método CajaMensualBLL.Cancelar para "
            "el estado Cancelada (RN-08), sin un CU dedicado en esta entrega: la cancelación de una "
            "caja mal armada queda como operación de soporte sin flujo propio en N02."
        ),
    },
    # ───── CU-29 ─────────────────────────────────────────────────────
    {
        "id": "CU-29",
        "nombre": "Registrar Sustitución por Falta de Stock",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Alta",
        "version": "1.0",
        "fecha_creacion": "18/09/2026",
        "autor": "Equipo TP — Ingeniería de Software",
        "historial_revision": [
            {"version": "1.0", "fecha": "18/09/2026", "autor": "Equipo TP",
             "descripcion": "Versión inicial — Entrega 2 (análisis y diseño). Dominio de curación y armado de cajas mensuales, Club de Socios."},
        ],
        "proposito": (
            "Permitir que un Usuario con el permiso \"Registrar picking y sustituciones\" (rol "
            "Encargado de Depósito) registre, de forma trazable y append-only, el reemplazo de un "
            "vino de una caja armada cuando el picking detecta que no hay stock suficiente."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Registrar picking y sustituciones\".",
            "La caja está en estado Armada.",
            "El Usuario no es quien armó la caja (RN-10).",
        ],
        "postcondiciones_exito": [
            "Se inserta una fila en SUSTITUCION (vino original, vino de reemplazo, motivo, responsable, fecha); la línea original de CAJA_VINO no se edita.",
            "La composición efectiva de la caja, consultada desde ese momento, refleja el vino de reemplazo.",
        ],
        "postcondiciones_fallo": [
            "Si el Usuario es quien armó la caja (RN-10): se rechaza y no se inserta ninguna fila.",
            "Si la caja ya no está en estado Armada (RN-08): se rechaza.",
            "Si el reemplazo excede el presupuesto disponible o no cumple RN-06: se rechaza.",
        ],
        "disparador": "El Usuario selecciona una línea con faltante y presiona \"Registrar sustitución\" en frmPickingCaja.",
        "puntos_extension": [
            {"paso": "Tras el paso 8 (sustitución registrada)",
             "extension": "La composición efectiva actualizada es la que se despacha en CU-31 Despachar Caja."},
        ],
        "grafico_cu_desc": (
            "Actor: Usuario (permiso \"Registrar picking y sustituciones\")\n"
            "  ──> (Registrar Sustitución por Falta de Stock)\n\n"
            "(Registrar Sustitución) ── «precedido por» ──> (Armar Caja Mensual, CU-28)"
        ),
        "flujo_principal": [
            "El Usuario abre el picking de la caja en frmPickingCaja.",
            "El sistema obtiene la composición efectiva de la caja, derivada de CAJA_VINO y su historial de sustituciones (RN-07).",
            "El sistema marca las líneas con faltante de stock y deshabilita \"Registrar sustitución\" si el Usuario es quien armó la caja (RN-10).",
            "El Usuario selecciona una línea con faltante y presiona \"Registrar sustitución\".",
            "El sistema lista vinos de reemplazo candidatos, filtrados por el saldo de presupuesto liberado por esa línea.",
            "El Usuario elige un vino de reemplazo y un motivo.",
            "El sistema valida RN-10, RN-08 y RN-05/RN-06 sobre el reemplazo, e inserta la sustitución como fila append-only.",
            "El sistema informa que la sustitución quedó registrada y refresca la grilla con la composición efectiva actualizada.",
        ],
        "flujos_alternativos": [
            {"id": "7a", "nombre": "El usuario armó esta caja (RN-10)",
             "pasos": ["El sistema muestra \"No puede registrar una sustitución en una caja que usted mismo armó.\"",
                       "El caso de uso vuelve al paso 4."]},
            {"id": "7b", "nombre": "La caja no está en estado Armada (RN-08)",
             "pasos": ["El sistema muestra \"La caja ya fue despachada; su composición es inmutable.\"",
                       "El caso de uso vuelve al paso 1."]},
            {"id": "7c", "nombre": "El reemplazo excede el presupuesto o no cumple RN-06",
             "pasos": ["El sistema muestra \"El vino de reemplazo excede el presupuesto disponible de la caja.\" o \"El vino de reemplazo no está disponible.\", según corresponda.",
                       "El caso de uso vuelve al paso 5."]},
        ],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Error de conexión con la base de datos durante el registro.",
             "manejo": "Mensaje genérico al Usuario; no se persiste ninguna fila parcial."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-07", "regla": "Toda sustitución es una fila append-only en SUSTITUCION; no existen SP de UPDATE/DELETE sobre esa tabla. La composición efectiva de la caja se deriva, nunca se edita una línea existente."},
            {"codigo": "RN-08", "regla": "Solo se pueden registrar sustituciones sobre cajas en estado Armada."},
            {"codigo": "RN-10", "regla": "El responsable de la sustitución no puede ser quien armó la caja (separación de funciones), verificado en UI, BLL y SP."},
            {"codigo": "RN-05 / RN-06", "regla": "El vino de reemplazo debe cumplir, por sí mismo, el tope de presupuesto restante y el catálogo gobernado."},
        ],
        "relaciones": [
            {"tipo": "«precedido por»", "destino": "CU-28 Armar Caja Mensual",
             "condicion": "Solo existen sustituciones sobre cajas ya armadas."},
            {"tipo": "«precede» a", "destino": "CU-31 Despachar Caja",
             "condicion": "Las sustituciones registradas antes del despacho determinan la composición efectiva que se despacha."},
        ],
        "diagrama_clases_imagen": "DiagramaClases_CuracionCajas.png",
        "diagrama_secuencia_imagen": "DiagramaSecuencia_CU29_RegistrarSustitucion.png",
        "der_imagen": "DER.png",
        "der_entidades_afectadas": ["CAJA_MENSUAL", "CAJA_VINO", "SUSTITUCION", "VINO"],
        "prototipo_interfaz": (
            "frmPickingCaja (WinForms — MaterialForm)\n"
            "+-----------------------------------------------------+\n"
            "|  Picking de Caja #128 — Socio: Juana Pérez     [x]    |\n"
            "+-------------------------------------------------------+\n"
            "| [DataGridView: Vino | Cantidad | Stock | Faltante]     |\n"
            "|                                                        |\n"
            "|          [ Registrar sustitución ]  [ Cerrar ]         |\n"
            "+----------------------------------------------------------+"
        ),
        "observaciones": (
            "Una línea puede sustituirse más de una vez (A→B→C): la identidad estable es "
            "CAJA_VINO_ID, y el vino efectivo es el VINO_REEMPLAZO_ID de la sustitución con mayor "
            "ID para esa línea. Como una sustitución solo puede ocurrir mientras la caja está "
            "Armada (RN-08), es decir antes de cualquier MOVIMIENTO_STOCK (RN-09), nunca hace falta "
            "compensar kardex por una sustitución."
        ),
    },
    # ───── CU-30 ─────────────────────────────────────────────────────
    {
        "id": "CU-30",
        "nombre": "Actualizar Perfil de Socio",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": (
            "Permitir que un Usuario con el permiso \"Gestionar socios\" (rol Atención al Socio) "
            "registre y actualice el perfil de un SOCIO — nombre, contacto, presupuesto mensual y "
            "varietales preferidos — sin crear un USUARIO ni credenciales de acceso: el socio es "
            "una entidad de dominio, nunca un actor con sesión propia."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Gestionar socios\".",
        ],
        "postcondiciones_exito": [
            "El SOCIO queda registrado o actualizado (nombre, contacto, presupuesto mensual, varietales preferidos), sin vínculo a ningún USUARIO.",
        ],
        "postcondiciones_fallo": [
            "Si el presupuesto mensual es menor o igual a cero: no se persiste ningún cambio.",
            "Si el nombre está vacío: no se persiste ningún cambio.",
        ],
        "disparador": "El Usuario abre frmSocios y presiona \"Nuevo socio\" o selecciona un socio existente para editarlo.",
        "flujo_principal": [
            "El Usuario abre frmSocios.",
            "El Usuario ingresa o edita nombre, apellido, contacto, presupuesto mensual y varietales preferidos del socio.",
            "El sistema valida que el nombre esté completo y que el presupuesto mensual sea mayor a cero.",
            "El sistema guarda el socio y, si cambiaron, reemplaza la lista de varietales preferidos.",
            "El sistema informa que el perfil quedó guardado.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "Nombre vacío",
             "pasos": ["El sistema muestra \"El nombre del socio es obligatorio.\"",
                       "El caso de uso vuelve al paso 2."]},
            {"id": "3b", "nombre": "Presupuesto mensual inválido",
             "pasos": ["El sistema muestra \"El presupuesto mensual debe ser mayor a cero.\"",
                       "El caso de uso vuelve al paso 2."]},
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "Validación", "regla": "Actualizar los varietales preferidos de un socio no recalcula cajas ya armadas — el nuevo perfil solo afecta futuros armados (ver CU-28)."},
        ],
        "relaciones": [
            {"tipo": "relacionado con", "destino": "CU-28 Armar Caja Mensual",
             "condicion": "El presupuesto mensual y los varietales preferidos del socio son la entrada para el armado de sus futuras cajas."},
        ],
        "observaciones": "Especificación simple — no requiere diagrama de secuencia dedicado (ver diseño: solo los CU principales de N02 llevan modelado completo).",
    },
    # ───── CU-31 ─────────────────────────────────────────────────────
    {
        "id": "CU-31",
        "nombre": "Despachar Caja",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Alta",
        "prioridad": "Alta",
        "proposito": (
            "Permitir que un Usuario con el permiso \"Despachar cajas\" (rol Logística) despache una "
            "caja mensual en estado Armada, transicionándola a Despachada y registrando, por cada "
            "línea efectiva, un movimiento de kardex de tipo Salida."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Despachar cajas\".",
            "La caja está en estado Armada.",
            "El Usuario no es quien armó la caja (RN-10).",
        ],
        "postcondiciones_exito": [
            "La caja pasa a estado Despachada.",
            "Se crea un MOVIMIENTO_STOCK de tipo Salida por cada línea efectiva de la caja, referenciando la caja (REFERENCIA_TIPO='CAJA', REFERENCIA_ID=caja).",
            "Se genera y abre el Remito de Despacho en PDF (Remitos/Remito_Caja{id}.pdf); si la generación falla, el despacho ya confirmado no se revierte (RN-11).",
        ],
        "postcondiciones_fallo": [
            "Si la caja no está en estado Armada: se rechaza y no se genera ningún movimiento.",
            "Si el Usuario es quien armó la caja (RN-10): se rechaza.",
            "Si alguna línea no tiene stock suficiente al momento del despacho (RN-03): toda la operación se revierte, sin movimientos parciales.",
        ],
        "disparador": "El Usuario presiona \"Despachar\" sobre una caja en estado Armada.",
        "flujo_principal": [
            "El Usuario abre la lista de cajas en estado Armada.",
            "El Usuario selecciona una caja y presiona \"Despachar\".",
            "El sistema valida que el Usuario no sea quien armó la caja (RN-10) y que la caja siga en estado Armada (RN-08).",
            "El sistema verifica el stock disponible de cada línea efectiva de la caja (RN-03).",
            "El sistema inserta un MOVIMIENTO_STOCK de tipo Salida por cada línea y actualiza la caja a estado Despachada, en una única transacción.",
            "El sistema genera el Remito de Despacho en PDF y lo abre (RN-11).",
            "El sistema informa que la caja fue despachada.",
        ],
        "flujos_alternativos": [
            {"id": "3a", "nombre": "El usuario armó la caja (RN-10)",
             "pasos": ["El sistema muestra \"No puede despachar una caja que usted mismo armó.\"",
                       "El caso de uso vuelve al paso 1."]},
            {"id": "3b", "nombre": "La caja no está en estado Armada (RN-08)",
             "pasos": ["El sistema muestra \"Solo se pueden despachar cajas en estado Armada.\"",
                       "El caso de uso vuelve al paso 1."]},
            {"id": "4a", "nombre": "Stock insuficiente en alguna línea (RN-03)",
             "pasos": ["El sistema muestra \"No hay stock suficiente para despachar la línea del vino.\"",
                       "La transacción se revierte por completo; no se genera ningún movimiento.",
                       "El caso de uso vuelve al paso 1."]},
            {"id": "6a", "nombre": "Falla la generación del remito de despacho (RN-11)",
             "pasos": ["El sistema muestra \"No se pudo generar el remito de despacho; la caja fue despachada igualmente.\"",
                       "El despacho ya confirmado no se revierte.",
                       "El caso de uso continúa al paso 7."]},
        ],
        "excepciones": [],
        "reglas_negocio": [
            {"codigo": "RN-03", "regla": "Todo movimiento de Salida que dejaría el stock derivado por debajo de cero se rechaza (bloqueo por stock cero, reactivado desde N01)."},
            {"codigo": "RN-08", "regla": "Solo se pueden despachar cajas en estado Armada; el ciclo de una caja despachada es unidireccional."},
            {"codigo": "RN-09", "regla": "El despacho es el único punto del proceso que genera movimientos de kardex (Salida) para este dominio."},
            {"codigo": "RN-10", "regla": "El responsable del despacho no puede ser quien armó la caja (separación de funciones)."},
            {"codigo": "RN-11", "regla": "La generación del Remito de Despacho es best-effort: si falla, el despacho queda confirmado igual (CAJA_DESPACHAR es la fuente de verdad transaccional), se informa al usuario, y no se revierte ninguna operación."},
        ],
        "relaciones": [
            {"tipo": "«precedido por»", "destino": "CU-28 Armar Caja Mensual",
             "condicion": "Solo se pueden despachar cajas previamente armadas."},
            {"tipo": "relacionado con", "destino": "CU-29 Registrar Sustitución por Falta de Stock",
             "condicion": "Las sustituciones registradas antes del despacho determinan la composición efectiva que se despacha."},
        ],
        "observaciones": (
            "REFERENCIA_TIPO/REFERENCIA_ID de MOVIMIENTO_STOCK fueron creados en N01 y dejados en "
            "null en esa entrega; el despacho de una caja es su primer consumidor real, vinculando "
            "cada Salida a su CAJA_MENSUAL por una referencia blanda (soft link, sin FK física). "
            "Especificación simple — no requiere diagrama de secuencia dedicado en esta entrega "
            "(ver diseño: solo CU-28 y CU-29 llevan modelado completo con diagrama de secuencia)."
        ),
    },
    # ───── CU-32 ─────────────────────────────────────────────────────
    {
        "id": "CU-32",
        "nombre": "Consultar Historial de Despachos",
        "actor_primario": "Usuario",
        "actor_secundario": None,
        "frecuencia": "Media",
        "prioridad": "Media",
        "proposito": (
            "Permitir que un Usuario con el permiso \"Despachar cajas\" (rol Logística) liste las "
            "cajas mensuales ya despachadas y reimprima el Remito de Despacho de cualquiera de "
            "ellas bajo demanda, sin rehacer el despacho."
        ),
        "precondiciones": [
            "El Usuario inició sesión correctamente y tiene el permiso \"Despachar cajas\".",
            "Existe al menos una caja en estado Despachada.",
        ],
        "postcondiciones_exito": [
            "El Remito de Despacho de la caja seleccionada se regenera, sobrescribiendo "
            "Remitos/Remito_Caja{id}.pdf en el mismo path, sin sufijo de timestamp (RN-12), y se abre.",
        ],
        "postcondiciones_fallo": [
            "Si la generación del PDF falla: se muestra un mensaje de error y no se abre ningún "
            "archivo; el historial permanece navegable y ninguna otra operación se ve afectada.",
        ],
        "disparador": "El Usuario abre frmHistorialDespachos.",
        "flujo_principal": [
            "El Usuario abre el historial de despachos.",
            "El sistema lista las cajas en estado Despachada, ordenadas por fecha de despacho descendente.",
            "El Usuario selecciona una caja y presiona \"Regenerar remito\".",
            "El sistema obtiene la composición efectiva de la caja y regenera el PDF, sobrescribiendo Remito_Caja{id}.pdf (RN-12).",
            "El sistema abre el PDF regenerado.",
        ],
        "flujos_alternativos": [],
        "excepciones": [
            {"codigo": "EX-01", "descripcion": "Falla la generación del remito (RN-11): permisos, disco lleno, lector de PDF ausente, etc.",
             "manejo": "El sistema muestra \"No se pudo generar el remito de despacho; la caja fue despachada igualmente.\"; no abre ningún archivo; el historial sigue navegable."},
        ],
        "reglas_negocio": [
            {"codigo": "RN-07", "regla": "El remito regenerado refleja la composición efectiva de la caja (líneas con sustituciones aplicadas), igual que en el despacho original."},
            {"codigo": "RN-11", "regla": "La generación del Remito de Despacho es best-effort: una falla no afecta el estado de la caja, que ya es Despachada e inmutable desde este form."},
            {"codigo": "RN-12", "regla": "La regeneración del remito desde el historial sobrescribe Remito_Caja{id}.pdf en el mismo path — no se generan copias versionadas con timestamp."},
        ],
        "relaciones": [
            {"tipo": "relacionado con", "destino": "CU-31 Despachar Caja",
             "condicion": "Ambos comparten el renderer RemitoDespachoPdf como detalle de implementación; no existe relación «include»/«extend» entre los dos casos de uso."},
        ],
        "observaciones": (
            "Especificación simple — no requiere diagrama de secuencia dedicado en esta entrega "
            "(ver diseño: solo CU-28 y CU-29 llevan modelado completo con diagrama de secuencia). "
            "Reutiliza el mismo permiso \"Despachar cajas\" que CU-31 — no se creó ningún permiso nuevo."
        ),
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


def agregar_caratula_extra(doc, cu):
    """Carátula extendida (versión / fecha / autor) — solo para CU con plantilla completa."""
    if not (cu.get("version") or cu.get("fecha_creacion") or cu.get("autor")):
        return
    filas = []
    if cu.get("version"):
        filas.append(("Versión", cu["version"]))
    if cu.get("fecha_creacion"):
        filas.append(("Fecha", cu["fecha_creacion"]))
    if cu.get("autor"):
        filas.append(("Autor", cu["autor"]))

    tabla = doc.add_table(rows=0, cols=2)
    tabla.style = 'Light Grid Accent 1'
    for etiqueta, valor in filas:
        fila = tabla.add_row()
        fila.cells[0].text = etiqueta
        fila.cells[1].text = valor
        for run in fila.cells[0].paragraphs[0].runs:
            run.bold = True
        set_cell_bg(fila.cells[0], AZUL_CLARO_BG)
    doc.add_paragraph()


def agregar_historial_revision(doc, historial):
    if not historial:
        return
    doc.add_heading("Historial de revisión", level=2)
    tabla = doc.add_table(rows=1, cols=4)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    for i, txt in enumerate(["Versión", "Fecha", "Autor", "Descripción"]):
        headers[i].text = txt
        for run in headers[i].paragraphs[0].runs:
            run.bold = True
        set_cell_bg(headers[i], AZUL_CLARO_BG)
    for h in historial:
        fila = tabla.add_row().cells
        fila[0].text = h["version"]
        fila[1].text = h["fecha"]
        fila[2].text = h["autor"]
        fila[3].text = h["descripcion"]
    doc.add_paragraph()


def agregar_puntos_extension(doc, puntos):
    if not puntos:
        return
    doc.add_heading("Puntos de extensión", level=2)
    tabla = doc.add_table(rows=1, cols=2)
    tabla.style = 'Light Grid Accent 1'
    headers = tabla.rows[0].cells
    headers[0].text = "Paso"
    headers[1].text = "Extensión posible"
    for c in headers:
        for run in c.paragraphs[0].runs:
            run.bold = True
        set_cell_bg(c, AZUL_CLARO_BG)
    for p in puntos:
        fila = tabla.add_row().cells
        fila[0].text = p["paso"]
        fila[1].text = p["extension"]
    doc.add_paragraph()


def agregar_grafico_cu(doc, texto):
    """Gráfico del CU: representación esquemática en texto (sin imagen dedicada)."""
    if not texto:
        return
    doc.add_heading("Gráfico del caso de uso", level=2)
    p = doc.add_paragraph()
    run = p.add_run(texto)
    run.font.name = 'Consolas'
    run.font.size = Pt(9)
    doc.add_paragraph()


def agregar_imagen_o_nota(doc, titulo, filename, ancho_cm=15):
    """Embebe la imagen si ya fue renderizada (PNG); si no, deja una nota de referencia."""
    if not filename:
        return
    doc.add_heading(titulo, level=2)
    if os.path.exists(filename):
        doc.add_picture(filename, width=Cm(ancho_cm))
    else:
        puml_name = os.path.splitext(filename)[0] + ".puml"
        doc.add_paragraph(
            f"[Imagen pendiente de generar: {filename}. Renderizar con "
            f"'python generar_png.py {puml_name} {filename}' o 'python generar_pngs_lote.py']"
        )
    doc.add_paragraph()


def agregar_der_con_entidades(doc, filename, entidades):
    if not filename and not entidades:
        return
    agregar_imagen_o_nota(doc, "DER — entidades afectadas", filename)
    if entidades:
        doc.add_paragraph("Entidades afectadas: " + ", ".join(entidades))


def agregar_prototipo_interfaz(doc, texto):
    if not texto:
        return
    doc.add_heading("Prototipo de interfaz de usuario", level=2)
    p = doc.add_paragraph()
    run = p.add_run(texto)
    run.font.name = 'Consolas'
    run.font.size = Pt(8)


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
    agregar_caratula_extra(doc, cu)
    agregar_tabla_resumen(doc, cu)
    agregar_historial_revision(doc, cu.get("historial_revision"))
    agregar_seccion_parrafo(doc, "Objetivo", cu["proposito"])
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

    agregar_seccion_parrafo(doc, "Evento disparador", cu["disparador"])
    agregar_puntos_extension(doc, cu.get("puntos_extension"))
    agregar_grafico_cu(doc, cu.get("grafico_cu_desc"))
    agregar_seccion_lista(doc, "Flujo principal", cu["flujo_principal"], numerada=True)
    agregar_flujos_alternativos(doc, cu.get("flujos_alternativos", []))
    agregar_excepciones(doc, cu.get("excepciones", []))
    agregar_reglas_negocio(doc, cu.get("reglas_negocio", []))
    agregar_relaciones(doc, cu.get("relaciones", []))
    agregar_imagen_o_nota(doc, "Diagrama de clases afectadas", cu.get("diagrama_clases_imagen"))
    agregar_imagen_o_nota(doc, "Diagrama de secuencia", cu.get("diagrama_secuencia_imagen"))
    agregar_der_con_entidades(doc, cu.get("der_imagen"), cu.get("der_entidades_afectadas"))
    agregar_prototipo_interfaz(doc, cu.get("prototipo_interfaz"))
    agregar_seccion_parrafo(doc, "Observaciones", cu.get("observaciones"))


def generar_tabla_de_contenidos(doc):
    doc.add_heading("Casos de Uso", level=0)
    doc.add_paragraph(
        "Sistema de Gestión de Usuarios — TP Ingeniería de Software.\n"
        f"Documento de descripción de los {len(CUS)} casos de uso identificados: CU-01..CU-20 "
        "(esqueleto base — usuarios, roles/permisos, idiomas, bitácora, integridad) y "
        "CU-21..CU-25 y CU-27 (Proponer/Autorizar Alta, Registrar Movimiento de Stock, "
        "Solicitar/Autorizar Descontinuación, Registrar Ajuste de Inventario) más el caso "
        "de soporte CU-26 Consultar Alerta de Stock Mínimo, del dominio de Gestión de "
        "Catálogo y Stock de Vinos (Entrega N01); y CU-28..CU-32 (Armar Caja Mensual, "
        "Registrar Sustitución por Falta de Stock, Actualizar Perfil de Socio, Despachar "
        "Caja, Consultar Historial de Despachos), del dominio de Curación y Armado de "
        "Cajas Mensuales, Club de Socios (Entrega N02)."
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
