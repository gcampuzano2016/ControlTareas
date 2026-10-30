/* ============================================================================
   Perfil 41: nombre en dbo.Perfiles
   ReporTarea  |  2026-10-29
   41 no existia en dbo.Perfiles (solo en R_Usuarios.Id_Perfil). Su unico
   usuario es la cuenta medicolaboral, del medico externo (no es colaborador:
   no tiene ficha de vacaciones). Se crea con IdPerfiles = Codigo = 41.
   No cambia lo que ve: el menu va por PerfilMenu y R_UsuarioMenu.
   Efecto: IDENTITY_INSERT con 41 sube el contador de 25 a 41. No se vuelve
   atras con CHECKIDENT: al llegar a 41 chocaria con este perfil.
   Repetible. Reversa al final, comentada.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* "Medico" con tilde via NCHAR(233): el archivo va sin tildes (ver 2026-10-22-bonos-menu.sql). */
DECLARE @Nombre VARCHAR(150) = CONVERT(VARCHAR(150), N'M' + NCHAR(233) + N'dico Laboral');

IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = 41)
BEGIN
    SET IDENTITY_INSERT dbo.Perfiles ON;
    INSERT INTO dbo.Perfiles (IdPerfiles, Codigo, NombrePerfil, Estado, Fecha)
    VALUES (41, 41, @Nombre, 1, CONVERT(DATE, GETDATE()));
    SET IDENTITY_INSERT dbo.Perfiles OFF;
END

COMMIT TRANSACTION;

SELECT IdPerfiles, Codigo, NombrePerfil, UNICODE(SUBSTRING(NombrePerfil, 2, 1)) AS CodLetra2, Estado, Fecha
  FROM dbo.Perfiles WHERE IdPerfiles = 41;
SELECT IDENT_CURRENT('dbo.Perfiles') AS IdentActual;   -- queda en 41: el proximo perfil nuevo sera el 42 (el hueco 26-40 no molesta)
GO

/* ------------------------------------------------------------ REVERSA ------
DELETE FROM dbo.Perfiles WHERE IdPerfiles = 41;
---------------------------------------------------------------------------- */
