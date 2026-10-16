/* ============================================================================
   Equipo y agenda (Entrega 1) - precarga de areas
   ReporTarea  |  2026-10-15
   Requiere 2026-10-15-agenda-esquema.sql.
   1. Un area por cada Departamento distinto entre las personas activas, con
      las variantes que solo difieren en tildes o mayusculas unificadas
      (ADMINISTRACION / ADMINISTRACION con tilde). Colores por defecto.
   2. A cada persona activa con Departamento, su area. Las que no tienen
      Departamento quedan sin fila: caen en "Sin area".
   Repetible: no crea areas que ya existen (comparando sin tildes ni
   mayusculas) ni pisa el area que una persona ya tenga.
   No modifica R_Usuarios.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

/* El chequeo va en el MISMO lote que la carga: un RETURN solo corta su lote. */
IF NOT EXISTS (SELECT 1 FROM dbo.AG_Area WHERE EsSinArea = 1)
BEGIN
    RAISERROR('Falta correr 2026-10-15-agenda-esquema.sql.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

DECLARE @Colores TABLE (Orden INT PRIMARY KEY, Color CHAR(7));
INSERT INTO @Colores (Orden, Color) VALUES
 (1, '#0078D4'), (2, '#107C10'), (3, '#CA5010'), (4, '#8764B8'), (5, '#E3008C'), (6, '#00B7C3'),
 (7, '#D13438'), (8, '#5C2D91'), (9, '#8E8CD8'), (10, '#498205'), (11, '#986F0B'), (12, '#004E8C');

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A'
       AND u.EstadoUsuario IS NULL
       AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
),
Deptos AS
(
    SELECT Nombre   = MAX(Depto),
           Personas = COUNT(*)
      FROM Activos
     WHERE Depto <> ''
     GROUP BY UPPER(Depto) COLLATE Latin1_General_CI_AI
),
Nuevos AS
(
    SELECT d.Nombre, d.Personas,
           Fila = ROW_NUMBER() OVER (ORDER BY d.Personas DESC, d.Nombre)
      FROM Deptos AS d
     WHERE NOT EXISTS (SELECT 1 FROM dbo.AG_Area AS a
                        WHERE a.Nombre COLLATE Latin1_General_CI_AI = d.Nombre COLLATE Latin1_General_CI_AI)
)
INSERT INTO dbo.AG_Area (Nombre, Color, Activa, EsSinArea, Usuario_Registro)
SELECT n.Nombre, c.Color, 1, 0, 'MIGRACION'
  FROM Nuevos AS n
  JOIN @Colores AS c ON c.Orden = ((n.Fila - 1) % 12) + 1;

PRINT 'Areas creadas: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, ''))),
           Orden = ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(u.Cod_Usuario)) ORDER BY u.Id_Usuario DESC)
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A'
       AND u.EstadoUsuario IS NULL
       AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
INSERT INTO dbo.AG_PersonaArea (Cod_Usuario, IdArea, Usuario_Registro)
SELECT x.Cod, a.IdArea, 'MIGRACION'
  FROM Activos AS x
  JOIN dbo.AG_Area AS a
    ON a.EsSinArea = 0
   AND a.Nombre COLLATE Latin1_General_CI_AI = x.Depto COLLATE Latin1_General_CI_AI
 WHERE x.Orden = 1
   AND x.Depto <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);

PRINT 'Personas asignadas: ' + CONVERT(VARCHAR(10), @@ROWCOUNT);

COMMIT TRANSACTION;
GO

/* --------------------------------------------------------- resumen -------- */
SELECT a.Nombre, a.Color, a.Activa, a.EsSinArea, Personas = COUNT(pa.Cod_Usuario)
  FROM dbo.AG_Area AS a
  LEFT JOIN dbo.AG_PersonaArea AS pa ON pa.IdArea = a.IdArea
 GROUP BY a.Nombre, a.Color, a.Activa, a.EsSinArea
 ORDER BY a.EsSinArea, Personas DESC;

;WITH Activos AS
(
    SELECT Cod   = LTRIM(RTRIM(u.Cod_Usuario)),
           Depto = LTRIM(RTRIM(ISNULL(u.Departamento, '')))
      FROM dbo.R_Usuarios AS u
     WHERE u.Usuario_Estado = 'A' AND u.EstadoUsuario IS NULL AND ISNULL(u.IdCliente, 0) = 0
       AND LTRIM(RTRIM(ISNULL(u.Cod_Usuario, ''))) <> ''
)
SELECT Veredicto = CASE WHEN COUNT(*) = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END,
       ConDepartamentoSinArea = COUNT(*)
  FROM Activos AS x
 WHERE x.Depto <> ''
   AND NOT EXISTS (SELECT 1 FROM dbo.AG_PersonaArea AS pa WHERE pa.Cod_Usuario = x.Cod);
GO
