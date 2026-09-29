# Horas Extras: decisión por tramo y pago por minutos — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que Nómina decida tramo por tramo qué se paga (incluir o excluir, tipo 50/100, minutos reducidos con motivo), con el pago calculado por minutos exactos, para que el período 16-08 / 15-09-2026 cuadre al centavo con el análisis manual (6.241,14).

**Architecture:**
- **SQL:** un procedimiento de solo lectura entrega los tramos candidatos, las decisiones y los períodos. Otro inserta un lote de decisiones y valida el pago doble dentro de una transacción.
- **Cálculo:** una clase pura, `NegHeDecisiones`, resuelve las decisiones y calcula los minutos.
- **Pago:** `NegHorasExtras.Calcular` acepta minutos. La siembra (`AbrirPeriodo`) toma los minutos de las decisiones en lugar de `Sp_RTA_HeHorasAprobadas`.
- **Columnas «no incluido»:** salen de los mismos candidatos, reutilizando `NegHeFueraDeHorario`.

**Tech Stack:** ASP.NET WebForms (.NET 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, EPPlus 6, MSTest v1.

**Spec:** `docs/superpowers/specs/2026-09-29-horas-extras-decision-por-tramo-design.md`

## Global Constraints

- `HE_TramoDecision` **solo recibe INSERT.** Manda la decisión con el `IdDecision` más alto por `(IdPeriodo, Id_RegDetTareas, SegmentoInicio)`.
- **No se recrea ni se modifica ningún procedimiento existente.** Todo lo nuevo son procedimientos nuevos. `Sp_RTA_HeHorasAprobadas` y `Sp_RTA_HeFueraDeHorario` quedan sin uso, como vuelta atrás.
- **Pago por minutos:** `Total = redondeo(minutos / 60 × valor hora sin recortar, 2, AwayFromZero)`. Las horas guardadas en `HE_Detalle` son `redondeo(minutos / 60, 2)`, solo para mostrar.
- **Valor por omisión:** un aprobado dentro de su período dueño va **incluido**, con su tipo aprobado. Todo lo demás va **excluido**.
- **Dueño de un tramo:** el período cuyo rango contiene la fecha del tramo. Un segmento incluido en otro período se **rechaza** (código -2).
- **Ventana:** de `FechaInicio − 7` a `FechaFin + 7`, por `Det_Fch_RegDetalleIni`.
- **Perfiles que deciden:** `{14, 18}`. Se reutiliza `PerfilesQueEditanMaestro`.
- **Minutos reducidos:** de 1 hasta los minutos del segmento. Se toman **los primeros N minutos** del segmento.
- **Motivo obligatorio:** cuando se reducen minutos o se incluye algo que no está aprobado.
- **Período cerrado:** no admite decisiones (código -1).
- **Codificación:** los `.sql`, sin BOM ni tildes. `HorasExtras.aspx` y `horasExtras.js`, con BOM. La página pasa a `?v=11`.
- **Build** con el MSBuild de VS2019 desde PowerShell. **Pruebas** con `vstest.console.exe`. **Commits** solo con los archivos de la tarea.

## Review Focus

1. **Un tramo aprobado del 09 al 13-08** pertenece al período 15-07 / 13-08 y queda en la ventana del período 16-08. Por omisión va incluido en su dueño; incluirlo en el otro se rechaza. → Pruebas `Resolver_AprobadoDeOtroPeriodo_Bloqueado` y `Resolver_ExcluidoEnSuDuenio_SePuedeIncluirEnOtro`.
2. **Reducir minutos por encima de los del segmento**, o a 0 o menos: se rechaza. → `Validar_MinutosFueraDeRango`.
3. **Cambiar el tipo de un tramo aprobado** paga con el tipo nuevo y deja al jefe como aprobador: el estado del tramo no cambia. → `Minutos_TipoCambiado`.
4. **Doble clic o dos pestañas guardando decisiones distintas:** gana la última y ninguna se pierde del historial. El pago doble se valida bajo bloqueo. → procedimiento (Task 1) y Review.
5. **Observación guardada desde la grilla:** no cambia horas ni centavos. Vuelve a calcular desde las decisiones. → `GuardarHoras` (Task 4), comprobado a mano.

---

### Task 1: SQL

**Create:** `docs/sql/2026-09-29-horas-extras-decision-por-tramo.sql`

- **`HE_TramoDecision`**, creada solo si no existe, con estas columnas:
  - `IdDecision` INT IDENTITY, clave primaria;
  - `IdPeriodo` INT; `Id_RegDetTareas` BIGINT; `SegmentoInicio` DATETIME;
  - `Incluir` BIT; `TipoHora` INT, con `CHECK IN (1, 2)`; `MinutosIncluidos` INT NULL;
  - `Motivo` VARCHAR(400) NULL; `Usuario` VARCHAR(50); `Fecha` DATETIME2(0), con valor por omisión `SYSDATETIME()`; `Ip` VARCHAR(64).

  Índice `IX_HE_TramoDecision_Clave` sobre `(IdPeriodo, Id_RegDetTareas, SegmentoInicio, IdDecision DESC)`, y otro sobre `(Id_RegDetTareas, SegmentoInicio)`.
- **`Sp_RTA_HeTramosCandidatos(@IdPeriodo INT)`**, de solo lectura, con tres conjuntos:
  1. **Candidatos** del maestro activo en la ventana: `IdEmpleado, IdRegDetTareas, TramoInicio, TramoFin, SegmentoInicio, SegmentoFin, TipoHora, EstadoTramo, TipoRegistrado, Actividad`.
     - Los aprobados (estado 2, tipo 1 o 2) van como **un** segmento igual al tramo, con `TipoHora` = el tipo registrado.
     - El resto, con los segmentos de `FN_RTA_ClasificarTramosHorario` cuyo `TipoHora` sea 1 o 2.
  2. **Decisiones vigentes**, de cualquier período, sobre esos tramos: `IdPeriodo, IdRegDetTareas, SegmentoInicio, Incluir, TipoHora, MinutosIncluidos, Motivo, Usuario, Fecha`.
  3. **Períodos:** `IdPeriodo, FechaInicio, FechaFin, EstadoPeriodo`.
- **`Sp_RTA_HeGuardarDecisiones(@IdPeriodo INT, @Decisiones XML, @Usuario VARCHAR(50), @Ip VARCHAR(64))`**. Cada elemento del XML es `<d t="IdRegDetTareas" s="yyyy-MM-ddTHH:mm:ss" i="0|1" h="1|2" m="" r="motivo"/>`. Devuelve `Respuestas`:
  - `0`: guardado;
  - `-1`: el período no existe o no está abierto;
  - `-2`: un segmento con `i = 1` ya está incluido en otro período, por decisión vigente o por omisión en su dueño;
  - `-3`: un tipo, unos minutos o un motivo inválido;
  - `-4`: el tramo no existe o está fuera de la ventana.

  Todo va en **una sola** transacción, con `UPDLOCK, HOLDLOCK` sobre `HE_TramoDecision`. Las salidas tempranas hacen **COMMIT**, no ROLLBACK, por el error 3915.
- **Verificación con `@Fallos`:**
  - los objetos existen;
  - `Sp_RTA_HeTramosCandidatos` corre sobre el período más reciente;
  - `Sp_RTA_HeGuardarDecisiones` con un período inexistente devuelve -1;
  - un lote con `h = 3` sobre un período abierto devuelve -3 **sin escribir nada**.
- **Antes de commitear:** se compila con `SET NOEXEC ON`. El cuerpo del primer procedimiento se ejecuta como consulta suelta y tiene que devolver los candidatos del período 3.

### Task 2: entidades y resolución pura (TDD)

- **`EntHeCandidato`:** los campos del conjunto 1, más estos resueltos:
  - `DentroDelPeriodo`, `EsAprobado`, `Grupo` («Aprobado», «Sin solicitar», «Pendiente», «Aprobado sin tipo», «Rechazado»);
  - `Incluir`, `TipoElegido`, `MinutosIncluidos` (int?), `Motivo`, `DecididoPor`, `DecididoEl`;
  - `Bloqueado`, `BloqueadoPor` (texto con el rango del otro período);
  - los calculados `MinutosSegmento` y `MinutosEfectivos`.
- **`EntHeDecision`:** los campos del conjunto 2. **`EntHeRangoPeriodo`:** `IdPeriodo, FechaInicio, FechaFin, EstadoPeriodo`.
- **`NegHeDecisiones.Resolver(int idPeriodo, candidatos, decisiones, periodos)`:** aplica, por segmento:
  1. la decisión vigente de **este** período, si la hay; si no, el valor por omisión, que incluye solo si es aprobado y este período es su dueño;
  2. marca `Bloqueado` si **otro** período lo tiene incluido: por su decisión vigente, o porque es su dueño, el segmento es aprobado y ese período no tiene una decisión que lo excluya;
  3. un segmento bloqueado nunca queda incluido aquí.
- **`NegHeDecisiones.Minutos(resueltos)`:** devuelve `Dictionary<long, int[2]>` con los minutos 50 y 100 por persona. Une por minuto, gana el 100%, y toma los primeros N si se redujo.
- **`NegHeDecisiones.Validar(resueltos, cambios)`:** devuelve `null` o el mensaje del error. Revisa tipo 1 o 2, minutos entre 1 y `MinutosSegmento`, el motivo cuando corresponde, un segmento bloqueado que se quiere incluir, y un cambio sobre un segmento que no es candidato.
- **Pruebas (`NegHeDecisionesTests`):**
  - `Resolver_AprobadoDentro_IncluidoPorOmision`;
  - `Resolver_NoAprobado_ExcluidoPorOmision`;
  - `Resolver_DecisionVigenteGana` (gana la del `IdDecision` más alto);
  - `Resolver_AprobadoDeOtroPeriodo_Bloqueado`;
  - `Resolver_ExcluidoEnSuDuenio_SePuedeIncluirEnOtro`;
  - `Resolver_IncluidoEnOtroPeriodoPorDecision_Bloqueado`;
  - `Resolver_SinDuenio_SePuedeIncluir`;
  - `Minutos_UnionYCienGana`;
  - `Minutos_Reducidos_PrimerosN`;
  - `Minutos_TipoCambiado`;
  - `Validar_MinutosFueraDeRango`;
  - `Validar_MotivoObligatorio`;
  - `Validar_Bloqueado`;
  - `Validar_TipoInvalido`.

### Task 3: pago por minutos (TDD)

- `EntHeInsumo.Minutos50` y `Minutos100` (int?). Si vienen, `NegHorasExtras.Calcular` usa `minutos / 60m` **sin redondear** para los totales, y `TotalHoras = redondeo(minutos / 60, 2)`. Si no vienen, el comportamiento no cambia: las pruebas existentes siguen igual.
- `EntHeFila.Minutos50` y `Minutos100` (int?, no se guardan). `AplicarCalculo` los pasa al insumo.
- **Pruebas:**
  - `Calcular_ConMinutos_NoRedondeaAntesDeMultiplicar`: 660 min al 100% con sueldo 1850 da 169,58;
  - `Calcular_ConMinutos_HorasMostradasRedondeadas`;
  - `Calcular_SinMinutos_IgualQueAntes`.

### Task 4: lectura, siembra, guardar y columnas «no incluido»

- **DAO:** `LeerCandidatos(idPeriodo, out candidatos, out decisiones, out periodos)` y `GuardarDecisiones(idPeriodo, string xml, usuario, ip)`.
- **`NegHorasExtrasPantalla.AbrirPeriodo`:** en lugar de `LeerHorasAprobadas`, lee los candidatos, llama a `Resolver` y a `Minutos`, y pone en cada fila `Minutos50/100` y `Horas50/100 = redondeo(min / 60, 2)`, con `HorasOrigen = "Tareas"`. Si falla la lectura de candidatos, **no** siembra en cero: devuelve el error y deja el período como estaba.
- **`GuardarHoras`:** ignora las horas del cliente. Recalcula a la persona desde las decisiones y guarda solo la observación.
- **`CalcularFueraDeHorario`:** arma los segmentos no incluidos, no aprobados y no bloqueados como `EntHeSegmentoFuera`, y lo incluido como `EntHeIntervaloPagado`, y llama a `NegHeFueraDeHorario.Calcular`. Así «sin aprobar» y «rechazado» pasan a ser lo **no incluido**. Agrega a `EntHeFueraDeHorario`:
  - `Candidatos` (los resueltos, para la ventana y la hoja «Decisiones»);
  - `Conciliacion`: por persona, incluido aprobado, incluido no aprobado, excluido y total a pagar, en minutos y USD, con el valor hora de su fila.
- **`NegHeDecisionesPantalla.Guardar(idPeriodo, idEmpleado, cambios, usuario, ip)`:** carga los candidatos, valida, arma el XML, llama al DAO y usa `NegHeMaestroPantalla.CombinarTrasGuardar` con un recálculo, igual que al completar datos.

### Task 5: handler y pantalla

- **Handler:** la acción `GuardarDecisiones`, con `EstaEn(PerfilesQueEditanMaestro)`, que recibe `idPeriodo`, `idEmpleado` y `cambios` (un arreglo de `{idTramo, segmentoInicio, incluir, tipoHora, minutos, motivo}`).
- **Página:** la ventana de detalle pasa a tener la tabla de candidatos con controles, el aviso de confirmación y el botón «Guardar decisiones». Las horas de la grilla quedan de solo lectura. Columna nueva «Total incluyendo no incluido (USD)». `?v=11`.
- **JS:**
  - `MostrarDetalleFuera` pinta los candidatos de la persona agrupados, con **USD** por segmento, la casilla, el selector, los minutos y el motivo;
  - controles deshabilitados sin `HE_PUEDE_EDITAR_MAESTRO`, con el período cerrado o con el segmento bloqueado;
  - guardar en dos clics, con el nombre destacado;
  - con la respuesta, `PintarPantalla`.

### Task 6: Excel (TDD)

- **Hoja «Conciliación»:** por persona, las columnas de la entidad `Conciliacion` y los totales.
- **Hoja «Decisiones»:** cada candidato resuelto, con grupo, incluir, tipo, minutos, motivo, quién y cuándo.
- **Pruebas:** que las dos hojas existan; que el total de la Conciliación sea igual a la suma de `TotalHE`; y un mensaje cuando no hay cálculo.

### Task 7: validación contra el análisis manual

`Actualizacion/validar-decisiones-avelina.ps1`, en memoria y sin escribir nada:

1. Lee los candidatos reales del cuerpo del procedimiento.
2. Aplica las decisiones del análisis manual y los tres sueldos confirmados en memoria.
3. Calcula con `Resolver`, `Minutos` y `Calcular`.
4. Compara persona por persona con la hoja «Validación Avelina».

**Criterio de terminado:** el total es **6.241,14** y cada persona coincide al centavo.

### Task 8: despliegue y paquete

- `DESPLIEGUE.md` §14, con el aviso a Nómina sobre los centavos.
- El paquete regenerado y comprobado.

### Task 9: publicar (con aprobación)

El script SQL y los binarios. Después, cargar desde la pantalla las decisiones del análisis manual, o hacerlo con un script de carga, si el usuario lo aprueba.
