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
DECLARE @EsperadoAjustes               INT = 5;
DECLARE @EsperadoRol                   INT = 64;
DECLARE @EsperadoMontoNoPositivo       INT = 0;
DECLARE @EsperadoConObservacion        INT = 69;
DECLARE @EsperadoConHorarioAsignado    INT = 58;

/* Origen paso a decidir cuanto cobran 5 personas: el desempate de
   NegHorasExtras.SalarioVigente da prioridad al ajuste sobre el rol cuando
   dos filas comparten fecha de vigencia. Esta fecha de corte no es un
   parametro del sistema, es solo el punto donde se comprueba el desempate:
   con todos los sueldos vigentes desde el 2026-09-01, cualquier fecha
   posterior sirve. */
DECLARE @FechaCorte                    DATE = '2026-09-30';
DECLARE @EsperadoConSueldoVigente      INT = 64;
DECLARE @EsperadoVigenteEsAjuste       INT = 5;
DECLARE @EsperadoEmpateDeFechas        INT = 5;

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
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE e.IdEmpleado = c.IdEmpleado)
UNION ALL
/* Estas cuatro no existian en la ronda anterior: las diez comprobaciones de
   arriba no miran Origen, y si esa columna llegara vacia el generador la
   trataria como Rol y las diez seguirian dando su numero esperado. */
SELECT 'sueldos con Origen = Ajuste (esperado ' + CONVERT(VARCHAR(12), @EsperadoAjustes) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario WHERE Origen = 'Ajuste'
UNION ALL
SELECT 'sueldos con Origen = Rol (esperado ' + CONVERT(VARCHAR(12), @EsperadoRol) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario WHERE Origen = 'Rol'
UNION ALL
SELECT 'sueldos con Monto no positivo (esperado ' + CONVERT(VARCHAR(12), @EsperadoMontoNoPositivo) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario WHERE Monto <= 0
UNION ALL
SELECT 'sueldos con Observacion informada (esperado ' + CONVERT(VARCHAR(12), @EsperadoConObservacion) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM dbo.HE_Salario WHERE Observacion IS NOT NULL
UNION ALL
/* Esta comprobacion existe para que la del desempate, mas abajo, no se
   vuelva una prueba vacia. Esa consulta ordena por fecha DESC y solo
   despues por Origen: si los ajustes llegaran con fecha POSTERIOR al rol
   en vez de la misma, la fecha resolveria sola y el desempate por Origen
   no se ejercitaria nunca -pero la consulta seguiria devolviendo 64 y 5,
   aparentando que probo algo-. Este conteo afirma que el empate existe.
   Si algun dia baja de 5, la comprobacion del desempate dejo de tener
   sentido y hay que revisarla, no ignorarla. */
SELECT 'empleados con dos sueldos en la MISMA fecha de vigencia (esperado '
       + CONVERT(VARCHAR(12), @EsperadoEmpateDeFechas) + ')',
       CONVERT(VARCHAR(12), COUNT(*)) FROM (
    SELECT IdEmpleado, FechaVigenciaDesde FROM dbo.HE_Salario
     GROUP BY IdEmpleado, FechaVigenciaDesde HAVING COUNT(*) > 1) y;

/* La jornada que declara la plantilla contra la que el sistema ya conoce.
   Esto NO corrige nada: reporta para que RRHH lo mire, porque el Excel no es
   autoridad sobre el horario de nadie. */
SELECT Comprobacion = 'colaboradores con horario asignado en el sistema (esperado '
                       + CONVERT(VARCHAR(12), @EsperadoConHorarioAsignado) + ' de '
                       + CONVERT(VARCHAR(12), @EsperadoColaboradores) + ')',
       Valor        = CONVERT(VARCHAR(12), COUNT(DISTINCT c.IdEmpleado))
  FROM dbo.HE_ColaboradorParametro c
  JOIN dbo.Empleados e ON e.IdEmpleado = c.IdEmpleado
  JOIN dbo.R_UsuarioHorarioLaboral uh ON uh.Id_Responsable = e.Cod_Usuario AND uh.Activo = 1;

/* La comprobacion mas importante de todas: es el sustituto de la validacion
   de corte del 9.5 funcional, bloqueada porque RRHH no ha entregado
   Calculadora_Horas_Extras_50_100.xlsx. Mientras ese archivo no llegue, esta
   es la unica prueba de que el desempate de salario funciona sobre datos
   reales. Replica en SQL la misma regla que NegHorasExtras.SalarioVigente
   aplica en C#: la fecha de vigencia mas reciente que no sea posterior al
   corte, y a igual fecha gana Ajuste sobre Rol. IdSalario DESC es un tercer
   desempate defensivo para dos filas identicas en fecha Y origen, que hoy no
   ocurre en los datos pero que la consulta no debe asumir.

   Solo cuenta, nunca un monto: este archivo se versiona y el repositorio es
   publico. */
;WITH Vigente AS (
    SELECT h.IdEmpleado, h.Monto, h.Origen,
           ROW_NUMBER() OVER (PARTITION BY h.IdEmpleado
                               ORDER BY h.FechaVigenciaDesde DESC,
                                        CASE WHEN h.Origen = 'Ajuste' THEN 1 ELSE 0 END DESC,
                                        h.IdSalario DESC) AS Orden
      FROM dbo.HE_Salario h
     WHERE h.FechaVigenciaDesde <= @FechaCorte
)
SELECT Comprobacion = 'empleados con sueldo vigente mayor que cero al '
                       + CONVERT(VARCHAR(12), @FechaCorte, 23) + ' (esperado '
                       + CONVERT(VARCHAR(12), @EsperadoConSueldoVigente) + ')',
       Valor        = CONVERT(VARCHAR(12), COUNT(*))
  FROM Vigente WHERE Orden = 1 AND Monto > 0
UNION ALL
SELECT 'empleados cuyo sueldo vigente al ' + CONVERT(VARCHAR(12), @FechaCorte, 23)
       + ' es el del ajuste (esperado ' + CONVERT(VARCHAR(12), @EsperadoVigenteEsAjuste) + ')',
       CONVERT(VARCHAR(12), COUNT(*))
  FROM Vigente WHERE Orden = 1 AND Origen = 'Ajuste';
GO
