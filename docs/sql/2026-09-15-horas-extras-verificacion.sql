/* ============================================================================
   Verificacion de la carga del modulo de Horas Extras.

   Solo cuenta. No muestra ni un nombre, ni una cedula, ni un sueldo: este
   archivo si se versiona y el repositorio es publico. Para saber si la carga
   quedo bien basta con los numeros.
   ============================================================================ */

SET NOCOUNT ON;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Estos numeros son los que imprimio el generador (generar-carga-horas-extras.py)
   al correr contra la plantilla vigente el 2026-09-15. Si la plantilla cambia
   -entra o sale un colaborador, se agrega un sueldo, RRHH llena un divisor
   manual- hay que actualizarlos aqui a mano. Si no se actualizan, las
   etiquetas de abajo van a afirmar un numero que ya no es cierto sin que la
   consulta falle: por eso viven en un solo bloque y no repartidos en el
   texto de cada etiqueta. */
DECLARE @EsperadoColaboradores         INT = 64;
DECLARE @EsperadoSueldos               INT = 69;
DECLARE @EsperadoPersonasConDosSueldos INT = 5;
DECLARE @EsperadoSinSueldo             INT = 0;
DECLARE @EsperadoJornada8              INT = 56;
DECLARE @EsperadoJornada4              INT = 8;
DECLARE @EsperadoNoAplicaHE            INT = 3;
DECLARE @EsperadoConDivisorManual      INT = 0;
DECLARE @EsperadoParametrosVigentes    INT = 7;
DECLARE @EsperadoNoExistenEnEmpleados  INT = 0;

SELECT Comprobacion = 'colaboradores cargados (esperado ' + CONVERT(VARCHAR(12), @EsperadoColaboradores) + ')',
       Valor        = CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_ColaboradorParametro
UNION ALL
SELECT 'sueldos cargados (esperado ' + CONVERT(VARCHAR(12), @EsperadoSueldos) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario
UNION ALL
SELECT 'personas con mas de un sueldo (esperado ' + CONVERT(VARCHAR(12), @EsperadoPersonasConDosSueldos) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM (
    SELECT IdEmpleado FROM dbo.HE_Salario GROUP BY IdEmpleado HAVING COUNT(*) > 1) x
UNION ALL
SELECT 'colaboradores sin ningun sueldo (esperado ' + CONVERT(VARCHAR(12), @EsperadoSinSueldo) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro c
 WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Salario s WHERE s.IdEmpleado = c.IdEmpleado)
UNION ALL
SELECT 'jornada de 8 h (esperado ' + CONVERT(VARCHAR(12), @EsperadoJornada8) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE JornadaHorasDia = 8
UNION ALL
SELECT 'jornada de 4 h (esperado ' + CONVERT(VARCHAR(12), @EsperadoJornada4) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE JornadaHorasDia = 4
UNION ALL
SELECT 'no aplican horas extras (esperado ' + CONVERT(VARCHAR(12), @EsperadoNoAplicaHE) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE AplicaHE = 0
UNION ALL
SELECT 'con divisor manual (esperado ' + CONVERT(VARCHAR(12), @EsperadoConDivisorManual) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro WHERE DivisorManual IS NOT NULL
UNION ALL
SELECT 'parametros vigentes (esperado ' + CONVERT(VARCHAR(12), @EsperadoParametrosVigentes) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_Parametro WHERE FechaVigenciaHasta IS NULL
UNION ALL
SELECT 'colaboradores que no existen en Empleados (esperado ' + CONVERT(VARCHAR(12), @EsperadoNoExistenEnEmpleados) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM dbo.HE_ColaboradorParametro c
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE e.IdEmpleado = c.IdEmpleado);

/* La jornada que declara la plantilla contra la que el sistema ya conoce.
   58 de los 64 tienen horario asignado. Esto NO corrige nada: reporta para que
   RRHH lo mire, porque el Excel no es autoridad sobre el horario de nadie. */
SELECT Comprobacion = 'colaboradores con horario asignado en el sistema',
       Valor        = CONVERT(VARCHAR(12), COUNT(DISTINCT c.IdEmpleado))
  FROM dbo.HE_ColaboradorParametro c
  JOIN dbo.Empleados e ON e.IdEmpleado = c.IdEmpleado
  JOIN dbo.R_UsuarioHorarioLaboral uh ON uh.Id_Responsable = e.Cod_Usuario AND uh.Activo = 1;
GO
