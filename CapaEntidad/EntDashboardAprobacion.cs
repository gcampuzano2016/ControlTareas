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
}
