-- ============================================================
-- PAGINACIÓN — BITACORA, USUARIO_HISTORIAL, USUARIO
-- ============================================================
-- Se agregan versiones paginadas de los listados que más crecen
-- con el uso del sistema (bitácora global, historial por usuario,
-- listado de usuarios). Usan OFFSET/FETCH de SQL Server.
--
-- IMPORTANTE: Acceso.Leer() usa SqlDataAdapter.Fill(DataTable), que
-- solo lee el PRIMER result set de un SP. Por eso el total de filas
-- que matchean el filtro se devuelve como columna repetida en cada
-- fila (COUNT(*) OVER()) en lugar de un segundo SELECT separado —
-- así un solo Leer() trae los datos de la página y el total juntos,
-- sin tocar Acceso.cs ni agregar soporte multi-resultset.

-- ------------------------------------------------------------
-- BITACORA — filtro + paginado en SQL (antes se traía todo y se
-- filtraba en memoria con LINQ del lado de la UI)
-- ------------------------------------------------------------
USE [BDCAPAS]
GO
IF OBJECT_ID('dbo.BITACORA_LISTAR_PAGINADO', 'P') IS NOT NULL DROP PROCEDURE [dbo].[BITACORA_LISTAR_PAGINADO]
GO
CREATE PROCEDURE [dbo].[BITACORA_LISTAR_PAGINADO]
    @usuario      VARCHAR(50)  = NULL,
    @accion       VARCHAR(50)  = NULL,
    @fecha_desde  DATETIME     = NULL,
    @fecha_hasta  DATETIME     = NULL,
    @pagina       INT          = 1,
    @tamanio      INT          = 50
AS
BEGIN
    IF @pagina  < 1 SET @pagina  = 1
    IF @tamanio < 1 SET @tamanio = 50

    SELECT ID, USUARIO, ACCION, FECHA,
           COUNT(*) OVER() AS TOTAL_FILAS
    FROM BITACORA
    WHERE (@usuario     IS NULL OR USUARIO = @usuario)
      AND (@accion      IS NULL OR ACCION  = @accion)
      AND (@fecha_desde IS NULL OR FECHA  >= @fecha_desde)
      AND (@fecha_hasta IS NULL OR FECHA  <  @fecha_hasta)
    ORDER BY FECHA DESC
    OFFSET (@pagina - 1) * @tamanio ROWS
    FETCH NEXT @tamanio ROWS ONLY
END
GO

IF OBJECT_ID('dbo.BITACORA_LISTAR_USUARIOS_DISTINCT', 'P') IS NOT NULL DROP PROCEDURE [dbo].[BITACORA_LISTAR_USUARIOS_DISTINCT]
GO
CREATE PROCEDURE [dbo].[BITACORA_LISTAR_USUARIOS_DISTINCT]
AS
    SELECT DISTINCT USUARIO FROM BITACORA ORDER BY USUARIO
GO

IF OBJECT_ID('dbo.BITACORA_LISTAR_ACCIONES_DISTINCT', 'P') IS NOT NULL DROP PROCEDURE [dbo].[BITACORA_LISTAR_ACCIONES_DISTINCT]
GO
CREATE PROCEDURE [dbo].[BITACORA_LISTAR_ACCIONES_DISTINCT]
AS
    SELECT DISTINCT ACCION FROM BITACORA ORDER BY ACCION
GO

-- ------------------------------------------------------------
-- USUARIO_HISTORIAL — paginado por usuario
-- ------------------------------------------------------------

IF OBJECT_ID('dbo.USUARIO_HISTORIAL_LISTAR_PAGINADO', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_HISTORIAL_LISTAR_PAGINADO]
GO
CREATE PROCEDURE [dbo].[USUARIO_HISTORIAL_LISTAR_PAGINADO]
    @usuario_id INT,
    @pagina     INT = 1,
    @tamanio    INT = 25
AS
BEGIN
    IF @pagina  < 1 SET @pagina  = 1
    IF @tamanio < 1 SET @tamanio = 25

    SELECT ID, USUARIO_ID, USUARIO_LOGIN, ROL, ROL_ID, BLOQUEADO, INTENTOS_FALLIDOS,
           PERFILES, FECHA_CAMBIO, REALIZADO_POR, TIPO_CAMBIO, VERSION_ORIGEN,
           NOMBRE, APELLIDO, TELEFONO, EMAIL,
           COUNT(*) OVER() AS TOTAL_FILAS
    FROM USUARIO_HISTORIAL
    WHERE USUARIO_ID = @usuario_id
    ORDER BY ID DESC
    OFFSET (@pagina - 1) * @tamanio ROWS
    FETCH NEXT @tamanio ROWS ONLY
END
GO

-- ------------------------------------------------------------
-- USUARIO — paginado del listado general de administración
-- ------------------------------------------------------------

IF OBJECT_ID('dbo.USUARIO_LISTAR_PAGINADO', 'P') IS NOT NULL DROP PROCEDURE [dbo].[USUARIO_LISTAR_PAGINADO]
GO
CREATE PROCEDURE [dbo].[USUARIO_LISTAR_PAGINADO]
    @busqueda VARCHAR(100) = NULL,
    @pagina   INT          = 1,
    @tamanio  INT          = 25
AS
BEGIN
    IF @pagina  < 1 SET @pagina  = 1
    IF @tamanio < 1 SET @tamanio = 25

    SELECT u.ID, u.USUARIO, u.ROL, u.ROL_ID, r.NOMBRE AS ROL_NOMBRE, u.BLOQUEADO,
           u.NOMBRE, u.APELLIDO, u.TELEFONO, u.EMAIL,
           COUNT(*) OVER() AS TOTAL_FILAS
    FROM USUARIO u
    LEFT JOIN ROL r ON r.ID = u.ROL_ID
    WHERE (@busqueda IS NULL
           OR u.USUARIO  LIKE '%' + @busqueda + '%'
           OR u.NOMBRE   LIKE '%' + @busqueda + '%'
           OR u.APELLIDO LIKE '%' + @busqueda + '%')
    ORDER BY u.USUARIO
    OFFSET (@pagina - 1) * @tamanio ROWS
    FETCH NEXT @tamanio ROWS ONLY
END
GO