namespace CapaEntidad
{
    /// <summary>
    /// Lo que sale del calculo de una fila.
    ///
    /// Los tres valores hora llevan seis decimales a proposito: son
    /// intermedios y no se redondean durante la cadena. Solo los totales se
    /// redondean, y a dos decimales.
    /// </summary>
    public class EntHeResultado
    {
        public int Divisor { get; set; }
        public decimal ValorHoraOrdinaria { get; set; }
        public decimal ValorHora50 { get; set; }
        public decimal ValorHora100 { get; set; }
        public decimal Total50 { get; set; }
        public decimal Total100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalHE { get; set; }

        /// <summary>
        /// Se activa en cualquiera de estos casos: el insumo o los parametros
        /// llegan nulos; alguna hora (50% o 100%) es negativa; o el salario
        /// vigente o el divisor son cero. En todos, los totales en dinero
        /// quedan en cero -nunca un numero negativo o inventado.
        ///
        /// TotalHoras es la excepcion y la diferencia es deliberada: con horas
        /// negativas queda en cero, porque el dato es basura y un negativo no
        /// debe colarse en una columna que alguien suma; pero con salario o
        /// divisor en cero conserva la suma de horas, porque esas horas son
        /// validas -las cargo un jefe- y lo que falta es el sueldo. Ponerla en
        /// cero ahi borraria de la pantalla que esa persona trabajo.
        ///
        /// La fila se muestra en rojo y bloquea el cierre del periodo: no es un
        /// error de programa, es un dato que alguien tiene que arreglar.
        /// </summary>
        public bool TieneAdvertencia { get; set; }
    }
}
