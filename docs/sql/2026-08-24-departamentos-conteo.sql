/* ============================================================================
   Departamentos reales de R_Usuarios, para decidir si se unifican.

   Solo lectura: no crea, no modifica, no borra nada. Se puede correr en
   produccion sin riesgo.

   Para que sirve: el combo de la pantalla de usuarios se arma con los
   departamentos que ya existen en los datos, y ahi conviven variantes del
   mismo departamento escritas distinto (GTH y GTH-, ADMINISTRACION y
   ADMINISTRATIVO). Antes de descomentar el UPDATE de normalizacion que trae
   2026-08-24-usuarios-combos-y-telefonos-emergencia.sql hay que ver esto:
   cuantas variantes hay de verdad y cuantos usuarios cuelgan de cada una.

   Como leerlo: las variantes de un mismo departamento salen juntas porque el
   orden es alfabetico. La que tiene mas usuarios suele ser la buena; la de
   uno o dos, el error de tipeo. Ojo con las que difieren solo en espacios o
   en mayusculas: Largo y Diferencias las delatan.

   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;

SELECT
    Departamento = ISNULL(u.Departamento, '(NULL)'),
    Usuarios     = COUNT(*),
    /* Si Largo no coincide con el largo del texto visible, sobran espacios. */
    Largo        = MAX(LEN(ISNULL(u.Departamento, ''))),
    LargoConEspacios = MAX(DATALENGTH(ISNULL(u.Departamento, ''))),
    Ejemplo      = MIN(u.Nom_Usuario)
FROM dbo.R_Usuarios u
GROUP BY u.Departamento
ORDER BY ISNULL(u.Departamento, '(NULL)');

/* Lo mismo agrupado sin distinguir espacios de sobra, para ver si dos filas
   de la consulta anterior son en realidad el mismo departamento. */
SELECT
    Departamento = LTRIM(RTRIM(ISNULL(u.Departamento, ''))),
    Usuarios     = COUNT(*),
    Variantes    = COUNT(DISTINCT u.Departamento)
FROM dbo.R_Usuarios u
GROUP BY LTRIM(RTRIM(ISNULL(u.Departamento, '')))
HAVING COUNT(DISTINCT u.Departamento) > 1
ORDER BY 1;
