# Procesos (Entrega 2) — prueba de concurrencia de la doble marca

**Fecha:** 2026-10-23 · **Dónde:** base desechable en LocalDB (`(localdb)\MSSQLLocalDB`, SQL Server 2019 con
nivel de compatibilidad 130), borrada al terminar. **Nada de esto se corrió contra producción.**

**Qué se prueba:** dos sesiones marcan a la vez el último entregable del paso en curso. Tiene que activarse **un solo**
paso siguiente (una actividad, y una sola sesión recibe `IdProcesoPasoActivado`, que es la que manda el correo).

## Preparación

1. Base nueva: `CREATE DATABASE ZZ_ProcesosPrueba` + tabla `Feriado` mínima.
2. `2026-10-15-agenda-esquema.sql` (versión de `HEAD`, la que está en producción), unas filas de la Entrega 1, y
   `2026-10-23-procesos-esquema.sql` dos veces (las dos `VEREDICTO: OK`).
3. Datos confirmados (fuera de transacción, en la base local): un área `ZZ Conc`, una plantilla de 2 pasos y, con
   `Sp_RTA_AgendaIniciarProceso` (iniciador `ZZINI`):
   - ronda 0: un proceso cuyo paso 1 tiene un solo entregable `Unico`;
   - ronda 100: un proceso cuyo paso 1 tiene dos entregables `E1`, `E2`;
   - rondas 201–300: 100 procesos; pares con un solo entregable, impares con dos.

   Una tabla `ZZ_ConcMapa (Ronda, IdProceso, EUnico, E1, E2)` dice qué entregable marca cada sesión, y
   `ZZ_ConcLog` guarda lo que devolvió cada llamada (`Respuestas`, `IdProcesoPasoActivado`, hora de inicio y fin).

Todas las marcas usan `@FechaEntregaSiguiente = '20301118'`.

## Prueba 1 — mismo entregable, la primera sesión retiene su transacción

Dos `sqlcmd` lanzados a la vez:

- **Sesión A** (`ZZA`): `BEGIN TRAN; EXEC Sp_RTA_AgendaMarcarEntregable @EUnico, 1, 'ZZA', '20301118';
  WAITFOR DELAY '00:00:08'; COMMIT;`
- **Sesión B** (`ZZB`): `WAITFOR DELAY '00:00:02'; EXEC Sp_RTA_AgendaMarcarEntregable @EUnico, 1, 'ZZB', '20301118';`

| Sesión | Inicio | Fin | Respuestas | IdProcesoPasoActivado |
|---|---|---|---|---|
| A | 11:02:39.827 | 11:02:47.908 | 0 | 12 |
| B | 11:02:41.842 | 11:02:47.910 | **-9** | NULL |

B quedó bloqueada 6 s y terminó 2 ms después del `COMMIT` de A; entonces vio el paso `Completo` y devolvió `-9` sin
tocar nada. El paso 2 tiene **una** actividad (`Usuario_Registro = ZZA`, `Cod_AsignadoPor = ZZINI`).

## Prueba 2 — dos entregables, cada sesión marca uno

Igual que la 1, pero A marca `E1` (y retiene 8 s) y B marca `E2`.

| Sesión | Inicio | Fin | Respuestas | IdProcesoPasoActivado |
|---|---|---|---|---|
| A | 11:02:48.008 | 11:02:56.036 | 0 | NULL |
| B | 11:02:50.027 | 11:02:56.063 | 0 | 114 |

B esperó a A en el bloqueo del proceso; al entrar, su marca completó la actividad y fue la **única** que activó el paso
siguiente. A no activó nada. Una actividad en el paso 2 (`Usuario_Registro = ZZB`).

## Prueba 3 — carrera real, 100 rondas sin espera artificial

Dos `sqlcmd` (`-v Ses=A` y `-v Ses=B`) con el mismo script: para cada ronda r de 201 a 300, ambos esperan en un bucle
activo hasta el mismo instante (`T0 + (r − 200) × 150 ms`, `T0` leído de una tabla común) y entonces llaman a
`Sp_RTA_AgendaMarcarEntregable`, sin transacción propia: en las rondas pares los dos marcan el mismo entregable; en las
impares A marca `E1` y B marca `E2`.

| Tipo de ronda | Rondas | Con exactamente 1 activación | Con las 2 llamadas solapadas en el tiempo | Mayor diferencia de arranque | Ganó A | Con un `-9` |
|---|---|---|---|---|---|---|
| mismo entregable | 50 | **50** | 50 | 4 ms | 18 | 50 |
| dos entregables | 50 | **50** | 50 | 2 ms | 35 | 0 |

Después: los 100 procesos con el paso 1 `Completo` y el paso 2 `EnCurso`, y **ningún paso con más de una actividad**
(`MAX(COUNT(*)) = 1` por `IdProcesoPaso`). Ningún error ni abrazo mortal en ninguna de las dos sesiones (`sqlcmd -b`
no reportó nada). Las dos sesiones ganaron rondas: las llamadas se cruzaron de verdad.

## Por qué funciona

`Sp_RTA_AgendaMarcarEntregable` / `MarcarCompleta`, cuando la actividad es de un paso, abren una transacción y
**antes de leer o escribir nada** toman `UPDLOCK, HOLDLOCK` sobre la fila de `AG_Proceso` y luego sobre los pasos del
proceso (`Sp_RTA_AgendaPasoBloquear`). La segunda sesión espera ahí y, cuando entra, ya ve el estado que dejó la
primera: o el paso `Completo` (`-9`), o el entregable de la otra marcado (y entonces es ella la que completa). Además, el
índice único filtrado `UX_AG_Actividad_ProcesoPaso` impide una segunda actividad para el mismo paso aunque algo se
saltara el bloqueo (caso P12 de `2026-10-23-procesos-prueba.sql`: error 2601).

## Limpieza

`DROP DATABASE ZZ_ProcesosPrueba` y `DROP DATABASE ZZ_ProcesosE1` al terminar.
