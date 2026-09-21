namespace CapaEntidad
{
    /// <summary>
    /// Una empresa y los minutos que se le dedicaron en el rango.
    ///
    /// Minutos y no horas: la conversion a horas decimales es para la etiqueta
    /// del grafico y la hace NegDashboardAprobacion.HorasDecimales. Guardar aqui
    /// un decimal obligaria a redondear antes de sumar, y las sumas de valores ya
    /// redondeados no dan lo mismo que el redondeo de la suma.
    /// </summary>
    public class EntDashboardEmpresa
    {
        public string Empresa { get; set; } = "";
        public int Minutos { get; set; }
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
    }

    /// <summary>Una barra del grafico por persona.</summary>
    public class EntDashboardResponsable
    {
        public string Nombre { get; set; } = "";
        public int MinutosAprobados { get; set; }
        public int MinutosPendientes { get; set; }
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
        public int DiasPromedio { get; set; }
        public int DiasMaximo { get; set; }
        public int AprobadasConFecha { get; set; }
        public int AprobadasSinFecha { get; set; }
        public int DiasMasViejoPendiente { get; set; }
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
