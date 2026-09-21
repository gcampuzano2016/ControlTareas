using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

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
    /// </summary>
    public static class NegDashboardAprobacion
    {
        /// <summary>Etiqueta "Otras" de la porcion que agrupa la cola larga.</summary>
        private const string EtiquetaOtras = "Otras";

        /// <summary>
        /// Los cinco conjuntos del dashboard, con las empresas ya reducidas a un
        /// top 10 mas "Otras".
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

            datos.Empresas = TopConOtras(datos.Empresas, 10);

            return datos;
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
        /// Los dias de demora, en texto. "hoy", "1 dia", "N dias".
        ///
        /// Los negativos se leen como "hoy": una aprobacion fechada antes del
        /// registro existe en datos viejos, y mostrar "-3 dias" haria que quien
        /// lo vea desconfie de todo el tablero por un caso que no importa.
        /// </summary>
        public static string TextoDemora(int dias)
        {
            if (dias <= 0) { return "hoy"; }
            if (dias == 1) { return "1 día"; }

            return dias.ToString(System.Globalization.CultureInfo.InvariantCulture) + " días";
        }
    }
}
