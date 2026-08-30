/* =====================================================================
   Limpieza de permisos duplicados en NODO_PERMISO causados por una
   mala codificacion (mojibake) de tildes/enie en una ejecucion previa
   de script.sql.

   Para evitar volver a introducir el problema, este script NO usa
   caracteres acentuados literales: los construye con NCHAR() a partir
   de los codepoints Unicode, asi el resultado es independiente de la
   codificacion con la que se abra/ejecute este archivo.

   Por cada par (version mojibake, version correcta):
     - si existen ambas filas: reasigna referencias en ROL_PERMISO y
       PADRE_ID hacia la version correcta, y borra la fila mojibake.
     - si solo existe la version mojibake: la renombra a la correcta.

   Ejecutar una sola vez contra la base BDCAPAS.
   ===================================================================== */

SET NOCOUNT ON;

PRINT '--- Filas con posible mojibake antes de la limpieza ---';
SELECT ID, NOMBRE, TIPO, PADRE_ID FROM NODO_PERMISO WHERE NOMBRE LIKE '%' + NCHAR(195) + '%';

DECLARE @Mapeo TABLE (NombreMojibake NVARCHAR(100), NombreCorrecto NVARCHAR(100), Tipo VARCHAR(10));

INSERT INTO @Mapeo (NombreMojibake, NombreCorrecto, Tipo) VALUES
    -- "Ver bitácora"
    (N'Ver bit' + NCHAR(195) + NCHAR(161) + N'cora',
     N'Ver bit' + NCHAR(225) + N'cora',
     'PERMISO'),
    -- "Gestión de roles"
    (N'Gesti' + NCHAR(195) + NCHAR(179) + N'n de roles',
     N'Gesti' + NCHAR(243) + N'n de roles',
     'PERMISO'),
    -- "Gestión de idiomas"
    (N'Gesti' + NCHAR(195) + NCHAR(179) + N'n de idiomas',
     N'Gesti' + NCHAR(243) + N'n de idiomas',
     'PERMISO'),
    -- "Cambiar contraseña"
    (N'Cambiar contrase' + NCHAR(195) + NCHAR(177) + N'a',
     N'Cambiar contrase' + NCHAR(241) + N'a',
     'PERMISO');

DECLARE @NombreMoji NVARCHAR(100), @NombreCorr NVARCHAR(100), @Tipo VARCHAR(10);
DECLARE @MojibakeId INT, @CorrectoId INT;

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT NombreMojibake, NombreCorrecto, Tipo FROM @Mapeo;

OPEN cur;
FETCH NEXT FROM cur INTO @NombreMoji, @NombreCorr, @Tipo;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @MojibakeId = NULL;
    SET @CorrectoId = NULL;

    SELECT @MojibakeId = ID FROM NODO_PERMISO WHERE NOMBRE = @NombreMoji AND TIPO = @Tipo AND PADRE_ID IS NULL;
    SELECT @CorrectoId = ID FROM NODO_PERMISO WHERE NOMBRE = @NombreCorr AND TIPO = @Tipo AND PADRE_ID IS NULL;

    IF @MojibakeId IS NOT NULL AND @CorrectoId IS NOT NULL AND @MojibakeId <> @CorrectoId
    BEGIN
        PRINT 'Migrando ID ' + CAST(@MojibakeId AS VARCHAR) + ' -> ID ' + CAST(@CorrectoId AS VARCHAR);

        -- Reasignar referencias como PERMISO_ID en ROL_PERMISO (evitando choque con la PK compuesta)
        UPDATE rp SET PERMISO_ID = @CorrectoId
        FROM ROL_PERMISO rp
        WHERE rp.PERMISO_ID = @MojibakeId
          AND NOT EXISTS (SELECT 1 FROM ROL_PERMISO rp2 WHERE rp2.ROL_ID = rp.ROL_ID AND rp2.PERMISO_ID = @CorrectoId);
        DELETE FROM ROL_PERMISO WHERE PERMISO_ID = @MojibakeId;

        -- Reasignar referencias como ROL_ID en ROL_PERMISO
        UPDATE rp SET ROL_ID = @CorrectoId
        FROM ROL_PERMISO rp
        WHERE rp.ROL_ID = @MojibakeId
          AND NOT EXISTS (SELECT 1 FROM ROL_PERMISO rp2 WHERE rp2.ROL_ID = @CorrectoId AND rp2.PERMISO_ID = rp.PERMISO_ID);
        DELETE FROM ROL_PERMISO WHERE ROL_ID = @MojibakeId;

        -- Reasignar hijos que cuelguen del nodo mojibake
        UPDATE NODO_PERMISO SET PADRE_ID = @CorrectoId WHERE PADRE_ID = @MojibakeId;

        -- Eliminar el nodo duplicado
        DELETE FROM NODO_PERMISO WHERE ID = @MojibakeId;
    END
    ELSE IF @MojibakeId IS NOT NULL AND @CorrectoId IS NULL
    BEGIN
        PRINT 'Renombrando ID ' + CAST(@MojibakeId AS VARCHAR) + ' a la version correcta';
        UPDATE NODO_PERMISO SET NOMBRE = @NombreCorr WHERE ID = @MojibakeId;
    END

    FETCH NEXT FROM cur INTO @NombreMoji, @NombreCorr, @Tipo;
END

CLOSE cur;
DEALLOCATE cur;

PRINT '--- Filas con posible mojibake despues de la limpieza (deberia ser 0) ---';
SELECT ID, NOMBRE, TIPO, PADRE_ID FROM NODO_PERMISO WHERE NOMBRE LIKE '%' + NCHAR(195) + '%';
