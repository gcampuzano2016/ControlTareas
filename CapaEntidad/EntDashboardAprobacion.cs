namespace CapaEntidad
{
    /// <summary>
    /// Una empresa y los minutos que se le dedicaron en el rango.
    ///
    /// Vienen los minutos Y las horas: los minutos son el dato que se suma
    /// -sumar valores ya redondeados no da lo mismo que redondear la suma- y
    /// Horas es la conversion final, la que el navegador pinta sin volver a
    /// tocarla. La llena NegDashboardAprobacion.Cargar, despues de agrupar.
    /// </summary>
    public class EntDashboardEmpresa
    {
        public string Empresa { get; set; } = "";
        public int Minutos { get; set; }

        /* Lo aprobado, aparte del total. El grafico muestra esto; Minutos se
           conserva porque el DAO viejo lo lee durante la ventana entre el
           script y los binarios.

           No hay HorasAprobadas a proposito: Convertir hace
           e.Horas = HorasDecimales(e.Minutos), y despues de SoloAprobadas ese
           Minutos ya es lo aprobado. Un campo mas viajaria siempre en cero. */
        public int MinutosAprobados { get; set; }

        /// <summary>Los mismos minutos en horas, con un decimal.</summary>
        public decimal Horas { get; set; }
    }

    /// <summary>Las cifras de las tarjetas de arriba.</summary>
    public class EntDashboardTotales
    {
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }

        /// <summary>
        /// Estados distintos de 1 y 2. Son los estados 5 y 7 -592 filas- que hoy
        /// no aparecen en ninguna pantalla ni en el catalogo. Se muestran
        /// agrupados para que el total de las tarjetas cuadre con el rango.
        /// </summary>
        public int MinutosOtros { get; set; }

        /// <summary>
        /// Los mismos minutos ya convertidos a horas. Convierte el servidor y no
        /// el navegador: con la regla escrita en los dos lados, las dos copias
        /// daban numeros distintos para el mismo dato (75 minutos: 1,2 en C# por
        /// el redondeo bancario, 1,3 en JavaScript).
        /// </summary>
        public decimal HorasAprobadas { get; set; }
        public decimal HorasPendientes { get; set; }
        public decimal HorasOtros { get; set; }

        /// <summary>
        /// Personas-dia del rango, sin filtrar por estado. Es el unico total
        /// valido: los dos parciales de abajo se solapan -un dia con tareas
        /// aprobadas y pendientes cae en los dos- y sumarlos cuenta de mas.
        /// </summary>
        public int PersonasDiaTotal { get; set; }

        public int PersonasDiaAprob { get; set; }
        public int PersonasDiaPend { get; set; }
        public int Responsables { get; set; }
    }

    /// <summary>Un punto de la linea de evolucion. Semana es el lunes.</summary>
    public class EntDashboardSemana
    {
        public System.DateTime Semana { get; set; }
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }

        /// <summary>Los mismos minutos en horas. Ver EntDashboardTotales.</summary>
        public decimal HorasAprobadas { get; set; }
        public decimal HorasPendientes { get; set; }
    }

    /// <summary>Una barra del grafico por persona.</summary>
    public class EntDashboardResponsable
    {
        public string Nombre { get; set; } = "";
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }

        /// <summary>Los mismos minutos en horas. Ver EntDashboardTotales.</summary>
        public decimal HorasAprobadas { get; set; }
        public decimal HorasPendientes { get; set; }

        public int PersonasDia { get; set; }

        /// <summary>Dias del rango en que esa persona no llego a 8 horas.</summary>
        public int DiasBajoJornada { get; set; }
    }

    /// <summary>
    /// Cuanto se tarda en aprobar. AprobadasSinFecha no es decorativo: si crece,
    /// el promedio se calcula sobre cada vez menos filas y deja de representar.
    /// </summary>
    public class EntDashboardDemora
    {
        /// <summary>
        /// Promedio de dias entre el registro y la aprobacion. Decimal y no
        /// entero: el AVG entero del procedimiento truncaba, y el tablero decia
        /// siempre que se tarda menos de lo que se tarda.
        /// </summary>
        public decimal DiasPromedio { get; set; }

        public int DiasMaximo { get; set; }
        public int AprobadasConFecha { get; set; }
        public int AprobadasSinFecha { get; set; }
        public int DiasMasViejoPendiente { get; set; }

        /// <summary>
        /// DiasPromedio en texto, con el caso sin datos ya resuelto. Con
        /// AprobadasConFecha en cero no hay con que calcular el promedio, y la
        /// tarjeta no puede decir "hoy": se leeria como el mejor resultado
        /// posible justo cuando el dato es el peor.
        /// </summary>
        public string TextoDemoraPromedio { get; set; } = "";

        /// <summary>DiasMasViejoPendiente en texto.</summary>
        public string TextoMasViejoPendiente { get; set; } = "";
    }

    /// <summary>Los cinco bloques, tal como viajan al navegador.</summary>
    public class EntDashboardAprobacion
    {
        public EntDashboardTotales Totales { get; set; } = new EntDashboardTotales();
        public System.Collections.Generic.List<EntDashboardSemana> Semanas { get; set; }
            = new System.Collections.Generic.List<EntDashboardSemana>();
        public System.Collections.Generic.List<EntDashboardResponsable> Responsables { get; set; }
            = new System.Collections.Generic.List<EntDashboardResponsable>();
        public EntDashboardDemora Demora { get; set; } = new EntDashboardDemora();
        public System.Collections.Generic.List<EntDashboardEmpresa> Empresas { get; set; }
            = new System.Collections.Generic.List<EntDashboardEmpresa>();
    }
}
