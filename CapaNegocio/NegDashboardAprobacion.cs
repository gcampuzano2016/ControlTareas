using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// Lo que ocurre entre el dato agregado que devuelve la base y lo que se
    /// dibuja. No toca la base ni HttpContext: es la unica parte del dashboard
    /// que se puede probar sin levantar nada.
    ///
    /// Aca NO se parsea Det_Tiempo. Eso lo hace el procedimiento, y una copia en
    /// C# seria una segunda fuente de verdad para la misma regla; el dia que una
    /// cambie y la otra no, el grafico y la tabla de la misma pantalla diran
    /// cifras distintas y no habra forma de saber cual miente.
    ///
    /// Por lo mismo, la conversion de minutos a horas y el texto de la demora
    /// viven ACA y en ningun otro lado. Estuvieron un tiempo tambien en
    /// dashboardAprobacion.js, y las dos copias ya daban numeros distintos: 75
    /// minutos eran 1,2 horas en C# -Math.Round redondea al par- y 1,3 en
    /// JavaScript. Cargar deja los valores ya convertidos en la entidad y el
    /// navegador solo los pinta.
    /// </summary>
    public static class NegDashboardAprobacion
    {
        /// <summary>Etiqueta "Otras" de la porcion que agrupa la cola larga.</summary>
        private const string EtiquetaOtras = "Otras";

        /// <summary>Lo que dice la tarjeta de demora cuando no hay con que calcularla.</summary>
        private const string SinDato = "sin datos";

        /// <summary>
        /// Los cinco conjuntos del dashboard, con las empresas ya reducidas a un
        /// top 10 mas "Otras" y con las horas y los textos ya resueltos.
        ///
        /// El recorte se hace aca y no en SQL: la regla esta probada en
        /// TopConOtras, y en SQL quedaria sin prueba y repartida en dos
        /// lenguajes. Y aca y no en el handler, para que la capa web no tenga que
        /// orquestar dos llamadas ni conocer el tope.
        /// </summary>
        public static EntDashboardAprobacion Cargar(string idUsuarioJefe, string fechaDesde, string fechaHasta)
        {
            EntDashboardAprobacion datos =
                DaoDashboardAprobacion.Cargar(idUsuarioJefe, fechaDesde, fechaHasta);

            /* Solo aprobadas: hasta hoy este grafico sumaba todos los estados y
               mostraba un numero que no correspondia a ninguna de las dos
               tarjetas de arriba. */
            datos.Empresas = TopConOtras(SoloAprobadas(datos.Empresas), 10);

            Convertir(datos);

            return datos;
        }

        /// <summary>
        /// Deja en la entidad lo que la pantalla muestra: las horas y los dos
        /// textos de demora.
        ///
        /// Va DESPUES de TopConOtras a proposito: la porcion "Otras" se arma
        /// sumando minutos, y convertir antes obligaria a sumar horas ya
        /// redondeadas, que no da lo mismo que redondear la suma.
        /// </summary>
        private static void Convertir(EntDashboardAprobacion datos)
        {
            if (datos == null) { return; }

            if (datos.Totales != null)
            {
                datos.Totales.HorasAprobadas  = HorasDecimales(datos.Totales.MinutosAprobados);
                datos.Totales.HorasPendientes = HorasDecimales(datos.Totales.MinutosPendientes);
                datos.Totales.HorasOtros      = HorasDecimales(datos.Totales.MinutosOtros);
            }

            if (datos.Semanas != null)
            {
                foreach (EntDashboardSemana s in datos.Semanas)
                {
                    s.HorasAprobadas  = HorasDecimales(s.MinutosAprobados);
                    s.HorasPendientes = HorasDecimales(s.MinutosPendientes);
                }
            }

            if (datos.Responsables != null)
            {
                foreach (EntDashboardResponsable r in datos.Responsables)
                {
                    r.HorasAprobadas  = HorasDecimales(r.MinutosAprobados);
                    r.HorasPendientes = HorasDecimales(r.MinutosPendientes);
                }
            }

            if (datos.Empresas != null)
            {
                foreach (EntDashboardEmpresa e in datos.Empresas)
                {
                    e.Horas = HorasDecimales(e.Minutos);
                }
            }

            if (datos.Demora != null)
            {
                datos.Demora.TextoDemoraPromedio =
                    TextoDemoraPromedio(datos.Demora.DiasPromedio, datos.Demora.AprobadasConFecha);
                /* datos.Totales puede venir nulo -el resto del metodo lo
                   comprueba-, y en ese caso no hay con que distinguir el cero
                   bueno del cero por ausencia: se toma 1 para caer en la rama
                   de siempre y no inventar un "sin pendientes" que no consta. */
                int pendientes = (datos.Totales != null) ? datos.Totales.PersonasDiaPend : 1;

                datos.Demora.TextoMasViejoPendiente =
                    TextoMasViejo(datos.Demora.DiasMasViejoPendiente, pendientes);
            }
        }

        /// <summary>
        /// Minutos a horas con un decimal. Un decimal y no dos: son horas de
        /// trabajo en un grafico, y el segundo decimal es ruido que solo hace la
        /// etiqueta mas larga.
        ///
        /// Los negativos dan cero: no deberian existir, pero una barra hacia
        /// abajo en un grafico de horas no la sabe leer nadie.
        /// </summary>
        public static decimal HorasDecimales(int minutos)
        {
            if (minutos <= 0) { return 0m; }

            return System.Math.Round(minutos / 60m, 1);
        }

        /// <summary>
        /// La misma lista, con lo aprobado puesto en Minutos.
        ///
        /// Existe para no cambiarle el criterio a TopConOtras, que ordena y
        /// agrupa por Minutos y tiene pruebas que dependen de eso. Devuelve
        /// copias: las entidades originales viajan al JSON de la pantalla y
        /// pisarlas cambiaria lo que ve el navegador.
        /// </summary>
        public static List<EntDashboardEmpresa> SoloAprobadas(List<EntDashboardEmpresa> lista)
        {
            List<EntDashboardEmpresa> resultado = new List<EntDashboardEmpresa>();
            if (lista == null) { return resultado; }

            foreach (EntDashboardEmpresa e in lista)
            {
                resultado.Add(new EntDashboardEmpresa
                {
                    Empresa = e.Empresa,
                    Minutos = e.MinutosAprobados,
                    MinutosAprobados = e.MinutosAprobados
                });
            }

            return resultado;
        }

        /// <summary>
        /// Las <paramref name="tope"/> empresas con mas minutos, y el resto sumado
        /// en una porcion "Otras".
        ///
        /// Una torta de 71 porciones no se lee. Agrupar la cola en vez de
        /// descartarla deja ver de un vistazo cuanto pesa: si "Otras" es la
        /// porcion mas grande, el top 10 no estaba contando la historia.
        ///
        /// Ordena antes de cortar: si la lista llega desordenada, quedarse con
        /// los primeros daria un "top" que no es el de mas horas.
        /// </summary>
        public static List<EntDashboardEmpresa> TopConOtras(List<EntDashboardEmpresa> lista, int tope)
        {
            List<EntDashboardEmpresa> resultado = new List<EntDashboardEmpresa>();
            if (lista == null) { return resultado; }

            List<EntDashboardEmpresa> ordenada = new List<EntDashboardEmpresa>(lista);
            ordenada.Sort((a, b) => b.Minutos.CompareTo(a.Minutos));

            /* Con un tope que no acota, devolver todo sin agrupar es menos
               sorprendente que una sola porcion "Otras" con el total. */
            if (tope <= 0 || ordenada.Count <= tope) { return ordenada; }

            int minutosDeLaCola = 0;

            for (int i = 0; i < ordenada.Count; i++)
            {
                if (i < tope) { resultado.Add(ordenada[i]); }
                else { minutosDeLaCola += ordenada[i].Minutos; }
            }

            resultado.Add(new EntDashboardEmpresa
            {
                Empresa = EtiquetaOtras,
                Minutos = minutosDeLaCola
            });

            return resultado;
        }

        /// <summary>
        /// El promedio de demora, en texto.
        ///
        /// Con <paramref name="aprobadasConFecha"/> en cero el promedio no
        /// existe: no hay ninguna tarea aprobada con fecha sobre la cual
        /// calcularlo. El procedimiento devuelve cero en ese caso, y cero se
        /// escribe "hoy": el peor dato posible se presentaria como el mejor
        /// resultado posible. Por eso hay que mirar las dos cifras juntas y no
        /// solo el promedio.
        /// </summary>
        public static string TextoDemoraPromedio(decimal dias, int aprobadasConFecha)
        {
            if (aprobadasConFecha <= 0) { return SinDato; }

            return TextoDemora(dias);
        }

        /// <summary>
        /// Los dias de demora, en texto. "hoy", "1 dia", "N dias".
        ///
        /// Recibe decimal y no entero porque el promedio llega con un decimal:
        /// redondearlo aca para escribirlo devolveria el sesgo a la baja que el
        /// procedimiento acaba de sacar.
        ///
        /// Los negativos se leen como "hoy": una aprobacion fechada antes del
        /// registro existe en datos viejos, y mostrar "-3 dias" haria que quien
        /// lo vea desconfie de todo el tablero por un caso que no importa.
        /// </summary>
        /// <summary>
        /// Lo mismo que TextoDemora para la tarjeta de "lo mas viejo sin
        /// aprobar", pero distinguiendo el cero bueno del cero por ausencia.
        ///
        /// El procedimiento calcula DiasMasViejoPendiente como un DATEDIFF
        /// contra MIN(Fecha) de lo pendiente, envuelto en ISNULL(..., 0). Sin
        /// nada pendiente el MIN es NULL y la tarjeta terminaba diciendo "hoy",
        /// que se lee como "hay algo esperando desde hoy" cuando en realidad no
        /// hay nada esperando. Es el mismo defecto de lectura que TextoDemoraPromedio
        /// resuelve en la tarjeta de al lado.
        ///
        /// Se usa PersonasDiaPend y no los dias: cuenta las combinaciones
        /// responsable-dia en estado pendiente, asi que cero significa que no
        /// hay ni una fila pendiente en el rango.
        /// </summary>
        public static string TextoMasViejo(decimal dias, int personasDiaPend)
        {
            if (personasDiaPend <= 0) { return "sin pendientes"; }
            return TextoDemora(dias);
        }

        public static string TextoDemora(decimal dias)
        {
            /* Se redondea ANTES de decidir, no despues. El texto se arma con
               "0.#", que ya redondea a un decimal: sin esto, 1,04 no entra por
               la rama del singular -no es igual a 1- pero se imprime "1", y
               sale "1 días"; y 0,04 sale "0 días" en vez de "hoy". Hoy el
               procedimiento ya devuelve un decimal redondeado, asi que no
               ocurre, pero el metodo es publico y no puede confiar en eso. */
            dias = Math.Round(dias, 1, MidpointRounding.AwayFromZero);

            if (dias <= 0m) { return "hoy"; }
            if (dias == 1m) { return "1 día"; }

            /* El numero se arma con InvariantCulture y la coma se pone a mano,
               en vez de dejarle el separador a la cultura del hilo. Web.config
               fija es-ES para la aplicacion web y hoy daria la coma igual, pero
               este metodo tambien corre desde las pruebas y desde cualquier cosa
               que no sea la web, donde la cultura es la de la maquina: el mismo
               numero saldria "1.9" o "1,9" segun donde se lo llame. Lo lee una
               persona en espaniol: coma siempre, y sin depender de un ajuste que
               vive tres capas mas arriba. */
            string numero = dias.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',');

            return numero + " días";
        }
    }
}
