-- ============================================================
-- Regenerar idioma 'Español' (eliminado por error con CU-18)
-- y todas sus traducciones en CONTROL_IDIOMA / TRADUCCION.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM IDIOMA WHERE NOMBRE = 'Español')
    INSERT INTO IDIOMA (NOMBRE, HABILITADO) VALUES ('Español', 1)
GO

DECLARE @espId INT = (SELECT ID FROM IDIOMA WHERE NOMBRE = 'Español')
DECLARE @cide  INT

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'LogIn')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Iniciar Sesión'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblUsuario')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblContrasena')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Contraseña'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnIngresar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Ingresar'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'usuarioToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'cerrarSesionToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cerrar Sesión'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'configuraciónToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Configuración'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'cambiarContraseñaToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cambiar Contraseña'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'bitacoraToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Bitácora'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'administracionToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Administración'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'usuariosBloqueadosToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Gestión de usuarios'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'perfilesToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Perfiles y Permisos'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'idiomasToolStripMenuItem')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Gestión de idiomas'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmAdminUsuarios')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Administración de usuarios'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblTitulo_AdminUsuarios')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Gestión de usuarios'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnNuevo')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Nuevo usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnModificarPerfiles')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Modificar perfiles'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnDesbloquear')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Desbloquear seleccionado'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnEliminar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Eliminar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnRefrescar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Refrescar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnCerrar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cerrar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnHistorial')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Ver historial'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmNuevoUsuario')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Nuevo usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblNombre')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Usuario:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblRol')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Rol:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnAceptar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Crear'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnCancelar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cancelar'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmContraseña')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cambiar Contraseña'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'label1')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Contraseña Actual:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'label2')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Nueva Contraseña:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'label3')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Confirmar Contraseña:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'label4')
EXEC TRADUCCION_GUARDAR @espId, @cide, '- 6 o más caracteres / 1 MAYÚSCULA / 1 número'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'label5')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cambiar Contraseña'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnContinuar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Continuar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'button1')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Cancelar'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmBitacora')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Bitácora'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblTitulo_Bitacora')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Bitácora del Sistema'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblAccion')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Acción:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblDesde')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Desde:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblHasta')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Hasta:'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnFiltrar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Filtrar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnLimpiar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Limpiar'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmPerfiles')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Gestión de Roles y Permisos'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblSeleccionado')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Seleccioná un nodo'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnAgregarRol')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Agregar Rol'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnGuardar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Guardar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'prefijo_Rol')
EXEC TRADUCCION_GUARDAR @espId, @cide, '[Rol] '
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'prefijo_Permiso')
EXEC TRADUCCION_GUARDAR @espId, @cide, '[Permiso] '

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmAsignarPerfiles')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Asignar roles al usuario'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmIdiomas')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Gestión de idiomas'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblIdiomas')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Idiomas'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnAgregarIdioma')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Agregar idioma'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnRenombrar')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Renombrar idioma'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnToggleHabilitado')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Habilitar/Deshabilitar'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnEliminarIdioma')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Eliminar idioma'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblTraducciones')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Traducciones del idioma seleccionado'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnGuardarTraducciones')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Guardar traducciones'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'frmHistorialUsuario')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Historial de usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'lblTitulo_HistorialUsuarios')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Historial de cambios'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'btnRollback')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Restaurar versión'

SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Id')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'ID'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Usuario')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Usuario'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Rol')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Rol'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Bloqueado')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Bloqueado'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Accion')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Acción'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Fecha')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Fecha y Hora'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Nombre')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Idioma'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Habilitado')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Habilitado'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Clave')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Clave'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_TextoDefault')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Texto por defecto'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_TextoTraduccion')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Traducción'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_FechaCambio')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Fecha'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_TipoCambio')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Tipo'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_IntentosFallidos')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Intentos'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_Perfiles')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Perfiles'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_RealizadoPor')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Realizado por'
SET @cide = (SELECT ID FROM CONTROL_IDIOMA WHERE CLAVE = 'colhdr_VersionOrigen')
EXEC TRADUCCION_GUARDAR @espId, @cide, 'Versión origen'
GO
