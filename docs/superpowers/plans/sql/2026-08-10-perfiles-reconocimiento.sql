SET QUOTED_IDENTIFIER ON;
USE ReporTarea;
GO

-- 1. Columnas reales de dbo.Perfiles
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Perfiles'
ORDER BY ORDINAL_POSITION;

-- 2. Como se llama de verdad la tabla de pagina de inicio por perfil
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%PerfilInicio%' OR TABLE_NAME LIKE '%Inicio%';

-- 4. Claves foraneas hacia dbo.Perfiles
SELECT OBJECT_NAME(fk.parent_object_id) AS TablaQueReferencia,
       COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnaQueReferencia
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'Perfiles';

-- 5. Toda tabla que guarde un id de perfil
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('IdPerfil', 'Id_Perfil', 'IdPerfiles', 'Perfil', 'Cod_Perfil')
ORDER BY TABLE_NAME;

/* ============================================================================
   RESULTADOS — ejecutado el 2026-08-10 contra 192.168.11.14 / ReporTarea
   ----------------------------------------------------------------------------
   1. dbo.Perfiles:
        IdPerfiles    bigint       NOT NULL   <-- BIGINT, no int
        Codigo        int          NOT NULL   <-- NOT NULL, nadie lo fija hoy
        NombrePerfil  varchar(150) NULL       <-- 150, no 100
        Estado        int          NULL       <-- numerico, no 'A'/'I'
        Fecha         date         NULL

   2. La tabla de pagina de inicio se llama RTA_PerfilInicio, NO PerfilInicio.
      Columnas: IdPerfil int, Href varchar, IdTipo int, Estado bit,
                UsuarioRegistro varchar, FechaRegistro datetime

   3. Claves foraneas hacia dbo.Perfiles: NINGUNA (0 filas).
      No hay red de seguridad por integridad referencial: un DELETE que olvide
      una tabla NO falla, borra en silencio.

   4. Tablas que guardan un id de perfil:
        Perfiles.IdPerfiles         (el catalogo)
        PerfilMenu.IdPerfil         <- contemplado
        RTA_PerfilInicio.IdPerfil   <- contemplado (con el nombre corregido)
        R_Usuarios.Id_Perfil        <- contemplado
        R_Usuarios.Cod_Perfil       <- CUARTA REFERENCIA, no estaba en el spec
        R_Perfil.Id_Perfil          <- otro catalogo (R_Perfil), no aplica
        R_PerfilRol.Id_Perfil       <- cuelga de R_Perfil, no aplica

   5. Sobre R_Usuarios.Cod_Perfil:
      IdPerfiles y Codigo valen lo mismo en dbo.Perfiles, pero los usuarios
      usan las dos columnas de forma inconsistente: de 245 usuarios, 225
      tienen Cod_Perfil <> Id_Perfil.
      Ejemplo: el perfil 1 tiene 141 usuarios por Id_Perfil y 17 por Codigo.

      HOY ningun perfil quedaria en riesgo (la consulta de perfiles con 0
      usuarios por Id_Perfil y >0 por Codigo devuelve 0 filas), pero eso puede
      cambiar. DECISION: Sp_RTA_EliminarPerfil cuenta por AMBAS columnas.
      Es barato y cierra el riesgo de borrar un perfil que si estaba en uso.
   ============================================================================ */
