/* ============================================================================
   Perfiles 4 y 21: nombre en dbo.Perfiles
   ReporTarea  |  2026-10-29
   - 4 se llamaba "falta". Sus activos: JEFE DE CAS y COORDINADOR GPF.
   - 21 no existia en dbo.Perfiles (solo en R_Usuarios.Id_Perfil). Su activo:
     ASISTENTE ADMINISTRATIVO GTH. Se crea con IdPerfiles = Codigo = 21, para
     que coincida con lo que ya tienen los usuarios.
   Ninguno cambia lo que ven: el menu va por PerfilMenu, no por esta tabla.
   El 5 tambien se llama "falta", pero no tiene usuarios activos: no se toca.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

UPDATE dbo.Perfiles SET NombrePerfil = 'Jefatura'
 WHERE IdPerfiles = 4 AND ISNULL(NombrePerfil, '') <> 'Jefatura';

IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = 21)
BEGIN
    SET IDENTITY_INSERT dbo.Perfiles ON;
    INSERT INTO dbo.Perfiles (IdPerfiles, Codigo, NombrePerfil, Estado, Fecha)
    VALUES (21, 21, 'Asistente Administrativo GTH', 1, CONVERT(DATE, GETDATE()));
    SET IDENTITY_INSERT dbo.Perfiles OFF;
END

COMMIT TRANSACTION;

SELECT IdPerfiles, Codigo, NombrePerfil, Estado, Fecha
  FROM dbo.Perfiles WHERE IdPerfiles IN (4, 21) ORDER BY IdPerfiles;
SELECT IDENT_CURRENT('dbo.Perfiles') AS IdentActual;   -- debe seguir en 25
GO

/* ------------------------------------------------------------ REVERSA ------
UPDATE dbo.Perfiles SET NombrePerfil = 'falta' WHERE IdPerfiles = 4;
DELETE FROM dbo.Perfiles WHERE IdPerfiles = 21;
---------------------------------------------------------------------------- */
