using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;

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
        /// El corte de un periodo es el ULTIMO dia del mes. Con el primero, un
        /// ajuste salarial que entra en vigencia a mitad de mes quedaria fuera
        /// y la persona cobraria el mes entero al sueldo anterior.
        /// </summary>
        public static DateTime UltimoDiaDelMes(int anio, int mes)
        {
            return new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
        }

        /// <summary>
        /// Rellena una fila a partir de su salario y los parametros. Es el unico
        /// punto donde la pantalla toca el calculo, y delega entero en
        /// NegHorasExtras: aqui no hay ni una division ni un factor.
        /// </summary>
        public static void AplicarCalculo(EntHeFila fila, decimal salario, EntHeParametros parametros)
        {
            if (fila == null) { return; }

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
        /// Abre un periodo: lo crea si no existe y le arma el snapshot, una fila
        /// por colaborador activo.
        ///
        /// Correrlo dos veces sobre el mismo mes es inofensivo y ademas util: si
        /// la primera vez fallo a medias, la segunda completa lo que falte. Lo
        /// que NO hace es pisar las horas ya digitadas de una fila que existe,
        /// porque Sp_RTA_HeGuardarFila recibe las horas que se le pasan y aqui
        /// solo se le pasan las de la fila que ya estaba.
        /// </summary>
        public static EntRespuesta AbrirPeriodo(int anio, int mes, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int idPeriodo;
            int creado = DaoHorasExtras.CrearPeriodo(anio, mes, usuario, ip, out idPeriodo);

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
            DateTime corte = UltimoDiaDelMes(anio, mes);

            List<EntHeFila> colaboradores;
            Dictionary<long, List<EntHeSalario>> salarios;
            DaoHorasExtras.LeerInsumos(corte, out colaboradores, out salarios);

            /* GuardarFila puede fallar fila por fila -por ejemplo si alguien
               cierra el periodo a mitad del bucle-. No se detalla cual: basta
               con cuantas, porque quien lo vea vuelve a abrir el periodo, que
               es idempotente y se autocura. Callar el fallo aqui seria peor:
               CargarPantalla devolveria exito con lo que haya quedado. */
            int filasConError = 0;

            foreach (EntHeFila fila in colaboradores)
            {
                if (yaEstan.ContainsKey(fila.IdEmpleado))
                {
                    fila.Horas50 = yaEstan[fila.IdEmpleado].Horas50;
                    fila.Horas100 = yaEstan[fila.IdEmpleado].Horas100;
                    fila.Observacion = yaEstan[fila.IdEmpleado].Observacion;
                }

                List<EntHeSalario> historial = salarios.ContainsKey(fila.IdEmpleado)
                                               ? salarios[fila.IdEmpleado]
                                               : new List<EntHeSalario>();

                AplicarCalculo(fila, NegHorasExtras.SalarioVigente(historial, corte), parametros);

                int guardado = DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip);
                if (guardado != 0) { filasConError++; }
            }

            EntRespuesta resultado = CargarPantalla(idPeriodo);

            if (filasConError > 0 && resultado.estado == "1")
            {
                resultado.mensaje = filasConError + " fila(s) no se pudieron guardar. Vuelva a abrir el período para reintentar.";
                resultado.tipoMensaje = "warning";
            }

            return resultado;
        }

        /// <summary>El periodo, sus filas y el tablero, listos para la pantalla.</summary>
        public static EntRespuesta CargarPantalla(int idPeriodo)
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

            /* El 6.6 funcional manda revalidar la elegibilidad y el sueldo contra
               la base al guardar, porque pudieron cambiar desde que se cargo la
               pantalla. No choca con el snapshot: el snapshot protege al periodo
               CERRADO de que lo muevan por detras, y un periodo cerrado ni
               siquiera llega aqui -la guarda de arriba lo rechaza-. Mientras
               esta abierto, que un sueldo corregido se aplique es lo correcto:
               si no, alguien arregla un sueldo mal cargado y el periodo del mes
               sigue pagando sobre el equivocado, sin avisar. */
            DateTime corte = UltimoDiaDelMes(pantalla.Periodo.Anio, pantalla.Periodo.Mes);

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

            decimal salario;

            if (activoEnMaestro)
            {
                List<EntHeSalario> historial = salarios.ContainsKey(idEmpleado)
                                               ? salarios[idEmpleado]
                                               : new List<EntHeSalario>();

                salario = NegHorasExtras.SalarioVigente(historial, corte);
            }
            else
            {
                /* Ya no esta activo en el maestro -Sp_RTA_HeInsumos filtra
                   Estado = '1' tanto para colaboradores como para el
                   historial de sueldos-, asi que no hay con que revalidar. No
                   se recalcula contra un historial vacio: eso daria salario
                   cero, fila marcada con advertencia y el pago que ya tenia
                   se borraria. Se usa el sueldo congelado del periodo -la
                   jornada, el divisor y AplicaHESnapshot quedan como
                   estaban- porque alguien que salio a mitad de mes trabajo
                   horas antes de salir y Nomina tiene que poder pagarselas. */
                salario = fila.SalarioBaseSnapshot;
            }

            AplicarCalculo(fila, salario, NegHeParametros.Vigentes());

            int guardado = DaoHorasExtras.GuardarFila(idPeriodo, fila, usuario, ip);

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

            EntRespuesta resultado = CargarPantalla(idPeriodo);

            if (!activoEnMaestro && resultado.estado == "1")
            {
                resultado.mensaje = "Este colaborador ya no está activo en el maestro: se usó el sueldo congelado del período.";
                resultado.tipoMensaje = "warning";
            }

            return resultado;
        }
    }
}
