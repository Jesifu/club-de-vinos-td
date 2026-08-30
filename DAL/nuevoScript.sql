
-- ============================================================
-- MIGRACIÓN — TABLAS ROL Y PERMISO SEPARADAS (reemplazan NODO_PERMISO)
-- ============================================================
-- NODO_PERMISO mezclaba roles (TIPO='PERFIL') y permisos de catálogo
-- (TIPO='PERMISO') en una sola tabla. Se separan en ROL y PERMISO,
-- preservando los IDs originales para no romper referencias existentes
-- en ROL_PERMISO / USUARIO_PERFIL durante la migración.
USE [BDCAPAS]
GO
IF OBJECT_ID('dbo.ROL', 'U') IS NULL
CREATE TABLE [dbo].[ROL] (
    [ID]        INT          IDENTITY(1,1) NOT NULL,
    [NOMBRE]    VARCHAR(100) NOT NULL,
    [PADRE_ID]  INT          NULL,
    [PROTEGIDO] BIT          NOT NULL DEFAULT 0,
    CONSTRAINT PK_ROL PRIMARY KEY ([ID]),
    CONSTRAINT FK_ROL_PADRE FOREIGN KEY ([PADRE_ID]) REFERENCES [dbo].[ROL]([ID])
)
GO

IF OBJECT_ID('dbo.PERMISO', 'U') IS NULL
CREATE TABLE [dbo].[PERMISO] (
    [ID]     INT          IDENTITY(1,1) NOT NULL,
    [NOMBRE] VARCHAR(100) NOT NULL,
    CONSTRAINT PK_PERMISO PRIMARY KEY ([ID])
)
GO

-- Migrar datos de NODO_PERMISO -> ROL / PERMISO, preservando ID original
-- NOTA: esta migración asume que corre una sola vez, antes de que se
-- creen roles/permisos nuevos directamente en ROL/PERMISO. Si ya se
-- crearon roles nuevos vía ROL_INSERTAR antes de ejecutar este bloque,
-- podría haber colisión de ID; no es un escenario esperado en el
-- flujo normal de actualización del script.
IF OBJECT_ID('dbo.NODO_PERMISO', 'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT [dbo].[ROL] ON

    INSERT INTO [dbo].[ROL] (ID, NOMBRE, PADRE_ID, PROTEGIDO)
    SELECT np.ID, np.NOMBRE, np.PADRE_ID, np.PROTEGIDO
    FROM NODO_PERMISO np
    WHERE np.TIPO = 'PERFIL'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[ROL] r WHERE r.ID = np.ID)

    SET IDENTITY_INSERT [dbo].[ROL] OFF

    SET IDENTITY_INSERT [dbo].[PERMISO] ON

    INSERT INTO [dbo].[PERMISO] (ID, NOMBRE)
    SELECT np.ID, np.NOMBRE
    FROM NODO_PERMISO np
    WHERE np.TIPO = 'PERMISO'
      AND NOT EXISTS (SELECT 1 FROM [dbo].[PERMISO] p WHERE p.ID = np.ID)

    SET IDENTITY_INSERT [dbo].[PERMISO] OFF
END
GO

-- Reconstruir ROL_PERMISO apuntando a ROL y PERMISO
IF OBJECT_ID('dbo.ROL_PERMISO', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ROLPERM_ROL')
        ALTER TABLE [dbo].[ROL_PERMISO] DROP CONSTRAINT FK_ROLPERM_ROL
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ROLPERM_PERMISO')
        ALTER TABLE [dbo].[ROL_PERMISO] DROP CONSTRAINT FK_ROLPERM_PERMISO
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ROLPERM_ROL')
    ALTER TABLE [dbo].[ROL_PERMISO] ADD CONSTRAINT FK_ROLPERM_ROL FOREIGN KEY ([ROL_ID]) REFERENCES [dbo].[ROL]([ID])
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ROLPERM_PERMISO')
    ALTER TABLE [dbo].[ROL_PERMISO] ADD CONSTRAINT FK_ROLPERM_PERMISO FOREIGN KEY ([PERMISO_ID]) REFERENCES [dbo].[PERMISO]([ID])
GO

-- USUARIO_PERFIL.PERFIL_ID también referencia ROL ahora
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_USUARIOPERFIL_ROL')
    ALTER TABLE [dbo].[USUARIO_PERFIL] ADD CONSTRAINT FK_USUARIOPERFIL_ROL FOREIGN KEY ([PERFIL_ID]) REFERENCES [dbo].[ROL]([ID])
GO

-- ============================================================
-- MIGRACIÓN — FK USUARIO -> ROL
-- ============================================================

IF COL_LENGTH('dbo.USUARIO','ROL_ID') IS NULL
    ALTER TABLE [dbo].[USUARIO] ADD [ROL_ID] INT NULL
GO

-- Backfill: cada usuario apunta al ROL cuyo nombre coincide con
-- 'Administrador' o 'Usuario' según el string legacy en USUARIO.ROL
UPDATE u
SET u.ROL_ID = r.ID
FROM USUARIO u
JOIN ROL r ON r.NOMBRE = CASE WHEN u.ROL = 'admin' THEN 'Administrador' ELSE 'Usuario' END
WHERE u.ROL_ID IS NULL
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_USUARIO_ROL')
    ALTER TABLE [dbo].[USUARIO] ADD CONSTRAINT FK_USUARIO_ROL FOREIGN KEY ([ROL_ID]) REFERENCES [dbo].[ROL]([ID])
GO

-- ============================================================
-- MIGRACIÓN — CAMPOS DUMMY (TELEFONO, EMAIL)
-- ============================================================
-- No sensibles: el rollback de historial los restaura sin tocar
-- PASS, BLOQUEADO ni INTENTOS_FALLIDOS. No forman parte del array
-- canónico de IntegridadBLL, por lo que no afectan el cálculo de DVH/DVV.

IF COL_LENGTH('dbo.USUARIO','TELEFONO') IS NULL
    ALTER TABLE [dbo].[USUARIO] ADD [TELEFONO] VARCHAR(30) NULL
IF COL_LENGTH('dbo.USUARIO','EMAIL') IS NULL
    ALTER TABLE [dbo].[USUARIO] ADD [EMAIL] VARCHAR(100) NULL
GO

IF COL_LENGTH('dbo.USUARIO_HISTORIAL','TELEFONO') IS NULL
    ALTER TABLE [dbo].[USUARIO_HISTORIAL] ADD [TELEFONO] VARCHAR(30) NULL
IF COL_LENGTH('dbo.USUARIO_HISTORIAL','EMAIL') IS NULL
    ALTER TABLE [dbo].[USUARIO_HISTORIAL] ADD [EMAIL] VARCHAR(100) NULL
GO

-- ROL_ID también se versiona en el historial para que el rollback
-- pueda restaurar el rol real (FK), no solo el string legacy ROL.
IF COL_LENGTH('dbo.USUARIO_HISTORIAL','ROL_ID') IS NULL
    ALTER TABLE [dbo].[USUARIO_HISTORIAL] ADD [ROL_ID] INT NULL
GO

-- ============================================================
-- STORED PROCEDURES — ROL (reemplazan PERFIL_*)
-- ============================================================

IF OBJECT_ID('dbo.ROL_LISTAR_TODOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_LISTAR_TODOS]
GO
CREATE PROCEDURE [dbo].[ROL_LISTAR_TODOS]
AS
    SELECT ID, NOMBRE, PADRE_ID, PROTEGIDO
    FROM ROL
    ORDER BY ID
GO

IF OBJECT_ID('dbo.ROL_TIENE_USUARIOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_TIENE_USUARIOS]
GO
CREATE PROCEDURE [dbo].[ROL_TIENE_USUARIOS]
    @rol_id INT
AS
BEGIN
    DECLARE @ids TABLE (ID INT)
    ;WITH Descendientes AS (
        SELECT ID FROM ROL WHERE ID = @rol_id
        UNION ALL
        SELECT r.ID FROM ROL r
            INNER JOIN Descendientes d ON r.PADRE_ID = d.ID
    )
    INSERT INTO @ids SELECT ID FROM Descendientes

    SELECT
        (SELECT COUNT(*) FROM USUARIO_PERFIL WHERE PERFIL_ID IN (SELECT ID FROM @ids))
      + (SELECT COUNT(*) FROM USUARIO        WHERE ROL_ID    IN (SELECT ID FROM @ids))
      AS TOTAL
END
GO

IF OBJECT_ID('dbo.ROL_INSERTAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_INSERTAR]
GO
CREATE PROCEDURE [dbo].[ROL_INSERTAR]
    @nombre   VARCHAR(100),
    @padre_id INT = NULL
AS
BEGIN
    INSERT INTO ROL (NOMBRE, PADRE_ID) VALUES (@nombre, @padre_id)
    SELECT SCOPE_IDENTITY() AS ID
END
GO

IF OBJECT_ID('dbo.ROL_ELIMINAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_ELIMINAR]
GO
CREATE PROCEDURE [dbo].[ROL_ELIMINAR]
    @id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM ROL WHERE ID = @id AND PROTEGIDO = 1)
    BEGIN
        RAISERROR('Este rol es del sistema y no puede eliminarse.', 16, 1)
        RETURN
    END

    DECLARE @ids TABLE (ID INT)
    ;WITH Descendientes AS (
        SELECT ID FROM ROL WHERE ID = @id
        UNION ALL
        SELECT r.ID FROM ROL r
            INNER JOIN Descendientes d ON r.PADRE_ID = d.ID
    )
    INSERT INTO @ids SELECT ID FROM Descendientes

    DELETE FROM USUARIO_PERFIL WHERE PERFIL_ID IN (SELECT ID FROM @ids)
    DELETE FROM ROL_PERMISO    WHERE ROL_ID    IN (SELECT ID FROM @ids)
    DELETE FROM ROL            WHERE ID        IN (SELECT ID FROM @ids)
END
GO

IF OBJECT_ID('dbo.ROL_CAMBIAR_PADRE', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_CAMBIAR_PADRE]
GO
CREATE PROCEDURE [dbo].[ROL_CAMBIAR_PADRE]
    @id       INT,
    @padre_id INT = NULL
AS
    UPDATE ROL SET PADRE_ID = @padre_id WHERE ID = @id
GO

IF OBJECT_ID('dbo.ROL_LISTAR_PARA_COMBO', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_LISTAR_PARA_COMBO]
GO
CREATE PROCEDURE [dbo].[ROL_LISTAR_PARA_COMBO]
AS
    SELECT ID, NOMBRE FROM ROL ORDER BY NOMBRE
GO

-- ============================================================
-- STORED PROCEDURES — PERMISO (reemplaza PERMISO_LISTAR_TODOS)
-- ============================================================

IF OBJECT_ID('dbo.PERMISO_LISTAR_TODOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[PERMISO_LISTAR_TODOS]
GO
CREATE PROCEDURE [dbo].[PERMISO_LISTAR_TODOS]
AS
    SELECT ID, NOMBRE
    FROM PERMISO
    ORDER BY NOMBRE
GO

-- ============================================================
-- STORED PROCEDURES — ROL_PERMISO (re-apuntan a ROL/PERMISO)
-- ============================================================

IF OBJECT_ID('dbo.ROL_PERMISO_LISTAR_TODOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[ROL_PERMISO_LISTAR_TODOS]
GO
CREATE PROCEDURE [dbo].[ROL_PERMISO_LISTAR_TODOS]
AS
    SELECT rp.ROL_ID, rp.PERMISO_ID, p.NOMBRE
    FROM ROL_PERMISO rp
    JOIN PERMISO p ON p.ID = rp.PERMISO_ID
GO

IF OBJECT_ID('dbo.USUARIO_PERMISOS_LISTAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_PERMISOS_LISTAR]
GO
-- No hereda permisos de roles ancestros: PADRE_ID es solo organización
-- visual del árbol de roles (frmPerfiles), sin efecto sobre permisos.
CREATE PROCEDURE [dbo].[USUARIO_PERMISOS_LISTAR]
    @usuario_id INT
AS
BEGIN
    SELECT DISTINCT p.NOMBRE
    FROM USUARIO_PERFIL up
    JOIN ROL_PERMISO rp ON rp.ROL_ID = up.PERFIL_ID
    JOIN PERMISO     p  ON p.ID      = rp.PERMISO_ID
    WHERE up.USUARIO_ID = @usuario_id
END
GO

-- ============================================================
-- STORED PROCEDURES — USUARIO (rol real + campos dummy)
-- ============================================================

IF OBJECT_ID('dbo.USUARIO_LOGIN', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_LOGIN]
GO
CREATE PROCEDURE [dbo].[USUARIO_LOGIN]
    @usuario VARCHAR(64),
    @pass    VARCHAR(64)
AS
    SELECT ID, USUARIO, ROL, ROL_ID, IDIOMA_ID
    FROM USUARIO
    WHERE USUARIO = @usuario AND PASS = @pass
GO

IF OBJECT_ID('dbo.USUARIO_CREAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_CREAR]
GO
CREATE PROCEDURE [dbo].[USUARIO_CREAR]
    @usuario  VARCHAR(50),
    @pass     VARCHAR(64),
    @rol_id   INT,
    @nombre   VARCHAR(50) = NULL,
    @apellido VARCHAR(50) = NULL,
    @telefono VARCHAR(30) = NULL,
    @email    VARCHAR(100) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM USUARIO WHERE USUARIO = @usuario)
    BEGIN
        SELECT 0 AS OK
        RETURN
    END

    DECLARE @rolNombre VARCHAR(20) = (SELECT CASE WHEN PROTEGIDO = 1 THEN 'admin' ELSE 'usuario' END
                                       FROM ROL WHERE ID = @rol_id)

    INSERT INTO USUARIO (USUARIO, PASS, ROL, ROL_ID, NOMBRE, APELLIDO, TELEFONO, EMAIL)
    VALUES (@usuario, @pass, ISNULL(@rolNombre, 'usuario'), @rol_id, @nombre, @apellido, @telefono, @email)

    DECLARE @nuevoId INT = SCOPE_IDENTITY()

    -- El rol elegido en el combo también se asigna como perfil,
    -- así el usuario nuevo ya hereda los permisos correspondientes
    -- sin requerir el paso manual de "Modificar perfiles".
    IF NOT EXISTS (SELECT 1 FROM USUARIO_PERFIL WHERE USUARIO_ID = @nuevoId AND PERFIL_ID = @rol_id)
        INSERT INTO USUARIO_PERFIL (USUARIO_ID, PERFIL_ID) VALUES (@nuevoId, @rol_id)

    SELECT 1 AS OK
END
GO

IF OBJECT_ID('dbo.USUARIO_LISTAR_TODOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_LISTAR_TODOS]
GO
CREATE PROCEDURE [dbo].[USUARIO_LISTAR_TODOS]
AS
    SELECT u.ID, u.USUARIO, u.ROL, u.ROL_ID, r.NOMBRE AS ROL_NOMBRE, u.BLOQUEADO,
           u.NOMBRE, u.APELLIDO, u.TELEFONO, u.EMAIL
    FROM USUARIO u
    LEFT JOIN ROL r ON r.ID = u.ROL_ID
    ORDER BY u.USUARIO
GO

IF OBJECT_ID('dbo.USUARIO_OBTENER_POR_ID', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_OBTENER_POR_ID]
GO
CREATE PROCEDURE [dbo].[USUARIO_OBTENER_POR_ID]
    @id INT
AS
    SELECT ID, USUARIO, ROL, ROL_ID, INTENTOS_FALLIDOS, BLOQUEADO,
           NOMBRE, APELLIDO, TELEFONO, EMAIL
    FROM USUARIO WHERE ID = @id
GO

IF OBJECT_ID('dbo.USUARIO_ACTUALIZAR_DATOS', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_ACTUALIZAR_DATOS]
GO
CREATE PROCEDURE [dbo].[USUARIO_ACTUALIZAR_DATOS]
    @id       INT,
    @nombre   VARCHAR(50),
    @apellido VARCHAR(50),
    @telefono VARCHAR(30) = NULL,
    @email    VARCHAR(100) = NULL
AS
    UPDATE USUARIO
    SET NOMBRE = @nombre, APELLIDO = @apellido, TELEFONO = @telefono, EMAIL = @email
    WHERE ID = @id
GO

IF OBJECT_ID('dbo.USUARIO_APLICAR_ESTADO', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_APLICAR_ESTADO]
GO
CREATE PROCEDURE [dbo].[USUARIO_APLICAR_ESTADO]
    @id                INT,
    @rol               VARCHAR(20),
    @bloqueado         BIT,
    @intentos_fallidos INT,
    @rol_id            INT = NULL
AS
    UPDATE USUARIO
    SET ROL = @rol, BLOQUEADO = @bloqueado, INTENTOS_FALLIDOS = @intentos_fallidos,
        ROL_ID = ISNULL(@rol_id, ROL_ID)
    WHERE ID = @id
GO

-- ============================================================
-- STORED PROCEDURES — USUARIO_HISTORIAL (campos dummy)
-- ============================================================

IF OBJECT_ID('dbo.USUARIO_HISTORIAL_INSERTAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_HISTORIAL_INSERTAR]
GO
CREATE PROCEDURE [dbo].[USUARIO_HISTORIAL_INSERTAR]
    @usuario_id        INT,
    @usuario_login     VARCHAR(50),
    @rol               VARCHAR(20),
    @bloqueado         BIT,
    @intentos_fallidos INT,
    @perfiles          VARCHAR(500),
    @realizado_por     VARCHAR(50),
    @tipo_cambio       VARCHAR(25),
    @version_origen    INT,
    @nombre            VARCHAR(50) = NULL,
    @apellido          VARCHAR(50) = NULL,
    @telefono          VARCHAR(30) = NULL,
    @email             VARCHAR(100) = NULL,
    @rol_id            INT = NULL
AS
    INSERT INTO USUARIO_HISTORIAL
        (USUARIO_ID, USUARIO_LOGIN, ROL, BLOQUEADO, INTENTOS_FALLIDOS,
         PERFILES, REALIZADO_POR, TIPO_CAMBIO, VERSION_ORIGEN, NOMBRE, APELLIDO, TELEFONO, EMAIL, ROL_ID)
    VALUES
        (@usuario_id, @usuario_login, @rol, @bloqueado, @intentos_fallidos,
         @perfiles, @realizado_por, @tipo_cambio, @version_origen, @nombre, @apellido, @telefono, @email, @rol_id)
GO

IF OBJECT_ID('dbo.USUARIO_HISTORIAL_LISTAR', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_HISTORIAL_LISTAR]
GO
CREATE PROCEDURE [dbo].[USUARIO_HISTORIAL_LISTAR]
    @usuario_id INT
AS
    SELECT ID, USUARIO_ID, USUARIO_LOGIN, ROL, ROL_ID, BLOQUEADO, INTENTOS_FALLIDOS,
           PERFILES, FECHA_CAMBIO, REALIZADO_POR, TIPO_CAMBIO, VERSION_ORIGEN,
           NOMBRE, APELLIDO, TELEFONO, EMAIL
    FROM USUARIO_HISTORIAL
    WHERE USUARIO_ID = @usuario_id
    ORDER BY ID DESC
GO

IF OBJECT_ID('dbo.USUARIO_HISTORIAL_OBTENER', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_HISTORIAL_OBTENER]
GO
CREATE PROCEDURE [dbo].[USUARIO_HISTORIAL_OBTENER]
    @id INT
AS
    SELECT ID, USUARIO_ID, USUARIO_LOGIN, ROL, ROL_ID, BLOQUEADO, INTENTOS_FALLIDOS,
           PERFILES, FECHA_CAMBIO, REALIZADO_POR, TIPO_CAMBIO, VERSION_ORIGEN,
           NOMBRE, APELLIDO, TELEFONO, EMAIL
    FROM USUARIO_HISTORIAL WHERE ID = @id
GO

-- ============================================================
-- DATOS — controles de idioma para campos nuevos
-- ============================================================

EXEC CONTROL_REGISTRAR 'lblTelefono',         'Teléfono:'
EXEC CONTROL_REGISTRAR 'lblEmail',            'Email:'
EXEC CONTROL_REGISTRAR 'colhdr_Telefono',     'Teléfono'
EXEC CONTROL_REGISTRAR 'colhdr_Email',        'Email'
GO

-- ============================================================
-- NOTA
-- ============================================================
-- A partir de esta migración, NODO_PERMISO es un remanente legacy
-- (igual que PERSONA): sus datos ya fueron copiados a ROL y PERMISO,
-- y ningún SP nuevo la referencia. Se conserva sin DROP por seguridad,
-- pero puede eliminarse manualmente una vez verificado el funcionamiento
-- completo de la migración en el entorno de pruebas.
