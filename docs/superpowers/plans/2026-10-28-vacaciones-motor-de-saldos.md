# Plan: llevar el Excel de vacaciones de GTH al sistema

Fuente: `SISTEMA_VACACIONES_DOS (15).xlsx` (Talento Humano y Cultura), revisado el 2026-10-28.

> **No commitear el Excel.** Trae cédulas y sueldos de 107 personas, y el repo es público.

## 1. Qué es el Excel

No es una solicitud. Es el **libro mayor de vacaciones** que GTH lleva a mano. Tiene 10 hojas:

| Hoja | Qué hace |
|---|---|
| COLABORADORES | Maestro: cédula, empresa (DOS/AGILITY), área, cargo, **fecha de ingreso**, estado, fecha de salida, sueldo, **fecha y saldo de corte** (31 personas tienen corte) |
| REGISTRO | 1.529 movimientos de 96 personas, de 2007 a 2027: 1.110 vacaciones, 418 permisos por horas y 1 compra de vacaciones |
| CALCULO_PERIODOS | Motor: 150 colaboradores × 40 periodos. Consume siempre el periodo más antiguo |
| SALDOS | Saldo por persona: disponible, proporcional, usado del periodo en curso, por vencer, vencido y estado |
| ESTADO INDIVIDUAL | Estado de cuenta imprimible para firmar |
| DASHBOARD / NOMINA | Totales de activos: 95 personas, saldo de 2.326 días y provisión de $113.257 |
| LIQUIDACION | Días y valor a liquidar en una salida, periodo por periodo |
| PARAMETROS | Parámetros y feriados nacionales de 2005 a 2027 |

### Reglas que aplica

1. **Días por año**: 15. Desde el 6.º año suma 1 por cada año sobre 5, con un tope de +15 (máximo 30). Art. 69.
2. **Periodos**: van de aniversario a aniversario. Los días se ganan al cumplir el año; mientras tanto se muestra el **proporcional** (`días × transcurrido ÷ duración del periodo`).
3. **Consumo FIFO**: se descuenta del periodo más antiguo que tenga saldo y no haya vencido.
4. **Vencimiento**: el periodo *n* vence en el aniversario *n+3* (`EDATE(ingreso, 12·(n+3))`), con 3 periodos acumulables. Alerta 90 días antes.
5. **Anticipo**: tomar días de un periodo que aún no se cumple deja el saldo en negativo. Hoy hay 6 personas así.
6. **Reingreso**: los movimientos anteriores a la fecha de ingreso vigente no cuentan («RELACIÓN ANTERIOR»).
7. **Saldo de corte**: si hay un saldo conciliado y firmado, el cálculo parte de él e ignora lo anterior.
8. **Cargo de fin de semana**: las vacaciones son días calendario.
   - Si una solicitud termina en viernes, se cargan sábado y domingo (+2).
   - Cada 5 días sueltos entre semana dentro del mismo año de servicio cargan +2.
   - Tope: 2 fines de semana cargados por periodo.
   - **No aplica** si la observación dice «BENEFICIO DOS» (95 filas, sobre todo los días entre Navidad y Año Nuevo).
9. **Permisos por horas**: se convierten a días (`(h + min/60) ÷ 8`) y **descuentan** del saldo. Por defecto no suman como «días sueltos».
10. **Estados**:
    - DADA BAJA no descuenta y nunca se borra.
    - HISTÓRICO y PROCESADO sí descuentan.
    - Se detectan solicitudes que se cruzan en fechas (posible duplicado).
11. **Liquidación**: valor referencial = días × sueldo ÷ 30. También muestra el Art. 71 (1/24 de lo ganado en el año).

## 2. Qué hace hoy el sistema y dónde choca

| Tema | Sistema actual | Excel | Choque |
|---|---|---|---|
| Saldo | `SaldoVacaciones`, carga estática del 2021-11-08, nunca se actualiza (`Sp_RTAConsultarSaldoVacaciones`). SAP `PA2006` solo se lee | Lo calcula a partir del ingreso y los movimientos | **El saldo del sistema está congelado en 2021** |
| Fecha de ingreso | El módulo no la usa | Es la base de todo | Falta el dato |
| Días de una vacación | Calendario − feriados (`feriadosVacaciones.js:54`) | Calendario. Si se descuenta un feriado, lo marca «Faltan X días» | Ver decisión D1 |
| Fin de semana / Beneficio DOS | No existe | Reglas 8 | Falta |
| Permisos por horas | Bolsa de 3 h/mes; el excedente solo marca `CargoVacaciones=1`. No convierte ni descuenta | Todas las horas ÷ 8 descuentan | Ver decisión D3 |
| Anticipo | Solo un aviso; el automático espera la conciliación con SAP (`ProcesoConvenio.js:485`) | Saldo negativo permitido | Ver decisión D5 |
| Cruces de fechas, compra, liquidación, provisión, estado de cuenta | No existen | Existen | Falta |
| Clave de persona | `Cod_Usuario`/`Cod_Sap` | Nombre completo | Hay que mapear por **cédula** |

## 3. Decisiones que GTH debe tomar antes de codificar

- **D1 Feriados dentro de vacaciones.** El sistema los descuenta y el Excel los marca como faltantes. Aun así, unas 30 filas dicen «Feriado(s) no descontado(s) … (antes N días)», o sea que GTH sí los descontó a mano. ¿Cuál es la regla?
- **D2 Texto de vencimiento.** La hoja INICIO dice «vence cuando se cumple el siguiente», pero la fórmula vence a los 3 periodos. ¿Vale la fórmula?
- **D3 Permisos por horas contra el saldo.** ¿Descuentan todas las horas, o solo el excedente sobre las 3 h/mes con `CargoVacaciones=1`?
- **D4 Beneficio DOS.** ¿Qué es exactamente: qué fechas, quién lo tiene, aplica a AGILITY? Debe ser un campo, no texto en la observación.
- **D5 Fuente de verdad del saldo.** ¿El motor nuevo reemplaza a SAP/`SaldoVacaciones`, o sigue siendo SAP y el motor solo concilia? Nómina tiene que participar.
- **D6 Anticipos.** ¿Se permiten desde la solicitud, con qué tope, y quién aprueba?
- **D7 Compra de vacaciones.** ¿Quién la registra y si genera algo en nómina?

## 4. Fases

### F0 — Decisiones y limpieza del Excel (GTH, sin código)
- [ ] Cerrar D1 a D7 por escrito.
- [ ] Corregir en el Excel:
  - 3 filas «SIN FECHA - REVISAR» (filas 61, 62, 96).
  - 1 fila en estado REVISAR (110).
  - 66 POR APROBAR, algunas de 2022 (p. ej. 496).
  - 4 filas con más días que el rango, todas históricas.
  - 1 colaborador sin empresa.
  - Varios de AGILITY sin área.
- [ ] Confirmar la cédula de las 107 personas. Es la llave del mapeo; el módulo de Bonos espera lo mismo.

### F1 — Datos maestros
- [ ] Tabla `VAC_Colaborador`, ligada a `Empleados` por cédula: empresa, fecha de ingreso, fecha de salida, fecha y saldo de corte, observación. El sueldo **no** se guarda aquí: se reutiliza `HE_Salario`, que ya tiene ese acceso restringido.
- [ ] Tabla `VAC_Parametro`, con los valores de PARAMETROS (15, 5, 15, 3, 90, 30, 5, 2, 2, 8, NO, SÍ).
- [ ] Cargar en `Feriado` los años 2005 a 2025 desde la hoja PARAMETROS (hoy solo hay 2026 y 2027). Sin esos años, el cálculo histórico falla.
- [ ] Script de carga de COLABORADORES que mapee por cédula y liste los que no casen.

### F2 — Motor de saldos (SQL), el corazón del trabajo
- [ ] Función o SP `Sp_VAC_CalcularPeriodos(@Cedula, @FechaCalculo)` que replique CALCULO_PERIODOS: periodos, días ganados, FIFO, vencido, por vencer, estado.
- [ ] `Sp_VAC_Saldos(@FechaCalculo)` que replique SALDOS: disponible, proporcional, usado, saldo, ANTICIPO/POR VENCER/ACUMULANDO/AL DÍA.
- [ ] **Prueba de oro**: exportar SALDOS y CALCULO_PERIODOS del Excel a la misma fecha y comparar persona por persona y periodo por periodo, al centésimo. No se avanza hasta que cuadren las 107 personas. Las diferencias se documentan como se hizo con Avelina en horas extras.

### F3 — Migración del histórico
- [ ] Tabla `VAC_Movimiento`. Guarda la clase del movimiento (VACACIONES / PERMISO HORAS / COMPRA / AJUSTE), el origen (EXCEL / SISTEMA), el estado, las fechas, los días ingresados, los días que descuenta y la bandera de Beneficio DOS.
- [ ] Importar las 1.529 filas de REGISTRO. Las solicitudes nuevas del sistema (`Vacaciones`) entran a través de una vista, para no duplicar.
- [ ] Cuidar el solape: lo que ya está en `Vacaciones` **y** en el Excel debe casar por persona y fechas, no sumarse dos veces.

### F4 — Reglas en la solicitud
- [ ] Calcular en el servidor (SP), no en JS, los días que descuenta: días + cargo de fin de semana según la regla 8 y D4.
  - Así no se tocan dos veces `Convenio.js` y `ProcesoConvenio.js`, que están duplicados. Las dos pantallas solo muestran lo que devuelve el SP.
- [ ] Mostrar el saldo del motor (F2) en lugar de `SaldoVacaciones`, junto con el periodo del que se descuenta y si es anticipo.
- [ ] Validar cruces de fechas con otras solicitudes vigentes.
- [ ] Permisos por horas: aplicar la conversión ÷ 8 según D3.
- [ ] Recordatorios técnicos:
  - Las `.aspx` necesitan BOM.
  - El `.ashx` debe fijar `ContentEncoding` en las acciones con tildes.
  - Correr `node --check` en cada `.js`.
  - Publicar juntos el `.dll` y los `.exe` de capa.

### F5 — Pantallas de GTH (menú `MenuDos` + `PerfilMenu`, solo perfiles GTH)
- [x] Estado individual con PDF para firmar: botón en la ficha de `VacacionesSaldos.aspx`, `DescargarVacEstado.ashx`, folio en `Vac_EstadoEmitido` (2026-10-29, DESPLIEGUE §26).
- [x] Dashboard de saldos, alertas y anticipos: `VacacionesSaldos.aspx` (2026-10-28). Construida y probada con datos reales; falta publicar binarios y correr `2026-10-28-vacaciones-saldos-menu.sql` (DESPLIEGUE §24).
- [x] Nómina y provisión: `VacacionesNomina.aspx`, sueldo de `HE_Salario` o `Vac_Sueldo`; cuadra con NOMINA al centavo ($113.257,39 al 28-10). DESPLIEGUE §28.
- [x] Liquidación por periodo para salidas: `VacacionesLiquidacion.aspx` con `@Liquidacion` en el motor; cuadra con el ejemplo del Excel (14,14 días). DESPLIEGUE §29.
- [ ] Mantenimiento de saldo de corte y de compra de vacaciones, con bitácora.

- [x] «Mis vacaciones»: cada colaborador ve su saldo (`MisVacaciones.aspx`, autorizado por GTH el 2026-10-29, DESPLIEGUE §27).

### F6 — Paralelo y corte
- [ ] Un mes con el Excel y el sistema en paralelo. Conciliar cada semana con la prueba de oro.
- [ ] GTH firma un saldo de corte por persona en el sistema.
- [ ] Retirar el Excel. Definir con Nómina la conciliación con SAP (D5).

## Estado al 2026-10-28: motor instalado en ReporTarea (192.168.11.14)

| Archivo | Qué hace |
|---|---|
| `docs/sql/2026-10-28-vacaciones-esquema.sql` | `Vac_Parametro` (12 valores), `Vac_Colaborador`, `Vac_Movimiento`, `Vac_FeriadoHistorico` |
| `docs/sql/2026-10-28-vacaciones-motor.sql` | `Sp_RTA_VacCalcular @FechaCalculo, @IdColaborador, @Salida` (`SALDOS` / `PERIODOS` / `MOVIMIENTOS`). Solo lee |
| `docs/sql/2026-10-28-vacaciones-motor-prueba.sql` | 10 reglas con 3 personas ficticias, dentro de transacción con ROLLBACK |
| `docs/sql/generar-carga-vacaciones.py` | Arma `carga-generada/vacaciones-carga.sql` y `vacaciones-prueba-oro.sql` desde el Excel (en .gitignore) |

**Prueba de oro contra el Excel (15), al 2026-10-28:** cuadran las 1.529 solicitudes, los 803 periodos y los 106 saldos, al centésimo. El total de activos da 2.326,41 días, igual que el DASHBOARD. La prueba de reglas pasa 10 de 10. El 2026-10-28 se instalaron en la base real el esquema, el motor y la carga del Excel (15): 106 colaboradores, 1.529 movimientos y 232 feriados históricos, todos con `Origen=EXCEL`. Después de instalar, la prueba de reglas y la prueba de oro volvieron a dar OK. Ninguna pantalla usa todavía el motor. Para recargar desde un Excel más nuevo, se regenera la carga y se vuelve a correr: borra y recarga, salvo que ya existan movimientos `Origen=SISTEMA`, y en ese caso se niega a correr.

Hallazgos:
- **Feriados 2027 distintos.** El Excel trae 03-11, 24-12 y 31-12; `dbo.Feriado` trae 02-11 y 25-12. El motor usa la tabla, y hoy eso afecta a 2 solicitudes.
- **Cédulas.** 105 de 106 casan con `Empleados` por cédula. Falta la del N.º 102 de COLABORADORES.
- **Estados que descuentan.** POR APROBAR y REVISAR descuentan igual que APROBADO. Así lo hace el Excel, y GTH tiene que confirmarlo.

Para instalar, en este orden: esquema → motor → prueba → `python -I docs/sql/generar-carga-vacaciones.py "<excel>"` → `carga-generada/vacaciones-carga.sql` → `carga-generada/vacaciones-prueba-oro.sql`. Ninguna pantalla lee todavía el motor.

## 5. Orden y riesgo
- La ruta crítica es **F0 → F1 → F2**. F2 es lo que más vale: aunque no se haga nada más, el sistema dejaría de mostrar un saldo congelado en 2021.
- F4 cambia lo que ve todo el personal, así que sale solo con F2 cuadrado y con D1, D3 y D4 cerradas.
- F5 puede ir en paralelo a F4.
- Riesgo principal: mapear por cédula. Riesgo secundario: que GTH cambie reglas a mitad del camino. Por eso todo valor numérico va a `VAC_Parametro`.
