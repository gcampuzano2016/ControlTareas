# Módulo de Horas Extras (50% / 100%) — Diseño

**Fecha:** 2026-09-15
**Estado:** aprobado, pendiente de plan de implementación
**Entradas:** `Actualizacion/ESPEC_MODULO_HORAS_EXTRAS.md` (especificación funcional del usuario) y `Actualizacion/Plantilla_Carga_Modulo_HE.xlsx` (plantilla de carga con 64 colaboradores)

Este documento **no sustituye** a la especificación funcional: la toma como entrada, la contrasta contra el sistema real y registra en qué se confirma, en qué se aparta y por qué. Donde diga «el §N del documento funcional», se refiere a ese archivo.

> **Sin datos personales.** Este diseño describe estructura y reglas. La plantilla trae nombres, cédulas y sueldos de 64 personas; nada de eso se transcribe aquí. El repositorio es público.

---

## Terreno verificado

Todo lo de esta sección está medido contra producción el 2026-09-15, no supuesto.

### Las 64 personas ya existen en el sistema

| Comprobación | Resultado |
|---|---|
| Cédulas en la plantilla | 64, todas únicas, todas de 10 dígitos |
| Dígito verificador ecuatoriano | **64 de 64 válidas** |
| Presentes en `Empleados.Cedula` | **64 de 64**, y única en esa tabla |
| Presentes en `R_Usuarios.Cedula` | **64 de 64**, todas con usuario activo |
| Ausentes de ambas tablas | **0** |
| Con `PuestoTrabajo` y `AreaTrabajo` en `Empleados` | 64 de 64 |
| Con `Empleados.Cod_Usuario` ya poblado | 63 de 64 |

Los 64 son un subconjunto de los 133 empleados de la tabla. **No hace falta un maestro de colaboradores propio**, que era la primera pregunta abierta del §12 del documento funcional.

### Una cédula es ambigua contra `R_Usuarios`

Una de las 64 la comparten **seis usuarios activos distintos, con seis nombres distintos y cinco códigos de usuario**, en `R_Usuarios`. En `Empleados` es única.

No es casualidad que sea exactamente la única de las 64 sin `Empleados.Cod_Usuario` poblado: el módulo de perfil, al enlazar por cédula, la dejó fuera a propósito en vez de adivinar. Es el mismo criterio que este módulo hereda.

### La plantilla es internamente consistente

| Hoja | Filas | Comprobación |
|---|---:|---|
| `Parametros` | 7 | Coinciden exactamente con el §2.1 del documento funcional |
| `Colaboradores` | 64 | Sin cédulas repetidas ni vacías |
| `Salarios` | 69 | Toda cédula existe en `Colaboradores`; nadie se queda sin sueldo; 5 personas con dos filas (los ajustes que anuncia el §9) |
| `Horas_Periodo` | 61 | Toda cédula existe en `Colaboradores`. 61 = 64 menos los 3 que no aplican |

Elegibilidad ya resuelta a enum en la hoja: 59 `Activo`, 2 `Inactivo`, 1 `NoAplicaHorasExtras`, 2 `EnRevisionSalarial`. Cuadra con el §2.5: los 3 que no aplican son los 2 inactivos más el de dirección; los 2 en revisión sí cobran.

Jornada: **56 personas de 8 h/día y 8 de 4 h/día**. Ninguna trae `DivisorManual`.

### El sistema ya tenía horas extras, y están muertas

Existen `R_TareasArandaHorasExtras`, su histórico, trece procedimientos y tres pantallas (`AprobarHorasExtras`, `AprobarHorasExtraPendientes`, `ReporteHorasExtras`), con una opción de menú activa.

| Comprobación | Resultado |
|---|---|
| Filas en la tabla | 31 |
| Filas con `Horas_Extras` no nula | **0** |
| Filas en el histórico | 0 |
| Última actividad de cualquier tipo | **2019-06-03** |
| Responsables distintos | 15, de los cuales 6 están entre los 64 |
| Filas con fecha de detalle | 0 |

Ese flujo aprueba horas extras colgadas de una tarea u orden de servicio. **Nunca registró una hora**: la columna que las guardaría está vacía en las 31 filas. Tampoco hay fecha de detalle, así que no se podría deducir si una hora fue al 50% o al 100%.

### No hay sueldos en ninguna parte

Ninguna tabla de la base tiene columna de sueldo, salario ni remuneración. **Este módulo sería el primer lugar del sistema donde vive esa información.** No es un detalle: cambia el perfil de riesgo de la base entera.

### No hay marcaciones de las que deducir horas

Las tablas con «marca» en el nombre son de marcas comerciales (`PrmMarca`, `Ger_Marcas`, `PrmGerenteMarca`). Lo único de marcación es `RegistroCorreoMarcacion`, un registro de correos enviados. La decisión del §1 de capturar las horas a mano se sostiene: no hay alternativa.

### La «Empresa» de la plantilla no es la del sistema

| Origen | Valores |
|---|---|
| Plantilla | `AGILITY`, `AGILITY - SERVICIOS PROFESIONALES`, `DOS S.A.`, `SERVICIOS PROFESIONALES DOS S.A.` |
| `Empleados.Sociedad` | `DOS S.A` (sin punto final, 105), `AGILITY S.A` (21), `GREEN DC` (6), nula (1) |

Nómina distingue «servicios profesionales» y el sistema no. Son dos clasificaciones distintas, no una mal escrita.

### Lo que el sistema sí sabe y conviene aprovechar

58 de los 64 tienen horario laboral asignado (`R_UsuarioHorarioLaboral`, 93 asignaciones activas sobre 5 horarios definidos). Sirve para contrastar la jornada que declara la plantilla.

---

## Decisiones

### 1. El módulo cuelga de `Empleados`, y la relación es por `IdEmpleado`

No se crea un maestro propio. Cuatro tablas satélite con prefijo `HE_`, siguiendo el patrón del módulo de perfil: no se le añaden columnas a `Empleados`, que la comparte `RRHHEmpleados.aspx`.

**La cédula se usa una sola vez, en la carga inicial, y nunca más para identificar a nadie.** Esto resuelve de raíz el problema de la cédula ambigua: seis candidatos en `R_Usuarios`, uno solo en `Empleados`. Y es la debilidad que el §11 del documento funcional señala del Excel, llevada un paso más allá: allí la cédula escrita a mano desalineaba las fórmulas; aquí ni siquiera es la llave.

El script de carga **no** intentará resolver esa cédula a un `Cod_Usuario`. Elegir uno de seis sería inventar.

### 2. El flujo de horas extras de 2019 no se toca ni se consume

Se consideró que el módulo nuevo tomara las horas ya aprobadas, para no tener dos verdades. No es posible: **no hay horas aprobadas**. La columna está vacía en las 31 filas y la última actividad es de hace siete años.

El módulo nuevo captura sus propias horas, que es lo que dice el §1. El flujo viejo se queda donde está, sin tocarlo.

### 3. Dos totales por persona y período, sin detalle por día

Una fila por colaborador con dos casillas editables: horas al 50% y horas al 100%. Es lo que reemplaza al Excel y lo que Nómina ya sabe usar.

**Consecuencia asumida:** no queda registro de qué día ni por qué una hora se pagó al 100%. Ante una inspección, el sustento del recargo hay que buscarlo fuera del sistema. Se decidió a sabiendas, priorizando que el módulo se parezca a lo que hoy usan.

**Consecuencia técnica que el §6.9 no contempla:** el tope **diario** de 4 horas **no se puede validar**, porque nadie sabe de qué día son. Sólo cabe una advertencia sobre el total mensual. El cierre del período no exigirá la validación diaria, porque no puede.

### 4. Sólo Nómina/RRHH, y ven todo

Una pantalla, un perfil con acceso, sin acotar por área ni por jefatura. Quien captura las horas es Nómina, y ver el salario es parte de su trabajo.

Se descartó repartir la digitación entre las 22 jefaturas del sistema: habría expuesto sueldos a 22 personas más. La guarda de jefatura que el módulo de perfil construyó existe y se podría reusar, pero **no se usa aquí**.

### 5. Período mensual calendario

`Año` + `Mes`, como propone el §3.1. El salario vigente se toma al último día del mes. Sin quincenas.

### 6. La jornada se contrasta, no se cree

La plantilla declara 8 h para 56 personas y 4 h para 8. El sistema conoce el horario de 58 de los 64. La carga **compara y reporta las diferencias** en vez de aceptar el Excel a ciegas. El valor de la plantilla se carga igual; el reporte es para que RRHH lo revise.

### 7. El divisor manual nace vacío

El §2.3 deja pendiente con Legal el divisor de jornada parcial, que afecta a 8 personas reales. El campo existe desde el día uno y, si tiene valor, manda sobre la fórmula. Hasta que RRHH escriba uno, manda `horas/día × 30`.

### 8. La empresa de la plantilla se guarda y se muestra

Es la clasificación que usa quien paga, y el sistema no la tiene. Va en la tabla satélite. `Empleados.Sociedad` se queda como está, sin tocar.

### 9. Sin autoguardado

El §5.4 lo pide. Se descarta: ningún otro sitio del sistema lo hace, y un guardado parcial automático sobre un período de nómina es de las cosas que dan sustos. Se usa el patrón de la casa: botón **Guardar**, aviso al salir con cambios pendientes, e indicador visible de «sin guardar».

---

## Diseño

### Modelo de datos

Seis tablas nuevas. Estilo de la casa: `INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`, **sin claves foráneas**, y las columnas `Fec_Modificacion` / `Usu_Modificacion` / `Ip_Modificacion` que ya usan las tablas del módulo de perfil.

| Tabla | Contenido |
|---|---|
| `HE_ColaboradorParametro` | Por `IdEmpleado`: `JornadaHorasDia`, `DivisorManual` (nulo), `AplicaHE`, `MotivoNoAplica`, `Empresa`. Lo que `Empleados` no tiene |
| `HE_Salario` | Historial: `IdEmpleado`, `Monto`, `FechaVigenciaDesde`, `Origen` (`Rol` \| `Ajuste`), `Observacion` |
| `HE_Parametro` | `Clave`, `Valor`, `FechaVigenciaDesde`, `FechaVigenciaHasta` (nula) |
| `HE_Periodo` | `Anio`, `Mes`, `Estado` (`Abierto` \| `Cerrado` \| `Anulado`), fechas y usuarios de creación y cierre |
| `HE_Detalle` | Una fila por período y colaborador, con el snapshot congelado. `UNIQUE (IdPeriodo, IdEmpleado)` |
| `HE_DetalleAuditoria` | Valor anterior, valor nuevo, usuario, fecha e IP — **sólo sobre las dos columnas editables** |

**El snapshot se mantiene tal como lo diseña el §3.1**, y es la mejor idea del documento funcional: un período cerrado no cambia porque alguien editó el maestro después. `HE_Detalle` guarda su propia copia de cédula, nombre, empresa, cargo, jornada, salario base y elegibilidad, más el divisor y los tres valores hora con los que calculó.

Índices: `HE_Detalle (IdPeriodo)` y `HE_Salario (IdEmpleado, FechaVigenciaDesde DESC)`.

Se audita sólo lo editable. Auditar las columnas derivadas sería auditar una fórmula.

### El cálculo

Clase pura en `CapaNegocio`, sin base de datos ni UI, como pide el §4 y como ya hacen `NegPerfilCampos` y `NegPerfilCv`. La cadena es la del §2.4 sin cambios, con `decimal` en todo, seis decimales en los valores intermedios y redondeo `AwayFromZero` a dos decimales **sólo en los totales**, al persistir. El total del período es la **suma de los totales ya redondeados**.

Las capas van `CapaNegocio → CapaDato → CapaEntidad`; `CapaDato` no referencia `CapaNegocio`, que cerraría un ciclo.

### La misma fórmula en dos lenguajes

El §5.2 quiere recálculo inmediato en el cliente y el §6.7 que el servidor recalcule e ignore lo que el cliente mande. Ambas son correctas, y juntas significan la fórmula escrita dos veces, en C# y en JavaScript. Eso se desincroniza en silencio.

**La regla:** el número del cliente no se persiste ni se cree nunca. Al guardar, el servidor recalcula y **devuelve sus valores, y la grilla se repinta con ellos**. Si el cliente calculó distinto, el usuario ve el número saltar. La divergencia se vuelve visible en vez de silenciosa.

### La pantalla

`HorasExtras.aspx` con el molde del sistema: `Master.Master`, Bootstrap 3, un handler `.ashx` que recibe `[{"action": "...", "parameters": {...}}]`, respuestas `EntRespuesta` y el modal de mensajes. La identidad sale siempre de `context.Session["Cod_Usuario"]`.

A diferencia de `MiPerfil.aspx`, **sí se registra en `MenuDos` + `PerfilMenu`**: no se llega desde el desplegable de usuario.

Las tablas no llevan `bg-primary` ni tamaños de fuente en línea: `css/dos-tema.css` ya diseña las tablas del sistema y cualquier sobreescritura desentona.

**La grilla pliega lo derivado.** Las 18 columnas del §5.3 suman unos 2 100 px y la columna fija con desplazamiento horizontal es frágil en jQuery plano. Por omisión se ven ocho: colaborador, cargo, jornada, aplica, **horas 50%**, **horas 100%**, total horas y **total HE**. Un botón despliega las seis derivadas —divisor, valor hora ordinaria, valor hora 50%, total 50%, valor hora 100%, total 100%— que siguen ahí para auditar, sin estorbar a quien digita. Son 64 filas: sin paginación.

Se conservan del §5.4: las dos únicas casillas editables, `Enter` que baja a la fila siguiente, y **pegar una columna desde Excel**. Eso último es lo que hace que el cambio desde la hoja no duela.

Los estilos condicionales del §5.5 se mantienen, salvo los que dependen del tope diario.

---

## Fases

El corte no es por capa técnica, sino por lo que cada fase deja funcionando y verificable por sí sola.

**Fase 1 — el cálculo y sus datos.** Las seis tablas, el script de carga desde la plantilla, y la clase pura de cálculo con sus 10 pruebas. Al terminar, los 64 colaboradores, sus 69 sueldos y los 7 parámetros están en producción, y el cálculo está probado contra los casos del §10 **sin que exista aún ninguna pantalla**. Es la fase que se puede verificar entera sin interfaz, y la que decide si los números salen bien.

**Fase 2 — la pantalla.** El período, la grilla de captura, el tablero de totales, guardar y recalcular en servidor. Al terminar, Nómina puede registrar horas y ver el valor a pagar.

**Fase 3 — cerrar y exportar.** El cierre del período con sus validaciones, el bloqueo de edición, la reapertura con permiso, la auditoría de las dos columnas editables y la exportación para nómina.

Cada fase entrega software que funciona. La 1 se puede usar desde una consulta si hiciera falta; la 2 ya reemplaza al Excel; la 3 es lo que lo convierte en un proceso con cierre.

**El formato exacto del archivo de exportación sigue abierto** — es la quinta pregunta del §12 y nadie la ha respondido. No bloquea las fases 1 y 2; hay que resolverla antes de la 3.

---

## Verificación

Los **8 casos del §10 se convierten en 8 pruebas unitarias** sobre la clase de cálculo pura, junto a las 109 que el proyecto ya tiene. El caso 4 —salario 329, jornada 4 h, divisor 120— es el que cubre a las 8 personas de media jornada.

Se añaden dos que el §10 no tiene, y son los que fallan en silencio:

1. Que el redondeo sea `AwayFromZero` a dos decimales **sólo al final**, no durante la cadena.
2. Que el total del período sea la **suma de los totales ya redondeados**, no el redondeo de la suma.

El §2.6 especifica bien las dos; sin prueba, son lo primero que alguien «simplifica».

Lista de comprobación manual aparte, con los casos que dictaron los datos: una persona de media jornada, una de las que no aplican, la de la cédula ambigua, y un período cerrado que no cambia al editar el maestro.

---

## Riesgos

**La validación de corte del §9.5 no se puede ejecutar hoy.** Ese paso manda cargar un período con horas conocidas y cuadrar al centavo contra el Excel. Las horas conocidas están en `Calculadora_Horas_Extras_50_100.xlsx`, **que no se entregó**. La hoja `Horas_Periodo` de la plantilla viene con las 61 filas en cero: es un esqueleto, no datos. Sin ese archivo, la migración no se puede dar por buena. **Bloquea el cierre, no la construcción.**

**Este módulo mete sueldos en una base que hoy no los tiene.** Cambia el perfil de riesgo de todo el sistema, no sólo de esta pantalla. El acceso por perfil y la auditoría son la mitigación, pero conviene que alguien lo decida sabiendo que lo está decidiendo.

**El tope diario no se puede validar** con totales mensuales. Queda como advertencia sobre el total del mes, no como control.

**Los sueldos cargados envejecen.** La plantilla trae una foto de hoy. Nada en el sistema los actualizará solos: cada cambio salarial hay que registrarlo a mano en `HE_Salario` o el cálculo del mes siguiente usará el sueldo viejo, sin avisar.

**Cuatro empresas en la plantilla contra tres en el sistema.** Si la distinción de «servicios profesionales» importa para pagar, vive sólo en esta plantilla y en este módulo.

---

## Fuera de alcance

- Aportes IESS, décimos, fondos de reserva, impuesto a la renta (§1).
- Marcaciones y reloj biométrico (§1). Verificado: no hay dato del que deducirlas.
- Generación del rol de pagos. Este módulo entrega un insumo.
- **Detalle por día y motivo del recargo** (sábado, domingo, feriado, nocturna).
- **Visibilidad por jefatura.** Sólo Nómina.
- **Quincenas.** Sólo mes calendario.
- **El flujo de aprobación de 2019.** No se toca, no se consume, no se retira.
- **Autoguardado.**
- **Prorrateo de un cambio salarial dentro del período.** Se usa el salario vigente al último día (§7).
- Corregir la cédula ambigua en `R_Usuarios`. Es de Talento Humano; el módulo convive con ella.

---

## Despliegue

Según `DESPLIEGUE.md`, y como las tres fases del módulo de perfil:

1. **El script SQL corre primero**, siempre. Idempotente, con `PRINT` y aserciones, y arrancando con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` dentro del propio script.
2. Publicar con `FolderProfile` (Release) y **copiar sin sincronizar**: un `robocopy /MIR` borra `connections.config` y `appsettings.config` y el sitio no arranca.
3. **Regenerar y commitear el paquete de `obj/Release/Package/PackageTmp`**, que el repositorio versiona a propósito. Se olvidó en las cuatro entregas anteriores.
4. Registrar la pantalla en `MenuDos` + `PerfilMenu`.

**Rollback:** todo el cambio es aditivo —tablas, procedimientos y pantalla nuevos—. Se republican los binarios anteriores y se retira la opción de menú. Las tablas quedan con datos que nadie lee.
