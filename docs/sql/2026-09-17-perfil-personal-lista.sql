/* ============================================================================
   Perfil del colaborador: el listado de personal para Talento Humano
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Es el paso 1 de la entrega 2; el orden completo esta
   en DESPLIEGUE.md.

   ----------------------------------------------------------------------------
   Que hace y por que

   Sp_RTA_PerfilEquipoLista devuelve el equipo directo de una jefatura. Este
   procedimiento es el mismo SELECT sin el filtro por Cod_Jefe_Inm: el personal
   activo entero, para que Talento Humano busque a cualquiera.

   Quien puede llamarlo NO se decide aca. El procedimiento no conoce perfiles;
   la barrera esta en AdministrarPerfil.ashx.cs, que solo expone la accion a los
   perfiles 14 y 18. Mismo reparto que el resto del modulo: SQL hace, C# decide.

   Conserva a proposito dos cosas del original:

   1. NO lista a quien tiene el Cod_Usuario repetido entre usuarios activos.
      Sp_RTA_PerfilColaborador se niega a abrir esos perfiles -no puede saber de
      cual de dos personas son los datos-, asi que listarlos seria ofrecer un
      boton que nunca funciona. Son 4 usuarios; necesitan que alguien les
      corrija el codigo en R_Usuarios, y eso no se arregla desde esta pantalla.

   2. LTRIM(RTRIM(...)) en las comparaciones. Los codigos traen relleno.

   El filtro es obligatorio y de 2 caracteres como minimo. Eso lo impone
   NegPerfilCampos.ValidarFiltroPersonal antes de llegar aca; el WHERE de abajo
   lo repite por si alguien llama al procedimiento a mano.

   Idempotente: DROP + CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '== Perfil: listado de personal - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_PerfilPersonalLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilPersonalLista;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilPersonalLista
    @Filtro VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @F VARCHAR(100) = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    /* Sin filtro no se devuelve nada. La regla de los 2 caracteres vive en
       NegPerfilCampos.ValidarFiltroPersonal, que es donde se puede probar; esto
       es la red por si alguien llega al procedimiento por otro camino. */
    IF LEN(@F) < 2
    BEGIN
        SELECT  CodUsuario     = CAST(NULL AS VARCHAR(50)),
                NombreCompleto = CAST(NULL AS VARCHAR(200)),
                Cargo          = CAST(NULL AS VARCHAR(200)),
                Area           = CAST(NULL AS VARCHAR(200)),
                Ciudad         = CAST(NULL AS VARCHAR(200))
         WHERE 1 = 0;
        RETURN;
    END

    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  ISNULL(u.EstadoUsuario, 0) = 0

       /* Misma regla que la lista de equipo: si el codigo esta repetido entre
          usuarios activos, Sp_RTA_PerfilColaborador se niega a abrir ese perfil.
          Mejor no listarlo que listarlo con un boton que nunca funciona. */
       AND  (SELECT COUNT(*) FROM dbo.R_Usuarios r
              WHERE LTRIM(RTRIM(r.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Usuario))
                AND ISNULL(r.EstadoUsuario,0) = 0) = 1

       AND  (ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario) LIKE '%' + @F + '%'
             OR LTRIM(RTRIM(u.Cod_Usuario)) LIKE '%' + @F + '%'
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo) LIKE '%' + @F + '%')
     ORDER BY NombreCompleto;
END
GO
PRINT 'Sp_RTA_PerfilPersonalLista creado.';
GO

PRINT '== Perfil: listado de personal - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   1. Un filtro corto no devuelve nada:
        EXEC dbo.Sp_RTA_PerfilPersonalLista @Filtro = 'a';
      Esperado: 0 filas.

   2. Un filtro normal devuelve personal activo:
        EXEC dbo.Sp_RTA_PerfilPersonalLista @Filtro = 'ar';
      Esperado: filas ordenadas por nombre, ninguna con Cod_Usuario repetido.

   3. Los codigos repetidos quedan fuera. Esta consulta los nombra:
        SELECT Cod_Usuario, COUNT(*) AS Veces
          FROM dbo.R_Usuarios
         WHERE ISNULL(EstadoUsuario,0) = 0
         GROUP BY Cod_Usuario
        HAVING COUNT(*) > 1;
      Ninguno de esos codigos puede aparecer en el punto 2. Son los que hay que
      corregir a mano en R_Usuarios; Talento Humano tiene que saberlo, o va a
      buscar a esas personas y no las va a encontrar.
   ============================================================================ */
