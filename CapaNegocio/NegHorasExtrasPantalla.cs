using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// La orquestacion de la pantalla de horas extras: abrir un periodo,
    /// cargarlo y guardar.
    ///
    /// Vive aparte de NegHorasExtras a proposito. Aquella es una clase pura que
    /// no sabe que existe una base de datos, y se prueba entera sin levantar
    /// nada; esta habla con el DAO. Mezclarlas obligaria a una u otra a mentir
    /// sobre lo que necesita para funcionar.
    /// </summary>
    public static class NegHorasExtrasPantalla
    {
        /// <summary>
        /// Tope de horas por celda del lado del servidor. El documento
        /// funcional lo declara como validacion de cliente, pero el cliente
        /// no es de fiar: un dedazo de 10000 horas pasa el parseo y el
        /// calculo si nadie lo detiene aqui.
        /// </summary>
        private const decimal TopeHorasPorCelda = 200m;

        /// <summary>
        /// La lista de periodos, para el selector de la pantalla. Envuelve al
        /// DAO para que el handler nunca lo toque directo: toda la casa entra
        /// por CapaNegocio.
        /// </summary>
        public static List<EntHePeriodo> ListarPeriodos()
        {
            return DaoHorasExtras.ListarPeriodos();
        }

        /// <summary>
        /// Si una celda de horas supera el tope. No es dato corrupto como las
        /// horas negativas -que se marcan y se pagan en cero-: es un error de
        /// captura que hay que devolver para que lo corrijan, no absorber.
        /// </summary>
        public static bool ExcedeTopePorCelda(decimal horas)
        {
            return horas > TopeHorasPorCelda;
        }

        /// <summary>
        /// Rellena una fila a partir de su salario y los parametros. Es el unico
        /// punto donde la pantalla toca el calculo, y delega entero en
        /// NegHorasExtras: aqui no hay ni una division ni un factor.
        ///
        /// El invariante que protege es uno solo: cuando no se puede resolver
        /// un sueldo desde el maestro, nunca se pisa lo que el periodo ya tenia
        /// congelado. Por eso "salarioCongelado" es un parametro obligatorio y
        /// no algo que se infiere de fila.SalarioBaseSnapshot al entrar: leerlo
        /// de ahi fallo en silencio la primera vez que un llamador (AbrirPeriodo)
        /// armo una fila fresca sin haberlo trasladado antes -su
        /// SalarioBaseSnapshot nacia en 0 por omision, y el resguardo nunca se
        /// activaba-. Exigirlo como argumento obliga a cada llamador a
        /// decidirlo explicitamente, en vez de depender de que alguien se
        /// acuerde de poblar el campo por adelantado.
        /// </summary>
        public static void AplicarCalculo(EntHeFila fila, decimal salarioDelMaestro,
                                          decimal salarioCongelado, EntHeParametros parametros)
        {
            if (fila == null) { return; }

            /* Si el maestro da un sueldo valido, ese gana -incluso si es
               distinto del congelado: un sueldo corregido tiene que aplicarse-.
               Si no da ninguno, sobrevive el congelado. Si ninguno de los dos
               tiene nada, es el caso legitimo de alguien que nunca tuvo sueldo
               cargado: sigue la rama normal, con advertencia. */
            decimal salario = salarioDelMaestro > 0m ? salarioDelMaestro : salarioCongelado;

            fila.SalarioBaseSnapshot = salario;

            EntHeInsumo insumo = new EntHeInsumo();
            insumo.SalarioBaseVigente = salario;
            insumo.JornadaHorasDia = fila.JornadaHorasDiaSnapshot;
            insumo.DivisorManual = fila.DivisorManual;
            insumo.AplicaHE = fila.AplicaHESnapshot;
            insumo.Horas50 = fila.Horas50;
            insumo.Horas100 = fila.Horas100;

            EntHeResultado r = NegHorasExtras.Calcular(insumo, parametros);

            fila.Divisor = r.Divisor;
            fila.ValorHoraOrdinaria = r.ValorHoraOrdinaria;
            fila.ValorHora50 = r.ValorHora50;
            fila.ValorHora100 = r.ValorHora100;
            fila.Total50 = r.Total50;
            fila.Total100 = r.Total100;
            fila.TotalHoras = r.TotalHoras;
            fila.TotalHE = r.TotalHE;
            fila.TieneAdvertencia = r.TieneAdvertencia;
        }

        /// <summary>
        /// Los seis indicadores del tablero. Suma totales YA redondeados, que no
        /// es lo mismo que redondear la suma: asi el gran total cuadra con lo que
        /// cualquiera obtiene sumando a mano la columna de la pantalla.
        /// </summary>
        public static void SumarTotales(EntHePantalla pantalla)
        {
            if (pantalla == null || pantalla.Filas == null) { return; }

            List<decimal> pago50 = new List<decimal>();
            List<decimal> pago100 = new List<decimal>();
            List<decimal> pagoTotal = new List<decimal>();
            decimal horas50 = 0m, horas100 = 0m, horas = 0m;

            foreach (EntHeFila f in pantalla.Filas)
            {
                if (f == null) { continue; }
                pago50.Add(f.Total50);
                pago100.Add(f.Total100);
                pagoTotal.Add(f.TotalHE);
                horas50 += f.Horas50;
                horas100 += f.Horas100;
                horas += f.TotalHoras;
            }

            pantalla.TotalPago50 = NegHorasExtras.TotalDelPeriodo(pago50);
            pantalla.TotalPago100 = NegHorasExtras.TotalDelPeriodo(pago100);
            pantalla.TotalPagar = NegHorasExtras.TotalDelPeriodo(pagoTotal);
            pantalla.TotalHoras50 = horas50;
            pantalla.TotalHoras100 = horas100;
            pantalla.TotalHoras = horas;
        }

        /// <summary>
        /// El sueldo congelado a usar como respaldo al reabrir un periodo: el
        /// que ya tenia la fila existente, o cero si es la primera vez que el
        /// colaborador aparece en el periodo -no hay nada que congelar todavia,
        /// y ese es el caso legitimo de sin-sueldo si el maestro tampoco da
        /// ninguno-.
        ///
        /// Separado en su propio metodo, puro, porque es exactamente el valor
        /// que se perdia antes de esta correccion: la fila que arma AbrirPeriodo
        /// nace fresca de LeerInsumos con SalarioBaseSnapshot en 0, y sin este
        /// paso explicito ese 0 viajaba a AplicarCalculo como si fuera el
        /// congelado real.
        /// </summary>
        public static decimal SalarioCongeladoAlReabrir(EntHeFila filaExistente)
        {
            return filaExistente == null ? 0m : filaExistente.SalarioBaseSnapshot;
        }

        /// <summary>
        /// Decide con que horas se queda la fila. Tres entradas y una regla: la
        /// correccion manual gana siempre; si nadie corrigio, mandan las tareas.
        ///
        /// Que la siembra se repita en cada apertura no es un descuido: al 2026-09-16
        /// el 80% de las horas del periodo seguia en "Solicitado", asi que volver a
        /// abrir es como entran las que se aprobaron despues.
        ///
        /// La Observacion se preserva pase lo que pase: es una nota de quien reviso,
        /// no un numero que se recalcule.
        /// </summary>
        public static void ResolverHoras(EntHeFila fila, EntHeFila anterior, EntHeFila aprobadas)
        {
            if (fila == null) { return; }

            if (anterior != null) { fila.Observacion = anterior.Observacion; }

            bool esManual = anterior != null
                            && string.Equals(anterior.HorasOrigen, "Manual", StringComparison.OrdinalIgnoreCase);

            if (esManual)
            {
                fila.Horas50 = anterior.Horas50;
                fila.Horas100 = anterior.Horas100;
                fila.HorasOrigen = "Manual";
                return;
            }

            fila.Horas50 = aprobadas != null ? aprobadas.Horas50 : 0m;
            fila.Horas100 = aprobadas != null ? aprobadas.Horas100 : 0m;
            fila.HorasOrigen = "Tareas";
        }

        /// <summary>
        /// Si dos filas son identicas en todo lo que Sp_RTA_HeGuardarFila
        /// escribe -los snapshots del colaborador, el divisor, los tres
        /// valores hora, las horas, los totales, la observacion y el origen
        /// de las horas-.
        ///
        /// Reabrir un periodo llama a GuardarFila para cada colaborador sin
        /// comparar antes, y el UPDATE del procedimiento pisa
        /// Fec_Modificacion y Usu_Modificacion en cada pasada. Como reabrir es
        /// el mecanismo de autocuracion que se dispara cada vez que alguien
        /// incorpora a un colaborador nuevo, el efecto acumulado es que esas
        /// dos columnas dejan de significar "quien toco estas horas" para
        /// pasar a significar "quien reabrio el mes por ultima vez" -y la
        /// fase 3 monta la auditoria encima de esa idea-. Esta comparacion es
        /// lo que evita escribir cuando no hace falta.
        ///
        /// Los decimales se comparan con == de decimal, que es por valor
        /// -1.50m y 1.5m son iguales-, no por representacion. Las cadenas
        /// tratan null y "" como el mismo valor: el DAO nunca devuelve null
        /// para estas columnas, pero una fila armada en memoria si podria
        /// llegar sin inicializar.
        ///
        /// HorasOrigen entra en la comparacion porque el UPDATE tambien lo
        /// escribe. Si faltara aqui, una fila que solo cambia de origen -la
        /// que alguien corrigio a mano y despues vuelve a ser sembrable, o al
        /// reves- se daria por igual y no se escribiria: la siembra se
        /// perderia sin un solo error.
        /// </summary>
        public static bool FilaSinCambios(EntHeFila nueva, EntHeFila anterior)
        {
            if (nueva == null || anterior == null) { return false; }

            return TextoIgual(nueva.CedulaSnapshot, anterior.CedulaSnapshot)
                && TextoIgual(nueva.NombreSnapshot, anterior.NombreSnapshot)
                && TextoIgual(nueva.EmpresaSnapshot, anterior.EmpresaSnapshot)
                && TextoIgual(nueva.CargoSnapshot, anterior.CargoSnapshot)
                && nueva.JornadaHorasDiaSnapshot == anterior.JornadaHorasDiaSnapshot
                && nueva.SalarioBaseSnapshot == anterior.SalarioBaseSnapshot
                && nueva.AplicaHESnapshot == anterior.AplicaHESnapshot
                && nueva.Divisor == anterior.Divisor
                && nueva.ValorHoraOrdinaria == anterior.ValorHoraOrdinaria
                && nueva.ValorHora50 == anterior.ValorHora50
                && nueva.ValorHora100 == anterior.ValorHora100
                && nueva.Horas50 == anterior.Horas50
                && nueva.Horas100 == anterior.Horas100
                && nueva.Total50 == anterior.Total50
                && nueva.Total100 == anterior.Total100
                && nueva.TotalHoras == anterior.TotalHoras
                && nueva.TotalHE == anterior.TotalHE
                && TextoIgual(nueva.Observacion, anterior.Observacion)
                && TextoIgual(nueva.HorasOrigen, anterior.HorasOrigen);
        }

        private static bool TextoIgual(string a, string b)
        {
            return (a ?? "") == (b ?? "");
        }

        /// <summary>
        /// Traduce el codigo del procedimiento de cierre a algo que una persona
        /// pueda leer. Funcion pura y publica para poder probarla sin base.
        /// </summary>
        public static string MensajeDeCierre(int codigo, int filasConProblema)
        {
            if (codigo == -1) { return "Ese periodo no existe."; }
            if (codigo == -2) { return "Solo se puede cerrar un periodo abierto."; }

            if (codigo == -3)
            {
                string cuantas = filasConProblema == 1
                                 ? "Hay 1 fila"
                                 : "Hay " + filasConProblema.ToString() + " filas";

                return cuantas + " sin sueldo vigente, con el valor hora en cero. "
                     + "Corrija el sueldo de esas personas antes de cerrar: el período "
                     + "quedaría congelado con un pago que nadie calculó.";
            }

            return "No se pudo cerrar el periodo.";
        }

        /// <summary>Cierra el periodo y devuelve la pantalla ya en estado cerrado.</summary>
        public static EntRespuesta CerrarPeriodo(int idPeriodo, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int filasConProblema;
            int codigo = DaoHorasExtras.CerrarPeriodo(idPeriodo, usuario, ip, out filasConProblema);

            if (codigo != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = MensajeDeCierre(codigo, filasConProblema);
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntRespuesta pantalla = CargarPantalla(idPeriodo);

            if (pantalla.estado == "1")
            {
                pantalla.mensaje = "Periodo cerrado. Ya no admite cambios.";
                pantalla.tipoMensaje = "success";
            }

            return pantalla;
        }

        /// <summary>
        /// Reabre un periodo cerrado.
        ///
        /// NO comprueba perfiles: eso es de la capa web, que es la unica que
        /// conoce la sesion. Este metodo asume que quien llega aqui ya tiene
        /// permiso, y por eso el handler tiene que comprobarlo ANTES.
        /// </summary>
        public static EntRespuesta ReabrirPeriodo(int idPeriodo, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int codigo = DaoHorasExtras.ReabrirPeriodo(idPeriodo, usuario, ip);

            if (codigo == -1)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (codigo != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Solo se puede reabrir un periodo cerrado.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntRespuesta pantalla = CargarPantalla(idPeriodo);

            if (pantalla.estado == "1")
            {
                pantalla.mensaje = "Periodo reabierto. Vuelve a admitir cambios.";
                pantalla.tipoMensaje = "success";
            }

            return pantalla;
        }

        /// <summary>
        /// Deja constancia de que alguien descargo el periodo en Excel: la unica
        /// accion de este modulo que saca el sueldo de las 62 personas de un
        /// clic, y hasta ahora la unica sin rastro -la edicion celda por celda
        /// ya lo tenia, dentro de Sp_RTA_HeGuardarFila-.
        ///
        /// NO comprueba perfiles, igual que ReabrirPeriodo: eso ya lo hizo el
        /// handler antes de llegar aqui. Una excepcion de la base se deja
        /// subir tal cual -no hay try/catch aqui- para que el handler decida:
        /// sin registro, no hay descarga.
        /// </summary>
        public static void RegistrarDescarga(int idPeriodo, string usuario, string ip)
        {
            DaoHorasExtras.RegistrarDescarga(idPeriodo, usuario, ip);
        }

        /// <summary>
        /// Abre un periodo -un rango de fechas, ya no un mes calendario-: lo
        /// crea si no existe y le arma el snapshot, una fila por colaborador
        /// activo, con las horas extras ya aprobadas del rango sembradas.
        ///
        /// Correrlo dos veces sobre el mismo rango es inofensivo y ademas
        /// necesario: si la primera vez fallo a medias, la segunda completa lo
        /// que falte, y sobre todo entran las horas que se aprobaron despues
        /// de la ultima apertura -al 2026-09-16 el 80% del periodo seguia en
        /// "Solicitado"-. Lo que NO pisa es una correccion manual: eso lo
        /// decide ResolverHoras, fila por fila.
        /// </summary>
        public static EntRespuesta AbrirPeriodo(DateTime inicio, DateTime fin, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int idPeriodo;
            int creado = DaoHorasExtras.CrearPeriodo(inicio, fin, usuario, ip, out idPeriodo);

            /* El solapamiento no es "no se pudo": es un rango que pisa a otro
               periodo, y quien lo vea tiene que saber que le paso para poder
               corregir las fechas en vez de reintentar lo mismo. */
            if (creado == -5)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El rango se cruza con otro periodo ya abierto.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (creado != 0 || idPeriodo <= 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No se pudo abrir el periodo.";
                respuesta.tipoMensaje = "danger";
                return respuesta;
            }

            EntHePantalla existente = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (existente.Periodo != null && !existente.Periodo.EstaAbierto)
            {
                return CargarPantalla(idPeriodo);
            }

            Dictionary<long, EntHeFila> yaEstan = new Dictionary<long, EntHeFila>();
            foreach (EntHeFila f in existente.Filas) { yaEstan[f.IdEmpleado] = f; }

            EntHeParametros parametros = NegHeParametros.Vigentes();

            /* El corte es el ULTIMO dia del rango. Con el primero, un ajuste
               salarial que entra en vigencia a mitad de periodo quedaria fuera
               y la persona cobraria el periodo entero al sueldo anterior. */
            DateTime corte = fin;

            List<EntHeFila> colaboradores;
            Dictionary<long, List<EntHeSalario>> salarios;
            DaoHorasExtras.LeerInsumos(corte, out colaboradores, out salarios);

            /* Las horas ya aprobadas del rango, una sola lectura para las 62
               filas. Solo trae a quien tiene alguna: el que no esta en el
               diccionario no registro ninguna, y ResolverHoras lo traduce a
               cero. */
            Dictionary<long, EntHeFila> aprobadas = DaoHorasExtras.LeerHorasAprobadas(inicio, fin);

            /* GuardarFila puede fallar fila por fila -por ejemplo si alguien
               cierra el periodo a mitad del bucle-. No se detalla cual: basta
               con cuantas, porque quien lo vea vuelve a abrir el periodo, que
               es idempotente y se autocura. Callar el fallo aqui seria peor:
               CargarPantalla devolveria exito con lo que haya quedado. */
            int filasConError = 0;

            /* Cuantas filas se quedaron con el sueldo congelado del periodo
               porque el maestro no dio ninguno vigente al corte. No es un
               error -el pago quedo bien calculado-, pero TieneAdvertencia no
               se persiste ni se lee de vuelta: este aviso es la unica senal
               que le llega a quien reabrio el periodo. */
            int filasConSalarioCongelado = 0;

            /* Cuanto entro de nuevo respecto a lo que ya habia. Es la unica
               senal de que valio la pena recalcular: sin esto, para saber si
               alguien aprobo algo desde la ultima vez hay que comparar dos
               Excel a mano.

               Se cuenta solo lo que SUBE. Una fila que baja -porque una
               aprobacion se revirtio- no resta aqui: mezclar las dos
               direcciones en un numero daria cero cuando entraron cinco horas
               y salieron cinco, que es justo el caso que hay que ver. */
            int personasQueEntraron = 0;
            decimal horasQueEntraron = 0m;

            foreach (EntHeFila fila in colaboradores)
            {
                EntHeFila anterior = yaEstan.ContainsKey(fila.IdEmpleado) ? yaEstan[fila.IdEmpleado] : null;

                ResolverHoras(fila, anterior,
                              aprobadas.ContainsKey(fila.IdEmpleado) ? aprobadas[fila.IdEmpleado] : null);

                /* Antes del descarte por FilaSinCambios a proposito: si las
                   horas cambiaron, la fila no se descarta, asi que contar aqui
                   da lo mismo y se lee al lado de ResolverHoras, que es quien
                   acaba de decidirlas. */
                decimal horasAntes = anterior != null ? anterior.Horas50 + anterior.Horas100 : 0m;
                decimal horasAhora = fila.Horas50 + fila.Horas100;

                if (horasAhora > horasAntes)
                {
                    horasQueEntraron += horasAhora - horasAntes;
                    if (horasAntes == 0m) { personasQueEntraron++; }
                }

                List<EntHeSalario> historial = salarios.ContainsKey(fila.IdEmpleado)
                                               ? salarios[fila.IdEmpleado]
                                               : new List<EntHeSalario>();

                decimal salarioDelMaestro = NegHorasExtras.SalarioVigente(historial, corte);
                decimal salarioCongelado = SalarioCongeladoAlReabrir(anterior);

                AplicarCalculo(fila, salarioDelMaestro, salarioCongelado, parametros);

                /* Si la fila calculada es identica a la que ya esta en la
                   base, no se escribe: ver FilaSinCambios para el porque. Una
                   fila que no se escribe tampoco cuenta como que uso el
                   sueldo congelado -si contara, el conteo volveria a hablar
                   de las 62 en vez de las que de verdad se tocaron-. */
                if (FilaSinCambios(fila, anterior)) { continue; }

                if (salarioDelMaestro <= 0m && salarioCongelado > 0m) { filasConSalarioCongelado++; }

                /* auditar = false: son 62 filas de golpe por cada apertura o
                   reapertura. Auditarlas llenaria la tabla de ruido y
                   enterraria los cambios reales, que son lo unico que
                   importa en una disputa de nomina. */
                int guardado = DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip, false, fila.HorasOrigen);
                if (guardado != 0) { filasConError++; }
            }

            EntRespuesta resultado = CargarPantalla(idPeriodo, parametros);

            if (resultado.estado == "1" && (filasConError > 0 || filasConSalarioCongelado > 0))
            {
                List<string> avisos = new List<string>();

                if (filasConError > 0)
                {
                    avisos.Add(filasConError + " fila(s) no se pudieron guardar");
                }

                if (filasConSalarioCongelado > 0)
                {
                    avisos.Add(filasConSalarioCongelado + " fila(s) sin sueldo vigente al corte usaron el sueldo congelado del período");
                }

                resultado.mensaje = string.Join("; ", avisos) + ". Vuelva a abrir el período si hace falta reintentar.";
                resultado.tipoMensaje = "warning";
            }
            else if (resultado.estado == "1")
            {
                resultado.mensaje = ResumenDeLaSiembra(personasQueEntraron, horasQueEntraron);
                resultado.tipoMensaje = horasQueEntraron > 0m ? "success" : "info";
            }

            return resultado;
        }

        /// <summary>
        /// Que trajo esta apertura. Se dice SIEMPRE, tambien cuando no entro
        /// nada: «no entro nada nuevo» y «no llegue a consultar» se ven igual
        /// en la pantalla si el unico aviso es el que aparece cuando hay algo,
        /// y confundirlos lleva a dar por cerrado un periodo al que todavia le
        /// faltan aprobaciones.
        ///
        /// Las horas van con dos decimales y punto decimal invariante: es el
        /// mismo criterio que usa el resto del modulo para no depender de la
        /// cultura del servidor.
        /// </summary>
        public static string ResumenDeLaSiembra(int personas, decimal horas)
        {
            if (horas <= 0m)
            {
                return "No se aprobaron horas nuevas en este rango desde la última vez.";
            }

            string texto = "Entraron "
                           + horas.ToString("0.00", CultureInfo.InvariantCulture)
                           + " horas aprobadas";

            if (personas == 1)
            {
                texto += ", de 1 colaborador que antes no tenía ninguna";
            }
            else if (personas > 1)
            {
                texto += ", de " + personas.ToString(CultureInfo.InvariantCulture)
                         + " colaboradores que antes no tenían ninguna";
            }

            return texto + ".";
        }

        /// <summary>
        /// El periodo, sus filas y el tablero, listos para la pantalla. Sin
        /// parametros a mano: los lee una sola vez, para el llamador directo
        /// -la accion CargarPeriodo del handler- que no calculo nada antes y
        /// no tiene ninguno cargado.
        /// </summary>
        public static EntRespuesta CargarPantalla(int idPeriodo)
        {
            return CargarPantalla(idPeriodo, null);
        }

        /// <summary>
        /// Misma carga, pero recibe los parametros ya leidos. AbrirPeriodo y
        /// GuardarHoras los necesitan de todos modos para AplicarCalculo, y
        /// pasarlos aqui evita una segunda lectura de HE_Parametro en la misma
        /// peticion -null significa "no los tengo, leelos vos"-.
        /// </summary>
        public static EntRespuesta CargarPantalla(int idPeriodo, EntHeParametros parametros)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntHePantalla pantalla = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (pantalla.Periodo == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntHeParametros p = parametros ?? NegHeParametros.Vigentes();
            pantalla.Factor50 = p.Factor50;
            pantalla.Factor100 = p.Factor100;

            SumarTotales(pantalla);

            respuesta.estado = "1";
            respuesta.resultado = pantalla;
            respuesta.mensaje = "";
            respuesta.tipoMensaje = "success";
            return respuesta;
        }

        /// <summary>
        /// Guarda las horas de una fila.
        ///
        /// NO recibe ningun total del cliente y no usaria uno aunque se lo
        /// mandaran: relee el snapshot de la base, recalcula, guarda sus propios
        /// numeros y los devuelve para que la grilla se repinte con ellos. Si el
        /// JavaScript calculo distinto, el usuario ve el numero saltar y la
        /// divergencia se vuelve visible en vez de silenciosa.
        /// </summary>
        public static EntRespuesta GuardarHoras(int idPeriodo, long idEmpleado,
                                                decimal horas50, decimal horas100,
                                                string observacion, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntHePantalla pantalla = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (pantalla.Periodo == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (!pantalla.Periodo.EstaAbierto)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El periodo esta cerrado: ya no admite cambios.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntHeFila fila = null;
            foreach (EntHeFila f in pantalla.Filas)
            {
                if (f.IdEmpleado == idEmpleado) { fila = f; break; }
            }

            if (fila == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Esa persona no esta en este periodo.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            /* Tope del lado del servidor: nunca confiar solo en el cliente.
               Un dedazo de 10000 horas pasaria el parseo y el calculo si nadie
               lo detiene aqui. No es como las horas negativas -que se marcan y
               se pagan en cero porque son dato corrupto-: esto es un error de
               captura que hay que devolver para que lo corrijan. */
            if (ExcedeTopePorCelda(horas50) || ExcedeTopePorCelda(horas100))
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Las horas de una celda no pueden superar " + TopeHorasPorCelda.ToString("0") + ".";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            fila.Horas50 = horas50;
            fila.Horas100 = horas100;
            fila.Observacion = observacion ?? "";

            /* Aqui hay una persona escribiendo, asi que la fila pasa a ser
               Manual. Es lo unico que hace que su correccion sobreviva a la
               proxima apertura: ResolverHoras respeta lo manual y vuelve a
               sembrar todo lo demas desde las horas aprobadas. */
            fila.HorasOrigen = "Manual";

            /* El 6.6 funcional manda revalidar la elegibilidad y el sueldo contra
               la base al guardar, porque pudieron cambiar desde que se cargo la
               pantalla. No choca con el snapshot: el snapshot protege al periodo
               CERRADO de que lo muevan por detras, y un periodo cerrado ni
               siquiera llega aqui -la guarda de arriba lo rechaza-. Mientras
               esta abierto, que un sueldo corregido se aplique es lo correcto:
               si no, alguien arregla un sueldo mal cargado y el periodo del mes
               sigue pagando sobre el equivocado, sin avisar. */
            DateTime corte = pantalla.Periodo.FechaFin;

            List<EntHeFila> colaboradores;
            Dictionary<long, List<EntHeSalario>> salarios;
            DaoHorasExtras.LeerInsumos(corte, out colaboradores, out salarios);

            bool activoEnMaestro = false;

            foreach (EntHeFila actual in colaboradores)
            {
                if (actual.IdEmpleado != idEmpleado) { continue; }

                activoEnMaestro = true;
                fila.CedulaSnapshot = actual.CedulaSnapshot;
                fila.NombreSnapshot = actual.NombreSnapshot;
                fila.AplicaHESnapshot = actual.AplicaHESnapshot;
                fila.JornadaHorasDiaSnapshot = actual.JornadaHorasDiaSnapshot;
                fila.DivisorManual = actual.DivisorManual;
                fila.CargoSnapshot = actual.CargoSnapshot;
                fila.EmpresaSnapshot = actual.EmpresaSnapshot;
                break;
            }

            /* Una sola via para resolver el sueldo, sin ramificar por
               activoEnMaestro: si el colaborador no esta activo, no aparece
               en ninguno de los dos result sets de Sp_RTA_HeInsumos y
               "historial" sale vacio solo. Si esta activo pero no tiene
               ningun sueldo vigente a la fecha de corte -todos sus sueldos
               dados de baja, o su unica vigencia es posterior al corte-,
               "historial" trae filas pero SalarioVigente da cero igual. Los
               dos casos llegan al mismo cero, y el sueldo congelado -el que
               ya traia la fila antes de este guardado- se le pasa explicito
               a AplicarCalculo, que es quien decide si hace falta usarlo. */
            List<EntHeSalario> historial = salarios.ContainsKey(idEmpleado)
                                           ? salarios[idEmpleado]
                                           : new List<EntHeSalario>();

            decimal salarioDelMaestro = NegHorasExtras.SalarioVigente(historial, corte);
            decimal salarioCongelado = fila.SalarioBaseSnapshot;
            EntHeParametros parametros = NegHeParametros.Vigentes();

            AplicarCalculo(fila, salarioDelMaestro, salarioCongelado, parametros);

            /* Se detecta aqui solo para avisar -la fila ya quedo bien
               calculada por AplicarCalculo-. La distincion entre "no esta
               activo" y "esta activo pero sin sueldo vigente" es solo para
               que el mensaje diga la causa correcta. */
            bool seUsoSnapshot = salarioDelMaestro <= 0m && salarioCongelado > 0m;

            /* auditar = true: es una persona editando una fila, y registrar
               eso es lo que el 8 funcional exige que no falte -el Excel no
               dejaba rastro de quien escribio una hora-. */
            int guardado = DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip, true, fila.HorasOrigen);

            if (guardado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El periodo esta cerrado: ya no admite cambios.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (guardado != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No se pudo guardar.";
                respuesta.tipoMensaje = "danger";
                return respuesta;
            }

            EntRespuesta resultado = CargarPantalla(idPeriodo, parametros);

            if (seUsoSnapshot && resultado.estado == "1")
            {
                resultado.mensaje = activoEnMaestro
                    ? "No se encontró un sueldo vigente a la fecha de corte: se usó el sueldo congelado del período."
                    : "Este colaborador ya no está activo en el maestro: se usó el sueldo congelado del período.";
                resultado.tipoMensaje = "warning";
            }

            return resultado;
        }
    }
}
